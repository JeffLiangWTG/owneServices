using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class OverseasAgentChargesPoster : IProcessor
	{
		public OverseasAgentChargesPoster(IJobCostingPlugIn consol)
		{
			this.consol = consol;
		}

		readonly IJobCostingPlugIn consol;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			if (consol != null && consol.CostSupporter.ShipmentsList.Length > 0)
			{
				var companies = GetCompanies();

				Notifications = new NotificationCollection();
				try
				{
					foreach (GlbCompany currentCompany in companies)
					{
						var branch = currentCompany.Branches.FirstOrDefault(x => x.GB_IsActive);

						if (branch != null)
						{
							using (branch.SetAsTemporaryContext())
							{
								PostPerCompany();
							}
						}
					}

					if (Notifications.HasErrors())
					{
						foreach (GlbCompany currentCompany in companies)
						{
							var branch = currentCompany.Branches.FirstOrDefault(x => x.GB_IsActive);

							if (branch != null)
							{
								using (branch.SetAsTemporaryContext())
								{
									new OverseasAgentChargesPosterEmail(consol, Notifications.ToMessageListString()).Send();
								}
							}
						}
					}
				}
				finally
				{
					notifications.AddRange(Notifications);

					RecycleFactory();
				}
			}
		}

		IEnumerable<GlbCompany> GetCompanies()
		{
			var pksParam = new ZStringBuilder();
			foreach (ZGuid pk in consol.CostSupporter.ShipmentsListPKs)
			{
				pksParam.Append("'" + pk.ToString() + "'");
			}

			string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT DISTINCT JH_GC AS GC_PK
FROM dbo.JobHeader 
	INNER JOIN dbo.JobCharge ON JR_JH = JH_PK
	LEFT OUTER JOIN dbo.AccTransactionLines ON AL_PK = JR_AL_ARLine AND AL_LineType = @Rev
	LEFT OUTER JOIN dbo.OrgAddress ON OA_PK = JH_OA_AgentCollectAddr
WHERE JH_GC IS NOT NULL AND 
	JH_ParentID IN ({0}) AND
	AL_PK IS NULL AND
	JR_OH_SellAccount IS NOT NULL AND 
	(
		{1} 
		{2} 
		JR_OH_SellAccount = OA_OH
	)
UNION ALL
SELECT E6_GC AS GC_PK
FROM dbo.JobConsolCost
WHERE E6_ParentID = @ConsolPK AND
	E6_AH_APInvoice IS NULL AND
	E6_IsForCollectInvoice = @True",
				pksParam.ToStringWithDelimiterBetweenAppends(","),
				consol.CostSupporter.SendingForwarder != null ? "JR_OH_SellAccount = @SendingForwarder OR" : "",
				consol.CostSupporter.ReceivingForwarder != null ? "JR_OH_SellAccount = @ReceivingForwarder OR" : "");

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@Rev", TransactionLineTypes.Revenue, AccTransactionLinesSchema.AL_LineType);
			if (consol.CostSupporter.SendingForwarder != null)
			{
				parameters.Add("@SendingForwarder", consol.CostSupporter.SendingForwarder.PK, JobChargeSchema.JR_OH_SellAccount);
			}
			if (consol.CostSupporter.ReceivingForwarder != null)
			{
				parameters.Add("@ReceivingForwarder", consol.CostSupporter.PK, JobConsolCostSchema.E6_ParentID);
			}
			parameters.Add("@ConsolPK", consol.CostSupporter.PK, JobConsolCostSchema.E6_ParentID);
			parameters.Add("@True", ZBool.True, JobConsolCostSchema.E6_IsForCollectInvoice);

			var companyPKs = new DynamicBusinessObjectCollection(Factory);
			companyPKs.Load(sql, parameters);
			var pks = companyPKs.Select(x => (ZGuid)x[GlbCompanySchema.PK]);

			var companyQuery = new ZQuery(GlbCompanySchema.PK, pks);
			companyQuery.AddToFilter(GlbCompanySchema.GC_IsActive, ZBool.True);
			return Factory.Load<GlbCompany>(companyQuery);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void PostPerCompany()
		{
			RecycleFactory();
			Notifications.Add(NotificationSubscriberType.Info, Res.GetString("783db037-09a4-41fe-9ee6-73d3f4813bf5", "Started posting Agent Charges on the {0} - {1}", GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.GC_Name));

			ConsolInvoicingPostManager poster = null;

			try
			{
				var jobFilter = new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				var shipmentPKs = consol.CostSupporter.ShipmentsList.Select(x => x.PK).ToArray();
				jobFilter.AddToFilter(JobHeaderSchema.JH_ParentID, shipmentPKs);
				var jobs = Factory.Load<Job>(new JobCollection(Factory, jobFilter).CompleteFilter);

				bool hasError = false;

				foreach (Job job in jobs)
				{
					if (!string.IsNullOrEmpty(job.ReasonNotToAllowPosting))
					{
						hasError = true;
						Notifications.AddError(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", job.JH_JobNum, job.ReasonNotToAllowPosting));
					}
				}

				if (!hasError)
				{
					var validation = CreatePostManagerValidation(jobs);
					var validationResult = validation.Validate();
					if (validationResult != null && validationResult.Type == CargoWise.ComponentModel.NotificationType.Error)
					{
						hasError = true;
						Notifications.AddError(validationResult.Message);
					}

					if (!hasError)
					{
						var costs = new ApportionmentListing(Factory, consol);
						poster = GetPostManager(jobs, costs, consol);

						AttachEventHandlers(poster);

						var transactions = poster.CreateTransactions(JobInvoicingPostingOption.Agent);
						if (!poster.CancelPosting)
						{
							poster.Factory.Save();
						}
						else
						{
							foreach (var transaction in transactions.Values)
							{
								var invoicing = transaction as InvoicingBase;
								if (invoicing.HasRowErrors)
								{
									Notifications.AddRange(invoicing.RowErrors);
								}
								else if (invoicing.HasErrors)
								{
									Notifications.AddRange(invoicing.GetErrors());
								}
							}
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Notifications.AddError(Res.GetString("a53cf651-8137-4b53-89ba-e2973d21c488",
					"An exception of {0} type was thrown during posting Agent Charges. Exception Message: {1}", ex.GetType(),
					ex.Message));
			}
			finally
			{
				DetachEventHandlers(poster);

				Notifications.Add(NotificationSubscriberType.Info, Res.GetString("a1d6d2d9-ad69-4ab8-9f0e-17929010aaec", "Finished posting Agent Charges on the {0} - {1}", GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.GC_Name));
			}
		}

		protected virtual ConsolInvoicingPostManager GetPostManager(Job[] jobs, ApportionmentListing costs, IJobCostingPlugIn jobCostingPlugIn)
		{
			return new ConsolInvoicingPostManager(Factory, jobs, jobCostingPlugIn, costs);
		}

		protected void AttachEventHandlers(ConsolInvoicingPostManager poster)
		{
			poster.OnCriticalPostError += OnCriticalPostError;
			poster.OnUserWarningNotification += OnUserWarningNotification;
			poster.OnJobOnHold += OnJobOnHold;
			poster.IncorrectRegistrySetup += IncorrectRegistrySetup;
			poster.ProfitShareConfirmation += ProfitShareConfirmation;
		}

		protected void DetachEventHandlers(ConsolInvoicingPostManager poster)
		{
			if (poster != null)
			{
				poster.OnCriticalPostError -= OnCriticalPostError;
				poster.OnUserWarningNotification -= OnUserWarningNotification;
				poster.OnJobOnHold -= OnJobOnHold;
				poster.IncorrectRegistrySetup -= IncorrectRegistrySetup;
				poster.ProfitShareConfirmation -= ProfitShareConfirmation;
			}
		}

		protected PostManagerValidation CreatePostManagerValidation(IEnumerable<Job> jobs)
		{
			return new PostManagerValidation(jobs, JobInvoicingPostingOption.Agent, jobs);
		}

		protected NotificationCollection Notifications;

		void OnCriticalPostError(object sender, CriticalPostingErrorEventArgs e)
		{
			Notifications.AddError(e.ErrorMessage);
		}

		void OnUserWarningNotification(object sender, UserMessageEventArgs e)
		{
			Notifications.AddWarning(e.Message);
		}

		void OnJobOnHold(object sender, BasePostManager.OnJobOnHoldEventArgs e)
		{
			var jobNumbers = new ZStringBuilder(e.Jobs.Cast<Job>().Where(x => x.IsWorkOnHold).Select(x => x.JH_JobNum));
			Notifications.AddError(Res.GetString("88c7da6f-7af5-4641-b6a5-c46d8395ee8a", "Charges from the following job(s) cannot be posted because {0} is on hold.", jobNumbers.ToStringWithDelimiterBetweenAppends(", ")));
		}

		void IncorrectRegistrySetup(object sender, IncorrectRegistrySetupEventArgs e)
		{
			Notifications.AddError(Res.GetString("19347614-065f-4480-9d16-666fbc5f6b01", "Incorrect Registry setting located at {0}", e.RegistyItemLocation));
		}

		bool ProfitShareConfirmation(object sender, ProfitShareConfirmationEventArgs e)
		{
			return true;
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		[NonSerialized]
		BusinessObjectFactory factory;

		void RecycleFactory()
		{
			factory = null;
		}
	}
}

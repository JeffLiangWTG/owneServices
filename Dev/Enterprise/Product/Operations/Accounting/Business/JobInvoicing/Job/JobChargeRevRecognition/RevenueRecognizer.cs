using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class RevenueRecognizer : IProcessor
	{
		public RevenueRecognizer(IJobInvoicingPlugIn plugIn)
		{
			this.PlugIn = plugIn;
		}

		public RevenueRecognizer(IJobCostingPlugIn consol)
		{
			this.Consol = consol;
		}

		readonly IJobInvoicingPlugIn PlugIn;
		readonly IJobCostingPlugIn Consol;

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			if (Consol != null && Consol.CostSupporter.ShipmentsList.Length > 0 || PlugIn != null)
			{
				var shouldSave = true;
				var jobFilter = new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				if (Consol != null)
				{
					var multipleJobFilter = new ZQuery();
					multipleJobFilter.DefaultJoinCondition = JoinCondition.Or;
					foreach (IJobInvoicingPlugIn shipment in Consol.CostSupporter.ShipmentsList)
					{
						multipleJobFilter.AddToFilter(JobHeaderSchema.JH_ParentID, shipment.PK);
					}
					jobFilter.AddToFilter(multipleJobFilter);
				}
				else
				{
					jobFilter.AddToFilter(JobHeaderSchema.JH_ParentID, PlugIn.PK);
				}
				var jobs = Factory.Load<Job>(jobFilter);

				var emails = new List<AccountingEmailDef>();
				foreach (var jobToProcess in jobs)
				{
					token.ThrowIfCancellationRequested();

					var errors = string.Empty;

					var jobValidation = jobToProcess.Validation as JobValidation;
					if (jobValidation != null)
					{
						errors = jobValidation.GetRevenueRecognitionDateValidationErrors(false);
					}

					var dummyBoolValue = false;
					jobToProcess.ApplyRevenueRecognitionDateForWholeJob(out dummyBoolValue);
					jobToProcess.RunPreSaveValidation();

					if (jobToProcess.HasErrors || !string.IsNullOrEmpty(errors))
					{
						emails.Add(new RevenueRecognitionEmail(jobToProcess, errors));
					}

					if (jobToProcess.HasErrors)
					{
						shouldSave = false;
					}
				}

				if (!shouldSave)
				{
					throw new LogSubscriberToAbortLogGroupProcessingSilentlyException("Revenue recognized processing is not successful.", emails.ToArray());
				}

				emails.ForEach(x => x.Create(Factory));
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ??
					(factory = PlugIn != null
						? PlugIn.Factory
						: Consol != null
							? Consol.Factory
							: new BusinessObjectFactory()); }
		}
		[NonSerialized]
		BusinessObjectFactory factory;
	}
}

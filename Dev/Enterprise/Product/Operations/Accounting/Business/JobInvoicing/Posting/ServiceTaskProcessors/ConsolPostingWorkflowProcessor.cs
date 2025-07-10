using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract class ConsolPostingWorkflowProcessor : IProcessor
	{
		public ConsolPostingWorkflowProcessor(IJobInvoicingPlugIn plugIn)
		{
			if (plugIn is ForwardingConsol)
			{
				Consol = plugIn as IJobCostingPlugIn;
			}
			else
			{
				throw new ArgumentException("Plugin not supported.");
			}
		}

		readonly protected IJobCostingPlugIn Consol;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			bool result = false;
			this.notifications = new NotificationCollection();
			TransactionCreatorHashtable transactions = null;

			var postingFactory = Consol.Factory;
			ZQuery jobsQuery = new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			jobsQuery.AddToFilter(JobHeaderSchema.JH_ParentID, Consol.CostSupporter.ShipmentsListPKs);
			jobs = postingFactory.Load<Job>(jobsQuery).OrderBy(x => x.JH_JobNum).ToList();

			if (jobs != null && jobs.Count > 0)
			{
				try
				{
					postManager = new ConsolInvoicingPostManager(postingFactory, jobs, Consol); // this line need to be called before job.ReasonNotToAllowPosting so the jobs' parents are initialized.

					foreach (Job job in jobs)
					{
						if (!string.IsNullOrEmpty(job.ReasonNotToAllowPosting))
						{
							this.notifications.AddError(string.Format(CultureInfo.InvariantCulture, "{0} - {1}", job.JH_JobNum, job.ReasonNotToAllowPosting));
						}
					}

					if (this.notifications.Count() == 0)
					{
						var validation = CreatePostManagerValidation(jobs.Cast<Job>());
						var validationResult = validation.Validate();

						if (validationResult != null && validationResult.Type == CargoWise.ComponentModel.NotificationType.Error)
						{
							this.notifications.AddError(validationResult.Message);
						}

						if (!this.notifications.HasErrors())
						{
							AttachEventHandlers(postManager);

							try
							{
								transactions = CreateTransacions(postManager);
								postManager.PerformTransactionDescriptionDefaulting(Consol as IJobInvoicingPlugIn, transactions);
								if (!postManager.CancelPosting)
								{
									result = true;
								}
							}
							finally
							{
								DetachEventHandlers(postManager);
							}
						}
					}

					AccountingEmailDef email = null;
					if (this.notifications.HasErrors())
					{
						email = GetEmailer(Consol, this.notifications.ToMessageListString());
					}

					if (!result)
					{
						throw new LogSubscriberToAbortLogGroupProcessingSilentlyException("Cannot continue posting Consol Job", email);
					}

					email?.Create(postingFactory); //we can't reproduce the case when create an email here, but we keep it for logical consistency: if email is not null, it is created or passed to the exception
				}
				finally
				{
					postManager = null;
					jobs = null;
#if DEBUG
					if (Globals.IsTest)
					{
						notifications.AddRange(this.notifications);
					}
#endif
				}
			}
		}

		protected abstract ConsolPostManagerValidation CreatePostManagerValidation(IEnumerable<Job> jobs);
		protected abstract TransactionCreatorHashtable CreateTransacions(ConsolInvoicingPostManager postManager);
		protected abstract AccountingEmailDef GetEmailer(IJobCostingPlugIn consol, string errors);

		protected virtual void AttachEventHandlers(ConsolInvoicingPostManager postManager)
		{
			postManager.OnCriticalPostError += postManager_OnCriticalPostError;
			postManager.OnNothingPosted += postManager_NothingPostedHandler;
			postManager.OnJobOnHold += postManager_JobOnHoldHandler;
		}

		protected virtual void DetachEventHandlers(ConsolInvoicingPostManager postManager)
		{
			postManager.OnCriticalPostError -= postManager_OnCriticalPostError;
			postManager.OnNothingPosted -= postManager_NothingPostedHandler;
			postManager.OnJobOnHold -= postManager_JobOnHoldHandler;
		}

		void postManager_OnCriticalPostError(object sender, CriticalPostingErrorEventArgs e)
		{
			notifications.AddError(e.ErrorMessage);
		}

		void postManager_NothingPostedHandler(object sender, EventArgs e)
		{
			if (!postManager.CancelPosting)
			{
				notifications.AddError(ConsolInvoicingPostManager.NoChargesToPostMessage());
			}
		}

		void postManager_JobOnHoldHandler(object sender, EventArgs e)
		{
			StringBuilder builder = new StringBuilder(ConsolInvoicingPostManager.JobOnHoldMessage);
			foreach (Job job in jobs)
			{
				builder.Append(" " + job.JH_JobNum + System.Environment.NewLine);
			}
			notifications.AddError(builder.ToString());
		}

		protected NotificationCollection notifications;
		protected ConsolInvoicingPostManager postManager;
		protected List<Job> jobs;
	}
}

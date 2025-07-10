using System.Collections.Generic;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract class JobPostingWorkflowProcessor : IProcessor
	{
		public JobPostingWorkflowProcessor(IJobInvoicingPlugIn plugIn)
		{
			this.plugIn = plugIn;
		}

		protected readonly IJobInvoicingPlugIn plugIn;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			bool result = false;
			Notifications = new NotificationCollection();
			InvoicingPostManager postManager = null;

			var job = new Job.Loader(plugIn.Factory, plugIn).Load(true, false);
			if (job != null)
			{
				try
				{
					job.Factory.SetContext(BusinessContext.PostingChargesFromLogWalker);
					AttachEventHandlersForJob(job);

					string reasonNotToAllowPosting = job.ReasonNotToAllowPosting;
					if (!string.IsNullOrEmpty(reasonNotToAllowPosting))
					{
						Notifications.AddError(reasonNotToAllowPosting);
					}
					else
					{
						var validation = CreatePostManagerValidation(new[] { job });
						var validationResult = validation.Validate();
						if (validationResult != null && validationResult.Type == CargoWise.ComponentModel.NotificationType.Error)
						{
							Notifications.AddError(validationResult.Message);
						}
					}

					if (!Notifications.HasErrors())
					{
						postManager = GetPostManager(job);

						AttachEventHandlers(postManager);

						result = PerformPost(postManager);
					}

					AccountingEmailDef email = null;
					if (Notifications.HasErrors())
					{
						email = GetEmailer(job);
					}

					if (!result)
					{
						throw new LogSubscriberToAbortLogGroupProcessingSilentlyException("Cannot continue posting transactions", email);
					}

					email?.Create(plugIn.Factory); //we can't reproduce the case when create an email here, but we keep it for logical consistency: if email is not null, it is created or passed to the exception
				}
				finally
				{
					notifications.AddRange(Notifications);
					job.Factory.RemoveContext(BusinessContext.PostingChargesFromLogWalker);
				}
			}
		}

#if DEBUG
		internal bool UseDummyInvoicingPostManager_ForTestOnly { get; set; }
#endif

		InvoicingPostManager GetPostManager(Job job)
		{
#if DEBUG
			if (Globals.IsTest && UseDummyInvoicingPostManager_ForTestOnly)
			{
				return new Testing.DummyInvoicingPostManager(job);
			}
#endif
			return CreatePostManager(job);
		}

		protected virtual InvoicingPostManager CreatePostManager(Job job)
		{
			return new InvoicingPostManager(job);
		}

		protected abstract PostManagerValidation CreatePostManagerValidation(IEnumerable<Job> jobs);
		protected abstract bool PerformPost(InvoicingPostManager postManager);
		protected abstract AccountingEmailDef GetEmailer(Job job);

		protected virtual void AttachEventHandlers(InvoicingPostManager postManager)
		{
			postManager.OnCriticalPostError += postManager_OnCriticalPostError;
			postManager.OnUserWarningNotification += postManager_OnUserWarningNotification;
			postManager.OnJobOnHold += postManager_OnJobOnHold;
		}

		protected virtual void AttachEventHandlersForJob(Job job)
		{
		}

		void postManager_OnJobOnHold(object sender, BasePostManager.OnJobOnHoldEventArgs e)
		{
			Notifications.AddError(Res.GetString("a53b606b-ad0e-47a8-8f0e-88f521e27ee8", "You cannot post because this job is on hold.\r\nTo post, change the job status from '{0}'.", JobHeaderStatus.WorkOnHold.Code));
		}

		void postManager_OnCriticalPostError(object sender, CriticalPostingErrorEventArgs e)
		{
			Notifications.AddError(e.ErrorMessage);
		}

		void postManager_OnUserWarningNotification(object sender, UserMessageEventArgs e)
		{
			Notifications.AddWarning(e.Message);
		}

		protected NotificationCollection Notifications;
	}
}

#region Test
#if DEBUG

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class DummyInvoicingPostManager : InvoicingPostManager
	{
		public DummyInvoicingPostManager(Job job)
			: base(job)
		{ }

		protected override UserMessageEventArgs BuildInvoiceDateInFutureWarningMessage(TransactionCreatorHashtable transactions)
		{
			return new UserMessageEventArgs("this is only for test warning message!");
		}
	}
}

#endif
#endregion

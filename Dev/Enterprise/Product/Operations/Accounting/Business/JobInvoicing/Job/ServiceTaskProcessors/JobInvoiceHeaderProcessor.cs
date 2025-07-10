using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	class JobInvoiceHeaderProcessor : IProcessor
	{
		public JobInvoiceHeaderProcessor(IJobInvoicingPlugIn plugIn)
		{
			this.plugIn = plugIn;
		}

		readonly IJobInvoicingPlugIn plugIn;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			bool processHasError = false;
			this.notifications = new NotificationCollection();
			var loader = new Job.Loader(plugIn);
			var job = loader.Load(true, false);

			if (job == null) // only create job header if it doesn't exist
			{
				try
				{
					job = loader.TryCreateWithMutex(GlbBranch.CurrentBranch);
					if (job == null)
					{
						this.notifications.AddError(loader.GetJobCreationError());
						processHasError = true;
					}
					else
					{
						job.RunPreSaveValidation();
#if DEBUG
						if (Globals.IsTest && ShouldFailJobPreSaveValidation_ForTestOnly)
						{
							job.AddRowError("Force to fail PreSaveValidation.");
						}
#endif
						if (job.HasErrors)
						{
							foreach (string error in job.NotificationsIncludingChildren.GetErrors().GetUniqueMessageList())
							{
								this.notifications.AddError(error);
							}
							processHasError = true;
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					this.notifications.AddError(ex.Message);
					processHasError = true;
				}
				finally
				{
					if (processHasError)
					{
						AccountingEmailDef email = new JobInvoiceHeaderCreationErrorEmail(plugIn, this.notifications);
#if DEBUG
						if (Globals.IsTest)
						{
							notifications.AddRange(this.notifications);
						}
#endif
						if (job != null)
						{
							job.DisposeAndDeleteNew();

							throw new LogSubscriberToAbortLogGroupProcessingSilentlyException("Create Job Invoice Header processing is not successful.", email);
						}

						email.Create(plugIn.Factory);
					}
					else if (job != null)
					{
						plugIn.Factory.Saved += (factory, savedSuccessfully) => { job.DisposeAndDeleteNew(); };
					}
				}
			}
		}

		NotificationCollection notifications;

#if DEBUG
		public bool ShouldFailJobPreSaveValidation_ForTestOnly { get; set; }
#endif
	}
}

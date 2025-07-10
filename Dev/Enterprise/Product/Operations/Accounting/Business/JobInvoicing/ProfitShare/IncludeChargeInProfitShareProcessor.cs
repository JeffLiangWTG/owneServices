using System;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	class IncludeChargeInProfitShareProcessor : IProcessor
	{
		public IncludeChargeInProfitShareProcessor(IJobInvoicingPlugIn plugIn)
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
			var processHasError = false;
			this.notifications = new NotificationCollection();
			var loader = new Job.Loader(plugIn.Factory, plugIn);
			var job = loader.Load(setParent: true, setJobDefaults: false);

			if (job != null)
			{
				try
				{
					foreach (BaseCharge charge in job.Charges)
					{
						if (!charge.JR_IsIncludedInProfitShare)
						{
							charge.SetProfitShareIncludedFlag();
						}
					}

					if (job.HasChanges)
					{
						job.MarkAsNeedingValidationIncludingChildren();
						job.RunPreSaveValidation();

						if (!job.HasErrors())
						{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
							job.Logs.AddNew(AutoEvents.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Charge(s) included in Profit Share by '{0}' trigger action.", WorkflowTriggerActionTypeConstants.Codes.IncludeChargeInProfitShare));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
						}
						else
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
						if (job != null)
						{
							job.Dispose();
						}

						AccountingEmailDef email = new IncludeChargeInProfitShareErrorEmail(plugIn, this.notifications);
						throw new LogSubscriberToAbortLogGroupProcessingSilentlyException("Include charge in profit share processing unsuccessful.", email);
					}
					else if (job != null)
					{
						plugIn.Factory.Saved += (factory, savedSuccessfully) => { job.Dispose(); };
					}
				}
			}
		}

		NotificationCollection notifications;
	}
}

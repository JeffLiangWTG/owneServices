using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoMessageProcessor : Integration.Customs.AU.ISeaCargoMessageProcessor, ICargoMessageProcessor
	{
		public SeaCargoMessageProcessor(INotifications notifications)
		{
			this.notifications = Argument.NotNull(notifications, "notifications");
		}
		readonly INotifications notifications;

		void ICargoMessageProcessor.Process(ICargoMessageProcessorJob processorJob, bool saveFactory)
		{
			Process(processorJob as SeaCargoProcessorJobForConsol, saveFactory);
		}

		public void Process(SeaCargoProcessorJobForConsol processorJob, bool saveFactory = true)
		{
			if (processorJob != null)
			{
				try
				{
					if (processorJob.IsAcceptable && ProcessAcceptableJob(processorJob, saveFactory))
					{
						notifications.Notify(new InfoNotification(ZString.Format("Sea Cargo Messages for {0} were sent.", processorJob.JobNumber)));
					}
					else
					{
						notifications.AddWarning(ZString.Format("Sea Cargo Messages for {0} were not sent.", processorJob.JobNumber));
					}
				}
				catch (ZSaveConcurrencyException ex)
				{
					notifications.AddWarning(ZString.Format("Sea Cargo Messages for {0} were not sent, error during saving.", processorJob.JobNumber));
					notifications.Notify(new ErrorNotification(ErrorType.PostToDatabaseError, ex.Message.Replace("\n", "")));
				}
			}
		}

		bool ProcessAcceptableJob(SeaCargoProcessorJobForConsol processorJob, bool saveFactory)
		{
			bool result = false;

			var synchroniser = processorJob.Synchroniser;
			var oceanBill = synchroniser.ExistingOceanBill;
			if (oceanBill == null)
			{
				if (synchroniser.Consol.IsInDatabase)
				{
					var mutex = processorJob.Mutex;
					if (mutex.Lock())
					{
						try
						{
							oceanBill = CreateOceanBillInAnotherFactory(synchroniser);
						}
						finally
						{
							mutex.Unlock();
						}
					}
					else
					{
						var jobNumber = processorJob.JobNumber;
						var userWithLock = mutex.GetLockInfo()?.UserWithLock?.GS_FullName ?? ZString.Empty;
						var notificationMessage = ZString.Format("Could not send Sea Cargo messages for {0} as a lock could not be obtained on the master. Probably caused by user {1} sending messages for this Job at this time.", jobNumber, userWithLock);
						notifications.Notify(new InfoNotification(notificationMessage));
					}
				}
				else
				{
					synchroniser.SynchroniseOceanBill();
					oceanBill = synchroniser.OceanBill;
				}
			}

			if (oceanBill != null && !oceanBill.HasErrors)
			{
				processorJob.SetSACIfRequired();

				var messageAction = new SendOriginalCargoReportMessageActionNotify(notifications)
				{
					SendWithMessageErrors = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed
				};

				var messageManager = new CusSCAOceanBillMessageManager(oceanBill);
				var messages = messageManager.SendOriginalMessages(messageAction);

				if (saveFactory)
				{
					try
					{
						oceanBill.Factory.Save(); // This save shouldn't be here.
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						foreach (var message in messages)
						{
							message.Delete();
						}
						throw;
					}
				}

				result = messages.Any();
			}

			return result;
		}

		CusSCAOceanBill CreateOceanBillInAnotherFactory(CMRSeaCargoSynchroniser synchroniser)
		{
			var newFactory = new BusinessObjectFactory();
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(synchroniser.Consol.PK);
			if (consolInNewFactory != null)
			{
				var synchroniserInNewFactory = new CMRSeaCargoSynchroniser(consolInNewFactory);
				synchroniserInNewFactory.SynchroniseOceanBill();
				newFactory.Save();
			}

			return synchroniser.ExistingOceanBill;
		}
	}
}

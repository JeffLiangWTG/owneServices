using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CargoMessagingTriggerActionProcessor
	{
		public CargoMessagingTriggerActionProcessor(IRegistryItem cargoSendErrorsToGroup)
		{
			this.cargoSendErrorsToGroup = cargoSendErrorsToGroup;
		}
		readonly IRegistryItem cargoSendErrorsToGroup;

		public void Process(IHouseBillsCargoMessageProcessorJobWithMutex processorJob, INotifications notifications, ZString queuedUserNK, ILogger logger)
		{
			var branch = processorJob.Branch;
			using (branch != null ? DisposableEnvironment.ForBranch(branch.PK.ToGuid()) : null)
			{
				ProcessCore(processorJob, notifications, queuedUserNK, logger);
			}
		}

		void ProcessCore(IHouseBillsCargoMessageProcessorJobWithMutex processorJob, INotifications notifications, ZString queuedUserNK, ILogger logger)
		{
			var jobNumber = processorJob.JobNumber;
			var factory = processorJob.MasterBill.Factory;

			try
			{
				if (processorJob.Mutex.Lock())
				{
					logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Generating Cargo messages for Job {0}", jobNumber));

					var processor = new HouseBillsCargoMessageProcessor(notifications);
					processor.Process(processorJob, false);

					var generatedMessages = processor.GeneratedHouseMessages.ToList();
					foreach (var message in generatedMessages)
					{
						message.EM_SystemCreateUser = queuedUserNK;
					}

					var notificationsString = processor.NotificationBuffer.AsString;
					if (processor.NotificationBuffer.HasErrors || processor.NotificationBuffer.HasWarnings)
					{
						logger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "{0} messages sucessfully created, but errors or warnings occured on others:\r\n {1}", generatedMessages.Count, notificationsString));
						var emailBodyText = string.Format(CultureInfo.InvariantCulture, "Errors or warnings have occurred when sending Cargo messages for Job {0}, please see the Event Log Walker Service Task log.", jobNumber);
						AUBatchProcessorSupporter.SendEmail(factory, GlbBranch.CurrentBranch, "AU Cargo Send errors or warnings", emailBodyText, cargoSendErrorsToGroup);
					}
					else if (string.IsNullOrEmpty(notificationsString) || generatedMessages.Count == 0)
					{
						logger.Log(LogType.Information, "No messages were created.");
					}
					else
					{
						logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "{0} messages sucessfully created.", generatedMessages.Count));
					}
				}
				else
				{
					var lockInfo = processorJob.Mutex.GetLockInfo();
					var userWithLock = lockInfo != null && lockInfo.UserWithLock != null ? lockInfo.UserWithLock.GS_FullName : ZString.Empty;
					string emailBodyText = string.Format(CultureInfo.InvariantCulture, "Could not send Cargo messages for Job {0}, as a lock could not be obtained on the master. Probably caused by user {1} sending messages for this Job at this time.", jobNumber, userWithLock);
					logger.Log(LogType.Warning, emailBodyText);
					AUBatchProcessorSupporter.SendEmail(factory, GlbBranch.CurrentBranch, "AU CARGO Message not sent", emailBodyText, cargoSendErrorsToGroup);
				}
			}
			finally
			{
				if (processorJob.Mutex.HasLock)
				{
					processorJob.Mutex.Unlock();
				}
			}
		}
	}
}

using System.Globalization;
using CargoWise.ComponentModel;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDMessagingTriggerActionProcessor
	{
		public IMDMessagingTriggerActionProcessor(JobDeclaration jobDeclaration, CMRMessageTypes messageType)
		{
			this.jobDeclaration = jobDeclaration;
			this.messageType = messageType;
		}
		readonly JobDeclaration jobDeclaration;
		readonly CMRMessageTypes messageType;

		public void Process(INotifications notifications, ILogger logger, IGlbStaff user)
		{
			var branch = jobDeclaration.Branch;
			using (branch != null ? DisposableEnvironment.ForBranch(branch.PK.ToGuid()) : null)
			{
				ProcessCore(notifications, logger, user);
			}
		}

		void ProcessCore(INotifications notifications, ILogger logger, IGlbStaff user)
		{
			var jobNumber = jobDeclaration.JobNumber;
			switch (messageType)
			{
				case CMRMessageTypes.LodgeWithPay:
				case CMRMessageTypes.LodgeWithoutPay:
					logger.Log(LogType.Information, $"Generating entry lodgement messages for Job {jobNumber}");
					break;
				case CMRMessageTypes.Payment:
					logger.Log(LogType.Information, $"Generating payment messages for Job {jobNumber}");
					break;
				default:
					break;
			}

			int messageCount = 0;
			var notificationBuffer = new NotificationBuffer(notifications);
			jobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var messageManager = new IMDMultiMessageManager(jobDeclaration, messageType);
				if (messageType == CMRMessageTypes.Payment)
				{
					messageManager.EFTPaymentInformations = new EFTPaymentInformationCollection(jobDeclaration, initialiseFromLastClearance: false);
				}
				messageCount = messageManager.SendQueuedMessage(jobDeclaration, notificationBuffer, user.GS_Code);
			}

			var notificationsString = notificationBuffer.AsString;
			if (notificationBuffer.HasErrors || notificationBuffer.HasWarnings)
			{
				logger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "{0} messages successfully created. Errors or warnings occurred:\r\n {1}", messageCount, notificationsString));

				var emailBodyText = string.Format(CultureInfo.InvariantCulture, "Errors or warnings have occurred when sending messages for Job {0}. Please see the Event Log Walker Service Task log.", jobNumber);
				AUBatchProcessorSupporter.SendEmail(jobDeclaration.Factory, GlbBranch.CurrentBranch, "AU Import Declaration Send errors or warnings", emailBodyText, Env.Registry.RawRegistry.EdificeSendErrorsToGroup);
			}
			else if (string.IsNullOrEmpty(notificationsString) && messageCount == 0)
			{
				logger.Log(LogType.Information, "No messages were created.");
			}
			else
			{
				logger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "{0} messages successfully created.", messageCount));
			}
		}
	}
}

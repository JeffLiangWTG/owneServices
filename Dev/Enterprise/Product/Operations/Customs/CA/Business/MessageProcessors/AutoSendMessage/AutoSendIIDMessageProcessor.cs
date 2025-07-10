using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Business
{
	public class AutoSendIIDMessageProcessor : CAAutoSendCustomsMessageProcessor
	{
		public AutoSendIIDMessageProcessor(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override ZString EntryType
		{
			get { return MessageTypeList.Codes.EDIRelease; }
		}

		protected override ZString MessageDescription
		{
			get { return JobDeclaration.Constants.MessageNames.IID; }
		}

		protected override ZBool SendMessageCore(INotifications notifications, CusEntryHeader entryHeader)
		{
			try
			{
				var messageWrapper = new IIDMessageWrapper(entryHeader);
				var messageManager = new IIDMessageManager(messageWrapper, new UserNotificationWrapper(notifications));
				using (new MessageManagerFactorySaveSuspender(messageManager))
				{
					return messageManager.SendMessage(MessageSubTypes.Undefined);
				}
			}
			catch(InvalidMessageContentException ex)
			{
				if (ex.InnerException is DeveloperNotificationException developerNotificationException)
				{
					throw developerNotificationException;
				}
				return false;
			}
		}
	}
}

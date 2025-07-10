using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.CA.Business
{
	public class AutoSendACROSSMessageProcessor : CAAutoSendCustomsMessageProcessor
	{
		public AutoSendACROSSMessageProcessor(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override ZString EntryType
		{
			get { return MessageTypeList.Codes.EDIRelease; }
		}

		protected override ZString MessageDescription
		{
			get { return JobDeclaration.Constants.MessageNames.Release; }
		}

		protected override ZBool SendMessageCore(INotifications notifications, CusEntryHeader entryHeader)
		{
			var dataWrapper = new EDIReleaseImportMessageWrapper(entryHeader);
			var messageManager = new EDIReleaseImportMessageManager(dataWrapper, new UserNotificationWrapper(notifications));
			using (new MessageManagerFactorySaveSuspender(messageManager))
			{
				return messageManager.SendMessage(MessageSubTypes.Undefined);
			}
		}
	}
}

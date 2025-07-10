using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.MessageBuilders;

namespace Enterprise.Customs.CA.Business
{
	public class AutoSendG7ExportMessageProcessor : CAAutoSendCustomsMessageProcessor
	{
		public AutoSendG7ExportMessageProcessor(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override ZString EntryType
		{
			get { return MessageTypeList.Codes.G7Export; }
		}

		protected override ZString MessageDescription
		{
			get { return JobDeclaration.Constants.MessageNames.G7; }
		}

		protected override ZBool SendMessageCore(INotifications notifications, CusEntryHeader entryHeader)
		{
			var dataWrapper = new G7ExportMessageWrapper(entryHeader);
			var messageManager = new G7ExportDeclarationMessageManager(dataWrapper, new UserNotificationWrapper(notifications));
			using (new MessageManagerFactorySaveSuspender(messageManager))
			{
				return messageManager.SendMessage(MessageSubTypes.Undefined);
			}
		}
	}
}

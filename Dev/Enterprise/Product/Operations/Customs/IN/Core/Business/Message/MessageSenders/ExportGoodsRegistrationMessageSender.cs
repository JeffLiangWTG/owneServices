using CargoWise.Customs.IN.MessageContracts.ExportSbGoodsRegistration;
using CargoWise.Types;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb;

namespace Enterprise.Customs.IN.Business
{
	internal class ExportGoodsRegistrationMessageSender : BaseMessageSender<DeclarationMessageSendingObject>
	{
		public ExportGoodsRegistrationMessageSender(DeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
		{
		}

		protected override ZString MessageType => EDIMessageTypeList.Codes.ShippingBill;

		protected override ZString MessageSubType => EDIMessageSubTypeList.Codes.GoodsRegistration;

		protected override ZString GetMessageText()
		{
			var dataProvider = ExportSbGoodsRegistrationCACHE05DataProvider.CreateProvider(messageSendingObject);
			return new ExportSbGoodsRegistrationCACHE05MessageBuilder(dataProvider).GetFlatFileMessage();
		}
	}
}

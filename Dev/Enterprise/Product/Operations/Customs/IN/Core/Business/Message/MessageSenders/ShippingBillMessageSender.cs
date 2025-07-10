using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.IN.Business.MessageSending.ExportSb;

namespace Enterprise.Customs.IN.Business;

public class ShippingBillMessageSender : BaseMessageSender<DeclarationMessageSendingObject>
{
	public ShippingBillMessageSender(DeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override ZString MessageType => EDIMessageTypeList.Codes.ShippingBill;

	protected override ZString MessageSubType => EDIMessageTypeList.Codes.ShippingBill + messageSendingObject.MessageType;

	protected override ZString GetMessageText()
	{
		var dataProvider = ExportSbCACHE01DataProvider.CreateProvider(messageSendingObject.Header, new ExportSbCACHE01AdditionalDataProvider(messageSendingObject));
		return new ExportSbCACHE01MessageBuilder(dataProvider).GetFlatFileMessage();
	}
}

using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BE.Business;

public class AmendmentSenderProvider : AESMessageSender<ICC513CDataProvider>
{
	public AmendmentSenderProvider(ExportEntryMessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
	}

	protected override ZString EntryStatus => StatusCodes.AmendmentRequest;

	protected override ICC513CDataProvider GetDataProvider(BusinessObject messageObject) => new CC513CDataProvider(messageSendingAction);

	protected override IXmlMessageBuilder GetProducer(ICC513CDataProvider dataProvider) => new CC513CMessageBuilder(dataProvider);
}

using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BE.Business;

public class PresentationNotificationSenderProvider : AESMessageSender<ICC511CDataProvider>
{
	public PresentationNotificationSenderProvider(ExportEntryMessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
	}

	protected override ZString EntryStatus => StatusCodes.Presented;

	protected override ICC511CDataProvider GetDataProvider(BusinessObject messageObject) => new CC511CDataProvider(messageSendingAction);

	protected override IXmlMessageBuilder GetProducer(ICC511CDataProvider dataProvider) => new CC511CMessageBuilder(dataProvider);
}

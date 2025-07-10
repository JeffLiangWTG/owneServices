using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BE.Business;

public class InvalidationSenderProvider : AESMessageSender<ICC514CDataProvider>
{
	public InvalidationSenderProvider(ExportEntryMessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
	}

	protected override ZString EntryStatus => StatusCodes.InvalidationRequest;

	protected override ICC514CDataProvider GetDataProvider(BusinessObject messageObject) => new CC514CDataProvider(messageSendingAction);

	protected override IXmlMessageBuilder GetProducer(ICC514CDataProvider dataProvider) => new CC514CMessageBuilder(dataProvider);
}

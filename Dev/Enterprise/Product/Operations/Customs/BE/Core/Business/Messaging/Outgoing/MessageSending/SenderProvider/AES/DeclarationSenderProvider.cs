using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BE.Business;

public class DeclarationSenderProvider : AESMessageSender<ICC515CDataProvider>
{
	public DeclarationSenderProvider(ExportEntryMessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
	}

	protected override ZString EntryStatus => ZString.Empty;

	protected override ICC515CDataProvider GetDataProvider(BusinessObject messageObject) => new CC515CDataProvider(messageSendingAction);

	protected override IXmlMessageBuilder GetProducer(ICC515CDataProvider dataProvider) => new CC515CMessageBuilder(dataProvider);
}

using CargoWise.Customs.IN.MessageContracts.SeaCgm;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.IN.Manifest.Business.MessageSending.SeaCgm;

namespace Enterprise.Customs.IN.Manifest.Business;

public class SeaCgmCMCHI21MessageSender : BaseMessageSender<ManifestMessageSendingObject>
{
	public SeaCgmCMCHI21MessageSender(ManifestMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override ZString MessageType => EDIMessageTypeList.Codes.ConsolGeneralManifest;

	protected override ZString MessageSubType => EDIMessageSubTypeList.Codes.SeaCgm;

	protected override ZString GetMessageText()
	{
		var dataProvider = SeaCgmCMCHI21DataProvider.CreateProvider(messageSendingObject.Parent as CGMAsycudaManifestHeader, new SeaCgmCMCHI21AdditionalDataProvider(messageSendingObject));
		return new SeaCgmCMCHI21MessageBuilder(dataProvider).GetFlatFileMessage();
	}
}

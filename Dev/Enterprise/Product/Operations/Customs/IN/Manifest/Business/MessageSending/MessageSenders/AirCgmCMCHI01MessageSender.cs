using CargoWise.Customs.IN.MessageContracts.AirCgm;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.IN.Manifest.Business.MessageSending.AirCgm;

namespace Enterprise.Customs.IN.Manifest.Business;

public class AirCgmCMCHI01MessageSender : BaseMessageSender<ManifestMessageSendingObject>
{
	public AirCgmCMCHI01MessageSender(ManifestMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override ZString MessageType => EDIMessageTypeList.Codes.ConsolGeneralManifest;

	protected override ZString MessageSubType => EDIMessageSubTypeList.Codes.AirCgm;

	protected override ZString GetMessageText()
	{
		var dataProvider = AirCgmCMCHI01DataProvider.CreateProvider(messageSendingObject.Parent as CGMAsycudaManifestHeader, new AirCgmCMCHI01AdditionalDataProvider(messageSendingObject));
		return new AirCgmCMCHI01MessageBuilder(dataProvider).GetFlatFileMessage();
	}
}

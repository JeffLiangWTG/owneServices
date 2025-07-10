using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.Business;

public class CC514CDataProvider : AESMessageHeaderProvider, ICC514CDataProvider
{
	public CC514CDataProvider(ExportEntryMessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
	}

	public IParty Exporter => exporter ?? (exporter = new PartyProvider(declaration.ExporterDocAddress, isTransitionPeriodAES30: declaration.IsTransitionPeriodAES30));
	IParty exporter;

	public IParty Declarant => declarant ?? (declarant = new PartyWithContactProvider(declaration.DeclarantAddress, isTransitionPeriodAES30: declaration.IsTransitionPeriodAES30));
	IParty declarant;

	public IAESRepresentative Representative => representative ?? (representative = RepresentativeProvider.New(declaration.Representative, declaration.JE_DeclarantType, isTransitionPeriodAES30: declaration.IsTransitionPeriodAES30));

	IAESRepresentative representative;

	public override string MessageType => Constants.BECMessageTypes.Outgoing.CC514C;

	public string CustomsOfficeOfExportReferenceNumber => declaration.JE_CustomsOffice;
}

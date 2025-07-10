using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business;

public class CC511CDataProvider : AESMessageHeaderProvider, ICC511CDataProvider
{
	public CC511CDataProvider(ExportEntryMessageSendingAction messageSendingAction) : base(messageSendingAction)
	{
	}

	public IParty Declarant => declarant ?? (declarant = new PartyWithContactProvider(declaration.DeclarantAddress, isTransitionPeriodAES30: declaration.IsTransitionPeriodAES30));
	IParty declarant;

	public IAESRepresentative Representative => representative ?? (representative = RepresentativeProvider.New(declaration.Representative, declaration.JE_DeclarantType, isTransitionPeriodAES30: declaration.IsTransitionPeriodAES30));
	IAESRepresentative representative;

	public override string MessageType => Constants.BECMessageTypes.Outgoing.CC511C;

	public string CustomsOfficeOfPresentationReferenceNumber => CachedValueHelper.GetValue(ref customsOfficeOfPresentationReferenceNumber, () => declaration.CustomsOffices.Where(co => co.CY_Code == EU.Business.EuOfficeCodesTypes.Codes.OfficeOfPresentation).FirstOrDefault() is EuOfficeCode office ? office.CY_Data : null);
	CachedValue<string> customsOfficeOfPresentationReferenceNumber;

	public string CustomsOfficeOfExportReferenceNumber => declaration.JE_CustomsOffice;

	public IConsignment Consignment => consignment ?? (consignment = new ConsignmentProvider(declaration, entryHeader.EntryInstruction, entryHeader));
	IConsignment consignment;
}

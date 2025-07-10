using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class TransitOperationDataProvider : BaseTransitOperationDataProvider, ITransitOperation
{
	public static TransitOperationDataProvider New(NctsHeaderDepartureMessageSendingObject sendingObject) => sendingObject?.NctsHeader?.MovementHeader == null ? null : new TransitOperationDataProvider(sendingObject);

	TransitOperationDataProvider(NctsHeaderDepartureMessageSendingObject sendingObject) : base(sendingObject.NctsHeader)
	{
		this.sendingObject = sendingObject;
		movementHeader = nctsHeader.MovementHeader;
	}
	readonly NctsHeaderDepartureMessageSendingObject sendingObject;
	readonly NctsDepartureMovementHeader movementHeader;

	public string DeclarationType => movementHeader.BM_InBondEntryType;

	public string Security => GetSecurityValueFromCode(movementHeader.BM_TypeOfSecurity);

	public string SpecificCircumstanceIndicator => movementHeader.BM_SpecificCircumstance.ReturnNullIfEmpty();

	public string CommunicationLanguage => nctsHeader.BH_CommunicationLanguage.ToLower();

	public bool BindingItinerary => movementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON && nctsHeader.CountriesOfRouting.Any();

	public string TimeLimitForTransit => movementHeader.BM_ExportTimeLimit.ToString();

	protected override bool IncludeMRN => sendingObject.IsAmend;

	static ZString GetSecurityValueFromCode(ZString typeOfSecurity)
	{
		switch (typeOfSecurity)
		{
			case NctsTypeOfSecurityList.Codes.NON:
				return "0";
			case NctsTypeOfSecurityList.Codes.ENT:
				return "1";
			case NctsTypeOfSecurityList.Codes.EXI:
				return "2";
			case NctsTypeOfSecurityList.Codes.BTH:
				return "3";
			default:
				return ZString.Empty;
		}
	}
}

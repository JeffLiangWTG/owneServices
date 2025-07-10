using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsDepartureMovementHeaderLookups : NctsDepartureMovementHeaderPhase5Lookups
{
	public NctsDepartureMovementHeaderLookups(NctsDepartureMovementHeader parent) : base(parent)
	{
	}
	new NctsDepartureMovementHeader Parent => (NctsDepartureMovementHeader)base.Parent;

	public override CodeDescriptionPairList DeclarationTypeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.NCTSDeclarationType, Parent.ValuationDate);

	protected override CodeDescriptionPairList NctsSpecificCircumstanceIndicatorListCore => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.N0296, Parent.ValuationDate);

	protected override CodeDescriptionPairList BorderModeOfTransportListCore => Factory.GetCachedValue("CH.NCTS.BorderModeOfTransportList", () =>
	{
		var list = new ModeOfTransportList();
		list.RemoveCode(EU.Business.ModeOfTransportList.Codes._5_PostalConsignment);
		return list;
	});

	protected override CodeDescriptionPairList TransportAtBorderTypeOfIdListCore => Factory.GetCachedValue("CH.NCTS.TransportAtBorderTypeOfIdList", () =>
	{
		var list = GetTransportAtBorderTypeOfIdList();
		list.RemoveCode(NctsTransportTypeOfIdList.Codes._11);
		return list;
	});

	protected override CodeDescriptionPairList ModeOfTransportListCore => Factory.GetCachedValue("CH.NCTS.ModeOfTransportList", () =>
	{
		var list = new ModeOfTransportList();
		list.RemoveCode(EU.Business.ModeOfTransportList.Codes._1_SeaTransport);
		list.RemoveCode(EU.Business.ModeOfTransportList.Codes._5_PostalConsignment);
		return list;
	});

	protected override CodeDescriptionPairList TransportAtDepartureTypeOfIdListCore => NctsLookupsHelper.TransportTypeOfIdList(Factory);

	protected override CodeDescriptionPairList NctsTransitStatusListCore => Factory.GetCachedValue<NCTS5DepartureCustomsStatusList>();

	protected override CodeDescriptionPairList NctsMovementHeaderTransactionStatusListCore => Factory.GetCachedValue<DeparturePhaseList>();

	public ExportEntryHeaderCollection ExportEntryHeaderCollection => new ExportEntryHeaderCollection(Parent);

	protected override bool IsOfficeValidForOfficeCodeList(EU.NCTS.Business.NctsEuOfficeCode office) => office.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
}

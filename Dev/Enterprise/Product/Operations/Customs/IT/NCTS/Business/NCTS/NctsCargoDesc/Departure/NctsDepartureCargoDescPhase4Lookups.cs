using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureCargoDescPhase4Lookups : EU.NCTS.Business.NctsDepartureCargoDescPhase4Lookups, INctsDepartureCargoDescLookups
{
	public NctsDepartureCargoDescPhase4Lookups(NctsDepartureCargoDesc parent)
		: base(parent)
	{
	}

	public ICodeDescriptionPairList CPCList => GetProcedureCodesCollection();

	public override RefCountryStatesCollection OriginStates => new RefCountryStatesCollection(Factory, new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, Parent.BY_RN_NKCountryOfOrigin));

	public CodeDescriptionPairList StatusList => Factory.GetCachedValue<EntryLineCustomsStatusList>();

	public CodeDescriptionPairList PortTaxRateList => UniversalReferenceHelper.GetPortTaxRateList(Factory);

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant isBlank")]
	ICodeDescriptionPairList GetProcedureCodesCollection()
	{
		const string transitProcedureCode = UniversalReferenceConstants.RefCusProcedureCodes.Transit;
		const string isBlank = "is blank";

		var result = RefCusProcedureCollection.LoadDistinctCodesForCountry(Factory, Core.Constants.CountryCodes.Italy, ZDateTime.Today);
		result.AdditionalFilter = new ZQuery(RefCusProcedureSchema.ZZ6_ProcedureCode, transitProcedureCode).AddToFilter(RefCusProcedureSchema.ZZ6_Concession, string.Empty);
		result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusProcedureFilters.ProcedureCode, "Property", (ZString)transitProcedureCode, false));
		result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusProcedureFilters.Concession, "ComparisonOperator", (ZString)isBlank, false));
		return result;
	}

	new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;
}

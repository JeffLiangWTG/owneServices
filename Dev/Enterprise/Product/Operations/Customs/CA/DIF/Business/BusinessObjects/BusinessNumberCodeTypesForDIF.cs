using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.CA.DIF.Business;

public static class BusinessNumberCodeTypesForDIF
{
	[ThreadSafe] static readonly ZString[] dIFBusinessNumberCodeTypes =
	{
		OrgCusCode.CACodeTypes.BusinessNumberForImportExport,
		OrgCusCode.CACodeTypes.BusinessNumberForLowValueShipments,
		OrgCusCode.CACodeTypes.BusinessNumberForCorporateIncomeTax,
		OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax,
		OrgCusCode.CACodeTypes.BusinessNumberForPayrollDeductions,
		OrgCusCode.CACodeTypes.BusinessNumberCustomsBroker,
		OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial,
		OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial
	};

	public static ZString[] GetDIFBusinessNumberCodeTypes() => dIFBusinessNumberCodeTypes;
}

using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class ImportCustomsChargeTypeList
	{
		public static string GetImportCustomsChargeTypeValue(ZString chargeId)
		{
			switch (chargeId)
			{
				case Codes.InternalFreightImportingCountry:
					return "01";
				case Codes.InternalInsuranceImportingCountry:
					return "02";
				case Codes.LoadingUnloadingHandlingImportingCountry:
					return "03";
				case Codes.RightsOtherTaxes:
					return "04";
				case Codes.FinancingInterest:
					return "05";
				case Codes.ConstructionInstallationAssembly:
					return "06";
				case Codes.OtherDeductionsCustomsValue:
					return "07";
				case Codes.CommissionsBrokerage:
					return "01";
				case Codes.PackagingReceptacles:
					return "02";
				case Codes.PackingCosts:
					return "03";
				case Codes.MaterialsComponents:
					return "04";
				case Codes.ToolsMatricesMolds:
					return "05";
				case Codes.MaterialsConsumedProduction:
					return "06";
				case Codes.EngineeringProjects:
					return "07";
				case Codes.RoyaltiesLicenseRights:
					return "08";
				case Codes.ValueInstallment:
					return "09";
				case Codes.InternalFreightExportingCountry:
					return "10";
				case Codes.InternalInsuranceExportingCountry:
					return "11";
				case Codes.LoadingUnloadingHandlingExportingCountry:
					return "12";
				case Codes.InternationalLoadingUnloadingHandling:
					return "15";
				case Codes.LoadingUnloadingHandlingEntranceImportingCountry:
					return "16";
				case Codes.OtherAdditionsCustomsValue:
					return "17";
				default:
					return string.Empty;
			}
		}

		internal static ZString[] OverseasFreightChargeTypes => new ZString[] { ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid };
	}
}

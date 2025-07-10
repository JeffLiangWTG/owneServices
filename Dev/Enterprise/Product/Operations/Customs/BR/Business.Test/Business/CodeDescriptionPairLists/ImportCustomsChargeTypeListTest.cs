using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ImportCustomsChargeTypeListTest : TestCase
	{
		public void TestGetProcessRelatedTypeValue()
		{
			var result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue("");
			AssertEquals("Result empty", ZString.Empty, result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.InternalFreightImportingCountry);
			AssertEquals("InternalFreightImportingCountry", "01", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.InternalInsuranceImportingCountry);
			AssertEquals("InternalInsuranceImportingCountry", "02", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.LoadingUnloadingHandlingImportingCountry);
			AssertEquals("LoadingUnloadingHandlingImportingCountry", "03", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.RightsOtherTaxes);
			AssertEquals("RightsOtherTaxes", "04", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.FinancingInterest);
			AssertEquals("FinancingInterest", "05", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.ConstructionInstallationAssembly);
			AssertEquals("ConstructionInstallationAssembly", "06", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.OtherDeductionsCustomsValue);
			AssertEquals("OtherDeductionsCustomsValue", "07", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.CommissionsBrokerage);
			AssertEquals("CommissionsBrokerage", "01", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.PackagingReceptacles);
			AssertEquals("PackagingReceptacles", "02", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.PackingCosts);
			AssertEquals("PackingCosts", "03", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.MaterialsComponents);
			AssertEquals("MaterialsComponents", "04", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.ToolsMatricesMolds);
			AssertEquals("ToolsMatricesMolds", "05", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.MaterialsConsumedProduction);
			AssertEquals("MaterialsConsumedProduction", "06", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.EngineeringProjects);
			AssertEquals("EngineeringProjects", "07", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.RoyaltiesLicenseRights);
			AssertEquals("RoyaltiesLicenseRights", "08", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.ValueInstallment);
			AssertEquals("ValueInstallment", "09", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.InternalFreightExportingCountry);
			AssertEquals("InternalFreightExportingCountry", "10", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.InternalInsuranceExportingCountry);
			AssertEquals("InternalInsuranceExportingCountry", "11", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.LoadingUnloadingHandlingExportingCountry);
			AssertEquals("LoadingUnloadingHandlingExportingCountry", "12", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.InternationalLoadingUnloadingHandling);
			AssertEquals("InternationalLoadingUnloadingHandling", "15", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.LoadingUnloadingHandlingEntranceImportingCountry);
			AssertEquals("LoadingUnloadingHandlingEntranceImportingCountry", "16", result);

			result = ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(ImportCustomsChargeTypeList.Codes.OtherAdditionsCustomsValue);
			AssertEquals("OtherAdditionsCustomsValue", "17", result);
		}
	}
}


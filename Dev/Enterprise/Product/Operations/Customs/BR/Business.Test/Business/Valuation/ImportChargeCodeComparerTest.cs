using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportChargeCodeComparerTest : TestCase
	{
		public void TestCompare()
		{
			CombineAssertions(() =>
			{
				AssertEquals("FNT > OFC", -1, new ImportChargeCodeComparer().Compare(ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, ImportCustomsChargeTypeList.Codes.OverseasFreightCollect));
				AssertEquals("OFC > CIA", -1, new ImportChargeCodeComparer().Compare(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, ImportCustomsChargeTypeList.Codes.ConstructionInstallationAssembly));
				AssertEquals("ROT < CIA", 1, new ImportChargeCodeComparer().Compare(ImportCustomsChargeTypeList.Codes.RightsOtherTaxes, ImportCustomsChargeTypeList.Codes.ConstructionInstallationAssembly));
				AssertEquals("CIA < COM", 1, new ImportChargeCodeComparer().Compare(ImportCustomsChargeTypeList.Codes.ConstructionInstallationAssembly, ImportCustomsChargeTypeList.Codes.CommissionsBrokerage));
				AssertEquals("COM < OFC", 1, new ImportChargeCodeComparer().Compare(ImportCustomsChargeTypeList.Codes.CommissionsBrokerage, ImportCustomsChargeTypeList.Codes.OverseasFreightCollect));
			});
		}
	}
}

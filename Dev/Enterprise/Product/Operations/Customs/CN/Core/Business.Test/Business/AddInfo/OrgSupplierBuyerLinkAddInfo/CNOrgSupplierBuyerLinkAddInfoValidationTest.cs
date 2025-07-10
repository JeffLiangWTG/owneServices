using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNOrgSupplierBuyerLinkAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZO_ProcedureCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("CN", "A", "10", "", "", "Import", "IMP");
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var link = Factory.New<OrgSupplierBuyerLink>();
				var addInfo = new CNOrgSupplierBuyerLinkAddInfo(link.GetAddInfo());
				link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.China;
				addInfo.ZO_ProcedureCode = "A1";
				AssertHasWarningContaining(addInfo.ZO_ProcedureCodeInfo, ListValidation.InvalidCodeMessage);
				addInfo.ZO_ProcedureCode = "10";
				AssertNoWarnings(addInfo.ZO_ProcedureCodeInfo);
			}
		}

		public void TestCheckZO_LevyType()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			var addInfo = new CNOrgSupplierBuyerLinkAddInfo(link.GetAddInfo());
			addInfo.ZO_LevyType = "A1";
			AssertHasWarningContaining(addInfo.ZO_LevyTypeInfo, ListValidation.InvalidCodeMessage);
			addInfo.ZO_LevyType = LevyTypeList.Codes._101;
			AssertNoWarnings(addInfo.ZO_LevyTypeInfo);
		}

		public void TestCheckZO_ManualNo()
		{
			var link = Factory.New<OrgSupplierBuyerLink>();
			var addInfo = new CNOrgSupplierBuyerLinkAddInfo(link.GetAddInfo());
			var targetInfo = addInfo.ZO_ManualNoInfo;
			AssertNoWarnings(targetInfo);
			addInfo.ZO_ManualNo = "12AArr33";
			AssertHasWarningContaining(targetInfo, CusEntryInstructionValidation.ManualNumberFormatErrorMessage);
			addInfo.ZO_ManualNo = "A12345678901";
			AssertNoWarnings(targetInfo);
			addInfo.ZO_LevyType = "201";
			addInfo.ZO_ManualNo = "A12345678901";
			AssertHasWarningContaining(targetInfo, "Manual Number should start with Z");
			addInfo.ZO_ManualNo = "Z12345678901";
			AssertNoWarnings(targetInfo);
			addInfo.ZO_LevyType = "501";
			addInfo.ZO_ManualNo = "A12345678901";
			AssertHasWarningContaining(targetInfo, "Manual Number should start with D");
		}
	}
}

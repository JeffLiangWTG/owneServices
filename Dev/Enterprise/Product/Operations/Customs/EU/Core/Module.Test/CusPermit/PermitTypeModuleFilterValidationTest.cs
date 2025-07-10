using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Module.Testing
{
	class PermitTypeModuleFilterValidationTest : TestCaseWithFactory
	{
		public void TestCheckProperty3()
		{
			Factory.CreateSupportingDocumentCodeLists(new TestSupportingDocumentCodeList("2800"), new TestSupportingDocumentCodeList("3200"));
			Factory.Save();

			var permitTypeModuleFilter = new PermitTypeModuleFilter("Permit Type", null, () => new GlbCompanyCollection(Factory)
			{
				GlbCompany.CurrentCompany
			});
			permitTypeModuleFilter.DefaultProperty0 = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			permitTypeModuleFilter.Property3 = "6600";
			AssertHasWarningContaining(permitTypeModuleFilter.Property3Info, ListValidation.InvalidCodeMessage);

			permitTypeModuleFilter.Property3 = "2800";
			AssertNoWarningContaining(permitTypeModuleFilter.Property3Info, ListValidation.InvalidCodeMessage);
		}
	}
}

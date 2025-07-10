using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class CusExitConsignmentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOrganizationsFindBoxList()
		{
			AssertType<OrganisationsFindBoxCollection>(lookups.OrganizationsFindBoxList);
		}

		public void TestStatusList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "Export Customs Status");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NctsCustomsStatus, "NCTS Customs Status");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "111", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "222", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "333", ZDateTime.Today.AddDays(-4), ZDateTime.Today.AddDays(-2));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "444", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NctsCustomsStatus, "555", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			CombineAssertions(() =>
			{
				var list = lookups.StatusList;
				AssertEquals("CodesAsString", "111, 222", list.CodesAsString);
				AssertSame("Cached", list, lookups.StatusList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new CusExitConsignmentLookups(Factory.NewWithValidTestData<CusExitConsignment>());
		}
		CusExitConsignmentLookups lookups;
	}
}

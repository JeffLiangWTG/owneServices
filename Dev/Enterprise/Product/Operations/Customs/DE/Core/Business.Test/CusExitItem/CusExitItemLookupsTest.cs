using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CusExitItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "Export Customs Status");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "123", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "515", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			var exitDetail = Factory.New<CusExitDetail>();
			var exitItem = exitDetail.CusExitItems.AddNew();
			CombineAssertions(() =>
			{
				var list = exitItem.Lookups.StatusList;
				AssertEquals("CodesAsString", "123, 515", list.CodesAsString);
				AssertSame("Cached", exitDetail.Lookups.StatusList, list);
			});
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	class CusExitReportLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageStatusList()
		{
			(var report, _, var header) = CusExitReportTest.GetNewBusinessObject(Factory);
			var list = report.Lookups.MessageStatusList;
			CombineAssertions(() =>
			{
				AssertCollectionContains("SNT", IELogicalStatusList.Codes.Sent, list.GetAllCodes());
				AssertCollectionContains("MLT", IELogicalStatusList.Codes.SeeEntries, list.GetAllCodes());
				AssertCollectionNotContains("REL", AESEntryStatusList.Codes.ReleasedForExport, list.GetAllCodes());
				AssertSame("MessageStatusList Cached", list, report.Lookups.MessageStatusList);
			});
		}
	}
}

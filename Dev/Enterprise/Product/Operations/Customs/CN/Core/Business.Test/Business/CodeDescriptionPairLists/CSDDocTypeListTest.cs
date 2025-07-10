using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CSDDocTypeListTest : TestCaseWithFactory
	{
		public void TestCSDocTypeListUntranslatable()
		{
			var testList = Factory.GetCachedValue<CSDDocTypeList>();
			Assert("CS Doc type list should be untranslatable", testList is UntranslatableCodeDescriptionPairList);
		}

		public void TestGetCachedCSDDocTypeList()
		{
			var importDocTypeList = CSDDocTypeList.GetCachedCSDDocTypeList(Factory, true);
			AssertSame("Should have been cached.", importDocTypeList, CSDDocTypeList.GetCachedCSDDocTypeList(Factory, true));
			AssertContainsExactElementsInExactOrder(new[]
			{
				"00000001", "00000002", "00000003", "00000004", "00000008", "00000009", "00000010", "00000011", "00000012", "00000013", "00000014", "00000015", "00000016", "00000017",
				"10000001", "10000002", "10000003", "10000004",
				"20000011", "20000012", "20000013", "20000014", "20000015", "20000016", "20000017", "20000018", "20000019", "20000020", "20000021", "20000022", "20000023", "20000025", "20000026", "20000028", "20000029", "20000030",
				"50000001", "50000002", "50000003", "50000004", "50000005", "50000006", "50000007", "50000008", "50000009", "50000010", "50000011",
				"60000001", "60000007", "60000008", "60000009", "60000011",
				"80000001", "80000002", "80000003", "80000004",
			}, importDocTypeList.GetAllCodes());

			var exportDocTypeList = CSDDocTypeList.GetCachedCSDDocTypeList(Factory, false);
			AssertSame("Should have been cached.", exportDocTypeList, CSDDocTypeList.GetCachedCSDDocTypeList(Factory, false));
			AssertContainsExactElementsInExactOrder(new[]
			{
				"00000001", "00000002", "00000003", "00000004", "00000008", "00000009", "00000010", "00000015",
				"10000001", "10000002", "10000003", "10000004",
				"20000011", "20000012", "20000013", "20000015", "20000017", "20000018", "20000019", "20000020", "20000022", "20000023", "20000024", "20000025", "20000026", "20000027", "20000028", "20000029", "20000030",
				"50000001", "50000002", "50000003", "50000004", "50000005", "50000007", "50000008", "50000010", "50000011", "50000012", "50000013", "50000014",
				"60000002", "60000003", "60000004", "60000005", "60000006", "60000007", "60000008", "60000009", "60000010", "60000011",
			}, exportDocTypeList.GetAllCodes());
		}

		public void TestCanLinkToInvoiceLine()
		{
			AssertContainsExactElementsInAnyOrder(new[] {
				CSDDocTypeList.Codes._80000001,
				CSDDocTypeList.Codes._80000002,
				CSDDocTypeList.Codes._80000003,
				CSDDocTypeList.Codes._80000004
			}, new CSDDocTypeList().GetAllCodes().Where(x => CSDDocTypeList.CanLinkToInvoiceLine(x)));
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAdditionalInfoCodeList()
		{
			var header = Factory.New<AdditionalInfo>();
			var list = header.Lookups.CodeList;
			var codeList = list as CodeDescriptionPairList;

			CombineAssertions(() =>
			{
				AssertEquals(2, list.Count);
				AssertSame(list, header.Lookups.CodeList);

				Assert(codeList.ContainsCode("10600"));
				AssertEquals("Consignee Unknown", codeList.GetDescriptionFromCode("10600"));

				Assert(codeList.ContainsCode("10900"));
				AssertEquals("ENS lodged together with custom declaration for low value consignments", codeList.GetDescriptionFromCode("10900"));
			});
		}
	}
}

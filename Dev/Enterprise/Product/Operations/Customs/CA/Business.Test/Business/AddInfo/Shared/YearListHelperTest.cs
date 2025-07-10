using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class YearListHelperTest : TestCaseWithFactory
	{
		public void TestYearList()
		{
			var list = YearListHelper.GetYearList(1900);

			Assert(list.ContainsCode("1900"));
			Assert(list.ContainsCode("1899"));
			Assert(list.ContainsCode("1898"));
			Assert(list.ContainsCode("1897"));
			Assert(list.ContainsCode("1896"));
			Assert(list.ContainsCode("1895"));
			Assert(list.ContainsCode("1894"));
			Assert(list.ContainsCode("1893"));
			Assert(list.ContainsCode("1892"));
			Assert(list.ContainsCode("1891"));
			Assert(list.ContainsCode("1890"));
			Assert(list.ContainsCode("1889"));
			Assert(list.ContainsCode("1888"));
			Assert(list.ContainsCode("1887"));
			Assert(list.ContainsCode("1886"));
			Assert(list.ContainsCode("1885"));
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.Testing
{
	public class FRCountrySpecificValueProviderTest : TestCaseWithFactory
	{
		public void TestGetCountrySpecificValueList()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_StatisticalValue = 123m;
			var provider = new FRCountrySpecificValueProvider();
			AssertEquals(1, provider.GetCountrySpecificValueList(entryLine).Count);
			AssertEquals(123m, provider.GetCountrySpecificValueList(entryLine)["STATVAL"]);
		}
	}
}

using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.FilterStrips.Testing
{
	sealed class FilterNameAndValueTest : TestCase
	{
		public void TestConstructor()
		{
			ZString expectedFilterName = "FilterName";
			ZString[] values = new ZString[] { "Value1", "Value2" };
			ZString expectedValue1 = "Value1";
			ZString expectedValue2 = "Value2";
			FilterNameAndValue pair = new FilterNameAndValue(expectedFilterName, values);

			AssertEquals("Filter Name", expectedFilterName, pair.FilterName);
			AssertEquals("Value1", expectedValue1, pair.Values[0]);
			AssertEquals("Value2", expectedValue2, pair.Values[1]);
		}
	}
}

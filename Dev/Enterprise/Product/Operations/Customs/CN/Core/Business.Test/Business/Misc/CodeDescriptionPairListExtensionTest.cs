using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CodeDescriptionPairListExtensionTest : TestCaseWithFactory
	{
		public void TestFilterListByCodes()
		{
			var testList = new CodeDescriptionPairList { new CodeDescriptionPair("1", "item1"), new CodeDescriptionPair("2", "item2"), new CodeDescriptionPair("3", "item3"), new CodeDescriptionPair("4", "item4"), new CodeDescriptionPair("5", "item5"), };
			var testResult = testList.FilterListByCodes(new[] { "2", "4", "X", "3" });
			var expectedList = new[]
			{
				 new CodeDescriptionPair("2", "item2"),
				 new CodeDescriptionPair("3", "item3"),
				 new CodeDescriptionPair("4", "item4"),
			};
			AssertContainsExactElementsInExactOrder(expectedList, testResult);
			AssertNoExceptionThrown(() => testList.FilterListByCodes(new[] { "x" }));
		}
	}
}

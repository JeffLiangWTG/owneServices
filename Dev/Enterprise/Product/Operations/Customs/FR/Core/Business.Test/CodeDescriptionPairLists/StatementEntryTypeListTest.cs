using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CodeDescriptionPairLists.Testing
{
	public class StatementEntryTypeListTest : TestCase
	{
		public void TestGetAllCodes()
		{
			var list = new StatementEntryTypeList();
			AssertContainsExactElementsInAnyOrder(new[] { "IMP", "EXP", "DCG" }, list.GetAllCodes());
		}
	}
}

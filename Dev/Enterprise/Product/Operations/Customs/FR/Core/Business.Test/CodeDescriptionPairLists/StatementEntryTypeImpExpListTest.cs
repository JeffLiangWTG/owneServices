using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CodeDescriptionPairLists.Testing
{
	public class StatementEntryTypeImpExpListTest : TestCase
	{
		public void TestGetAllCodes()
		{
			var list = new StatementEntryTypeImpExpList();
			AssertContainsExactElementsInAnyOrder(new[] { "IMP", "EXP" }, list.GetAllCodes());
		}
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Snapshot.Testing
{
	class ChildDataTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var childData = new ChildData("DOC", "AAA", "ABC123");
			AssertEquals(ChildType.DOC, childData.Type);
			AssertEquals("AAA", childData.Code);
			AssertEquals("ABC123", childData.Reference);

			childData = new ChildData("CAN", "AAA", "ABC123");
			AssertEquals(ChildType.CAN, childData.Type);

			childData = new ChildData("CAC", "AAA", "ABC123");
			AssertEquals(ChildType.CAC, childData.Type);

			childData = new ChildData("ZZZ", "AAA", "ABC123");
			AssertEquals(ChildType.DOC, childData.Type);
		}

		public void TestTypeConstants()
		{
			AssertEquals("DOC", ChildData.DOC);
			AssertEquals("CAN", ChildData.CAN);
			AssertEquals("CAC", ChildData.CAC);
		}
	}
}

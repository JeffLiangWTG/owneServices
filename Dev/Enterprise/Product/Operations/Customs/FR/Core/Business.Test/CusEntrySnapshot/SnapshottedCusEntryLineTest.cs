using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Snapshot.Testing
{
	class SnapshottedCusEntryLineTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var childData1 = new ChildData("DOC", "AAA", "ABC123");
			var childData2 = new ChildData("CAC", "Z000", ZString.Empty);
			var childDatas = new ChildData[] { childData1, childData2 };

			var lineNumber = 1;

			var entrySnapshot = new SnapshottedCusEntryLine(lineNumber, childDatas);

			AssertEquals(1, entrySnapshot.LineNumber);
			AssertEquals(2, entrySnapshot.Children.Length);
			AssertEquals("AAA", entrySnapshot.Children[0].Code);
			AssertEquals("Z000", entrySnapshot.Children[1].Code);
			AssertEquals(ChildType.DOC, entrySnapshot.Children[0].Type);
			AssertEquals(ChildType.CAC, entrySnapshot.Children[1].Type);
			AssertEquals("ABC123", entrySnapshot.Children[0].Reference);
			AssertEquals(ZString.Empty, entrySnapshot.Children[1].Reference);
		}
	}
}

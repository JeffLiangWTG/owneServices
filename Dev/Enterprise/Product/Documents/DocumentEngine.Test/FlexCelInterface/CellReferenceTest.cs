using NUnit.Framework;

namespace Enterprise.DocumentEngine.FlexCelInterface.Testing
{
	sealed class CellReferenceTest : TestCase
	{
		public void TestIsEmpty()
		{
			AssertEquals("new CellReference().IsEmpty", true, new CellReference().IsEmpty);
			AssertEquals("CellReference.UnKnown.IsEmpty", true, CellReference.UnKnown.IsEmpty);
			AssertEquals("new CellReference(null, null).IsEmpty", true, new CellReference(null, null).IsEmpty);
			AssertEquals("new CellReference('Something', 'Else').IsEmpty", false, new CellReference("Something", "Else").IsEmpty);
		}

		public void TestConstructor()
		{
			CellReference cellReference = new CellReference();
			AssertEquals("cellReference.SheetName", "(unknown)", cellReference.SheetName);
			AssertEquals("cellReference.Cell", "N/A", cellReference.Cell);

			cellReference = new CellReference("Freddy", "G8");
			AssertEquals("cellReference.SheetName", "Freddy", cellReference.SheetName);
			AssertEquals("cellReference.Cell", "G8", cellReference.Cell);

			cellReference = new CellReference("Johnny", 3, 5);
			AssertEquals("cellReference.SheetName", "Johnny", cellReference.SheetName);
			AssertEquals("cellReference.Cell", "F4", cellReference.Cell);
		}

		public void Test0()
		{
			AssertEquals("A1", CellReference.GetCellRef(0, 0));
		}

		public void TestRow()
		{
			AssertEquals("A5", CellReference.GetCellRef(4, 0));
		}

		public void Test1()
		{
			AssertEquals("B1", CellReference.GetCellRef(0, 1));
		}

		public void Test25()
		{
			AssertEquals("Z1", CellReference.GetCellRef(0, 25));
		}

		public void Test26()
		{
			AssertEquals("AA1", CellReference.GetCellRef(0, 26));
		}

		public void Test27()
		{
			AssertEquals("AB1", CellReference.GetCellRef(0, 27));
		}

		public void TestWithThreeLetters()
		{
			AssertEquals("ZZ1", CellReference.GetCellRef(0, 701));
			AssertEquals("AAA2", CellReference.GetCellRef(1, 702));
			AssertEquals("AAB3", CellReference.GetCellRef(2, 703));
		}
	}
}

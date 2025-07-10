namespace CargoWise.EntityFramework.Testing.DataAccess
{
	public class DbIndex
	{
		internal DbIndex(string[] columnNames)
		{
			this.ColumnNames = columnNames;
		}

		public readonly string[] ColumnNames;
	}
}

#region Test

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class DbIndexTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			DbIndex index = new DbIndex(new string[] { "Column1", "Column2" });
			AssertEquals(2, index.ColumnNames.Length);
			AssertEquals("Column1", index.ColumnNames[0]);
			AssertEquals("Column2", index.ColumnNames[1]);
		}
	}
}

#endregion

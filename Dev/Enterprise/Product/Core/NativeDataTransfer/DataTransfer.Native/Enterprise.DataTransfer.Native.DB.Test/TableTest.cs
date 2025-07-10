using System.Linq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.DB
{
	class TableTest : TestCase
	{
		public void TestReferenceTables()
		{
			var orgHeader = Table.Get("OrgHeader");
			var referenceTables = orgHeader.ReferenceTables;
			Assert("Reference Tables should have RefUNLOCO", referenceTables.Any(t => t.Name.Equals("RefUNLOCO")));

			var zonePivot = Table.Get("RefZonePivot");
			referenceTables = zonePivot.ReferenceTables;
			AssertEquals("Reference Tables should only have one table", 1, referenceTables.Count());
		}

		public void TestForeignKeys()
		{
			var orgContact = Table.Get("OrgContact");
			var foreignKeys = orgContact.Columns.ForeignKeys;
			Assert("Foreign Key should have OC_OH", foreignKeys.Any(c => c.Name.Equals("OC_OH")));

			var orgHeader = Table.Get("OrgHeader");
			foreignKeys = orgHeader.Columns.ForeignKeys;
			Assert("Foreign Key should have OH_RL_NKClosestPort", foreignKeys.Any(c => c.Name.Equals("OH_RL_NKClosestPort")));

			var zonePivot = Table.Get("RefZonePivot");
			foreignKeys = zonePivot.Columns.ForeignKeys;
			Assert("Foreign Key should have F2_ParentID", foreignKeys.Any(c => c.Name.Equals("F2_ParentID")));
		}

		#region Find Column

		public void TestGetColumnByColumnName()
		{
			AssertEquals("Z0_Code", table.Columns["Z0_Code"].Name);
		}

		#endregion

		#region Table Prefix

		public void TestGetTablePrefix()
		{
			var result = table.Prefix;
			AssertEquals("Z0", result);
		}

		#endregion

		#region Column Definition

		public void TestGetPrimaryKeys()
		{
			var result = table.Columns.PrimaryKey;

			AssertNotNull("Table should have one and only one Primary Key", result);
		}

		public void TestGetTableCodes()
		{
			table = Table.Get("RefZonePivot");

			var result = table.Columns.TableCodes;

			AssertEquals(1, result.Count());
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			table = Table.Get("DummyBizo");
		}

		#endregion

		Table table;
	}

	class TableExtensionTest : TestCase
	{
		public void TestToDataTable()
		{
			var tableDef = Table.Get("DummyBizo");
			var dataTable = tableDef.ToDataTable();
			AssertEquals("Table Name should be equal", "DummyBizo", dataTable.TableName);
			AssertEquals("Should have same number of column", tableDef.Columns.Count(), dataTable.Columns.Count);
		}
	}
}

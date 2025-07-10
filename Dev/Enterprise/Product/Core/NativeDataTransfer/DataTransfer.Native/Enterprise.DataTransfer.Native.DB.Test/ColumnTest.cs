using System;
using Enterprise.DataTransfer.Native.DB.Keys;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.DB
{
	public class ColumnTest : TestCase
	{
		public void TestEquals()
		{
			var column = new ColumnDef(table, "column", "uint");
			var other = new ColumnDef(table, "column", "uint");
			AssertEquals(true, column.Equals(other));

			other = new ColumnDef(table, "column1", "uint");
			AssertEquals(false, column.Equals(other));

			other = new ColumnDef(table, "column", "char");
			AssertEquals(false, column.Equals(other));

			other = new ColumnDef(table, "column1", "char");
			AssertEquals(false, column.Equals(other));
		}

		public void TestGetHumanName()
		{
			var column = new ColumnDef(table, "F2_Z0", "uniqueidentifier");
			AssertEquals("DummyBizo", column.GetHumanName());

			column = new ColumnDef(table, "F2_Z0_NKSomething", "uniqueidentifier");
			AssertEquals("Something", column.GetHumanName());

			column = new ColumnDef(table, "F2_Z0", "char");
			AssertEquals("Z0", column.GetHumanName());

			column = new ColumnDef(table, "F2_A_Natural", "char");
			AssertEquals("A_Natural", column.GetHumanName());

			column = new ColumnDef(table, "F2_ABCD_Natural", "char");
			AssertEquals("ABCD_Natural", column.GetHumanName());

			column = new ColumnDef(table, "F2_Z0_Natural", "char");
			AssertEquals("Natural", column.GetHumanName());

			column = new ColumnDef(table, "F2_Z0_NK", "uniqueidentifier");
			AssertEquals("", column.GetHumanName());

			column = new ColumnDef(table, "F2", "uniqueidentifier");
			AssertEquals("F2", column.GetHumanName());
		}

		public void TestIsDefaultValueBoolean()
		{
			var column = new ColumnDef(table);
			AssertEquals(false, column.DoesDataTypeRepresentBoolean());

			column.DataType = DbDataType.Char;
			AssertEquals(false, column.DoesDataTypeRepresentBoolean());

			column.DefaultValue = "('Y')";
			AssertEquals(true, column.DoesDataTypeRepresentBoolean());

			column.DefaultValue = "('N')";
			AssertEquals(true, column.DoesDataTypeRepresentBoolean());
		}

		public void TestInvalidPrefixHasException()
		{
			var key = new ForeignKey(new ColumnDef(table, "F2_aweeg_we", "uniqueidentifier"));
			AssertExceptionThrown<InvalidOperationException>("TestInvalidPrefixHasException didn't throw the expected exception message",
				"The column name [F2_aweeg_we] does not have a valid foreign key prefix", () => _ = key.ReferenceTable);
		}

		protected override void SetUp()
		{
			base.SetUp();
			table = new Table();
		}
		Table table;
	}
}

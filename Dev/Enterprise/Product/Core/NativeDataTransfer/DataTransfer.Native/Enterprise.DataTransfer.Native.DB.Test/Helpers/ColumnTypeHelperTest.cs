using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.DB.Helpers
{
	public class ColumnTypeHelperTest : TestCase
	{
		public void TestIsTableName()
		{
			helper.ColumnDef = new ColumnDef(table, "F2_ParentTableCode", "char");
			AssertEquals("helper.IsTableName() for F2_ParentTableCode", false, helper.IsTableName());

			helper.ColumnDef = new ColumnDef(table, "CE_ParentTable", "char");
			AssertEquals("helper.IsTableName() for CE_ParentTable", true, helper.IsTableName());

			helper.ColumnDef = new ColumnDef(table, "K2_FKSomething", "char");
			AssertEquals("helper.IsTableName() for K2_FKSomething", false, helper.IsTableName());
		}

		public void TestHasKeyName()
		{
			helper.ColumnDef = new ColumnDef(table, "", "");
			AssertEquals(false, helper.HasKeyName());

			helper.ColumnDef = new ColumnDef(table, "K2", "");
			AssertEquals(false, helper.HasKeyName());

			helper.ColumnDef = new ColumnDef(table, "K2_FKSomething", "");
			AssertEquals(false, helper.HasKeyName());

			helper.ColumnDef = new ColumnDef(table, "K2_PK", "");
			AssertEquals(true, helper.HasKeyName());
		}

		public void TestHasParentIdName()
		{
			helper.ColumnDef = new ColumnDef(table, "", "");
			AssertEquals(false, helper.HasParentIdName());

			helper.ColumnDef = new ColumnDef(table, "K2", "");
			AssertEquals(false, helper.HasParentIdName());

			helper.ColumnDef = new ColumnDef(table, "K2_ParentID", "");
			AssertEquals(true, helper.HasParentIdName());

			helper.ColumnDef = new ColumnDef(table, "K2_ForeignKey", "");
			AssertEquals(true, helper.HasParentIdName());

			helper.ColumnDef = new ColumnDef(table, "K2_FK_Something", "");
			AssertEquals(false, helper.HasParentIdName());
		}

		public void TestIsTableCode()
		{
			helper.ColumnDef = new ColumnDef(table, "F2_ParentTableCode", "char");
			AssertEquals(true, helper.IsTableCode());

			helper.ColumnDef = new ColumnDef(table, "F2_Something", "char");
			AssertEquals(false, helper.IsTableCode());
		}

		public void TestIsPolymorphicKey()
		{
			helper.ColumnDef = new ColumnDef(table, "F2_ParentID", "uniqueidentifier");
			AssertEquals(true, helper.IsPolyMorphicKey());

			helper.ColumnDef = new ColumnDef(table, "F2_ForeignKey", "uniqueidentifier");
			AssertEquals(true, helper.IsPolyMorphicKey());

			helper.ColumnDef = new ColumnDef(table, "F2_Something", "uniqueidentifier");
			AssertEquals(false, helper.IsPolyMorphicKey());

			helper.ColumnDef = new ColumnDef(table, "F2_ParentID", "char");
			AssertEquals(false, helper.IsPolyMorphicKey());

			helper.ColumnDef = new ColumnDef(table, "F2_ForeignKey", "char");
			AssertEquals(false, helper.IsPolyMorphicKey());

			helper.ColumnDef = new ColumnDef(table, "F2_Something", "char");
			AssertEquals(false, helper.IsPolyMorphicKey());
		}

		public void TestIsForeignKey()
		{
			helper.ColumnDef = new ColumnDef(table, "F2_Z0", "uniqueidentifier");
			AssertEquals(true, helper.IsForeignKey());

			helper.ColumnDef = new ColumnDef(table, "F2_Z0_Something", "uniqueidentifier");
			AssertEquals(true, helper.IsForeignKey());

			helper.ColumnDef = new ColumnDef(table, "F2_PK", "uniqueidentifier");
			AssertEquals(false, helper.IsForeignKey());

			helper.ColumnDef = new ColumnDef(table, "F2_PK_Something", "uniqueidentifier");
			AssertEquals(false, helper.IsForeignKey());

			helper.ColumnDef = new ColumnDef(table, "F2_Something", "uniqueidentifier");
			AssertEquals(false, helper.IsForeignKey());

			helper.ColumnDef = new ColumnDef(table, "F2_Z0", "char");
			AssertEquals(false, helper.IsForeignKey());

			helper.ColumnDef = new ColumnDef(table, "F2_ParentID", "char");
			AssertEquals(false, helper.IsForeignKey());
		}

		public void TestIsNaturalKey()
		{
			helper.ColumnDef = new ColumnDef(table, "F2_FK_NK", "uniqueidentifier");
			AssertEquals(true, helper.IsNaturalKey());

			helper.ColumnDef = new ColumnDef(table, "F2_NK", "uniqueidentifier");
			AssertEquals(true, helper.IsNaturalKey());

			helper.ColumnDef = new ColumnDef(table, "F2_NK", "char");
			AssertEquals(true, helper.IsNaturalKey());

			helper.ColumnDef = new ColumnDef(table, "F2_PK", "uniqueidentifier");
			AssertEquals(false, helper.IsNaturalKey());
		}

		public void TestIsPrimaryKey()
		{
			helper.ColumnDef = new ColumnDef(table, "F2_PK", "uniqueidentifier");
			AssertEquals(true, helper.IsPrimaryKey());

			helper.ColumnDef = new ColumnDef(table, "F2_PK_Something", "uniqueidentifier");
			AssertEquals(true, helper.IsPrimaryKey());

			helper.ColumnDef = new ColumnDef(table, "F2_PK", "char");
			AssertEquals(false, helper.IsPrimaryKey());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			helper = new ColumnTypeHelper();
			table = new Table();
		}

		ColumnTypeHelper helper;
		Table table;
		#endregion
	}
}

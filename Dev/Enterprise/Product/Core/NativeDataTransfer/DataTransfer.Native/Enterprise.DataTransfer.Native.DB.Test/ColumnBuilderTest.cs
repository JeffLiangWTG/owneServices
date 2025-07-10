using System;
using Enterprise.DataTransfer.Native.DB.Keys;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.DB
{
	public class ColumnBuilderTest : TestCase
	{
		public void TestConstruct()
		{
			var column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals("ZD1_Z0", column.Name);
			AssertEquals(DbDataType.UniqueIdentifier, column.DataType);
			AssertEquals(true, column.DoesNotRequireAValue);
			AssertEquals(true, column.Nullable);
			AssertEquals(0, column.Length);
		}

		public void TestConstruct_CreatePrimaryKey()
		{
			columnSchema.Name = "ZD1_PK";
			var column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(ColumnType.PrimaryKey, column.Type);
			AssertEquals(typeof(PrimaryKey), column.GetType());
		}

		public void TestConstruct_CreateForeignKey()
		{
			var column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(ColumnType.ForeignKey, column.Type);
			AssertEquals(typeof(ForeignKey), column.GetType());
		}

		public void TestConstruct_CreateNaturalKey()
		{
			columnSchema.Name = "ZD1_Z0_NKParentID";
			var column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(ColumnType.NaturalKey, column.Type);
			AssertEquals(typeof(NaturalKey), column.GetType());
		}

		public void TestConstruct_CreatePolymorphicKey()
		{
			columnSchema.Name = "ZD1_ParentID";
			var column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(typeof(PolymorphicKey), column.GetType());

			columnSchema.Name = "ZD1_ForeignKey";
			column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(typeof(PolymorphicKey), column.GetType());
		}

		public void TestConstruct_CreateTableCode()
		{
			columnSchema.Name = "ZD1_ParentTableCode";
			var column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(typeof(ColumnDef), column.GetType());
		}

		public void TestConstruct_IsNullable()
		{
			columnSchema.DefaultValue = DBNull.Value;
			columnSchema.IsNullable = "YES";
			var column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(true, column.Nullable);

			columnSchema.DefaultValue = DBNull.Value;
			columnSchema.IsNullable = "NO";
			column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(false, column.Nullable);

			columnSchema.DefaultValue = "(\"N\")";
			columnSchema.IsNullable = "YES";
			column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(true, column.Nullable);

			columnSchema.DefaultValue = "(\"N\")";
			columnSchema.IsNullable = "NO";
			column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(false, column.Nullable);
		}

		public void TestConstruct_DoesNotRequireAValue()
		{
			columnSchema.DefaultValue = DBNull.Value;
			columnSchema.IsNullable = "YES";
			var column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(true, column.DoesNotRequireAValue);

			columnSchema.DefaultValue = DBNull.Value;
			columnSchema.IsNullable = "NO";
			column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(false, column.DoesNotRequireAValue);

			columnSchema.DefaultValue = "(\"N\")";
			columnSchema.IsNullable = "YES";
			column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(true, column.DoesNotRequireAValue);

			columnSchema.DefaultValue = "(\"N\")";
			columnSchema.IsNullable = "NO";
			column = new ColumnBuilder(null).Construct(columnSchema);
			AssertEquals(true, column.DoesNotRequireAValue);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			columnSchema = BuildColumnDefinition();
		}

		ColumnSchema BuildColumnDefinition()
		{
			columnSchema = new ColumnSchema();
			columnSchema.Name = "ZD1_Z0";
			columnSchema.DataType = DbDataType.UniqueIdentifier;
			columnSchema.DefaultValue = "(\"N\")";
			columnSchema.IsNullable = "YES";
			columnSchema.Length = 1;
			return columnSchema;
		}
		ColumnSchema columnSchema;

		#endregion
	}
}

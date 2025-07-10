using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SqlValidationTest : TestCaseWithFactory
	{
		public void TestCheckValidSql()
		{
			var dummy = Factory.New<DummyWithSqlValidation>();

			dummy.Z0_VarCharMax = "SELECT 1;";

			AssertNoErrors(dummy.Z0_VarCharMaxInfo);

			dummy.Z0_VarCharMax = "SELECT YOUR MUM";
			AssertHasError(dummy.Z0_VarCharMaxInfo, "Invalid SQL Statement\r\nInvalid column name 'YOUR'.");
		}

		public void TestCheckSingleTableColumnNames()
		{
			var dummy = Factory.New<DummyWithSqlValidation>();
			dummy.ExpectedColumnNames = new[] { "GS_Code", "GG_Code" };

			dummy.Z0_VarCharMax = "SELECT GS_Code from dbo.GlbStaff; SELECT GG_Code from dbo.GlbGroup;";
			AssertHasError(dummy.Z0_VarCharMaxInfo, "The SQL statement must only have a single table result.");

			dummy.ExpectedColumnNames = new[] { "GS_Code" };
			dummy.Z0_VarCharMax = "SELECT GS_Code from dbo.GlbStaff";
			AssertNoErrors(dummy.Z0_VarCharMaxInfo);
		}

		public void TestColumnNames()
		{
			var dummy = Factory.New<DummyWithSqlValidation>();
			dummy.ExpectedColumnNames = new[] { "MAICODE", "MAINAME" };

			dummy.Z0_VarCharMax = "SELECT * from dbo.GlbStaff";
			AssertHasError(dummy.Z0_VarCharMaxInfo, "The SQL statement must return 2 columns: MAICODE, MAINAME");

			dummy.Z0_VarCharMax = "SELECT GS_PK, GS_CODE from dbo.GlbStaff";
			AssertHasError(dummy.Z0_VarCharMaxInfo, "The column at position 1 must have the name MAICODE.");
			AssertHasError(dummy.Z0_VarCharMaxInfo, "The column at position 2 must have the name MAINAME.");

			dummy.Z0_VarCharMax = "SELECT GS_Code \"MaiCode\", GS_FullName from dbo.GlbStaff";
			AssertHasError(dummy.Z0_VarCharMaxInfo, "The column at position 2 must have the name MAINAME.");
			AssertEquals(1, dummy.Z0_VarCharMaxInfo.Notifications.Count());

			dummy.Z0_VarCharMax = "SELECT GS_Code \"MaiCode\", GS_FullName \"MaiName\" from dbo.GlbStaff";
			AssertNoErrors(dummy.Z0_VarCharMaxInfo);
		}

		public void TestColumnNames_WithOptionalColumns()
		{
			var dummy = Factory.New<DummyWithSqlValidation>();
			dummy.ExpectedColumnNames = new[] { "one", "two", "three" };
			dummy.OptionalColumnNames = new[] { "four", "five" };

			dummy.Z0_VarCharMax = "SELECT 1 one, 1 two FROM dbo.GlbStaff";
			AssertHasError(dummy.Z0_VarCharMaxInfo, @"The SQL statement must return 3 columns: one, two, three
The following columns are optional: four, five");

			dummy.Z0_VarCharMax = "SELECT 1 one, 1 two, 1 four FROM dbo.GlbStaff";
			AssertHasError(dummy.Z0_VarCharMaxInfo, @"The column at position 3 must have the name three.");

			dummy.Z0_VarCharMax = "SELECT 1 one, 1 two, 1 four, 1 three FROM dbo.GlbStaff";
			AssertHasError(dummy.Z0_VarCharMaxInfo, "The column at position 3 must have the name three.");

			dummy.Z0_VarCharMax = "SELECT 1 one, 1 two, 1 three, 1 four, 1 five, 1 six FROM dbo.GlbStaff";
			AssertHasError(dummy.Z0_VarCharMaxInfo, @"The SQL statement must return 3 columns: one, two, three
The following columns are optional: four, five");

			dummy.Z0_VarCharMax = "SELECT 1 one, 1 two, 1 three, 1 four, 1 six FROM dbo.GlbStaff";
			AssertHasError(dummy.Z0_VarCharMaxInfo, @"The SQL statement must return 3 columns: one, two, three
The following columns are optional: four, five");

			dummy.Z0_VarCharMax = "SELECT 1 one, 1 two, 1 four, 1 GABBO FROM dbo.GlbStaff";
			AssertHasError(dummy.Z0_VarCharMaxInfo, "The column at position 3 must have the name three.");

			dummy.Z0_VarCharMax = "SELECT 1 one, 1 two, 1 three, 1 four, 1 five FROM dbo.GlbStaff";
			AssertNoErrors(dummy.Z0_VarCharMaxInfo);

			dummy.Z0_VarCharMax = "SELECT 1 one, 1 two, 1 three, 1 four FROM dbo.GlbStaff";
			AssertNoErrors(dummy.Z0_VarCharMaxInfo);

			dummy.Z0_VarCharMax = "SELECT 1 one, 1 two, 1 three, 1 five FROM dbo.GlbStaff";
			AssertNoErrors(dummy.Z0_VarCharMaxInfo);
		}

		public void TestCheckValidSql_WithDifferentValueForColumnCheck_ShouldCheckValuesCorrectly()
		{
			var dummy = Factory.New<DummyWithSqlValidation>();
			SqlValidation.CheckValidStatement(dummy.Z0_VarCharMaxInfo, new ZString("SELECT GS_Code FROM dbo.GlbStaff"), new ZString("SELECT GG_Code FROM dbo.GlbGroup"), "GS_Code");
			AssertHasError(dummy.Z0_VarCharMaxInfo, "The column at position 1 must have the name GS_Code.");

			dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			SqlValidation.CheckValidStatement(dummy.Z0_VarCharMaxInfo, new ZString("SELECT GG_Code FROM dbo.GlbGroup"), new ZString("SELECT GS_Code FROM dbo.GlbStaff"), "GS_Code");
			AssertNoErrors(dummy.Z0_VarCharMaxInfo);

			dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			SqlValidation.CheckValidStatement(dummy.Z0_VarCharMaxInfo, new ZString("SELECT something FROM nothing"), new ZString("SELECT GS_Code FROM dbo.GlbStaff"), "GS_Code");
			AssertHasError(dummy.Z0_VarCharMaxInfo, "Invalid SQL Statement\r\nInvalid object name 'nothing'.");

			dummy.Z0_VarCharMaxInfo.ClearAllNotifications();
			SqlValidation.CheckValidStatement(dummy.Z0_VarCharMaxInfo, new ZString("SELECT GG_Code FROM dbo.GlbGroup"), new ZString("SELECT something FROM nothing"), "GS_Code");
			AssertHasError(dummy.Z0_VarCharMaxInfo, "Invalid SQL Statement\r\nInvalid object name 'nothing'.");
		}

		public void TestFillSchemaWithInlineComment_BreaksTheQueries()
		{
			var dummy = Factory.New<DummyWithSqlValidation>();

			dummy.ExpectedColumnNames = new[] { "GS_Code" };
			dummy.Z0_VarCharMax = "SELECT GS_Code from dbo.GlbStaff";
			AssertNoErrors(dummy.Z0_VarCharMaxInfo);

			Assert((int)Db.Connection.ExecuteScalar("SELECT 10") == 10);

			dummy.Z0_VarCharMax = "SELECT GS_Code from dbo.GlbStaff -- comment\r\n";
			AssertNoErrors(dummy.Z0_VarCharMaxInfo);

			Assert((int)Db.Connection.ExecuteScalar("SELECT 10") == 10);

			dummy.Z0_VarCharMax = "SELECT GS_Code from dbo.GlbStaff -- comment";
			AssertNoErrors(dummy.Z0_VarCharMaxInfo);

			Assert((int)Db.Connection.ExecuteScalar("SELECT 10") == 10);
		}

		class DummyWithSqlValidation : DummyBusinessObject, IObsoleteValidation
		{
			public DummyWithSqlValidation(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZString Z0_VarCharMax
			{
				get { return base.Z0_VarCharMax; }
				set
				{
					base.Z0_VarCharMax = value;

					if (OptionalColumnNames != null)
					{
						SqlValidation.CheckValidStatementWithOptionalColumns(Z0_VarCharMaxInfo, value, value, ExpectedColumnNames, OptionalColumnNames);
					}
					else
					{
						SqlValidation.CheckValidStatement(Z0_VarCharMaxInfo, value, value, ExpectedColumnNames);
					}
				}
			}

			internal string[] ExpectedColumnNames { get; set; }
			internal string[] OptionalColumnNames { get; set; }
		}
	}
}

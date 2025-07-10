using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.StabilityCheck
{
	public class DbCheckConstraintTest : TestCase
	{
		public void TestParsing()
		{
			DbCheckConstraint checkConstraint1 = new DbCheckConstraint("dbo", "TestTable", "Constraint_Test", "([ColumnName]='ValA' OR [ColumnName]='ValB' OR [ColumnName]='ValC' OR [ColumnName]='')");
			AssertEquals("dbo", checkConstraint1.SchemaName);
			AssertEquals("TestTable", checkConstraint1.TableName);
			AssertEquals("Constraint_Test", checkConstraint1.ConstraintName);
			Assert(checkConstraint1.IsInSetConstraint);
			AssertEquals("ColumnName", checkConstraint1.ColumnName);
			AssertContainsExactElementsInAnyOrder(new string[] { "ValA", "ValB", "ValC", "" }, checkConstraint1.ColumnValues);

			DbCheckConstraint checkConstraint2 = new DbCheckConstraint("dbo", "TestTable", "Constraint_Test", "([ColumnName]='Val')");
			AssertEquals("dbo", checkConstraint2.SchemaName);
			AssertEquals("TestTable", checkConstraint2.TableName);
			AssertEquals("Constraint_Test", checkConstraint2.ConstraintName);
			Assert(checkConstraint2.IsInSetConstraint);
			AssertEquals("ColumnName", checkConstraint2.ColumnName);
			AssertContainsExactElementsInAnyOrder(new string[] { "Val" }, checkConstraint2.ColumnValues);

			DbCheckConstraint checkConstraint3 = new DbCheckConstraint("dbo", "TestTable", "Constraint_Test", "([Column_Name_123]='Test Value 123')");
			AssertEquals("dbo", checkConstraint3.SchemaName);
			AssertEquals("TestTable", checkConstraint3.TableName);
			AssertEquals("Constraint_Test", checkConstraint3.ConstraintName);
			Assert(checkConstraint3.IsInSetConstraint);
			AssertEquals("Column_Name_123", checkConstraint3.ColumnName);
			AssertContainsExactElementsInAnyOrder(new string[] { "Test Value 123" }, checkConstraint3.ColumnValues);

			// no values
			DbCheckConstraint checkConstraint4 = new DbCheckConstraint("dbo", "TestTable", "Constraint_Test", "()");
			AssertEquals("dbo", checkConstraint4.SchemaName);
			AssertEquals("TestTable", checkConstraint4.TableName);
			AssertEquals("Constraint_Test", checkConstraint4.ConstraintName);
			Assert(!checkConstraint4.IsInSetConstraint);
			AssertNull("ColumnName", checkConstraint4.ColumnName);
			AssertNull(checkConstraint4.ColumnValues);

			// different columns
			DbCheckConstraint checkConstraint5 = new DbCheckConstraint("dbo", "TestTable", "Constraint_Test", "([ColumnNameA]='ValA' OR [ColumnNameB]='ValB')");
			AssertEquals("dbo", checkConstraint5.SchemaName);
			AssertEquals("TestTable", checkConstraint5.TableName);
			AssertEquals("Constraint_Test", checkConstraint5.ConstraintName);
			Assert(!checkConstraint5.IsInSetConstraint);
			AssertNull("ColumnName", checkConstraint5.ColumnName);
			AssertNull(checkConstraint5.ColumnValues);

			// wrong operator
			DbCheckConstraint checkConstraint6 = new DbCheckConstraint("dbo", "TestTable", "Constraint_Test", "([ColumnName]='ValA' AND [ColumnName]='ValB')");
			AssertEquals("dbo", checkConstraint6.SchemaName);
			AssertEquals("TestTable", checkConstraint6.TableName);
			AssertEquals("Constraint_Test", checkConstraint6.ConstraintName);
			Assert(!checkConstraint6.IsInSetConstraint);
			AssertNull("ColumnName", checkConstraint6.ColumnName);
			AssertNull(checkConstraint6.ColumnValues);

			// wrong operator
			DbCheckConstraint checkConstraint7 = new DbCheckConstraint("dbo", "TestTable", "Constraint_Test", "([ColumnName]>'Val')");
			AssertEquals("dbo", checkConstraint7.SchemaName);
			AssertEquals("TestTable", checkConstraint7.TableName);
			AssertEquals("Constraint_Test", checkConstraint7.ConstraintName);
			Assert(!checkConstraint7.IsInSetConstraint);
			AssertNull("ColumnName", checkConstraint7.ColumnName);
			AssertNull(checkConstraint7.ColumnValues);

			// not string literal
			DbCheckConstraint checkConstraint8 = new DbCheckConstraint("dbo", "TestTable", "Constraint_Test", "[ColumnNameA]=[ColumnNameA]");
			AssertEquals("dbo", checkConstraint8.SchemaName);
			AssertEquals("TestTable", checkConstraint8.TableName);
			AssertEquals("Constraint_Test", checkConstraint8.ConstraintName);
			Assert(!checkConstraint8.IsInSetConstraint);
			AssertNull("ColumnName", checkConstraint8.ColumnName);
			AssertNull(checkConstraint8.ColumnValues);

			// not string literal
			DbCheckConstraint checkConstraint9 = new DbCheckConstraint("dbo", "TestTable", "Constraint_Test", "[ColumnNameA]=(5)");
			AssertEquals("dbo", checkConstraint9.SchemaName);
			AssertEquals("TestTable", checkConstraint9.TableName);
			AssertEquals("Constraint_Test", checkConstraint9.ConstraintName);
			Assert(!checkConstraint9.IsInSetConstraint);
			AssertNull("ColumnName", checkConstraint9.ColumnName);
			AssertNull(checkConstraint9.ColumnValues);

			// not simple string literal
			DbCheckConstraint checkConstraint10 = new DbCheckConstraint("dbo", "TestTable", "Constraint_Test", "[ColumnNameA]='Te''st'");
			AssertEquals("dbo", checkConstraint10.SchemaName);
			AssertEquals("TestTable", checkConstraint10.TableName);
			AssertEquals("Constraint_Test", checkConstraint10.ConstraintName);
			Assert(!checkConstraint10.IsInSetConstraint);
			AssertNull("ColumnName", checkConstraint10.ColumnName);
			AssertNull(checkConstraint10.ColumnValues);
		}
	}
}
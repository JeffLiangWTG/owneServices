using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing.StabilityCheck
{
	public class DbCheckConstraintReaderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			Db.Connection.ExecuteNonQuery("CREATE TABLE dbo.Test_DbCheckConstraintReader_Table ( Id INT NOT NULL, Val1 NVARCHAR(16), Val2 NVARCHAR(16))");
			Db.Connection.ExecuteNonQuery("ALTER TABLE dbo.Test_DbCheckConstraintReader_Table ADD CONSTRAINT CK_Test_DbCheckConstraintReader_Table_InSet CHECK (Val1 in ('ValA', 'ValB', ''))");
			Db.Connection.ExecuteNonQuery("ALTER TABLE dbo.Test_DbCheckConstraintReader_Table ADD CONSTRAINT CK_Test_DbCheckConstraintReader_Table_Between CHECK (Val2 between 'A' and 'Z')");
		}

		public void TestReadAllCheckConstraints()
		{
			DbCheckConstraintReader reader = new DbCheckConstraintReader();
			List<DbCheckConstraint> allConstraints = reader.ReadAllCheckConstraints();

			DbCheckConstraint inSetConstraint = allConstraints.SingleOrDefault(c =>
				c.SchemaName == "dbo" &&
				c.TableName == "Test_DbCheckConstraintReader_Table" &&
				c.ConstraintName == "CK_Test_DbCheckConstraintReader_Table_InSet"
			);

			AssertNotNull(inSetConstraint);
			Assert(inSetConstraint.IsInSetConstraint);
			AssertEquals("Val1", inSetConstraint.ColumnName);
			AssertContainsExactElementsInAnyOrder(new string[] { "", "ValA", "ValB" }, inSetConstraint.ColumnValues);

			DbCheckConstraint betweenConstraint = allConstraints.SingleOrDefault(c =>
				c.SchemaName == "dbo" &&
				c.TableName == "Test_DbCheckConstraintReader_Table" &&
				c.ConstraintName == "CK_Test_DbCheckConstraintReader_Table_Between"
			);

			AssertNotNull(betweenConstraint);
			Assert(!betweenConstraint.IsInSetConstraint);
			AssertNull(betweenConstraint.ColumnName);
			AssertNull(betweenConstraint.ColumnValues);
		}
	}
}
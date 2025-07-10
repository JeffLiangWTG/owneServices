using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class DbConstraintReaderTest : TestCaseWithDummy
	{
		public void TestReadingConstraints()
		{
			DbConstraintReader reader = DbConstraintReader.GetInstance(DummyBusinessObject.Schema.TableName);
			DbConstraintCollection constraints = reader.Constraints;
			Assert("Should be at least 10 constraints read", constraints.Count > 10);

			DbConstraint constraint = reader.Constraints["DF_Z0_Code"] ?? reader.Constraints["DF_DummyBizo_Z0_Code"];
			Assert("DF_Z0_Code" == constraint.ConstraintName || "DF_DummyBizo_Z0_Code" == constraint.ConstraintName);
			AssertEquals(DbConstraintType.Default, constraint.Type);
			AssertEquals("Z0_Code", constraint.ColumnNames[0]);
			AssertEquals("('')", constraint.Data);
		}

		public void TestGetConstraintsOfType()
		{
			DbConstraintReader reader = DbConstraintReader.GetInstance(DummyBusinessObject.Schema.TableName);
			DbConstraint[] defaultConstraints = reader.Constraints.GetConstraintsOfType(DbConstraintType.Default);
			Assert("Should be at least 1 default constraint read", reader.Constraints.Count >= 1);
			foreach (DbConstraint defaultConstraint in defaultConstraints)
			{
				AssertEquals("Constraints should be of type default", DbConstraintType.Default, defaultConstraint.Type);
			}
		}

		public void TestGetUniqueConstraints()
		{
			DbConstraintReader reader = DbConstraintReader.GetInstance(DummyBusinessObject.Schema.TableName);
			DbConstraint[] uniqueConstraints = reader.Constraints.GetConstraintsOfType(DbConstraintType.Unique);

			AssertEquals("Should be 1 unique constraint", 1, uniqueConstraints.Length);
			AssertEquals("Constraint type should be Unique", DbConstraintType.Unique, uniqueConstraints[0].Type);
			AssertEquals("Should be 1 column participating in the unique constraint", 1, uniqueConstraints[0].ColumnNames.Length);
			AssertEquals("ColumnName correct", "Z0_PK", uniqueConstraints[0].ColumnNames[0]);
		}

		public void TestGetForeignKeyConstraints()
		{
			DbConstraintReader reader = DbConstraintReader.GetInstance(DummyDependentBizoSchema.Constants.TableName);
			DbConstraintCollection fKConstraints = new DbConstraintCollection(reader.Constraints.GetConstraintsOfType(DbConstraintType.ForeignKey));
			Assert("OrgMiscServ ForeignKey collection should have at least 1 item", fKConstraints.Count >= 1);

			DbConstraint fKConstraint = fKConstraints["DummyDependentBizo_ZD1_Z0_FK2_DummyBizo_RRR_120N"];
			AssertNotNull("DummyDependentBizo_ZD1_Z0_FK2_DummyBizo_RRR_120N should be in the collection", fKConstraint);
			AssertEquals("Constraint type should be ForeignKey", DbConstraintType.ForeignKey, fKConstraint.Type);
			AssertEquals("Should be 1 column participating in the FK constraint", 1, fKConstraint.ColumnNames.Length);
			AssertEquals("ColumnName correct", "ZD1_Z0", fKConstraint.ColumnNames[0]);
		}
	}
}

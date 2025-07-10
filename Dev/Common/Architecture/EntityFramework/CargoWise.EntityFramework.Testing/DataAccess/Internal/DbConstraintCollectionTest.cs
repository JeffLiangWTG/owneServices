namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class DbConstraintCollectionTest : TestCaseWithDummy
	{
		public void TestGetConstraintsOfType()
		{
			DbConstraintReader reader = DbConstraintReader.GetInstance(DummyBusinessObject.Schema.TableName);
			DbConstraint[] defaultConstraints = reader.Constraints.GetConstraintsOfType(DbConstraintType.Default);
			Assert("Should be at least 1 default constraint read", defaultConstraints.Length >= 1);
			foreach (DbConstraint defaultConstraint in defaultConstraints)
			{
				AssertEquals("Constraints should be of type default", DbConstraintType.Default, defaultConstraint.Type);
			}
		}

		public void TestGetContraintsForColumn()
		{
			DbConstraintReader reader = DbConstraintReader.GetInstance(DummyBusinessObject.Schema.TableName);
			DbConstraint[] constraintsOnColumn = reader.Constraints.GetConstraintsForColumn(DummyBusinessObject.Schema.Z0_Code);
			AssertEquals("Should be only 1 constraint read", 1, constraintsOnColumn.Length);
			AssertEquals("Constraint read should be Z0_Code", DummyBusinessObject.Schema.Z0_Code, constraintsOnColumn[0].ColumnNames[0]);
		}
	}
}

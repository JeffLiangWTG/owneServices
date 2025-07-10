using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	sealed class StmSystemDefinedFieldWithOrderAndGridValidationTest : StmSystemDefinedFieldBaseValidationTestCase
	{
		public override void TestCheckOrder()
		{
			TestCheckOrder(Parent.S1_OrderInfo);
		}

		public void TestValidateIsValidGridOrColumn()
		{
			Parent.S1_Type = "TXT";
			Parent.Validation.ValidateAll();
			AssertEquals("Parent.HasRowErrors", false, Parent.HasRowErrors);

			Parent.S1_Type = "GRD";
			Parent.Validation.ValidateAll();
			AssertHasRowError(Parent, "There are no Columns defined for this Grid.");

			Parent.FieldColumns.AddNew();
			Parent.Validation.ValidateAll();
			AssertEquals("Parent.HasRowErrors", false, Parent.HasRowErrors);
		}

		#region Implementation

		new StmSystemDefinedField Parent
		{
			get { return (StmSystemDefinedField)base.Parent; }
		}

		protected override AutoStmSystemDefinedField GetNewParent()
		{
			return Factory.New<StmSystemDefinedField>();
		}

		protected override BusinessObjectCollection GetNewCollection()
		{
			return new StmSystemDefinedFieldCollection(Factory);
		}

		void AssertHasRowError(StmSystemDefinedField field, string errorMessage)
		{
			AssertEquals("RowErrors.Length", 1, field.RowErrors.Count());
			AssertEquals("RowErrors[0]", errorMessage, field.RowErrors.GetFirstMessage());
		}

		#endregion
	}
}

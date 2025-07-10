using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	sealed class StmSystemDefinedFieldColumnValidationTest : StmSystemDefinedFieldBaseValidationTestCase
	{
		public override void TestCheckOrder()
		{
			TestCheckOrder(Parent.S1_OrderColumnInfo);
		}

		#region Implementation

		protected override AutoStmSystemDefinedField GetNewParent()
		{
			return Factory.New<StmSystemDefinedFieldColumn>();
		}

		protected override BusinessObjectCollection GetNewCollection()
		{
			StmSystemDefinedField parentField = Factory.New<StmSystemDefinedField>();
			return new StmSystemDefinedFieldColumnCollection(parentField, Factory);
		}

		#endregion
	}
}

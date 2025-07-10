using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(FixDocBuilderAdvancePaymentMenuItem))]
	internal class FixDocBuilderAdvancePaymentMenuItemTest : DataTransformationTestCase
	{
		public FixDocBuilderAdvancePaymentMenuItemTest()
		{
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new FixDocBuilderAdvancePaymentMenuItem();

		readonly string selectSQL = @"SELECT SU_PreventAutoDelivery
						FROM dbo.StmMenuItem
						WHERE SU_MenuName = 'DocBuilder Advance Payment Request'
						AND SU_IsSystemDefined = 1
						AND SU_BusinessContext = 'Shipment'";

		protected override void AssertTransformationResults()
		{
			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, selectSQL);
			AssertEquals(1, dataTable.Rows.Count);
			AssertEquals(false, (bool)dataTable.Rows[0]["SU_PreventAutoDelivery"]);
		}

		protected override void PrepareTestData()
		{
			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, selectSQL);
			AssertEquals(1, dataTable.Rows.Count);
		}
	}
}

using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations;
using Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(DeleteTransitWarehouseCustomerRoleTransformation))]
	public class DeleteTransitWarehouseCustomerRoleTransformationTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			Assert(!Db.Connection.Exists(@"FROM dbo.GlbGroupRole WHERE GGR_RoleName = 'TransitWarehouseCustomer'"));
			Assert(Db.Connection.Exists(@"FROM dbo.GlbGroupRole WHERE GGR_RoleName = 'transitwarehouseclientrole'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new DeleteTransitWarehouseCustomerRoleTransformation();

		protected override void PrepareTestData()
		{
			var sql = @"DELETE dbo.GlbGroupRole WHERE GGR_RoleName = 'TransitWarehouseCustomer';";
			TestConnection.ExecuteNonQuery(sql);

			var testDataCreator = new TransformationTestDataCreator();
			var group = testDataCreator.GetOrCreateGroupByCode("WHSCLIENTPORTAL", "ORG", "", isSystemDefined: true);
			var groupRole1 = testDataCreator.CreateGlbGroupRole(group, "TransitWarehouseCustomer");
			var groupRole2 = testDataCreator.CreateGlbGroupRole(group, "transitwarehouseclientrole");

			Assert(Db.Connection.Exists(@"FROM dbo.GlbGroup WHERE GG_Code = 'WHSCLIENTPORTAL'"));
			Assert(Db.Connection.Exists(@"FROM dbo.GlbGroupRole WHERE GGR_RoleName = 'TransitWarehouseCustomer'"));
			Assert(Db.Connection.Exists(@"FROM dbo.GlbGroupRole WHERE GGR_RoleName = 'transitwarehouseclientrole'"));
		}
	}
}

using System;
using System.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Testing
{
	[TestedType(typeof(DeleteRoleOrderManagerCustomerFromGroupTrackPortal))]
	class DeleteRoleOrderManagerCustomerFromGroupTrackPortalTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			Assert(!DoesRecordExist(group1_Role1PK));
			Assert(DoesRecordExist(group1_Role2PK));
			Assert(DoesRecordExist(group2_Role1PK));
			Assert(DoesRecordExist(group2_Role2PK));
			Assert(DoesRecordExist(group3_Role1PK));
			Assert(DoesRecordExist(group3_Role2PK));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new DeleteRoleOrderManagerCustomerFromGroupTrackPortal();
		}

		bool DoesRecordExist(Guid pk)
		{
			return TestConnection.Exists("FROM dbo.GlbGroupRole WHERE GGR_PK = @pk", cmd => cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk));
		}

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var group1PK = TestConnection.Exists("FROM dbo.GlbGroup WHERE GG_Code = 'TRACKPORTAL'")
				? TestConnection.ExecuteScalar<Guid>("SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Code = 'TRACKPORTAL'")
				: testDataCreator.CreateGlbGroup("TRACKPORTAL", "ORG", "");
			var group2PK = testDataCreator.CreateGlbGroup("SECONDPORTAL", "ORG", "");
			var group3PK = testDataCreator.CreateGlbGroup("THIRDPORTAL", "ORG", "");

			group1_Role1PK = TestConnection.Exists($"FROM dbo.GlbGroupRole WHERE GGR_GG_Group = '{group1PK}' AND GGR_RoleName = 'OrderManagerCustomer'")
				? TestConnection.ExecuteScalar<Guid>($"SELECT GGR_PK FROM dbo.GlbGroupRole WHERE GGR_GG_Group = '{group1PK}' AND GGR_RoleName = 'OrderManagerCustomer'")
				: testDataCreator.CreateGlbGroupRole(group1PK, "OrderManagerCustomer");
			group1_Role2PK = TestConnection.Exists($"FROM dbo.GlbGroupRole WHERE GGR_GG_Group = '{group1PK}' AND GGR_RoleName = 'TrackingPortalUser'")
				? TestConnection.ExecuteScalar<Guid>($"SELECT GGR_PK FROM dbo.GlbGroupRole WHERE GGR_GG_Group = '{group1PK}' AND GGR_RoleName = 'TrackingPortalUser'")
				: testDataCreator.CreateGlbGroupRole(group1PK, "TrackingPortalUser");

			group2_Role1PK = testDataCreator.CreateGlbGroupRole(group2PK, "OrderManagerCustomer");
			group2_Role2PK = testDataCreator.CreateGlbGroupRole(group2PK, "TrackingPortalUser");

			group3_Role1PK = testDataCreator.CreateGlbGroupRole(group3PK, "OrderManagerCustomer");
			group3_Role2PK = testDataCreator.CreateGlbGroupRole(group3PK, "TransitWarehouseCustomer");
		}

		Guid group1_Role1PK;
		Guid group1_Role2PK;
		Guid group2_Role1PK;
		Guid group2_Role2PK;
		Guid group3_Role1PK;
		Guid group3_Role2PK;
	}
}

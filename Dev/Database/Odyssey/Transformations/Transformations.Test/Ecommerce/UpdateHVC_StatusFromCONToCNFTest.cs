using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Ecommerce
{
	[TestedType(typeof(UpdateHVC_StatusFromCONToCNF))]
	class UpdateHVC_StatusFromCONToCNFTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals("Number of consignments with CON status after transform", 0, GetConsignmentCountForStatus("CON"));
			AssertEquals("Number of consignments with CNF status after transform", 1, GetConsignmentCountForStatus("CNF"));
			AssertEquals("Number of consignments with ARV status after transform", 1, GetConsignmentCountForStatus("ARV"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateHVC_StatusFromCONToCNF();

		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery(@"ALTER TABLE dbo.HVLVConsignment DROP CONSTRAINT Constraint_HVC_Status");

			testDataCreator = testDataCreator ?? new TransformationTestDataCreator();
			var orgAddressPK = (Guid)Db.Connection.ExecuteScalar(@"SELECT TOP 1 OA_PK FROM dbo.OrgAddress");
			var bookingHeaderPK = testDataCreator.CreateHVLVBookingHeader(orgAddressPK, "HVH_001", "XXX", 1);
			_ = testDataCreator.CreateHVLVConsignment(bookingHeaderPK, "HVC_001", "HVC_001", "User", 1, 1, "", default, status: "CON");
			_ = testDataCreator.CreateHVLVConsignment(bookingHeaderPK, "HVC_002", "HVC_002", "User", 1, 1, "", default, status: "ARV");
		}

		int GetConsignmentCountForStatus(string status)
		{
			var sql = @"SELECT COUNT(0) FROM dbo.HVLVConsignment WHERE HVC_Status = @status";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@status", SqlDbType.VarChar, status);
				return (int)cmd.ExecuteScalar();
			}
		}

		TransformationTestDataCreator testDataCreator;

		#region TestIndex

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Update HVC_Status from CON to CNF._1] ON [dbo].[HVLVConsignment] ([HVC_Status]) WHERE ([HVC_Status]='CON') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		#endregion
	}
}

using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles.Organisation.Other;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.MasterFiles.Organisation.Other
{
	[TestedType(typeof(UpdateOrgAirlineMAWBStockManagementOHM_GC_Company))]
	public class UpdateOrgAirlineMAWBStockManagementOHM_GC_CompanyTest : DataTransformationTestCase
	{
		Guid companyPk1, companyPk2, ohmPk1, ohmPk2;

		protected override void AssertTransformationResults()
		{
			AssertCompanyId(ohmPk1, companyPk1);
			AssertCompanyId(ohmPk2, companyPk2);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
			=> new UpdateOrgAirlineMAWBStockManagementOHM_GC_Company();

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var carrierId = testDataCreator.CreateOrg("AIR", "Air Carrier Org");

			companyPk1 = testDataCreator.CreateGlbCompany("USC", "US");
			var branchPK = testDataCreator.CreateGlbBranch("USB", companyPk1);
			ohmPk1 = CreateOrgAirlineMAWBStockManagement(branchPK, carrierId);

			companyPk2 = testDataCreator.CreateGlbCompany("AUC", "AU");
			branchPK = testDataCreator.CreateGlbBranch("AUB", companyPk2);
			ohmPk2 = CreateOrgAirlineMAWBStockManagement(branchPK, carrierId);
		}

		void AssertCompanyId(Guid ohmPk, Guid expectedCompanyId)
		{
			var companyId = Db.Connection.ExecuteScalar($"SELECT [OHM_GC_Company] FROM [dbo].[OrgAirlineMAWBStockManagement] WHERE [OHM_PK] = '{ohmPk}'");
			AssertEquals(expectedCompanyId, companyId);
		}

		const string sqlInsert = @"
IF EXISTS (SELECT 1 
           FROM sys.check_constraints 
           WHERE name = 'Constraint_OHM_GB_Branch_OHM_GC_Company' 
           AND parent_object_id = OBJECT_ID('dbo.OrgAirlineMAWBStockManagement'))
BEGIN
    ALTER TABLE dbo.OrgAirlineMAWBStockManagement 
    NOCHECK CONSTRAINT Constraint_OHM_GB_Branch_OHM_GC_Company;
END

INSERT INTO [dbo].[OrgAirlineMAWBStockManagement] ([OHM_PK],[OHM_GB_Branch],[OHM_OH_Carrier],[OHM_SystemCreateTimeUtc],[OHM_SystemCreateUser],[OHM_SystemLastEditTimeUtc],[OHM_SystemLastEditUser])
VALUES (@pk, @branchId, @carrierId, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		Guid CreateOrgAirlineMAWBStockManagement(Guid branchId, Guid carrierId)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(sqlInsert))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@branchId", SqlDbType.UniqueIdentifier, branchId);
				command.AddParameter("@carrierId", SqlDbType.UniqueIdentifier, carrierId);
				command.ExecuteNonQuery();
			}

			return pk;
		}
	}
}

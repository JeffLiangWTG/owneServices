using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.Build.Database.Script.Public.Test;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Testing
{
	[TestedType(typeof(GlowDeleteJobService))]
	internal sealed class GlowDeleteJobServiceTest : DbCreateScriptTest
	{
		public void TestDeleteRelatedRecords()
		{
			InsertTestData();
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageJob WHERE KJ_PK = 'f751b13c-6d2d-41e1-b2aa-b2c594c5547c'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_PK = 'a8e4eed3-6b4b-485b-a05a-32a100305acf'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageHeader WHERE KPH_PK = 'd3937e36-f9b4-4211-858e-975178f00844'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.JobService WHERE ES_PK = '25006eca-7451-4050-a812-5956b6a675bd'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.JobServiceLink WHERE ESL_PK = '125cad3f-26ac-4739-802e-7280bd8cb2a2'"));

			RunSP("25006eca-7451-4050-a812-5956b6a675bd");

			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageJob WHERE KJ_PK = 'f751b13c-6d2d-41e1-b2aa-b2c594c5547c'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackage WHERE KP_PK = 'a8e4eed3-6b4b-485b-a05a-32a100305acf'"));
			AssertEquals(1, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.PkgPackageHeader WHERE KPH_PK = 'd3937e36-f9b4-4211-858e-975178f00844'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.JobService WHERE ES_PK = '25006eca-7451-4050-a812-5956b6a675bd'"));
			AssertEquals(0, TestConnection.ExecuteScalar("SELECT COUNT(*) FROM dbo.JobServiceLink WHERE ESL_PK = '125cad3f-26ac-4739-802e-7280bd8cb2a2'"));
		}

		void InsertTestData()
		{
			GlbGeneratorForTests.NewStaff("S1");
			var packageJobPK = "f751b13c-6d2d-41e1-b2aa-b2c594c5547c";
			var pkgPackageHeaderPK = "d3937e36-f9b4-4211-858e-975178f00844";
			var pkgPackagePK = "a8e4eed3-6b4b-485b-a05a-32a100305acf";
			var jobServicePK = "25006eca-7451-4050-a812-5956b6a675bd";
			var jobServiceLinkPK = "125cad3f-26ac-4739-802e-7280bd8cb2a2";
			var companyPK = Guid.NewGuid();
			var branchPK = Guid.NewGuid();
			var warehouseOrgPK = Guid.NewGuid();
			var addressPK = Guid.NewGuid();
			var wltPK = Guid.NewGuid();
			var warehousePK = Guid.NewGuid();
			var rcnPK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.PkgPackageJob (KJ_PK, KJ_ParentID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES ('{packageJobPK}', NewID(), 'Z0', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.PkgPackageHeader (KPH_PK, KPH_PackageID, KPH_SystemCreateTimeUtc, KPH_SystemCreateUser, KPH_SystemLastEditTimeUtc, KPH_SystemLastEditUser) VALUES('{pkgPackageHeaderPK}', 'Parent', GetUtcDate(), 'S1', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.PkgPackage (KP_PK, KP_KPH_PackageHeader, KP_KJ_ParentPackageJob, KP_Sequence, KP_PackageQty, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES('{pkgPackagePK}', '{pkgPackageHeaderPK}', '{packageJobPK}', '1', '1', 'PKG', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('{companyPK}', 'WTG', 'AU company', 'AU', 'AUD')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code) VALUES ('{branchPK}', '{companyPK}', 'BR1')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) values ('{warehouseOrgPK}', 'WHS001')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1) values ('{addressPK}', '{warehouseOrgPK}', 'Test Address')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsLocationType(WLT_PK, WLT_Code, WLT_Description, [WLT_SystemCreateTimeUtc], [WLT_SystemCreateUser], [WLT_SystemLastEditTimeUtc], [WLT_SystemLastEditUser]) VALUES ('{wltPK}', 'WLT', 'WHS Loc Type', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsWarehouse([WW_PK], [WW_GB_RelatedCompanyBranch], [WW_OA_WarehouseAddress], [WW_WLT_DefaultLocationType], [WW_WarehouseCode], [WW_DefaultOutboundDockDoor], [WW_DefaultInboundDockDoor], [WW_SystemCreateTimeUtc], [WW_SystemCreateUser], [WW_SystemLastEditTimeUtc], [WW_SystemLastEditUser]) VALUES ('{warehousePK}','{branchPK}','{addressPK}','{wltPK}','W01', NEWID(), NEWID(), GETUTCDATE(), 'E', GETUTCDATE(), 'E')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsItemReceiveConsignment([WRC_PK], [WRC_ConsignmentID], [WRC_SystemCreateTimeUtc], [WRC_SystemCreateUser], [WRC_SystemLastEditTimeUtc], [WRC_SystemLastEditUser], [WRC_WW_IntendedWarehouse], [WRC_JobID]) VALUES ('{rcnPK}', 'RCN1', GETUTCDATE(), 'E', GETUTCDATE(), 'E', '{warehousePK}', 'RC00000001')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.JobService([ES_PK], [ES_ParentID], [ES_ParentTableCode], [ES_SystemCreateTimeUtc], [ES_SystemCreateUser], [ES_SystemLastEditTimeUtc], [ES_SystemLastEditUser]) VALUES ('{jobServicePK}', '{rcnPK}', 'WRC', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.JobServiceLink([ESL_PK], [ESL_ParentTableCode], [ESL_ParentID], [ESL_ES_JobService], [ESL_SystemCreateTimeUtc], [ESL_SystemCreateUser], [ESL_SystemLastEditTimeUtc], [ESL_SystemLastEditUser]) VALUES ('{jobServiceLinkPK}', 'KP', '{pkgPackagePK}', '{jobServicePK}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");
		}

		static void RunSP(string pk)
		{
			short version = (short)Db.Connection.ExecuteScalar($"SELECT ES_AutoVersion FROM dbo.JobService WHERE ES_PK = '{pk}'");
			using (var command = Db.Connection.Command("GlowDeleteJobService"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@JobServicePK", SqlDbType.UniqueIdentifier, new Guid(pk));
				command.AddParameter("@Version", SqlDbType.SmallInt, version);
				command.ExecuteScalar();
			}
		}
	}
}


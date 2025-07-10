using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_ShipmentContainersByClientAndCarrier))]
	class Report_ShipmentContainersByClientAndCarrierTest : DbCreateScriptTest
	{
		public void TestReportDeclarationContainersByClientAndCarrier_MatchCompany()
		{
			var companyPk1 = new Guid();
			var orgPk = new Guid();

			var prepareSql = @"
DECLARE @CompanyPk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @BranchPk1 UNIQUEIDENTIFIER = NEWID(), @BranchPk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @DeclarationPk1 UNIQUEIDENTIFIER = NEWID(), @DeclarationPk2 UNIQUEIDENTIFIER = NEWID(), @DeclarationPk3 UNIQUEIDENTIFIER = NEWID();
DECLARE @jobContainerPK1 UNIQUEIDENTIFIER = NEWID(), @jobContainerPK2 UNIQUEIDENTIFIER = NEWID(), @jobContainerPK3 UNIQUEIDENTIFIER = NEWID();
DECLARE @cusContainerPK1 UNIQUEIDENTIFIER = NEWID(), @cusContainerPK2 UNIQUEIDENTIFIER = NEWID(), @cusContainerPK3 UNIQUEIDENTIFIER = NEWID(), @cusContainerPK4 UNIQUEIDENTIFIER = NEWID();
DECLARE @cusDecHouseBillPk1 UNIQUEIDENTIFIER = NEWID(), @cusDecHouseBillPk2 UNIQUEIDENTIFIER = NEWID(), @cusDecHouseBillPk3 UNIQUEIDENTIFIER = NEWID();
DECLARE @cusDecHouseContainerPivotPk1 UNIQUEIDENTIFIER = NEWID(), @cusDecHouseContainerPivotPk2 UNIQUEIDENTIFIER = NEWID(), @cusDecHouseContainerPivotPk3 UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_IsReciprocal)
VALUES
	(@CompanyPk1, 'FR', 'AAA', 'EUR company', 'EUR', 0),
	(@CompanyPk2, 'CA', 'BBB', 'CAD company', 'CAD', 0);

INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_RL_NKHomePort)
VALUES
	(@BranchPk1, @CompanyPk1, 'AAA', 'MX5CL'),
	(@BranchPk2, @CompanyPk2, 'BBB', 'CAHJU');

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code)
VALUES (@OrgPk, 'TESTORG')

INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_IsCancelled, JE_GB, JE_GC, JE_ClusterKey, JE_ApplicationCode, JE_MessageType, JE_OH_Supplier, JE_ContainerMode, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES
	(@DeclarationPk1, 'FR', 'B0001', 0, @BranchPK1, @CompanyPk1, 1, 'BLT', 'IMP', @OrgPk, 'FCL', GETDATE(), '~BP', GETDATE(), '~BP'),
	(@DeclarationPk2, 'CA', 'B0002', 0, @BranchPK2, @CompanyPk2, 2, 'BLT', 'IMP', @OrgPk, 'FCL', GETDATE(), '~BP', GETDATE(), '~BP'),
	(@DeclarationPk3, 'FR', 'B0003', 0, @BranchPK1, @CompanyPk1, 3, 'BLT', 'IMP', @OrgPk, 'FCL', GETDATE(), '~BP', GETDATE(), '~BP');

INSERT INTO dbo.JobContainer (JC_PK, JC_ContainerNum) 
VALUES
	(@jobContainerPK1, 'JC1'),
	(@jobContainerPK2, 'JC2'),
	(@jobContainerPK3, 'JC3');

INSERT INTO dbo.CusContainer (CO_PK, CO_ContainerNumber, CO_JE, CO_JC, CO_RC, CO_ClusterKey, CO_DataModel) 
VALUES
	(@cusContainerPK1, 'A001', @DeclarationPk1, @jobContainerPK1, null, 1, 'FR'),
	(@cusContainerPK2, 'A002', @DeclarationPk2, @jobContainerPK2, null, 2, 'CA'),
	(@cusContainerPK3, 'A003', @DeclarationPk1, @jobContainerPK3, null, 1, 'FR'),
	(@cusContainerPK4, 'A004', @DeclarationPk3, null, null, 3, 'FR');

INSERT dbo.CusDecHouseBill (CU_PK, CU_JE, CU_ClusterKey) VALUES
	(@cusDecHouseBillPk1, @DeclarationPk1, 1),
	(@cusDecHouseBillPk2, @DeclarationPk2, 2),
	(@cusDecHouseBillPk3, @DeclarationPk3, 3);

INSERT dbo.CusDecHouseContainerPivot (CR_PK, CR_CU_HouseBill, CR_CO_Container, CR_ClusterKey) VALUES
	(@cusDecHouseContainerPivotPk1, @cusDecHouseBillPk1, @cusContainerPK1, 1),
	(@cusDecHouseContainerPivotPk2, @cusDecHouseBillPk2, @cusContainerPK2, 2),
	(@cusDecHouseContainerPivotPk3, @cusDecHouseBillPk3, @cusContainerPK4, 3);

INSERT dbo.CusDecHouseContainerPack (CW_PK, CW_CR_HouseContainer, CW_ClusterKey, CW_PackQty) VALUES
	(NEWID(), @cusDecHouseContainerPivotPk1, 1, 10),
	(NEWID(), @cusDecHouseContainerPivotPk2, 2, 20),
	(NEWID(), @cusDecHouseContainerPivotPk3, 3, 30);
";

			using (var command = Db.Connection.Command(prepareSql))
			{
				command.AddParameter("@CompanyPK1", SqlDbType.UniqueIdentifier, companyPk1);
				command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, orgPk);
				command.ExecuteNonQuery();
			}

			var reportSql = $@"
DECLARE @ClientPKs TVP_uniqueidentifier
INSERT @ClientPKs
VALUES ('{orgPk}')

SELECT JC_BusinessType, JC_ContainerNum, JobReference, PackageCountTotal
FROM Report_ShipmentContainersByClientAndCarrier(@ClientPKs, 'Either Consignor or Consignee', 'ALL', 'Declaration Only', null, null, '', '{companyPk1}')";

			var resultList = new List<(string, string, string, int)>();
			Db.Connection.ExecuteReader(reportSql,
				reader =>
				{
					resultList.Add((reader["JC_BusinessType"].ToString(), reader["JC_ContainerNum"].ToString(), reader["JobReference"].ToString(), (int)reader["PackageCountTotal"]));
				});
			AssertContainsExactElementsInAnyOrder("Only FR and non-null JobContainer selected", new[] { ("DEC", "JC1", "B0001", 10), ("DEC", "JC3", "B0001", 0) }, resultList);
		}

		public void TestReportContainersByClientAndCarrier_MatchConsignor()
		{
			var companyPk1 = Guid.NewGuid();
			var orgPk = Guid.NewGuid();

			var prepareSql = @"
DECLARE @CompanyPk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @BranchPk1 UNIQUEIDENTIFIER = NEWID(), @BranchPk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @shipmentPK UNIQUEIDENTIFIER = NEWID(), @shipmentPKMaster UNIQUEIDENTIFIER = NEWID();
DECLARE @ConsolPK UNIQUEIDENTIFIER = NEWID();
DECLARE @ConsolLinkPK UNIQUEIDENTIFIER = NEWID();
DECLARE @JobPackLinePK UNIQUEIDENTIFIER = NEWID();
DECLARE @AddressPk UNIQUEIDENTIFIER = NEWID();
DECLARE @jobContainerPK1 UNIQUEIDENTIFIER = NEWID(), @jobContainerPK2 UNIQUEIDENTIFIER = NEWID(), @jobContainerPK3 UNIQUEIDENTIFIER = NEWID();
DECLARE @cusContainerPK1 UNIQUEIDENTIFIER = NEWID(), @cusContainerPK2 UNIQUEIDENTIFIER = NEWID(), @cusContainerPK3 UNIQUEIDENTIFIER = NEWID(), @cusContainerPK4 UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_IsReciprocal)
VALUES
	(@CompanyPk1, 'FR', 'AAA', 'EUR company', 'EUR', 0),
	(@CompanyPk2, 'CA', 'BBB', 'CAD company', 'CAD', 0);

INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_RL_NKHomePort)
VALUES
	(@BranchPk1, @CompanyPk1, 'AAA', 'MX5CL'),
	(@BranchPk2, @CompanyPk2, 'BBB', 'CAHJU');

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code)
VALUES (@OrgPk, 'TESTORG')

INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES (@AddressPk, @OrgPk, 'Address Line')

INSERT INTO dbo.JobShipment (JS_PK,JS_UniqueConsignRef, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser, JS_IsForwardRegistered,JS_TransportMode, JS_RL_NKOrigin,JS_IsCancelled,JS_ShipmentType)
VALUES
	(@shipmentPKMaster, 'B0002', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 1 ,'AIR', 'AUSYD',0,'CLB')

INSERT INTO dbo.JobShipment (JS_PK,JS_UniqueConsignRef, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser, JS_IsForwardRegistered,JS_TransportMode, JS_RL_NKOrigin,JS_IsCancelled, JS_ShipmentType, JS_JS_ColoadMasterShipment)
VALUES
	(@shipmentPK, 'B0001', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 1 ,'AIR', 'AUSYD',0, 'STD', @shipmentPKMaster)

INSERT INTO dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode, E2_OA_Address, E2_AddressType)
			VALUES (NEWID(), @shipmentPKMaster, 'JS', @AddressPk, 'CRD')


INSERT INTO dbo.JobConsol([JK_PK], [JK_UniqueConsignRef], [JK_SystemCreateTimeUtc], [JK_SystemCreateUser], [JK_SystemLastEditTimeUtc], [JK_SystemLastEditUser])
			VALUES (@ConsolPK, 'C12', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.JobConShipLink([JN_PK], [JN_JS], [JN_JK], [JN_SystemCreateTimeUtc], [JN_SystemCreateUser], [JN_SystemLastEditTimeUtc], [JN_SystemLastEditUser])
			VALUES (@ConsolLinkPK, @shipmentPK, @ConsolPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.JobConsolTransport([JW_PK], [JW_ParentGUID], [JW_ETD], [JW_ETA], [JW_SystemCreateTimeUtc], [JW_SystemCreateUser], [JW_SystemLastEditTimeUtc], [JW_SystemLastEditUser])
			VALUES (newID(), @ConsolPK, '2022-03-03', '2022-03-04', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.JobContainer (JC_PK, JC_ContainerNum, JC_ContainerMode) 
VALUES
	(@jobContainerPK1, 'JC1','FCL'),
	(@jobContainerPK2, 'JC2','FCL'),
	(@jobContainerPK3, 'JC3','FCL');

INSERT INTO dbo.JobPackLines (JL_PK, JL_JS, JL_PackLineId,JL_SystemCreateTimeUtc, JL_SystemCreateUser, JL_SystemLastEditTimeUtc, JL_SystemLastEditUser, JL_PackageCount)
				VALUES (@JobPackLinePK,@shipmentPK, 'CFS-JL301', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 10)

INSERT INTO dbo.JobContainerPackPivot (J6_PK, J6_JC, J6_JL, J6_SystemCreateTimeUtc, J6_SystemCreateUser, J6_SystemLastEditTimeUtc, J6_SystemLastEditUser)
				VALUES (NEWID(), @jobContainerPK1,@JobPackLinePK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

			using (var command = Db.Connection.Command(prepareSql))
			{
				command.AddParameter("@CompanyPK1", SqlDbType.UniqueIdentifier, companyPk1);
				command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, orgPk);
				command.ExecuteNonQuery();
			}

			var reportSql = $@"
DECLARE @ClientPKs TVP_uniqueidentifier
INSERT @ClientPKs
VALUES ('{orgPk}')

SELECT JC_BusinessType, JC_ContainerNum, JobReference, PackageCountTotal
FROM Report_ShipmentContainersByClientAndCarrier(@ClientPKs, 'Only where Client is Shipment Consignor', 'ALL', 'Shipment Only', null, null, '', '{companyPk1}')";

			var resultList = new List<(string, string, string, int)>();
			Db.Connection.ExecuteReader(reportSql,
				reader =>
				{
					resultList.Add((reader["JC_BusinessType"].ToString(), reader["JC_ContainerNum"].ToString(), reader["JobReference"].ToString(), (int)reader["PackageCountTotal"]));
				});
			AssertContainsExactElementsInAnyOrder("Only FR and non-null JobContainer selected", new[] { ("SHP", "JC1", "B0002", 10) }, resultList);
		}
	}
}


using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc.Testing
{
	[TestedType(typeof(HasEnhancedAccessOSMGShipment))]
	class HasEnhancedAccessOSMGShipmentTest : DbCreateScriptTest
	{
		public void TestGeneralUsage()
		{
			CreateTestData();

			var expectShipmentsForStaff1 = new List<Guid>
			{
				shipmentA4.PK,
				shipmentC4.PK
			};
			AssertContainsExactElementsInAnyOrder(expectShipmentsForStaff1, GetShipments(companyPK1, staffPK1));

			var expectShipmentsForStaff2 = new List<Guid>
			{
				shipmentA1.PK, shipmentA4.PK, shipmentA5.PK,
				shipmentC1.PK, shipmentC4.PK, shipmentC5.PK,
				shipmentD1.PK, shipmentD4.PK, shipmentD5.PK
			};
			AssertContainsExactElementsInAnyOrder(expectShipmentsForStaff2, GetShipments(companyPK1, staffPK2));
		}

		List<Guid> GetShipments(Guid companyPK, Guid staffPK)
		{
			using (var command = TestConnection.Command($"SELECT JS_PK FROM dbo.JobShipment WHERE (SELECT HasAccess FROM HasEnhancedAccessOSMGShipment (JS_PK, '{companyPK}', '{staffPK}' )) = 1"))
			using (var reader = command.ExecuteReader())
			{
				var shipmentPKs = new List<Guid>();

				while (reader.Read())
				{
					shipmentPKs.Add((Guid)reader["JS_PK"]);
				}

				return shipmentPKs;
			}
		}

		void CreateTestData()
		{
			var insertSQL = new StringBuilder($@"
DECLARE @Company1 UNIQUEIDENTIFIER = '{companyPK1}';
DECLARE @Company2 UNIQUEIDENTIFIER = '{companyPK2}';

DECLARE @Branch1 UNIQUEIDENTIFIER = '{branchPK1}';
DECLARE @Branch2 UNIQUEIDENTIFIER = '{branchPK2}';

DECLARE @Department1 UNIQUEIDENTIFIER = '{departmentPK1}';
DECLARE @Department2 UNIQUEIDENTIFIER = '{departmentPK2}';

DECLARE @GlbStaff1 UNIQUEIDENTIFIER = '{staffPK1}';
DECLARE @GlbStaff2 UNIQUEIDENTIFIER = '{staffPK2}';

DECLARE @GlbGroupOSMG1 UNIQUEIDENTIFIER = '{groupOSMG1PK}';
DECLARE @GlbGroupOSMG2 UNIQUEIDENTIFIER = '{groupOSMG2PK}';

DECLARE @GlbGroupLinkStaff1OSMG1 UNIQUEIDENTIFIER = NEWID();
DECLARE @GlbGroupLinkStaff2OSMG1 UNIQUEIDENTIFIER = NEWID();
DECLARE @GlbGroupLinkStaff2OSMG2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES
	(@Company1, 'XCU', 'AU company1', 'AU', 'AUD'),
	(@Company2, 'YCU', 'AU company2', 'AU', 'AUD');

INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES
	(@Branch1, @Company1, 'XBU'),
	(@Branch2, @Company2, 'YBU');

INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code) VALUES
	(@Department1, 'XDU'),
	(@Department2, 'YDU');

INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES
	(@GlbStaff1, 'ST1', 'LoginST1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@GlbStaff2, 'ST2', 'LoginST2', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.GlbGroup (GG_PK, GG_Code) VALUES
	(@GlbGroupOSMG1, 'GP1'),
	(@GlbGroupOSMG2, 'GP2');

INSERT INTO dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS) VALUES
	(@GlbGroupLinkStaff1OSMG1, @GlbGroupOSMG1, @GlbStaff1),
	(@GlbGroupLinkStaff2OSMG1, @GlbGroupOSMG1, @GlbStaff2),
	(@GlbGroupLinkStaff2OSMG2, @GlbGroupOSMG2, @GlbStaff2);
");

			var i = 0;
			CreateTestData(insertSQL, shipmentA1, groupOSMG1PK, groupOSMG2PK, false, null, i++);
			CreateTestData(insertSQL, shipmentA2, null, groupOSMG1PK, false, null, i++);
			CreateTestData(insertSQL, shipmentA3, null, groupOSMG2PK, false, null, i++);
			CreateTestData(insertSQL, shipmentA4, groupOSMG1PK, groupOSMG1PK, false, null, i++);
			CreateTestData(insertSQL, shipmentA5, groupOSMG2PK, groupOSMG2PK, false, null, i++);

			CreateTestData(insertSQL, shipmentB1, groupOSMG1PK, groupOSMG2PK, true, null, i++);
			CreateTestData(insertSQL, shipmentB2, null, groupOSMG1PK, true, null, i++);
			CreateTestData(insertSQL, shipmentB3, null, groupOSMG2PK, true, null, i++);
			CreateTestData(insertSQL, shipmentB4, groupOSMG1PK, groupOSMG1PK, true, null, i++);
			CreateTestData(insertSQL, shipmentB5, groupOSMG2PK, groupOSMG2PK, true, null, i++);

			CreateTestData(insertSQL, shipmentC1, groupOSMG1PK, groupOSMG2PK, true, groupOSMG1PK, i++);
			CreateTestData(insertSQL, shipmentC2, null, groupOSMG1PK, true, groupOSMG1PK, i++);
			CreateTestData(insertSQL, shipmentC3, null, groupOSMG2PK, true, groupOSMG1PK, i++);
			CreateTestData(insertSQL, shipmentC4, groupOSMG1PK, groupOSMG1PK, true, groupOSMG1PK, i++);
			CreateTestData(insertSQL, shipmentC5, groupOSMG2PK, groupOSMG2PK, true, groupOSMG1PK, i++);

			CreateTestData(insertSQL, shipmentD1, groupOSMG1PK, groupOSMG2PK, true, groupOSMG2PK, i++);
			CreateTestData(insertSQL, shipmentD2, null, groupOSMG1PK, true, groupOSMG2PK, i++);
			CreateTestData(insertSQL, shipmentD3, null, groupOSMG2PK, true, groupOSMG2PK, i++);
			CreateTestData(insertSQL, shipmentD4, groupOSMG1PK, groupOSMG1PK, true, groupOSMG2PK, i++);
			CreateTestData(insertSQL, shipmentD5, groupOSMG2PK, groupOSMG2PK, true, groupOSMG2PK, i++);

			using (var command = TestConnection.Command(insertSQL.ToString()))
			{
				command.ExecuteNonQuery();
			}
		}

		void CreateTestData(StringBuilder insertSQL, (Guid PK, string JobNum) shipment, Guid? osmgGroupForJobDocAddres1, Guid? osmgGroupForJobDocAddres2, bool includeJobHeader, Guid? osmgGroupForJobHeader, int sequence)
		{
			var orgHeader1 = Guid.NewGuid();
			var orgHeader2 = Guid.NewGuid();
			var orgHeader3 = Guid.NewGuid();
			var orgAddress1 = Guid.NewGuid();
			var orgAddress2 = Guid.NewGuid();
			var orgAddress3 = Guid.NewGuid();

			insertSQL.AppendLine(GetOrgHeaderInsertSQL(orgHeader1, $"TSTORGA{sequence}"));
			insertSQL.AppendLine(GetOrgHeaderInsertSQL(orgHeader2, $"TSTORGB{sequence}"));
			insertSQL.AppendLine(GetOrgHeaderInsertSQL(orgHeader3, $"TSTORGC{sequence}"));
			insertSQL.AppendLine(GetOrgAddressInsertSQL(orgAddress1, orgHeader1));
			insertSQL.AppendLine(GetOrgAddressInsertSQL(orgAddress2, orgHeader2));
			insertSQL.AppendLine(GetOrgAddressInsertSQL(orgAddress3, orgHeader3));
			insertSQL.AppendLine(GetJobShipmentInsertSQL(shipment));
			insertSQL.AppendLine(GetJobDocAddressInsertSQL(shipment.PK, orgAddress1, "CED"));
			insertSQL.AppendLine(GetJobDocAddressInsertSQL(shipment.PK, orgAddress2, "CRD"));
			insertSQL.AppendLine(GetJobDocAddressInsertSQL(shipment.PK, orgAddress3, "XYZ"));
			insertSQL.AppendLine(GetOrgMiscServInsertSQL(orgHeader1, osmgGroupForJobDocAddres1));
			insertSQL.AppendLine(GetOrgMiscServInsertSQL(orgHeader2, osmgGroupForJobDocAddres2));
			insertSQL.AppendLine(GetOrgMiscServInsertSQL(orgHeader3, osmgGroupForJobDocAddres1));

			if (includeJobHeader)
			{
				var orgHeader4 = Guid.NewGuid();
				var orgHeader5 = Guid.NewGuid();
				var orgAddress4 = Guid.NewGuid();
				var orgAddress5 = Guid.NewGuid();

				insertSQL.AppendLine(GetOrgHeaderInsertSQL(orgHeader4, $"TSTORGD{sequence}"));
				insertSQL.AppendLine(GetOrgHeaderInsertSQL(orgHeader5, $"TSTORGE{sequence}"));
				insertSQL.AppendLine(GetOrgAddressInsertSQL(orgAddress4, orgHeader4));
				insertSQL.AppendLine(GetOrgAddressInsertSQL(orgAddress5, orgHeader5));
				insertSQL.AppendLine(GetJobHeaderInsertSQL(shipment, companyPK1, branchPK1, departmentPK1, orgAddress4));
				insertSQL.AppendLine(GetJobHeaderInsertSQL(shipment, companyPK2, branchPK2, departmentPK2, orgAddress4));
				insertSQL.AppendLine(GetOrgMiscServInsertSQL(orgHeader4, osmgGroupForJobHeader));
				insertSQL.AppendLine(GetOrgMiscServInsertSQL(orgHeader5, osmgGroupForJobHeader));
			}
		}

		string GetOrgHeaderInsertSQL(Guid pk, string code)
			=> $"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES ('{pk}', '{code}');";

		string GetOrgAddressInsertSQL(Guid addressPK, Guid orgPk)
			=> $"INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES ('{addressPK}', '{orgPk}', 'Address Line')";

		string GetJobShipmentInsertSQL((Guid PK, string JobNum) shipment)
			=> $"INSERT INTO dbo.JobShipment(JS_PK, JS_UniqueConsignRef) VALUES ('{shipment.PK}', '{shipment.JobNum}')";

		string GetJobDocAddressInsertSQL(Guid shipmentPK, Guid addressPK, string addressType)
			=> $"INSERT INTO dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode, E2_OA_Address, E2_AddressType) VALUES (NEWID(), '{shipmentPK}', 'JS', '{addressPK}', '{addressType}')";

		string GetOrgMiscServInsertSQL(Guid orgPK, Guid? osmgGroup)
			=> $"INSERT INTO dbo.OrgMiscServ (OM_PK, OM_OH, OM_GG_OrgSecurityGroup) VALUES (NEWID(), '{orgPK}', {(osmgGroup.HasValue ? "'" + osmgGroup.Value.ToString() + "'" : "null")});";

		string GetJobHeaderInsertSQL((Guid PK, string JobNum) shipment, Guid companyPK, Guid branchPK, Guid departmentPK, Guid orgAddress)
			=> $"INSERT INTO dbo.JobHeader (JH_PK, JH_GC, JH_GB, JH_GE, JH_ParentID, JH_Status, JH_JobNum, JH_OA_LocalChargesAddr) VALUES (NEWID(), '{companyPK}', '{branchPK}', '{departmentPK}', '{shipment.PK}', 'WRK', '{shipment.JobNum}', '{orgAddress}');";

		#region Implementation

		readonly Guid companyPK1 = Guid.NewGuid();
		readonly Guid companyPK2 = Guid.NewGuid();

		readonly Guid branchPK1 = Guid.NewGuid();
		readonly Guid branchPK2 = Guid.NewGuid();

		readonly Guid departmentPK1 = Guid.NewGuid();
		readonly Guid departmentPK2 = Guid.NewGuid();

		readonly Guid staffPK1 = Guid.NewGuid();
		readonly Guid staffPK2 = Guid.NewGuid();

		readonly Guid groupOSMG1PK = Guid.NewGuid();
		readonly Guid groupOSMG2PK = Guid.NewGuid();

		(Guid PK, string JobNum) shipmentA1 = (Guid.NewGuid(), "S000987A1");
		(Guid PK, string JobNum) shipmentA2 = (Guid.NewGuid(), "S000987A2");
		(Guid PK, string JobNum) shipmentA3 = (Guid.NewGuid(), "S000987A3");
		(Guid PK, string JobNum) shipmentA4 = (Guid.NewGuid(), "S000987A4");
		(Guid PK, string JobNum) shipmentA5 = (Guid.NewGuid(), "S000987A5");

		(Guid PK, string JobNum) shipmentB1 = (Guid.NewGuid(), "S000987B1");
		(Guid PK, string JobNum) shipmentB2 = (Guid.NewGuid(), "S000987B2");
		(Guid PK, string JobNum) shipmentB3 = (Guid.NewGuid(), "S000987B3");
		(Guid PK, string JobNum) shipmentB4 = (Guid.NewGuid(), "S000987B4");
		(Guid PK, string JobNum) shipmentB5 = (Guid.NewGuid(), "S000987B5");

		(Guid PK, string JobNum) shipmentC1 = (Guid.NewGuid(), "S000987C1");
		(Guid PK, string JobNum) shipmentC2 = (Guid.NewGuid(), "S000987C2");
		(Guid PK, string JobNum) shipmentC3 = (Guid.NewGuid(), "S000987C3");
		(Guid PK, string JobNum) shipmentC4 = (Guid.NewGuid(), "S000987C4");
		(Guid PK, string JobNum) shipmentC5 = (Guid.NewGuid(), "S000987C5");

		(Guid PK, string JobNum) shipmentD1 = (Guid.NewGuid(), "S000987D1");
		(Guid PK, string JobNum) shipmentD2 = (Guid.NewGuid(), "S000987D2");
		(Guid PK, string JobNum) shipmentD3 = (Guid.NewGuid(), "S000987D3");
		(Guid PK, string JobNum) shipmentD4 = (Guid.NewGuid(), "S000987D4");
		(Guid PK, string JobNum) shipmentD5 = (Guid.NewGuid(), "S000987D5");

		#endregion
	}
}

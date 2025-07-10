using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Shipment.Testing
{
	[TestedType(typeof(Report_ShipmentDeliveryLegsProofOfDelivery))]
	internal class Report_ShipmentDeliveryLegsProofOfDeliveryTest : DbCreateScriptTest
	{
		public void TestPackagesDelivered()
		{
			var sqlCmd = "SELECT SUM(CAST(J8_PackagesDelivered AS bigint)) FROM dbo.JobTransportLegPackLineDivot";

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(sqlCmd));
		}

		public void TestDeliveryDueDate()
		{
			var sqlCmd = $@"
UPDATE dbo.JobShipment
SET
	JS_DeliveryDueDate = '06/06/2000 12:00:00',
	JS_SystemLastEditTimeUtc = GETUTCDATE(),
	JS_SystemLastEditUser = '~BP'
WHERE
	JS_PK = '{adlShipmentPk}'
";
			TestConnection.ExecuteNonQuery(sqlCmd);

			var query = @"
SELECT JS_DeliveryDueDate FROM Report_ShipmentDeliveryLegsProofOfDelivery
(
	@CompanyPk, 'Y'
)";
			using (var command = TestConnection.Command(query))
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, Guid.Parse(companyPk));

				using (var reader = command.ExecuteReader())
				{
					Assert("Should have a row", reader.Read());
					AssertEquals("JS_DeliveryDueDate", new DateTime(2000, 6, 6, 12, 0, 0), (DateTime)reader["JS_DeliveryDueDate"]);
				}
			}
		}

		public void TestRevisedDeliveryDueDate()
		{
			var sqlCmd = $@"
UPDATE dbo.JobShipment
SET
	JS_RevisedDeliveryDueDate = '06/06/2050 13:00:00',
	JS_SystemLastEditTimeUtc = GETUTCDATE(),
	JS_SystemLastEditUser = '~BP'
WHERE JS_PK = '{adlShipmentPk}'
";
			TestConnection.ExecuteNonQuery(sqlCmd);

			var query = @"
SELECT JS_RevisedDeliveryDueDate FROM Report_ShipmentDeliveryLegsProofOfDelivery
(
	@CompanyPk, 'Y'
)";
			using (var command = TestConnection.Command(query))
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, Guid.Parse(companyPk));

				using (var reader = command.ExecuteReader())
				{
					Assert("Should have a row", reader.Read());
					AssertEquals("JS_RevisedDeliveryDueDate", new DateTimeOffset(2050, 6, 6, 13, 0, 0, TimeSpan.Zero), (DateTimeOffset)reader["JS_RevisedDeliveryDueDate"]);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			#region Prepare Test Data

			var sqlCmd = $@"
DECLARE @CompanyPk AS UNIQUEIDENTIFIER = '{companyPk}'
DECLARE @BranchPk AS UNIQUEIDENTIFIER = '{branchPk}'
DECLARE @DepartmentPk AS UNIQUEIDENTIFIER = 'A433C594-8C39-46D2-B601-30877A9F69FB'
DECLARE @OrgPk AS UNIQUEIDENTIFIER = 'DA5097B3-064E-4EEA-A9D3-A9E192311D89'
DECLARE @OrgAddress AS UNIQUEIDENTIFIER = '14FEC16A-88BC-4484-9D42-732D2AEF358E'

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'DAN', 'AU company', 'AU', 'AUD')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk, 'TESTORGADL')
INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES (@OrgAddress, @OrgPk, 'Address 1')

DECLARE @CommisionableChargeCodePk AS UNIQUEIDENTIFIER = '6717DBA0-BDF4-41A2-BA3A-A44246D71955'
DECLARE @UncommisionableChargeCodePk AS UNIQUEIDENTIFIER = 'E01CB27E-D33A-4CC7-B8BB-82D8A52434D8'
INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_IsCommissionable) VALUES (@CommisionableChargeCodePk, 'ISCOM', 'FRT', 1)
INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeGroup, AC_IsCommissionable) VALUES (@UncommisionableChargeCodePk, 'NOTCOM', 'FRT', 0)

DECLARE @PerPk UNIQUEIDENTIFIER = newid()
INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName) values (@PerPk, 'name')

DECLARE @AdlStaffPk AS UNIQUEIDENTIFIER = 'A1A752D4-59F0-433B-839B-C70E49A0E979'
DECLARE @RisStaffPk AS UNIQUEIDENTIFIER = '3B4EE755-4883-4E82-97D7-DE563FF418E2'
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_IsSalesRep, GS_CommissionBasis, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@AdlStaffPk, 'ADL', 'Andrew', 1, 'REV', @PerPk, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_IsSalesRep, GS_CommissionBasis, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@RisStaffPk, 'RIS', 'Richard', 1, 'PRF', @PerPk, GETUTCDATE(), 'E', GETUTCDATE(), 'E')

DECLARE @AdlShipmentPk AS UNIQUEIDENTIFIER = '{adlShipmentPk}'
DECLARE @AdlShipmentJobPk AS UNIQUEIDENTIFIER = '3AB47702-EDF6-4E19-89AF-BF42A2753380'
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef) VALUES (@AdlShipmentPk, 'S00001005')
INSERT INTO dbo.JobHeader (JH_PK, JH_JobNum, JH_GB, JH_GE, JH_GC, JH_OA_LocalChargesAddr, JH_ParentTableCode, JH_ParentID, JH_GS_NKRepSales, JH_Status) VALUES (@AdlShipmentJobPk, 'S00001005',  @BranchPk, @DepartmentPk, @CompanyPk, @OrgAddress, 'JS', @AdlShipmentPk, 'ADL', 'CLS')
INSERT INTO dbo.AccTransactionLines (AL_PK, AL_JH, AL_LineType, AL_LineAmount, AL_AC, AL_GE, AL_GB, AL_ReverseDate, AL_GC) VALUES (newid(), @AdlShipmentJobPk, 'REV', 922337203685477.5807, @CommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-1-1', @CompanyPk)
INSERT INTO dbo.AccTransactionLines (AL_PK, AL_JH, AL_LineType, AL_LineAmount, AL_AC, AL_GE, AL_GB, AL_ReverseDate, AL_GC) VALUES (newid(), @AdlShipmentJobPk, 'CST', -922337203685477.5808, @CommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-1-1', @CompanyPk)
INSERT INTO dbo.AccTransactionLines (AL_PK, AL_JH, AL_LineType, AL_LineAmount, AL_AC, AL_GE, AL_GB, AL_ReverseDate, AL_GC) VALUES (newid(), @AdlShipmentJobPk, 'REV', 20.00,  @UncommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-1-1', @CompanyPk)
INSERT INTO dbo.AccTransactionLines (AL_PK, AL_JH, AL_LineType, AL_LineAmount, AL_AC, AL_GE, AL_GB, AL_ReverseDate, AL_GC) VALUES (newid(), @AdlShipmentJobPk, 'CST', -10.00, @UncommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-1-1', @CompanyPk)

INSERT INTO dbo.JobTransportLegPackLineDivot (J8_PK, J8_PackagesDelivered, J8_SystemCreateTimeUtc, J8_SystemCreateUser, J8_SystemLastEditTimeUtc, J8_SystemLastEditUser) VALUES (newid(), 999999999, '2001-1-1', 'ABC', '2001-1-1', 'ABC')
INSERT INTO dbo.JobTransportLegPackLineDivot (J8_PK, J8_PackagesDelivered, J8_SystemCreateTimeUtc, J8_SystemCreateUser, J8_SystemLastEditTimeUtc, J8_SystemLastEditUser) VALUES (newid(), 999999999, '2001-1-1', 'ABC', '2001-1-1', 'ABC')
INSERT INTO dbo.JobTransportLegPackLineDivot (J8_PK, J8_PackagesDelivered, J8_SystemCreateTimeUtc, J8_SystemCreateUser, J8_SystemLastEditTimeUtc, J8_SystemLastEditUser) VALUES (newid(), 999999999, '2001-1-1', 'ABC', '2001-1-1', 'ABC')
";
			TestConnection.ExecuteNonQuery(sqlCmd);

			#endregion
		}

		const string adlShipmentPk = "9A1E098C-037A-4A62-989E-72E14F9C2618";
		const string companyPk = "D381CB3B-281E-4BA4-B7B8-A0B045FDA68D";
		const string branchPk = "3C282841-E41C-4CBD-9A98-15ED3C775433";
	}
}


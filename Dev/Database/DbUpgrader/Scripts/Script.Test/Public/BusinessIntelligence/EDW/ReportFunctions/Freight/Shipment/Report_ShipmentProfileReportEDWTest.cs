using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment.Testing
{
	[TestedType(typeof(Report_ShipmentProfileReport))]
	class Report_ShipmentProfileReportEDWTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestNoExceptionWhenAL_LineAmountSumOutOfMoneyRange()
		{
			var query = $@"
					SELECT * FROM {ScriptDbName}.dbo.Report_ShipmentProfileReport
					(
						'AU', @CompanyPk, '20F, 20R, 20H, 40F, 40R, 40H, 45F, GEN', '', '', 'ALL', '', '', NULL, 'ALL', 'N', 
						'01/01/1900 00:00:00', '06/06/2079 23:59:29', '01/01/1900 00:00:00', '06/06/2079 23:59:29'
					)";
			using (var command = TestConnection.Command(query))
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, Guid.Parse(companyPk));

				using (var reader = command.ExecuteReader())
				{
					Assert("Should have a row", reader.Read());
					AssertEquals("ShipmentID", "S00001005", reader["ShipmentID"]);
					CombineAssertions("Shipment S00001005 sums", () =>
					{
						AssertEquals("REVAmount", 922337203685497.5807m, (decimal)reader["REVAmount"]);
						AssertEquals("CSTAmount", -922337203685487.5808m, (decimal)reader["CSTAmount"]);
					});
				}
			}
		}
		public void TestCusEntryInfo_JobShipment()
		{
			var sqlCmd = $@"
					INSERT INTO {ScriptDbName}.InternationalLogistics.GRP__ShipmentEntryNumbers (EntryNumbers, JS)
					VALUES ('One:11111', '9A1E098C-037A-4A62-989E-72E14F9C2618')
					";
			AssertCusEntryInfo(sqlCmd, "One:11111");
		}
		public void TestCusEntryInfo_JobDeclaration()
		{
			var sqlCmd = $@"
					INSERT INTO {ScriptDbName}.Customs.BAS__Declaration (DeclarationID, JobNumber, BranchKey, JobShipmentKey, DeclarationKey, CompanyID, JobShipmentID)
					VALUES ('62735747-6A21-4803-B02F-A9F2C16B3905', 'DEC01', 348, 91, 1379, '{companyPk}', '{adlShipmentPk}')

					INSERT INTO {ScriptDbName}.InternationalLogistics.GRP__ShipmentEntryNumbers (EntryNumbers, JS)
					VALUES ('Two:22222', '9A1E098C-037A-4A62-989E-72E14F9C2618')
					";
			AssertCusEntryInfo(sqlCmd, "Two:22222");
		}
		void AssertCusEntryInfo(string sqlCmdToPrepareData, string expectCusEntryInfo)
		{
			TestConnection.ExecuteNonQuery(sqlCmdToPrepareData);

			var query = $@"
					SELECT CusEntryInfo FROM {ScriptDbName}.dbo.Report_ShipmentProfileReport
					(
						'AU', @CompanyPk, '20F, 20R, 20H, 40F, 40R, 40H, 45F, GEN', '', '', 'ALL', '', '', NULL, 'ALL', 'N', 
						'01/01/1900 00:00:00', '06/06/2079 23:59:29', '01/01/1900 00:00:00', '06/06/2079 23:59:29'
					)";
			using (var command = TestConnection.Command(query))
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, Guid.Parse(companyPk));

				using (var reader = command.ExecuteReader())
				{
					Assert("Should have a row", reader.Read());
					AssertEquals("CusEntryInfo", expectCusEntryInfo, reader["CusEntryInfo"]);
				}
			}
		}
		public void TestDeclaration()
		{
			var sqlCmd = $@"
						INSERT INTO {ScriptDbName}.Customs.BAS__Declaration (DeclarationID, JobNumber, BranchKey, DeclarationKey, JobShipmentKey, CompanyID, JobShipmentID)
						VALUES ('{adlShipmentPk}', 'DEC01', 348, 1, 91, '{companyPk}', '{adlShipmentPk}')
						";
			TestConnection.ExecuteNonQuery(sqlCmd);

			var query = $@"
						SELECT Declaration FROM {ScriptDbName}.dbo.Report_ShipmentProfileReport
						(
							'AU', @CompanyPk, '20F, 20R, 20H, 40F, 40R, 40H, 45F, GEN', '', '', 'ALL', '', '', NULL, 'ALL', 'N', 
							'01/01/1900 00:00:00', '06/06/2079 23:59:29', '01/01/1900 00:00:00', '06/06/2079 23:59:29'
						)";
			using (var command = TestConnection.Command(query))
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, Guid.Parse(companyPk));

				using (var reader = command.ExecuteReader())
				{
					Assert("Should have a row", reader.Read());
					AssertEquals("Declaration", "Y", reader["Declaration"]);
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
					DECLARE @OrgMiscServPk AS UNIQUEIDENTIFIER = 'DA5097B3-064E-4EEA-A9D3-A9E192311D89'
					DECLARE @OrgAddress AS UNIQUEIDENTIFIER = '14FEC16A-88BC-4484-9D42-732D2AEF358E'
					
					INSERT INTO {ScriptDbName}.Organization.BAS__Company (CompanyID, CountryCode, LocalCurrency, CompanyKey) VALUES (@CompanyPk, 'AU', 'AUD', 1)
					INSERT INTO {ScriptDbName}.Organization.BAS__Branch (BranchID, BranchKey) VALUES (@BranchPk, 11)
					INSERT INTO {ScriptDbName}.Organization.BAS__Department (DepartmentID, DepartmentKey) VALUES (@DepartmentPk, 21)
					INSERT INTO {ScriptDbName}.Organization.BAS__Organization (OrganizationID, Code, OrganizationKey) VALUES (@OrgPk, 'TESTORGADL', 31)
					INSERT INTO {ScriptDbName}.Organization.BAS__OrganizationMiscServ (OrganizationMiscServID, OrganizationMiscServKey) VALUES (@OrgMiscServPk, 41)
					INSERT INTO {ScriptDbName}.Organization.BAS__OrganizationAddress (OrganizationAddressID, Organization, Address1, OrganizationAddressKey) VALUES (@OrgAddress, @OrgPk, 'Address 1', 51)
					
					DECLARE @CommisionableChargeCodePk AS UNIQUEIDENTIFIER = '6717DBA0-BDF4-41A2-BA3A-A44246D71955'
					DECLARE @UncommisionableChargeCodePk AS UNIQUEIDENTIFIER = 'E01CB27E-D33A-4CC7-B8BB-82D8A52434D8'
					INSERT INTO {ScriptDbName}.Finance.BAS__ChargeCode (ChargeCodeID, Code, ChargeGroup, IsCommissionable, ChargeCodeKey) VALUES (@CommisionableChargeCodePk, 'ISCOM', 'FRT', 1, 61)
					INSERT INTO {ScriptDbName}.Finance.BAS__ChargeCode (ChargeCodeID, Code, ChargeGroup, IsCommissionable, ChargeCodeKey) VALUES (@UncommisionableChargeCodePk, 'NOTCOM', 'FRT', 0, 62)
					
					DECLARE @PerPk UNIQUEIDENTIFIER = newid()
					INSERT INTO {ScriptDbName}.Organization.BAS__Person(PersonID, FullName, PersonKey) values (@PerPk, 'name', 71)
					
					DECLARE @AdlStaffPk AS UNIQUEIDENTIFIER = 'A1A752D4-59F0-433B-839B-C70E49A0E979'
					DECLARE @RisStaffPk AS UNIQUEIDENTIFIER = '3B4EE755-4883-4E82-97D7-DE563FF418E2'
					INSERT INTO {ScriptDbName}.Organization.BAS__Staff (StaffID, Code, StaffKey) VALUES (@AdlStaffPk, 'ADL', 81)
					INSERT INTO {ScriptDbName}.Organization.BAS__Staff (StaffID, Code, StaffKey) VALUES (@RisStaffPk, 'RIS', 82)
					
					DECLARE @AdlShipmentPk AS UNIQUEIDENTIFIER = '{adlShipmentPk}'
					DECLARE @AdlShipmentJobPk AS UNIQUEIDENTIFIER = '3AB47702-EDF6-4E19-89AF-BF42A2753380'
					INSERT INTO {ScriptDbName}.InternationalLogistics.BAS__Shipment (ShipmentID, JobNumber, IsForwardRegistered, IsCancelled, ShipmentKey) VALUES (@AdlShipmentPk, 'S00001005', 1, 0, 91)
					INSERT INTO {ScriptDbName}.Finance.BAS__JobHeader (JobHeaderID, JobNo, BranchID, DepartmentID, CompanyID, LocalAgentAddressID, ParentTableCode, ParentID, RepresentativeSales, Status, IsActive, CompanyKey, JobHeaderKey) VALUES (@AdlShipmentJobPk, 'S00001005',  @BranchPk, @DepartmentPk, @CompanyPk, @OrgAddress, 'JS', @AdlShipmentPk, 'ADL', 'CLS', 1, 1, 101)
					INSERT INTO {ScriptDbName}.Finance.BAS__AccGLTransactionLine (AccGLTransactionLineID, JobHeaderID, LineType, LineAmount, ChargeCodeID, DepartmentID, BranchID, ReverseDate, CompanyID, AccGLTransactionLineKey) VALUES (newid(), @AdlShipmentJobPk, 'REV', 922337203685477.5807, @CommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-1-1', @CompanyPk, 111)
					INSERT INTO {ScriptDbName}.Finance.BAS__AccGLTransactionLine (AccGLTransactionLineID, JobHeaderID, LineType, LineAmount, ChargeCodeID, DepartmentID, BranchID, ReverseDate, CompanyID, AccGLTransactionLineKey) VALUES (newid(), @AdlShipmentJobPk, 'CST', -922337203685477.5808, @CommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-1-1', @CompanyPk, 112)
					INSERT INTO {ScriptDbName}.Finance.BAS__AccGLTransactionLine (AccGLTransactionLineID, JobHeaderID, LineType, LineAmount, ChargeCodeID, DepartmentID, BranchID, ReverseDate, CompanyID, AccGLTransactionLineKey) VALUES (newid(), @AdlShipmentJobPk, 'REV', 20.00,  @UncommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-1-1', @CompanyPk, 113)
					INSERT INTO {ScriptDbName}.Finance.BAS__AccGLTransactionLine (AccGLTransactionLineID, JobHeaderID, LineType, LineAmount, ChargeCodeID, DepartmentID, BranchID, ReverseDate, CompanyID, AccGLTransactionLineKey) VALUES (newid(), @AdlShipmentJobPk, 'CST', -10.00, @UncommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-1-1', @CompanyPk, 114)
					INSERT INTO {ScriptDbName}.Finance.BAS__GLTransactionLine(JobHeaderKey, LocalAmount, LineType, TransactionDate, GLTransactionLineID, GLTransactionLineKey, TransactionDateCheck) VALUES(101, 922337203685497.5807, 'REV', '2010-08-13 14:17:00', '52E5080A-3670-4480-B8D2-C7E3FA23DBF8', -1, 'ReverseDate')
					INSERT INTO {ScriptDbName}.Finance.BAS__GLTransactionLine(JobHeaderKey, LocalAmount, LineType, TransactionDate, GLTransactionLineID, GLTransactionLineKey, TransactionDateCheck) VALUES(101, -922337203685487.5808, 'CST', '2010-08-13 14:17:00', '52E5080A-3670-4480-B8D2-C7E3FA23DBF8', -2, 'ReverseDate')
					";
			TestConnection.ExecuteNonQuery(sqlCmd);

			#endregion
		}

		const string adlShipmentPk = "9A1E098C-037A-4A62-989E-72E14F9C2618";
		const string companyPk = "D381CB3B-281E-4BA4-B7B8-A0B045FDA68D";
		const string branchPk = "3C282841-E41C-4CBD-9A98-15ED3C775433";

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}

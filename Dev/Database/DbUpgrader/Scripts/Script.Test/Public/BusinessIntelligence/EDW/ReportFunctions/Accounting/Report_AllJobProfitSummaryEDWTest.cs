using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Accounting.Testing
{
	[TestedType(typeof(Report_AllJobProfitSummary))]
	internal class Report_AllJobProfitSummaryEDWTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			SetUp();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 3, resultTable.Rows.Count);
		}

		public void TestActiveStatusFiltering()
		{
			SetUp();

			var dtWithAllLines = SelectRows(activeStatus: "All");
			var dtWithActiveLines = SelectRows(activeStatus: "Active");
			var dtWithInactiveLines = SelectRows(activeStatus: "Inactive");

			AssertActiveStatusCount(dtWithAllLines, "All", 3, 2, 1);
			AssertActiveStatusCount(dtWithActiveLines, "Active", 2, 2, 0);
			AssertActiveStatusCount(dtWithInactiveLines, "Inactive", 1, 0, 1);
		}

		void AssertActiveStatusCount(DataTable dt, string activeStatus, int allLinesCount, int activeLinesCount, int inactiveLinesCount)
		{
			AssertEquals($"Should have {allLinesCount} lines when active status filter is '{activeStatus}'", allLinesCount, dt.Rows.Count);
			AssertEquals($"Should have {activeLinesCount} lines of active job when active status filter is '{activeStatus}'", activeLinesCount, dt.Select("JH_IsActive = 1").Length);
			AssertEquals($"Should have {inactiveLinesCount} lines of inactive job when active status filter is '{activeStatus}'", inactiveLinesCount, dt.Select("JH_IsActive = 0").Length);
		}

		#region Show Reverse WIP ACR filter test

		public void TestDoNotShowReverseWIPACRFilter()
		{
			var dtWithReverseWIPACR = SelectRows( activeStatus: "All", notIncludeReversedWIPACR: "");
			var dtWithoutReverseeWIPACR = SelectRows(activeStatus: "All", notIncludeReversedWIPACR: "Y");

			AssertEquals("Number of result", 3, dtWithReverseWIPACR.Rows.Count);
			AssertEquals("Number of result", 2, dtWithoutReverseeWIPACR.Rows.Count);
		}

		#endregion

		DataTable SelectRows(string activeStatus = "", string notIncludeReversedWIPACR = "")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT * FROM [{0}].dbo.Report_AllJobProfitSummary('C360CBCB-C34C-43A0-9B00-D3A5779B2882', '1900-01-01 00:00:00', '2079-06-06 23:59:59', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2, NULL, '1900-01-01 00:00:00', '2079-06-06 23:59:29', NULL, '{1}', '{2}')",
				ScriptDbName,
				activeStatus,
				notIncludeReversedWIPACR
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		protected override void SetUp()
		{
			base.SetUp();

			#region Prepare Test Data

			var sqlCmd = $@"
					DECLARE @CompanyPk AS UNIQUEIDENTIFIER = 'C360CBCB-C34C-43A0-9B00-D3A5779B2882'
					DECLARE @BranchPk AS UNIQUEIDENTIFIER = '3C282841-E41C-4CBD-9A98-15ED3C775433'
					DECLARE @DepartmentPk AS UNIQUEIDENTIFIER = 'A433C594-8C39-46D2-B601-30877A9F69FB'
					DECLARE @OrgPk AS UNIQUEIDENTIFIER = 'DA5097B3-064E-4EEA-A9D3-A9E192311D89'
					DECLARE @OrgMiscServPk AS UNIQUEIDENTIFIER = 'DA5097B3-064E-4EEA-A9D3-A9E192311D89'
					DECLARE @OrgAddress AS UNIQUEIDENTIFIER = '14FEC16A-88BC-4484-9D42-732D2AEF358E'
					DECLARE @adlShipmentPk AS UNIQUEIDENTIFIER = '9A1E098C-037A-4A62-989E-72E14F9C2618'
					DECLARE @AdlShipmentJobPk AS UNIQUEIDENTIFIER = '3AB47702-EDF6-4E19-89AF-BF42A2753380'
					DECLARE @InactiveJobShipmentPK AS UNIQUEIDENTIFIER = '23C31636-0273-4D35-BF65-59E317C0BF23'
					DECLARE @InactiveJobPK AS UNIQUEIDENTIFIER = 'F30C4B1F-1061-40BC-BA52-E10EE8550719'
					DECLARE @JobShipment3PK AS UNIQUEIDENTIFIER = '718C1E89-7316-41B3-A6AD-414F34079DC8'
					DECLARE @Job3PK AS UNIQUEIDENTIFIER = '37119C47-8B44-4250-848C-C97CFE009B84'

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
					
					INSERT INTO {ScriptDbName}.InternationalLogistics.BAS__Shipment (ShipmentID, JobNumber, IsForwardRegistered, IsCancelled, ShipmentKey) VALUES (@AdlShipmentPk, 'S00001005', 1, 0, 91)
					INSERT INTO {ScriptDbName}.Finance.BAS__JobHeader (JobHeaderID, JobNo, BranchID, DepartmentID, CompanyID, LocalAgentAddressID, ParentTableCode, ParentID, RepresentativeSales, Status, IsActive, CompanyKey, JobHeaderKey) VALUES (@AdlShipmentJobPk, 'S00001005',  @BranchPk, @DepartmentPk, @CompanyPk, @OrgAddress, 'JS', @AdlShipmentPk, 'ADL', 'CLS', 1, 1, 101)
					INSERT INTO {ScriptDbName}.Finance.BAS__AccGLTransactionLine (AccGLTransactionLineID, JobHeaderID, LineType, LineAmount, ChargeCodeID, DepartmentID, BranchID, PostDateTime, ReverseDateTime, CompanyID, AccGLTransactionLineKey) VALUES (newid(), @AdlShipmentJobPk, 'REV', 922337203685477.5807, @CommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-01-01 00:00:00', '2002-01-01 00:00:00', @CompanyPk, 111)
					INSERT INTO {ScriptDbName}.Finance.BAS__AccGLTransactionLine (AccGLTransactionLineID, JobHeaderID, LineType, LineAmount, ChargeCodeID, DepartmentID, BranchID, PostDateTime, ReverseDateTime, CompanyID, AccGLTransactionLineKey) VALUES (newid(), @AdlShipmentJobPk, 'CST', -922337203685477.5808, @CommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-01-01 00:00:00', '2002-01-01 00:00:00', @CompanyPk, 112)
					INSERT INTO {ScriptDbName}.Finance.BAS__AccGLTransactionLine (AccGLTransactionLineID, JobHeaderID, LineType, LineAmount, ChargeCodeID, DepartmentID, BranchID, PostDateTime, ReverseDateTime, CompanyID, AccGLTransactionLineKey) VALUES (newid(), @AdlShipmentJobPk, 'REV', 20.00,  @UncommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-01-01 00:00:00', '2002-01-01 00:00:00', @CompanyPk, 113)
					INSERT INTO {ScriptDbName}.Finance.BAS__AccGLTransactionLine (AccGLTransactionLineID, JobHeaderID, LineType, LineAmount, ChargeCodeID, DepartmentID, BranchID, PostDateTime, ReverseDateTime, CompanyID, AccGLTransactionLineKey) VALUES (newid(), @AdlShipmentJobPk, 'CST', -10.00, @UncommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-01-01 00:00:00', '2002-01-01 00:00:00', @CompanyPk, 114)
					
					INSERT INTO {ScriptDbName}.InternationalLogistics.BAS__Shipment (ShipmentID, JobNumber, IsForwardRegistered, IsCancelled, ShipmentKey) VALUES (@InactiveJobShipmentPK, 'S00001006', 1, 0, 92)
					INSERT INTO {ScriptDbName}.Finance.BAS__JobHeader (JobHeaderID, JobNo, BranchID, DepartmentID, CompanyID, LocalAgentAddressID, ParentTableCode, ParentID, RepresentativeSales, Status, IsActive, CompanyKey, JobHeaderKey) VALUES (@InactiveJobPK, 'S00001006',  @BranchPk, @DepartmentPk, @CompanyPk, @OrgAddress, 'JS', @InactiveJobShipmentPK, 'ADL', 'CLS', 0, 1, 102)
					INSERT INTO {ScriptDbName}.Finance.BAS__AccGLTransactionLine (AccGLTransactionLineID, JobHeaderID, LineType, LineAmount, ChargeCodeID, DepartmentID, BranchID, PostDateTime, ReverseDateTime, CompanyID, AccGLTransactionLineKey) VALUES (newid(), @InactiveJobPK, 'REV', 922337203685477.5807, @CommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-01-01 00:00:00', '2002-01-01 00:00:00', @CompanyPk, 115)
					INSERT INTO {ScriptDbName}.Finance.BAS__AccGLTransactionLine (AccGLTransactionLineID, JobHeaderID, LineType, LineAmount, ChargeCodeID, DepartmentID, BranchID, PostDateTime, ReverseDateTime, CompanyID, AccGLTransactionLineKey) VALUES (newid(), @InactiveJobPK, 'CST', -922337203685477.5808, @CommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-01-01 00:00:00', '2002-01-01 00:00:00', @CompanyPk, 116)
					INSERT INTO {ScriptDbName}.Finance.BAS__AccGLTransactionLine (AccGLTransactionLineID, JobHeaderID, LineType, LineAmount, ChargeCodeID, DepartmentID, BranchID, PostDateTime, ReverseDateTime, CompanyID, AccGLTransactionLineKey) VALUES (newid(), @InactiveJobPK, 'WIP', -922337203685477.5808, @CommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-01-01 00:00:00', '2002-01-01 00:00:00', @CompanyPk, 117)

					INSERT INTO {ScriptDbName}.InternationalLogistics.BAS__Shipment (ShipmentID, JobNumber, IsForwardRegistered, IsCancelled, ShipmentKey) VALUES (@JobShipment3PK, 'S00001006', 1, 0, 92)
					INSERT INTO {ScriptDbName}.Finance.BAS__JobHeader (JobHeaderID, JobNo, BranchID, DepartmentID, CompanyID, LocalAgentAddressID, ParentTableCode, ParentID, RepresentativeSales, Status, IsActive, CompanyKey, JobHeaderKey) VALUES (@Job3PK, 'S00001007',  @BranchPk, @DepartmentPk, @CompanyPk, @OrgAddress, 'JS', @JobShipment3PK, 'ADL', 'CLS', 1, 1, 103)
					INSERT INTO {ScriptDbName}.Finance.BAS__AccGLTransactionLine (AccGLTransactionLineID, JobHeaderID, LineType, LineAmount, ChargeCodeID, DepartmentID, BranchID, PostDateTime, ReverseDateTime, CompanyID, AccGLTransactionLineKey) VALUES (newid(), @Job3PK, 'WIP', 20.00, @CommisionableChargeCodePk, @DepartmentPk, @BranchPk, '2001-01-01 00:00:00', '2002-01-01 00:00:00', @CompanyPk, 118)
";
			TestConnection.ExecuteNonQuery(sqlCmd);

			#endregion
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}

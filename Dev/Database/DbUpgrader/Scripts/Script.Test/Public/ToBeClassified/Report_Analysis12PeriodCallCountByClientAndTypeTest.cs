using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(Report_Analysis12PeriodCallCountByClientAndType))]
	class Report_Analysis12PeriodCallCountByClientAndTypeTest : DbCreateScriptTest
	{
		public void TestReport()
		{
			Guid orgGuid = Guid.NewGuid();
			Guid org2Guid = Guid.NewGuid();
			Guid staffGuid = Guid.NewGuid();
			Guid staff2Guid = Guid.NewGuid();
			Guid salesCallGuid = Guid.NewGuid();
			Guid salesCall2Guid = Guid.NewGuid();
			var personPk = Guid.NewGuid();

			DataTable companyResult = DataUtils.GetDataTableFromQuery(TestConnection, "select GC_PK from dbo.GlbCompany where GC_Code = 'DEM'");
			Guid glbCompanyPK = (Guid)companyResult.Rows[0][0];

			StringBuilder builder = new StringBuilder();

			builder.AppendLine(@"
declare @org uniqueidentifier
set @org = NEWID()");

			builder.AppendLine(string.Format(@"
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES (@org, 'TESTORG', 'Test Organisation')"));

			builder.AppendLine(string.Format(@"
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName) VALUES ('{0}', 'asd')", personPk));

			builder.AppendLine(string.Format(@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
	VALUES ('{0}', 'KST', 'Kyle Stewart', 'KyleStewart', '{2}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
	VALUES ('{1}', 'SCW', 'Samuel Wang', 'SamuelWang', '{2}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
			", staffGuid, staff2Guid, personPk));

			builder.AppendLine(string.Format(@"
INSERT INTO dbo.OrgStaffAssignments (O8_PK, O8_Role, O8_Department, O8_OH, O8_GS_NKPersonResponsible) VALUES ('{0}', 'CUS', 'ALL', @org, 'KST')
INSERT INTO dbo.OrgStaffAssignments (O8_PK, O8_Role, O8_Department, O8_OH, O8_GS_NKPersonResponsible) VALUES ('{1}', 'SAL', 'ALL', @org, 'SCW')
			", salesCallGuid, salesCall2Guid));

			builder.AppendLine(string.Format(@"
INSERT INTO dbo.OrgSalesCall (OQ_PK, OQ_CallDate, OQ_TypeOfCall, OQ_Status, OQ_OH, OQ_CallSummary, OQ_GS_NKSalesRep, OQ_CommunicationID, OQ_SystemCreateTimeUtc, OQ_SystemCreateUser, OQ_SystemLastEditTimeUtc, OQ_SystemLastEditUser) 
	VALUES ('{0}', '2012-01-03 00:00:00', 'SRV', 'CUR', @org, 'test call', 'KST', 'CM00000001', GetUtcDate(), 'E', GetUtcDate(), 'E')
			", salesCallGuid));

			//setup accounting periods
			builder.AppendLine(string.Format(@"INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
	VALUES('{0}', 201201, 2012, '2012-01-01 00:00:00', '2012-01-31 23:59:00', 1	, 1, '{12}', 0, 0)
INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
	VALUES('{1}', 201202, 2012, '2012-02-01 00:00:00', '2012-02-28 23:59:00', 1	, 1, '{12}', 0, 0)
INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
	VALUES('{2}', 201203, 2012, '2012-03-01 00:00:00', '2012-03-31 23:59:00', 1	, 1, '{12}', 0, 0)
INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
	VALUES('{3}', 201204, 2012, '2012-04-01 00:00:00', '2012-04-30 23:59:00', 1	, 1, '{12}', 0, 0)
INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
	VALUES('{4}', 201205, 2012, '2012-05-01 00:00:00', '2012-05-31 23:59:00', 1	, 1, '{12}', 0, 0)
INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
	VALUES('{5}', 201206, 2012, '2012-06-01 00:00:00', '2012-06-30 23:59:00', 1	, 1, '{12}', 0, 0)
INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
	VALUES('{6}', 201207, 2012, '2012-07-01 00:00:00', '2012-07-31 23:59:00', 1	, 1, '{12}', 0, 0)
INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
	VALUES('{7}', 201208, 2012, '2012-08-01 00:00:00', '2012-08-31 23:59:00', 1	, 1, '{12}', 0, 0)
INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
	VALUES('{8}', 201209, 2012, '2012-09-01 00:00:00', '2012-09-30 23:59:00', 1	, 1, '{12}', 0, 0)
INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
	VALUES('{9}', 201210, 2012, '2012-10-01 00:00:00', '2012-10-31 23:59:00', 1	, 1, '{12}', 0, 0)
INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
	VALUES('{10}', 201211, 2012, '2012-11-01 00:00:00', '2012-11-30 23:59:00', 1	, 1, '{12}', 0, 0)
INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_IsSubLedgerClosed, AM_IsGeneralLedgerClosed, AM_GC_Company, AM_IsSubledgerClosedForAdjustments, AM_ArchiveCommenced)
	VALUES('{11}', 201212, 2012, '2012-12-01 00:00:00', '2012-12-31 23:59:00', 1	, 1, '{12}', 0, 0)
			", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), glbCompanyPK));

			TestConnection.ExecuteNonQuery(builder.ToString());

			//Communication without OrgHeader
			var salesInquiry = NewSalesInquiryWithDates("salesInquiryRef12", new DateTime(2012, 6, 1), new DateTime(2012, 6, 1));
			TestConnection.ExecuteNonQuery($@"
UPDATE dbo.OrgColdCallRegister SET O1_CompanyName = 'Z CompanyName', O1_ContactName = 'Z ContactName', O1_SystemLastEditTimeUtc = GETUTCDATE(), O1_SystemLastEditUser = 'E' WHERE O1_PK = '{salesInquiry}';");

			var communication = NewCommunicationWithDates("CM00000045", null, new DateTime(2012, 6, 1), new DateTime(2012, 6, 1), new DateTime(2012, 6, 1));

			var linkSql = $@"
INSERT INTO dbo.RelatedActivityPivot (RAP_PK,RAP_ChildActivityID, RAP_ChildActivityTableCode, RAP_ParentActivityID, RAP_ParentActivityTableCode, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser) 
VALUES(NEWID(), '{communication}', 'OQ', '{salesInquiry}', 'O1', GetUtcDate(), 'E', GetUtcDate(), 'E')";

			TestConnection.ExecuteNonQuery(linkSql);

			string reportQuery = string.Format(@"
exec sp_executesql N'SELECT *
FROM Report_Analysis12PeriodCallCountByClientAndType(''{0}'', 201212) 
ORDER BY OH_FullName,OH_Code,OQ_TypeOfCall option (recompile)'", glbCompanyPK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, reportQuery);

			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);
			AssertEquals("should include Client Sales Rep", "SCW", result.Rows[0]["ClientSalesRepCode"]);
			AssertEquals("should include Client Sales Rep", "Samuel Wang", result.Rows[0]["ClientSalesRepFullName"]);
			AssertEquals("should include Client Customer Service Rep", "KST", result.Rows[0]["ClientCustomerServiceRepCode"]);
			AssertEquals("should include Client Customer Service Rep", "Kyle Stewart", result.Rows[0]["ClientCustomerServiceRepFullName"]);
			AssertEquals("should have 1 call in the first period", 1, result.Rows[0]["Period1"]);
			AssertEquals("Test Organisation", result.Rows[0]["OH_FullName"]);
			AssertEquals("TESTORG", result.Rows[0]["OH_Code"]);
			AssertEquals("TESTORG : Test Organisation", result.Rows[0]["GroupByClient"]);

			var row2 = result.Rows[1];
			AssertEquals("Z CompanyName", row2["OH_FullName"]);
			AssertEquals(DBNull.Value, row2["OH_Code"]);
			AssertEquals(" : Z CompanyName", row2["GroupByClient"]);
			AssertEquals(DBNull.Value, row2["ClientSalesRepCode"]);
			AssertEquals(DBNull.Value, row2["ClientSalesRepFullName"]);
			AssertEquals(DBNull.Value, row2["ClientCustomerServiceRepCode"]);
			AssertEquals(DBNull.Value, row2["ClientCustomerServiceRepFullName"]);
			AssertEquals(1, row2["Period6"]);
		}

		Guid NewSalesInquiryWithDates(string reference, DateTime createdTime, DateTime? callDate)
		{
			var insertQuery = string.Format(@"INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7})
VALUES (@O1_PK, @O1_LeadUniqueReference, @O1_LeadCalledDate, @O1_SystemCreateTimeUtc, 'E', GetUtcDate(), 'E')",
				OrgColdCallRegisterSchema.Constants.TableName,
				OrgColdCallRegisterSchema.Constants.PK,
				OrgColdCallRegisterSchema.Constants.O1_LeadUniqueReference,
				OrgColdCallRegisterSchema.Constants.O1_LeadCalledDate,
				OrgColdCallRegisterSchema.Constants.O1_SystemCreateTimeUtc,
				OrgColdCallRegisterSchema.Constants.O1_SystemCreateUser,
				OrgColdCallRegisterSchema.Constants.O1_SystemLastEditTimeUtc,
				OrgColdCallRegisterSchema.Constants.O1_SystemLastEditUser);

			var inquiryPk = Guid.NewGuid();
			using (var command = TestConnection.Command(insertQuery))
			{
				command.AddParameterBasedOnDbColumn("@O1_PK", inquiryPk, OrgColdCallRegisterSchema.PK);
				command.AddParameterBasedOnDbColumn("@O1_LeadUniqueReference", reference, OrgColdCallRegisterSchema.O1_LeadUniqueReference);
				command.AddParameterBasedOnDbColumn("@O1_LeadCalledDate", (object)callDate ?? DBNull.Value, OrgColdCallRegisterSchema.O1_LeadCalledDate);
				command.AddParameterBasedOnDbColumn("@O1_SystemCreateTimeUtc", createdTime, OrgColdCallRegisterSchema.O1_SystemCreateTimeUtc);
				command.ExecuteNonQuery();
			}

			return inquiryPk;
		}

		Guid NewCommunicationWithDates(string id, Guid? orgPk, DateTime createdTime, DateTime? scheduledDate, DateTime? actualDate)
		{
			var insertCommunication = string.Format(@"INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})
VALUES (@OQ_PK, @OQ_CommunicationID, @OQ_OH, @OQ_NextCall, @OQ_CallDate, @OQ_SystemCreateTimeUtc, 'E', GetUtcDate(), 'E')",
				OrgSalesCallSchema.Constants.TableName,
				OrgSalesCallSchema.Constants.PK,
				OrgSalesCallSchema.Constants.OQ_CommunicationID,
				OrgSalesCallSchema.Constants.OQ_OH,
				OrgSalesCallSchema.Constants.OQ_NextCall,
				OrgSalesCallSchema.Constants.OQ_CallDate,
				OrgSalesCallSchema.Constants.OQ_SystemCreateTimeUtc,
				OrgSalesCallSchema.Constants.OQ_SystemCreateUser,
				OrgSalesCallSchema.Constants.OQ_SystemLastEditTimeUtc,
				OrgSalesCallSchema.Constants.OQ_SystemLastEditUser);

			var communicationPk = Guid.NewGuid();
			using (var command = TestConnection.Command(insertCommunication))
			{
				command.AddParameterBasedOnDbColumn("@OQ_PK", communicationPk, OrgSalesCallSchema.PK);
				command.AddParameterBasedOnDbColumn("@OQ_CommunicationID", id, OrgSalesCallSchema.OQ_CommunicationID);
				command.AddParameterBasedOnDbColumn("@OQ_OH", (object)orgPk ?? DBNull.Value, OrgSalesCallSchema.OQ_OH);
				command.AddParameterBasedOnDbColumn("@OQ_NextCall", (object)scheduledDate ?? DBNull.Value, OrgSalesCallSchema.OQ_NextCall);
				command.AddParameterBasedOnDbColumn("@OQ_CallDate", (object)actualDate ?? DBNull.Value, OrgSalesCallSchema.OQ_CallDate);
				command.AddParameterBasedOnDbColumn("@OQ_SystemCreateTimeUtc", createdTime, OrgSalesCallSchema.OQ_SystemCreateTimeUtc);
				command.ExecuteNonQuery();
			}

			return communicationPk;
		}
	}
}


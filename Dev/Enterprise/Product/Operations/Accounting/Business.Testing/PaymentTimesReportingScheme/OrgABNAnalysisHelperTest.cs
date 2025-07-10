using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.PaymentTimesReportingScheme.Testing
{
	class OrgABNAnalysisHelperTest : TestCaseWithFactory
	{
		public void TestAnalysisEmptyOrNullABNList()
		{
			var helper = new OrgABNImportHelper(Factory);
			string result = null;

			AssertEquals("Pre-condition", 0, Factory.Load<JobRequiredDocument>(new ZQuery()).Length);

			AssertNoExceptionThrown("Should have no exception with null list", () => result = helper.AnalysisABNList(null, ZDateTime.Now, ZDateTimeOffset.Now));
			AssertNullOrEmpty("Should not return any message", result);
			AssertEquals("Should not create any document tracking records", 0, Factory.Load<JobRequiredDocument>(new ZQuery()).Length);

			AssertNoExceptionThrown("Should have no exception with empty list", () => result = helper.AnalysisABNList(new List<string>(), ZDateTime.Now, ZDateTimeOffset.Now));
			AssertNullOrEmpty("Should not return any message", result);
			AssertEquals("Should not create any document tracking records", 0, Factory.Load<JobRequiredDocument>(new ZQuery()).Length);
		}

		public void TestAnalysisABNList()
		{
			var helper = new OrgABNImportHelper(Factory);
			var oldReportingPeriod = new ZDateTime(2020, 12, 31);
			var reportingPeriod = new ZDateTime(2021, 6, 30);
			var dateReceived = ZDateTimeOffset.Today;
			var nonCurrentCompanyBranch = TestObjectCreator.NonCurrentCompanyBranch;

			var org1 = TestObjectCreator.Creditor1;
			var org2 = TestObjectCreator.Creditor2;
			var org3 = TestObjectCreator.Creditor3;
			var org4 = TestObjectCreator.Creditor4;
			OrgHeader org5 = null;
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), nonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				org5 = testObjectCreator.Creditor5;
			}

			Factory.Save();

			CreateDocumentTrackingRecordForTest(org2.PK, oldReportingPeriod);
			CreateDocumentTrackingRecordForTest(org4.PK, reportingPeriod);

			Factory.Save();

			var abnList = new[]
			{
				"73004700485",
				"73304600485",
				"62604510485",
				"51604520485",
				"40604530485"
			};

			CreateOrgCusCodeForTest(org1.PK, abnList[0]);
			CreateOrgCusCodeForTest(org2.PK, abnList[1]);
			CreateOrgCusCodeForTest(org3.PK, abnList[1]); // org3 and org2 have same ABN
			CreateOrgCusCodeForTest(org4.PK, abnList[2]);
			CreateOrgCusCodeForTest(org5.PK, abnList[2]);

			Factory.Save();

			AssertEquals("Pre-condition", 2, Factory.Load<JobRequiredDocument>(new ZQuery()).Length);
			AssertEquals("org2 has tracking record but with old reporting period", true, IsMatchSingleDocTrackingRecord(org2, oldReportingPeriod));
			AssertEquals("org4 has tracking record with new reporting period", true, IsMatchSingleDocTrackingRecord(org4, reportingPeriod));
			AssertEquals(true, org5.IsCreditorForCompany(nonCurrentCompanyBranch.Company.PK));
			AssertEquals("org5 is NOT a creditor for current company", false, org5.IsCreditorForCompany(GlbCompany.CurrentCompany.PK));

			var result = helper.AnalysisABNList(abnList, reportingPeriod, dateReceived);
			Factory.Save();

			AssertEquals("Notification message for the 1st round import", $@"
Below organizations / ABNs had document tracking record added:
73004700485 - {org1.OH_Code}
73304600485 - {org2.OH_Code}
73304600485 - {org3.OH_Code}

Below organizations / ABNs already had document tracking record created for given reporting period:
62604510485 - {org4.OH_Code}

Below ABNs did not have matched organization:
51604520485
40604530485
			".Trim(), result.Trim());

			var docTrackingRecords = Factory.Load<JobRequiredDocument>(new ZQuery());
			AssertEquals("3 new document tracking records created", 5, docTrackingRecords.Length);
			AssertEquals(true, IsMatchSingleDocTrackingRecord(org1, reportingPeriod));
			AssertEquals(true, IsMatchSingleDocTrackingRecord(org2, oldReportingPeriod));
			AssertEquals(true, IsMatchSingleDocTrackingRecord(org2, reportingPeriod));
			AssertEquals(true, IsMatchSingleDocTrackingRecord(org3, reportingPeriod));
			AssertEquals(true, IsMatchSingleDocTrackingRecord(org4, reportingPeriod));
			AssertEquals(false, IsMatchSingleDocTrackingRecord(org5, reportingPeriod));

			var expectedReference = "Added a new RSB Document Tracking record";

			AssertEquals("Log should be created", 1, org1.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedReference)));
			AssertEquals(2, org2.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedReference)));
			AssertEquals(1, org3.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedReference)));
			AssertEquals(1, org4.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedReference)));
			AssertEquals(0, org5.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.EditedARecord.Code && x.SL_Reference.Contains(expectedReference)));

			var newAbnList = new[]
			{
				"73004700485",
			};

			result = helper.AnalysisABNList(newAbnList, reportingPeriod, dateReceived);
			Factory.Save();

			AssertEquals("Notification message for the 2nd round import", $@"
Below organizations / ABNs already had document tracking record created for given reporting period:
73004700485 - {org1.OH_Code}
			".Trim(), result.Trim());
		}

		#region Implementation

		JobRequiredDocument CreateDocumentTrackingRecordForTest(ZGuid parentId, ZDateTime reportingPeriod)
		{
			var result = Factory.New<JobRequiredDocument>();
			result.EQ_DocCategory = Constants.ReferenceTypes.ComplianceReport;
			result.EQ_DocType = AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines;
			result.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			result.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			result.EQ_ValidToDate = reportingPeriod;
			result.EQ_DateReceived = result.EQ_ValidToDate.ToDateTimeOffset(null);
			result.EQ_RN_NKRelatedCountry = CountryCodes.Australia;
			result.EQ_ParentID = parentId;
			result.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			result.ParentType = typeof(OrgHeader);

			return result;
		}

		OrgCusCode CreateOrgCusCodeForTest(ZGuid parentId, string abnStr)
		{
			var result = Factory.New<OrgCusCode>();
			result.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			result.OK_CustomsRegNo = abnStr;
			result.OK_RN_NKCodeCountry = CountryCodes.Australia;
			result.OK_OH = parentId;

			return result;
		}

		bool IsMatchSingleDocTrackingRecord(OrgHeader org, ZDateTime reportingPeriod)
		{
			var query = new ZQuery(JobRequiredDocumentSchema.EQ_ParentID, org.PK);
			query.AddToFilter(JobRequiredDocumentSchema.EQ_DocCategory, Constants.ReferenceTypes.ComplianceReport);
			query.AddToFilter(JobRequiredDocumentSchema.EQ_DocType, AccountingMasterFilesConstants.ComplianceReportCodes.ReportableSmallBusines);
			query.AddToFilter(JobRequiredDocumentSchema.EQ_DocUsage, JobRequiredDocument.DocUsage.Creditor);
			query.AddToFilter(JobRequiredDocumentSchema.EQ_DocPeriod, Constants.JobRequiredDocuments.DocumentPeriods.Periodic);
			query.AddToFilter(JobRequiredDocumentSchema.EQ_ValidToDate, reportingPeriod);
			query.AddToFilter(JobRequiredDocumentSchema.EQ_RN_NKRelatedCountry, CountryCodes.Australia);
			query.AddToFilter(JobRequiredDocumentSchema.EQ_ParentTableCode, OrgHeaderSchema.Constants.Prefix);

			var result = Factory.Load<JobRequiredDocument>(query);
			return result.Length == 1;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}

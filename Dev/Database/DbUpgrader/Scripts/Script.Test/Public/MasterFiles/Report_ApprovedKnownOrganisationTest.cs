using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles
{
	[TestedType(typeof(Report_ApprovedKnownOrganisation))]
	class Report_ApprovedKnownOrganisationTest : DbCreateScriptTest
	{
		public void TestGetContactInformation()
		{
			var orgPK = Guid.NewGuid();
			var addressPK = Guid.NewGuid();
			var personPK = Guid.NewGuid();
			var testDataSql = $@"
INSERT INTO dbo.OrgHeader
(OH_PK, OH_Code, OH_FullName)
VALUES
('{orgPK}', 'QANAIR_WW', 'QANTAS AIRWAYS')

INSERT INTO dbo.OrgAddress
(OA_PK, OA_Address1, OA_Email, OA_Fax, OA_Mobile, OA_OH, OA_Phone)
VALUES
('{addressPK}', 'CNR QANTAS DRIVE & LINK ROAD', 'sales@qantas.com.au', '+61296912211', '13851888431', '{orgPK}', '+61296910000')

INSERT INTO dbo.GlbPerson
(PER_PK, PER_FullName)
VALUES
('{personPK}', 'John Cooper')

INSERT INTO dbo.OrgContact
(OC_PK, OC_ContactName, OC_Email, OC_Fax, OC_JobCategory, OC_Mobile, OC_OH, OC_PER, OC_Phone)
VALUES
('{Guid.NewGuid()}', 'John Cooper', 'john@qantas.com.au', '+61296912288', 'SEM', '15915471964', '{orgPK}', '{personPK}', '+61296912128')

INSERT INTO dbo.OrgCountryData
(OV_PK, OV_OA_ApprovedLocation, OV_OH_OrgHeader, OV_EXApprovedOrMajorExporter)
VALUES
('{Guid.NewGuid()}', '{addressPK}', '{orgPK}', 'YES')";
			TestConnection.ExecuteNonQuery(testDataSql);

			var reportSql = $"select * from Report_ApprovedKnownOrganisation(null, '') where OrgPK = '{orgPK}'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			CombineAssertions(() =>
			{
				AssertEquals("Result should have rows", 1, result.Rows.Count);
				AssertEquals("John Cooper", result.Rows[0]["SECURITY_CONTACT"]);
				AssertEquals("+61296912128", result.Rows[0]["PHONE"]);
				AssertEquals("15915471964", result.Rows[0]["MOBILE"]);
				AssertEquals("+61296912288", result.Rows[0]["FAX"]);
				AssertEquals("john@qantas.com.au", result.Rows[0]["EMAIL"]);
			});
		}

		public void TestGetPhoneMobileFaxEmail_FromContact_IfThereIsContact_EventhoughTheValuesAreEmpty()
		{
			var orgPK = Guid.NewGuid();
			var addressPK = Guid.NewGuid();
			var personPK = Guid.NewGuid();
			var testDataSql = $@"
INSERT INTO dbo.OrgHeader
(OH_PK, OH_Code, OH_FullName)
VALUES
('{orgPK}', 'QANAIR_WW', 'QANTAS AIRWAYS')

INSERT INTO dbo.OrgAddress
(OA_PK, OA_Address1, OA_Email, OA_Fax, OA_Mobile, OA_OH, OA_Phone)
VALUES
('{addressPK}', 'CNR QANTAS DRIVE & LINK ROAD', 'sales@qantas.com.au', '+61296912211', '13851888431', '{orgPK}', '+61296910000')

INSERT INTO dbo.GlbPerson
(PER_PK, PER_FullName)
VALUES
('{personPK}', 'John Cooper')

INSERT INTO dbo.OrgContact
(OC_PK, OC_ContactName, OC_Email, OC_Fax, OC_JobCategory, OC_Mobile, OC_OH, OC_PER, OC_Phone)
VALUES
('{Guid.NewGuid()}', 'John Cooper', '', '', 'SEM', '', '{orgPK}', '{personPK}', '')

INSERT INTO dbo.OrgCountryData
(OV_PK, OV_OA_ApprovedLocation, OV_OH_OrgHeader, OV_EXApprovedOrMajorExporter)
VALUES
('{Guid.NewGuid()}', '{addressPK}', '{orgPK}', 'YES')";

			TestConnection.ExecuteNonQuery(testDataSql);

			var reportSql = $"select * from Report_ApprovedKnownOrganisation(null, '') where OrgPK = '{orgPK}'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			CombineAssertions(() =>
			{
				AssertEquals("Result should have rows", 1, result.Rows.Count);
				AssertEquals("John Cooper", result.Rows[0]["SECURITY_CONTACT"]);
				AssertEquals("Phone", string.Empty, result.Rows[0]["PHONE"]);
				AssertEquals("Mobile", string.Empty, result.Rows[0]["MOBILE"]);
				AssertEquals("Fax", string.Empty, result.Rows[0]["FAX"]);
				AssertEquals("Email", string.Empty, result.Rows[0]["EMAIL"]);
			});
		}

		public void TestGetPhoneMobileFaxEmail_FromOrgAddress_IfThereIsNoContact()
		{
			var orgPK = Guid.NewGuid();
			var addressPK = Guid.NewGuid();
			var personPK = Guid.NewGuid();
			var testDataSql = $@"
INSERT INTO dbo.OrgHeader
(OH_PK, OH_Code, OH_FullName)
VALUES
('{orgPK}', 'QANAIR_WW', 'QANTAS AIRWAYS')

INSERT INTO dbo.OrgAddress
(OA_PK, OA_Address1, OA_Email, OA_Fax, OA_Mobile, OA_OH, OA_Phone)
VALUES
('{addressPK}', 'CNR QANTAS DRIVE & LINK ROAD', 'sales@qantas.com.au', '+61296912211', '13851888431', '{orgPK}', '+61296910000')

INSERT INTO dbo.OrgCountryData
(OV_PK, OV_OA_ApprovedLocation, OV_OH_OrgHeader, OV_EXApprovedOrMajorExporter)
VALUES
('{Guid.NewGuid()}', '{addressPK}', '{orgPK}', 'YES')";

			TestConnection.ExecuteNonQuery(testDataSql);

			var reportSql = $"select * from Report_ApprovedKnownOrganisation(null, '') where OrgPK = '{orgPK}'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			CombineAssertions(() =>
			{
				AssertEquals("Result should have rows", 1, result.Rows.Count);
				AssertEquals("Phone", "+61296910000", result.Rows[0]["PHONE"]);
				AssertEquals("Mobile", "13851888431", result.Rows[0]["MOBILE"]);
				AssertEquals("Fax", "+61296912211", result.Rows[0]["FAX"]);
				AssertEquals("Email", "sales@qantas.com.au", result.Rows[0]["EMAIL"]);
			});
		}

		public void TestLastReviwedDate()
		{
			Guid orgPK = Guid.NewGuid();

			string testDataSql = string.Format(@"
insert into dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) values ('{0}', 'TESTORG', 'Test Organisation')
insert into dbo.OrgCountryData(OV_PK, OV_OH_OrgHeader, OV_EXApprovedOrMajorExporter, OV_RN_NKClientCountryRelation, OV_EXSiteInspectionDate) values(NEWID(), '{0}', 'YES', 'NZ', '2011-01-11')
insert into dbo.OrgCountryData(OV_PK, OV_OH_OrgHeader, OV_EXApprovedOrMajorExporter, OV_RN_NKClientCountryRelation, OV_LastReviewedOn) values(NEWID(), '{0}', 'YES', 'AU', '2011-01-12')
insert into dbo.OrgCountryData(OV_PK, OV_OH_OrgHeader, OV_EXApprovedOrMajorExporter, OV_RN_NKClientCountryRelation) values(NEWID(), '{0}', 'YES', 'SG')",
				orgPK.ToString());
			TestConnection.ExecuteNonQuery(testDataSql);

			string reportSql = string.Format(@"select * from Report_ApprovedKnownOrganisation(null, '') where OrgPK = '{0}' order by APPROVAL_LASTREVIEWED", orgPK.ToString());
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			AssertEquals("Result should have rows", 3, result.Rows.Count);
			Assert(result.Rows[0]["APPROVAL_LASTREVIEWED"] is DBNull);
			AssertEquals(new DateTime(2011, 1, 11), result.Rows[1]["APPROVAL_LASTREVIEWED"]);
			AssertEquals(new DateTime(2011, 1, 12), result.Rows[2]["APPROVAL_LASTREVIEWED"]);
		}
	}
}


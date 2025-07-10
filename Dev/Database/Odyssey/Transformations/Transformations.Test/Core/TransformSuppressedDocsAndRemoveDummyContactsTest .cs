using System;
using System.Collections.Generic;
using System.Threading;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Core;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Core
{
	[TestedType(typeof(TransformSuppressedDocsAndRemoveDummyContacts))]
	class TransformSuppressedDocsAndRemoveDummyContactsTest : DataTransformationTestCase
	{
		Guid orgHeader1;
		Guid orgHeader2;
		Guid orgHeader3;

		Guid orgHeader1Contact1Doc1;
		Guid orgHeader1Contact1Doc2;
		Guid orgHeader1Contact2Doc3;
		Guid orgHeader2Contact1Doc1;
		Guid orgHeader2Contact1Doc2;
		Guid orgHeader2Contact2Doc3;
		Guid orgHeader3Contact1Doc1;
		Guid orgHeader3Contact2Doc1;

		Guid orgHeader1DummyContact;
		Guid orgHeader2DummyContact;

		Guid orgHeader11DummyContact;
		Guid orgHeader12DummyContact;
		Guid orgHeader13DummyContact;
		Guid orgHeader14DummyContact;
		Guid orgHeader15DummyContact;
		Guid orgHeader16DummyContact;

		readonly string DummyContactName = "DUMMY CONTACT TO SUPPRESS DOCS";
		readonly string DNDNotifyMode = "DND";

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				AssertEquals("After transformation: orgHeader1Contact1Doc1 should be suppressed document", true, DocumentShouldBeMarkedSuppressedAfterTransformation(orgHeader1, orgHeader1Contact1Doc1));
				AssertEquals("After transformation: orgHeader1Contact1Doc2 should be suppressed document", true, DocumentShouldBeMarkedSuppressedAfterTransformation(orgHeader1, orgHeader1Contact1Doc2));
				AssertEquals("After transformation: orgHeader1Contact2Doc3 should not be suppressed document and won't be affected", true, !DocumentShouldBeMarkedSuppressedAfterTransformation(orgHeader1, orgHeader1Contact2Doc3) && OrgDocumentIsNotUpdated(orgHeader1Contact2Doc3));
				AssertEquals("After transformation: orgHeader2Contact1Doc1 should be suppressed document", true, DocumentShouldBeMarkedSuppressedAfterTransformation(orgHeader2, orgHeader2Contact1Doc1));
				AssertEquals("After transformation: orgHeader2Contact1Doc2 should be suppressed document", true, DocumentShouldBeMarkedSuppressedAfterTransformation(orgHeader2, orgHeader2Contact1Doc2));
				AssertEquals("After transformation: orgHeader2Contact2Doc3 should not be suppressed document and won't be affected", true, !DocumentShouldBeMarkedSuppressedAfterTransformation(orgHeader2, orgHeader2Contact2Doc3) && OrgDocumentIsNotUpdated(orgHeader2Contact2Doc3));
				AssertEquals("After transformation: orgHeader3Contact1Doc1 should not be suppressed document and won't be affected", true, !DocumentShouldBeMarkedSuppressedAfterTransformation(orgHeader3, orgHeader3Contact1Doc1) && OrgDocumentIsNotUpdated(orgHeader3Contact1Doc1));
				AssertEquals("After transformation: orgHeader3Contact2Doc1 should not be suppressed document and won't be affected", true, !DocumentShouldBeMarkedSuppressedAfterTransformation(orgHeader3, orgHeader3Contact2Doc1) && OrgDocumentIsNotUpdated(orgHeader3Contact2Doc1));
			});

			CombineAssertions(() =>
			{
				AssertEquals(
					"orgHeader1DummyContact & orgHeader5DummyContact should not be removed as it's still referenced",
					2,
					TestConnection.ExecuteScalar<int>(
						$"SELECT COUNT(*) FROM dbo.OrgContact WHERE OC_PK in ('{orgHeader11DummyContact}', '{orgHeader15DummyContact}') AND OC_IsActive = 0 AND OC_SystemLastEditUser = '~BP'"));

				AssertEquals(
					"orgHeader3DummyContact should not be removed as it's still useful(maybe it's manually activated by clients)",
					1,
					TestConnection.ExecuteScalar<int>(
						$"SELECT COUNT(*) FROM dbo.OrgContact WHERE OC_PK = '{orgHeader13DummyContact}' AND OC_IsActive = 1 AND OC_SystemLastEditUser = '{TestDbHelper.UserStaffCode}'"));

				AssertEquals(
					"orgHeader2DummyContact & orgHeader4DummyContact & orgHeader6DummyContact should be removed as they are not referenced",
					0,
					TestConnection.ExecuteScalar<int>(
						$"SELECT COUNT(*) FROM dbo.OrgContact WHERE OC_PK IN ('{orgHeader12DummyContact}', '{orgHeader14DummyContact}', '{orgHeader16DummyContact}')"));
			});
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new TransformSuppressedDocsAndRemoveDummyContactsForTest();
		}

		protected override void PrepareTestData()
		{
			var originalDummyContactCount = GetDummyContactCount();

			var helper = new TestDbHelper(TestConnection);
			orgHeader1 = helper.InsertOrgHeader("OH1", "OH1Name");
			orgHeader2 = helper.InsertOrgHeader("OH2", "OH2Name");
			orgHeader3 = helper.InsertOrgHeader("OH3", "OH3Name");

			orgHeader1DummyContact = helper.InsertOrgContact(DummyContactName, "test@wtg.com", false, orgHeader1, DNDNotifyMode);
			var orgHeader1Contact2 = helper.InsertOrgContact("Sales", "test@wtg.com", false, orgHeader1, "EML", "PDF");
			orgHeader2DummyContact = helper.InsertOrgContact(DummyContactName, "test@wtg.com", false, orgHeader2, DNDNotifyMode);
			var orgHeader2Contact2 = helper.InsertOrgContact("Account", "test@wtg.com", false, orgHeader2, "EML", "PDF");
			var orgHeader3Contact1 = helper.InsertOrgContact("Product", "test@wtg.com", false, orgHeader3, "EML", "PDF");
			var orgHeader3Contact2 = helper.InsertOrgContact("Develop", "test@wtg.com", false, orgHeader3, "EML", "PDF");

			var stmMenuItem1 = helper.InsertStmMenuItem("TestMenu1", "LCLShipment", "DOC");
			var stmMenuItem2 = helper.InsertStmMenuItem("TestMenu2", "LCLShipment", "DOC");
			var stmMenuItem3 = helper.InsertStmMenuItem("TestMenu3", "LCLShipment", "DOC");
			var stmMenuItem4 = helper.InsertStmMenuItem("TestMenu4", "LCLShipment", "DOC");

			orgHeader1Contact1Doc1 = helper.InsertOrgDocument("AAA", null, orgHeader1DummyContact, DNDNotifyMode);
			orgHeader1Contact1Doc2 = helper.InsertOrgDocument("", stmMenuItem1, orgHeader1DummyContact, DNDNotifyMode);
			orgHeader1Contact2Doc3 = helper.InsertOrgDocument("BBB", null, orgHeader1Contact2, "EML", "PDF");
			orgHeader2Contact1Doc1 = helper.InsertOrgDocument("", stmMenuItem2, orgHeader2DummyContact, DNDNotifyMode);
			orgHeader2Contact1Doc2 = helper.InsertOrgDocument("CCC", null, orgHeader2DummyContact, DNDNotifyMode);
			orgHeader2Contact2Doc3 = helper.InsertOrgDocument("", stmMenuItem3, orgHeader2Contact2, "EML", "PDF");
			orgHeader3Contact1Doc1 = helper.InsertOrgDocument("DDD", null, orgHeader3Contact1, "EML", "PDF");
			orgHeader3Contact2Doc1 = helper.InsertOrgDocument("", stmMenuItem4, orgHeader3Contact2, "EML", "PDF");

			CombineAssertions(() =>
			{
				AssertEquals("Pre-condition: orgHeader1Contact1Doc1 should be suppressed document", true, DocumentShouldBeMarkedSuppressedBeforeTransformation(orgHeader1, orgHeader1Contact1Doc1));
				AssertEquals("Pre-condition: orgHeader1Contact1Doc2 should be suppressed document", true, DocumentShouldBeMarkedSuppressedBeforeTransformation(orgHeader1, orgHeader1Contact1Doc2));
				AssertEquals("Pre-condition: orgHeader1Contact2Doc3 should not be suppressed document", false, DocumentShouldBeMarkedSuppressedBeforeTransformation(orgHeader1, orgHeader1Contact2Doc3));
				AssertEquals("Pre-condition: orgHeader2Contact1Doc1 should be suppressed document", true, DocumentShouldBeMarkedSuppressedBeforeTransformation(orgHeader2, orgHeader2Contact1Doc1));
				AssertEquals("Pre-condition: orgHeader2Contact1Doc2 should be suppressed document", true, DocumentShouldBeMarkedSuppressedBeforeTransformation(orgHeader2, orgHeader2Contact1Doc2));
				AssertEquals("Pre-condition: orgHeader2Contact2Doc3 should not be suppressed document", false, DocumentShouldBeMarkedSuppressedBeforeTransformation(orgHeader2, orgHeader2Contact2Doc3));
				AssertEquals("Pre-condition: orgHeader3Contact1Doc1 should not be suppressed document", false, DocumentShouldBeMarkedSuppressedBeforeTransformation(orgHeader3, orgHeader3Contact1Doc1));
				AssertEquals("Pre-condition: orgHeader3Contact2Doc1 should not be suppressed document", false, DocumentShouldBeMarkedSuppressedBeforeTransformation(orgHeader3, orgHeader3Contact2Doc1));
				AssertEquals("Pre-condition: newly added dummy contacts for test should be 2", 2, GetDummyContactCount() - originalDummyContactCount);
			});

			var dummyOrgHeader1 = helper.InsertOrgHeader("DOH1", "DOH1Name");
			var dummyOrgHeader2 = helper.InsertOrgHeader("DOH2", "DOH2Name");
			var dummyOrgHeader3 = helper.InsertOrgHeader("DOH3", "DOH3Name");
			var dummyOrgHeader4 = helper.InsertOrgHeader("DOH4", "DOH4Name");
			var dummyOrgHeader5 = helper.InsertOrgHeader("DOH5", "DOH5Name");
			var dummyOrgHeader6 = helper.InsertOrgHeader("DOH6", "DOH6Name");

			orgHeader11DummyContact = helper.InsertOrgContact(DummyContactName, "test1@wtg.com", false, dummyOrgHeader1, DNDNotifyMode, isActive: false, lastEditUser: "~BP");
			orgHeader12DummyContact = helper.InsertOrgContact(DummyContactName, "test2@wtg.com", false, dummyOrgHeader2, DNDNotifyMode, isActive: false, lastEditUser: "~BP");
			orgHeader13DummyContact = helper.InsertOrgContact(DummyContactName, "test3@wtg.com", false, dummyOrgHeader3, "PRN", isActive: true, lastEditUser: TestDbHelper.UserStaffCode);
			orgHeader14DummyContact = helper.InsertOrgContact(DummyContactName, "test4@wtg.com", false, dummyOrgHeader4, DNDNotifyMode, isActive: false, lastEditUser: "~BP");
			orgHeader15DummyContact = helper.InsertOrgContact(DummyContactName, "test5@wtg.com", false, dummyOrgHeader5, DNDNotifyMode, isActive: false, lastEditUser: "~BP");
			orgHeader16DummyContact = helper.InsertOrgContact(DummyContactName, "test6@wtg.com", false, dummyOrgHeader6, DNDNotifyMode, isActive: false, lastEditUser: "~BP");

			helper.InsertOrgOpportunity(dummyOrgHeader1, TestDbHelper.DefaultCompanyPK, orgHeader11DummyContact, "Test1");
			helper.InsertOrgOpportunity(dummyOrgHeader5, TestDbHelper.DefaultCompanyPK, orgHeader15DummyContact, "Test5");
		}

		bool DocumentShouldBeMarkedSuppressedBeforeTransformation(Guid orgHeaderPK, Guid ordDocumentPK)
		{
			var sqlText = $@"
SELECT 
	COUNT(*)
FROM 
	dbo.OrgDocument 
	INNER JOIN dbo.OrgContact ON OD_OC = OC_PK
WHERE 
	OC_ContactName = '{DummyContactName}' AND OC_NotifyMode = '{DNDNotifyMode}' AND OD_PK = '{ordDocumentPK}' AND OC_OH = '{orgHeaderPK}'
";
			return TestConnection.ExecuteScalar<int>(sqlText) == 1;
		}

		bool DocumentShouldBeMarkedSuppressedAfterTransformation(Guid orgHeaderPK, Guid orgDocumentPK)
		{
			var sqlText = $"SELECT COUNT(*) FROM dbo.OrgDocument WHERE OD_OC IS NULL AND OD_OH_Suppressed = '{orgHeaderPK}' AND OD_PK = '{orgDocumentPK}'";
			return TestConnection.ExecuteScalar<int>(sqlText) == 1;
		}

		int GetDummyContactCount()
		{
			var sqlText = $"SELECT COUNT(*) FROM dbo.OrgContact WHERE OC_ContactName = '{DummyContactName}' AND OC_NotifyMode = '{DNDNotifyMode}'";
			return TestConnection.ExecuteScalar<int>(sqlText);
		}

		bool OrgDocumentIsNotUpdated(Guid orgDocumentPK)
		{
			var sqlText = $"SELECT OD_AutoVersion FROM dbo.OrgDocument WHERE OD_PK ='{orgDocumentPK}'";
			return TestConnection.ExecuteScalar<short>(sqlText) == 0;
		}

		public void TestLogging()
		{
			PrepareTestData();
			var rowsCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.OrgContact");
			var logger = new List<string>();
			var transformation = (IOnlineTransformation)GetNewTestTransformationInstance();
			var batchSize = (transformation as TransformSuppressedDocsAndRemoveDummyContactsForTest).BatchSize;
			var chunksCount = Math.Ceiling(1.0 * rowsCount / batchSize);
			transformation.Run(s => logger.Add(s), CancellationToken.None);

			var expectedLogs = new List<string>();
			var processedCount = 0;
			for (var i = 0; i < chunksCount; i++)
			{
				processedCount = Math.Min(processedCount + batchSize, rowsCount);
				expectedLogs.Add($"Processed {processedCount} of {rowsCount} record(s).");
			}
			expectedLogs.Add("\tCompleted: Transform Suppressed Documents and Remove Dummy Contacts");
			AssertContainsExactElementsInExactOrder(expectedLogs, logger);
		}

		class TransformSuppressedDocsAndRemoveDummyContactsForTest : TransformSuppressedDocsAndRemoveDummyContacts
		{
			internal override int BatchSize => 2;
		}
	}
}

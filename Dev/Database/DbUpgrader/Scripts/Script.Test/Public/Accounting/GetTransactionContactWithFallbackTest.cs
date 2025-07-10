using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(GetTransactionContactWithFallback))]
	class GetTransactionContactWithFallbackTest : DbCreateScriptTest
	{
		public void TestGetTransactionContact()
		{
			var insertSql = @"
			DECLARE @CompanyPk UNIQUEIDENTIFIER = '300261E4-A3C9-43F7-92CC-A183DEAA2F56';
			DECLARE @BranchPk UNIQUEIDENTIFIER = 'ACA88F60-EC09-44A8-A8C2-4F8BD4BC9847';
			DECLARE @OrgPK UNIQUEIDENTIFIER = '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392';
			DECLARE @Contact1 UNIQUEIDENTIFIER = '7BC96E02-B034-4846-9662-C6B2FD426E93';
			DECLARE @Contact2 UNIQUEIDENTIFIER = 'F3CD76B2-BEA9-4DB9-B903-C983A8130212';
			DECLARE @Contact4 UNIQUEIDENTIFIER = '2AE62B73-B3FC-4E6F-A1FA-8DF05ABC54FE';
			DECLARE @ContactOverride UNIQUEIDENTIFIER = '9ECC5884-C0CB-4F72-A7AE-987FAF89E138';

			DECLARE @OrgPK1 UNIQUEIDENTIFIER = 'B8E7C3B4-6C7D-49B2-A835-60A46F245196';

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPK, 'TESTORG1');

			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Email, OC_Phone, OC_Fax, OC_NotifyMode) VALUES (@Contact1, @OrgPK, 'AR contact', 'ar_contactem@il', 'AR3243', 'ARFAX1', '')
			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Email, OC_Phone, OC_Fax, OC_NotifyMode) VALUES (@Contact2, @OrgPK, 'AP contact', 'ap_contactem@il', 'AP3243', 'APFAX1', '')
			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Email, OC_Phone, OC_Fax, OC_NotifyMode) VALUES (@ContactOverride, @OrgPK, 'Overridden contact', 'Override_contactem@il', 'OV3243', 'OVFAX1', '')
			
			INSERT INTO [dbo].[OrgDocument]([OD_PK],[OD_DocumentGroup],[OD_OC],[OD_DefaultContact]) VALUES(newid(),'A/R',@Contact1,1)
			INSERT INTO [dbo].[OrgDocument]([OD_PK],[OD_DocumentGroup],[OD_OC],[OD_DefaultContact]) VALUES(newid(),'ALL',@Contact2,1)

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPK1, 'TESTORG2');

			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Email, OC_Phone, OC_Fax, OC_NotifyMode) VALUES (@Contact4, @OrgPK1, 'Zoo Keeper', 'zoolandia@il', 'ZOO3243', 'TorongaFAX1', '')";

			TestConnection.ExecuteNonQuery(insertSql);

			var sql = "SELECT * FROM GetTransactionContactWithFallback('9ECC5884-C0CB-4F72-A7AE-987FAF89E138', 'AR', '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392')";
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("Overridden contact", result.Rows[0]["OC_ContactName"]);
			AssertEquals("Override_contactem@il", result.Rows[0]["OC_Email"]);
			AssertEquals("OV3243", result.Rows[0]["OC_Phone"]);
			AssertEquals("OVFAX1", result.Rows[0]["OC_Fax"]);

			sql = "SELECT * FROM GetTransactionContactWithFallback('9ECC5884-C0CB-4F72-A7AE-987FAF89E138', 'AP', '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392')"; //If override contact is provided then it does not matter if ledger is provided or not
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("Overridden contact", result.Rows[0]["OC_ContactName"]);
			AssertEquals("Override_contactem@il", result.Rows[0]["OC_Email"]);
			AssertEquals("OV3243", result.Rows[0]["OC_Phone"]);
			AssertEquals("OVFAX1", result.Rows[0]["OC_Fax"]);

			sql = "SELECT * FROM GetTransactionContactWithFallback(NULL, 'AR', '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("AR contact", result.Rows[0]["OC_ContactName"]);
			AssertEquals("ar_contactem@il", result.Rows[0]["OC_Email"]);
			AssertEquals("AR3243", result.Rows[0]["OC_Phone"]);
			AssertEquals("ARFAX1", result.Rows[0]["OC_Fax"]);

			sql = "SELECT * FROM GetTransactionContactWithFallback(NULL, 'AP', '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392')"; //Since there is no contact setup to receive AP documents, it will fallback to contact to receive ALL documents.
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("AP contact", result.Rows[0]["OC_ContactName"]);
			AssertEquals("ap_contactem@il", result.Rows[0]["OC_Email"]);
			AssertEquals("AP3243", result.Rows[0]["OC_Phone"]);
			AssertEquals("APFAX1", result.Rows[0]["OC_Fax"]);

			sql = "SELECT * FROM GetTransactionContactWithFallback(NULL, 'AR', 'B8E7C3B4-6C7D-49B2-A835-60A46F245196')"; //The org has one contact that too the Dummy contact which is first in alphabetical order; we should skip this dummy contact in our result
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("Zoo Keeper", result.Rows[0]["OC_ContactName"]);
			AssertEquals("zoolandia@il", result.Rows[0]["OC_Email"]);
			AssertEquals("ZOO3243", result.Rows[0]["OC_Phone"]);
			AssertEquals("TorongaFAX1", result.Rows[0]["OC_Fax"]);
		}

		public void TestTransactionContactSequence()
		{
			var insertSql = @"
			DECLARE @CompanyPk UNIQUEIDENTIFIER = '300261E4-A3C9-43F7-92CC-A183DEAA2F56';
			DECLARE @BranchPk UNIQUEIDENTIFIER = 'ACA88F60-EC09-44A8-A8C2-4F8BD4BC9847';

			DECLARE @OrgPK1 UNIQUEIDENTIFIER = '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392';

			DECLARE @Contact1 UNIQUEIDENTIFIER = '7BC96E02-B034-4846-9662-C6B2FD426E93';
			DECLARE @Contact2 UNIQUEIDENTIFIER = 'F3CD76B2-BEA9-4DB9-B903-C983A8130212';
			DECLARE @Contact3 UNIQUEIDENTIFIER = '928D9B6F-DAAD-4E9C-9C68-93F29EFDC4EF';

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPK1, 'TESTORG1');

			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Email, OC_Phone, OC_Fax, OC_NotifyMode) VALUES (@Contact1, @OrgPK1, 'AR contact', 'ar_contact@email', 'ARPH', 'ARFA', '')
			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Email, OC_Phone, OC_Fax, OC_NotifyMode) VALUES (@Contact2, @OrgPK1, 'AP contact', 'ap_contact@email', 'APPH', 'APFAX', '')
			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_ContactName, OC_Email, OC_Phone, OC_Fax, OC_NotifyMode) VALUES (@Contact3, @OrgPK1, 'ALL contact', 'all_contact@email', 'ALLPH', 'ALLFAX', '')
			
			INSERT INTO dbo.OrgDocument(OD_PK, OD_DocumentGroup, OD_OC, OD_DefaultContact) VALUES (newid(), 'A/R', @Contact1, 1)
			INSERT INTO dbo.OrgDocument(OD_PK, OD_DocumentGroup, OD_OC, OD_DefaultContact) VALUES (newid(), 'A/P', @Contact2, 1)
			INSERT INTO dbo.OrgDocument(OD_PK, OD_DocumentGroup, OD_OC, OD_DefaultContact) VALUES (newid(), 'ALL', @Contact3, 1)";

			TestConnection.ExecuteNonQuery(insertSql);

			var sql = @"
			SELECT OC_ContactName FROM dbo.OrgContact LEFT JOIN dbo.OrgDocument ON OrgDocument.OD_OC = OrgContact.OC_PK 
			WHERE OC_OH = '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392'
				AND OC_IsActive = 1
				AND UPPER(OD_DocumentGroup) IN('A/R', 'ALL')
			ORDER BY OC_ContactName";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals("Pre-condition: 2 candidate contacts", 2, result.Rows.Count);
			AssertEquals("ALL contact", result.Rows[0]["OC_ContactName"]);
			AssertEquals("AR contact", result.Rows[1]["OC_ContactName"]);

			sql = "SELECT * FROM GetTransactionContactWithFallback(NULL, 'AR', '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals("Test 2nd and 4th union-all SQL statements", 1, result.Rows.Count);
			AssertEquals("'AR contact' is the result from 2nd union-all SQL statement. It has higher sequence priority than 'ALL contact'", "AR contact", result.Rows[0]["OC_ContactName"]);

			sql = @"
			SELECT OC_ContactName FROM dbo.OrgContact LEFT JOIN dbo.OrgDocument ON OrgDocument.OD_OC = OrgContact.OC_PK 
			WHERE OC_OH = '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392'
				AND OC_IsActive = 1
				AND UPPER(OD_DocumentGroup) IN('A/P', 'ALL')
			ORDER BY OC_ContactName";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals("Pre-condition: 2 candidate contacts", 2, result.Rows.Count);
			AssertEquals("ALL contact", result.Rows[0]["OC_ContactName"]);
			AssertEquals("AP contact", result.Rows[1]["OC_ContactName"]);

			sql = "SELECT * FROM GetTransactionContactWithFallback(NULL, 'AP', '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392')";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals("Test 3rd and 4th union-all SQL statements", 1, result.Rows.Count);
			AssertEquals("'AP contact' is the result from 3rd union-all SQL statement. It has higher sequence priority than 'ALL contact'", "AP contact", result.Rows[0]["OC_ContactName"]);
		}
	}
}


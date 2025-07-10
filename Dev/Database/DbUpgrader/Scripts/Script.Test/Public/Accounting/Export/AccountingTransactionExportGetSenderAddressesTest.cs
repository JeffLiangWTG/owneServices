using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Export;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Export.Testing
{
	[TestedType(typeof(AccountingTransactionExportGetSenderAddresses))]
	class AccountingTransactionExportGetSenderAddressesTest : DbCreateScriptTest
	{
		public void TestGetOrgProxyAddress()
		{
			var insertSql = @"
			DECLARE @CompanyPk UNIQUEIDENTIFIER = '300261E4-A3C9-43F7-92CC-A183DEAA2F56';
			DECLARE @BranchPk UNIQUEIDENTIFIER = 'ACA88F60-EC09-44A8-A8C2-4F8BD4BC9847';
			DECLARE @BranchPk1 UNIQUEIDENTIFIER = '7DF8E10B-C28A-4E3B-9FF7-20CB7E1912B0';
			DECLARE @BranchOrgProxyPk UNIQUEIDENTIFIER = '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392';
			DECLARE @CompanyOrgProxyPk UNIQUEIDENTIFIER = '8D659375-3B9F-4481-86D5-2DAC8ACA2F7B';
			DECLARE @BranchOrgProxyAddressPK UNIQUEIDENTIFIER = '0D1DA9C0-32F5-4BFE-9F5E-340E9D17BB48';
			DECLARE @CompanyOrgProxyAddressPK UNIQUEIDENTIFIER = 'CC548369-1D4E-4479-9866-21494C90D624';
			DECLARE @BranchOrgProxyContactPK UNIQUEIDENTIFIER = 'F9C98A32-CF0B-414E-93F6-212B3ECD2E96';
			DECLARE @CompanyOrgProxyContactPK UNIQUEIDENTIFIER = '479BD056-B9F6-4923-8A94-04676AC160CA';

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@BranchOrgProxyPk, 'TESTORG1')
			INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_State, OA_Email, OA_Phone, OA_Fax) VALUES (@BranchOrgProxyAddressPK, @BranchOrgProxyPk, 'Branch Org Proxy Address 1', 'Branch Address 2', 'City', 'XXXX', 'State', 'e@mail', '123456', 'FAX')
			INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (newid(), @BranchOrgProxyAddressPK, 'OFC', 1)
			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_OA_OrgAddress, OC_ContactName, OC_Email, OC_Phone, OC_Fax) VALUES (@BranchOrgProxyContactPK, @BranchOrgProxyPk, @BranchOrgProxyAddressPK, 'Branch Org Proxy Contact', 'contactem@il', '3243', 'FAX1')
			INSERT INTO dbo.OrgDocument([OD_PK],[OD_DocumentGroup],[OD_OC],[OD_DefaultContact]) VALUES(newid(),'A/R',@BranchOrgProxyContactPK,1)

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@CompanyOrgProxyPk, 'TESTORG2')
			INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES (@CompanyOrgProxyAddressPK, @CompanyOrgProxyPk, 'Company Org Proxy Address 1')
			INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (newid(), @CompanyOrgProxyAddressPK, 'OFC', 1)
			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_OA_OrgAddress, OC_ContactName) VALUES (@CompanyOrgProxyContactPK, @CompanyOrgProxyPk, @CompanyOrgProxyAddressPK, 'Company Org Proxy Contact')
			INSERT INTO dbo.OrgDocument([OD_PK],[OD_DocumentGroup],[OD_OC],[OD_DefaultContact]) VALUES(newid(),'A/P',@CompanyOrgProxyContactPK,1)

			INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_OH_OrgProxy) VALUES (@CompanyPk, 'CAU', 'AU company', 'AU','AUD', @CompanyOrgProxyPk)
			INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_OH_OrgProxy) VALUES (@BranchPk, @CompanyPk, 'TB1', @BranchOrgProxyPk)
			INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_OH_OrgProxy) VALUES (@BranchPk1, @CompanyPk, 'TB2', null)

			DECLARE @DepartmentPk UNIQUEIDENTIFIER = 'DD3E23A1-7C44-4DAE-8F38-ACE05FDBA455';
			INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)
			
			DECLARE @ARInvoicePk UNIQUEIDENTIFIER = '815D206E-5A42-4886-B319-FC6D0AA8B322';
			DECLARE @APInvoicePk UNIQUEIDENTIFIER = 'D0EF24CF-5214-4717-B1B8-F6600B9BB205';

			INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate) 
			VALUES 
				(@ARInvoicePk, 'AR', 'INV', @CompanyPk, @BranchPk, @DepartmentPk, '2017-2-2', '2017-2-2'),
				(@APInvoicePk, 'AP', 'INV', @CompanyPk, @BranchPk1, @DepartmentPk, '2017-1-1', '2017-1-1')
			";

			TestConnection.ExecuteNonQuery(insertSql);

			var sql = "EXEC AccountingTransactionExportGetSenderAddresses 'CAU', 0, '815D206E-5A42-4886-B319-FC6D0AA8B322'"; //AR invoice, AH_GB has a branch org proxy

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("Branch Org Proxy Address 1", result.Rows[0]["OA_Address1"]);
			AssertEquals("Branch Address 2", result.Rows[0]["OA_Address2"]);
			AssertEquals("City", result.Rows[0]["OA_City"]);
			AssertEquals("XXXX", result.Rows[0]["OA_PostCode"]);
			AssertEquals("State", result.Rows[0]["OA_State"]);
			AssertEquals("e@mail", result.Rows[0]["OA_Email"]);
			AssertEquals("123456", result.Rows[0]["OA_Phone"]);
			AssertEquals("FAX", result.Rows[0]["OA_Fax"]);
			AssertEquals("Branch Org Proxy Contact", result.Rows[0]["OC_ContactName"]);
			AssertEquals("contactem@il", result.Rows[0]["OC_Email"]);
			AssertEquals("3243", result.Rows[0]["OC_Phone"]);
			AssertEquals("FAX1", result.Rows[0]["OC_Fax"]);
			AssertEquals("TESTORG1", result.Rows[0]["OH_Code"]);

			sql = "EXEC AccountingTransactionExportGetSenderAddresses 'CAU', 0, 'D0EF24CF-5214-4717-B1B8-F6600B9BB205'"; //AP invoice, AH_GB does not have a branch org proxy

			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("Company Org Proxy Address 1", result.Rows[0]["OA_Address1"]);
			AssertEquals("Company Org Proxy Contact", result.Rows[0]["OC_ContactName"]);
			AssertEquals("TESTORG2", result.Rows[0]["OH_Code"]);
		}

		public void TestColumnsReturnedAreSameInBothAddressGetterFunctions()
		{
			var sql = "EXEC AccountingTransactionExportGetSenderAddresses 'XXX', 0, 'ffffffff-ffff-ffff-ffff-ffffffffffff'";

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			sql = "EXEC AccountingTransactionExportGetAddresses 'XXX', 0, 'ffffffff-ffff-ffff-ffff-ffffffffffff'";

			DataTable result1 = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			foreach (DataColumn column in result.Columns)
			{
				Assert($"{column.ColumnName} is not returned by AccountingTransactionExportGetAddresses", result1.Columns.Contains(column.ColumnName));
			}

			foreach (DataColumn column in result1.Columns)
			{
				Assert($"{column.ColumnName} is not returned by AccountingTransactionExportGetSenderAddresses", result.Columns.Contains(column.ColumnName));
			}
		}
	}
}


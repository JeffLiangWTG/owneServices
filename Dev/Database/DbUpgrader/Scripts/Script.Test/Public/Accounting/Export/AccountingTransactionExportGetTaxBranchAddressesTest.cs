using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Export;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Export.Testing
{
	[TestedType(typeof(AccountingTransactionExportGetTaxBranchAddresses))]
	class AccountingTransactionExportGetTaxBranchAddressesTest : DbCreateScriptTest
	{
		public void TestGetOrgProxyAddress()
		{
			var insertSql = @"
			DECLARE @Company1Pk UNIQUEIDENTIFIER = '300261E4-A3C9-43F7-92CC-A183DEAA2F56';
			DECLARE @Company2Pk UNIQUEIDENTIFIER = 'DE017F49-2668-4044-83B1-E80D1B099C77';
			DECLARE @Branch1Pk UNIQUEIDENTIFIER = 'ACA88F60-EC09-44A8-A8C2-4F8BD4BC9847';
			DECLARE @Branch1NoProxyPk UNIQUEIDENTIFIER = '7DF8E10B-C28A-4E3B-9FF7-20CB7E1912B0';
			DECLARE @Branch2Pk UNIQUEIDENTIFIER = '4840B222-FEA1-4461-9458-0D7B011E4B20';
			DECLARE @Branch2NoProxyPk UNIQUEIDENTIFIER = '37952254-1B02-4DC0-9A88-2CD8533CF377';
			DECLARE @TaxBranchPk UNIQUEIDENTIFIER = '28319E8D-8E93-4EBC-A71E-5EFBABA97381';
			DECLARE @TaxBranchNoProxyPk UNIQUEIDENTIFIER = '10BD6A03-C1E5-4FE8-B053-507C5E89E0B5';
			DECLARE @Branch1OrgProxyPk UNIQUEIDENTIFIER = '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392';
			DECLARE @Branch2OrgProxyPk UNIQUEIDENTIFIER = '954EF72F-A380-4735-B84B-6307E5C97987';
			DECLARE @Company1OrgProxyPk UNIQUEIDENTIFIER = '8D659375-3B9F-4481-86D5-2DAC8ACA2F7B';
			DECLARE @Company2OrgProxyPk UNIQUEIDENTIFIER = '5F157B40-3A65-4B19-9002-0AF2552F1324';
			DECLARE @TaxBranchOrgProxyPk UNIQUEIDENTIFIER = '718DD70F-A9B2-4143-B88F-C2F813091416';
			DECLARE @Branch1OrgProxyAddressPK UNIQUEIDENTIFIER = '0D1DA9C0-32F5-4BFE-9F5E-340E9D17BB48';
			DECLARE @Branch2OrgProxyAddressPK UNIQUEIDENTIFIER = '28D509B6-C632-4438-8B5B-DCB335867536';
			DECLARE @Company1OrgProxyAddressPK UNIQUEIDENTIFIER = 'CC548369-1D4E-4479-9866-21494C90D624';
			DECLARE @Company2OrgProxyAddressPK UNIQUEIDENTIFIER = 'A4229450-AA73-4EF3-973B-076ABE905F95';
			DECLARE @TaxBranchOrgProxyAddressPK UNIQUEIDENTIFIER = '5FF1525B-B672-4BB1-85FB-A0FB168802EE';
			DECLARE @Branch1OrgProxyContactPK UNIQUEIDENTIFIER = 'F9C98A32-CF0B-414E-93F6-212B3ECD2E96';
			DECLARE @Branch2OrgProxyContactPK UNIQUEIDENTIFIER = 'C93F05E7-E5A5-4716-8A63-740F52998BD7';
			DECLARE @Company1OrgProxyContactPK UNIQUEIDENTIFIER = '479BD056-B9F6-4923-8A94-04676AC160CA';
			DECLARE @Company2OrgProxyContactPK UNIQUEIDENTIFIER = '064C9A89-C7C4-45B9-BD5A-FE2AB38890E6';
			DECLARE @TaxBranchOrgProxyContactPK UNIQUEIDENTIFIER = '1D8D90F0-5EB1-4075-8CDA-175F5CB97697';

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@Branch1OrgProxyPk, 'TESTORG1')
			INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_State, OA_Email, OA_Phone, OA_Fax) VALUES (@Branch1OrgProxyAddressPK, @Branch1OrgProxyPk, 'Branch 1 Org Proxy Address 1', 'Branch 1 Address 2', 'City1', 'XXX1', 'State1', 'e1@mail', '123456', 'FAXB1')
			INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (newid(), @Branch1OrgProxyAddressPK, 'OFC', 1)
			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_OA_OrgAddress, OC_ContactName, OC_Email, OC_Phone, OC_Fax) VALUES (@Branch1OrgProxyContactPK, @Branch1OrgProxyPk, @Branch1OrgProxyAddressPK, 'Branch 2 Org Proxy Contact', 'contactem1@il', '3243', 'FAX1')
			INSERT INTO dbo.OrgDocument([OD_PK],[OD_DocumentGroup],[OD_OC],[OD_DefaultContact]) VALUES(newid(),'A/R',@Branch1OrgProxyContactPK,1)

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@Branch2OrgProxyPk, 'TESTORG2')
			INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_State, OA_Email, OA_Phone, OA_Fax) VALUES (@Branch2OrgProxyAddressPK, @Branch2OrgProxyPk, 'Branch 2 Org Proxy Address 1', 'Branch 2 Address 2', 'City2', 'XXX2', 'State2', 'e2@mail', '555555', 'FAXB2')
			INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (newid(), @Branch2OrgProxyAddressPK, 'OFC', 1)
			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_OA_OrgAddress, OC_ContactName, OC_Email, OC_Phone, OC_Fax) VALUES (@Branch2OrgProxyContactPK, @Branch2OrgProxyPk, @Branch2OrgProxyAddressPK, 'Branch 2 Org Proxy Contact', 'contactem2@il', '2434', 'FAX2')
			INSERT INTO dbo.OrgDocument([OD_PK],[OD_DocumentGroup],[OD_OC],[OD_DefaultContact]) VALUES(newid(),'A/R',@Branch2OrgProxyContactPK,1)

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@Company1OrgProxyPk, 'TESTORG3')
			INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES (@Company1OrgProxyAddressPK, @Company1OrgProxyPk, 'Company 1 Org Proxy Address 1')
			INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (newid(), @Company1OrgProxyAddressPK, 'OFC', 1)
			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_OA_OrgAddress, OC_ContactName) VALUES (@Company1OrgProxyContactPK, @Company1OrgProxyPk, @Company1OrgProxyAddressPK, 'Company 1 Org Proxy Contact')
			INSERT INTO dbo.OrgDocument([OD_PK],[OD_DocumentGroup],[OD_OC],[OD_DefaultContact]) VALUES(newid(),'A/P',@Company1OrgProxyContactPK,1)

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@Company2OrgProxyPk, 'TESTORG4')
			INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES (@Company2OrgProxyAddressPK, @Company2OrgProxyPk, 'Company 2 Org Proxy Address 1')
			INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (newid(), @Company2OrgProxyAddressPK, 'OFC', 1)
			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_OA_OrgAddress, OC_ContactName) VALUES (@Company2OrgProxyContactPK, @Company2OrgProxyPk, @Company2OrgProxyAddressPK, 'Company 2 Org Proxy Contact')
			INSERT INTO dbo.OrgDocument([OD_PK],[OD_DocumentGroup],[OD_OC],[OD_DefaultContact]) VALUES(newid(),'A/P',@Company2OrgProxyContactPK,1)

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@TaxBranchOrgProxyPk, 'TESTORG5')
			INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_State, OA_Email, OA_Phone, OA_Fax) VALUES (@TaxBranchOrgProxyAddressPK, @TaxBranchOrgProxyPk, 'Tax Org Proxy Address 1', 'Tax Address 2', 'TaxCity', 'TXXX', 'TaxState', 'tax_e@mail', '654321', 'FAXT')
			INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (newid(), @TaxBranchOrgProxyAddressPK, 'OFC', 1)
			INSERT INTO dbo.OrgContact (OC_PK, OC_OH, OC_OA_OrgAddress, OC_ContactName, OC_Email, OC_Phone, OC_Fax) VALUES (@TaxBranchOrgProxyContactPK, @TaxBranchOrgProxyPk, @TaxBranchOrgProxyAddressPK, 'Tax Org Proxy Contact', 'tax@abc', '1234', 'FAXT1')
			INSERT INTO dbo.OrgDocument([OD_PK],[OD_DocumentGroup],[OD_OC],[OD_DefaultContact]) VALUES(newid(),'A/R',@TaxBranchOrgProxyContactPK,1)

			INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_OH_OrgProxy) VALUES (@Company1Pk, 'CAU', 'AU company1', 'AU', 'AUD', @Company1OrgProxyPk)
			INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_OH_OrgProxy) VALUES (@Company2Pk, 'CAI', 'AU company2', 'AU', 'AUD', @Company2OrgProxyPk)
			INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_OH_OrgProxy) VALUES (@Branch1Pk, @Company1Pk, 'TB1', @Branch1OrgProxyPk)
			INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_OH_OrgProxy) VALUES (@Branch2Pk, @Company2Pk, 'TB3', @Branch2OrgProxyPk)
			INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_OH_OrgProxy) VALUES (@TaxBranchNoProxyPk, @Company1Pk, 'TB6', null)
			INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_OH_OrgProxy) VALUES (@TaxBranchPk, @Company2Pk, 'TB5', @TaxBranchOrgProxyPk)

			DECLARE @DepartmentPk UNIQUEIDENTIFIER = 'DD3E23A1-7C44-4DAE-8F38-ACE05FDBA455';
			INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)
			
			DECLARE @APInvoice_NoTaxBranch_Pk UNIQUEIDENTIFIER = '43BCC595-5EDE-42E7-85E9-4C5A6F835AAA';
			DECLARE @ARInvoice_TaxBranchWithNoOrgProxy_Pk UNIQUEIDENTIFIER = 'A8AF364F-415C-44B6-B4DB-5D7E7FE3192E';
			DECLARE @ARInvoice_TaxBranch_Pk UNIQUEIDENTIFIER = '6DD14D8D-25A6-4DFC-B926-B58EDB1AEB6C';

			INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_GC, AH_GB, AH_GB_TaxBranch, AH_GE, AH_InvoiceDate, AH_PostDate) 
			VALUES 
				(@APInvoice_NoTaxBranch_Pk, 'AP', 'INV', @Company1Pk, @Branch1Pk, NULL, @DepartmentPk, '2017-2-2', '2017-2-2'),
				(@ARInvoice_TaxBranchWithNoOrgProxy_Pk, 'AR', 'INV', @Company1Pk, @Branch1Pk, @TaxBranchNoProxyPk, @DepartmentPk, '2017-2-2', '2017-2-2'),
				(@ARInvoice_TaxBranch_Pk, 'AR', 'INV', @Company2Pk, @Branch2Pk, @TaxBranchPk, @DepartmentPk, '2017-2-2', '2017-2-2')
			";

			TestConnection.ExecuteNonQuery(insertSql);

			var sql = "EXEC AccountingTransactionExportGetTaxBranchAddresses 'CAU', 0, '43BCC595-5EDE-42E7-85E9-4C5A6F835AAA'";
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should not have any rows", 0, result.Rows.Count);

			sql = "EXEC AccountingTransactionExportGetTaxBranchAddresses 'CAU', 0, 'A8AF364F-415C-44B6-B4DB-5D7E7FE3192E'";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("Company 1 Org Proxy Address 1", result.Rows[0]["OA_Address1"]);
			AssertEquals("Company 1 Org Proxy Contact", result.Rows[0]["OC_ContactName"]);
			AssertEquals("TESTORG3", result.Rows[0]["OH_Code"]);

			sql = "EXEC AccountingTransactionExportGetTaxBranchAddresses 'CAU', 0, '6DD14D8D-25A6-4DFC-B926-B58EDB1AEB6C'";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("Tax Org Proxy Address 1", result.Rows[0]["OA_Address1"]);
			AssertEquals("Tax Address 2", result.Rows[0]["OA_Address2"]);
			AssertEquals("TaxCity", result.Rows[0]["OA_City"]);
			AssertEquals("TXXX", result.Rows[0]["OA_PostCode"]);
			AssertEquals("TaxState", result.Rows[0]["OA_State"]);
			AssertEquals("tax_e@mail", result.Rows[0]["OA_Email"]);
			AssertEquals("654321", result.Rows[0]["OA_Phone"]);
			AssertEquals("FAXT", result.Rows[0]["OA_Fax"]);
			AssertEquals("Tax Org Proxy Contact", result.Rows[0]["OC_ContactName"]);
			AssertEquals("tax@abc", result.Rows[0]["OC_Email"]);
			AssertEquals("1234", result.Rows[0]["OC_Phone"]);
			AssertEquals("FAXT1", result.Rows[0]["OC_Fax"]);
			AssertEquals("TESTORG5", result.Rows[0]["OH_Code"]);
		}

		public void TestColumnsReturnedAreSameInBothAddressGetterFunctions()
		{
			var sql = "EXEC AccountingTransactionExportGetTaxBranchAddresses 'XXX', 0, 'ffffffff-ffff-ffff-ffff-ffffffffffff'";

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			sql = "EXEC AccountingTransactionExportGetAddresses 'XXX', 0, 'ffffffff-ffff-ffff-ffff-ffffffffffff'";

			DataTable result1 = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			foreach (DataColumn column in result.Columns)
			{
				Assert($"{column.ColumnName} is not returned by AccountingTransactionExportGetAddresses", result1.Columns.Contains(column.ColumnName));
			}

			foreach (DataColumn column in result1.Columns)
			{
				Assert($"{column.ColumnName} is not returned by AccountingTransactionExportGetTaxBranchAddresses", result.Columns.Contains(column.ColumnName));
			}
		}
	}
}

using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_TaxTransactionAnalysisbyTransLine))]
	class Report_TaxTransactionAnalysisbyTransLineTest : DbCreateScriptTest
	{
		public void TestSimpleRun()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '200605', '2006-05-01 00:00:00','2006-05-31 23:59:00','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_Desc, AH_InvoiceAmount, AH_InvoiceDate, AH_DueDate, AH_PostDate, AH_GB, AH_GB_TaxBranch, AH_GC, AH_GE, AH_TransactionCount, AH_OH, AH_ExchangeRate) 
				values('01FE98E0-E893-463C-BE75-01ACD2E72E0D', 'AP', 'INV', '00001001', 'Test_Desc', -10.00, '2006-05-25 15:44:00', '2006-05-25 15:44:00', '2006-05-25 15:44:00', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '4E5A97E8-85F4-41EC-95C6-5D14A676157C', 1, '4F1F6B5D-F65F-4B9F-A769-8C170A7A8642', 1)");

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccTransactionLines(AL_PK, AL_LineType, AL_LineAmount, AL_ExchangeRate, AL_OSAmount, AL_AH, AL_AG, AL_GC, AL_GB, AL_GB_TaxBranch, AL_GE, AL_PostDate, AL_PlaceOfSupplyType, AL_PlaceOfSupply)
									VALUES(NEWID(), 'CST', -10.00, 1, -10.00, '01FE98E0-E893-463C-BE75-01ACD2E72E0D', 'ac129d82-b88d-45ee-bce5-25592f734023', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '54226AD9-9E8A-4E29-A7F7-E73A7D18DDF1', '2B67864D-42E9-4A43-A9C8-09D2083C4227', '2006-05-25 15:44:00', 'STA', 'NSW')");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "select * from Report_TaxTransactionAnalysisbyTransLine('AU', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 200605, null, null, 'Both', 'ALL', null, null, 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', null, null, null, 'GST')");

			AssertEquals("Result should have one row", 1, result.Rows.Count);

			var row = result.Rows[0];

			AssertEquals("-", row.Field<string>("TaxType"));
			AssertEquals(0m, row.Field<decimal?>("TaxRate"));
			AssertEquals("-", row.Field<string>("ExtraTaxRateType"));
			AssertEquals(0m, row.Field<decimal?>("ExtraTaxRate"));
			AssertEquals(-10M, row.Field<decimal>("AH_TotalAmountIncludingTax"));
			AssertEquals(-10M, row.Field<decimal>("AH_InvoiceAmount"));
			AssertEquals(0M, row.Field<decimal>("AH_GSTAmount"));
			AssertNotNull(row.Field<DateTime>("AH_InvoiceDate"));
			AssertEquals("SYD", row.Field<string>("TransactionHeaderBranchCode"));
			AssertEquals("EDICUS      ", row.Field<string>("HeaderBranchOrgProxyCode"));
			AssertEquals("", row.Field<string>("HeaderBranchOrgProxyState"));
			AssertEquals(DBNull.Value, row["HeaderBranchTaxRegistration"]);
			AssertEquals("EDICUS      ", row.Field<string>("TransactionOrgCode"));
			AssertEquals("EDI CUSTOMS BROKERS", row.Field<string>("TransactionOrgFullName"));
			AssertEquals("BUS", row.Field<string>("TransactionOrgCategory"));
			AssertEquals("AU", row.Field<string>("OrgMainOfficeAddressCountryCode"));
			AssertEquals("", row.Field<string>("OrgMainOfficeAddressState"));
			AssertEquals("", row.Field<string>("AL_GovtChargeCode"));
			AssertEquals("SYD", row.Field<string>("AL_LineBranchCode"));
			AssertNull(row.Field<string>("ParentTransactionPostDate"));
			AssertNull(row.Field<string>("ParentTransactionInvoiceDate"));
			AssertEquals("New South Wales", row.Field<string>("PlaceOfSupplyDescription"));
			AssertEquals("BNE", row.Field<string>("TransactionHeaderTaxBranchCode"));
			AssertEquals("TES", row.Field<string>("AL_LineTaxBranchCode"));
			AssertEquals(1M, row.Field<decimal>("AH_ExchangeRate"));
			AssertEquals("01fe98e0-e893-463c-be75-01acd2e72e0d", row.Field<Guid>("AH_PK").ToString());
		}
	}
}


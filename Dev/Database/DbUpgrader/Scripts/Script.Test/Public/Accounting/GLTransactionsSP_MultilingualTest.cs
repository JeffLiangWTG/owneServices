using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(GLTransactionsSP_Multilingual))]
	class GLTransactionsSP_MultilingualTest : DbCreateScriptTest
	{
		public void TestTransactionDescPaymentReferenceNumberMaxLength()
		{
			GLTransactionsSPTest.SetUpDataForTestTransactionDescPaymentReferenceNumberMaxLength(TestConnection);

			var template = @"EXEC GLTransactionsSP_Multilingual
							@CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
							@StartPeriod = NULL,
							@EndPeriod = NULL,
							@StartDate = '2015-04-01',
							@EndDate = '2015-04-30',
							@StartGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
							@EndGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
							@BranchPK = NULL,
							@DepartmentPK = NULL,
							@DisplayDescription = 'L',
							@TransactionCategory = NULL,
							@BatchNumberToGet = NULL,
							@BatchNumberToSet = NULL,
							@IncludeZeroBalance = 'n',
							@IsExportingBatch = 'Y',
							@Language = 'ABC'";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, template);
			AssertEquals("Should be 1 rows in report", 1, result.Rows.Count);

			var transactionDesc = result.Rows[0]["TransactionDesc"].ToString();
			AssertEquals(GLTransactionsSPTest.StringLen1024, transactionDesc);

			var paymentReferenceNumber = result.Rows[0]["PaymentReferenceNumber"].ToString();
			AssertEquals(35, paymentReferenceNumber.Length);
			AssertEquals(GLTransactionsSPTest.StringLen35, paymentReferenceNumber);
		}

		public void TestTransactionExRatePrecision()
		{
			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company) values (NEWID(), 201504,2015,'04/01/2005','04/30/2015 23:59',1,1,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccGLAccountDescriptor(AJ_PK,AJ_Language,AJ_AccountDescription,AJ_LocalAccountNumber,AJ_ReportType,AJ_DebitCredit,AJ_RN_NKCountryOfCompliance) 
VALUES('E94C4723-A84E-4D01-85B3-B09F6C6D3AFB','CHS','测试','12340000','COA','CR','CN')");

			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccGLDescriptorPivot (YJ_PK, YJ_AG, YJ_AJ) values (NEWID(), '907B7A0F-3700-4C4C-B313-971C8B0AE6FC', 'E94C4723-A84E-4D01-85B3-B09F6C6D3AFB')");

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_AG, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AR', 'JNL', '00001000', 1, '907B7A0F-3700-4C4C-B313-971C8B0AE6FC', 'AR Journal', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 25487.28, 30058.95, 'TWD', 0.847910, '2015-04-24 14:04:00', 'FIN', 25487.28, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491')");
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionHeader where AH_PK = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have 1 row", 1, result.Rows.Count);

			var template = @"EXEC GLTransactionsSP_Multilingual
					@CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
					@StartPeriod = NULL,
					@EndPeriod = NULL,
					@StartDate = '2015-04-01',
					@EndDate = '2015-04-30',
					@StartGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
					@EndGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
					@BranchPK = NULL,
					@DepartmentPK = NULL,
					@DisplayDescription = NULL,
					@TransactionCategory = NULL,
					@BatchNumberToGet = NULL,
					@BatchNumberToSet = NULL,
					@IncludeZeroBalance = 'n',
					@Language = 'CHS',
					@CountryCode = 'CN',
					@IsExportingBatch = NULL";

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(template, "NULL", "NULL", "NULL"));
			result.DefaultView.RowFilter = "LocalGLAccount = '12340000'";
			var filteredTable = result.DefaultView.ToTable();
			AssertEquals("Should be 1 local GL Account in report", 1, filteredTable.Rows.Count);
			AssertEquals("Shouldn't lose precision", "0.847910000", filteredTable.Rows[0]["ExRate"].ToString());
		}

		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 03, 16);
			int categoryGroupCount = 26;
			helper.InsertAccPeriod(2020, 3, TestDbHelper.DefaultCompanyPK);

			var categoryGroups = TestHelper.CreateTransactionHeaderByTransactionType("A", categoryGroupCount, "GJL", "T", InsertTransactionHeader);

			string sqlText = $@"EXEC GLTransactionsSP_Multilingual
							@CompanyPK = '{TestDbHelper.DefaultCompanyPK}',
							@StartPeriod = NULL,
							@EndPeriod = NULL,
							@StartDate = '2020-03-01',
							@EndDate = '2020-03-31',
							@StartGLAccountPK = NULL,
							@EndGLAccountPK = NULL,
							@BranchPK = NULL,
							@DepartmentPK = NULL,
							@DisplayDescription = 'L',
							@TransactionCategory = '{"{0}"}',
							@BatchNumberToGet = NULL,
							@BatchNumberToSet = NULL,
							@IncludeZeroBalance = 'N',
							@IsExportingBatch = 'Y',
							@Language = 'ABC'";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Should be 1 rows in report", 1, result.Rows.Count);
			AssertEquals("T00", result.Rows[0]["TransactionNum"].ToString());

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categoryGroups));
			AssertEquals("Should be 25 rows in report", 25, result.Rows.Count);
			result.DefaultView.Sort = "TransactionNum";
			var resultForAssert = result.DefaultView.ToTable();
			for (int index = 0; index < categoryGroupCount - 1; index++)
			{
				var transactionNum = $"T{index.ToString().PadLeft(2, '0')}";
				AssertEquals($"TransactionNum should be {transactionNum}", transactionNum, resultForAssert.Rows[index]["TransactionNum"].ToString());
			}

			void InsertTransactionHeader(string transactionType, string transactionNum, string category)
			{
				var headerPK = helper.InsertTransactionHeader("GL", transactionType, transactionNum, 110, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK, category: category);
				var linePK = helper.InsertTransactionLine(headerPK, null, null, helper.GLAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 100, "CST", postDate, null, 10, 1, "A");
				helper.InsertGenExportBatchSequence("GPS", 1, "AL", linePK);
			}
		}

		public void TestContainUnits()
		{
			var helper = new TestDbHelper(TestConnection);
			var postDate = new DateTime(2020, 03, 16);
			int categoryGroupCount = 26;

			helper.InsertAccPeriod(2020, 3, TestDbHelper.DefaultCompanyPK);

			string sqlText = $@"EXEC GLTransactionsSP_Multilingual
							@CompanyPK = '{TestDbHelper.DefaultCompanyPK}',
							@StartPeriod = NULL,
							@EndPeriod = NULL,
							@StartDate = '2020-03-01',
							@EndDate = '2020-03-31',
							@StartGLAccountPK = NULL,
							@EndGLAccountPK = NULL,
							@BranchPK = NULL,
							@DepartmentPK = NULL,
							@DisplayDescription = 'L',
							@TransactionCategory = '{"{0}"}',
							@BatchNumberToGet = NULL,
							@BatchNumberToSet = NULL,
							@IncludeZeroBalance = 'N',
							@IsExportingBatch = 'Y',
							@Language = 'ABC'";

			var categoryGroups = TestHelper.CreateTransactionHeaderByTransactionType("A", categoryGroupCount, "NJL", "ATN", InsertTransactionHeader);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categoryGroups));
			AssertEquals("Should be 25 rows in report", 25, result.Rows.Count);
			result.DefaultView.Sort = "TransactionNum";
			var resultForAssert = result.DefaultView.ToTable();
			for (int index = 0; index < categoryGroupCount - 1; index++)
			{
				AssertEquals($"transaction num should be KWH", "KWH", resultForAssert.Rows[index]["Units"].ToString());
			}

			void InsertTransactionHeader(string transactionType, string transactionNum, string category)
			{
				var headerPK = helper.InsertTransactionHeader("GL", transactionType, transactionNum, 110, postDate, helper.DefaultBranchPK, helper.DefaultDepartmentPK, category: category);
				var linePK = helper.InsertTransactionLine(headerPK, null, null, helper.GLNoteAccountPK1, helper.DefaultBranchPK, helper.DefaultDepartmentPK, null, 100, "CST", postDate, null, 10, 1, "A");
				helper.InsertGenExportBatchSequence("GPS", 1, "AL", linePK);
			}
		}
	}
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Billing.Collectors.Accounting;
using Enterprise.Integration.Billing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Accounting
{
	[TestedType(typeof(AcceInvoicingTransactionPivotCollector))]
	sealed class AcceInvoicingTransactionPivotCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => new RecurringRange(new DateTime(2024, 10, 10), new DateTime(2024, 10, 11));

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var testCreationDate = new DateTime(2024, 10, 10);
			var sqlQuery = $@"
DECLARE @DepartmentPk UNIQUEIDENTIFIER = NEWID()
DECLARE @DESCompanyPk UNIQUEIDENTIFIER = NEWID();
DECLARE @MADBranchPk UNIQUEIDENTIFIER = NEWID();
DECLARE @BCNBranchPk UNIQUEIDENTIFIER = NEWID();
DECLARE @BRCompanyPk UNIQUEIDENTIFIER = NEWID();
DECLARE @BSTNBranchPk UNIQUEIDENTIFIER = NEWID();
DECLARE @BRJNBranchPk UNIQUEIDENTIFIER = NEWID();
DECLARE @THPK1 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK2 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK3 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK4 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK5 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK6 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK7 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK8 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK9 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK10 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK11 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK12 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK13 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK14 UNIQUEIDENTIFIER = NEWID()
DECLARE @THPK15 UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code) VALUES (@DepartmentPk, 'DEP')
INSERT GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@DESCompanyPk, 'DES', 'DE company', 'EUR', 'DE');
INSERT GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@BRCompanyPk, 'BRS', 'BR company', 'BRL', 'BR');

INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_RL_NKHomePort) VALUES
(@MADBranchPk, 'MAD', @DESCompanyPk, 'ESMAD'),
(@BCNBranchPk, 'BCN', @DESCompanyPk, 'ESBCN'),
(@BSTNBranchPk, 'BSP', @DESCompanyPk, 'BRRIO'),
(@BRJNBranchPk, 'BRJ', @DESCompanyPk, 'BRSSZ');


INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_GB_TaxBranch, AH_InvoiceDate, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_ComplianceSubType, AH_TransactionCategory, AH_InvoiceAmount, AH_OSTaxAmountOtherTaxes, AH_RX_NKTransactionCurrency, AH_SystemCreateTimeUtc, AH_SystemCreateUser) VALUES
(@THPK1, @DESCompanyPk, @MADBranchPk, @DepartmentPk, @MADBranchPk, '{testCreationDate}', 'AP', 'INV', '011', 'TKT', 'STD', -181.5, 0, 'EUR', '{testCreationDate}', 'EFT'),
(@THPK2, @DESCompanyPk, @BCNBranchPk, @DepartmentPk, @MADBranchPk, '{testCreationDate}', 'AP', 'INV', '012', 'TKT', 'STD', -192.39, 0, 'EUR', '{testCreationDate}', 'EFT'),
(@THPK3, @DESCompanyPk, @BCNBranchPk, @DepartmentPk, @MADBranchPk, '{testCreationDate}', 'AP', 'INV', '013', 'TXI', 'FIN', -181.5, 0, 'USD', '{testCreationDate}', 'EFT'),
(@THPK4, @DESCompanyPk, @MADBranchPk, @DepartmentPk, @MADBranchPk, '{testCreationDate}', 'AR', 'CRD', '014', 'TXI', 'STD', 181.5, 0, 'EUR', '{testCreationDate}', 'EFT'),
(@THPK5, @DESCompanyPk, @MADBranchPk, @DepartmentPk, @MADBranchPk, '{testCreationDate}', 'AR', 'CRD', '015', 'TXI', 'STD', -523.93, 0, 'EUR', '{testCreationDate}', 'EFT'),
(@THPK6, @BRCompanyPk, @BSTNBranchPk, @DepartmentPk, @BSTNBranchPk, '{testCreationDate}', 'AR', 'INV', '013', 'NFS', 'FIN', 1407.75, -92.25, 'BRL', '{testCreationDate}', 'EFT'),
(@THPK7, @BRCompanyPk, @BSTNBranchPk, @DepartmentPk, @BRJNBranchPk, '{testCreationDate}', 'AR', 'INV', '014', 'NFS', 'CUR', 2145.05, -21.52, 'BRL', '{testCreationDate}', 'EFT'),
(@THPK8, @BRCompanyPk, @BSTNBranchPk, @DepartmentPk, @BSTNBranchPk, '{testCreationDate}', 'AP', 'INV', '015', 'NFS', 'CUR', 2098.21, -20.34, 'USD', '{testCreationDate}', 'EFT'),
(@THPK9, @BRCompanyPk, @BSTNBranchPk, @DepartmentPk, @BSTNBranchPk, '{testCreationDate}', 'AP', 'INV', '016', 'NFS', 'CUR', 2145.05, -21.52, 'USD', '{testCreationDate}', 'EFT'),
(@THPK10, @BRCompanyPk, @BSTNBranchPk, @DepartmentPk, @BSTNBranchPk, '{testCreationDate}', 'AR', 'CRD', '017', 'NFS', 'CUR', 300.05, 0.00, 'USD', '{testCreationDate}', 'EFT'),
(@THPK11, @BRCompanyPk, @BSTNBranchPk, @DepartmentPk, @BSTNBranchPk, '{testCreationDate}', 'AR', 'CRD', '018', 'NFS', 'CUR', 2145.05, -21.52, 'USD', '{testCreationDate}', 'EFT'),
(@THPK12, @BRCompanyPk, @BSTNBranchPk, @DepartmentPk, NULL, '{testCreationDate}', 'AR', 'CRD', '019', 'NFS', 'CUR', 2145.05, -21.52, 'USD', '{testCreationDate}', 'EFT'),
(@THPK13, @BRCompanyPk, @BSTNBranchPk, @DepartmentPk, NULL, '{testCreationDate}', 'AP', 'INV', '020', 'TKT', 'STD', 2145.05, -21.52, 'EUR', '{testCreationDate}', 'EFT'),
(@THPK14, @BRCompanyPk, @BSTNBranchPk, @DepartmentPk, @BSTNBranchPk, '{testCreationDate}', 'AR', 'CRD', '021', 'NFS', 'CUR', 100.00, -10.50, 'USD', '{testCreationDate}', 'EFT')


INSERT INTO dbo.AccEInvoicingTransactionPivot (AIP_PK, AIP_GC, AIP_RN_NKCountryCode, AIP_ParentID, AIP_ParentTableCode, AIP_Status, AIP_ActionType, AIP_LastResponseReceivedUtc) VALUES
(NEWID(), @DESCompanyPk, 'ES', @THPK1, 'AH', 'FAL', 'SUB', '{testCreationDate.AddHours(1)}'),
(NEWID(), @DESCompanyPk, 'ES', @THPK2, 'AH', 'SUC', 'SUB', '{testCreationDate.AddHours(2)}'),
(NEWID(), @DESCompanyPk, 'ES', @THPK3, 'AH', 'FAL', 'SUB', '{testCreationDate.AddHours(3)}'),
(NEWID(), @DESCompanyPk, 'ES', @THPK4, 'AH', 'SUC', 'SUB', '{testCreationDate.AddHours(4)}'),
(NEWID(), @DESCompanyPk, 'ES', @THPK5, 'AH', 'SUC', 'SUB', '{testCreationDate.AddHours(5)}'),
(NEWID(), @BRCompanyPk, 'BR', @THPK6, 'AH', 'FAL', 'SUB', '{testCreationDate.AddHours(7)}'),
(NEWID(), @BRCompanyPk, 'BR', @THPK7, 'AH', 'FAL', 'SUB', '{testCreationDate.AddHours(8)}'),
(NEWID(), @BRCompanyPk, 'BR', @THPK8, 'AH', 'SUC', 'SUB', '{testCreationDate.AddHours(9)}'),
(NEWID(), @BRCompanyPk, 'BR', @THPK9, 'AH', 'SUC', 'SUB', '{testCreationDate.AddHours(2)}'),
(NEWID(), @BRCompanyPk, 'BR', @THPK10, 'AH', 'SUC', 'SUB', '{testCreationDate.AddHours(1)}'),
(NEWID(), @BRCompanyPk, 'BR', @THPK11, 'AH', 'SUC', 'SUB', '{testCreationDate}'),
-- TransactionPivots in out range of date 
(NEWID(), @BRCompanyPk, 'BR', @THPK12, 'AH', 'SUC', 'SUB', '{new DateTime(2024, 10, testCreationDate.Day + 2)}'),
-- TransactionPivots in with not SUB ActionType 
(NEWID(), @BRCompanyPk, 'BR', @THPK13, 'AH', 'SUC', 'REJ', '{testCreationDate}'),
(NEWID(), @BRCompanyPk, 'BR', @THPK14, 'AH', 'SUC', 'SUB', '{testCreationDate}')
";

			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var transactionList = transactions.ToList();

			var expectedRows = new List<ExpectedGeneratedTransaction>()
			{
				new ExpectedGeneratedTransaction
				{
					CountryCode = "BR",
					Ledger = "AR",
					TransactionType = "INV",
					ComplianceSubType = "NFS",
					BranchCode = "BSP",
					CompanyCode = "BRS",
					ItemCount = 1,
					AdditionalRefs = GenerateAdditionalRefs("BRJ", 2145.0500m, -21.5200m, "BRRIO", "BRSSZ", "FAL", "CUR", "BRL", "BRL")
				},
				new ExpectedGeneratedTransaction
				{
					CountryCode = "BR",
					Ledger = "AR",
					TransactionType = "INV",
					ComplianceSubType = "NFS",
					BranchCode = "BSP",
					CompanyCode = "BRS",
					ItemCount = 1,
					AdditionalRefs = GenerateAdditionalRefs("BSP", 1407.7500m, -92.2500m, "BRRIO", "BRRIO", "FAL", "FIN", "BRL", "BRL")
				},
				new ExpectedGeneratedTransaction
				{
					CountryCode = "ES",
					Ledger = "AP",
					TransactionType = "INV",
					ComplianceSubType = "TXI",
					BranchCode = "BCN",
					CompanyCode = "DES",
					ItemCount = 1,
					AdditionalRefs = GenerateAdditionalRefs("MAD", -181.5000m, 0.0000m, "ESBCN", "ESMAD", "FAL", "FIN", "EUR", "USD")
				},
				new ExpectedGeneratedTransaction
				{
					CountryCode = "BR",
					Ledger = "AP",
					TransactionType = "INV",
					ComplianceSubType = "NFS",
					BranchCode = "BSP",
					CompanyCode = "BRS",
					ItemCount = 2,
					AdditionalRefs = GenerateAdditionalRefs("BSP", 4243.2600m, -41.8600m, "BRRIO", "BRRIO", "SUC", "CUR", "BRL", "USD")
				},
				new ExpectedGeneratedTransaction
				{
					CountryCode = "BR",
					Ledger = "AR",
					TransactionType = "CRD",
					ComplianceSubType = "NFS",
					BranchCode = "BSP",
					CompanyCode = "BRS",
					ItemCount = 3,
					AdditionalRefs = GenerateAdditionalRefs("BSP", 2545.1000m, -32.0200m, "BRRIO", "BRRIO", "SUC", "CUR", "BRL", "USD")
				},
				new ExpectedGeneratedTransaction
				{
					CountryCode = "ES",
					Ledger = "AP",
					TransactionType = "INV",
					ComplianceSubType = "TKT",
					BranchCode = "BCN",
					CompanyCode = "DES",
					ItemCount = 1,
					AdditionalRefs = GenerateAdditionalRefs("MAD", -192.3900m, 0.0000m, "ESBCN", "ESMAD", "SUC", "STD", "EUR", "EUR")
				},
				new ExpectedGeneratedTransaction
				{
					CountryCode = "ES",
					Ledger = "AR",
					TransactionType = "CRD",
					ComplianceSubType = "TXI",
					BranchCode = "MAD",
					CompanyCode = "DES",
					ItemCount = 2,
					AdditionalRefs = GenerateAdditionalRefs("MAD", -342.4300m, 0.0000m, "ESMAD", "ESMAD", "SUC", "STD", "EUR", "EUR")
				},
				new ExpectedGeneratedTransaction
				{
					CountryCode = "ES",
					Ledger = "AP",
					TransactionType = "INV",
					ComplianceSubType = "TKT",
					BranchCode = "MAD",
					CompanyCode = "DES",
					ItemCount = 1,
					AdditionalRefs = GenerateAdditionalRefs("MAD", -181.50000m, 0.0000m, "ESMAD", "ESMAD", "FAL", "STD", "EUR", "EUR")
				}
			};

			AssertEquals("Number of Transactions", expectedRows.Count, transactionList.Count);

			foreach (var expectedRow in expectedRows)
			{
				var row = transactions.FirstOrDefault(t => t.GetCompanyCode() == expectedRow.CompanyCode &&
						t.GetBranchCode() == expectedRow.BranchCode &&
						t.Reference1 == expectedRow.Ledger &&
						t.Reference2 == expectedRow.TransactionType &&
						t.Reference3 == expectedRow.CountryCode &&
						t.Reference4 == expectedRow.ComplianceSubType &&
						JsonConvert.DeserializeObject<JObject>(t.AdditionalRefs).ToString().Equals(JsonConvert.DeserializeObject<JObject>(expectedRow.AdditionalRefs).ToString())
						);
				AssertEquals($"There are {expectedRow.ItemCount} rows with " +
					$"{expectedRow.CompanyCode} CompanyCode, {expectedRow.BranchCode} BranchCode, " +
					$"{expectedRow.Ledger} Ledger, {expectedRow.TransactionType} TransactionType, " +
					$"{expectedRow.ComplianceSubType} ComplianceSubType, {expectedRow.CountryCode} CountryCode",
					expectedRow.ItemCount,
					row?.BillableCount ?? 0);
			}
	
			AssertESRows();
			AssertBRRows();

			void AssertESRows()
			{
				var esCountryGroup = transactions.Where(t => t.Reference3 == "ES").ToList();
				AssertEquals("Number of ES Transactions", 4, esCountryGroup.Count);
				AssertEquals("Number of ES AR Transactions", 1, esCountryGroup.Count(t => t.Reference1 == "AR"));
				AssertEquals("Number of ES AP Transactions", 3, esCountryGroup.Count(t => t.Reference1 == "AP"));
				AssertEquals("Number of ES AR CRD Transactions", 1, esCountryGroup.Count(t => t.Reference2 == "CRD"));
				AssertEquals("Number of ES AR INV Transactions", 3, esCountryGroup.Count(t => t.Reference2 == "INV"));
				AssertEquals("Number of ES AR CRD TXI Transactions", 1, esCountryGroup.Count(t => t.Reference2 == "CRD" && t.Reference4 == "TXI"));
				AssertEquals("Number of ES AP INV Transactions", 3, esCountryGroup.Count(t => t.Reference2 == "INV"));
				AssertEquals("Number of ES AP INV TKT Transactions", 2, esCountryGroup.Count(t => t.Reference2 == "INV" && t.Reference4 == "TKT"));
				AssertEquals("Number of ES AP INV TXI Transactions", 1, esCountryGroup.Count(t => t.Reference2 == "INV" && t.Reference4 == "TXI"));
				AssertEquals("Number of ES MAD Transactions", 2, esCountryGroup.Count(t => t.GetBranchCode() == "MAD"));
				AssertEquals("Number of ES BCN Transactions", 2, esCountryGroup.Count(t => t.GetBranchCode() == "BCN"));
			}
			void AssertBRRows()
			{
				var brCountryGroup = transactions.Where(t => t.Reference3 == "BR").ToList();
				AssertEquals("Number of BR Transactions", 4, brCountryGroup.Count);
				AssertEquals("Number of BR AR Transactions", 3, brCountryGroup.Count(t => t.Reference1 == "AR"));
				AssertEquals("Number of BR AP Transactions", 1, brCountryGroup.Count(t => t.Reference1 == "AP"));
				AssertEquals("Number of BR AR CRD Transactions", 1, brCountryGroup.Count(t => t.Reference2 == "CRD"));
				AssertEquals("Number of BR AR INV Transactions", 3, brCountryGroup.Count(t => t.Reference2 == "INV"));
				AssertEquals("Number of BR AP INV Transactions", 3, brCountryGroup.Count(t => t.Reference2 == "INV"));
				AssertEquals("Number of BR AR CRD NFS Transactions", 1, brCountryGroup.Count(t => t.Reference2 == "CRD" && t.Reference4 == "NFS"));
				AssertEquals("Number of BR AR INV NFS Transactions", 3, brCountryGroup.Count(t => t.Reference2 == "INV" && t.Reference4 == "NFS"));
				AssertEquals("Number of BR BSP Transactions", 4, brCountryGroup.Count(t => t.GetBranchCode() == "BSP"));
			}
		}

		string GenerateAdditionalRefs(string taxBranchCode, decimal totalLocalAmount, decimal totalOSTaxAmount, string branchHomePort, string taxBranchHomePort, string aipStatus, string transactionCategory, string localCurrency, string transactionCurrency)
		{
			var additionalRefs = new StringBuilder("{");

			if (!string.IsNullOrEmpty(taxBranchCode))
			{
				additionalRefs.Append($"\"TaxBranchCode\":\"{taxBranchCode}\",");
			}
			additionalRefs.Append($"\"Total_LocalAmount\":{totalLocalAmount},");
			additionalRefs.Append($"\"Total_OSTaxAmount\":{totalOSTaxAmount},");
			additionalRefs.Append($"\"BranchHomePort\":\"{branchHomePort}\",");

			if (!string.IsNullOrEmpty(taxBranchHomePort))
			{
				additionalRefs.Append($"\"TaxBranchHomePort\":\"{taxBranchHomePort}\",");
			}
			additionalRefs.Append($"\"AIP_Status\":\"{aipStatus}\",");
			additionalRefs.Append($"\"AH_TransactionCategory\":\"{transactionCategory}\",");
			additionalRefs.Append($"\"LocalCurrency\":\"{localCurrency}\",");
			additionalRefs.Append($"\"TransactionCurrency\":\"{transactionCurrency}\"");
			additionalRefs.Append("}");
			return additionalRefs.ToString();
		}

		class ExpectedGeneratedTransaction
		{
			public string CountryCode { get; set; }
			public string Ledger { get; set; }
			public string TransactionType { get; set; }
			public string ComplianceSubType { get; set; }
			public string BranchCode { get; set; }
			public string CompanyCode { get; set; }
			public int ItemCount { get; set; }
			public string AdditionalRefs { get; set; }
		}
	}
}

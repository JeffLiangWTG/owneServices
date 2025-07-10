using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_Validation_TransactionLineFieldsTest : BatchAggregatorValidationTest
	{
		public override void TestAggregationValidation()
		{
			PrepareForAggregation();
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_TransactionReference,AH_Desc,AH_InvoiceDate,AH_TransactionCategory,AH_DueDate,AH_InvoiceAmount,AH_GSTAmount,AH_WithholdingTax,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_AgePeriod,AH_PostPeriod,AH_PostDate,AH_ChequeOrReference,AH_ReceiptType,AH_CashBasisGSTIndicator,AH_CashBasisGSTRealisedToGL,AH_ChequeDrawer,AH_DrawerBank,AH_DrawerBranch,AH_InvoiceApproved,AH_ConsolidatedInvoiceRef,AH_FullyPaidDate,AH_InvoicePrinted,AH_IsCancelled,AH_DateClearedInCashbook,AH_NotAllocated,AH_OutstandingAmount,AH_PostedToEFT,AH_PostToGL,AH_ReceiptBatchNo,AH_TransactionCreatedByMatching,AH_InvoiceTerm,AH_InvoiceTermDays,AH_POST1,AH_POST2,AH_POST3,AH_POST4,AH_AB,AH_OH,AH_JH,AH_GB,AH_GE,AH_AG,AH_TransactionBelongsToGroup,AH_AH_InvoiceStatement,AH_PostedInternal,AH_GC)VALUES('7CC1FE49-9E8C-420C-8299-9522DB4856D8','AP','INV','00005013',1,'','DIRECT PAYMENT','May 25 2005  3:44:00:000PM','','May 25 2005  3:44:00:000PM',-30.0000,-1.0000,0.0000,-31.0000,'AUD',1.000000000,0,0,'May 25 2005  3:44:00:000PM','CASH','CSH',0,0,'CASH','','',0,'',NULL,0,0,NULL,0,0.0000,0,0,'',0,'',0,0,0,0,0,NULL,NULL,NULL,'FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227',NULL,NULL,NULL,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("UPDATE dbo.AccTransactionHeader SET AH_PostToGL = 'N', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST'");
			TestConnection.ExecuteNonQuery("insert into dbo.Acctransactionlines (AL_PK, AL_AH, AL_GB, AL_GE, AL_GC, AL_ReverseDate, AL_LineType) VALUES (NEWID(), '7CC1FE49-9E8C-420C-8299-9522DB4856D8', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'May 25 2005  3:44:00:000PM', 'CST')");

			var messageCaught = RunAndAssertAggregationWithError();

			AssertEquals("The following transactions have been posted with lines that do not have a Charge or GL code:\r\nAP\tINV\t00005013", messageCaught);
		}

		public void TestAggregationValidation_ChargeCodeValidation_JC_JRJ()
		{
			AssertAggregateValidation_ChargeCodeValidation("JC", "JRJ");
		}

		public void TestAggregationValidation_ChargeCodeValidation_AP_CRD()
		{
			AssertAggregateValidation_ChargeCodeValidation("AP", "CRD");
		}

		public void TestAggregationValidation_ChargeCodeValidation_AP_ADJ()
		{
			AssertAggregateValidation_ChargeCodeValidation("AP", "ADJ");
		}

		public void TestAggregationValidation_ChargeCodeValidation_AR_INV()
		{
			AssertAggregateValidation_ChargeCodeValidation("AR", "INV");
		}

		public void TestAggregationValidation_ChargeCodeValidation_AR_CRD()
		{
			AssertAggregateValidation_ChargeCodeValidation("AR", "CRD");
		}

		public void TestAggregationValidation_ChargeCodeValidation_AR_ADJ()
		{
			AssertAggregateValidation_ChargeCodeValidation("AR", "ADJ");
		}

		void AssertAggregateValidation_ChargeCodeValidation(string ledger, string transactionType)
		{
			PrepareForAggregation();
			var transactionNumber = "00001001";

			var headerPK = DbHelper.InsertTransactionHeader(ledger, transactionType, transactionNum: transactionNumber, invoiceAmount: 101, postDate: TestHelper.DefaultPostDate, postToGL: false);
			DbHelper.InsertTransactionLine(headerPK, chargeCodePK: null, glAccountPK: null, reverseDate: TestHelper.DefaultPostDate);

			var messageCaught = RunAndAssertAggregationWithError();

			AssertEquals($"The following transactions have been posted with lines that do not have a Charge or GL code:\r\n{ledger}\t{transactionType}\t{transactionNumber}", messageCaught);
		}
	}
}


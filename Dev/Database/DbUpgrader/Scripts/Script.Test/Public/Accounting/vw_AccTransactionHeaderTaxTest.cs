using System.Text;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(vw_AccTransactionHeaderTax))]
	class vw_AccTransactionHeaderTaxTest : DbCreateScriptTest
	{
		public void TestQueryHintExistsOnView()
		{
			try
			{
				using (var command1 = TestConnection.Command("SET SHOWPLAN_ALL ON"))
				using (var command2 = TestConnection.Command("SELECT TOP 10 * FROM dbo.vw_AccTransactionHeaderTax WHERE AH_PK = '133C8AA6-35F4-491F-A4B9-0008B56F1047'"))
				{
					command1.CommandType = System.Data.CommandType.Text;
					command2.CommandType = System.Data.CommandType.Text;
					command1.ExecuteNonQuery();
					StringBuilder sb = new StringBuilder();
					using (var reader = command2.ExecuteReader())
					{
						do
						{
							while (reader.Read())
							{
								sb.Append(reader.GetString(0) + System.Environment.NewLine);
							}

							sb.Append(System.Environment.NewLine);
						} while (reader.NextResult());
					}
					AssertEquals("With 'Query Hint', view should not use 'Steam Aggregate'", false, sb.ToString().Contains("Stream Aggregate"));
				}
			}
			finally
			{
				TestConnection.ExecuteNonQuery("SET SHOWPLAN_ALL OFF");
			}
		}

		public void TestOSTaxAmountIsAlwaysZeroWhenLineTaxIsZero()
		{
			InsertTransactionHeader();
			InsertTransactionLine(7189000m, 611.07m, 0.000085m, 0m);
			var aH_OSTaxAmountWithNoReciprocal = TestConnection.ExecuteScalar("SELECT  SUM(AH_OSTaxAmountWithNoReciprocal) FROM dbo.vw_AccTransactionHeaderTax WHERE AH_PK = '7CC1FE49-9E8C-420C-8299-9522DB4856D8'");
			var aH_OSTaxAmountWithReciprocal = TestConnection.ExecuteScalar("SELECT  SUM(AH_OSTaxAmountWithReciprocal) FROM dbo.vw_AccTransactionHeaderTax WHERE AH_PK = '7CC1FE49-9E8C-420C-8299-9522DB4856D8'");

			AssertEquals(0m, aH_OSTaxAmountWithReciprocal);
			AssertEquals(0m, aH_OSTaxAmountWithNoReciprocal);
		}

		public void TestPostedBigAmountForWithReciprocalCompanyTransaction()
		{
			InsertTransactionHeader();
			InsertTransactionLine(135802468.03m, 2420721350695.00m, 19607.843100002m, 10m);
			var aH_OSTaxAmountWithNoReciprocal = TestConnection.ExecuteScalar("SELECT  SUM(AH_OSTaxAmountWithNoReciprocal) FROM dbo.vw_AccTransactionHeaderTax WHERE AH_PK = '7CC1FE49-9E8C-420C-8299-9522DB4856D8'");
			var aH_OSTaxAmountWithReciprocal = TestConnection.ExecuteScalar("SELECT  SUM(AH_OSTaxAmountWithReciprocal) FROM dbo.vw_AccTransactionHeaderTax WHERE AH_PK = '7CC1FE49-9E8C-420C-8299-9522DB4856D8'");
			AssertEquals("The value is more than money so it should return '0'.", 0m, aH_OSTaxAmountWithNoReciprocal);
			AssertEquals(12345678.91000m, aH_OSTaxAmountWithReciprocal);
		}

		public void TestPostedBigAmountForWithNoReciprocalCompanyTransaction()
		{
			InsertTransactionHeader();
			InsertTransactionLine(12m, 2420721350695.00m, 0.002m, 10m);
			var aH_OSTaxAmountWithNoReciprocal = TestConnection.ExecuteScalar("SELECT  SUM(AH_OSTaxAmountWithNoReciprocal) FROM dbo.vw_AccTransactionHeaderTax WHERE AH_PK = '7CC1FE49-9E8C-420C-8299-9522DB4856D8'");
			var aH_OSTaxAmountWithReciprocal = TestConnection.ExecuteScalar("SELECT  SUM(AH_OSTaxAmountWithReciprocal) FROM dbo.vw_AccTransactionHeaderTax WHERE AH_PK = '7CC1FE49-9E8C-420C-8299-9522DB4856D8'");
			AssertEquals(-4841442689.39m, aH_OSTaxAmountWithNoReciprocal);
			AssertEquals("The value is more than money so it should return '0'.", 0m, aH_OSTaxAmountWithReciprocal);
		}

		void InsertTransactionHeader()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_TransactionCount,AH_TransactionReference,AH_Desc,AH_InvoiceDate,AH_TransactionCategory,AH_DueDate,AH_InvoiceAmount,AH_GSTAmount,AH_WithholdingTax,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_AgePeriod,AH_PostPeriod,AH_PostDate,AH_ChequeOrReference,AH_ReceiptType,AH_CashBasisGSTIndicator,AH_CashBasisGSTRealisedToGL,AH_ChequeDrawer,AH_DrawerBank,AH_DrawerBranch,AH_InvoiceApproved,AH_ConsolidatedInvoiceRef,AH_FullyPaidDate,AH_InvoicePrinted,AH_IsCancelled,AH_DateClearedInCashbook,AH_NotAllocated,AH_OutstandingAmount,AH_PostedToEFT,AH_PostToGL,AH_ReceiptBatchNo,AH_TransactionCreatedByMatching,AH_InvoiceTerm,AH_InvoiceTermDays,AH_POST1,AH_POST2,AH_POST3,AH_POST4,AH_AB,AH_OH,AH_JH,AH_GB,AH_GE,AH_AG,AH_TransactionBelongsToGroup,AH_AH_InvoiceStatement,AH_PostedInternal,AH_GC)VALUES('7CC1FE49-9E8C-420C-8299-9522DB4856D8','AP','INV','00005013',1,'','DIRECT PAYMENT','May 25 2005  3:44:00:000PM','','May 25 2005  3:44:00:000PM',-30.0000,-1.0000,0.0000,-31.0000,'AUD',1.000000000,0,0,'May 25 2005  3:44:00:000PM','CASH','CSH',0,0,'CASH','','',0,'',NULL,0,0,NULL,0,0.0000,0,0,'',0,'',0,0,0,0,0,NULL,NULL,NULL,'FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227',NULL,NULL,NULL,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
		}

		void InsertTransactionLine(decimal aL_OSAmount, decimal aL_LineAmount, decimal aL_ExchangeRate, decimal aL_TaxAmount)
		{
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.Acctransactionlines (AL_PK, AL_LineType, AL_AH, AL_GB, AL_GE, AL_ReverseDate,AL_RX_NKTransactionCurrency,AL_OSAmount,AL_LineAmount,AL_ExchangeRate, AL_GSTVAT, AL_GC, AL_AG) VALUES (NEWID(), 'WIP', '7CC1FE49-9E8C-420C-8299-9522DB4856D8', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', '2B67864D-42E9-4A43-A9C8-09D2083C4227', 'May 25 2005  3:44:00:000PM','AUD',{0},{1},{2},{3},'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '{4}')", aL_OSAmount, aL_LineAmount, aL_ExchangeRate, aL_TaxAmount, DbHelper.GLAccountPK1));
		}
	}
}


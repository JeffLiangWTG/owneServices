using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ARAP;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ARAP
{
	[TestedType(typeof(csfn_TransactionHeaderDetail))]
	class csfn_TransactionHeaderDetailTest : DbCreateScriptTest
	{
		[TestDate(2015, 11, 02)]
		public void TestAH_SystemCreateUser()
		{
			var invoicePK = Guid.NewGuid();
			var referenceDate = new DateTime(2015, 11, 02);
			var invoiceDate = referenceDate.ToString("s");
			var paymentDate = referenceDate.ToString("s");

			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_SystemCreateUser,AH_TransactionCount,AH_Desc,AH_InvoiceDate,AH_InvoiceAmount,AH_GSTAmount,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_PostDate,AH_FullyPaidDate,AH_OutstandingAmount,AH_OH,AH_GB,AH_GE,AH_GC)
	VALUES ('{0}','AP','INV','00001109','E',1,'AP INVOICE','{1}',-300.00,-30.00,-330.00,'NZD',1.000000000,'{1}','{2}',0,'C3F842EF-3BE5-448C-BED3-0017B232C624','FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
",
invoicePK, invoiceDate, paymentDate));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_TransactionHeaderDetail ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', '','INV', '', '', '', '', '', '', '' ,'', '')");
			AssertEquals("One row should be found", 1, result.Rows.Count);
			AssertEquals("E", result.Rows[0]["AH_SystemCreateUser"].ToString().Trim());
		}

		public void TestInvoiceTotalWithOtherTaxes()
		{
			var invoicePK = Guid.NewGuid();
			var referenceDate = DateTime.Today;
			var invoiceDate = referenceDate.ToString("s");
			var paymentDate = referenceDate.ToString("s");

			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_SystemCreateUser,AH_TransactionCount,AH_Desc,AH_InvoiceDate,AH_InvoiceAmount,AH_GSTAmount,AH_LocalTaxAmountOtherTaxes,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_PostDate,AH_FullyPaidDate,AH_OutstandingAmount,AH_OH,AH_GB,AH_GE,AH_GC)
	VALUES ('{0}','AP','INV','00001109','E',1,'AP INVOICE','{1}',-300.00,-30.00,-20.00,-350.00,'AUD',1.000000000,'{1}','{2}',0,'C3F842EF-3BE5-448C-BED3-0017B232C624','FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
",
invoicePK, invoiceDate, paymentDate));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM csfn_TransactionHeaderDetail ('878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', '','INV', '', '', '', '', '', '', '' ,'', '')");
			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
			AssertEquals(-350m, result.Rows[0]["AH_InvoiceAmount"]);
		}
	}
}

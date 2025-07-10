using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(ARTransactionsTaxInvoiceRegisterSP))]
	class ARTransactionsTaxInvoiceRegisterSPTest : DbCreateScriptTest
	{
		[TestDate(2016, 01, 01)]
		public void TestStaff()
		{
			var invoicePK = Guid.NewGuid();
			var referenceDate = new DateTime(2016, 01, 01);
			var invoiceDate = referenceDate.ToString("s");
			var paymentDate = referenceDate.ToString("s");

			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_SystemCreateUser,AH_SystemCreateTimeUtc,AH_TransactionCount,AH_Desc,AH_InvoiceDate,AH_InvoiceAmount,AH_GSTAmount,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_PostDate,AH_FullyPaidDate,AH_OutstandingAmount,AH_OH,AH_GB,AH_GE,AH_GC)
	VALUES ('{0}','AR','INV','00001109','E','{1}',1,'AR INVOICE','{1}',300.00,30.00,330.00,'NZD',1.000000000,'{1}','{2}',0,'C3F842EF-3BE5-448C-BED3-0017B232C624','FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')
",
invoicePK, invoiceDate, paymentDate));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ARTransactionsTaxInvoiceRegisterSP @Company = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', @BranchList = null, @OrgGroupList = null, @InvoiceStatus = 'N', @TransactionTypeList = '', @PostDateFrom = null, @PostDateTo = null, @TransactionDateFrom = null, @TransactionDateTo = null, @Cancelled = '', @CurrentCountry = 'AU', @Language = 'EN'");
			AssertEquals("One row should be found", 1, result.Rows.Count);
			AssertEquals("E", result.Rows[0]["AddedBy"].ToString().Trim());
			AssertEquals("CargoWise Support", result.Rows[0]["AddedName"].ToString().Trim());
			AssertEquals("1/01/2016 12:00:00 AM", result.Rows[0]["DateAdded"].ToString().Trim());
		}

		[TestDate(2016, 01, 01)]
		public void TestStaffFullNameLength()
		{
			var invoicePK = Guid.NewGuid();
			var referenceDate = new DateTime(2016, 01, 01);
			var invoiceDate = referenceDate.ToString("s");
			var paymentDate = referenceDate.ToString("s");
			var staffFullName = "abcdefghijklmnopqrstuvwxyz1234567890";

			TestConnection.ExecuteNonQuery(string.Format(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK,AH_Ledger,AH_TransactionType,AH_TransactionNum,AH_SystemCreateUser,AH_SystemCreateTimeUtc,AH_TransactionCount,AH_Desc,AH_InvoiceDate,AH_InvoiceAmount,AH_GSTAmount,AH_OSTotal,AH_RX_NKTransactionCurrency,AH_ExchangeRate,AH_PostDate,AH_FullyPaidDate,AH_OutstandingAmount,AH_OH,AH_GB,AH_GE,AH_GC)
	VALUES ('{0}','AR','INV','00001109','E','{1}',1,'AR INVOICE','{1}',300.00,30.00,330.00,'NZD',1.000000000,'{1}','{2}',0,'C3F842EF-3BE5-448C-BED3-0017B232C624','FDD429D2-648C-4895-8F9F-06E90DED2BE5','2B67864D-42E9-4A43-A9C8-09D2083C4227','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC');

UPDATE dbo.GlbStaff SET GS_FullName = '{3}', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_Code = 'E';
",
invoicePK, invoiceDate, paymentDate, staffFullName));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ARTransactionsTaxInvoiceRegisterSP @Company = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', @BranchList = null, @OrgGroupList = null, @InvoiceStatus = 'N', @TransactionTypeList = '', @PostDateFrom = null, @PostDateTo = null, @TransactionDateFrom = null, @TransactionDateTo = null, @Cancelled = '', @CurrentCountry = 'AU', @Language = 'EN'");
			AssertEquals("One row should be found", 1, result.Rows.Count);
			AssertEquals("abcdefghijklmnopqrstuvwxyz1234567890", result.Rows[0]["AddedName"].ToString().Trim());
		}
	}
}

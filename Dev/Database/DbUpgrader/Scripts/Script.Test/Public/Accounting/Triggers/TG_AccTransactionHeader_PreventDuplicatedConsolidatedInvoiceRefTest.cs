using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccTransactionHeader_PreventDuplicatedConsolidatedInvoiceRef))]
	class TG_AccTransactionHeader_PreventDuplicatedConsolidatedInvoiceRefTest : DbCreateScriptTest
	{
		[ExpectNoExceptions()]
		public void TestUniqueReferenceNumberCanUpdateExistingRecord()
		{
			string consolidatedInvoiceRef = new string('a', 6);
			string uniqueInvoiceRef = new string('b', 6);

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AP', 'INV', '00001000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '" + consolidatedInvoiceRef + @"')");

			TestConnection.ExecuteNonQuery(@"UPDATE dbo.AccTransactionHeader SET AH_ConsolidatedInvoiceRef = '" + uniqueInvoiceRef + "', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
		}

		[ExpectNoExceptions()]
		public void TestDuplicateReferenceNumberCanUpdateExistingRecord_ARLedger()
		{
			string consolidatedInvoiceRef = new string('a', 6);
			string duplicateInvoiceRef = new string('a', 6);

			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AR', 'INV', '00001000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '{consolidatedInvoiceRef}')");

			TestConnection.ExecuteNonQuery($"UPDATE dbo.AccTransactionHeader SET AH_ConsolidatedInvoiceRef = '{duplicateInvoiceRef}', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
		}

		public void TestDuplicatedReferenceNumberCanNotUpdateExistingRecord()
		{
			string consolidatedInvoiceRef = new string('a', 6);
			string uniqueInvoiceRef = new string('b', 6);
			string duplicatedInvoiceRef = new string('a', 6);

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AP', 'INV', '00001000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '" + consolidatedInvoiceRef + @"')");

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C1230', 'AP', 'INV', '00002000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '" + uniqueInvoiceRef + @"')");

			bool exceptionCatured = false;
			try
			{
				TestConnection.ExecuteNonQuery(@"UPDATE dbo.AccTransactionHeader SET AH_ConsolidatedInvoiceRef = '" + duplicatedInvoiceRef + "', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK = '591E6B52-95DF-42F8-8972-53FFF59C1230'");
			}
			catch (Exception e)
			{
				if (e.Message.Contains(SQL_ERROR_MSG))
				{
					exceptionCatured = true;
				}
			}

			Assert("Existing record should not be updated using a duplicated AH_ConsolidatedInvoiceRef successfully.", exceptionCatured);
		}

		[ExpectNoExceptions()]
		public void TestUniqueReferenceNumberCanBeInserted()
		{
			string consolidatedInvoiceRef = new string('a', 6);
			string uniqueInvoiceRef = new string('b', 6);

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AP', 'INV', '00001000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '" + consolidatedInvoiceRef + @"')");

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C1230', 'AP', 'INV', '00002000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '" + uniqueInvoiceRef + @"')");
		}

		[ExpectNoExceptions()]
		public void TestDuplicateReferenceNumberCanBeInserted_ARLedger()
		{
			string consolidatedInvoiceRef = new string('a', 6);
			string duplicateInvoiceRef = new string('a', 6);

			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AR', 'INV', '00001000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '{consolidatedInvoiceRef}')");

			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C1230', 'AR', 'INV', '00002000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '{duplicateInvoiceRef}')");
		}

		[ExpectNoExceptions()]
		public void TestUniqueReferenceNumberCanBeInserted_WhenManyRecordsWithEmptyValue()
		{
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AP', 'INV', '00001000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')
     ,('591E6B52-95DF-42F8-8972-53FFF59C123E', 'AP', 'INV', '00001001', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')");

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C1230', 'AP', 'INV', '00002000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1231', 'AP', 'INV', '00002001', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')");
		}

		[ExpectNoExceptions()]
		public void TestUniqueReferenceNumberCanBeInserted_WhenManyRecordsWithMixedValue()
		{
			string consolidatedInvoiceRef1 = new string('a', 6);
			string consolidatedInvoiceRef2 = new string('b', 6);
			string uniqueInvoiceRef = new string('z', 6);

			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AP', 'INV', '00001000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')
     ,('591E6B52-95DF-42F8-8972-53FFF59C123E', 'AP', 'INV', '00001001', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '{consolidatedInvoiceRef1}')
     ,('591E6B52-95DF-42F8-8972-53FFF59C123D', 'AP', 'INV', '00001002', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '{consolidatedInvoiceRef2}')");

			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C1230', 'AP', 'INV', '00002000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1231', 'AP', 'INV', '00002001', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '{uniqueInvoiceRef}')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1232', 'AP', 'INV', '00002002', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')");
		}

		public void TestDuplicatedReferenceNumberCanNotBeInserted()
		{
			string consolidatedInvoiceRef = new string('a', 6);
			string duplicatedInvoiceRef = new string('a', 6);

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AP', 'INV', '00001000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '" + consolidatedInvoiceRef + @"')");

			bool exceptionCatured = false;
			try
			{
				TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C1230', 'AP', 'INV', '00002000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '" + duplicatedInvoiceRef + @"')");
			}
			catch (Exception e)
			{
				if (e.Message.Contains(SQL_ERROR_MSG))
				{
					exceptionCatured = true;
				}
			}

			Assert("A new record with a duplicated AH_ConsolidatedInvoiceRef should not be inserted successfully.", exceptionCatured);
		}

		public void TestDuplicatedReferenceNumberCanNotBeInserted_WhenManyRecords()
		{
			string consolidatedInvoiceRef1 = new string('a', 6);
			string consolidatedInvoiceRef2 = new string('b', 6);
			string consolidatedInvoiceRef3 = new string('c', 6);
			string duplicatedInvoiceRef = new string('a', 6);
			string uniqueInvoiceRef1 = new string('z', 6);
			string uniqueInvoiceRef2 = new string('y', 6);

			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AP', 'INV', '00001000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '{consolidatedInvoiceRef1}')
     ,('591E6B52-95DF-42F8-8972-53FFF59C123E', 'AP', 'INV', '00001001', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '{consolidatedInvoiceRef2}')
     ,('591E6B52-95DF-42F8-8972-53FFF59C123D', 'AP', 'INV', '00001002', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '{consolidatedInvoiceRef3}')
     ,('591E6B52-95DF-42F8-8972-53FFF59C123C', 'AP', 'INV', '00001003', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')");

			bool exceptionCatured = false;
			try
			{
				TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C1230', 'AP', 'INV', '00002000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '{duplicatedInvoiceRef}')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1231', 'AP', 'INV', '00002001', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '{uniqueInvoiceRef1}')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1232', 'AP', 'INV', '00002002', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '{uniqueInvoiceRef2}')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1233', 'AP', 'INV', '00002003', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')");
			}
			catch (Exception e)
			{
				if (e.Message.Contains(SQL_ERROR_MSG))
				{
					exceptionCatured = true;
				}
			}

			Assert("A new record with a duplicated AH_ConsolidatedInvoiceRef should not be inserted successfully.", exceptionCatured);
		}

		public void TestDuplicatedReferenceNumberCanNotBeInserted_INV_CRD()
		{
			SetInvoiceReferenceNumberPolicy("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", true);

			string consolidatedInvoiceRef = new string('a', 6);
			string duplicatedInvoiceRef = new string('a', 6);

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AP', 'INV', '00001000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '" + consolidatedInvoiceRef + @"')");

			bool exceptionCatured = false;
			try
			{
				TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C1230', 'AP', 'CRD', '00002000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '" + duplicatedInvoiceRef + @"')");
			}
			catch (Exception e)
			{
				if (e.Message.Contains(SQL_ERROR_MSG))
				{
					exceptionCatured = true;
				}
			}

			Assert("A new record with a duplicated AH_ConsolidatedInvoiceRef should not be inserted successfully.", exceptionCatured);
		}

		public void TestDuplicatedReferenceNumberCanNotBeInserted_INV_ADJ()
		{
			SetInvoiceReferenceNumberPolicy("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", true);

			string consolidatedInvoiceRef = new string('a', 6);
			string duplicatedInvoiceRef = new string('a', 6);

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AP', 'INV', '00001000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '" + consolidatedInvoiceRef + @"')");

			bool exceptionCatured = false;
			try
			{
				TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C1230', 'AP', 'ADJ', '00002000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '" + duplicatedInvoiceRef + @"')");
			}
			catch (Exception e)
			{
				if (e.Message.Contains(SQL_ERROR_MSG))
				{
					exceptionCatured = true;
				}
			}

			Assert("A new record with a duplicated AH_ConsolidatedInvoiceRef should not be inserted successfully.", exceptionCatured);
		}

		public void TestDuplicatedReferenceNumberCanNotBeInserted_CRD_ADJ()
		{
			SetInvoiceReferenceNumberPolicy("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", true);

			string consolidatedInvoiceRef = new string('a', 6);
			string duplicatedInvoiceRef = new string('a', 6);
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AP', 'CRD', '00001000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '" + consolidatedInvoiceRef + @"')");

			bool exceptionCatured = false;
			try
			{
				TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C1230', 'AP', 'ADJ', '00002000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '" + duplicatedInvoiceRef + @"')");
			}
			catch (Exception e)
			{
				if (e.Message.Contains(SQL_ERROR_MSG))
				{
					exceptionCatured = true;
				}
			}

			Assert("A new record with a duplicated AH_ConsolidatedInvoiceRef should not be inserted successfully.", exceptionCatured);
		}

		public void TestPerformance_InsertWithAllAPLedgerAndBlankValueShouldNotReadRows()
		{
			InsertBulkTestRowsForPerformanceTest();

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				TestConnection.ExecuteReader(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C1230', 'AP', 'INV', '00002000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1231', 'AP', 'INV', '00002001', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1232', 'AP', 'INV', '00002002', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '');

SELECT 1;",
	 (_) => { });

				var queryAndPlans = TestConnection.ExecutedCommandsAndQueryPlans.First();
				var plan = queryAndPlans.Item2.FirstOrDefault(p => p.Contains("ShareSequentialInvoiceReferenceNumbers"));
				AssertNotNull("Precondition: Query to detect duplicate AH_ConsolidatedInvoiceRef must be executed", plan);

				var queryPlanAnalyzer = new QueryPlanalyzer(plan);
				Assert("Should be no index scans", !queryPlanAnalyzer.IndexScans.Any());
				Assert("Should be no table scans", !queryPlanAnalyzer.TableScans.Any());
				var seek = queryPlanAnalyzer.IndexSeeks.Single(x => x.IndexName == "NR_RX__" + AccTransactionHeaderSchema.Constants.AH_ConsolidatedInvoiceRef);
				AssertEquals("No rows should be read when all invoice refs are empty", null, seek.ActualRowsRead);
				AssertEquals("No seeks should happen when all invoice refs are empty", 0L, seek.ActualScans);
			}
		}

		public void TestPerformance_InsertWithAllARLedgerShouldNotReadRows()
		{
			InsertBulkTestRowsForPerformanceTest();

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				TestConnection.ExecuteReader(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C1230', 'AR', 'INV', '00002000', 1, 'AR INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1231', 'AR', 'INV', '00002001', 1, 'AR INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1232', 'AR', 'INV', '00002002', 1, 'AR INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '');

SELECT 1;",
	 (_) => { });

				var queryAndPlans = TestConnection.ExecutedCommandsAndQueryPlans.First();
				var plan = queryAndPlans.Item2.FirstOrDefault(p => p.Contains("ShareSequentialInvoiceReferenceNumbers"));
				AssertNotNull("Precondition: Query to detect duplicate AH_ConsolidatedInvoiceRef must be executed", plan);

				var queryPlanAnalyzer = new QueryPlanalyzer(plan);
				Assert("Should be no index scans", !queryPlanAnalyzer.IndexScans.Any());
				Assert("Should be no table scans", !queryPlanAnalyzer.TableScans.Any());
				var seek = queryPlanAnalyzer.IndexSeeks.Single(x => x.IndexName == "NR_RX__" + AccTransactionHeaderSchema.Constants.AH_ConsolidatedInvoiceRef);
				AssertEquals("No rows should be read when all invoice refs are empty", null, seek.ActualRowsRead);
				AssertEquals("No seeks should happen when all invoice refs are empty", 0L, seek.ActualScans);
			}
		}

		public void TestPerformance_InsertWithMixedLedgerAndSomeBlankValueShouldReadRowsForAPAndInvoiceRef()
		{
			InsertBulkTestRowsForPerformanceTest();

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				TestConnection.ExecuteReader(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C1230', 'AP', 'INV', '00002000', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1231', 'AP', 'INV', '00002001', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '1_')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1232', 'AP', 'INV', '00002002', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '2_')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1233', 'AR', 'INV', '00003000', 1, 'AR INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '')
     ,('591E6B52-95DF-42F8-8972-53FFF59C1234', 'AR', 'INV', '00003001', 1, 'AR INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '3_');

SELECT 1;",
	 (_) => { });

				var queryAndPlans = TestConnection.ExecutedCommandsAndQueryPlans.First();
				var plan = queryAndPlans.Item2.FirstOrDefault(p => p.Contains("ShareSequentialInvoiceReferenceNumbers"));
				AssertNotNull("Precondition: Query to detect duplicate AH_ConsolidatedInvoiceRef must be executed", plan);

				var queryPlanAnalyzer = new QueryPlanalyzer(plan);
				Assert("Should be no index scans", !queryPlanAnalyzer.IndexScans.Any());
				Assert("Should be no table scans", !queryPlanAnalyzer.TableScans.Any());
				var seek = queryPlanAnalyzer.IndexSeeks.Single(x => x.IndexName == "NR_RX__" + AccTransactionHeaderSchema.Constants.AH_ConsolidatedInvoiceRef);
				AssertEquals("One row should be read per AP+non-empty invoice ref", 2L, seek.ActualRowsRead);
				AssertEquals("One seek should happen per AP+non-empty invoice ref", 2L, seek.ActualScans);
			}
		}

		const string SQL_ERROR_MSG = "Internal Reference is duplicated.";

		#region helpers

		void SetInvoiceReferenceNumberPolicy(string owner, bool share)
		{
			string setting = string.Format(CultureInfo.InvariantCulture, @"<?xml version=""1.0"" encoding=""utf-16""?><ShareSequentialReferenceNumbers><Value>{0}</Value></ShareSequentialReferenceNumbers>", share ? "Y" : "N");

			string sql = string.Format(CultureInfo.InvariantCulture, "INSERT INTO dbo.StmData(SD_PK, SD_BinaryValue, SD_Owner, SD_Name) VALUES(NEWID(), CONVERT(varbinary(max), N'{0}'), '{1}', 'ShareSequentialInvoiceReferenceNumbers')", setting, owner);
			TestConnection.ExecuteNonQuery(sql);
		}

		void InsertBulkTestRowsForPerformanceTest(int countOfRowsWithConsolidatedInvoiceRefValue = 100, int countOfRowsWithEmptyConsolidatedInvoiceRef = 250)
		{
			var sql = new System.Text.StringBuilder();
			sql.AppendLine("INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference, AH_ConsolidatedInvoiceRef) VALUES");

			var consolidatedInvoiceRefValues = Enumerable.Range(1, countOfRowsWithConsolidatedInvoiceRefValue).Select(x => "" + x.ToString("0000", CultureInfo.InvariantCulture));
			foreach (var v in consolidatedInvoiceRefValues)
			{
				sql.AppendLine($"(NEWID(), 'AP', 'INV', 'VALUE{v}', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', '{v}'),");
			}
			for (int i = 0; i < countOfRowsWithEmptyConsolidatedInvoiceRef; i++)
			{
				sql.AppendLine($"(NEWID(), 'AP', 'INV', 'BLANK{i:0000}', 1, 'AP INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '000000', ''),");
			}
			sql.Remove(sql.Length - 3, 3);

			TestConnection.ExecuteNonQuery(sql.ToString());
			TestConnection.ExecuteNonQuery("UPDATE STATISTICS AccTransactionHeader WITH FULLSCAN");
		}

		#endregion
	}
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.Accounting.Netting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	[GuiTest]
	sealed class PaymentReceiptRemittanceDataImporterTest : FlatFileDataImporterTestCase
	{
		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataWithSavingError()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2009, 01, 01));
			TestObjectCreator.ABIGAS.OH_IsCreditor = true;
			TestObjectCreator.AALSHI.OH_IsDebtor = true;

			InvoicingBase invoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV123456", TestObjectCreator.AUD, 1M);
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.AUD, 1M, 100M, 0M, 0M);
			InvoicingBase invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV987654", TestObjectCreator.AUD, 1M);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 300M, 0M, 0M);
			InvoicingBase invoice3 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV2", TestObjectCreator.AUD, 1M);
			invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceLine(invoice3, TestObjectCreator.AUD, 1M, 100M, 0M, 0M);

			InvoicingBase invoice4 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", TestObjectCreator.USD, 0.67M);
			invoice4.AH_OH = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.CreateInvoiceLine(invoice4, TestObjectCreator.USD, 0.67M, 1500M, 0M, 0M);
			InvoicingBase invoice5 = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "00001000", TestObjectCreator.USD, 0.67M);
			invoice5.AH_OH = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceLine(invoice5, TestObjectCreator.USD, 0.67M, 800M, 0M, 0M);
			InvoicingBase invoice6 = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "apcredit", TestObjectCreator.USD, 0.67M);
			invoice6.AH_OH = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.CreateInvoiceLine(invoice6, TestObjectCreator.USD, 0.67M, 300M, 0M, 0M);

			Factory.Save();

			using (var reader = new StreamReader(PathToTestFile))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();

				TxnHeaderProcessorBase.SimulateSavingFailure = true;

				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@$"Begin processing Transaction AP PAY ABIGAS AUD 1: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Test saving error
  This transaction has errors and was not imported.
Begin processing Transaction AR REC AALSHI USD 103: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.");
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			setupInvoicesForImport();
			assertMatchedInvoices();
		}

		[TestDate(2015, 01, 16)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportNettingClearingJournal_Underpayment()
		{
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.AALSHI.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());

			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			var nettingSystem = testObjectCreator.CreateNettingSystem("NS", "Netting System", GlbCompany.CurrentCompany);

			var org = testObjectCreator.CreateOrgHeader("TSTORG1", true, true);
			org.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "EDIHYEDAU");

			testObjectCreator.CreateNettingOrganisation(nettingSystem, org, "FUL");

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			org.Addresses.Add(address);

			var invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "1003", testObjectCreator.AUD, 1M, org);
			invoice1.AH_ChequeOrReference = "1003";
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 499.99M, 0M, 0M);

			Factory.Save();

			using (StreamReader reader = new StreamReader(this.PathToTestDir + "NettingClearingRemittanceFile_UnderPayment.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_OH, org.PK);
				query.OrderBy = "AH_SystemCreateTimeUtc";

				APJournal[] apJournal = Factory.Load<APJournal>(query);
				AssertEquals("One journal created against participant", 1, apJournal.Length);
				AssertEquals("Journal created with full amount", 500M, apJournal[0].AH_OSTotalAmount);
				AssertEquals("Journal partially matched aganist the AP invoice", 0.01M, apJournal[0].AH_Calc_OSOutstandingAmount);
				AssertEquals(nameof(DebitCredit.CR), apJournal[0].DebitCreditSign);
				AssertEquals(AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), apJournal[0].AH_AG);

				AssertEquals("AP invoice is fully paid", 0M, invoice1.AH_OutstandingAmount);
			}
		}

		[TestDate(2015, 01, 16)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportNettingClearingJournal_OverPayment()
		{
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.AALSHI.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());

			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			var nettingSystem = testObjectCreator.CreateNettingSystem("NS", "Netting System", GlbCompany.CurrentCompany);

			var org = testObjectCreator.CreateOrgHeader("TSTORG1", true, true);
			org.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "EDIHYEDAU");

			testObjectCreator.CreateNettingOrganisation(nettingSystem, org, "FUL");

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			org.Addresses.Add(address);

			var invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "1003", testObjectCreator.AUD, 1M, org);
			invoice1.AH_ChequeOrReference = "1003";
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 500.01M, 0M, 0M);

			Factory.Save();

			using (StreamReader reader = new StreamReader(this.PathToTestDir + "NettingClearingRemittanceFile_OverPayment.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				var expectedErrorMessage = @"Error: Transaction AP JNL ZTSTORG1  : 
OSPartialPaymentAmount: Pay Amount must be between 0 and 500.00
  This transaction has errors and was not imported.";

				AssertContains(expectedErrorMessage, notificationBuffer.AsString);

				APJournal[] journal = Factory.Load<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals("No journals were created", 0, journal.Length);
			}
		}

		[TestDate(2015, 01, 16)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportNettingClearingJournal_Success()
		{
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = testObjectCreator.AALSHI.PK;

			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.AALSHI.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());

			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			var nettingSystem = testObjectCreator.CreateNettingSystem("NS", "Netting System", GlbCompany.CurrentCompany);

			var org = testObjectCreator.CreateOrgHeader("TSTORG1", true, true);
			org.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "EDIHYEDAU");

			testObjectCreator.CreateNewCompany("AU1", orgProxy: org);
			testObjectCreator.CreateNettingOrganisation(nettingSystem, org, "FUL");

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			org.Addresses.Add(address);

			var invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "00001003", testObjectCreator.AUD, 1M, org);
			invoice1.AH_ChequeOrReference = "00001003";
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 300M, 0M, 0M);

			var invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "00001004", testObjectCreator.AUD, 1M, org);
			invoice2.AH_ChequeOrReference = "00001004";
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.AUD, 1M, 200M, 0M, 0M);

			var invoice3 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "00001005", testObjectCreator.AUD, 1M, org);
			invoice3.AH_ChequeOrReference = "00001005";
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.AUD, 1M, 300M, 0M, 0M);

			var invoice4 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "00001006", testObjectCreator.AUD, 1M, org);
			invoice4.AH_ChequeOrReference = "00001006";
			testObjectCreator.CreateInvoiceLine(invoice4, testObjectCreator.AUD, 1M, 200M, 0M, 0M);

			Factory.Save();

			using (StreamReader reader = new StreamReader(this.PathToTestDir + "NettingClearingRemittanceFile.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_OH, org.PK);

				APJournal[] apJournal = Factory.Load<APJournal>(query);
				AssertEquals("1 AP journal created against participant", 1, apJournal.Length);
				AssertEquals(org.PK, apJournal[0].AH_OH);
				AssertEquals(500M, apJournal[0].AH_OSTotalAmount);
				AssertEquals(500M, apJournal[0].AH_LocalTotalAmount);
				AssertEquals(0M, apJournal[0].AH_Calc_OSOutstandingAmount);
				AssertEquals(nameof(DebitCredit.CR), apJournal[0].DebitCreditSign);
				AssertEquals(AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), apJournal[0].AH_AG);

				TransactionMatchLink matchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, apJournal[0].PK));
				AssertNotNull(matchLink);
				TransactionMatchLink invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice1.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);

				AssertEquals(AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), apJournal[0].AH_AG);

				AssertEquals(0M, invoice1.AH_OutstandingAmount);
				AssertEquals(0M, invoice2.AH_OutstandingAmount);

				query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_OH, org.PK);

				ARJournal[] arJournal = Factory.Load<ARJournal>(query);
				AssertEquals("1 Journal created against participant", 1, arJournal.Length);
				AssertEquals(org.PK, arJournal[0].AH_OH);
				AssertEquals(600M, arJournal[0].AH_OSTotalAmount);
				AssertEquals(600M, arJournal[0].AH_LocalTotalAmount);
				AssertEquals(100M, arJournal[0].AH_Calc_OSOutstandingAmount);
				AssertEquals(nameof(DebitCredit.CR), arJournal[0].DebitCreditSign);
				AssertEquals(AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), arJournal[0].AH_AG);

				matchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, arJournal[0].PK));
				AssertNotNull(matchLink);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice3.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice4.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);

				query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(AccTransactionHeaderSchema.AH_OH, testObjectCreator.AALSHI.PK);
				query.OrderBy = "AH_SystemCreateTimeUtc";

				arJournal = Factory.Load<ARJournal>(query);
				AssertEquals("1 Journals created against netting system org", 1, arJournal.Length);
				AssertEquals(testObjectCreator.AALSHI.PK, arJournal[0].AH_OH);

				AssertEquals(0M, invoice3.AH_OutstandingAmount);
				AssertEquals(0M, invoice4.AH_OutstandingAmount);

				AssertEquals(testObjectCreator.AALSHI.PK, arJournal[0].AH_OH); //JNL created against the netting system org header
				AssertEquals(200M, arJournal[0].AH_OSTotalAmount);
				AssertEquals(200M, arJournal[0].AH_LocalTotalAmount);
				AssertEquals(200M, arJournal[0].AH_Calc_OSOutstandingAmount);
				AssertEquals(nameof(DebitCredit.CR), arJournal[0].DebitCreditSign);
				AssertEquals(AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), arJournal[0].AH_AG);
				AssertEquals("FX Request", arJournal[0].AH_Desc);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataPaymentReceiptOrgValidation()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			TestObjectCreator.ABIGAS.CompanyData.OB_IsCreditor = false;
			TestObjectCreator.AALSHI.CompanyData.OB_IsDebtor = false;

			Factory.Save();

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "ARCREDIT12345", testObjectCreator.USD, 0.67M);
			invoice1.AH_ChequeOrReference = "ARCREDIT12345";
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.USD, 0.67M, 500M, 0M, 0M);

			InvoicingBase invoice2 = testObjectCreator.CreateInvoice(typeof(APCreditNote), "APCREDIT12345", testObjectCreator.USD, 0.67M);
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.USD, 0.67M, 700M, 0M, 0M);

			Factory.Save();

			Assert(!TestObjectCreator.ABIGAS.CompanyData.OB_IsCreditor);
			Assert(!TestObjectCreator.AALSHI.CompanyData.OB_IsDebtor);

			using (StreamReader reader = new StreamReader(this.PathToTestDir + "ARPayAPRecRemittance.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert("Should have no errors", !notificationBuffer.HasErrors);
				Assert("Should have no warnings", !notificationBuffer.HasWarnings);
			}
		}

		[TestDate(2009, 01, 15)]
		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportPayment_NoCriticalValidationDueToOutstandingAmountOutOfSyncWithLocalAmount()
		{
			using (var reader = new StreamReader(PathToTestDir + "APPAY_OutstandingAmountNotOutOfSyncWithInvoiceAmount.csv"))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestDir, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert("Should not have any error", !notificationBuffer.HasErrors);
				Assert("Should not have any warning", !notificationBuffer.HasWarnings);
			}
		}

		[TestDate(2009, 01, 15)]
		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataPaymentReceiptWithExceedLengthDesc()
		{
			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (StreamReader reader = new StreamReader(this.PathToTestDir + "ARPayAPRecRemittanceWithExceedLengthDesc.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);
				Assert("Should have exceed length warning", notificationBuffer.HasWarnings);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataPaymentReceiptWithChequeOrReferenceExceedingLength()
		{
			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (StreamReader reader = new StreamReader(this.PathToTestDir + "ARPayAPRecRemittanceWithExceedChequeOrReference.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);
				Assert("Should have warning", notificationBuffer.HasWarnings);
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@$"Begin processing Transaction AR PAY ABIGAS AUD 1: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Warning: Maximum length of this field has been exceeded (Cheque or Reference; value=ThisIsALongChequeReferenceThisIsALongChequeReference)
  Completed Processing Transaction.");
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataPaymentReceiptOrgValidationError()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			TestObjectCreator.ABIGAS.CompanyData.OB_IsDebtor = false;
			TestObjectCreator.AALSHI.CompanyData.OB_IsCreditor = false;

			Factory.Save();

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "ARCREDIT12345", testObjectCreator.USD, 0.67M);
			invoice1.AH_ChequeOrReference = "ARCREDIT12345";
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.USD, 0.67M, 500M, 0M, 0M);

			InvoicingBase invoice2 = testObjectCreator.CreateInvoice(typeof(APCreditNote), "APCREDIT12345", testObjectCreator.USD, 0.67M);
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.USD, 0.67M, 700M, 0M, 0M);

			Factory.Save();

			Assert(!TestObjectCreator.ABIGAS.CompanyData.OB_IsDebtor);
			Assert(!TestObjectCreator.AALSHI.CompanyData.OB_IsCreditor);

			using (StreamReader reader = new StreamReader(this.PathToTestDir + "ARPayAPRecRemittance.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert("Should have errors", notificationBuffer.HasErrors);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoJournalDoesNotValidateSubAccountParentTableId()
		{
			SetupExchangeBuyRate(Factory, "AUD", 1m);

			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AccGLHeader apControlAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)));
			AccGLHeader arControlAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)));

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "ARCREDIT12345", testObjectCreator.AUD, 1M);
			invoice1.AH_ChequeOrReference = "ARCREDIT12345";
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 300M, 0M, 0M);

			InvoicingBase invoice2 = testObjectCreator.CreateInvoice(typeof(APCreditNote), "APCREDIT12345", testObjectCreator.AUD, 1M);
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.AUD, 1M, 700M, 0M, 0M);

			Factory.Save();

			using (StreamReader reader = new StreamReader(this.PathToTestDir + "ARPayAPRecRemittanceLocalCurrencyUnmatchedAmount.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert("Should have no errors", !notificationBuffer.HasErrors);
				Assert("Should have no warnings", !notificationBuffer.HasWarnings);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoJournalDoesNotValidateSubAccountParentTableIdWithMultipleSubAccounts()
		{
			SetupExchangeBuyRate(Factory, "AUD", 1m);

			using (AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AccGLHeader apControlAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)));
				TestObjectCreator.CreateGLHeaderSubAccount(apControlAccount, OrgHeaderSchema.Constants.Prefix, true);
				TestObjectCreator.CreateGLHeaderSubAccount(apControlAccount, AccGroupsSchema.Constants.Prefix, true);
				TestObjectCreator.CreateGLHeaderSubAccount(apControlAccount, GlbGroupSchema.Constants.Prefix, true);

				AccGLHeader arControlAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)));
				TestObjectCreator.CreateGLHeaderSubAccount(arControlAccount, OrgHeaderSchema.Constants.Prefix, true);
				TestObjectCreator.CreateGLHeaderSubAccount(arControlAccount, AccGroupsSchema.Constants.Prefix, true);
				TestObjectCreator.CreateGLHeaderSubAccount(arControlAccount, GlbGroupSchema.Constants.Prefix, true);

				InvoicingBase invoice1 = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "ARCREDIT12345", TestObjectCreator.AUD, 1M);
				invoice1.AH_ChequeOrReference = "ARCREDIT12345";
				invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
				TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.AUD, 1M, 300M, 0M, 0M);

				InvoicingBase invoice2 = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "APCREDIT12345", TestObjectCreator.AUD, 1M);
				invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
				TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 600M, 0M, 0M);

				Factory.Save();

				var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
				var journalCountBeforeImport = Factory.Load<AccTransactionHeader>(query).Length;

				using (StreamReader reader = new StreamReader(this.PathToTestDir + "ARPayAPRecRemittanceLocalCurrencyUnmatchedAmount.csv"))
				{
					PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);
					AssertEquals("Created 4 Journal: ", 4, Factory.Load<AccTransactionHeader>(query).Length - journalCountBeforeImport);

					var createdAPJournals = Factory.Load<AccTransactionHeader>(query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable));
					AssertEquals("Created AP Journal's GL Header", apControlAccount.PK, createdAPJournals[0].AH_AG);
					AssertEquals("Created AP Journal's GL Header", apControlAccount.PK, createdAPJournals[1].AH_AG);

					var createdARJournals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal).AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable));
					AssertEquals("Created AR Journal's GL Header", arControlAccount.PK, createdARJournals[0].AH_AG);
					AssertEquals("Created AR Journal's GL Header", arControlAccount.PK, createdARJournals[1].AH_AG);

					Assert("Should have no errors", !notificationBuffer.HasErrors);
					Assert("Should have no warnings", !notificationBuffer.HasWarnings);
				}
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataStopMatchingWhenPTRLineNotFound()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			var invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV987654", testObjectCreator.AUD, 1M);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.AUD, 1M, 300M, 0M, 0M);
			var invoice3 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV2", testObjectCreator.AUD, 1M);
			invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.AUD, 1M, 100M, 0M, 0M);

			var invoice4 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", testObjectCreator.USD, 0.67M);
			invoice4.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice4, testObjectCreator.USD, 0.67M, 1700M, 0M, 0M);
			var invoice5 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "00001000", testObjectCreator.USD, 0.67M);
			invoice5.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice5, testObjectCreator.USD, 0.67M, 800M, 0M, 0M);

			Factory.Save();

			using (var reader = new StreamReader(PathToTestFile))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				var payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(0, payment.Length);

				var invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2.PK));
				AssertNull(invoiceMatchLink);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice3.PK));
				AssertNull(invoiceMatchLink);

				var receipt = Factory.Load<ARReceipt>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(0, receipt.Length);

				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice4.PK));
				AssertNull(invoiceMatchLink);
			}
		}

		void SetupOverpaidTestData()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			InvoicingBase invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV987654", testObjectCreator.AUD, 1M);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.AUD, 1M, 300M, 0M, 0M);
			InvoicingBase invoice3 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV2", testObjectCreator.AUD, 1M);
			invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.AUD, 1M, 90M, 0M, 0M);

			InvoicingBase invoice4 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", testObjectCreator.USD, 0.67M);
			invoice4.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice4, testObjectCreator.USD, 0.67M, 1000M, 0M, 0M);
			InvoicingBase invoice5 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "00001000", testObjectCreator.USD, 0.67M);
			invoice5.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice5, testObjectCreator.USD, 0.67M, 800M, 0M, 0M);

			Factory.Save();
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataOverPaid()
		{
			SetupOverpaidTestData();

			using (var reader = new StreamReader(PathToTestDir + "PayRecRemittanceOverPaidWithError.csv"))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@$"Begin processing Transaction AP PAY ABIGAS AUD 1: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Paid Transaction AP INV APINV123456: Matching failed. Transaction was not found.
Error: Transaction AP PAY ABIGAS AUD 1: 
OSPartialPaymentAmount: Pay Amount must be between 0 and 500
Error: Transaction AP INV APINV987654: 
OSPartialPaymentAmount: Pay Amount must be between -300.00 and 0
Error: Transaction AP INV APINV2: 
OSPartialPaymentAmount: Pay Amount must be between -90.00 and 0
  This transaction has errors and was not imported.");

				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@$"Begin processing Transaction AR REC AALSHI USD 103: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Paid Transaction AP CRD apcredit: Matching failed. Transaction was not found.
Error: Transaction AR REC AALSHI USD 103: 
OSPartialPaymentAmount: Pay Amount must be between -1000.00 and 0
Error: Transaction AR INV 00001000: 
OSPartialPaymentAmount: Pay Amount must be between 0 and 1000.00
  This transaction has errors and was not imported.");
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataOverPaidExchangeRateError()
		{
			SetupExchangeBuyRate(Factory, "AUD", 1m);
			SetupExchangeBuyRate(Factory, "USD", 0.8m);
			SetupExchangeBuyRate(Factory, "EUR", 0.7m);

			SetupOverpaidTestData();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceOverPaid.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@$"Begin processing Transaction AP PAY ABIGAS AUD 1: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AR REC AALSHI USD 103: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Exchange Rate For Currency 'USD' Is Not Set
  This transaction has errors and was not imported.");
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataMatchingToInvoicePaidViaWebService_PartialSuccess()
		{
			AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AccGLHeader apControlAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apControlAccount.PK.ToGuid());
			AccGLHeader arControlAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arControlAccount.PK.ToGuid());

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "5541", testObjectCreator.AUD, 1M);
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 30M, 0M, 0M);

			InvoicingBase invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "5542", testObjectCreator.AUD, 1M);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.AUD, 1M, 3M, 0M, 0M);

			InvoicingBase invoice3 = testObjectCreator.CreateInvoice(typeof(APInvoice), "5543", testObjectCreator.AUD, 1M);
			invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.AUD, 1M, 30M, 0M, 0M);

			InvoicingBase invoice4 = testObjectCreator.CreateInvoice(typeof(APInvoice), "5544", testObjectCreator.AUD, 1M);
			invoice4.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice4, testObjectCreator.AUD, 1M, 3M, 0M, 0M);
			Factory.Save();

			testObjectCreator.PartPayInvoiceViaWebService(invoice1, 1); //this should make the first payment in the file to fail

			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRemittanceInvoicePaidViaWebservice.csv"))
			{
				ZInt transactionsCount = Factory.GetDatabaseCount(typeof(AccTransactionHeader));
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@$"Begin processing Transaction AP PAY ABIGAS AUD : 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Invoice 5541 is part paid via Invoice Payment Web Service and cannot be matched against.
  This transaction has errors and was not imported.
Begin processing Transaction AP PAY ABIGAS AUD : 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.");

				AssertEquals("Should be 1 transaction created", transactionsCount + 1, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataMatchingToInvoicePaidViaWebService_FullFailure()
		{
			AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AccGLHeader apControlAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apControlAccount.PK.ToGuid());
			AccGLHeader arControlAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arControlAccount.PK.ToGuid());

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "5541", testObjectCreator.AUD, 1M);
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 30M, 0M, 0M);

			InvoicingBase invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "5542", testObjectCreator.AUD, 1M);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.AUD, 1M, 3M, 0M, 0M);

			InvoicingBase invoice3 = testObjectCreator.CreateInvoice(typeof(APInvoice), "5543", testObjectCreator.AUD, 1M);
			invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.AUD, 1M, 30M, 0M, 0M);

			InvoicingBase invoice4 = testObjectCreator.CreateInvoice(typeof(APInvoice), "5544", testObjectCreator.AUD, 1M);
			invoice4.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice4, testObjectCreator.AUD, 1M, 3M, 0M, 0M);
			Factory.Save();

			testObjectCreator.PartPayInvoiceViaWebService(invoice1, 1); //this should make the first payment in the file to fail
			testObjectCreator.PartPayInvoiceViaWebService(invoice3, 1); //this should make the second payment in the file to fail

			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRemittanceInvoicePaidViaWebservice.csv"))
			{
				ZInt transactionsCount = Factory.GetDatabaseCount(typeof(AccTransactionHeader));
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@$"Begin processing Transaction AP PAY ABIGAS AUD : 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Invoice 5541 is part paid via Invoice Payment Web Service and cannot be matched against.
  This transaction has errors and was not imported.
Begin processing Transaction AP PAY ABIGAS AUD : 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Invoice 5543 is part paid via Invoice Payment Web Service and cannot be matched against.
  This transaction has errors and was not imported.");

				AssertEquals("Should be no transaction created", transactionsCount, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataUnapprovedPaymentError()
		{
			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccGLHeader apControlAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apControlAccount.PK.ToGuid());
			AccGLHeader arControlAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arControlAccount.PK.ToGuid());

			InvoicingBase invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV987654", testObjectCreator.AUD, 1M);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1M, 500M, 0M, 0M);

			AccPaymentApproval paymentApproval = Factory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval.AV_Ledger = "AP";

			AccPaymentApprovalItem paymentApprovalItem = Factory.NewWithValidTestData<AccPaymentApprovalItem>();
			paymentApprovalItem.A2_AH = invoice.PK;
			paymentApprovalItem.A2_AV = paymentApproval.PK;
			paymentApprovalItem.A2_PaymentThisRun = -300M;
			Factory.Save();

			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceOverPaid.csv"))
			{
				ZInt transactionsCount = Factory.GetDatabaseCount(typeof(AccTransactionHeader));
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@$"Begin processing Transaction AP PAY ABIGAS AUD 1: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Invoice APINV987654 is being paid by an unapproved payment.
Revise the file to exclude this invoice, or remove the invoice from the unapproved payment.");

				AssertEquals("Should be no transactions created", transactionsCount, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			}
		}

		[ExpectNoExceptions]
		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataEmptyTransactionNumberError()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			var invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "5541", testObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 30M, 0M, 0M);
			Factory.Save();

			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRemittanceWithoutTransactionNumber.csv"))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@"Begin processing Transaction AP PAY ABIGAS AUD : 
Error: There are Paid Transaction without Transaction Number specified:
    AP,INV,,3,ABIGAS,5541
  This transaction has errors and was not imported.");
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataPayRecOrgContainPtrOrg()
		{
			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			AccGLHeader apControlAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)));

			var invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "5541", testObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 50M, 0M, 0M);
			Factory.Save();

			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRemittancePayRecOrgContainPtrOrg.csv"))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(2, journals.Length);
				AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "5541", 1, 30, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
				AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "5541", 1, -30, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);

				APPayment[] payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(1, payment.Length);
				AssertEquals("Amount", 100m, payment[0].AH_OSTotalAmount);
				AssertEquals("OutstandingAmount", 67m, payment[0].AH_OutstandingAmount);

				APInvoice[] invoice = Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(1, invoice.Length);
				AssertEquals("Amount", 50m, invoice[0].AH_OSTotalAmount);
				AssertEquals("OutstandingAmount", -47m, invoice[0].AH_OutstandingAmount);
			}
		}

		[TestDate(2009, 01, 15)]
		public void TestGetMatchedTransaction()
		{
			var adapter = new RemittanceFileImportAdapter();
			var map = new Dictionary<BusinessObject, ZDecimal>();

			var transactionAlreadyMatched = Factory.NewWithValidTestData<APInvoice>();
			transactionAlreadyMatched.AH_OH = TestObjectCreator.AALSHI.PK;
			transactionAlreadyMatched.AH_TransactionNum = "Test001";

			var transactionNotMatched = Factory.NewWithValidTestData<APInvoice>();
			transactionNotMatched.AH_OH = TestObjectCreator.ABIGAS.PK;
			transactionNotMatched.AH_TransactionNum = "Test001";

			Factory.Save();

			AssertEquals(0, map.Count);

			var query = new ZQuery().AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, "Test001");
			var result = TxnHeaderProcessorBase.GetMatchedTransaction_ForTest(Factory.CreateNewFactory(), map, query) as BusinessObject;
			AssertEquals("Should find any tranaction with given transaction number.", true, new[] { transactionAlreadyMatched.PK, transactionNotMatched.PK }.Contains(result.PK));

			map.Add(transactionAlreadyMatched, 0m);
			AssertEquals(1, map.Count);

			query = new ZQuery().AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, "Test001");
			result = TxnHeaderProcessorBase.GetMatchedTransaction_ForTest(Factory.CreateNewFactory(), map, query) as BusinessObject;
			AssertEquals("Should find tranaction not matched yet.", transactionNotMatched.PK, result.PK);

			for (int i = 0; i < 25; i++)
			{
				map.Add(Factory.NewWithValidTestData<APInvoice>(), 0m);
			}
			AssertEquals(26, map.Count);

			query = new ZQuery().AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, "Test001");
			result = TxnHeaderProcessorBase.GetMatchedTransaction_ForTest(Factory.CreateNewFactory(), map, query) as BusinessObject;
			AssertEquals("Should find tranaction not matched yet.", transactionNotMatched.PK, result.PK);
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataOverPaidNoErrors()
		{
			try
			{
				SetupExchangeBuyRate(Factory, "AUD", 1m);
				SetupExchangeBuyRate(Factory, "USD", 0.67m);
				SetupExchangeBuyRate(Factory, "USD", 1.39m, Core.Constants.ExchangeRateTypes.Code.SellRate);

				SetupOverpaidTestData();

				InvoicingBase creditNote = testObjectCreator.CreateInvoice(typeof(APCreditNote), "apcredit", testObjectCreator.USD, 0.67M);
				creditNote.AH_OH = TestObjectCreator.AALSHI.PK;

				//testObjectCreator.USD.ExchangeRates[0].RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceOverPaid.csv"))
				{
					AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

					PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);

					AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
					AssertEquals(12, journals.Length);

					Guid apControlAccountPK = (Guid)AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.Value;
					Guid arControlAccountPK = (Guid)AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.Value;

					AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "APINV123456", 1, 100, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "APINV987654", 1, 200, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);

					AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "APINV123456", 1, -100, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "APINV987654", 1, -200, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);

					AssertJournalExist(journals, "AR", TestObjectCreator.AALSHI.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AR JOURNAL", "00001000", 0.669999, -1000, arControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
					AssertJournalExist(journals, "AP", TestObjectCreator.AALSHI.PK, new ZDateTime(2009, 01, 01), new ZDateTime(2009, 01, 02), new ZDateTime(2009, 01, 03),
						"USD", "Balancing Journal", "PAY REF 003", 0.5, -300, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
					AssertJournalExist(journals, "AR", TestObjectCreator.AALSHI.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AR JOURNAL", "NoMatching", 1.389999, -2000, arControlAccountPK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AP", TestObjectCreator.AALSHI.PK, new ZDateTime(2009, 01, 01), new ZDateTime(2009, 01, 02), new ZDateTime(2009, 01, 03),
						"EUR", "Balancing Journal", "PAY REF 003", 0.5, -300, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionNotFound);

					AssertJournalExist(journals, "AR", TestObjectCreator.AALSHI.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AR JOURNAL", "00001000", 0.669999, 1000, arControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
					AssertJournalExist(journals, "AP", TestObjectCreator.AALSHI.PK, new ZDateTime(2009, 01, 01), new ZDateTime(2009, 01, 02), new ZDateTime(2009, 01, 03),
						"USD", "Balancing Journal", "PAY REF 003", 0.5, 300, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
					AssertJournalExist(journals, "AR", TestObjectCreator.AALSHI.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AR JOURNAL", "NoMatching", 1.389999, 2000, arControlAccountPK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AP", TestObjectCreator.AALSHI.PK, new ZDateTime(2009, 01, 01), new ZDateTime(2009, 01, 02), new ZDateTime(2009, 01, 03),
						"EUR", "Balancing Journal", "PAY REF 003", 0.5, 300, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionNotFound);
				}
			}
			finally
			{
				testObjectCreator.USD.ExchangeRates[0].RE_ExRateType = Constants.ExchangeRateTypes.Code.CustomsRate;
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataOverPaidBalancingJournalNoGLAccountError()
		{
			try
			{
				SetupExchangeBuyRate(Factory, "AUD", 1m);
				SetupExchangeBuyRate(Factory, "USD", 0.8m);

				SetupOverpaidTestData();

				AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
				AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

				InvoicingBase creditNote = testObjectCreator.CreateInvoice(typeof(APCreditNote), "apcredit", testObjectCreator.USD, 0.67M);
				creditNote.AH_OH = TestObjectCreator.AALSHI.PK;

				testObjectCreator.USD.ExchangeRates[0].RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceOverPaid.csv"))
				{
					PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

					Assert(notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);

					NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
	@"Error: Transaction AP JNL : 
AH_AG: Please enter a GL Account.
Error: Transaction AP JNL : 
AH_AG: Please enter a GL Account.
  This transaction has errors and was not imported.");

					NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
	@"Error: Transaction AR JNL : 
AH_AG: Please enter a GL Account.
Error: Transaction AR JNL : 
AH_AG: Please enter a GL Account.
  This transaction has errors and was not imported.");

					AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
					AssertEquals("No Journals should be saved", 0, journals.Length);
				}
			}
			finally
			{
				testObjectCreator.USD.ExchangeRates[0].RE_ExRateType = Constants.ExchangeRateTypes.Code.CustomsRate;
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}
		}

		[TestDate(2015, 11, 05)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataOverPaidInvoicesFound()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var apControlAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)));
			var arControlAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)));

			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			SetupExchangeBuyRate(Factory, "AUD", 1m);
			SetupExchangeBuyRate(Factory, "USD", 0.8m);

			var apInvoice = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV001001", testObjectCreator.USD, 0.8M, 100M, 0M, 125M, 0M, testObjectCreator.AALSHI, testObjectCreator.GLHeader1.PK
				, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today, false);
			Factory.Save();

			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceOverPaid2.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));

				AssertJournalExist(journals, "AP", TestObjectCreator.AALSHI.PK, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today, "USD", "AP JOURNAL", "INV001001", 0.8M, 10M, 12.5M, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
				AssertJournalExist(journals, "AP", TestObjectCreator.AALSHI.PK, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today, "USD", "AP JOURNAL", "INV001001", 0.8M, -10M, -12.5M, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataOverPaidNoInvoicesFoundNoErrors()
		{
			try
			{
				AccGLHeader apControlAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)));
				AccGLHeader arControlAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)));

				testObjectCreator.USD.ExchangeRates[0].RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceOverPaid.csv"))
				{
					AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

					PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);

					AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
					AssertEquals(16, journals.Length);

					AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "APINV123456", 1, 100, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "APINV987654", 1, 500, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "APINV2", 1, 50, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);

					AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "APINV123456", 1, -100, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "APINV987654", 1, -500, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "APINV2", 1, -50, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);

					AssertJournalExist(journals, "AR", TestObjectCreator.AALSHI.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AR JOURNAL", "00001000", 1.389999, -2000, arControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AR", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AR JOURNAL", "00001000", 1.389999, 800, arControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AP", TestObjectCreator.AALSHI.PK, new ZDateTime(2009, 01, 01), new ZDateTime(2009, 01, 02), new ZDateTime(2009, 01, 03),
						"USD", "Balancing Journal", "PAY REF 003", 0.5, -300, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AR", TestObjectCreator.AALSHI.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AR JOURNAL", "NoMatching", 1.389999, -2000, arControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AP", TestObjectCreator.AALSHI.PK, new ZDateTime(2009, 01, 01), new ZDateTime(2009, 01, 02), new ZDateTime(2009, 01, 03),
						"EUR", "Balancing Journal", "PAY REF 003", 0.5, -300, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);

					AssertJournalExist(journals, "AR", TestObjectCreator.AALSHI.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AR JOURNAL", "00001000", 1.389999, 2000, arControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AR", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AR JOURNAL", "00001000", 1.389999, -800, arControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AP", TestObjectCreator.AALSHI.PK, new ZDateTime(2009, 01, 01), new ZDateTime(2009, 01, 02), new ZDateTime(2009, 01, 03),
						"USD", "Balancing Journal", "PAY REF 003", 0.5, 300, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AR", TestObjectCreator.AALSHI.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AR JOURNAL", "NoMatching", 1.389999, 2000, arControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);
					AssertJournalExist(journals, "AP", TestObjectCreator.AALSHI.PK, new ZDateTime(2009, 01, 01), new ZDateTime(2009, 01, 02), new ZDateTime(2009, 01, 03),
						"EUR", "Balancing Journal", "PAY REF 003", 0.5, 300, apControlAccount.PK, Constants.TransactionCategory.Codes.TransactionNotFound);
				}
			}
			finally
			{
				testObjectCreator.USD.ExchangeRates[0].RE_ExRateType = Constants.ExchangeRateTypes.Code.CustomsRate;
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}
		}

		void AssertJournalExist(AccTransactionHeader[] journals, string ledger, ZGuid org, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime dueDate, string currency,
			string desc, string @ref, ZDecimal exchangeRate, ZDecimal osAmount, ZDecimal localAmount, ZGuid controlAccount, string transactionCategory)
		{
			bool result = false;

			foreach (AccTransactionHeader journal in journals)
			{
				if (journal.AH_Ledger == ledger && journal.AH_OH == org && journal.AH_InvoiceDate == invoiceDate && journal.AH_PostDate == postDate && journal.AH_DueDate == dueDate
					&& journal.AH_RX_NKTransactionCurrency == currency && journal.AH_ChequeOrReference == @ref && journal.AH_ExchangeRate == exchangeRate
					&& journal.AH_OSTotal == osAmount && (localAmount == 0M || localAmount == (journal.AH_InvoiceAmount - journal.AH_GSTAmount)) && journal.AH_AG == controlAccount && journal.AH_TransactionCategory == transactionCategory)
				{
					result = true;
					break;
				}
			}

			Assert(string.Format(@"Journal doesn't exist with - 
Ledger: {0}, Org: {1}, Currency: {2}, Reference: {3}, Exchange Rate: {4}, Os Amount: {5}, Local Amount: {6} Gl Account: {7}, Category: {8}",
ledger, org, currency, @ref, exchangeRate, osAmount, localAmount, controlAccount, transactionCategory), result);
		}

		void AssertJournalExist(AccTransactionHeader[] journals, string ledger, ZGuid org, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime dueDate, string currency,
			string desc, string @ref, ZDecimal exchangeRate, ZDecimal amount, ZGuid controlAccount, string transactionCategory)
		{
			AssertJournalExist(journals, ledger, org, invoiceDate, postDate, dueDate, currency, desc, @ref, exchangeRate, amount, 0M, controlAccount, transactionCategory);
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataErrors()
		{
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceInvalidData.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@"Begin processing Transaction AR REC AALSHI USD 103: 
Error: There are duplicate Paid Transactions:
    AP,CRD,apcredit,300,
    AP,CRD,apcredit,300,ABIGAS
  This transaction has errors and was not imported.");
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@$"Begin processing Transaction AP PAY ABIGAS USD 1: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Bank Account: Bank account currency does not match the Accounts Payable Payment currency.
  This transaction has errors and was not imported.");
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_MHR()
		{
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV45647", testObjectCreator.AUD, 1M);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1M, 100M, 0M, 0M);

			var creditNote = testObjectCreator.CreateInvoice(typeof(APCreditNote), "APCRD45651", testObjectCreator.AUD, 1m);
			creditNote.AH_OH = TestObjectCreator.AALSHI.PK;
			creditNote.AH_PostDate = new ZDateTime(2009, 1, 2);
			testObjectCreator.CreateInvoiceLine(creditNote, testObjectCreator.AUD, 1m, 100m, 0m, 0m);

			Factory.Save();

			string filePath = PathToTestDir + "PayRecRemittanceWithMHR.csv";

			using (var reader = new StreamReader(filePath))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, filePath, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				var dataImportEvents = invoice.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
				AssertEquals("DIM event should not be added.", 0, dataImportEvents.Length);

				var invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice.PK));
				AssertNotNull(invoiceMatchLink);
				var creditNoteMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, creditNote.PK));
				AssertNotNull(creditNoteMatchLink);
				AssertEquals(invoiceMatchLink.AP_MatchGroupNum, creditNoteMatchLink.AP_MatchGroupNum);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataErrorsForMHR()
		{
			var invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV45647", testObjectCreator.AUD, 1M);
			invoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 100M, 0M, 0M);

			var creditNote1 = testObjectCreator.CreateInvoice(typeof(APCreditNote), "APCRD45651", testObjectCreator.AUD, 1m);
			creditNote1.AH_OH = TestObjectCreator.AALSHI.PK;
			creditNote1.AH_PostDate = new ZDateTime(2009, 1, 2);
			testObjectCreator.CreateInvoiceLine(creditNote1, testObjectCreator.AUD, 1m, 100m, 0m, 0m);

			var invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV45648", testObjectCreator.AUD, 1M);
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.AUD, 1M, 200M, 0M, 0M);

			var creditNote2 = testObjectCreator.CreateInvoice(typeof(APCreditNote), "APCRD45652", testObjectCreator.AUD, 1m);
			creditNote2.AH_OH = TestObjectCreator.AALSHI.PK;
			creditNote2.AH_PostDate = new ZDateTime(2009, 1, 2);
			testObjectCreator.CreateInvoiceLine(creditNote2, testObjectCreator.AUD, 1m, 160m, 0m, 0m);

			Factory.Save();

			using (var reader = new StreamReader(PathToTestDir + "PayRecRemittanceInvalidDataWithMHR.csv"))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, @$"Begin processing Transaction AP INV AALSHI  : 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: There are duplicate Paid Transactions:
    AP,INV,APINV45647,50,AALSHI
  This transaction has errors and was not imported.
Begin processing Transaction AP INV AALSHI  : 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Matching session:
Balance: The balance must equal 0
  This transaction has errors and was not imported.
Begin processing Transaction AP CRD AALSHI  : 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Matching session:
Balance: The balance must equal 0
  This transaction has errors and was not imported.");
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataLessThanTwoPTRLinesForMHR()
		{
			var invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV45647", testObjectCreator.AUD, 1M);
			invoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 100M, 0M, 0M);

			var creditNote = testObjectCreator.CreateInvoice(typeof(APCreditNote), "APCRD45651", testObjectCreator.AUD, 1m);
			creditNote.AH_OH = TestObjectCreator.AALSHI.PK;
			creditNote.AH_PostDate = new ZDateTime(2009, 1, 2);
			testObjectCreator.CreateInvoiceLine(creditNote, testObjectCreator.AUD, 1m, 100m, 0m, 0m);

			Factory.Save();

			string filePath = PathToTestDir + "PayRecRemittanceWithLessThanTwoPTRLinesForMHR.csv";
			var importer = new PaymentReceiptRemittanceDataImporter();
			var notificationBuffer = new NotificationBuffer();

			using (var reader = new StreamReader(filePath))
			{
				importer.ImportData(reader, filePath, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "The MHR line type must be accompanied by at least two PTR lines.");
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_StopMatchingWhenNotFoundOrFullyPaidFirstPTRLineForMHR()
		{
			string filePath = PathToTestDir + "PayRecRemittanceWithMHR.csv";

			using (var reader = new StreamReader(filePath))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, filePath, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "Error: Paid Transaction AP INV APINV45647: Matching failed. Transaction was not found.");
			}

			ClearStmDataImportHistory();

			var fullyPaidInvoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV45647", testObjectCreator.AUD, 1M);
			fullyPaidInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(fullyPaidInvoice, testObjectCreator.AUD, 1M, 100M, 0M, 0M);
			testObjectCreator.CreateInvoiceLine(fullyPaidInvoice, testObjectCreator.AUD, 1M, -100M, 0M, 0M);
			Factory.Save();

			using (var reader = new StreamReader(filePath))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, filePath, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "Error: Paid Transaction AP INV APINV45647: Matching failed. Transaction was not found.");
				var invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, fullyPaidInvoice.PK));
				AssertNull(invoiceMatchLink);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_StopMatchingWhenNotFoundOrFullyPaidAfterFirstPTRLineForMHR()
		{
			var firstPTRLine = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV45647", testObjectCreator.AUD, 1M);
			firstPTRLine.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(firstPTRLine, testObjectCreator.AUD, 1M, 100M, 0M, 0M);
			Factory.Save();

			string filePath = PathToTestDir + "PayRecRemittanceWithMHR.csv";

			using (var reader = new StreamReader(filePath))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, filePath, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, @"Error: Paid Transaction AP CRD APCRD45651: Matching failed. Transaction was not found.
Error: Matching failed. There are no matched transaction.
  This transaction has errors and was not imported.");
			}

			ClearStmDataImportHistory();
			var fullyPaidCreditNote = testObjectCreator.CreateInvoice(typeof(APCreditNote), "APCRD45651", testObjectCreator.AUD, 1m);
			fullyPaidCreditNote.AH_OH = TestObjectCreator.AALSHI.PK;
			fullyPaidCreditNote.AH_PostDate = new ZDateTime(2009, 1, 2);
			testObjectCreator.CreateInvoiceLine(fullyPaidCreditNote, testObjectCreator.AUD, 1m, 100m, 0m, 0m);
			testObjectCreator.CreateInvoiceLine(fullyPaidCreditNote, testObjectCreator.AUD, 1m, -100m, 0m, 0m);
			Factory.Save();

			using (var reader = new StreamReader(filePath))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, filePath, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, @"Error: Paid Transaction AP CRD APCRD45651: Matching failed. Transaction was not found.
Error: Matching failed. There are no matched transaction.
  This transaction has errors and was not imported.");

				var invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, fullyPaidCreditNote.PK));
				AssertNull(invoiceMatchLink);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataWithMatchStatusAndMatchStatusReasonCodeInPTR()
		{
			var invoice1 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "ARCREDIT12345", testObjectCreator.USD, 0.67M);
			invoice1.AH_ChequeOrReference = "ARCREDIT12345";
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.USD, 0.67M, 500M, 0M, 0M);

			var invoice2 = testObjectCreator.CreateInvoice(typeof(APCreditNote), "APCREDIT12345", testObjectCreator.USD, 0.67M);
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.USD, 0.67M, 700M, 0M, 0M);

			Factory.Save();

			using (StreamReader reader = new StreamReader(this.PathToTestDir + "RemittanceImportWithPTRWithMatchStatusAndMatchStatusReasonCode.csv"))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);
				Assert("Should have no errors", !notificationBuffer.HasErrors);
				Assert("Should have no warnings", !notificationBuffer.HasWarnings);

				var invoice = Factory.Load<APCreditNote>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("MatchStatus should be UAC", "UAC", invoice[0].AH_MatchStatus);
				AssertEquals("MatchStatusReasonCode should be ADV", "ADV", invoice[0].AH_MatchStatusReasonCode);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataWithMatchStatusAndMatchStatusReasonCodeErrorInPTR()
		{
			var invoice1 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "ARCREDIT12345", testObjectCreator.USD, 0.67M);
			var invoice2 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "ARCREDIT12346", testObjectCreator.USD, 0.67M);
			var invoice3 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "ARCREDIT12347", testObjectCreator.USD, 0.67M);
			var invoice4 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "ARCREDIT12348", testObjectCreator.USD, 0.67M);

			invoice1.AH_ChequeOrReference = "ARCREDIT12345";
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_ChequeOrReference = "ARCREDIT12346";
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice3.AH_ChequeOrReference = "ARCREDIT12347";
			invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice4.AH_ChequeOrReference = "ARCREDIT12348";
			invoice4.AH_OH = TestObjectCreator.ABIGAS.PK;

			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.USD, 0.67M, 500M, 0M, 0M);
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.USD, 0.67M, 500M, 0M, 0M);
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.USD, 0.67M, 500M, 0M, 0M);
			testObjectCreator.CreateInvoiceLine(invoice4, testObjectCreator.USD, 0.67M, 500M, 0M, 0M);

			Factory.Save();

			using (StreamReader reader = new StreamReader(this.PathToTestDir + "RemittanceWithMatchStatusAndMatchStatusReasonCodeHasError.csv"))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert("Should have errors", notificationBuffer.HasErrors);
				Assert("Should have no warnings", !notificationBuffer.HasWarnings);
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, @"MatchStatus: Enter a valid Match Status Code.");
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, @"MatchStatusReasonCode: Enter a valid Match Status Reason Code.");
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, @"MatchStatusReasonCode: Please enter a value.");
			}
		}

		void ClearStmDataImportHistory()
		{
			using (var cmd = Db.Connection.Command("DELETE dbo.StmDataImportHistory"))
			{
				cmd.ExecuteNonQuery();
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataMatchingErrors()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			using (var reader = new StreamReader(PathToTestFile))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@$"Begin processing Transaction AP PAY ABIGAS AUD 1: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Paid Transaction AP INV APINV123456: Matching failed. Transaction was not found.
Error: Paid Transaction AP INV APINV987654: Matching failed. Transaction was not found.
Error: Paid Transaction AP INV APINV2: Matching failed. Transaction was not found.
Error: Matching failed. There are no matched transaction.
  This transaction has errors and was not imported.");
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataMatchDataLessThanPostData()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV123456", testObjectCreator.AUD, 1M); // Matches on Transaction Num
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice1.AH_PostDate = new ZDateTime(2009, 2, 1);
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 100M, 0M, 0M);
			InvoicingBase invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV987654", testObjectCreator.AUD, 1M); // Matches on Transaction Num
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.AUD, 1M, 300M, 0M, 0M);
			InvoicingBase invoice3 = testObjectCreator.CreateInvoice(typeof(APInvoice), "XYZ789", testObjectCreator.AUD, 1M); // Matches on Payment Reference
			invoice3.AH_ChequeOrReference = "APINV2";
			invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.AUD, 1M, 100M, 0M, 0M);

			InvoicingBase invoice4 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", testObjectCreator.USD, 0.67M);
			invoice4.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice4, testObjectCreator.USD, 0.67M, 1500M, 0M, 0M);
			InvoicingBase invoice5 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "00001000", testObjectCreator.USD, 0.67M);
			invoice5.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice5, testObjectCreator.USD, 0.67M, 800M, 0M, 0M);
			InvoicingBase invoice6 = testObjectCreator.CreateInvoice(typeof(APCreditNote), "apcredit", testObjectCreator.USD, 0.67M);
			invoice6.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice6, testObjectCreator.USD, 0.67M, 300M, 0M, 0M);

			Factory.Save();

			using (StreamReader reader = new StreamReader(PathToTestFile))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
				@$"Begin processing Transaction AP PAY ABIGAS AUD 1: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Matching session:
MatchDate: Match date must be equal or greater than the max post date of the transactions matched (01-Feb-09).
  This transaction has errors and was not imported.
Begin processing Transaction AR REC AALSHI USD 103: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.");
			}
		}

		[TestDate(2022, 3, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWhenNoRemittanceFileImportContextWithPTRAndMatchCreateAPWithholdingJournals()
		{
			AssertRemittanceFileImportWithPTRWithMatch(true, Times.Once);
		}

		[TestDate(2022, 3, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWhenHasRemittanceFileImportContextWithPTRAndMatchDoNotCreateAPWithholdingJournals()
		{
			AssertRemittanceFileImportWithPTRWithMatch(false, Times.Never);
		}

		void AssertRemittanceFileImportWithPTRWithMatch(bool shouldCreateAPPBWJounal, Func<Times> times)
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV11111", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1m, 400m, 0m, 0m);
			apInvoice.Factory.Save();
			AssertEquals("Precondition: AH_OutstandingAmount", -400m, apInvoice.AH_OutstandingAmount);

			var accountingTestObjectCreator = new AccountingTestObjectCreator(Factory);
			accountingTestObjectCreator.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, LedgerTypes.AccountsPayable, TaxSuperTypeList.StandardPaymentRetention.Code);

			using (var reader = new StreamReader(PathToTestDir + "RemittanceImportWithPTRWithMatch.csv"))
			{
				var accountingDependencyFactoryMock = new Mock<IAccountingDependencyFactory>();
				var withholdingJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();

				ObjectFactory.Substitute(accountingDependencyFactoryMock.Object);
				accountingDependencyFactoryMock.Setup(x => x.GetWithholdingJournalCreationManager()).Returns(withholdingJournalCreationManagerMock.Object);

				withholdingJournalCreationManagerMock.Setup(x => x.ShouldAPWithholdingJournalsBeCreated(It.Is<BusinessObjectFactory>(factory => factory.HasContext(BusinessContext.RemittanceFileImport)))).Returns(shouldCreateAPPBWJounal);

				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);
				Assert("Postcondition: HasErrors", !notificationBuffer.HasErrors);
				Assert("Postcondition: HasWarnings", !notificationBuffer.HasWarnings);

				withholdingJournalCreationManagerMock.Verify(x => x.CreateAPWithholdingJournalsIfApplicable(It.IsAny<IMatching>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<ZDate>()), times);

				var payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals(1, payment.Length);
				AssertEquals("Amount", 400m, payment[0].AH_OSTotalAmount);
				AssertEquals("After Import: AH_OutstandingAmount", 0m, payment[0].AH_OutstandingAmount);

				AssertEquals("Amount", 400m, apInvoice.AH_OSTotalAmount);
				AssertEquals("After Import: AH_OutstandingAmount", 0m, apInvoice.AH_OutstandingAmount);
			}
		}

		[TestDate(2009, 1, 2)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithInvoiceAndCreditNote()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(APReceipt)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(TransactionMatchLink)));

			InvoicingBase invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV123456", testObjectCreator.AUD, 1m);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.AH_PostDate = new ZDateTime(2009, 1, 2);
			testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1m, 22222m, 0m, 0m);
			InvoicingBase creditNote = testObjectCreator.CreateInvoice(typeof(APCreditNote), "APCRD987654", testObjectCreator.AUD, 1m);
			creditNote.AH_OH = TestObjectCreator.AALSHI.PK;
			creditNote.AH_PostDate = new ZDateTime(2009, 1, 2);
			testObjectCreator.CreateInvoiceLine(creditNote, testObjectCreator.AUD, 1m, 4600m, 0m, 0m);

			Factory.Save();

			string filePath = PathToTestDir + "RemittanceImportInvoiceAndCreditNote.csv";

			using (StreamReader reader = new StreamReader(filePath))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, filePath, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				APPayment[] payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(1, payment.Length);
				AssertEquals("Amount", 17622m, payment[0].AH_OSTotalAmount);
				AssertEquals("OutstandingAmount", 0m, payment[0].AH_OutstandingAmount);

				TransactionMatchLink matchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, payment[0].PK));
				AssertNotNull(matchLink);
			}
		}

		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataBadFile()
		{
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceBadFile.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
			}
		}

		[TestDate(2009, 01, 20)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_CorrectPostDate()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV123456", testObjectCreator.AUD, 1M);
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice1.AH_PostDate = ZDateTime.Now.AddDays(-10);
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 100M, 0M, 0M);
			InvoicingBase invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV987654", testObjectCreator.AUD, 1M);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_PostDate = ZDateTime.Now.AddDays(-10);
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.AUD, 1M, 300M, 0M, 0M);
			InvoicingBase invoice3 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV2", testObjectCreator.AUD, 1M);
			invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice3.AH_PostDate = ZDateTime.Now.AddDays(-10);
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.AUD, 1M, 100M, 0M, 0M);

			InvoicingBase invoice4 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", testObjectCreator.USD, 0.67M);
			invoice4.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice4.AH_PostDate = ZDateTime.Now.AddDays(-10);
			testObjectCreator.CreateInvoiceLine(invoice4, testObjectCreator.USD, 0.67M, 1500M, 0M, 0M);
			InvoicingBase invoice5 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "00001000", testObjectCreator.USD, 0.67M);
			invoice5.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice5.AH_PostDate = ZDateTime.Now.AddDays(-10);
			testObjectCreator.CreateInvoiceLine(invoice5, testObjectCreator.USD, 0.67M, 800M, 0M, 0M);
			InvoicingBase invoice6 = testObjectCreator.CreateInvoice(typeof(APCreditNote), "apcredit", testObjectCreator.USD, 0.67M);
			invoice6.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice6.AH_PostDate = ZDateTime.Now.AddDays(-10);
			testObjectCreator.CreateInvoiceLine(invoice6, testObjectCreator.USD, 0.67M, 300M, 0M, 0M);

			Factory.Save();

			using (StreamReader reader = new StreamReader(PathToTestFile))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(notificationBuffer.HasWarnings);

				APPayment[] payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(1, payment.Length);
				AssertEquals(500M, payment[0].AH_OSTotalAmount);
				AssertEquals(500M, payment[0].AH_LocalTotalAmount);
				AssertEquals(0M, payment[0].AH_Calc_OSOutstandingAmount);
				AssertEquals("000001", payment[0].AH_ChequeOrReference);
				AssertEquals(new ZDateTime(2009, 01, 15), payment[0].AH_PostDate);

				StmALog[] dataImportEvents = payment[0].Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
				AssertEquals("DIM event should be added.", 1, dataImportEvents.Length);

				AssertEquals(0M, invoice1.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice2.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice3.AH_Calc_OSOutstandingAmount);

				TransactionMatchLink matchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, payment[0].PK));
				AssertNotNull(matchLink);
				TransactionMatchLink invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice1.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice3.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);

				ARReceipt[] receipt = Factory.Load<ARReceipt>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(1, receipt.Length);
				AssertEquals(1000M, receipt[0].AH_OSTotalAmount);
				AssertEquals(1500M, receipt[0].AH_LocalTotalAmount);
				AssertEquals(0M, receipt[0].AH_Calc_OSOutstandingAmount);
				AssertEquals("103", receipt[0].AH_ChequeOrReference);
				AssertEquals(new ZDateTime(2009, 01, 15), receipt[0].AH_PostDate);

				dataImportEvents = receipt[0].Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
				AssertEquals("DIM event should be added.", 1, dataImportEvents.Length);

				AssertEquals(0M, invoice4.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice5.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice6.AH_Calc_OSOutstandingAmount);

				matchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receipt[0].PK));
				AssertNotNull(matchLink);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice4.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice5.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice6.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataWithExxTransaction()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV123456", testObjectCreator.USD, 0.65M);
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.USD, 0.65M, 200M, 0M, 0M);
			InvoicingBase invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV987654", testObjectCreator.USD, 0.65M);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.USD, 0.65M, 400M, 0M, 0M);
			InvoicingBase invoice3 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV2", testObjectCreator.USD, 0.65M);
			invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.USD, 0.65M, 400M, 0M, 0M);

			Factory.Save();

			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceExxTrans.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				APPayment[] payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(1, payment.Length);
				AssertEquals(1000M, payment[0].AH_OSTotalAmount);
				AssertEquals(1700.10M, payment[0].AH_LocalTotalAmount);
				AssertEquals(0M, payment[0].AH_Calc_OSOutstandingAmount);
				AssertEquals("000012", payment[0].AH_ChequeOrReference);
				AssertEquals(new ZDateTime(2009, 01, 15), payment[0].AH_PostDate);

				StmALog[] dataImportEvents = payment[0].Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
				AssertEquals("DIM event should be added.", 1, dataImportEvents.Length);

				AssertEquals(0M, invoice1.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice2.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice3.AH_Calc_OSOutstandingAmount);

				TransactionMatchLink matchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, payment[0].PK));
				AssertNotNull(matchLink);
				TransactionMatchLink invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice1.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice3.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);

				APExchangeDifference[] exxTransaction = Factory.Load<APExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(1, exxTransaction.Length);
				AssertEquals(161.65M, exxTransaction[0].AH_OSTotalAmount);
				AssertEquals(161.65M, exxTransaction[0].AH_LocalTotalAmount);
				AssertEquals(0M, exxTransaction[0].AH_Calc_OSOutstandingAmount);
				AssertEquals(new ZDateTime(2009, 01, 15), exxTransaction[0].AH_PostDate);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, exxTransaction[0].PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataWithExxTransactionStopMatchingWhenPTRLineNotFound()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			var invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV987654", testObjectCreator.USD, 0.65M);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.USD, 0.65M, 400M, 0M, 0M);
			var invoice3 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV2", testObjectCreator.USD, 0.65M);
			invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.USD, 0.65M, 400M, 0M, 0M);

			Factory.Save();

			using (var reader = new StreamReader(PathToTestDir + "PayRecRemittanceExxTrans.csv"))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				var payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(0, payment.Length);

				var invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2.PK));
				AssertNull(invoiceMatchLink);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice3.PK));
				AssertNull(invoiceMatchLink);

				var exxTransaction = Factory.Load<APExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(0, exxTransaction.Length);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataWithExxTransactionMixedCurrency()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV123456", testObjectCreator.USD, 0.65M);
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.USD, 0.65M, 200M, 0M, 0M);
			InvoicingBase invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV987654", testObjectCreator.GBP, 1.65M);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.GBP, 1.65M, 400M, 0M, 0M);
			InvoicingBase invoice3 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV2", testObjectCreator.USD, 0.65M);
			invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.USD, 0.65M, 400M, 0M, 0M);

			Factory.Save();

			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceExxTrans.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				APPayment[] payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(1, payment.Length);
				AssertEquals(1000M, payment[0].AH_OSTotalAmount);
				AssertEquals(1700.10M, payment[0].AH_LocalTotalAmount);
				AssertEquals(0M, payment[0].AH_Calc_OSOutstandingAmount);
				AssertEquals("000012", payment[0].AH_ChequeOrReference);
				AssertEquals(new ZDateTime(2009, 01, 15), payment[0].AH_PostDate);

				StmALog[] dataImportEvents = payment[0].Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
				AssertEquals("DIM event should be added.", 1, dataImportEvents.Length);

				AssertEquals(0M, invoice1.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice2.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice3.AH_Calc_OSOutstandingAmount);

				TransactionMatchLink matchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, payment[0].PK));
				AssertNotNull(matchLink);
				TransactionMatchLink invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice1.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice3.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);

				APExchangeDifference[] exxTransaction = Factory.Load<APExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(1, exxTransaction.Length);
				AssertEquals(534.61M, exxTransaction[0].AH_OSTotalAmount);
				AssertEquals(534.61M, exxTransaction[0].AH_LocalTotalAmount);
				AssertEquals(0M, exxTransaction[0].AH_Calc_OSOutstandingAmount);
				AssertEquals(new ZDateTime(2009, 01, 15), exxTransaction[0].AH_PostDate);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, exxTransaction[0].PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataWithExxTransactionMixedCurrencyStopMatchingWhenPTRLineNotFound()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			var invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV987654", testObjectCreator.GBP, 1.65M);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.GBP, 1.65M, 400M, 0M, 0M);
			var invoice3 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV2", testObjectCreator.USD, 0.65M);
			invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.USD, 0.65M, 400M, 0M, 0M);

			Factory.Save();

			using (var reader = new StreamReader(PathToTestDir + "PayRecRemittanceExxTrans.csv"))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				var payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(0, payment.Length);

				var invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2.PK));
				AssertNull(invoiceMatchLink);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice3.PK));
				AssertNull(invoiceMatchLink);

				var exxTransaction = Factory.Load<APExchangeDifference>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(0, exxTransaction.Length);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataWithoutMatchDocPrinting()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			setupInvoicesForImport();

			using (var reader = new StreamReader(PathToTestFile))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				importer.OnPrintMatchingDocument += new EventHandler<BoolResponseEventArgs>(Importer_OnPrintMatchingDocument);
				PrintMatchingDocument = false;
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				var payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(1, payment.Length);

				var receipt = Factory.Load<ARReceipt>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(1, receipt.Length);

				AssertEquals("Match Document printing form should not be shown.", null, ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataWithMatchDocPrinting()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			setupInvoicesForImport();

			using (var reader = new StreamReader(PathToTestFile))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				importer.OnPrintMatchingDocument += new EventHandler<BoolResponseEventArgs>(Importer_OnPrintMatchingDocument);
				PrintMatchingDocument = true;
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				var payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(1, payment.Length);

				var receipt = Factory.Load<ARReceipt>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(1, receipt.Length);

				AssertEquals("Match Document printing form should be shown.", "Enterprise.DocumentEngine.GUI.DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataWithZeroLocalAmountAndLocalCurrency()
		{
			var pathToTestFile = PathToTestDir + "PayRecRemittanceZeroLocalAmountWithLocalCurrency.csv";
			using (var reader = new StreamReader(pathToTestFile))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, pathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "Error: Local currency AP PAYMENT OS Amount does not equal Local Amount.");
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "Error: Local currency AR RECEIPT OS Amount does not equal Local Amount.");
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataWithInvalidCurrencies()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			var invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV111", testObjectCreator.AUD, 1M);
			invoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 500M, 0M, 0M);
			var invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV112", testObjectCreator.AUD, 1M);
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.AUD, 1M, 500M, 0M, 0M);
			var invoice3 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV113", testObjectCreator.AUD, 1M);
			invoice3.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.AUD, 1M, 500M, 0M, 0M);
			var invoice4 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV114", testObjectCreator.AUD, 1M);
			invoice4.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice4, testObjectCreator.AUD, 1M, 500M, 0M, 0M);
			var invoice5 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV115", testObjectCreator.AUD, 1M);
			invoice5.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice5, testObjectCreator.AUD, 1M, 500M, 0M, 0M);

			var invoice6 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", testObjectCreator.AUD, 1M);
			invoice6.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice6, testObjectCreator.AUD, 1M, 100M, 0M, 0M);
			var invoice7 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "00001001", testObjectCreator.AUD, 1M);
			invoice7.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice7, testObjectCreator.AUD, 1M, 100M, 0M, 0M);
			var invoice8 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "00001002", testObjectCreator.AUD, 1M);
			invoice8.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice8, testObjectCreator.AUD, 1M, 100M, 0M, 0M);
			var invoice9 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "00001003", testObjectCreator.AUD, 1M);
			invoice9.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice9, testObjectCreator.AUD, 1M, 100M, 0M, 0M);
			var invoice10 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "00001004", testObjectCreator.AUD, 1M);
			invoice10.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice10, testObjectCreator.AUD, 1M, 100M, 0M, 0M);

			var exRate = testObjectCreator.USD.ExchangeRates.AddNew();
			exRate.RE_SellRate = 2M;

			Factory.Save();

			string pathToTestFile = PathToTestDir + "PayRecRemittanceInvalidCurrencies.csv";
			using (var reader = new StreamReader(pathToTestFile))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, pathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@$"Begin processing Transaction AP PAY AALSHI AUD 1: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Local currency AP PAYMENT OS Amount does not equal Local Amount.
  This transaction has errors and was not imported.
Begin processing Transaction AP PAY AALSHI AUD 2: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AP PAY AALSHI AUD 3: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AP PAY AALSHI USD 1: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Bank Account: Bank account currency does not match the Accounts Payable Payment currency.
  This transaction has errors and was not imported.
Begin processing Transaction AP PAY AALSHI USD 2: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AR REC ABIGAS AUD 100: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Local currency AR RECEIPT OS Amount does not equal Local Amount.
  This transaction has errors and was not imported.
Begin processing Transaction AR REC ABIGAS AUD 101: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AR REC ABIGAS AUD 102: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.
Begin processing Transaction AR REC ABIGAS USD 103: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Bank Account: Bank account currency does not match the Accounts Receivable Receipt currency.
  This transaction has errors and was not imported.
Begin processing Transaction AR REC ABIGAS USD 104: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
  Completed Processing Transaction.");

				var importedPayments = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(3, importedPayments.Length);
				AssertReceiptPaymentByDescription(importedPayments, "Payment (local bank currency) amounts equal", testObjectCreator.AUD, 500M, 500M);
				AssertReceiptPaymentByDescription(importedPayments, "Payment (local bank foreign currency)", testObjectCreator.USD, 500M, 250M);
				AssertReceiptPaymentByDescription(importedPayments, "Payment (foreign bank currency)", testObjectCreator.USD, 500M, 250M);

				AssertEquals(500M, invoice1.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice2.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice3.AH_Calc_OSOutstandingAmount);
				AssertEquals(500M, invoice4.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice5.AH_Calc_OSOutstandingAmount);

				var importedReceipts = Factory.Load<ARReceipt>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt).AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(3, importedReceipts.Length);
				AssertReceiptPaymentByDescription(importedReceipts, "Receipt (local bank currency) amounts equal", testObjectCreator.AUD, 100M, 100M);
				AssertReceiptPaymentByDescription(importedReceipts, "Receipt (foreign bank currency)", testObjectCreator.USD, 100M, 50M);

				var invoices = new InvoicingBase[] { invoice6, invoice7, invoice8, invoice9, invoice10 }.OrderBy((invoice) => invoice.AH_TransactionNum).ToArray();

				AssertEquals(100M, invoices[0].AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoices[1].AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoices[2].AH_Calc_OSOutstandingAmount);
				AssertEquals(100M, invoices[3].AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoices[4].AH_Calc_OSOutstandingAmount);
			}
		}

		[TestDate(2009, 1, 2)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataWithENDPaymentError()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(APReceipt)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(TransactionMatchLink)));

			InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV123456", TestObjectCreator.AUD, 1m);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice.AH_PostDate = new ZDateTime(2009, 1, 2);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 22222m, 0m, 0m);

			InvoicingBase creditNote = testObjectCreator.CreateInvoice(typeof(APCreditNote), "APCRD987654", testObjectCreator.AUD, 1m);
			creditNote.AH_OH = TestObjectCreator.AALSHI.PK;
			creditNote.AH_PostDate = new ZDateTime(2009, 1, 2);
			testObjectCreator.CreateInvoiceLine(creditNote, testObjectCreator.AUD, 1m, 4600m, 0m, 0m);

			Factory.Save();

			//register for ComPay
			ENettRegisteredBankAccountCollection bankAccountCollection = new ENettRegisteredBankAccountCollection();
			ENettRegisteredBankAccount account = bankAccountCollection.AddNew();
			account.BankAccountPK = TestObjectCreator.AUDBankAccount.PK;
			AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, bankAccountCollection);

			TestObjectCreator.AALSHI.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "12345");

			Factory.Save();

			string filePath = PathToTestDir + "PayRecRemittanceWithENDPayment.csv";

			using (StreamReader reader = new StreamReader(filePath))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, filePath, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@$"Begin processing Transaction AP PAY AALSHI AUD EFT102110: 
Successfully matched organization with code 'AALSHI', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: AALSHI, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Matching session:
AH_TransactionType: A ComPay payment can only be matched to AP invoices.
  This transaction has errors and was not imported.");
			}

			TestObjectCreator.AALSHI.SetLocalCustomsCode(OrgCusCode.CodeTypes.eNettRegistrationNumber, "");
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreationOfJournalWithZeroAmountCreatesNotificationError()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceWithZeroAmountLine.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert("Should add error if journal is created with 0 amount", notificationBuffer.HasErrors);
				AssertContains("Importing a line for matching to transaction \"ACE1\" generated a journal with a zero amount.", notificationBuffer.AsString);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataRoundsValuesCorrectly_TransactionsNotFound_WithLineCurrency()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			string[] currencies = { Core.Constants.CurrencyCodes.Japan, Core.Constants.CurrencyCodes.Tunisia, Core.Constants.CurrencyCodes.EuropeanUnion };
			foreach (var currency in currencies)
			{
				setupSampleExchangeRate(Factory, 0.67m, Constants.ExchangeRateTypes.Code.BuyRate, currency);
				setupSampleExchangeRate(Factory, 1.39m, Constants.ExchangeRateTypes.Code.SellRate, currency);
			}

			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceWithHighPrecisionAmountsWithLineCurrency.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(2, journals.Length);
				journals.ForEach(x => AssertEquals("Journal should have csv line currency", "EUR", x.AH_RX_NKTransactionCurrency));
				journals.ForEach(x => AssertEquals("Journal amount should be rounded to csv line currency", 123.46m, Math.Abs(x.AH_OSTotal)));
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataRoundsValuesCorrectly_TransactionsNotFound_WithoutLineCurrency()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			string[] currencies = { Core.Constants.CurrencyCodes.Japan, Core.Constants.CurrencyCodes.Tunisia, Core.Constants.CurrencyCodes.EuropeanUnion };
			foreach (var currency in currencies)
			{
				setupSampleExchangeRate(Factory, 0.67m, Constants.ExchangeRateTypes.Code.BuyRate, currency);
				setupSampleExchangeRate(Factory, 1.39m, Constants.ExchangeRateTypes.Code.SellRate, currency);
			}

			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceWithHighPrecisionAmountsWithoutLineCurrency.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(2, journals.Length);
				journals.ForEach(x => AssertEquals("Journal should have payment currency", "JPY", x.AH_RX_NKTransactionCurrency));
				journals.ForEach(x => AssertEquals("Journal amount should be rounded to payment currency", 123m, Math.Abs(x.AH_OSTotal)));
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataRoundsValuesCorrectly_TransactionsFullyPaid_WithLineCurrency()
		{
			PrepareTestImportData("PayRecRemittanceWithHighPrecisionAmountsWithLineCurrency.csv", "ACE1", "EUR");
			AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			AssertEquals(2, journals.Length);
			journals.ForEach(x => AssertEquals("Journal should have csv line currency", "EUR", x.AH_RX_NKTransactionCurrency));
			journals.ForEach(x => AssertEquals("Journal amount should be rounded to csv line currency", 123.46m, Math.Abs(x.AH_OSTotal)));
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataRoundsValuesCorrectly_TransactionsFullyPaid_WithoutLineCurrency()
		{
			PrepareTestImportData("PayRecRemittanceWithHighPrecisionAmountsWithoutLineCurrency.csv", "ACE1", "TND");
			AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			AssertEquals(2, journals.Length);
			journals.ForEach(x => AssertEquals("Journal should have transaction currency", "TND", x.AH_RX_NKTransactionCurrency));
			journals.ForEach(x => AssertEquals("Journal amount should be rounded to transaction currency", 123.457m, Math.Abs(x.AH_OSTotal)));
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataRoundsValuesCorrectly_TransactionsOverPaidWithLineCurrency()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			string[] currencies = { Core.Constants.CurrencyCodes.Japan, Core.Constants.CurrencyCodes.Tunisia, Core.Constants.CurrencyCodes.EuropeanUnion };
			foreach (var currency in currencies)
			{
				setupSampleExchangeRate(Factory, 0.67m, Constants.ExchangeRateTypes.Code.BuyRate, currency);
				setupSampleExchangeRate(Factory, 1.39m, Constants.ExchangeRateTypes.Code.SellRate, currency);
			}

			InvoicingBase invoice = testObjectCreator.CreateAPInvoice<APInvoice>("ACE1", TestObjectCreator.EUR, 1m, 100m, 0.0m, 0.0m, 100m, 0.0m, 0.0m);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice.AH_PostDate = new ZDateTime(2009, 01, 14);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceWithHighPrecisionAmountsWithLineCurrency.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

				AssertEquals(2, journals.Length);
				journals.ForEach(x => AssertEquals("Journal should have csv line currency", "EUR", x.AH_RX_NKTransactionCurrency));
				journals.ForEach(x => AssertEquals("Journal amount should be rounded to csv line currency", 23.46m, Math.Abs(x.AH_OSTotal)));
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataShouldNotThrowExceptionDueToWrongExchangeRate()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			AssertEquals(Core.Constants.CurrencyCodes.Australia, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			setupSampleExchangeRate(Factory, 0.5m, Constants.ExchangeRateTypes.Code.BuyRate, Core.Constants.CurrencyCodes.UnitedStates);
			setupSampleExchangeRate(Factory, 2m, Constants.ExchangeRateTypes.Code.SellRate, Core.Constants.CurrencyCodes.UnitedStates);

			InvoicingBase invoice = testObjectCreator.CreateAPInvoice<APInvoice>("ACE1", TestObjectCreator.USD, 2m, 200m, 0.0m, 0.0m, 100m, 0.0m, 0.0m);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice.AH_PostDate = new ZDateTime(2009, 01, 14);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceWithForeignInvoiceAndLocalLineCurrency.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
					"The created journal related to the PTR record with transaction number ACE1 has a wrong Currency (AUD). It must be equal to the matched transaction currency (USD).");
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataRoundsValuesCorrectly_TransactionsOverPaidWithoutLineCurrency()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			string[] currencies = { Core.Constants.CurrencyCodes.Japan, Core.Constants.CurrencyCodes.Tunisia, Core.Constants.CurrencyCodes.EuropeanUnion };
			foreach (var currency in currencies)
			{
				setupSampleExchangeRate(Factory, 0.67m, Constants.ExchangeRateTypes.Code.BuyRate, currency);
				setupSampleExchangeRate(Factory, 1.39m, Constants.ExchangeRateTypes.Code.SellRate, currency);
			}

			InvoicingBase invoice = testObjectCreator.CreateAPInvoice<APInvoice>("ACE1", RefCurrency.LoadFromCurrencyCode(Factory, "TND"), 1m, 100m, 0.0m, 0.0m, 100m, 0.0m, 0.0m);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice.AH_PostDate = new ZDateTime(2009, 01, 14);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceWithHighPrecisionAmountsWithoutLineCurrency.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

				AssertEquals(2, journals.Length);
				journals.ForEach(x => AssertEquals("Journal should have transaction currency", "TND", x.AH_RX_NKTransactionCurrency));
				journals.ForEach(x => AssertEquals("Journal amount should be rounded to transaction currency", 23.457m, Math.Abs(x.AH_OSTotal)));
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataAPJournalUnmatchingCurrency()
		{
			setupSampleExchangeRate(Factory, 0.8m, Constants.ExchangeRateTypes.Code.SellRate);

			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			OrgHeader org = testObjectCreator.ABIGAS;
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			InvoicingBase invoice1 = testObjectCreator.CreateAPInvoice<APInvoice>("00640364", testObjectCreator.USD, 0.8m, 1000, 0.0m, 0.0m, 1250m, 0.0m, 0.0m);
			InvoicingBase invoice2 = testObjectCreator.CreateAPInvoice<APInvoice>("00643918", testObjectCreator.USD, 0.8m, 1000, 0.0m, 0.0m, 1250m, 0.0m, 0.0m);
			InvoicingBase invoice3 = testObjectCreator.CreateAPInvoice<APInvoice>("00644589", testObjectCreator.USD, 0.8m, 1000, 0.0m, 0.0m, 1250m, 0.0m, 0.0m);
			InvoicingBase invoice4 = testObjectCreator.CreateAPInvoice<APInvoice>("00643787", testObjectCreator.USD, 0.8m, 1000, 0.0m, 0.0m, 1250m, 0.0m, 0.0m);
			invoice1.AH_OH = org.PK;
			invoice2.AH_OH = org.PK;
			invoice3.AH_OH = org.PK;
			invoice4.AH_OH = org.PK;
			invoice1.AH_PostDate = new ZDateTime(2009, 01, 14);
			invoice2.AH_PostDate = new ZDateTime(2009, 01, 14);
			invoice3.AH_PostDate = new ZDateTime(2009, 01, 14);
			invoice4.AH_PostDate = new ZDateTime(2009, 01, 14);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceUnmatchingCurrency.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(2, journals.Length);

				Guid apControlAccountPK = (Guid)AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.Value;

				AssertJournalExist(journals, "AP", org.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "00643787a", 1, 1250, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionNotFound);
				AssertJournalExist(journals, "AP", org.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "00643787a", 1, -1250, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionNotFound);

				AccTransactionHeader[] exxDifferences = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "EXX").AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals("There should be no exchange difference in the matching", 0, exxDifferences.Length);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataAPJournalForeignPayment()
		{
			setupSampleExchangeRate(Factory, 0.8m, Constants.ExchangeRateTypes.Code.SellRate);
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			OrgHeader org = testObjectCreator.ABIGAS;
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			InvoicingBase invoice1 = testObjectCreator.CreateAPInvoice<APInvoice>("10000001", testObjectCreator.USD, 0.8m, 1000, 0.0m, 0.0m, 1250, 0.0m, 0.0m);
			InvoicingBase invoice2 = testObjectCreator.CreateAPInvoice<APInvoice>("10000002", testObjectCreator.USD, 0.8m, 1000, 0.0m, 0.0m, 1250, 0.0m, 0.0m);
			InvoicingBase invoice3 = testObjectCreator.CreateAPInvoice<APInvoice>("10000003", testObjectCreator.USD, 0.8m, 1000, 0.0m, 0.0m, 1250, 0.0m, 0.0m);
			InvoicingBase invoice4 = testObjectCreator.CreateAPInvoice<APInvoice>("10000004", testObjectCreator.USD, 0.8m, 1000, 0.0m, 0.0m, 1250, 0.0m, 0.0m);
			invoice1.AH_OH = org.PK;
			invoice2.AH_OH = org.PK;
			invoice3.AH_OH = org.PK;
			invoice4.AH_OH = org.PK;
			invoice1.AH_PostDate = new ZDateTime(2009, 01, 14);
			invoice2.AH_PostDate = new ZDateTime(2009, 01, 14);
			invoice3.AH_PostDate = new ZDateTime(2009, 01, 14);
			invoice4.AH_PostDate = new ZDateTime(2009, 01, 14);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceForeignPayment.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(2, journals.Length);

				Guid apControlAccountPK = (Guid)AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.Value;

				AssertJournalExist(journals, "AP", org.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AP JOURNAL", "10000004a", 0.8m, 1000, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionNotFound);
				AssertJournalExist(journals, "AP", org.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AP JOURNAL", "10000004a", 0.8m, -1000, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionNotFound);

				AccTransactionHeader[] exxDifferences = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "EXX").AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals("There should be no exchange difference in the matching", 0, exxDifferences.Length);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataAPJournalUnmatchingCompany()
		{
			setupSampleExchangeRate(Factory, 0.8m, Constants.ExchangeRateTypes.Code.SellRate);
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			OrgHeader org = testObjectCreator.ABIGAS;
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			InvoicingBase invoice1 = testObjectCreator.CreateAPInvoice<APInvoice>("1001", testObjectCreator.AUD, 1, 50, 0.0m, 0.0m, 50, 0.0m, 0.0m);
			InvoicingBase invoice2 = testObjectCreator.CreateAPInvoice<APInvoice>("1002", testObjectCreator.AUD, 1, 60, 0.0m, 0.0m, 60, 0.0m, 0.0m);
			InvoicingBase invoice3 = testObjectCreator.CreateAPInvoice<APInvoice>("1003", testObjectCreator.AUD, 1, 70, 0.0m, 0.0m, 70, 0.0m, 0.0m);
			InvoicingBase invoice4 = testObjectCreator.CreateAPInvoice<APInvoice>("1005", testObjectCreator.AUD, 1, 80, 0.0m, 0.0m, 80, 0.0m, 0.0m);
			invoice1.AH_OH = org.PK;
			invoice2.AH_OH = org.PK;
			invoice3.AH_OH = org.PK;
			invoice4.AH_OH = org.PK;
			invoice1.AH_PostDate = new ZDateTime(2009, 01, 14);
			invoice2.AH_PostDate = new ZDateTime(2009, 01, 14);
			invoice3.AH_PostDate = new ZDateTime(2009, 01, 14);
			invoice4.AH_PostDate = new ZDateTime(2009, 01, 14);
			invoice4.AH_GB = testObjectCreator.NonCurrentCompany.Branches[0].PK;
			invoice4.Lines[0].AL_GB = testObjectCreator.NonCurrentCompany.Branches[0].PK;

			Factory.Save();

			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceMatchingOtherCompany.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals(2, journals.Length);

				Guid apControlAccountPK = (Guid)AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.Value;

				AssertJournalExist(journals, "AP", org.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "1005", 1, 80, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionNotFound);
				AssertJournalExist(journals, "AP", org.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "1005", 1, -80, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionNotFound);

				AccTransactionHeader[] exxDifferences = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "EXX").AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));
				AssertEquals("There should be no exchange difference in the matching", 0, exxDifferences.Length);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMatchingTransactionsReloadingIsNotDependentOnSettlementGroup()
		{
			testObjectCreator.ABIGAS.ARSettlementGroupPK = testObjectCreator.AALSHI.PK;
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			setupInvoicesForImport();
			assertMatchedInvoices();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_OutOfRangeDateNotThrownExceptionAtSetExchangeRate()
		{
			var notificationBuffer = PrepareTestImportData("PayRecRemittanceDateOutOfRange.csv", "ACE1");

			AssertEquals("Should not have error reported", 0, CargoWise.Common.ErrorReporter.TotalErrorCount);
			Assert(notificationBuffer.HasErrors);
			Assert("Must contain date out of range error", notificationBuffer.AsString.Contains("Date out of range:"));
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_CurrencyExist()
		{
			var notificationBuffer = PrepareTestImportData("PayRecRemittanceWithCurrencyExist.csv", "ACE1", "EUR");

			Assert(!notificationBuffer.HasErrors);
			Assert(!notificationBuffer.HasWarnings);

			AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			Guid apControlAccountPK = (Guid)AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.Value;

			AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "EUR", "AP JOURNAL", "ACE1", 0.670003M, 345.46, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
			AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "EUR", "AP JOURNAL", "ACE1", 0.670003M, -345.46, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_CurrencyEmpty()
		{
			var notificationBuffer = PrepareTestImportData("PayRecRemittanceWithCurrencyEmpty.csv", "ACE2", "USD");

			Assert(!notificationBuffer.HasErrors);
			Assert(!notificationBuffer.HasWarnings);

			AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			Guid apControlAccountPK = (Guid)AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.Value;

			AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AP JOURNAL", "ACE2", 0.670003M, 345.46, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
			AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AP JOURNAL", "ACE2", 0.670003M, -345.46, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_CurrencyInvalid()
		{
			var notificationBuffer = PrepareTestImportData("PayRecRemittanceWithCurrencyInvalid.csv", "ACE3");

			Assert(notificationBuffer.HasErrors);
			Assert("Must contain exchange-rate error", notificationBuffer.AsString.Contains("AH_ExchangeRate: Please enter an Exchange Rate."));
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_LocalCurrency()
		{
			var notificationBuffer = PrepareTestImportData("PayRecRemittanceWithLocalCurrency.csv", "ACE4", "EUR");

			Assert(!notificationBuffer.HasErrors);
			Assert(!notificationBuffer.HasWarnings);

			AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(
				new ZQuery() { OrderBy = AccTransactionHeaderSchema.Constants.AH_TransactionNum }
				.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, "JNL")
				.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty)
				);

			Guid apControlAccountPK = (Guid)AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.Value;

			var expectedExchangeRate = 0.811434m;
			AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "EUR", "AP JOURNAL", "ACE4", expectedExchangeRate, 345.46, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
			AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "EUR", "AP JOURNAL", "ACE4", expectedExchangeRate, -345.46, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);

			AssertEquals("Local Amount must be set according to import file", 425.74m, journals[0].AH_InvoiceAmount);
			AssertEquals("Local Amount must be set according to import file", -425.74m, journals[1].AH_InvoiceAmount);
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_LocalCurrency_SameCurrency()
		{
			var notificationBuffer = PrepareTestImportData("TestImportData_LocalCurrency_SameCurrency.csv", "ACE5", "USD");

			Assert(!notificationBuffer.HasErrors);
			Assert(!notificationBuffer.HasWarnings);

			AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, "JNL").AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty));

			Guid apControlAccountPK = (Guid)AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.Value;

			var expectedExchangeRate = 0.811434m;
			AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AP JOURNAL", "ACE5", expectedExchangeRate, 345.46, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
			AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "USD", "AP JOURNAL", "ACE5", expectedExchangeRate, -345.46, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);

			var sortedJournals = journals.OrderByDescending(x => x.AH_InvoiceAmount).ToList();

			AssertEquals("Local Amount must be set according to import file", 425.74m, sortedJournals[0].AH_InvoiceAmount);
			AssertEquals("Local Amount must be set according to import file", -425.74m, sortedJournals[1].AH_InvoiceAmount);
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_LocalCurrencyEmpty()
		{
			var notificationBuffer = PrepareTestImportDataWithoutExchangeRate("PayRecRemittanceWithLocalCurrencyEmpty.csv", "ACE6", "AUD");

			Assert(!notificationBuffer.HasErrors);
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_SingleRecWithoutPtr()
		{
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceSingleRecWithoutPtr.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				AccTransactionHeader[] recs = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

				AssertEquals(1, recs.Length);
				Assert(recs.Any(x => x.AH_ChequeOrReference == "0001" && x.AH_Desc == "TEST WITHOUT PTR RCT"));
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_SinglePayWithoutPtr()
		{
			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceSinglePayWithoutPtr.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				AccTransactionHeader[] pays = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

				AssertEquals(1, pays.Length);
				Assert(pays.Any(x => x.AH_ChequeOrReference == "0003" && x.AH_Desc == "TEST WITHOUT PTR AP"));
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_MultipleRecsSomeWithoutPtr()
		{
			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "05520", testObjectCreator.AUD, 1M);
			invoice1.AH_ChequeOrReference = "05520";
			invoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 700, 0M, 0M);

			Factory.Save();

			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceMultipleRecsSomeWithoutPtr.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				Guid arControlAccountPK = (Guid)AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.Value;

				var expectedExchangeRate = 1m;

				AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AccTransactionHeader[] recs = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

				AssertEquals(2, recs.Length);
				AssertJournalExist(journals, "AR", TestObjectCreator.AALSHI.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "05520", expectedExchangeRate, 1, arControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
				AssertJournalExist(journals, "AR", TestObjectCreator.AALSHI.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "05520", expectedExchangeRate, -1, arControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
				Assert(recs.Any(x => x.AH_ChequeOrReference == "0001" && x.AH_Desc == "TEST WITHOUT PTR RCT"));
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_MultiplePaysSomeWithoutPtr()
		{
			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "GRP1A", testObjectCreator.AUD, 1M);
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 100, 0M, 0M);

			Factory.Save();

			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceMultiplePaysSomeWithoutPtr.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				Guid apControlAccountPK = (Guid)AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.Value;

				var expectedExchangeRate = 1m;

				AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AccTransactionHeader[] pays = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

				AssertEquals(2, pays.Length);
				AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "GRP1A", expectedExchangeRate, 1, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
				AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "GRP1A", expectedExchangeRate, -1, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
				Assert(pays.Any(x => x.AH_ChequeOrReference == "0003" && x.AH_Desc == "TEST WITHOUT PTR AP"));
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_MultipleRecsAndPaysSomeWithoutPtr()
		{
			AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "05520", testObjectCreator.AUD, 1M);
			invoice1.AH_ChequeOrReference = "05520";
			invoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 700, 0M, 0M);

			InvoicingBase invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "GRP1A", testObjectCreator.AUD, 1M);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.AUD, 1M, 100, 0M, 0M);

			Factory.Save();

			using (StreamReader reader = new StreamReader(PathToTestDir + "PayRecRemittanceMultipleRecsAndPaysSomeWithoutPtr.csv"))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				Guid arControlAccountPK = (Guid)AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount.Value;
				Guid apControlAccountPK = (Guid)AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.Value;

				var expectedExchangeRate = 1m;

				AccTransactionHeader[] journals = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AccTransactionHeader[] recs = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AccTransactionHeader[] pays = Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

				AssertEquals(2, recs.Length);
				AssertEquals(2, pays.Length);

				AssertJournalExist(journals, "AR", TestObjectCreator.AALSHI.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "05520", expectedExchangeRate, 1, arControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
				AssertJournalExist(journals, "AR", TestObjectCreator.AALSHI.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "05520", expectedExchangeRate, -1, arControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
				AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "GRP1A", expectedExchangeRate, 1, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
				AssertJournalExist(journals, "AP", TestObjectCreator.ABIGAS.PK, ZDateTime.Now, ZDateTime.Now, ZDateTime.Now, "AUD", "AP JOURNAL", "GRP1A", expectedExchangeRate, -1, apControlAccountPK, Constants.TransactionCategory.Codes.TransactionAlreadyPaid);
				Assert(recs.Any(x => x.AH_ChequeOrReference == "0001" && x.AH_Desc == "TEST WITHOUT PTR RCT"));
				Assert(pays.Any(x => x.AH_ChequeOrReference == "0003" && x.AH_Desc == "TEST WITHOUT PTR AP"));
			}
		}

		NotificationBuffer PrepareTestImportData(ZString importFile, ZString expectedTrxNumber, string strCurrency = "AUD")
		{
			NotificationBuffer notificationBuffer = null;
			try
			{
				string[] currencies = { Core.Constants.CurrencyCodes.UnitedStates, Core.Constants.CurrencyCodes.EuropeanUnion };//, Core.Constants.CurrencyCodes.NewZealand };
				foreach (var currency in currencies)
				{
					setupSampleExchangeRate(Factory, 0.67m, Constants.ExchangeRateTypes.Code.BuyRate, currency);
					setupSampleExchangeRate(Factory, 1.39m, Constants.ExchangeRateTypes.Code.SellRate, currency);
				}
				SetupOverpaidTestData();

				var transactionCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, strCurrency);
				InvoicingBase transaction = testObjectCreator.CreateInvoice(typeof(APInvoice), expectedTrxNumber, transactionCurrency, 0.67m);
				transaction.AH_OH = TestObjectCreator.ABIGAS.PK;
				Factory.Save();

				ExchangeRateReader.GetReaderInstance().ClearCache();
				AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				using (StreamReader reader = new StreamReader(PathToTestDir + importFile))
				{
					AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

					PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
					notificationBuffer = new NotificationBuffer();
					importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);
				}
			}
			finally
			{
				testObjectCreator.USD.ExchangeRates[0].RE_ExRateType = Constants.ExchangeRateTypes.Code.CustomsRate;
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}
			return notificationBuffer;
		}

		NotificationBuffer PrepareTestImportDataWithoutExchangeRate(ZString importFile, ZString expectedTrxNumber, string strCurrency = "AUD")
		{
			NotificationBuffer notificationBuffer = null;
			try
			{
				SetupOverpaidTestData();

				var transactionCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, strCurrency);
				InvoicingBase transaction = testObjectCreator.CreateInvoice(typeof(APInvoice), expectedTrxNumber, transactionCurrency, 1m);
				transaction.AH_OH = TestObjectCreator.ABIGAS.PK;
				Factory.Save();

				ExchangeRateReader.GetReaderInstance().ClearCache();
				AccountingConfigurationRegistry.Instance.AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				using (StreamReader reader = new StreamReader(PathToTestDir + importFile))
				{
					AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

					PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
					notificationBuffer = new NotificationBuffer();
					importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);
				}
			}
			finally
			{
				testObjectCreator.USD.ExchangeRates[0].RE_ExRateType = Constants.ExchangeRateTypes.Code.CustomsRate;
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}
			return notificationBuffer;
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataBranchDepartmentCombinationsValidationError()
		{
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 0, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			InvoicingBase invoice1 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "ARCREDIT12345", testObjectCreator.USD, 0.67M);
			invoice1.AH_ChequeOrReference = "ARCREDIT12345";
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice1.IsManuallySetTransactionNumber_ForTestOnly = true;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.USD, 0.67M, 500M, 0M, 0M);

			InvoicingBase invoice2 = testObjectCreator.CreateInvoice(typeof(APCreditNote), "APCREDIT12345", testObjectCreator.USD, 0.67M);
			invoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.USD, 0.67M, 700M, 0M, 0M);

			Factory.Save();

			AssertEquals("Precondition: ", 2, Factory.GetDatabaseCount(typeof(AccTransactionHeader)));
			AssertEquals("Precondition: ", 2, Factory.GetDatabaseCount(typeof(AccTransactionLines)));

			var bbbDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();
			var currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { bbbDepartment });
			Factory.Save();

			using (StreamReader reader = new StreamReader(this.PathToTestDir + "ARPayAPRecRemittance.csv"))
			{
				GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsErrorMessage
					(Factory, () =>
					{
						PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
						NotificationBuffer notificationBuffer = new NotificationBuffer();
						importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

						Assert("Should have error", notificationBuffer.HasErrors);
						return notificationBuffer.AsString;
					});
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataARMatchesReferenceIgnoringOrganization()
		{
			var debtor1 = TestObjectCreator.CreateOrgHeader("TSDEBT1", creditor: false, debtor: true);
			var debtor2 = TestObjectCreator.CreateOrgHeader("TSDEBT2", creditor: false, debtor: true);
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			var invoice1 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", testObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, testObjectCreator.ABIGAS, testObjectCreator.GLHeader1.PK);
			var invoice2 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001001", testObjectCreator.AUD, 1m, 200M, 0M, 200m, 0M, testObjectCreator.ABIGAS, testObjectCreator.GLHeader1.PK);
			invoice2.AH_ConsolidatedInvoiceRef = "1001";
			var invoice3 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001002", testObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, debtor2, testObjectCreator.GLHeader1.PK);
			invoice3.AH_ChequeOrReference = "00001000";

			Factory.Save();

			var path = PathToTestDir + "PayRecRemittance_AR_ReferenceMatching_IgnoringOrg.csv";
			using (StreamReader reader = new StreamReader(path))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, path, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				ARReceipt[] receipt = Factory.Load<ARReceipt>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals(1, receipt.Length);
				AssertEquals(0M, receipt[0].AH_Calc_OSOutstandingAmount);
				AssertEquals("103", receipt[0].AH_ChequeOrReference);

				var dataImportEvents = receipt[0].Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
				AssertEquals("DIM event should be added.", 1, dataImportEvents.Length);

				AssertEquals(0M, invoice1.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice2.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice3.AH_Calc_OSOutstandingAmount);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataARMatchesReferenceIgnoringOrganization_TransactionType()
		{
			TestObjectCreator.CreateOrgHeader("TSDEBT", creditor: false, debtor: true);
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			var invoice = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", testObjectCreator.AUD, 1M, 300M, 0M, 300M, 0M, testObjectCreator.ABIGAS, testObjectCreator.GLHeader1.PK);
			invoice.AH_ConsolidatedInvoiceRef = "1001";
			var creditNote = testObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "00001001", testObjectCreator.AUD, 1m, 200M, 0M, 200m, 0M, testObjectCreator.ABIGAS, testObjectCreator.GLHeader1.PK);
			creditNote.AH_ConsolidatedInvoiceRef = "1001";

			Factory.Save();

			var path = PathToTestDir + "PayRecRemittance_AR_ReferenceMatching_IgnoringOrg_TransactionType.csv";
			using (StreamReader reader = new StreamReader(path))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, path, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				ARReceipt[] receipt = Factory.Load<ARReceipt>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals(1, receipt.Length);
				AssertEquals(0M, receipt[0].AH_Calc_OSOutstandingAmount);
				AssertEquals("103", receipt[0].AH_ChequeOrReference);

				var dataImportEvents = receipt[0].Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
				AssertEquals("DIM event should be added.", 1, dataImportEvents.Length);

				AssertEquals(0M, invoice.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, creditNote.AH_Calc_OSOutstandingAmount);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataAPOrgMatching()
		{
			var creditor1 = TestObjectCreator.CreateOrgHeader("TSCRDT1", creditor: true, debtor: false);
			var creditor2 = TestObjectCreator.CreateOrgHeader("TSCRDT2", creditor: true, debtor: false);
			var creditor3 = TestObjectCreator.CreateOrgHeader("TSCRDT3", creditor: true, debtor: false);

			creditor3.APSettlementGroupPK = creditor2.PK;

			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			var invoice1 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1001", testObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, creditor1, testObjectCreator.GLHeader1.PK);
			var invoice2 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1001", testObjectCreator.AUD, 1m, 100M, 0M, 100m, 0M, creditor2, testObjectCreator.GLHeader1.PK);
			var invoice3 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1001", testObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, creditor3, testObjectCreator.GLHeader1.PK);

			var invoice4 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1002", testObjectCreator.AUD, 1M, 200M, 0M, 200M, 0M, creditor1, testObjectCreator.GLHeader1.PK);
			var invoice5 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1002", testObjectCreator.AUD, 1M, 200M, 0M, 200M, 0M, creditor2, testObjectCreator.GLHeader1.PK);
			var invoice6 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1002", testObjectCreator.AUD, 1M, 200M, 0M, 200M, 0M, creditor3, testObjectCreator.GLHeader1.PK);

			var invoice7 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1003", testObjectCreator.AUD, 1M, 300M, 0M, 300M, 0M, creditor1, testObjectCreator.GLHeader1.PK);
			var invoice8 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1003", testObjectCreator.AUD, 1M, 300M, 0M, 300M, 0M, creditor3, testObjectCreator.GLHeader1.PK);

			Factory.Save();

			var path = PathToTestDir + "PayRecRemittance_AP_DifferentOrg.csv";
			using (StreamReader reader = new StreamReader(path))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, path, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				APPayment[] payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals(1, payment.Length);
				AssertEquals(0M, payment[0].AH_Calc_OSOutstandingAmount);
				AssertEquals("103", payment[0].AH_ChequeOrReference);

				var dataImportEvents = payment[0].Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
				AssertEquals("DIM event should be added.", 1, dataImportEvents.Length);

				AssertEquals("invoice1 is matched since its org is same as the org in PTR line", 0M, invoice1.AH_Calc_OSOutstandingAmount);
				AssertEquals("invoice5 is matched since it has org same as in the PAY line", 0M, invoice5.AH_Calc_OSOutstandingAmount);
				AssertEquals("invoice8 is matched since it has org which is a child of the org in PAY line", 0M, invoice8.AH_Calc_OSOutstandingAmount);
			}
		}

		public void TestGetDebtorOrCreditorFilter()
		{
			var adapter = new RemittanceFileImportAdapter();
			var payRecOrgPK = ZGuid.NewZGuid();
			var pTRorgPK = ZGuid.NewZGuid();
			var childOrgAPK = ZGuid.NewZGuid();
			var childOrgPKs = new List<ZGuid>() { childOrgAPK };

			var debtorOrCreditorFilter = TxnHeaderProcessorBase.GetDebtorOrCreditorFilter_ForTest(payRecOrgPK, pTRorgPK, null);
			AssertEquals($"AH_OH = CONVERT('{pTRorgPK.ToString()}', 'System.Guid')", debtorOrCreditorFilter.LiteralTextADO);

			pTRorgPK = ZGuid.Empty;
			debtorOrCreditorFilter = TxnHeaderProcessorBase.GetDebtorOrCreditorFilter_ForTest(payRecOrgPK, pTRorgPK, null);
			AssertEquals($"AH_OH = CONVERT('{payRecOrgPK.ToString()}', 'System.Guid')", debtorOrCreditorFilter.LiteralTextADO);

			debtorOrCreditorFilter = TxnHeaderProcessorBase.GetDebtorOrCreditorFilter_ForTest(payRecOrgPK, pTRorgPK, childOrgPKs);
			if (debtorOrCreditorFilter.LiteralTextADO != $"(AH_OH in (CONVERT('{payRecOrgPK.ToString()}', 'System.Guid'), CONVERT('{childOrgAPK.ToString()}', 'System.Guid')))" &&
					debtorOrCreditorFilter.LiteralTextADO != $"(AH_OH in (CONVERT('{childOrgAPK.ToString()}', 'System.Guid'), CONVERT('{payRecOrgPK.ToString()}', 'System.Guid')))")
			{
				Fail();
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataBothARAPOrgMatching()
		{
			var org1 = TestObjectCreator.CreateOrgHeader("TORG1", creditor: true, debtor: true);
			var org2 = TestObjectCreator.CreateOrgHeader("TORG2", creditor: true, debtor: true);
			var org3 = TestObjectCreator.CreateOrgHeader("TORG3", creditor: true, debtor: true);

			org3.APSettlementGroupPK = org2.PK;

			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			var apInvoice1 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1001", testObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, org1, testObjectCreator.GLHeader1.PK);
			var apInvoice2 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1002", testObjectCreator.AUD, 1M, 200M, 0M, 200M, 0M, org2, testObjectCreator.GLHeader1.PK);
			var apInvoice3 = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "1003", testObjectCreator.AUD, 1M, 300M, 0M, 300M, 0M, org3, testObjectCreator.GLHeader1.PK);

			var arInvoice1 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", testObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, org1, testObjectCreator.GLHeader1.PK);
			var arInvoice2 = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001001", testObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, org1, testObjectCreator.GLHeader1.PK);

			Factory.Save();

			var path = PathToTestDir + "PayRecRemittance_BothARAPOrgMatching.csv";
			using (StreamReader reader = new StreamReader(path))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, path, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				APPayment[] payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals(1, payment.Length);
				AssertEquals(0M, payment[0].AH_Calc_OSOutstandingAmount);
				AssertEquals("103", payment[0].AH_ChequeOrReference);

				var dataImportEvents = payment[0].Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
				AssertEquals("DIM event should be added.", 1, dataImportEvents.Length);

				AssertEquals("apInvoice1 is matched since its org is same as the org in PTR line", 0M, apInvoice1.AH_Calc_OSOutstandingAmount);
				AssertEquals("apInvoice2 is matched since it has org same as in the PAY line", 0M, apInvoice2.AH_Calc_OSOutstandingAmount);
				AssertEquals("apInvoice3 is matched since it has org which is a child of the org in PAY line", 0M, apInvoice3.AH_Calc_OSOutstandingAmount);
				AssertEquals("arInvoice1 is matched since it has org which is a child of the org in PAY line", 0M, apInvoice3.AH_Calc_OSOutstandingAmount);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_InvalidPTRLedger()
		{
			TestObjectCreator.CreateOrgHeader("TSDEBT", creditor: false, debtor: true);
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";

			Factory.Save();

			var path = PathToTestDir + "PayRecRemittance_InvalidPTRLedger.csv";
			using (StreamReader reader = new StreamReader(path))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, path, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(notificationBuffer.HasErrors);

				var expectedError = @$"Registry value for Organizations -> Allow Numeric Characters In Code Generation is currently Enabled.
Registry value for Organizations -> Use Default Organization for Matching is currently Disabled.
Current Organization Match Threshold - MED.
Current Organization Proxy - EDICUS.
Current Company - EDI.
Current Branch - SYD.

Begin processing Transaction AR REC ZTSDEBT AUD 103: 
Successfully matched organization with code 'ZTSDEBT', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ZTSDEBT, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Invalid ledger: 'CB'
  This transaction has errors and was not imported.

Saving the data to the database...
";

				AssertContains(expectedError, notificationBuffer.AsString);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportDataShouldNotThrowExceptionDueToPaymentExchangeRatePrecision()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV001", TestObjectCreator.USD, 0.587623M);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.USD, 0.587623M, 7123.45M);
			Factory.Save();

			var pathToTestFile = PathToTestDir + "PayRemittanceWithHighExchangeRatePrecision.csv";
			using (var reader = new StreamReader(pathToTestFile))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				AssertNoExceptionThrown(() =>
					{
						importer.ImportData(reader, pathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);
					});

				Assert(!notificationBuffer.HasErrors);
			}
		}

		public void TestSupportsHistoryForDuplicatesPrevention()
		{
			var importer = new PaymentReceiptRemittanceDataImporter_ForTest();
			AssertEquals("ImportTypeForDuplicatesPrevention", "Remittance", importer.ImportTypeForDuplicatesPrevention_ForTestOnly);
			AssertEquals("DaysToKeepHistoryFor", 7, importer.DaysToKeepHistoryFor_ForTestOnly);
			Assert("SupportsHistoryForDuplicatesPrevention", importer.SupportsHistoryForDuplicatesPrevention_ForTestOnly);
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSmtDataImportHistoryPreventsSameFileImport()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV001", TestObjectCreator.USD, 0.587623M);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.USD, 0.587623M, 7123.45M);
			Factory.Save();

			AssertNull("Pre-condition: No StmDataImportHistory records in Db", Factory.LoadTop1<StmDataImportHistory>(new ZQuery()));

			var pathToTestFile = PathToTestDir + "PayRemittanceWithHighExchangeRatePrecision.csv";
			using (var reader = new StreamReader(pathToTestFile))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				AssertNoExceptionThrown(() =>
				{
					importer.ImportData(reader, pathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);
				});

				Assert(!notificationBuffer.HasErrors);
			}

			using (var reader2 = new StreamReader(pathToTestFile))
			{
				var importer2 = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer2 = new NotificationBuffer();
				AssertNoExceptionThrown(() =>
				{
					AssertEquals(false, importer2.ImportData(reader2, pathToTestFile, notificationBuffer2, SourceInfo.EmptySourceInfo));
				});

				Assert(notificationBuffer2.HasErrors);
				AssertContains("A flat file with the same Hash was imported by E at 15-Jan-09 00:00:00 +00:00 with file name " + pathToTestFile, notificationBuffer2.AsString);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStmDataImportHistoryCleanUp()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV001", TestObjectCreator.USD, 0.587623M);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.USD, 0.587623M, 7123.45M);
			Factory.Save();

			var recordToPurge = Factory.New<StmDataImportHistory>();
			recordToPurge.DIH_GC = Environment.Env.CurrentCompanyPK;
			recordToPurge.DIH_ImportType = "Remittance";
			recordToPurge.DIH_DataHash = new ZBlob(new byte[] { 1, 2, 3 });
			recordToPurge.DIH_GS_NKImportStaff = "E";
			recordToPurge.DIH_ImportDateTime = ZDateTimeOffset.Today.AddDays(-7).AddSeconds(-1);

			var recordToKeep = Factory.New<StmDataImportHistory>();
			recordToKeep.DIH_GC = Environment.Env.CurrentCompanyPK;
			recordToKeep.DIH_ImportType = "Remittance";
			recordToKeep.DIH_DataHash = new ZBlob(new byte[] { 4, 5, 6 });
			recordToKeep.DIH_GS_NKImportStaff = "E";
			recordToKeep.DIH_ImportDateTime = ZDateTimeOffset.Today.AddDays(-7);

			var unrelatedRecord = Factory.New<StmDataImportHistory>();
			unrelatedRecord.DIH_GC = Environment.Env.CurrentCompanyPK;
			unrelatedRecord.DIH_ImportType = "TEST";
			unrelatedRecord.DIH_DataHash = new ZBlob(new byte[] { 7, 8, 9 });
			unrelatedRecord.DIH_GS_NKImportStaff = "E";
			unrelatedRecord.DIH_ImportDateTime = ZDateTimeOffset.Today.AddDays(-8);

			Factory.Save();
			var recordToPurgePK = recordToPurge.PK;
			var recordToKeepPK = recordToKeep.PK;
			var unrelatedRecordPK = unrelatedRecord.PK;

			var pathToTestFile = PathToTestDir + "PayRemittanceWithHighExchangeRatePrecision.csv";
			using (var reader = new StreamReader(pathToTestFile))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				AssertNoExceptionThrown(() =>
				{
					importer.ImportData(reader, pathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);
				});

				Assert(!notificationBuffer.HasErrors);
				var newRecords = Factory.Load<StmDataImportHistory>(new ZQuery(StmDataImportHistorySchema.PK, SQLComparisonOperator.NotEqual, new ZGuid[] { recordToPurgePK, recordToKeepPK, unrelatedRecordPK }));
				var newRecord = newRecords.FirstOrDefault();
				AssertNotNull("A new history record should be created", newRecord);
				AssertEquals("Imported file", pathToTestFile, newRecord.DIH_SourceDescription);
			}

			var newFactory = new BusinessObjectFactory();
			AssertNull("Old history record should be deleted", newFactory.Load<StmDataImportHistory>(recordToPurgePK));
			AssertNotNull("Not so old history record should be kept", newFactory.Load<StmDataImportHistory>(recordToKeepPK));
			AssertNotNull("Unrelated history record should be kept", newFactory.Load<StmDataImportHistory>(unrelatedRecordPK));
		}

		public void TestPrintRecieptCountForRemittanceFileImport()
		{
			var headerCollection = new Xsd.TxnHeaderCollection();
			var testStream = new MemoryStream();
			using (var writer = new StreamWriter(testStream))
			{
				writer.WriteLine("NCL,AP,JNL,20150115,20150116,AALSHI,Description,AUD,500,AUD,500,SYD,FEA");
				writer.WriteLine("PTR,AP,INV,00001003,-300,AALSHI");
				writer.WriteLine("PTR,AP,INV,00001004,-200,AALSHI");
				writer.WriteLine("NCL,AP,JNL,20150115,20150116,AALSHI,Description,AUD,500,AUD,500,SYD,FEA");
				writer.WriteLine("PTR,AP,INV,00001003,-300,AALSHI");
				writer.WriteLine("PTR,AP,INV,00001004,-200,AALSHI");
				writer.Flush();
				testStream.Position = 0;
				var processor = new PaymentReceiptRemittanceFileProcessor_ForTest(null, null);
				using (var reader = new StreamReader(testStream))
				{
					var notificationBuffer = new NotificationBuffer();
					var converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);
					processor.ProcessTxnHeaderCollection(headerCollection);
				}

				AssertEquals(1, processor.PrintReceiptPopupCounter_ForTestOnly);
			}
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_InsertMiscTransaction()
		{
			var overpaymentQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Overpayment);
			var overpaymentCountBeforeImport = Factory.Load<TransactionHeader>(overpaymentQuery).Length;

			var bankFeeQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			bankFeeQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCreatedByMatching, true);
			var bankFeeCountBeforeImport = Factory.Load<TransactionHeader>(bankFeeQuery).Length;

			var discountQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Discount);
			var discountCountBeforeImport = Factory.Load<TransactionHeader>(discountQuery).Length;

			var exchangeDifferenceQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference);
			var exchangeDifferenceCountBeforeImport = Factory.Load<TransactionHeader>(exchangeDifferenceQuery).Length;

			var pathToTestFile = PathToTestDir + "PayRecRemittance_InsertMiscTransaction.csv";
			using (var reader = new StreamReader(pathToTestFile))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				AssertNoExceptionThrown(() =>
				{
					importer.ImportData(reader, pathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);
				});

				Assert(!notificationBuffer.HasErrors);
			}

			var overpaymentCountAfterImport = Factory.Load<TransactionHeader>(overpaymentQuery).Length;
			AssertEquals("Should insert overpayment after import.", overpaymentCountBeforeImport + 1, overpaymentCountAfterImport);

			var bankFeeCountAfterImport = Factory.Load<TransactionHeader>(bankFeeQuery).Length;
			AssertEquals("Should insert bank fee journal after import.", bankFeeCountBeforeImport + 2, bankFeeCountAfterImport);

			var discountCountAfterImport = Factory.Load<TransactionHeader>(discountQuery).Length;
			AssertEquals("Should insert discount after import.", discountCountBeforeImport + 2, discountCountAfterImport);

			var exchangeDifferenceCountAfterImport = Factory.Load<TransactionHeader>(exchangeDifferenceQuery).Length;
			AssertEquals("Should insert exchangeDifference after import.", exchangeDifferenceCountBeforeImport + 2, exchangeDifferenceCountAfterImport);
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData_ErrorWhenPaymentContainOverpayment()
		{
			var pathToTestFile = PathToTestDir + "PayRecRemittance_ErrorWhenPaymentContainOverpayment.csv";
			using (var reader = new StreamReader(pathToTestFile))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				AssertNoExceptionThrown(() =>
				{
					importer.ImportData(reader, pathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);
				});

				Assert(notificationBuffer.HasErrors);
				NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer,
@$"Registry value for Organizations -> Allow Numeric Characters In Code Generation is currently Enabled.
Registry value for Organizations -> Use Default Organization for Matching is currently Disabled.
Current Organization Match Threshold - MED.
Current Organization Proxy - EDICUS.
Current Company - EDI.
Current Branch - SYD.

Begin processing Transaction AP PAY ABIGAS AUD 1: 
Successfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by {BrandingFactory.Instance.ProductName} Code: ABIGAS, Using: {BrandingFactory.Instance.ProductName} Code Matcher, Found match: True
Error: Overpayment transaction cannot be created for payment match group.
Error: Matching failed. There are no matched transaction.
  This transaction has errors and was not imported.

Saving the data to the database...
");
			}
		}
		class PaymentReceiptRemittanceDataImporter_ForTest : PaymentReceiptRemittanceDataImporter
		{
			public bool SupportsHistoryForDuplicatesPrevention_ForTestOnly => SupportsHistoryForDuplicatesPrevention;
			public int DaysToKeepHistoryFor_ForTestOnly => DaysToKeepHistoryFor;
			public string ImportTypeForDuplicatesPrevention_ForTestOnly => ImportTypeForDuplicatesPrevention;
			public SqlApplicationLock ImportLockWithHash_ForTestOnly => ImportLockWithHash;
		}

		class PaymentReceiptRemittanceFileProcessor_ForTest : PaymentReceiptRemittanceFileProcessor
		{
			public PaymentReceiptRemittanceFileProcessor_ForTest(BusinessObjectFactory factory, INotifications notifications) : base(factory, notifications)
			{
				PrintReceiptPopupCounter_ForTestOnly = 0;
			}

			public int PrintReceiptPopupCounter_ForTestOnly
			{
				get;
				private set;
			}

			protected override void PrintReceiptAndPayment()
			{
				PrintReceiptPopupCounter_ForTestOnly++;
			}
		}

		void setupInvoicesForImport()
		{
			invoice1 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV123456", testObjectCreator.AUD, 1M); // Matches on Transaction Num
			invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice1, testObjectCreator.AUD, 1M, 100M, 0M, 0M);
			invoice2 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV987654", testObjectCreator.AUD, 1M); // Matches on Transaction Num
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice2, testObjectCreator.AUD, 1M, 300M, 0M, 0M);
			invoice3 = testObjectCreator.CreateInvoice(typeof(APInvoice), "XYZ789", testObjectCreator.AUD, 1M); // Matches on Payment Reference
			invoice3.AH_ChequeOrReference = "APINV2";
			invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice3, testObjectCreator.AUD, 1M, 100M, 0M, 0M);

			invoice4 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", testObjectCreator.USD, 0.67M);
			invoice4.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice4, testObjectCreator.USD, 0.67M, 1500M, 0M, 0M);
			invoice5 = testObjectCreator.CreateInvoice(typeof(ARCreditNote), "00001000", testObjectCreator.USD, 0.67M);
			invoice5.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice5, testObjectCreator.USD, 0.67M, 800M, 0M, 0M);
			invoice6 = testObjectCreator.CreateInvoice(typeof(APCreditNote), "apcredit", testObjectCreator.USD, 0.67M);
			invoice6.AH_OH = TestObjectCreator.AALSHI.PK;
			testObjectCreator.CreateInvoiceLine(invoice6, testObjectCreator.USD, 0.67M, 300M, 0M, 0M);

			invoice7 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV07", testObjectCreator.USD, 0.67M);
			invoice7.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice7, testObjectCreator.USD, 0.67M, 1000M, 0M, 0M);
			invoice8 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV08", testObjectCreator.USD, 0.67M);
			invoice8.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice8, testObjectCreator.USD, 0.67M, 700M, 0M, 0M);
			invoice9 = testObjectCreator.CreateInvoice(typeof(APInvoice), "APINV09", testObjectCreator.USD, 0.67M);
			invoice9.AH_OH = TestObjectCreator.ABIGAS.PK;
			testObjectCreator.CreateInvoiceLine(invoice9, testObjectCreator.USD, 0.67M, 200M, 0M, 0M);

			Factory.Save();
		}

		void assertMatchedInvoices()
		{
			using (StreamReader reader = new StreamReader(PathToTestFile))
			{
				PaymentReceiptRemittanceDataImporter importer = new PaymentReceiptRemittanceDataImporter();
				NotificationBuffer notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, PathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);
				Assert(!notificationBuffer.HasWarnings);

				APPayment[] payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals(1, payment.Length);
				AssertEquals(500M, payment[0].AH_OSTotalAmount);
				AssertEquals(500M, payment[0].AH_LocalTotalAmount);
				AssertEquals(0M, payment[0].AH_Calc_OSOutstandingAmount);
				AssertEquals("000001", payment[0].AH_ChequeOrReference);
				AssertEquals(new ZDateTime(2009, 01, 15), payment[0].AH_PostDate);

				StmALog[] dataImportEvents = payment[0].Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
				AssertEquals("DIM event should be added.", 1, dataImportEvents.Length);

				AssertEquals(0M, invoice1.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice2.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice3.AH_Calc_OSOutstandingAmount);

				TransactionMatchLink matchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, payment[0].PK));
				AssertNotNull(matchLink);
				TransactionMatchLink invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice1.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice3.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);

				ARReceipt[] receipt = Factory.Load<ARReceipt>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals(1, receipt.Length);
				AssertEquals(1000M, receipt[0].AH_OSTotalAmount);
				AssertEquals(1500M, receipt[0].AH_LocalTotalAmount);
				AssertEquals(0M, receipt[0].AH_Calc_OSOutstandingAmount);
				AssertEquals("103", receipt[0].AH_ChequeOrReference);
				AssertEquals(new ZDateTime(2009, 01, 15), receipt[0].AH_PostDate);

				dataImportEvents = receipt[0].Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataImport.Code));
				AssertEquals("DIM event should be added.", 1, dataImportEvents.Length);

				AssertEquals(0M, invoice4.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice5.AH_Calc_OSOutstandingAmount);
				AssertEquals(0M, invoice6.AH_Calc_OSOutstandingAmount);

				matchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receipt[0].PK));
				AssertNotNull(matchLink);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice4.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice5.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);
				invoiceMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice6.PK));
				AssertNotNull(invoiceMatchLink);
				AssertEquals(matchLink.AP_MatchGroupNum, invoiceMatchLink.AP_MatchGroupNum);

				AssertEquals("Unrelated transation 7 should not be paid", 1000M, invoice7.AH_Calc_OSOutstandingAmount);
				AssertEquals("Unrelated transation 8 should not be paid", 700M, invoice8.AH_Calc_OSOutstandingAmount);
				AssertEquals("Unrelated transation 9 should not be paid", 200M, invoice9.AH_Calc_OSOutstandingAmount);

				ZQuery otherInvoicesFilter = new ZQuery(AccTransactionHeaderSchema.PK, new ZGuid[] { invoice7.PK, invoice8.PK, invoice9.PK });
				otherInvoicesFilter.FetchOnlyFromLocalCache = true;
				AssertEquals("Unrelated transactions should not be loaded during import", 0, importer.TestOnlyFirstFactoryUsed.Load<TransactionHeader>(otherInvoicesFilter).Length);

				AssertEquals("Match Document printing form should not be shown by default (w/o Print Event subscription).", "Enterprise.DocumentEngine.GUI.DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
			}
		}

		InvoicingBase invoice1;
		InvoicingBase invoice2;
		InvoicingBase invoice3;
		InvoicingBase invoice4;
		InvoicingBase invoice5;
		InvoicingBase invoice6;
		InvoicingBase invoice7;
		InvoicingBase invoice8;
		InvoicingBase invoice9;

		void setupSampleExchangeRate(BusinessObjectFactory factory, ZDecimal sellRate, ZString type, string currency = Core.Constants.CurrencyCodes.UnitedStates)
		{
			//set up exchange rates
			RefExchangeRate exRate = Factory.New<RefExchangeRate>();
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_ExRateType = type;
			exRate.RE_RX_NKExCurrency = currency;
			exRate.RE_StartDate = new ZDateTime(2009, 1, 1);
			exRate.RE_ExpiryDate = new ZDateTime(2009, 12, 31);
			exRate.RE_SellRate = sellRate;
			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		void SetupExchangeBuyRate(BusinessObjectFactory factory, ZString code, ZDecimal rate, string type = Core.Constants.ExchangeRateTypes.Code.BuyRate)
		{
			//set up exchange rates
			var rateBuy = RefCurrency.LoadFromCurrencyCode(factory, code).ExchangeRates.AddNew();
			rateBuy.RE_StartDate = new ZDateTime(2009, 1, 1);
			rateBuy.RE_ExpiryDate = new ZDateTime(2009, 12, 31);
			rateBuy.RE_ExRateType = type;
			rateBuy.RE_SellRate = rate;
			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		#region Overrides

		protected override string PathToTestFile
		{
			get { return PathToTestDir + "PayRecRemittance.csv"; }
		}

		protected override FlatFileDataImporter GetDataImporter()
		{
			return new PaymentReceiptRemittanceDataImporter();
		}

		protected override void SetUp()
		{
			base.SetUp();

			var sydBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			TempContext = Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sydBranch.PK.ToGuid(), Env.CurrentDepartmentPK);

			TestObjectCreator.CreateTestPeriods(new ZDateTime(2009, 01, 01));
			TestObjectCreator.ABIGAS.OH_IsCreditor = true;
			TestObjectCreator.AALSHI.OH_IsDebtor = true;

			TestObjectCreator.AUDBankAccount.AB_Code = "AUD";
			TestObjectCreator.AUDChequeBook.AK_Code = "AUDC";
			TestObjectCreator.AUDChequeBook.AK_GB = GlbBranch.CurrentBranch.PK;

			AccChequeBook duplicateAUDCChequeBook = TestObjectCreator.CreateChequeBook("Duplicate AUDC Cheque Book", 1, TestObjectCreator.USDBankAccount);
			duplicateAUDCChequeBook.AK_Code = "AUDC";

			TestObjectCreator.USDBankAccount.AB_Code = "USD";
			TestObjectCreator.USDChequeBook.AK_Code = "USDC";
			TestObjectCreator.USDChequeBook.AK_GB = GlbBranch.CurrentBranch.PK;

			JPYBankAccount = TestObjectCreator.CreateBankAccount("JPY", "Japanese Yen Account", RefCurrency.LoadFromCurrencyCode(Factory, "JPY"), TestObjectCreator.GLHeader1);
			JPYChequeBook = TestObjectCreator.CreateChequeBook("JPY Cheque Book", 100, JPYBankAccount);
			JPYChequeBook.AK_Code = "JPYC";
			JPYChequeBook.AK_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();
		}

		IDisposable TempContext;

		protected override void TearDown()
		{
			base.TearDown();

			TempContext?.Dispose();
		}

		AccBankAccount JPYBankAccount;
		AccChequeBook JPYChequeBook;

		#endregion

		NettingObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new NettingObjectCreator(Factory)); }
		}
		NettingObjectCreator testObjectCreator;

		NotificationTestHelper NotificationTestHelper
		{
			get { return notificationTestHelper ?? (notificationTestHelper = new NotificationTestHelper()); }
		}
		NotificationTestHelper notificationTestHelper;

		string PathToTestDir
		{
			get { return BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\PaymentReceiptRemittanceFile\Testing\"; }
		}

		bool PrintMatchingDocument;

		void Importer_OnPrintMatchingDocument(object sender, BoolResponseEventArgs e)
		{
			e.Response = PrintMatchingDocument;
		}

		void AssertReceiptPaymentByDescription(ReceiptPaymentBase[] receiptPayments, string description, RefCurrency currency, ZDecimal osAmount, ZDecimal localAmount)
		{
			var receiptPayment = receiptPayments.FirstOrDefault((payment) => payment.AH_Desc == description);
			var messageBase = string.Format("Receipt or payment with description '{0}'", description);

			AssertNotNull(messageBase + " should be created.", receiptPayment);
			AssertEquals(messageBase + ": AH_OSTotalAmount", osAmount, receiptPayment.AH_OSTotalAmount);
			AssertEquals(messageBase + ": AH_LocalTotalAmount", localAmount, receiptPayment.AH_LocalTotalAmount);
			AssertEquals(messageBase + ": AH_Calc_OSOutstandingAmount", 0M, receiptPayment.AH_Calc_OSOutstandingAmount);
			AssertEquals(messageBase + ": AH_RX_NKTransactionCurrency", currency.RX_Code, receiptPayment.AH_RX_NKTransactionCurrency);
		}

		[TestDate(2009, 01, 15)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLocalAmountForPaymentIsSameAsValueInCsv()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV001", TestObjectCreator.USD, 0.100836M);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.USD, 0.100836M, 30111279M);
			Factory.Save();

			var pathToTestFile = PathToTestDir + "APPAY_LocalAmountMatchValueInCSV.csv";
			using (var reader = new StreamReader(pathToTestFile))
			{
				var importer = new PaymentReceiptRemittanceDataImporter();
				var notificationBuffer = new NotificationBuffer();
				importer.ImportData(reader, pathToTestFile, notificationBuffer, SourceInfo.EmptySourceInfo);

				Assert(!notificationBuffer.HasErrors);

				var payment = Factory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals(1, payment.Length);
				AssertEquals(3036294M, payment[0].AH_LocalTotalAmount);
			}
		}
	}
}

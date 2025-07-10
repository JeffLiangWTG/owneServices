using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.CountryCompliance.Interfaces.DataObjects;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using AuthRecordConstants = Enterprise.Accounting.Integration.DataTransferConstants.AccTransactionHeaderAuthorisationRecord;
using GenericJob = Enterprise.Accounting.Business.GenericJob.GenericJob;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	public class TransactionDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestGetEDocForRelatedBusinessObjects()
		{
			var arInvoice = createARInvoice();
			Factory.Save();
			var invoiceeDoc1 = ((IDocManagerSupport)arInvoice).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TestInvoiceFile1", "INV");
			var invoiceeDoc2 = ((IDocManagerSupport)arInvoice).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 4 }), "TestInvoiceFile2", "INV");
			invoiceeDoc2.IsPublished = true;
			var invoiceeDoc3 = ((IDocManagerSupport)arInvoice).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 5 }), "TestInvoiceFile3", "INV");
			invoiceeDoc3.IsPublished = true;

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
			using (var transactionDataObject = writer.GetDataObject(arInvoice))
			{
				CombineAssertions("logData Contents", delegate
				{
					AssertEquals("number of documents", 2, transactionDataObject.AttachedDocumentCollection.Count);
					AssertMultilineASCIIEquals("eventData.AttachedDocumentCollection", @"
INV - Invoice - TestInvoiceFile2 - Y
INV - Invoice - TestInvoiceFile3 - Y
".Trim(), string.Join("\r\n", transactionDataObject.AttachedDocumentCollection.Select(doc => { return doc.Type.Code + " - " + doc.Type.Description + " - " + doc.FileName + " - " + doc.IsPublished; }).ToArray()));
					AssertArrayEqualsByElements(new ZBlob(new byte[] { 1, 2, 4 }), transactionDataObject.AttachedDocumentCollection[0].ImageData.ToByteArray());
					AssertArrayEqualsByElements(new ZBlob(new byte[] { 1, 2, 5 }), transactionDataObject.AttachedDocumentCollection[1].ImageData.ToByteArray());
				});
			}
		}

		#region ARInvoice

		public void TestARInvoice()
		{
			var arInvoice = createARInvoice();
			Factory.Save();
			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
			var transactionDataObject = writer.GetDataObject(arInvoice);

			AssertTransactionDataObject(arInvoice, transactionDataObject, false, null, true);
		}

		public void TestARInvoiceWithShipment()
		{
			var arInvoice = createARInvoiceWithShipment();
			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
			var transactionDataObject = writer.GetDataObject(arInvoice);

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertEquals("GenericJob should not be loaded into the invoice factory.", 0, ((IBusinessObjectFactoryInternals)arInvoice.Factory).AllBusinessObjects.OfType<GenericJob>().Count());
			AssertTransactionDataObject(arInvoice, transactionDataObject);
			AssertRelatedShipment(arInvoice, transactionDataObject);
		}

		public void TestInvoiceSourceIsConsol()
		{
			var invoice = createARInvoice();
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			consol.JK_MasterBillNum = "TEST12345";
			invoice.AH_ConsolidatedInvoiceRef = "C001";
			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBill = "TESTSHIPMENT00012";
			shipment.Consols.Add(consol);
			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)));
			var transactionDataObject = writer.GetDataObject(invoice);

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertEquals("ConsolCosting.ApportionmentListing should not be loaded into the invoice factory.", 0, ((IBusinessObjectFactoryInternals)invoice.Factory).AllBusinessObjects.OfType<ApportionmentListing>().Count());
			AssertRelatedConsol(invoice, transactionDataObject);
		}

		[TestDate(2017, 04, 13)]
		public void TestInvoiceWithConsolLines()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var job = TestObjectCreator.CreateJob(shipment);
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1.0m, 10m, 10m, 0m);
			invoice.AH_PostDate = new ZDateTime(2009, 06, 15);
			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSSellAmt = 0M;
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = job.PK;
			line.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			charge.JR_AL_APLine = line.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(invoice, consol, TestObjectCreator.CC1, 10);
			invoice.ImportSingleCost(consolCost, line);

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)));
			var transactionDataObject = writer.GetDataObject(invoice);

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertEquals("ConsolCosting.ApportionmentListing should not be loaded into the invoice factory.", 0, ((IBusinessObjectFactoryInternals)invoice.Factory).AllBusinessObjects.OfType<ApportionmentListing>().Count());
			AssertRelatedConsolLines(invoice, transactionDataObject);
		}

		public void TestCashBasisVATARInvoice()
		{
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 100, 10);
			Factory.Save();
			invoice.Reload();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)));
			var transactionDataObject = writer.GetDataObject(invoice);

			AssertTransactionDataObject(invoice, transactionDataObject, isCashBasisVAT: true);

			var expectedCashVAtDate = ZDateTime.Today.AddDays(-2);
			TestObjectCreator.CreateMatchLinkToPayARInvoice(invoice, expectedCashVAtDate, 30);
			Factory.Save();

			var cashVATs = Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_AL_TransactionLine, invoice.Lines[0].PK));
			AssertEquals("Precondition: cashVATs count", 1, cashVATs.Length);

			var cashVAT = cashVATs[0];
			AssertNotNull("Precondition: cashVAT", cashVAT);
			AssertEquals("Precondition: YC_PostDate", expectedCashVAtDate, cashVAT.YC_PostDate);
			AssertEquals("Precondition: YC_PostDate", 2.73m, cashVAT.YC_TaxAmount);

			transactionDataObject = writer.GetDataObject(invoice);
			AssertTransactionDataObject(invoice, transactionDataObject, isCashBasisVAT: true, cashVAT: cashVAT);
		}

		public void TestARInvoice_AgreedPaymentMethod()
		{
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARInvoice), 100, 10);
			invoice.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer;
			Factory.Save();
			invoice.Reload();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)));
			var transactionDataObject = writer.GetDataObject(invoice);

			AssertEquals("AH_AgreedPaymentMethodOverride should be equal.", invoice.AH_AgreedPaymentMethodOverride, transactionDataObject.AgreedPaymentMethod);
		}

		public void TestARInvoiceForRoundingError()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				Factory.Save();
				var arInvoice = CreateARInvoiceWithSubUnitRoundingErrorCheck();
				var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
				var transactionDataObject = writer.GetDataObject(arInvoice);
				AssertEquals("OSExGSTVATAmount should be equal to 200.000", (ZDecimal)200.000, transactionDataObject.OSExGSTVATAmount);
				AssertEquals("OSTaxAmount should be equal to 28.000", (ZDecimal)28.000, transactionDataObject.PostingJournalCollection[0].OSGSTVATAmount);
				AssertEquals("OSTotal - OSGSTVATAmount should be 200.000", (ZDecimal)200.000, transactionDataObject.PostingJournalCollection[0].OSTotalAmount - transactionDataObject.PostingJournalCollection[0].OSGSTVATAmount);
			}
		}

		ARInvoice CreateARInvoiceWithSubUnitRoundingErrorCheck()
		{
			var localCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "ZAR");
			var oSCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "TND");
			var debtor = TestObjectCreator.ABIGAS;
			Factory.Save();
			var exchangeRate = 1 / 5.6019M;
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("002", oSCurrency, exchangeRate, debtor);
			arInvoice.AH_PostDate = new ZDateTime(2017, 03, 08);
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(arInvoice, oSCurrency, 1120.38m, 156.85m, 200.000m, 28.000m);
			line.AL_ExchangeRate = 5.6019M;
			line.AL_OSExTaxAmount = 200.000M;
			line.AL_OSTaxAmount = 28.000M;
			line.AL_OverseasTotal = 228.000M;
			line.AL_LocalExTaxAmount = 1120.38M;
			line.AL_LocalTaxAmount = 156.8532M;
			Factory.Save();
			return arInvoice;
		}

		public void TestARInvoiceWithShipment_OSGSTVATAmount()
		{
			var arInvoice = createARInvoiceWithMultiLines();
			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
			var transactionDataObject = writer.GetDataObject(arInvoice);

			AssertEquals("OSGSTVATAmount should be equal.", arInvoice.AH_OSTaxAmount, transactionDataObject.OSGSTVATAmount);
		}

		public void TestARInvoice_ExtraVATAmount()
		{
			var arInvoice = createARInvoiceWithExtraVATAmount();
			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
			var transactionDataObject = writer.GetDataObject(arInvoice);

			AssertExtraVATAmount(arInvoice, transactionDataObject);
		}

		[TestDate(2017, 10, 18)]
		public void TestARInvoice_ExtraVATAmount_IndiaStateTax()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				ARInvoice arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.USD, 1.2m, TestObjectCreator.ABIGAS);
				arInvoice.AH_PostDate = new ZDateTime(2009, 06, 15);

				var stateGST = TestObjectCreator.STAGST;
				var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 2m, 100.05m, 18m, 0m);
				line.AL_AT = stateGST.PK;

				Factory.Save();

				var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
				var transactionDataObject = writer.GetDataObject(arInvoice);

				AssertExtraVATAmount(arInvoice, transactionDataObject);
			}
		}

		[TestDate(2019, 05, 01)]
		public void TestARInvoice_ExtraVATAmount_ItalySPVTax()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				ARInvoice arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.USD, 2.8765M, TestObjectCreator.ABIGAS);

				var line1 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 2.8765M, 27.95M, GlbBranch.CurrentBranch.PK, false);
				line1.AL_AT = TestObjectCreator.VATSPV.PK;

				var line2 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 2.8765M, 27.98M, GlbBranch.CurrentBranch.PK, false);
				line2.AL_AT = TestObjectCreator.VATSPV.PK;

				Factory.Save();

				var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
				var transactionDataObject = writer.GetDataObject(arInvoice);

				AssertExtraVATAmount(arInvoice, transactionDataObject);
			}
		}

		[TestDate(2019, 05, 01)]
		public void TestARInvoice_ExtraVATAmount_ItalySPVTax_LocalExtraTaxIsNotPersistant()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				ARInvoice arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.EUR, 1m, TestObjectCreator.ABIGAS);

				var line1 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 1m, 27.95M, GlbBranch.CurrentBranch.PK, false);
				line1.AL_AT = TestObjectCreator.VATSPV.PK;

				var line2 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.EUR, 1m, 27.98M, GlbBranch.CurrentBranch.PK, false);
				line2.AL_AT = TestObjectCreator.VATSPV.PK;

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				InvoicingBase invoiceReloaded = newFactory.Load<InvoicingBase>(arInvoice.PK);
				AssertEquals(2, invoiceReloaded.Lines.Count);

				Assert("Local tax extra tax amount is persistant", invoiceReloaded.Lines[0].AL_GSTVATExtra != 0);
				Assert("Local tax extra tax amount is persistant", invoiceReloaded.Lines[1].AL_GSTVATExtra != 0);

				invoiceReloaded.Lines[0].AL_GSTVATExtra = invoiceReloaded.Lines[1].AL_GSTVATExtra = 0;
				newFactory.Save();

				var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
				var transactionDataObject = writer.GetDataObject(arInvoice);

				transactionDataObject.PostingJournalCollection.OrderBy(x => x.OSAmount);

				AssertEquals(-6.15M, transactionDataObject.PostingJournalCollection[0].LocalExtraVATAmount);
				AssertEquals(-6.15M, transactionDataObject.PostingJournalCollection[0].OSExtraVATAmount);
				AssertEquals(-6.16M, transactionDataObject.PostingJournalCollection[1].LocalExtraVATAmount);
				AssertEquals(-6.16M, transactionDataObject.PostingJournalCollection[1].OSExtraVATAmount);
			}
		}

		[TestDate(2009, 05, 01)]
		public void TestTaxTransactionsForPostedTransactions_WithGLMovements()
		{
			SetupDataWithTaxFramework(typeof(ARInvoice), "INV010101", 100M);

			AR_Invoice.MarkAsNeedingValidationIncludingChildren();
			AR_Invoice.RunPreSaveValidation();
			Assert("Precondition: AR_Invoice.HasErrors", !AR_Invoice.HasErrors);
			Factory.Save();

			AssertUniversalTransaction_WithGLMovements_HasTaxTransactions(AR_Invoice);
		}

		[TestDate(2009, 05, 01)]
		public void TestTaxTransactionsForReversedTransaction_WithGLMovements()
		{
			SetupDataWithTaxFramework(typeof(ARInvoice), "INV010101", 100M);

			AR_Invoice.MarkAsNeedingValidationIncludingChildren();
			AR_Invoice.RunPreSaveValidation();
			Assert("Precondition: AR_Invoice.HasErrors", !AR_Invoice.HasErrors);
			Factory.Save();

			var reversedTransaction = TestObjectCreator.ReverseTransaction(AR_Invoice, out string message);
			Assert("Precondition:", message.IsNullOrEmpty());
			reversedTransaction.MarkAsNeedingValidationIncludingChildren();
			reversedTransaction.RunPreSaveValidation();
			Assert("Precondition: reversedTransaction.HasErrors", !reversedTransaction.HasErrors());
			Factory.Save();

			var creditNote = Factory.Load<ARCreditNote>(reversedTransaction.PK);
			Assert("Precondition: ", creditNote.IsCancelled);
			AssertUniversalTransaction_WithGLMovements_HasTaxTransactions(creditNote);
		}

		void AssertUniversalTransaction_WithGLMovements_HasTaxTransactions(InvoicingBase invoice)
		{
			var taxTransactions = GetTaxTransactions(invoice.PK);
			AssertEquals("Precondition: Tax Transactions for a transaction", 1, taxTransactions.Length);
			AssertEquals("Precondition: GL Movements for a tax transaction", 1, GetGLMovements(taxTransactions[0].PK).Length);

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)));
			var transactionDataObject = writer.GetDataObject(invoice);

			CombineAssertions(() =>
			{
				AssertNotNull("TaxTransactionCollection should be populated.", transactionDataObject.TaxTransactionCollection);
				AssertEquals("1 TaxTransaction in TaxTransactionCollection.", 1, transactionDataObject.TaxTransactionCollection.Count);
				AssertNotNull("PostingJournalDetailCollection should be populated.", transactionDataObject.TaxTransactionCollection[0].PostingJournalDetailCollection);
				AssertEquals("1 PostingJournalDetail should be populated.", 1, transactionDataObject.TaxTransactionCollection[0].PostingJournalDetailCollection.Count);

				AssertNotNull("PostingJournalCollection should be populated.", transactionDataObject.PostingJournalCollection);
				AssertEquals("1 PostingJournal in PostingJournalCollection.", 1, transactionDataObject.PostingJournalCollection.Count);
				AssertNotNull("TaxTransactionLinkCollection should be populated in PostingJournal.", transactionDataObject.PostingJournalCollection[0].TaxTransactionLinkCollection);
				AssertEquals("1 TaxTransactionLink in TaxTransactionLinkCollection.", 1, transactionDataObject.PostingJournalCollection[0].TaxTransactionLinkCollection.Count);
			});
		}

		[TestDate(2009, 05, 01)]
		public void TestTaxTransactionsForPostedTransactions_WithoutGLMovements()
		{
			SetupDataWithTaxFramework(typeof(ARInvoice), "INV010101", 0.02M);

			AR_Invoice.MarkAsNeedingValidationIncludingChildren();
			AR_Invoice.RunPreSaveValidation();
			Assert("Precondition: AR_Invoice.HasErrors", !AR_Invoice.HasErrors);
			Factory.Save();

			AssertUniversalTransaction_WithoutGLMovements_HasTaxTransactions(AR_Invoice);
		}

		[TestDate(2009, 05, 01)]
		public void TestTaxTransactionsForReversedTransaction_WithoutGLMovments()
		{
			SetupDataWithTaxFramework(typeof(ARInvoice), "INV010101", 0.02M);

			AR_Invoice.MarkAsNeedingValidationIncludingChildren();
			AR_Invoice.RunPreSaveValidation();
			Assert("Precondition: AR_Invoice.HasErrors", !AR_Invoice.HasErrors);
			Factory.Save();

			var reversedTransaction = TestObjectCreator.ReverseTransaction(AR_Invoice, out string message);
			Assert("Precondition:", message.IsNullOrEmpty());
			reversedTransaction.MarkAsNeedingValidationIncludingChildren();
			reversedTransaction.RunPreSaveValidation();
			Assert("Precondition: reversedTransaction.HasErrors", !reversedTransaction.HasErrors());
			Factory.Save();

			var creditNote = Factory.Load<ARCreditNote>(reversedTransaction.PK);
			Assert("Precondition: ", creditNote.IsCancelled);
			AssertUniversalTransaction_WithoutGLMovements_HasTaxTransactions(creditNote);
		}

		void AssertUniversalTransaction_WithoutGLMovements_HasTaxTransactions(InvoicingBase invoice)
		{
			var taxTransactions = GetTaxTransactions(invoice.PK);
			AssertEquals("Precondition: Tax Transactions for a transaction", 1, taxTransactions.Length);
			AssertEquals("Precondition: GL Movements for a tax transaction", 0, GetGLMovements(taxTransactions[0].PK).Length);

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)));
			var transactionDataObject = writer.GetDataObject(invoice);

			CombineAssertions(() =>
			{
				AssertNotNull("TaxTransactionCollection should be populated.", transactionDataObject.TaxTransactionCollection);
				AssertEquals("1 Tax Transaction in TaxTransactionCollection.", 1, transactionDataObject.TaxTransactionCollection.Count);
				AssertNull("PostingJournalDetailCollection should not be populated.", transactionDataObject.TaxTransactionCollection[0].PostingJournalDetailCollection);

				AssertNotNull("PostingJournalCollection should be populated.", transactionDataObject.PostingJournalCollection);
				AssertEquals("1 PostingJournal in PostingJournalCollection.", 1, transactionDataObject.PostingJournalCollection.Count);
				AssertNotNull("TaxTransactionLinkCollection should be populated in PostingJournal.", transactionDataObject.PostingJournalCollection[0].TaxTransactionLinkCollection);
				AssertEquals("1 TaxTransactionLink in TaxTransactionLinkCollection.", 1, transactionDataObject.PostingJournalCollection[0].TaxTransactionLinkCollection.Count);
			});
		}

		public void TestARInvoiceCashAdvanceReceived()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			var arInvoiceLine = TestObjectCreator.CreateARInvoiceLine(arInvoice, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 100M);
			arInvoiceLine.AL_JH = TestObjectCreator.Job1.PK;

			var charge = TestObjectCreator.CreateJobCharge(arInvoiceLine, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(charge.Job as Job, TestObjectCreator.Debtor, LedgerTypes.AccountsReceivable, 300M, 300M, charge.JR_RX_NKSellCurrency);

			var cal = TestObjectCreator.CreateCashAdvanceRequestLine(cah, charge.JR_LocalSellAmt, charge.JR_OSSellAmt);
			charge.JR_CAL_ARLine = cal.PK;
			cal.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			cal.CAL_LocalPaidAmount = 100M;
			cal.CAL_OSPaidAmount = 100M;

			var line1 = TestObjectCreator.CreateARInvoiceLine(arInvoice, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 200M);
			line1.AL_JH = TestObjectCreator.Job1.PK;
			var charge1 = TestObjectCreator.CreateJobCharge(line1, TestObjectCreator.Job1, TestObjectCreator.CommentChargeCode, TestObjectCreator.AUD);
			var cal1 = TestObjectCreator.CreateCashAdvanceRequestLine(cah, charge1.JR_LocalSellAmt, charge1.JR_OSSellAmt);
			charge1.JR_CAL_ARLine = cal1.PK;
			cal1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
			var transactionDataObject = writer.GetDataObject(arInvoice);
			AssertNotNull("Total CashAdvanceAmount should be populated.", transactionDataObject.TotalCashAdvanceAmount);
			AssertEquals("Total CashAdvanceAmount", 300M, transactionDataObject.TotalCashAdvanceAmount);

			AssertNotNull("PostingJournalCollection should be populated.", transactionDataObject.PostingJournalCollection);
			AssertEquals("2 PostingJournals in PostingJournalCollection.", 2, transactionDataObject.PostingJournalCollection.Count);
			AssertEquals("CashAdvanceAmount in first PostingJournal", 100M, transactionDataObject.PostingJournalCollection[0].CashAdvanceAmount);
			AssertEquals("CashAdvanceAmount in second PostingJournal", 200M, transactionDataObject.PostingJournalCollection[1].CashAdvanceAmount);
		}

		ARInvoice createARInvoice()
		{
			ARInvoice arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			arInvoice.AH_PostDate = new ZDateTime(2009, 06, 15);
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m);
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			line.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;

			return arInvoice;
		}

		ARInvoice createARInvoiceWithShipment()
		{
			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001001"));
			ARInvoice arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m);
			arInvoice.AH_PostDate = new ZDateTime(2009, 06, 15);
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSSellAmt = 0M;
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_JH = job.PK;
			line.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			charge.JR_AL_ARLine = line.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			Factory.Save();

			return arInvoice;
		}

		ARInvoice createARInvoiceWithMultiLines()
		{
			ARInvoice arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.USD, 1.1365m, TestObjectCreator.ABIGAS);
			arInvoice.AH_PostDate = new ZDateTime(2009, 06, 15);

			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 1.1365m, 87.02m, 16.53m, 0m);
			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 1.1365m, 821.90m, 156.16m, 0m);
			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 1.1365m, 127.00m, 24.13m, 0m);
			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 1.1365m, 1365.00m, 259.35m, 0m);
			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 1.1365m, 106.84m, 20.30m, 0m);
			TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 1.1365m, 1187.99m, 225.72m, 0m);

			Factory.Save();

			return arInvoice;
		}

		ARInvoice createARInvoiceWithExtraVATAmount()
		{
			ARInvoice arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.USD, 1.2m, TestObjectCreator.ABIGAS);
			arInvoice.AH_PostDate = new ZDateTime(2009, 06, 15);

			ZQuery taxRateQuery = new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			taxRateQuery.AddToFilter(AccTaxRateSchema.AT_Type, AccTaxRate.Types.Rated);
			AccTaxRate taxRate = Factory.Load<AccTaxRate>(taxRateQuery).First(x => x.GetRate_ForTestOnly() == 10);
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
			taxRate.SetExtraRate_ForTestOnly(2, 1);

			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Italy;
			taxRate2.AT_Type = "RAT";
			taxRate2.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;
			taxRate2.SetRateNumerator_ForTestOnly(10);
			taxRate2.SetExtraRate_ForTestOnly(2, 1);

			var taxRate3 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate3.AT_RN_NKCountry = Core.Constants.CountryCodes.CostaRica;
			taxRate3.AT_Type = "RAT";
			taxRate3.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;
			taxRate2.SetRateNumerator_ForTestOnly(15);
			taxRate2.SetExtraRate_ForTestOnly(0, 1);

			var exempt = TestObjectCreator.CreateTaxRate("EXEMPT", "Exempt TAX", AccTaxRate.Types.Exempt, 0, AccTaxRate.ExtraTypes.ServiceTax, 0, 1);

			AccTaxRate stateGST = null;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				stateGST = TestObjectCreator.STAGST;
			}

			var line1 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 1.2m, 100m, 10m, 0m);
			var line2 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 1.2m, 200m, 20m, 0m);
			var line3 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 1.2m, 300m, 0m, 30m);
			var line4 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 1.2m, 300m, 0m, 0m);
			var line5 = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.USD, 1.2m, 400m, 0m, 0m);

			line1.AL_AT = taxRate.PK;
			line2.AL_AT = stateGST.PK;
			line3.AL_AT = taxRate2.PK;
			line4.AL_AT = exempt.PK;
			line5.AL_AT = taxRate3.PK;

			Factory.Save();

			return arInvoice;
		}

		#endregion

		#region ARCreditNote

		public void TestARCreditNote()
		{
			var arCreditNote = createARCreditNote();
			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arCreditNote)));
			var transactionDataObject = writer.GetDataObject(arCreditNote);

			AssertTransactionDataObject(arCreditNote, transactionDataObject);
			AssertRelatedShipment(arCreditNote, transactionDataObject);
		}

		public void TestCashBasisVATARCreditNote()
		{
			var creditNote = (ARCreditNote)TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(ARCreditNote), 100, 10);
			Factory.Save();
			creditNote.Reload();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, creditNote)));
			var transactionDataObject = writer.GetDataObject(creditNote);

			AssertTransactionDataObject(creditNote, transactionDataObject, isCashBasisVAT: true);

			var expectedCashVAtDate = ZDateTime.Today.AddDays(-2);
			TestObjectCreator.CreateMatchLinkToPayAPInvoice(creditNote, expectedCashVAtDate, -30);
			Factory.Save();

			var cashVATs = Factory.Load<AccCashBasisVAT>(new ZQuery(AccCashBasisVATSchema.YC_AL_TransactionLine, creditNote.Lines[0].PK));
			AssertEquals("Precondition: cashVATs count", 1, cashVATs.Length);

			var cashVAT = cashVATs[0];
			AssertNotNull("Precondition: cashVAT", cashVAT);
			AssertEquals("Precondition: YC_PostDate", expectedCashVAtDate, cashVAT.YC_PostDate);
			AssertEquals("Precondition: YC_PostDate", -2.73m, cashVAT.YC_TaxAmount);

			transactionDataObject = writer.GetDataObject(creditNote);
			AssertTransactionDataObject(creditNote, transactionDataObject, isCashBasisVAT: true, cashVAT: cashVAT);
		}

		ARCreditNote createARCreditNote()
		{
			ZDateTime date = new ZDateTime(2009, 06, 15);
			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00001001"));
			ARCreditNote arCreditNote = TestObjectCreator.CreateARCreditNoteWithLine("002", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1.0m, "Desc", job, TestObjectCreator.CC1, 1000.00m, date, false);
			arCreditNote.AH_PostDate = date;
			Charge charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OSSellAmt = 0M;
			arCreditNote.Lines[0].AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			arCreditNote.Lines[0].AL_JH = job.PK;
			charge.JR_AL_ARLine = arCreditNote.Lines[0].PK;
			charge.SetAmountsFromLinkedLinesForTests();
			Factory.Save();

			return arCreditNote;
		}

		#endregion

		#region AP Invoice
		public void TestAPInvoiceCashAdvanceReceived()
		{
			var apInvoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m);
			apInvoice.AH_TransactionNum = "111";
			apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;

			InvoicingLineBase apInvoiceLine = TestObjectCreator.CreateAPInvoiceLine(apInvoice, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 100M);
			apInvoiceLine.AL_JH = TestObjectCreator.Job1.PK;

			var charge = TestObjectCreator.CreateJobCharge(apInvoiceLine, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
			var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(TestObjectCreator.Job1, TestObjectCreator.Debtor, LedgerTypes.AccountsPayable, 300M, 300M, charge.JR_RX_NKCostCurrency);

			var cal = TestObjectCreator.CreateCashAdvanceRequestLine(cah, charge.JR_LocalCostAmt, charge.JR_OSCostAmt);
			charge.JR_CAL_APLine = cal.PK;
			cal.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			cal.CAL_LocalPaidAmount = 100M;
			cal.CAL_OSPaidAmount = 100M;

			InvoicingLineBase line1 = TestObjectCreator.CreateAPInvoiceLine(apInvoice, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 200M);
			line1.AL_JH = TestObjectCreator.Job1.PK;
			var charge1 = TestObjectCreator.CreateJobCharge(line1, TestObjectCreator.Job1, TestObjectCreator.CommentChargeCode, TestObjectCreator.AUD);
			var cal1 = TestObjectCreator.CreateCashAdvanceRequestLine(cah, charge1.JR_LocalCostAmt, charge1.JR_OSCostAmt);
			charge1.JR_CAL_APLine = cal1.PK;
			cal1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, apInvoice)));
			var transactionDataObject = writer.GetDataObject(apInvoice);
			AssertNotNull("Total CashAdvanceAmount should be populated.", transactionDataObject.TotalCashAdvanceAmount);
			AssertEquals("Total CashAdvanceAmount", 300M, transactionDataObject.TotalCashAdvanceAmount);

			AssertNotNull("PostingJournalCollection should be populated.", transactionDataObject.PostingJournalCollection);
			AssertEquals("2 PostingJournals in PostingJournalCollection.", 2, transactionDataObject.PostingJournalCollection.Count);
			AssertEquals("CashAdvanceAmount in first PostingJournal", 100M, transactionDataObject.PostingJournalCollection[0].CashAdvanceAmount);
			AssertEquals("CashAdvanceAmount in second PostingJournal", 200M, transactionDataObject.PostingJournalCollection[1].CashAdvanceAmount);
		}

		#endregion
		#region ARAdjusmentNote

		public void TestARAdjusmentNote()
		{
			var arAdjusmentNote = createARAdjusmentNote();
			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arAdjusmentNote)));
			var transactionDataObject = writer.GetDataObject(arAdjusmentNote);

			AssertTransactionDataObject(arAdjusmentNote, transactionDataObject);
		}

		ARAdjustmentNote createARAdjusmentNote()
		{
			ARAdjustmentNote positiveARAdjustmentNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("001", 100.00m, 10.00m, new ZDateTime(2008, 06, 15), TestObjectCreator.ABIGAS.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(positiveARAdjustmentNote, TestObjectCreator.CC1.PK, 100.00m, 10.00m);
			Factory.Save();

			return positiveARAdjustmentNote;
		}

		#endregion

		#region TransactionWithAuthorizationDetails

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionWithAuthorizationDetails_AR_NullDetails_OrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			Factory.Save();
			arInvoice.Reload();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
			var transactionDataObject = writer.GetDataObject(arInvoice);

			AssertNull("No auth record should not create an empty collection in XUT.", transactionDataObject.AuthorizationDetailCollection);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionWithAuthorizationDetails_AR_EmptyDetails_OrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Egypt
			);
			Factory.Save();
			arInvoice.Reload();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
			var transactionDataObject = writer.GetDataObject(arInvoice);

			AssertNull("Auth record with entirely empty placeholder details should not be included in XUT.", transactionDataObject.AuthorizationDetailCollection);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionWithAuthorizationDetails_AR_MinimalDetails_OrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Egypt,
				number: "6894732"
			);
			Factory.Save();
			arInvoice.Reload();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
			var transactionDataObject = writer.GetDataObject(arInvoice);

			AssertAuthorisationRecord("Authorization record with at least one relevant field should be included in XUT.",
				transactionDataObject.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "EG", "1"),
				expectedNumber: "6894732"
			);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionWithAuthorizationDetails_AR_MinimalDetails_NonOrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Egypt,
				number: "6894732"
			);
			Factory.Save();
			arInvoice.Reload();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.YIA, arInvoice)));
			var transactionDataObject = writer.GetDataObject(arInvoice);

			AssertNull("When Recipient Role is not Org Proxy, authorization details should not be included in XUT.", transactionDataObject.AuthorizationDetailCollection);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionWithAuthorizationDetails_AR_FullDetails_OrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			var twoKRandomBits = new byte[2048 / 8];
			new Random(1).NextBytes(twoKRandomBits);

			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Uruguay,
				number: "6534324",
				counter: "8",
				dateTime: new DateTimeOffset(2019, 3, 5, 13, 32, 53, 562, TimeSpan.FromHours(-6)),
				verificationUrl: "http://some.url.com/txn/68763254259",
				publicKey: new ZBlob(twoKRandomBits),
				authorisationData: ZBlob.FromUTF8("{ arbitrary: \"data\" }"),
				transactionHash: System.Security.Cryptography.SHA512.Create().ComputeHash(ZBlob.FromUTF8("6894732")),
				debtorNumber: "AB8882",
				issuerCertificateIdentifier: "GoDaddy Certificate Authority 1592",
				placeOfIssue: "UGMAN",
				issuerAuthorisationData: ZBlob.FromUTF8("{ arbitrary: \"data\", different: true }")
			);
			Factory.Save();
			arInvoice.Reload();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
			var transactionDataObject = writer.GetDataObject(arInvoice);

			AssertAuthorisationRecord("All authorization record fields should be populated in XUT.",
				transactionDataObject.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "UY", "1"),
				expectedNumber: "6534324",
				expectedCounter: "8",
				expectedDateTime: new DateTimeOffset(2019, 3, 5, 13, 32, 53, 562, TimeSpan.FromHours(-6)),
				expectedVerificationUrl: "http://some.url.com/txn/68763254259",
				expectedPublicKey: new ZBlob(twoKRandomBits),
				expectedAuthorisationData: ZBlob.FromUTF8("{ arbitrary: \"data\" }"),
				expectedTransactionHash: System.Security.Cryptography.SHA512.Create().ComputeHash(ZBlob.FromUTF8("6894732")),
				expectedDebtorNumber: "AB8882",
				expectedIssuerCertificateIdentifier: "GoDaddy Certificate Authority 1592",
				expectedPlaceOfIssue: "UGMAN",
				expectedIssuerAuthorisationData: ZBlob.FromUTF8("{ arbitrary: \"data\", different: true }")
			);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionWithAuthorizationDetails_AP_MinimalDetails_OrgProxyRecipient()
		{
			var apInvoice = CreateAPInvoice();
			CreateAuthorizationRecord(apInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Mexico,
				number: "nkj89080"
			);
			Factory.Save();
			apInvoice.Reload();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, apInvoice)));
			var transactionDataObject = writer.GetDataObject(apInvoice);

			AssertAuthorisationRecord("Authorization record should be included in XUT for AP as well.",
				transactionDataObject.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "MX", "1"),
				expectedNumber: "nkj89080"
			);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionWithAuthorizationDetails_AR_ManyDetails_OrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.India,
				number: "HKJ-8971-BMN-71"
			);
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Fiji,
				number: "43298321"
			);
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Brazil,
				number: "dsbkjhfsdbki789"
			);
			Factory.Save();
			arInvoice.Reload();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
			var transactionDataObject = writer.GetDataObject(arInvoice);

			AssertAuthorisationRecord("Many authorization records can be included on XUT; expected India EInvoicing.",
				transactionDataObject.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "IN", "1"),
				expectedNumber: "HKJ-8971-BMN-71"
			);
			AssertAuthorisationRecord("Many authorization records can be included on XUT; expected Fiji EInvoicing.",
				transactionDataObject.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "FJ", "1"),
				expectedNumber: "43298321"
			);
			AssertAuthorisationRecord("Many authorization records can be included on XUT; expected Brazil EInvoicing.",
				transactionDataObject.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "BR", "1"),
				expectedNumber: "dsbkjhfsdbki789"
			);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionWithAuthorizationDetails_AR_AllOtherCountriesType()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var arInvoice = createARInvoice();
			arInvoice.AH_GC = company.PK;
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.AllOtherCountries,
				number: "mnoiw98enhdcs"
			);
			Factory.Save();
			arInvoice.Reload();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
			var transactionDataObject = writer.GetDataObject(arInvoice);

			AssertAuthorisationRecord("All Other Countries 'ZZZ' type determines country from transaction company and uses version zero.",
				transactionDataObject.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "NZ", "0"),
				expectedNumber: "mnoiw98enhdcs"
			);
		}

		public void TestAuthorizationDetails_DoNotAddGovernmentBatchReference_WhenCountryDoesNotSupports()
		{
			var mockFactory = new Mock<IAccountingCountryComplianceGlobalFactory>();
			var mockFeatureConstants = new Mock<IFeatureConstants>();
			mockFactory.Setup(f => f.GetFeatureInterface<IFeatureConstants>(It.IsAny<ZString>()))
					   .Returns(mockFeatureConstants.Object);

			mockFeatureConstants.Setup(fc => fc.GetFeatureContants<SupportedFeatures>())
								.Returns(new SupportedFeatures(Features.IncludeGovernmentBatchReferenceInXUT));
			var arInvoice = createARInvoice();
			CreateEInvoicingBatchAndPivot(arInvoice, 100, "1124");
			Factory.Save();
			using (ObjectFactory.Substitute(mockFactory.Object))
			{
				var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
				var transactionDataObject = writer.GetDataObject(arInvoice);
				AssertEquals("Postcondition: AuthorizationDetailCollection", 1, transactionDataObject.AuthorizationDetailCollection.Count);
				AssertEquals("GovernmentBatchReference included", "1124", transactionDataObject.AuthorizationDetailCollection[0].GovernmentBatchReference);

				mockFeatureConstants.Setup(fc => fc.GetFeatureContants<SupportedFeatures>())
						.Returns(new SupportedFeatures());
				transactionDataObject = writer.GetDataObject(arInvoice);
				AssertNull("AuthorizationDetailCollection is null - GovernmentBatchReference not set.", transactionDataObject.AuthorizationDetailCollection);
			}
		}

		public void TestAuthorizationDetails_NotAddGovernmentBatchReference_WhenPivotActionTypeIsNotCommandActionType()
		{
			var countryComplianceFactory = CreateCountryComplianceFactoryWithGovernmentBatchReferenceInXUT();
			var arInvoice1 = createARInvoice();
			CreateEInvoicingBatchAndPivot(arInvoice1, 101, "1124");

			var arInvoice2 = createARInvoice();
			CreateEInvoicingBatchAndPivot(arInvoice2, 100, "1123", actionType: EInvoicingPivotActionType.StatusCheck);

			Factory.Save();

			using (ObjectFactory.Substitute(countryComplianceFactory))
			{
				var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice1)));
				var transactionDataObject1 = writer.GetDataObject(arInvoice1);

				AssertEquals("Postcondition: AuthorizationDetailCollection 1", 1, transactionDataObject1.AuthorizationDetailCollection.Count);
				AssertEquals("GovernmentBatchReference included", "1124", transactionDataObject1.AuthorizationDetailCollection[0].GovernmentBatchReference);

				writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice2)));
				var transactionDataObject2 = writer.GetDataObject(arInvoice2);

				AssertNull("AuthorizationDetailCollection is null - GovernmentBatchReference not set", transactionDataObject2.AuthorizationDetailCollection);
			}
		}

		public void TestAuthorizationDetails_AddsGovernmentBatchReference_BasedOnOrderOfPivotsByAIPStatus()
		{
			var countryComplianceFactory = CreateCountryComplianceFactoryWithGovernmentBatchReferenceInXUT();

			var arInvoice = createARInvoice();
			CreateEInvoicingBatchAndPivot(arInvoice, 100, "1123", ZDateTime.UtcNow, pivotStatus: EInvoicingBatchState.Discarded);
			CreateEInvoicingBatchAndPivot(arInvoice, 101, "1124", ZDateTime.UtcNow.AddMinutes(5), pivotStatus: EInvoicingBatchState.Discarded);

			Factory.Save();
			using (ObjectFactory.Substitute(countryComplianceFactory))
			{
				var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
				var transactionDataObject = writer.GetDataObject(arInvoice);
				AssertEquals("Postcondition: AuthorizationDetailCollection 1", 1, transactionDataObject.AuthorizationDetailCollection.Count);
				AssertEquals("GovernmentBatchReference", "1124", transactionDataObject.AuthorizationDetailCollection[0].GovernmentBatchReference);

				CreateEInvoicingBatchAndPivot(arInvoice, 102, "1125", ZDateTime.UtcNow.AddMinutes(-10), pivotStatus: EInvoicingBatchState.Sent);

				Factory.Save();

				writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
				transactionDataObject = writer.GetDataObject(arInvoice);
				AssertEquals("Postcondition: AuthorizationDetailCollection 2", 1, transactionDataObject.AuthorizationDetailCollection.Count);
				AssertEquals("GovernmentBatchReference", "1125", transactionDataObject.AuthorizationDetailCollection[0].GovernmentBatchReference);
			}
		}

		public void TestAuthorizationDetails_AddsGovernmentBatchReference_BasedOnOrderOfPivotsByAIP_LastSentTimeUtc()
		{
			var countryComplianceFactory = CreateCountryComplianceFactoryWithGovernmentBatchReferenceInXUT();
			var arInvoice = createARInvoice();

			CreateEInvoicingBatchAndPivot(arInvoice, 100, "1123", ZDateTime.UtcNow, pivotStatus: EInvoicingBatchState.Discarded);
			CreateEInvoicingBatchAndPivot(arInvoice, 101, "1124", pivotSystemCreateTimeUtc: ZDateTime.UtcNow.AddMinutes(-5), pivotStatus: EInvoicingBatchState.Discarded);
			CreateEInvoicingBatchAndPivot(arInvoice, 102, "1125", pivotSystemCreateTimeUtc: ZDateTime.UtcNow.AddMinutes(-10), pivotStatus: EInvoicingBatchState.Sent);

			Factory.Save();
			using (ObjectFactory.Substitute(countryComplianceFactory))
			{
				var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
				var transactionDataObject = writer.GetDataObject(arInvoice);
				AssertEquals("Postcondition: AuthorizationDetailCollection 1", 1, transactionDataObject.AuthorizationDetailCollection.Count);
				AssertEquals("GovernmentBatchReference", "1125", transactionDataObject.AuthorizationDetailCollection[0].GovernmentBatchReference);

				CreateEInvoicingBatchAndPivot(arInvoice, 104, "1126", ZDateTime.UtcNow.AddMinutes(10));

				Factory.Save();
				writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
				transactionDataObject = writer.GetDataObject(arInvoice);
				AssertEquals("Postcondition: AuthorizationDetailCollection 2", 1, transactionDataObject.AuthorizationDetailCollection.Count);
				AssertEquals("GovernmentBatchReference", "1126", transactionDataObject.AuthorizationDetailCollection[0].GovernmentBatchReference);
			}
		}

		public void TestAuthorizationDetails_AddsGovernmentBatchReference_BasedOnMostRecentPivot()
		{
			var countryComplianceFactory = CreateCountryComplianceFactoryWithGovernmentBatchReferenceInXUT();
			var arInvoice = createARInvoice();

			var earliestPivotWithBatch = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, actionType: EInvoicingPivotActionType.Submit);
			earliestPivotWithBatch.AIP_LastSentTimeUtc = ZDateTime.UtcNow.AddMinutes(-10);
			var batchForEarliestPivotWithBatch = TestObjectCreator.CreateEInvoicingBatchForPivot(earliestPivotWithBatch, 100, EInvoicingBatchState.Sent);
			batchForEarliestPivotWithBatch.AIB_GovernmentAllocatedNumber = "1124";

			Factory.Save();
			using (ObjectFactory.Substitute(countryComplianceFactory))
			{
				var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
				var transactionDataObject = writer.GetDataObject(arInvoice);
				AssertEquals("Precondition - GovernmentBatchReference when most recent pivot has batch", "1124", transactionDataObject.AuthorizationDetailCollection[0].GovernmentBatchReference);

				var mostRecentPivotWithoutBatch = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, actionType: EInvoicingPivotActionType.Adjustment);
				mostRecentPivotWithoutBatch.AIP_LastSentTimeUtc = ZDateTime.UtcNow;
				Factory.Save();

				writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
				transactionDataObject = writer.GetDataObject(arInvoice);
				AssertNull("Precondition - Pivot has no batch", mostRecentPivotWithoutBatch.Batch);
				AssertNull("AuthorizationDetailCollection is null - GovernmentBatchReference not set.", transactionDataObject.AuthorizationDetailCollection);
			}
		}

		public void TestAuthorizationDetails_AddsGovernmentBatchReference_WhenAuthorizationRecordExists()
		{
			var mockFactory = new Mock<IAccountingCountryComplianceGlobalFactory>();
			var mockFeatureConstants = new Mock<IFeatureConstants>();
			mockFactory.Setup(f => f.GetFeatureInterface<IFeatureConstants>(It.IsAny<ZString>()))
					   .Returns(mockFeatureConstants.Object);

			mockFeatureConstants.Setup(fc => fc.GetFeatureContants<SupportedFeatures>())
								.Returns(new SupportedFeatures(Features.IncludeGovernmentBatchReferenceInXUT));
			var arInvoice = createARInvoice();

			var twoKRandomBits = new byte[2048 / 8];
			new Random(1).NextBytes(twoKRandomBits);

			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Uruguay,
				number: "6534324",
				counter: "8",
				dateTime: new DateTimeOffset(2019, 3, 5, 13, 32, 53, 562, TimeSpan.FromHours(-6)),
				verificationUrl: "http://some.url.com/txn/68763254259",
				publicKey: new ZBlob(twoKRandomBits),
				authorisationData: ZBlob.FromUTF8("{ arbitrary: \"data\" }"),
				transactionHash: System.Security.Cryptography.SHA512.Create().ComputeHash(ZBlob.FromUTF8("6894732")),
				debtorNumber: "AB8882",
				issuerCertificateIdentifier: "GoDaddy Certificate Authority 1592",
				placeOfIssue: "UGMAN",
				issuerAuthorisationData: ZBlob.FromUTF8("{ arbitrary: \"data\", different: true }")
			);

			CreateEInvoicingBatchAndPivot(arInvoice, 100, "1124");
			Factory.Save();
			using (ObjectFactory.Substitute(mockFactory.Object))
			{
				var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice)));
				var transactionDataObject = writer.GetDataObject(arInvoice);
				AssertEquals("Postcondition: AuthorizationDetailCollection", 1, transactionDataObject.AuthorizationDetailCollection.Count);
				AssertEquals("GovernmentBatchReference included", "1124", transactionDataObject.AuthorizationDetailCollection[0].GovernmentBatchReference);

				AssertAuthorisationRecord("All authorization record fields should be populated in XUT.",
					transactionDataObject.AuthorizationDetailCollection,
					key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "UY", "1"),
					expectedNumber: "6534324",
					expectedCounter: "8",
					expectedDateTime: new DateTimeOffset(2019, 3, 5, 13, 32, 53, 562, TimeSpan.FromHours(-6)),
					expectedVerificationUrl: "http://some.url.com/txn/68763254259",
					expectedPublicKey: new ZBlob(twoKRandomBits),
					expectedAuthorisationData: ZBlob.FromUTF8("{ arbitrary: \"data\" }"),
					expectedTransactionHash: System.Security.Cryptography.SHA512.Create().ComputeHash(ZBlob.FromUTF8("6894732")),
					expectedDebtorNumber: "AB8882",
					expectedIssuerCertificateIdentifier: "GoDaddy Certificate Authority 1592",
					expectedPlaceOfIssue: "UGMAN",
					expectedIssuerAuthorisationData: ZBlob.FromUTF8("{ arbitrary: \"data\", different: true }"));
			}
		}

		public void TestAuthorizationDetails_AddsGovernmentBatchReference_ForAllCommandActionTypePivots()
		{
			int batchNumber = 100;
			var countryComplianceFactory = CreateCountryComplianceFactoryWithGovernmentBatchReferenceInXUT();
			var allActionTypes = typeof(EInvoicingPivotActionType)
									.GetFields(BindingFlags.Public | BindingFlags.Static)
									.Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
									.Select(fieldInfo => (string)fieldInfo.GetValue(null));

			AssertGovernmentBatchRefereneceForActionTypes(allActionTypes);

			void AssertGovernmentBatchRefereneceForActionTypes(IEnumerable<string> actionTypes)
			{
				foreach (var action in actionTypes)
				{
					var arInvoice1 = createARInvoice();
					CreateEInvoicingBatchAndPivot(arInvoice1, batchNumber++, "1124", actionType: action);
					Factory.Save();

					using (ObjectFactory.Substitute(countryComplianceFactory))
					{
						var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arInvoice1)));
						var transactionDataObject1 = writer.GetDataObject(arInvoice1);

						if (EInvoicingPivotActionType.CommandActionTypes.Contains(action))
						{
							AssertEquals("Postcondition: AuthorizationDetailCollection 1", 1, transactionDataObject1.AuthorizationDetailCollection.Count);
							AssertEquals("GovernmentBatchReference", "1124", transactionDataObject1.AuthorizationDetailCollection[0].GovernmentBatchReference);
						}
						else
						{
							AssertNull("AuthorizationDetailCollection is null - GovernmentBatchReference not set.", transactionDataObject1.AuthorizationDetailCollection);
						}
					}
				}
			}
		}

		#region TransactionWithAuthorizationDetailsInOriginalReference

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestOriginalReferenceWithAuthorizationDetails_AR_NullDetails_OrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			Factory.Save();

			var arReverse = CreateReverseTransaction(arInvoice);

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arReverse)));
			var transactionDataObject = writer.GetDataObject(arReverse);

			AssertNull("No auth record should not create an empty collection in XUT.OriginalReference.", transactionDataObject.OriginalReference.AuthorizationDetailCollection);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestOriginalReferenceWithAuthorizationDetails_AR_EmptyDetails_OrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Panama
			);
			Factory.Save();

			var arReverse = CreateReverseTransaction(arInvoice);

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arReverse)));
			var transactionDataObject = writer.GetDataObject(arReverse);

			AssertNull("Auth record with entirely empty placeholder details should not be included in XUT.OriginalReference.", transactionDataObject.OriginalReference.AuthorizationDetailCollection);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTOriginalReferenceWithAuthorizationDetails_AR_MinimalDetails_OrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Egypt,
				number: "6894732"
			);
			Factory.Save();

			var arReverse = CreateReverseTransaction(arInvoice);

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arReverse)));
			var transactionDataObject = writer.GetDataObject(arReverse);

			AssertAuthorisationRecord("Authorization record with at least one relevant field should be included in XUT.OriginalReference.",
				transactionDataObject.OriginalReference.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "EG", "1"),
				expectedNumber: "6894732"
			);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestOriginalReferenceWithAuthorizationDetails_AR_MinimalDetails_NonOrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Panama,
				number: "6894732"
			);
			Factory.Save();

			var arReverse = CreateReverseTransaction(arInvoice);

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.YIA, arReverse)));
			var transactionDataObject = writer.GetDataObject(arReverse);

			AssertNull("When Recipient Role is not Org Proxy, OriginalReference.AuthorizationDetails should not be included in XUT.", transactionDataObject.OriginalReference.AuthorizationDetailCollection);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestOriginalReferenceWithAuthorizationDetails_AR_FullDetails_OrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			var twoKRandomBits = new byte[2048 / 8];
			new Random(1).NextBytes(twoKRandomBits);

			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Uruguay,
				number: "6534324",
				counter: "8",
				dateTime: new DateTimeOffset(2019, 3, 5, 13, 32, 53, 562, TimeSpan.FromHours(-6)),
				verificationUrl: "http://some.url.com/txn/68763254259",
				publicKey: new ZBlob(twoKRandomBits),
				authorisationData: ZBlob.FromUTF8("{ arbitrary: \"data\" }"),
				transactionHash: System.Security.Cryptography.SHA512.Create().ComputeHash(ZBlob.FromUTF8("6894732")),
				debtorNumber: "AB8882",
				issuerCertificateIdentifier: "GoDaddy Certificate Authority 1592",
				placeOfIssue: "UGMAN",
				issuerAuthorisationData: ZBlob.FromUTF8("{ arbitrary: \"data\", different: true }")
			);
			Factory.Save();

			var arReverse = CreateReverseTransaction(arInvoice);

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arReverse)));
			var transactionDataObject = writer.GetDataObject(arReverse);

			AssertAuthorisationRecord("All authorization record fields should be populated in XUT.",
				transactionDataObject.OriginalReference.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "UY", "1"),
				expectedNumber: "6534324",
				expectedCounter: "8",
				expectedDateTime: new DateTimeOffset(2019, 3, 5, 13, 32, 53, 562, TimeSpan.FromHours(-6)),
				expectedVerificationUrl: "http://some.url.com/txn/68763254259",
				expectedPublicKey: new ZBlob(twoKRandomBits),
				expectedAuthorisationData: ZBlob.FromUTF8("{ arbitrary: \"data\" }"),
				expectedTransactionHash: System.Security.Cryptography.SHA512.Create().ComputeHash(ZBlob.FromUTF8("6894732")),
				expectedDebtorNumber: "AB8882",
				expectedIssuerCertificateIdentifier: "GoDaddy Certificate Authority 1592",
				expectedPlaceOfIssue: "UGMAN",
				expectedIssuerAuthorisationData: ZBlob.FromUTF8("{ arbitrary: \"data\", different: true }")
			);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestOriginalReferenceWithAuthorizationDetails_AP_MinimalDetails_OrgProxyRecipient()
		{
			var apInvoice = CreateAPInvoice();
			CreateAuthorizationRecord(apInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Mexico,
				number: "nkj89080"
			);
			Factory.Save();

			var reverseTransaction = CreateReverseTransaction(apInvoice);

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, reverseTransaction)));
			var transactionDataObject = writer.GetDataObject(reverseTransaction);

			AssertAuthorisationRecord("Authorization record should be included in XUT.OriginalReference for the AP transaction as well.",
				transactionDataObject.OriginalReference.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "MX", "1"),
				expectedNumber: "nkj89080"
			);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestOriginalReferenceWithAuthorizationDetails_AR_ManyDetails_OrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.India,
				number: "HKJ-8971-BMN-71"
			);
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Fiji,
				number: "43298321"
			);
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Brazil,
				number: "dsbkjhfsdbki789"
			);
			Factory.Save();

			var arReverse = CreateReverseTransaction(arInvoice);

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arReverse)));
			var transactionDataObject = writer.GetDataObject(arReverse);

			AssertAuthorisationRecord("Many authorization records can be included on XUT.OriginalReference; expected India EInvoicing.",
				transactionDataObject.OriginalReference.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "IN", "1"),
				expectedNumber: "HKJ-8971-BMN-71"
			);
			AssertAuthorisationRecord("Many authorization records can be included on XUT.OriginalReference; expected Fiji EInvoicing.",
				transactionDataObject.OriginalReference.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "FJ", "1"),
				expectedNumber: "43298321"
			);
			AssertAuthorisationRecord("Many authorization records can be included on XUT.OriginalReference; expected Brazil EInvoicing.",
				transactionDataObject.OriginalReference.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "BR", "1"),
				expectedNumber: "dsbkjhfsdbki789"
			);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestOriginalReferenceWithAuthorizationDetails_AR_AllOtherCountriesType()
		{
			var company = TestObjectCreator.CreateCompanyAndBranch("NZAKL");
			var arInvoice = createARInvoice();
			arInvoice.AH_GC = company.PK;
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.AllOtherCountries,
				number: "mnoiw98enhdcs"
			);
			Factory.Save();

			var arReverse = CreateReverseTransaction(arInvoice);

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arReverse)));
			var transactionDataObject = writer.GetDataObject(arReverse);

			AssertAuthorisationRecord("All Other Countries 'ZZZ' type determines country from transaction company and uses version zero.",
				transactionDataObject.OriginalReference.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "NZ", "0"),
				expectedNumber: "mnoiw98enhdcs"
			);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestOriginalReferenceWithAuthorizationDetails_AR_NullDefaultValues_OrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			var authorizationRecord = CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Panama,
				debtorNumber: "AB8882"
			);
			Factory.Save();

			var arReverse = CreateReverseTransaction(arInvoice);

			AssertEquals($"Pre-Condition {nameof(authorizationRecord.AHF_Number)}", string.Empty, authorizationRecord.AHF_Number);
			AssertEquals($"Pre-Condition {nameof(authorizationRecord.AHF_Counter)}", string.Empty, authorizationRecord.AHF_Counter);
			AssertEquals($"Pre-Condition {nameof(authorizationRecord.AHF_DateTime)}", ZDateTimeOffset.Empty, authorizationRecord.AHF_DateTime);

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arReverse)));
			var transactionDataObject = writer.GetDataObject(arReverse);

			AssertNull($"Post-Condition GovernmentNumber", transactionDataObject.OriginalReference.AuthorizationDetailCollection[0].GovernmentNumber);
			AssertNull($"Post-Condition GovernmentCounter", transactionDataObject.OriginalReference.AuthorizationDetailCollection[0].GovernmentCounter);
			AssertNull($"Post-Condition Date", transactionDataObject.OriginalReference.AuthorizationDetailCollection[0].Date);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestOriginalReferenceWithAuthorizationDetails_AR_LegacyNullPlaceholderDefaultValues_OrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			var authorizationRecord = CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Panama,
				debtorNumber: "AB8882",
				// Legacy records may contain null placeholder values which should have the same behavior as null / default values
				number: AuthRecordConstants.NullPlaceholderForNVarchar,
				counter: AuthRecordConstants.NullPlaceholderForNVarchar,
				dateTime: AuthRecordConstants.NullPlaceholderForDateTime
			);
			Factory.Save();

			var arReverse = CreateReverseTransaction(arInvoice);

			AssertEquals($"Pre-Condition {nameof(authorizationRecord.AHF_Number)}", AuthRecordConstants.NullPlaceholderForNVarchar, authorizationRecord.AHF_Number);
			AssertEquals($"Pre-Condition {nameof(authorizationRecord.AHF_Counter)}", DataTransferConstants.AccTransactionHeaderAuthorisationRecord.NullPlaceholderForNVarchar, authorizationRecord.AHF_Counter);
			AssertEquals($"Pre-Condition {nameof(authorizationRecord.AHF_DateTime)}", DataTransferConstants.AccTransactionHeaderAuthorisationRecord.NullPlaceholderForDateTime, authorizationRecord.AHF_DateTime);

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arReverse)));
			var transactionDataObject = writer.GetDataObject(arReverse);

			AssertNull($"Post-Condition GovernmentNumber", transactionDataObject.OriginalReference.AuthorizationDetailCollection[0].GovernmentNumber);
			AssertNull($"Post-Condition GovernmentCounter", transactionDataObject.OriginalReference.AuthorizationDetailCollection[0].GovernmentCounter);
			AssertNull($"Post-Condition Date", transactionDataObject.OriginalReference.AuthorizationDetailCollection[0].Date);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionWithAuthorizationDetails_And_OriginalReferenceWithAuthorizationDetails_AR_OrgProxyRecipient()
		{
			var arInvoice = createARInvoice();
			CreateAuthorizationRecord(arInvoice,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Uruguay,
				number: "123456789",
				counter: "10"
			);
			Factory.Save();

			var arReverse = CreateReverseTransaction(arInvoice);

			CreateAuthorizationRecord(arReverse,
				recordType: AccTransactionHeaderAuthorisationRecordTypes.Uruguay,
				number: "987654321",
				counter: "3",
				dateTime: new DateTimeOffset(2019, 3, 5, 13, 32, 53, 562, TimeSpan.FromHours(-6))
			);
			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, arReverse)));
			var transactionDataObject = writer.GetDataObject(arReverse);

			AssertAuthorisationRecord("Authorization record with at least one relevant field should be included in XUT for Reverse AR Credit Note.",
				transactionDataObject.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "UY", "1"),
				expectedNumber: "987654321",
				expectedCounter: "3",
				expectedDateTime: new DateTimeOffset(2019, 3, 5, 13, 32, 53, 562, TimeSpan.FromHours(-6))
			);

			AssertAuthorisationRecord("Authorization record with at least one relevant field should be included in XUT.OriginalReference.",
				transactionDataObject.OriginalReference.AuthorizationDetailCollection,
				key: (DataTransferConstants.AccTransactionHeaderAuthorisationRecord.EInvoicingModuleCode, "UY", "1"),
				expectedNumber: "123456789",
				expectedCounter: "10"
			);
		}

		public void TestOriginalReference_DoNotAddGovernmentBatchReference_WhenCountryDoesNotSupports()
		{
			var mockFactory = new Mock<IAccountingCountryComplianceGlobalFactory>();
			var mockFeatureConstants = new Mock<IFeatureConstants>();
			mockFactory.Setup(f => f.GetFeatureInterface<IFeatureConstants>(It.IsAny<ZString>()))
						.Returns(mockFeatureConstants.Object);

			mockFeatureConstants.Setup(fc => fc.GetFeatureContants<SupportedFeatures>())
								.Returns(new SupportedFeatures(Features.IncludeGovernmentBatchReferenceInXUT));

			var arInvoice = createARInvoice();
			var reverseTransaction = CreateReverseTransaction(arInvoice);
			CreateAuthorizationRecord(reverseTransaction, recordType: AccTransactionHeaderAuthorisationRecordTypes.Mexico, number: "nkj89080");
			CreateEInvoicingBatchAndPivot(reverseTransaction, 100, "1124");
			Factory.Save();
			using (ObjectFactory.Substitute(mockFactory.Object))
			{
				var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, reverseTransaction)));
				var transactionDataObject = writer.GetDataObject(reverseTransaction);
				AssertEquals("Postcondition: AuthorizationDetailCollection 1", 1, transactionDataObject.AuthorizationDetailCollection.Count);
				AssertEquals("GovernmentBatchReference included", "1124", transactionDataObject.AuthorizationDetailCollection[0].GovernmentBatchReference);

				mockFeatureConstants.Setup(fc => fc.GetFeatureContants<SupportedFeatures>())
					.Returns(new SupportedFeatures());

				transactionDataObject = writer.GetDataObject(reverseTransaction);
				AssertNull("GovernmentBatchReference not included", transactionDataObject.AuthorizationDetailCollection[0].GovernmentBatchReference);
			}
		}

		InvoicingBase CreateReverseTransaction(InvoicingBase invoice)
		{
			var reversingFactory = new ReversingFactory();
			var reversingBase = reversingFactory.NewReversing(invoice);
			reversingBase.Reverse();

			Factory.Save();
			return invoice.ReverseInvoice;
		}

		#endregion

		#region Helpers

		AccTransactionHeaderAuthorisationRecord CreateAuthorizationRecord(
			InvoicingBase transaction,
			string recordType,
			string number = null,
			string counter = null,
			ZDateTimeOffset? dateTime = null,
			string verificationUrl = null,
			byte[] publicKey = null,
			byte[] authorisationData = null,
			byte[] transactionHash = null,
			string debtorNumber = null,
			string issuerCertificateIdentifier = null,
			string placeOfIssue = null,
			byte[] issuerAuthorisationData = null
		)
		{
			var result = Factory.New<AccTransactionHeaderAuthorisationRecord>();
			result.AHF_RecordType = recordType;
			result.AHF_ParentId = transaction.PK;
			result.AHF_ParentTableCode = "AH";

			result.AHF_Number = number ?? result.AHF_Number;
			result.AHF_Counter = counter ?? result.AHF_Counter;
			result.AHF_DateTime = dateTime ?? ZDateTimeOffset.Empty;
			result.AHF_VerificationUrl = verificationUrl;
			result.AHF_PublicKey = publicKey;
			result.AHF_AuthorisationData = authorisationData;
			result.AHF_ITransactionHash = transactionHash;
			result.AHF_DebtorNumber = debtorNumber;
			result.AHF_IssuerCertificateIdentifier = issuerCertificateIdentifier;
			result.AHF_PlaceOfIssue = placeOfIssue;
			result.AHF_IssuerAuthorizationData = issuerAuthorisationData;

			return result;
		}

		void AssertAuthorisationRecord(
			string message,
			List<AuthorizationDetails> authorizationDetailCollection,
			(ZString purpose, ZString country, ZString version) key,
			ZString? expectedNumber = null,
			ZString? expectedCounter = null,
			ZDateTimeOffset? expectedDateTime = null,
			ZString? expectedVerificationUrl = null,
			byte[] expectedPublicKey = null,
			byte[] expectedAuthorisationData = null,
			byte[] expectedTransactionHash = null,
			ZString? expectedDebtorNumber = null,
			ZString? expectedIssuerCertificateIdentifier = null,
			ZString? expectedPlaceOfIssue = null,
			byte[] expectedIssuerAuthorisationData = null
		)
		{
			AssertNotNull(authorizationDetailCollection);
			var authDetail = authorizationDetailCollection
								.FirstOrDefault(x => x.Purpose.Code.Value == key.purpose
												  && x.Country.Code.Value == key.country
												  && x.Version.Value == key.version);
			AssertNotNull($"Should find AuthorizationDetail object with key '{key}'", authDetail);
			CombineAssertions(message, () =>
			{
				AssertEquals($"'{key}'.GovernmentNumber", expectedNumber, authDetail.GovernmentNumber);
				AssertEquals($"'{key}'.GovernmentCounter", expectedCounter, authDetail.GovernmentCounter);
				AssertEquals($"'{key}'.Date", expectedDateTime, authDetail.Date);
				AssertEquals($"'{key}'.URL", expectedVerificationUrl, authDetail.URL);
				AssertEquals($"'{key}'.PublicKey", expectedPublicKey, ToByteArrayOrNull(authDetail.PublicKey));
				AssertEquals($"'{key}'.SharedSpecialData", expectedAuthorisationData, ToByteArrayOrNull(authDetail.SharedSpecialData));
				AssertEquals($"'{key}'.SharedTransactionHash", expectedTransactionHash, ToByteArrayOrNull(authDetail.SharedTransactionHash));
				AssertEquals($"'{key}'.DebtorRegistrationNumber", expectedDebtorNumber, authDetail.DebtorRegistrationNumber);
				AssertEquals($"'{key}'.IssuerCertificateID", expectedIssuerCertificateIdentifier, authDetail.IssuerCertificateID);
				AssertEquals($"'{key}'.PlaceOfIssue", expectedPlaceOfIssue, authDetail.PlaceOfIssue);
				AssertEquals($"'{key}'.IssuerSpecialData", expectedIssuerAuthorisationData, ToByteArrayOrNull(authDetail.IssuerSpecialData));
			});
		}

		byte[] ToByteArrayOrNull(SubStreamableStream stream)
			=> stream == null ? null : stream.ToByteArray();

		APInvoice CreateAPInvoice()
		{
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("003", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m);
			apInvoice.AH_PostDate = new ZDateTime(2009, 06, 15);
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m);
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			line.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;

			return apInvoice;
		}

		#endregion

		#endregion

		#region TransactionHeaderReference

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionHeaderReference_ValidReference_OrgProxyRecipient()
		{
			ExecuteReferenceTest_ValidReference(createARInvoice);
			ExecuteReferenceTest_ValidReference(CreateAPInvoice);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionHeaderReference_UnknownReference_ReferenceDescriptionIsNull()
		{
			ExecuteReferenceTest_UnknownReference(createARInvoice);
			ExecuteReferenceTest_UnknownReference(CreateAPInvoice);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionHeaderReference_UnknownType_TypeDescriptionIsNull()
		{
			ExecuteReferenceTest_UnknownType(createARInvoice);
			ExecuteReferenceTest_UnknownType(CreateAPInvoice);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionHeaderReference_OriginalInvoice()
		{
			AssertReferenceTest_OriginalInvoice(createARInvoice);
			AssertReferenceTest_OriginalInvoice(CreateAPInvoice);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionHeaderReference_DoesNotIncludeReference_WhenReference_IsNull()
		{
			ExecuteReferenceTest_NullReference(createARInvoice);
			ExecuteReferenceTest_NullReference(CreateAPInvoice);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionHeaderReference_DoesNotIncludeReference_WhenReference_IsEmpty()
		{
			ExecuteReferenceTest_EmptyStringReference(createARInvoice);
			ExecuteReferenceTest_EmptyStringReference(CreateAPInvoice);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionHeaderReference_ValidReference_NonOrgProxyRecipient()
		{
			ExecuteReferenceTest_ValidReference_NonOrgProxyRecipient(createARInvoice);
			ExecuteReferenceTest_ValidReference_NonOrgProxyRecipient(CreateAPInvoice);
		}

		[MasterFiles.Business.Testing.SuspendCriticalValidation]
		public void TestTransactionHeaderReference_WithMultipleReferences()
		{
			ExecuteReferenceTest_MultipleReferences(createARInvoice);
			ExecuteReferenceTest_MultipleReferences(CreateAPInvoice);
		}

		void ExecuteReferenceTest_MultipleReferences(Func<InvoicingBase> createInvoice)
		{
			var invoice = createInvoice();

			CreateReference(invoice, "ERC", "01");
			CreateReference(invoice, "ZZZ", "99");
			Factory.Save();

			var result = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice))).GetDataObject(invoice);
			var references = result.TransactionHeaderReferenceCollection;

			AssertReferenceDetail(references, "ERC", "01");
			AssertReferenceDetail(references, "ZZZ", "99");
		}

		void ExecuteReferenceTest_ValidReference_NonOrgProxyRecipient(Func<InvoicingBase> createInvoice)
		{
			var invoice = createInvoice();

			CreateReference(invoice, AccTransactionHeaderReferenceTypes.EINV_REVERSAL_CODE, "01");
			Factory.Save();

			var transactionDataObject = new TransactionDataObjectWriter(
				new DataWritingManager(new ActionInfo(RecipientRoleType.YIA, invoice))
			).GetDataObject(invoice);

			AssertNull("When Recipient Role is not Org Proxy, reference details should not be included in XUT.", transactionDataObject.TransactionHeaderReferenceCollection);
		}

		void ExecuteReferenceTest_NullReference(Func<InvoicingBase> createInvoice)
		{
			var invoice = createInvoice();
			CreateReference(invoice, AccTransactionHeaderReferenceTypes.EINV_REVERSAL_CODE, null);
			Factory.Save();

			var reverse = CreateReverseTransaction(invoice);
			var transactionDataObject = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, reverse))).GetDataObject(invoice);

			var entry = transactionDataObject.TransactionHeaderReferenceCollection?.FirstOrDefault(x => x.Type.Value == AccTransactionHeaderReferenceTypes.EINV_REVERSAL_CODE);
			AssertNull("A reference of type EINV_REVERSAL_CODE should not be included when AH1_Reference is null.", entry);
		}

		void ExecuteReferenceTest_EmptyStringReference(Func<InvoicingBase> createInvoice)
		{
			var invoice = createInvoice();
			CreateReference(invoice, AccTransactionHeaderReferenceTypes.EINV_REVERSAL_CODE, "");
			Factory.Save();

			var reverse = CreateReverseTransaction(invoice);
			var transactionDataObject = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, reverse))).GetDataObject(invoice);

			var entry = transactionDataObject.TransactionHeaderReferenceCollection?.FirstOrDefault(x => x.Type.Value == AccTransactionHeaderReferenceTypes.EINV_REVERSAL_CODE);
			AssertNull("A reference of type EINV_REVERSAL_CODE should not be included when AH1_Reference is empty string.", entry);
		}

		void ExecuteReferenceTest_ValidReference(Func<InvoicingBase> createInvoice)
		{
			RegisterReversalCode("01", "Reference Description");
			var invoice = createInvoice();
			CreateReference(invoice, AccTransactionHeaderReferenceTypes.EINV_REVERSAL_CODE, "01");

			Factory.Save();

			var reverse = CreateReverseTransaction(invoice);
			var result = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, reverse))).GetDataObject(invoice);

			AssertReferenceDetail(result.TransactionHeaderReferenceCollection, AccTransactionHeaderReferenceTypes.EINV_REVERSAL_CODE, "01", "E-Invoicing Reversal Code", "Reference Description");
		}

		void ExecuteReferenceTest_UnknownReference(Func<InvoicingBase> createInvoice)
		{
			var invoice = createInvoice();
			CreateReference(invoice, AccTransactionHeaderReferenceTypes.EINV_REVERSAL_CODE, "03");
			Factory.Save();

			var reverse = CreateReverseTransaction(invoice);
			var result = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, reverse))).GetDataObject(invoice);

			AssertReferenceDetail(result.TransactionHeaderReferenceCollection, AccTransactionHeaderReferenceTypes.EINV_REVERSAL_CODE, "03", "E-Invoicing Reversal Code", string.Empty);
		}

		void ExecuteReferenceTest_UnknownType(Func<InvoicingBase> createInvoice)
		{
			RegisterReversalCode("01", "Reference Description");
			var invoice = createInvoice();
			CreateReference(invoice, "???", "01");
			Factory.Save();

			var reverse = CreateReverseTransaction(invoice);
			var result = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, reverse))).GetDataObject(invoice);

			AssertReferenceDetail(result.TransactionHeaderReferenceCollection, "???", "01", string.Empty, string.Empty);
		}

		void AssertReferenceTest_OriginalInvoice(Func<InvoicingBase> createInvoice)
		{
			var invoice = createInvoice();
			CreateReference(invoice, AccTransactionHeaderReferenceTypes.EINV_REVERSAL_CODE, "01");
			RegisterReversalCode("01", "Reference Description");
			Factory.Save();

			var result = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice))).GetDataObject(invoice);

			AssertNotNull("TransactionHeaderReferenceCollection should be assigned for original invoices.", result.TransactionHeaderReferenceCollection);

			AssertReferenceDetail(result.TransactionHeaderReferenceCollection,AccTransactionHeaderReferenceTypes.EINV_REVERSAL_CODE,"01","E-Invoicing Reversal Code","Reference Description");
		}

		AccTransactionHeaderReference CreateReference(InvoicingBase transaction, string type, string reference)
		{
			var r = Factory.New<AccTransactionHeaderReference>();
			r.AH1_AH = transaction.PK;
			r.AH1_Type = type;
			r.AH1_Reference = reference;

			return r;
		}

		void AssertReferenceDetail(List<TransactionHeaderReference> references, string expectedType, string expectedReference, string expectedTypeDesc = "", string expectedRefDesc = "")
		{
			var detail = references.FirstOrDefault(x => x.Type.Value == expectedType && x.Reference.Value == expectedReference);

			AssertNotNull($"Reference of type '{expectedType}' with value '{expectedReference}' expected.", detail);
			AssertEquals("Type", expectedType, detail.Type);
			AssertEquals("Reference", expectedReference, detail.Reference);

			if (!string.IsNullOrEmpty(expectedTypeDesc))
			{
				AssertEquals("TypeDescription", expectedTypeDesc, detail.TypeDescription);
			}

			if (!string.IsNullOrEmpty(expectedRefDesc))
			{
				AssertEquals("ReferenceDescription", expectedRefDesc, detail.ReferenceDescription);
			}
		}

		void RegisterReversalCode(string code, string description)
		{
			var list = new CodeDescriptionPairList();
			list.AddPair((NoResString)code, (NoResString)description);

			AccountingMasterFilesRegistry.Instance.EInvoicingReversalCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, list);
		}

		#endregion

		#region Implementation

		void AssertTransactionDataObject(InvoicingBase source, UniversalTransaction target, bool isCashBasisVAT = false, AccCashBasisVAT cashVAT = null, bool hasTax = false)
		{
			var postingJournal = target.PostingJournalCollection.First();
			CombineAssertions(delegate
			{
				AssertNotNull("DataContext", target.DataContext);
				AssertNotNull("DataContext.DataSourceCollection", target.DataContext.DataSourceCollection);
				AssertEquals("DataContext.DataSourceCollection has one element", 1, target.DataContext.DataSourceCollection.Count());
				AssertEquals("DataContext.DataSourceCollection.First().Type", "AccountingInvoice", target.DataContext.DataSourceCollection.First().Type);
				AssertEquals("DataContext.DataSourceCollection.First().Key", source.AH_Ledger + " " + source.AH_TransactionType + " " + source.AH_TransactionNum, target.DataContext.DataSourceCollection.First().Key);

				AssertEquals("Ledger", source.AH_Ledger, target.Ledger);
				AssertEquals("TransactionType", (TransactionType)Enum.Parse(typeof(TransactionType), source.AH_TransactionType, true), target.TransactionType);
				AssertEquals("PostDate", source.AH_PostDate, target.PostDate);
				AssertEquals("Number", source.AH_TransactionNum, target.Number);

				AssertNotNull("Branch", target.Branch);
				AssertEquals("Branch.Code", source.Branch.GB_Code, target.Branch.Code);
				AssertNotNull("Department", target.Department);
				AssertEquals("Department.Code", source.Department.GE_Code, target.Department.Code);

				ZDecimal sign = source.AH_TransactionType == TransactionTypes.CreditNote ? -1m : 1m;
				AssertNotNull("OSCurrency", target.OSCurrency);
				AssertEquals("OSCurrency.Code", source.AH_RX_NKTransactionCurrency, target.OSCurrency.Code);
				AssertEquals("OSExGSTVATAmount", source.AH_OSExTaxAmount * sign, target.OSExGSTVATAmount);
				AssertEquals("OSGSTVATAmount", source.AH_OSTaxAmount * sign, target.OSGSTVATAmount);
				AssertEquals("OSTotal", source.AH_OSTotal, target.OSTotal);

				AssertNotNull("LocalCurrency", target.LocalCurrency);
				AssertEquals("LocalCurrency.Code", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, target.LocalCurrency.Code);
				AssertEquals("LocalExVATAmount", source.AH_LocalExTaxAmount * sign, target.LocalExVATAmount);
				AssertEquals("LocalVATAmount", source.AH_LocalTaxAmount * sign, target.LocalVATAmount);
				AssertEquals("LocalTotal", source.AH_LocalTotalAmount * sign, target.LocalTotal);

				AssertNotNull("PostingJournalCollection", target.PostingJournalCollection);
				AssertEquals("PostingJournalCollection has one element", 1, target.PostingJournalCollection.Count);
				AssertNotNull("PostingJournalCollection.First().ChargeCode", postingJournal.ChargeCode);
				AssertEquals("PostingJournalCollection.First().ChargeCode.Code", source.Lines[0].ChargeCode.AC_Code, postingJournal.ChargeCode.Code);

				if (hasTax)
				{
					AssertNotNull("PostingJournalCollection.First().TaxRate", postingJournal.VATTaxID);
					AssertEquals("PostingJournalCollection.First().TaxRate.TaxCode", source.Lines[0].TaxRate.AT_Code, postingJournal.VATTaxID.TaxCode);
					AssertNotNull("PostingJournalCollection.First().TaxMessageID", postingJournal.TaxMessageID);
					AssertEquals("PostingJournalCollection.First().TaxMessageID.TaxMessageCode", source.Lines[0].VATClass.A9_Code, postingJournal.TaxMessageID.TaxMessageCode);
				}

				var lineAccount = source.Lines[0].GLHeader;
				var suspenseAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.Value);
				var controlAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.ARControlAccount.Value);
				var pendingTaxAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.Value);
				var taxAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value);

				AssertEquals("PostingJournalDetailCollection.Count", cashVAT != null ? 4 : 3, postingJournal.PostingJournalDetailCollection.Count);

				var swapAccounts = source is ARCreditNote;
				int i = 0;
				AssertPostingJournalDetails(postingJournal, i++, controlAccount, suspenseAccount, source.AH_LocalExTaxAmount, source.AH_PostDate, swapAccounts);
				AssertPostingJournalDetails(postingJournal, i++, controlAccount, isCashBasisVAT ? pendingTaxAccount : taxAccount, source.AH_LocalTaxAmount, source.AH_PostDate, swapAccounts);
				if (cashVAT != null)
				{
					AssertPostingJournalDetails(postingJournal, i++, pendingTaxAccount, taxAccount, Math.Abs(cashVAT.YC_TaxAmount), cashVAT.YC_PostDate, swapAccounts);
				}
				AssertPostingJournalDetails(postingJournal, i++, suspenseAccount, lineAccount, source.AH_LocalExTaxAmount, source.AH_PostDate, swapAccounts);

				AssertEquals("Postcondition: all records in PostingJournalDetailCollection were checked.", i, postingJournal.PostingJournalDetailCollection.Count);
			});
		}

		void AssertPostingJournalDetails(PostingJournal postingJournal, int postingJournalDetailNumber, AccGLHeader debitAccount, AccGLHeader creditAccount, decimal postingAmount, ZDateTime postingDate, bool swapAccounts)
		{
			var postingJournalDetail = postingJournal.PostingJournalDetailCollection[postingJournalDetailNumber];
			if (swapAccounts)
			{
				var tempDebitAccount = debitAccount;
				debitAccount = creditAccount;
				creditAccount = tempDebitAccount;
			}
			string message = "PostingJournalDetail {0}: {1}";
			AssertEquals(string.Format(message, postingJournalDetailNumber, "DebitGLAccount"), debitAccount.AccountNum, postingJournalDetail.DebitGLAccount.AccountCode);
			AssertEquals(string.Format(message, postingJournalDetailNumber, "CreditGLAccount"), creditAccount.AccountNum, postingJournalDetail.CreditGLAccount.AccountCode);
			AssertEquals(string.Format(message, postingJournalDetailNumber, "PostingAmount"), postingAmount, postingJournalDetail.PostingAmount);
			AssertEquals(string.Format(message, postingJournalDetailNumber, "PostingDate"), postingDate, postingJournalDetail.PostingDate);
			AssertEquals(string.Format(message, postingJournalDetailNumber, "PostingCurrency"), GlbCompany.CurrentCompany.LocalCurrency.RX_Code, postingJournalDetail.PostingCurrency.Code);
		}

		void AssertRelatedShipment(InvoicingBase source, UniversalTransaction target)
		{
			CombineAssertions(delegate
			{
				AssertNotNull("ShipmentCollection", target.ShipmentCollection);
				AssertEquals("ShipmentCollection has one element", 1, target.ShipmentCollection.Count);
				var genericJobPK = (from InvoicingLineBase line in source.Lines where line.Job != null select line.Job.JH_ParentID).FirstOrDefault();
				AssertNotNull("Related Job ParentID", genericJobPK);

				var genericJob = source.Factory.LoadGenericJob<GenericJob>(genericJobPK, JobShipmentSchema.Constants.Prefix);
				AssertNotNull("GenericJob", genericJob);
				var shipment = genericJob.Consumer as ForwardingShipment;
				AssertNotNull("Related Shipment", shipment);

				AssertEquals("ShipmentCollection.First().WayBillNumber", shipment.JS_ActualChargeable, target.ShipmentCollection.First().ActualChargeable);
			});
		}

		void AssertRelatedConsol(InvoicingBase source, UniversalTransaction target)
		{
			CombineAssertions(delegate
			{
				AssertNotNull("ShipmentCollection", target.ShipmentCollection);
				AssertEquals("ShipmentCollection has one element", 1, target.ShipmentCollection.Count);
				AssertNotNull("source Consol is not null", source.Consol);
				var genericConsol = GenericConsol.GetIJobCostingPlugInByPK(source.Factory, source.Consol.PK, ((BusinessObject)source.Consol).TablePrefix);
				AssertNotNull("genericConsol", genericConsol);
				var forwardingConsol = genericConsol as ForwardingConsol;
				AssertNotNull("Related Consol", forwardingConsol);
				AssertEquals("Same JK_UniqueConsignRef", forwardingConsol.JK_UniqueConsignRef, target.JobInvoiceNumber);
				AssertEquals("Same Consol Ref", forwardingConsol.JK_UniqueConsignRef, target.ShipmentCollection[0].DataContext.DataSourceCollection.FirstOrDefault().Key);
			});
		}

		void AssertRelatedConsolLines(InvoicingBase source, UniversalTransaction target)
		{
			CombineAssertions(delegate
			{
				AssertNotNull("ShipmentCollection", target.ShipmentCollection);
				AssertEquals("ShipmentCollection has one element", 2, target.ShipmentCollection.Count);
				var lineConsols = source.Lines.Cast<InvoicingLineBase>().Where(x => x.GetConsolID() != null).Select(x => x.GetConsolID()).Distinct().ToList();
				AssertEquals(1, lineConsols.Count);
				var genericConsol = GenericConsol.GetIJobCostingPlugInByPK(Factory, lineConsols[0].Item1, lineConsols[0].Item2);
				AssertNotNull("genericConsol", genericConsol);
				var forwardingConsol = genericConsol as ForwardingConsol;
				AssertNotNull("Related Consol", forwardingConsol);
				AssertEquals("Same Consol Ref", forwardingConsol.JK_UniqueConsignRef, target.ShipmentCollection[1].DataContext.DataSourceCollection.FirstOrDefault().Key);
			});
		}

		void AssertExtraVATAmount(InvoicingBase source, UniversalTransaction target)
		{
			AssertEquals(source.Lines.Count, target.PostingJournalCollection.Count);

			source.Lines.Cast<InvoicingLineBase>().OrderBy(x => x.AL_OSAmount);
			target.PostingJournalCollection.OrderBy(x => x.OSAmount);

			for (int i = 0; i < source.Lines.Count; i++)
			{
				var line = source.Lines[i];
				if (line.TaxRate.GetRate_ForTestOnly() != 0M)
				{
					AssertEquals($"Line{i + 1}: Local Extra Tax Amount:", line.AL_LocalExtraTaxAmount, target.PostingJournalCollection[i].LocalExtraVATAmount);
					AssertEquals($"Line{i + 1}: OS Extra Tax Amount:", line.AL_OSExtraTaxAmount, target.PostingJournalCollection[i].OSExtraVATAmount);
				}
			}
		}

		void SetupDataWithTaxFramework(Type invoiceType, ZString transactionNum, ZDecimal al_OSExTaxAmount, string tax_SuperType = "SLX")
		{
			var orgHeader = TestObjectCreator.ABIGAS;
			var chargeCode = TestObjectCreator.RevenueChargeCode;
			var arControlAccount = TestObjectCreator.CreateARControlAccount();
			var taxRealisedControlAccount = TestObjectCreator.CreatePrepaidAssetTaxControlAccount();

			TaxFrameworkTestObjectCreator.SetupMinimumSettingsForTaxFramework(GlbCompany.CurrentCompany, orgHeader, chargeCode, isJobRelated: false, taxSuperType: tax_SuperType, controlAccount: arControlAccount, taxRealisedControlAccount: taxRealisedControlAccount);

			AR_Invoice = TestObjectCreator.CreateInvoice(invoiceType, transactionNum, organisation: orgHeader);
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(AR_Invoice, al_OSExTaxAmount);
			invoiceLine.GenericCharge = chargeCode.PK;

			TaxFrameworkTestObjectCreator.CalculateTaxes(AR_Invoice);
		}

		AccTaxTransaction[] GetTaxTransactions(ZGuid value) => Factory.Load<AccTaxTransaction>(new ZQuery(AccTaxTransactionSchema.ATT_AH, value));

		AccTaxGLMovement[] GetGLMovements(ZGuid value) => Factory.Load<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, value));

		void setupCommonData()
		{
			TestObjectCreator.AUDBankAccount.GLHeader.AG_AccountNum = "AUDAcc";
			TestObjectCreator.USDBankAccount.GLHeader.AG_AccountNum = "USDAcc";
			TestObjectCreator.GLHeader1.AG_AccountNum = "GLHeader1";
			TestObjectCreator.GLHeader2.AG_AccountNum = "GLHeader2";
		}

		void setupPeriods()
		{
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(2008, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2009, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2010, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			helper.PostPeriodsForEntireYear(2011, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
		}

		void setupControlAccounts()
		{
			AccGLHeader aRSuspenseControlAccount = TestObjectCreator.CreateARSuspenseControlAccount();
			AccGLHeader aPSuspenseControlAccount = TestObjectCreator.CreateAPSuspenseControlAccount();
			AccGLHeader jobRevenueJournalControlAccount = TestObjectCreator.CreateJobRevenueJournalControlAccount();
			AccGLHeader cFXAccount = TestObjectCreator.CreateCFXAccount();
			AccGLHeader pendingInputTaxAccount = TestObjectCreator.CreateInputTaxReceivablePendingAccount();
			AccGLHeader pendingOutputTaxAccount = TestObjectCreator.CreateOutputTaxPayablePendingAccount();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid());
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), cFXAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingInputTaxAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingOutputTaxAccount.PK.ToGuid());
		}

		IAccountingCountryComplianceGlobalFactory CreateCountryComplianceFactoryWithGovernmentBatchReferenceInXUT()
		{
			var mockFactory = new Mock<IAccountingCountryComplianceGlobalFactory>();
			var mockFeatureConstants = new Mock<IFeatureConstants>();
			mockFactory.Setup(f => f.GetFeatureInterface<IFeatureConstants>(It.IsAny<ZString>()))
					   .Returns(mockFeatureConstants.Object);
			mockFeatureConstants.Setup(fc => fc.GetFeatureContants<SupportedFeatures>())
								.Returns(new SupportedFeatures(Features.IncludeGovernmentBatchReferenceInXUT));
			return mockFactory.Object;
		}

		void CreateEInvoicingBatchAndPivot(InvoicingBase invoice, ZInt batchNumber, string governmentAllocatedNumber, ZDateTime? lastSentTime = null, string actionType = EInvoicingPivotActionType.Submit, string pivotStatus = EInvoicingPivotState.Sent, ZDateTime? pivotSystemCreateTimeUtc = null)
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(batchNumber, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany, governmentAllocatedNumber);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, invoice, pivotStatus, actionType);
			pivot.AIP_LastSentTimeUtc = lastSentTime ?? ZDateTime.Empty;
			pivot.AIP_SystemCreateTimeUtc = pivotSystemCreateTimeUtc ?? ZDateTime.Empty;
		}

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		InvoicingBase AR_Invoice;

		TestObjectCreator TestObjectCreator { get; set; }

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);

			setupCommonData();
			setupPeriods();
			setupControlAccounts();

			AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			Factory.Save();
		}

		#endregion

	}
}

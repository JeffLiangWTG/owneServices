using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[UseSnapshotProtection]
	public class InvoicingBaseNonTransactionalTest : TransactionHeaderNonTransactionalTest
	{
		public void TestDontSkipFountainNumbersForAPInvoice()
		{
			var factory1 = new BusinessObjectFactory();
			var testObjectCreator1 = new TestObjectCreator(factory1);
			var invoiceInstance1 = testObjectCreator1.CreateInvoiceWithLine(typeof(APInvoiceForBulkPoster), "INV1", testObjectCreator1.AUD, 1, 10, 0, 10, 0);
			var invoiceInstance2 = factory1.Load<APInvoice>(invoiceInstance1.PK);
			AssertNotEquals("Precondition: different instances", invoiceInstance1, invoiceInstance2);
			AssertEquals("Precondition: the same data row", invoiceInstance1.PK, invoiceInstance2.PK);
			invoiceInstance2.IsSelfBillingInvoice = true;
			AssertEquals("Precondition: apInvoiceInstance1 should be self billing invoice to test AH_TransactionNum set from number fountain.", true, invoiceInstance1.IsSelfBillingInvoice);
			AssertEquals("Precondition: apInvoiceInstance2 should be self billing invoice to test AH_TransactionNum set from number fountain.", true, invoiceInstance2.IsSelfBillingInvoice);
			factory1.Save();
			AssertEquals("AH_ConsolidatedInvoiceRef", "00001000", invoiceInstance1.AH_ConsolidatedInvoiceRef);
			AssertEquals("AH_TransactionReference should be set for China only", "", invoiceInstance1.AH_TransactionReference);
			AssertEquals("AH_TransactionNum", "SB00001000", invoiceInstance1.AH_TransactionNum);

			invoiceInstance1 = testObjectCreator1.CreateInvoiceWithLine(typeof(APInvoiceForBulkPoster), "INV2", testObjectCreator1.AUD, 1, 10, 0, 10, 0);
			invoiceInstance2 = factory1.Load<APInvoice>(invoiceInstance1.PK);
			AssertNotEquals("Precondition: different instances", invoiceInstance1, invoiceInstance2);
			AssertEquals("Precondition: the same data row", invoiceInstance1.PK, invoiceInstance2.PK);
			invoiceInstance1.IsSelfBillingInvoice = true;
			AssertEquals("Precondition: apInvoiceInstance1 should be self billing invoice to test AH_TransactionNum set from number fountain.", true, invoiceInstance1.IsSelfBillingInvoice);
			AssertEquals("Precondition: apInvoiceInstance2 should be self billing invoice to test AH_TransactionNum set from number fountain.", true, invoiceInstance2.IsSelfBillingInvoice);
			var invoiceInvalidForSaving = testObjectCreator1.CreateInvoiceWithLine(typeof(APInvoice), "INV3", testObjectCreator1.AUD, 1, 10, 0, 10, 0);
			factory1.Saving += FactorySavingWithException;
			try
			{
				factory1.Save();
				Fail("Exception should be thrown to test unsuccessful saving.");
			}
			catch (NotImplementedException)
			{
			}
			finally
			{
				factory1.Saving -= FactorySavingWithException;
			}

			var factory2 = new BusinessObjectFactory();
			var testObjectCreator2 = new TestObjectCreator(factory2);
			var invoiceInstance1InNewFactory = testObjectCreator2.CreateInvoiceWithLine(typeof(APInvoiceForBulkPoster), "INV4", testObjectCreator2.AUD, 1, 10, 0, 10, 0);
			var invoiceInstance2InNewFactory = factory2.Load<APInvoice>(invoiceInstance1InNewFactory.PK);
			AssertNotEquals("Precondition: different instances", invoiceInstance1InNewFactory, invoiceInstance2InNewFactory);
			AssertEquals("Precondition: the same data row", invoiceInstance1InNewFactory.PK, invoiceInstance2InNewFactory.PK);
			invoiceInstance1InNewFactory.IsSelfBillingInvoice = true;
			AssertEquals("Precondition: apInvoiceInstance1 should be self billing invoice to test AH_TransactionNum set from number fountain.", true, invoiceInstance1InNewFactory.IsSelfBillingInvoice);
			AssertEquals("Precondition: apInvoiceInstance2 should be self billing invoice to test AH_TransactionNum set from number fountain.", true, invoiceInstance2InNewFactory.IsSelfBillingInvoice);
			factory2.Save();
			AssertEquals("AH_ConsolidatedInvoiceRef", "00001001", invoiceInstance1InNewFactory.AH_ConsolidatedInvoiceRef);
			AssertEquals("AH_TransactionReference should be set for China only", "", invoiceInstance1InNewFactory.AH_TransactionReference);
			AssertEquals("AH_TransactionNum", "SB00001001", invoiceInstance1InNewFactory.AH_TransactionNum);

			invoiceInstance1InNewFactory = testObjectCreator2.CreateInvoiceWithLine(typeof(APInvoiceForBulkPoster), "INV5", testObjectCreator2.AUD, 1, 10, 0, 10, 0);
			invoiceInstance2InNewFactory = factory2.Load<APInvoice>(invoiceInstance1InNewFactory.PK);
			AssertNotEquals("Precondition: different instances", invoiceInstance1InNewFactory, invoiceInstance2InNewFactory);
			AssertEquals("Precondition: the same data row", invoiceInstance1InNewFactory.PK, invoiceInstance2InNewFactory.PK);
			invoiceInstance1InNewFactory.IsSelfBillingInvoice = true;
			AssertEquals("Precondition: apInvoiceInstance1 should be self billing invoice to test AH_TransactionNum set from number fountain.", true, invoiceInstance1InNewFactory.IsSelfBillingInvoice);
			AssertEquals("Precondition: apInvoiceInstance2 should be self billing invoice to test AH_TransactionNum set from number fountain.", true, invoiceInstance2InNewFactory.IsSelfBillingInvoice);
			factory2.Save();
			AssertEquals("AH_ConsolidatedInvoiceRef", "00001002", invoiceInstance1InNewFactory.AH_ConsolidatedInvoiceRef);
			AssertEquals("AH_TransactionReference should be set for China only", "", invoiceInstance1InNewFactory.AH_TransactionReference);
			AssertEquals("AH_TransactionNum", "SB00001002", invoiceInstance1InNewFactory.AH_TransactionNum);

			invoiceInvalidForSaving.DeleteFromDB();
			factory1.Save();
			AssertEquals("AH_ConsolidatedInvoiceRef", "00001003", invoiceInstance1.AH_ConsolidatedInvoiceRef);
			AssertEquals("AH_TransactionReference should be set for China only", "", invoiceInstance1.AH_TransactionReference);
			AssertEquals("AH_TransactionNum", "SB00001003", invoiceInstance1.AH_TransactionNum);
		}

		public void TestSkipFountainNumbersForAH_TransactionReferenceForChina()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			var factory1 = new BusinessObjectFactory();
			var testObjectCreator1 = new TestObjectCreator(factory1);
			testObjectCreator1.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Guid.Empty, new JobHeaderStatusList());

			var invoiceInstance1 = testObjectCreator1.CreateInvoiceWithLine(typeof(APInvoiceForBulkPoster), "INV1", testObjectCreator1.AUD, 1, 10, 0, 10, 0);
			var invoiceInstance2 = factory1.Load<APInvoice>(invoiceInstance1.PK);
			AssertNotEquals("Precondition: different instances", invoiceInstance1, invoiceInstance2);
			AssertEquals("Precondition: the same data row", invoiceInstance1.PK, invoiceInstance2.PK);
			factory1.Save();
			AssertEquals("AH_TransactionReference", ZString.Empty, invoiceInstance1.AH_TransactionReference);

			invoiceInstance1 = testObjectCreator1.CreateInvoiceWithLine(typeof(APInvoiceForBulkPoster), "INV2", testObjectCreator1.AUD, 1, 10, 0, 10, 0);
			invoiceInstance2 = factory1.Load<APInvoice>(invoiceInstance1.PK);
			AssertNotEquals("Precondition: different instances", invoiceInstance1, invoiceInstance2);
			AssertEquals("Precondition: the same data row", invoiceInstance1.PK, invoiceInstance2.PK);
			var invoiceInvalidForSaving = testObjectCreator1.CreateInvoiceWithLine(typeof(APInvoice), "INV3", testObjectCreator1.AUD, 1, 10, 0, 10, 0);
			factory1.Saving += FactorySavingWithException;
			try
			{
				factory1.Save();
				Fail("Exception should be thrown to test unsuccessful saving.");
			}
			catch (NotImplementedException)
			{
			}
			finally
			{
				factory1.Saving -= FactorySavingWithException;
			}

			var factory2 = new BusinessObjectFactory();
			var testObjectCreator2 = new TestObjectCreator(factory2);
			testObjectCreator2.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Guid.Empty, new JobHeaderStatusList());

			var invoiceInstance1InNewFactory = testObjectCreator2.CreateInvoiceWithLine(typeof(APInvoiceForBulkPoster), "INV4", testObjectCreator2.AUD, 1, 10, 0, 10, 0);
			var invoiceInstance2InNewFactory = factory2.Load<APInvoice>(invoiceInstance1InNewFactory.PK);
			AssertNotEquals("Precondition: different instances", invoiceInstance1InNewFactory, invoiceInstance2InNewFactory);
			AssertEquals("Precondition: the same data row", invoiceInstance1InNewFactory.PK, invoiceInstance2InNewFactory.PK);
			factory2.Save();
			AssertEquals("AH_TransactionReference", ZString.Empty, invoiceInstance1InNewFactory.AH_TransactionReference);

			invoiceInstance1InNewFactory = testObjectCreator2.CreateInvoiceWithLine(typeof(APInvoiceForBulkPoster), "INV5", testObjectCreator2.AUD, 1, 10, 0, 10, 0);
			invoiceInstance2InNewFactory = factory2.Load<APInvoice>(invoiceInstance1InNewFactory.PK);
			AssertNotEquals("Precondition: different instances", invoiceInstance1InNewFactory, invoiceInstance2InNewFactory);
			AssertEquals("Precondition: the same data row", invoiceInstance1InNewFactory.PK, invoiceInstance2InNewFactory.PK);
			factory2.Save();
			AssertEquals("AH_TransactionReference", ZString.Empty, invoiceInstance1InNewFactory.AH_TransactionReference);

			invoiceInvalidForSaving.DeleteFromDB();
			factory1.Save();
			AssertEquals("AH_TransactionReference", ZString.Empty, invoiceInstance1.AH_TransactionReference);
		}

		public void TestShouldConfirmComplianceSubTypeCanReverse()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);

			var invoice = testObjectCreator.CreateARInvoice<ARInvoice>("INV0001", testObjectCreator.AUD, 1m, testObjectCreator.AALSHI);
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_TransactionReference = "test";
			invoice.AH_ComplianceSubType = "tes";
			factory.Save();

			Assert("This Invoice should confirm complianceSubType can reverse", invoice.ShouldConfirmComplianceSubTypeCanReverse);

			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print);

			var arInvoice = testObjectCreator.CreateARInvoice<ARInvoice>("00001001", testObjectCreator.LocalCurrency, 1, testObjectCreator.Debtor);
			arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice.AH_TransactionType = TransactionTypes.Invoice;
			arInvoice.AH_TransactionReference = "test";
			arInvoice.AH_ComplianceSubType = "tes";
			var arInvoiceLine = testObjectCreator.CreateARInvoiceLine(arInvoice, null, testObjectCreator.FRT, testObjectCreator.LocalCurrency, 1, "desc", 100);
			arInvoiceLine.AL_AT = testObjectCreator.CAP.PK;

			new ComplianceDocumentCreator(new[] { arInvoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
			factory.Save();

			Assert("This Invoice should not confirm complianceSubType can reverse", !arInvoice.ShouldConfirmComplianceSubTypeCanReverse);
		}

		public void TestDontSkipFountainNumbersForAH_TransactionReferenceForComplianceSequence()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Mexico);

			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var item = collection.AddNew();
			item.Country = Enterprise.Core.Constants.CountryCodes.Mexico;
			item.SubType = "TXI";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID; // "TID";
			item.DisbursementRule = DisbursementRuleCodes.NonDisbursementOnly; // "NDB";
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly; //"OTO";
			item.OrganisationLocation = "";
			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			var factory1 = new BusinessObjectFactory();
			var testObjectCreator1 = new TestObjectCreator(factory1);

			ZGuid menuPK = factory1.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "VN Govt Tax Invoice")).PK;
			AccComplianceSequence sequence = testObjectCreator1.CreateNewComplianceSequence(menuPK, "TXI", 1, 100, 25);
			sequence.XD_Prefix = "01.02-";
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			factory1.Save();

			var invoiceInstance1 = testObjectCreator1.CreateInvoiceWithLine(typeof(ARInvoiceToTestSecondInstance), "", testObjectCreator1.AUD, 1, 10, 0, 10, 0);
			var invoiceInstance2 = factory1.Load<ARInvoice>(invoiceInstance1.PK);
			invoiceInstance1.Lines[0].AL_AT = testObjectCreator1.GST1.PK;
			AssertNotEquals("Precondition: different instances", invoiceInstance1, invoiceInstance2);
			AssertEquals("Precondition: the same data row", invoiceInstance1.PK, invoiceInstance2.PK);
			factory1.Save();
			AssertEquals("AH_TransactionReference", "01.02-000000025", invoiceInstance1.AH_TransactionReference);

			invoiceInstance1 = testObjectCreator1.CreateInvoiceWithLine(typeof(ARInvoice), "", testObjectCreator1.AUD, 1, 10, 0, 10, 0);
			invoiceInstance2 = factory1.Load<ARInvoiceToTestSecondInstance>(invoiceInstance1.PK);
			invoiceInstance2.Lines[0].AL_AT = testObjectCreator1.GST1.PK;
			AssertNotEquals("Precondition: different instances", invoiceInstance1, invoiceInstance2);
			AssertEquals("Precondition: the same data row", invoiceInstance1.PK, invoiceInstance2.PK);
			var invoiceInvalidForSaving = testObjectCreator1.CreateInvoiceWithLine(typeof(ARInvoice), "", testObjectCreator1.AUD, 1, 10, 0, 10, 0);
			factory1.Saving += FactorySavingWithException;
			try
			{
				factory1.Save();
				Fail("Exception should be thrown to test unsuccessful saving.");
			}
			catch (NotImplementedException)
			{
			}
			finally
			{
				factory1.Saving -= FactorySavingWithException;
			}

			var factory2 = new BusinessObjectFactory();
			var testObjectCreator2 = new TestObjectCreator(factory2);
			var invoiceInstance1InNewFactory = testObjectCreator2.CreateInvoiceWithLine(typeof(ARInvoiceToTestSecondInstance), "", testObjectCreator2.AUD, 1, 10, 0, 10, 0);
			var invoiceInstance2InNewFactory = factory2.Load<ARInvoice>(invoiceInstance1InNewFactory.PK);
			invoiceInstance2InNewFactory.Lines[0].AL_AT = testObjectCreator1.GST1.PK;
			AssertNotEquals("Precondition: different instances", invoiceInstance1InNewFactory, invoiceInstance2InNewFactory);
			AssertEquals("Precondition: the same data row", invoiceInstance1InNewFactory.PK, invoiceInstance2InNewFactory.PK);
			factory2.Save();
			AssertEquals("AH_TransactionReference", "01.02-000000026", invoiceInstance1InNewFactory.AH_TransactionReference);

			invoiceInstance1InNewFactory = testObjectCreator2.CreateInvoiceWithLine(typeof(ARInvoice), "", testObjectCreator2.AUD, 1, 10, 0, 10, 0);
			invoiceInstance2InNewFactory = factory2.Load<ARInvoiceToTestSecondInstance>(invoiceInstance1InNewFactory.PK);
			invoiceInstance1InNewFactory.Lines[0].AL_AT = testObjectCreator1.GST1.PK;
			AssertNotEquals("Precondition: different instances", invoiceInstance1InNewFactory, invoiceInstance2InNewFactory);
			AssertEquals("Precondition: the same data row", invoiceInstance1InNewFactory.PK, invoiceInstance2InNewFactory.PK);
			factory2.Save();
			AssertEquals("AH_TransactionReference", "01.02-000000027", invoiceInstance1InNewFactory.AH_TransactionReference);

			invoiceInvalidForSaving.DeleteFromDB();
			factory1.Save();
			AssertEquals("AH_TransactionReference", "01.02-000000028", invoiceInstance1.AH_TransactionReference);
		}

		public void TestSaveAsIncompleteAfterConcurrencyError()
		{
			// simulate a data save collision with another user by creating a competing invoice before the main invoice is saved.

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			// create a job to be invoiced
			TestObjectCreator objectCreator1 = new TestObjectCreator(factory1);
			var shipment1 = objectCreator1.CreateShipment("S0001");
			var job1 = objectCreator1.CreateJob(shipment1, false, false);
			var charge1 = objectCreator1.CreateCharge(job1, objectCreator1.CC1, 50m, 50m);

			factory1.Save();
			AssertEquals("Precondition: jobcharge 1 should not be posted.", false, charge1.IsCostPosted);

			// create 1st invoice using original shipment objects
			var invoice1 = objectCreator1.CreateAPInvoice<APInvoice>("10001", objectCreator1.AUD, 1m, 50m, 0m, 0m, 50m, 0m, 0m);
			var inv1Line1 = invoice1.Lines[0];
			charge1.ReverseAccrual(ZDateTime.Now);
			charge1.JR_AL_APLine = inv1Line1.PK;
			charge1.SetAmountsToLinkedLinesForTests();

			//Create a second invoice
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			TestObjectCreator objectCreator2 = new TestObjectCreator(factory2);
			var invoice2 = objectCreator2.CreateAPInvoice<APInvoice>("10002", objectCreator2.AUD, 1m, 50m, 0m, 0m, 50m, 0m, 0m);

			// load shipment from database using PK
			var shipmentCopy = factory2.Load<ForwardingShipment>(shipment1.PK);
			var loader = new Job.Loader(factory2, shipmentCopy);
			var job2 = loader.Load();

			// Attach loaded charges to invoice 2
			var inv2Line1 = invoice2.Lines[0];
			var charge2 = job2.Charges[0];
			AssertEquals("Precondition: jobcharge 2 should not be posted.", false, charge2.IsCostPosted);

			charge2.ReverseAccrual(ZDateTime.Now);
			charge2.JR_AL_APLine = inv2Line1.PK;
			var line = charge2.APLine;
			AssertNotEquals("Precondition: jobcharge 2 line should exist.", null, line);
			charge2.SetAmountsToLinkedLinesForTests();

			// Post the first invoice
			factory1.Save();
			AssertEquals("Precondition: invoice 1 should be saved.", true, invoice1.IsInDatabase);
			AssertEquals("Precondition: jobcharge 1 should be posted.", true, charge1.IsCostPosted);
			AssertEquals("Precondition: jobcharge 2 should be marked HasChanges.", true, charge2.HasChanges);

			// Post the second invoice
			try
			{
				factory2.Save();
				Fail("Exception should be thrown to test unsuccessful saving.");
			}
			catch (Exception e)
			{
				var msg = e.Message;

				string expectedErrorMessage = $@"
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Tablename: AccTransactionLines";

				AssertEquals("Precondition: Invoice 2 should fail to save.", true, msg.StartsWith(expectedErrorMessage));

				// initial save fails, perform 'save incomplete', should not trigger "Resetting HasChanges" error in JobCharge
				invoice2.SaveAsIncomplete();
			}

			AssertEquals("Invoice 2 should be saved.", true, invoice2.IsInDatabase);
			AssertEquals("jobcharge 2 should remain marked HasChanges.", true, charge2.HasChanges);
		}

		public void TestLoadInvoiceLineTaxSummaries()
		{
			var factory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(factory);
			var invoice = objectCreator.CreateAPInvoice<APInvoice>("1", objectCreator.AUD, 1m, 1000m, 100m, 0m, 1000m, 100m, 0m);
			var creditor = objectCreator.Creditor1;
			var msg1 = factory.NewWithValidTestData<AccInvMsg>();
			var msg2 = factory.NewWithValidTestData<AccInvMsg>();
			var invoiceLine1 = invoice.Lines[0];

			invoice.AH_OH = creditor.PK;
			objectCreator.GST1.AT_A9_DefaultVatClass = msg1.PK;
			msg1.A9_Code = "TEST";
			msg2.A9_Code = "NTEST";

			invoice.IsTaxSummaryTabSelected = false;

			var taxSummaryCollection = invoice.InvoiceLineTaxSummaries; // will load data when call Load method and Is At Tax Summary Tab
			AssertEquals("Should contain no item", 0, taxSummaryCollection.Count);

			invoice.IsTaxSummaryTabSelected = true;
			invoice.LoadInvoiceLineTaxSummaries();
			AssertEquals("Should contain no item", 0, taxSummaryCollection.Count);

			var invoiceLine2 = objectCreator.CreateInvoiceLine(invoice, objectCreator.AUD, 1m, 10m, 20m, 30m);
			var invoiceLine3 = objectCreator.CreateInvoiceLine(invoice, objectCreator.AUD, 1m, 11m, 21m, 31m);
			var invoiceLine4 = objectCreator.CreateInvoiceLine(invoice, objectCreator.AUD, 1m, 12m, 22m, 32m);
			var invoiceLine5 = objectCreator.CreateInvoiceLine(invoice, objectCreator.AUD, 1m, 13m, 23m, 33m);
			invoiceLine2.AL_AT = objectCreator.GSTFREE1.PK;
			invoiceLine3.AL_AT = objectCreator.GST1.PK;
			invoiceLine4.AL_AT = objectCreator.GST1.PK;
			invoiceLine5.AL_AT = objectCreator.GST1.PK;
			invoiceLine5.AL_A9_VATClass = msg2.PK;

			invoice.IsTaxSummaryTabSelected = false;
			invoice.LoadInvoiceLineTaxSummaries();
			AssertEquals("Should still contain no item", 0, taxSummaryCollection.Count);

			invoice.IsTaxSummaryTabSelected = true;
			invoice.LoadInvoiceLineTaxSummaries();
			AssertEquals("Should group by 3 items", taxSummaryCollection.Count, 3);
			TaxSummaryTestHelper.AssertTaxSummaryResult(taxSummaryCollection[0], objectCreator.GSTFREE1.AT_Code, null, 10, invoiceLine2.AL_LocalTaxAmount, invoiceLine2.AL_LocalTotalAmount, objectCreator.GSTFREE1.AT_Description);
			TaxSummaryTestHelper.AssertTaxSummaryResult(taxSummaryCollection[1], objectCreator.GST1.AT_Code, "TEST", 23, invoiceLine3.AL_LocalTaxAmount + invoiceLine4.AL_LocalTaxAmount, invoiceLine3.AL_LocalTotalAmount + invoiceLine4.AL_LocalTotalAmount, objectCreator.GST1.AT_Description);
			TaxSummaryTestHelper.AssertTaxSummaryResult(taxSummaryCollection[2], objectCreator.GST1.AT_Code, "NTEST", 13, invoiceLine5.AL_LocalTaxAmount, invoiceLine5.AL_LocalTotalAmount, objectCreator.GST1.AT_Description);
		}

		public void TestInvoiceLineTaxSummariesRefreshByImportWhenTabNotSelected()
		{
			InvoiceLineTaxSummariesImportRefreshCore(false);
		}

		public void TestInvoiceLineTaxSummariesRefreshByImportWhenTabSelected()
		{
			InvoiceLineTaxSummariesImportRefreshCore(true);
		}

		public void TestInvoiceLineTaxSummariesWhenExchangeRateChangedAndTabNotSelected()
		{
			InvoiceLineTaxSummariesWhenExchangeRateChangedCore(false);
		}

		public void TestInvoiceLineTaxSummariesWhenExchangeRateChangedAndTabSelected()
		{
			InvoiceLineTaxSummariesWhenExchangeRateChangedCore(true);
		}

		public void TestWHTAmountLoad()
		{
			var factory = new BusinessObjectFactory();
			var invoice = factory.New<APInvoice>();

			(ZGuid TransactionPK, ZDecimal NotionalAmnt, ZDecimal RealizedAmnt) amnt = (invoice.PK, 20M, 45M);
			var inputNotionalPK = ZGuid.Empty;
			var inputRealizedPK = ZGuid.Empty;

			var loaderMock = new Mock<IWHTAmountLoader>();
			loaderMock.Setup(x => x.GetNotionalWHT(It.IsAny<ZGuid>())).Returns(20M).Callback<ZGuid>(x => inputNotionalPK = x);
			loaderMock.Setup(x => x.GetRealizedWHT(It.IsAny<ZGuid>())).Returns(45M).Callback<ZGuid>(x => inputRealizedPK = x);

			TaxFrameworkObjectFactory.SubstituteWHTAmountLoader_ForTestOnly(factory, loaderMock.Object);

			AssertEquals(20M, invoice.AH_NotionalWHTTax);
			AssertEquals(amnt.TransactionPK, inputNotionalPK);
			AssertEquals(45M, invoice.AH_RealizedWHTTax);
			AssertEquals(amnt.TransactionPK, inputRealizedPK);
		}

		void FactorySavingWithException(BusinessObjectFactory factory)
		{
			throw new NotImplementedException();
		}

		#region implementation

		void InvoiceLineTaxSummariesImportRefreshCore(bool isTaxSummaryTabSelected)
		{
			var factory = new BusinessObjectFactory();
			var branch = GlbBranch.CurrentBranch;
			var objectCreator = new TestObjectCreator(factory);
			var invoice = objectCreator.CreateAPInvoice<APInvoice>("Test", objectCreator.AUD, 1m, 1000m, 0m, 0m, 1000m, 0m, 0m);
			invoice.AH_OH = objectCreator.Creditor1.PK;

			AssertEquals("Should contains 1 line", invoice.Lines.Count, 1);

			var invoiceLine1 = invoice.Lines[0];
			invoiceLine1.AL_AT = objectCreator.GSTFREE1.PK;
			invoice.IsTaxSummaryTabSelected = isTaxSummaryTabSelected;

			TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoice);

			var jobCharges = new List<Charge>();
			var charge = factory.NewWithValidTestData<Charge>();
			charge.JR_OH_CostAccount = objectCreator.AALSHI.PK;
			charge.JR_RX_NKCostCurrency = "AUD";
			charge.JR_OSCostExRate = 1m;
			charge.JR_LocalCostAmt = 100m;
			jobCharges.Add(charge);

			AssertEquals("Should still contains 1 lines", 1, invoice.Lines.Count);

			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoice.ImportJobChargesIntoInvoice(jobCharges.ToArray<Charge>(), invoiceLine); // ImportJobChargesIntoInvoice
			AssertEquals("Should contains 2 lines", invoice.Lines.Count, 2);

			if (isTaxSummaryTabSelected)
			{
				TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesCount(invoice, 1);
				TaxSummaryTestHelper.AssertTaxSummaryResult(invoice.InvoiceLineTaxSummaries[0], invoice.Lines[0].TaxRate?.AT_Code ?? ZString.Empty, invoice.Lines[0].VATClass?.A9_Code ?? ZString.Empty, 1000, 0, 1000, invoice.Lines[0].TaxRate?.AT_Description ?? ZString.Empty);
			}
			else
			{
				TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoice);
			}

			var consol = objectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment1 = objectCreator.CreateShipment("S00001001", "AUSYD", "NZAKL", consol);
			var shipment2 = objectCreator.CreateShipment("S00001002", "AUSYD", "NZAKL", consol);
			var job1 = objectCreator.CreateJob(shipment1, false);
			var job2 = objectCreator.CreateJob(shipment2, false);
			var cost = objectCreator.CreateConsolCost(invoice, consol, objectCreator.CC1, 100m);

			AssertEquals("Should still contains 2 lines", invoice.Lines.Count, 2);
			invoice.ImportAllApportionmentsFromCosting(); // ImportAllApportionmentsFromCosting
			AssertEquals("Should contains 4 lines", invoice.Lines.Count, 4);

			if (isTaxSummaryTabSelected)
			{
				TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesCount(invoice, 2);
				TaxSummaryTestHelper.AssertTaxSummaryResult(invoice.InvoiceLineTaxSummaries[0], invoice.Lines[0].TaxRate?.AT_Code ?? ZString.Empty, invoice.Lines[0].VATClass?.A9_Code ?? ZString.Empty, 1000, 0, 1000, invoice.Lines[0].TaxRate?.AT_Description ?? ZString.Empty);
				TaxSummaryTestHelper.AssertTaxSummaryResult(invoice.InvoiceLineTaxSummaries[1], invoice.Lines[2].TaxRate?.AT_Code ?? ZString.Empty, invoice.Lines[2].VATClass?.A9_Code ?? ZString.Empty, 100, 10, 110, invoice.Lines[2].TaxRate?.AT_Description ?? ZString.Empty);
			}
			else
			{
				TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoice);
			}
		}

		void InvoiceLineTaxSummariesWhenExchangeRateChangedCore(bool isTaxSummaryTabSelected)
		{
			var factory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(factory);
			var invoice = objectCreator.CreateAPInvoice<APInvoice>("Test", objectCreator.AUD, 1m, 1000m, 100m, 0m, 1000m, 100m, 0m);
			invoice.AH_OH = objectCreator.Creditor1.PK;
			var invoiceLine1 = invoice.Lines[0];
			invoiceLine1.AL_AT = objectCreator.GST1.PK;

			AssertEquals("Should contains 1 line", invoice.Lines.Count, 1);
			AssertEquals("ExchangeRate should be", 1m, invoice.AH_ExchangeRate);

			invoice.IsTaxSummaryTabSelected = isTaxSummaryTabSelected;

			TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoice);

			invoice.AH_ExchangeRate = 2m;
			AssertEquals("ExchangeRate should be", 2m, invoice.AH_ExchangeRate);
			AssertEquals("Should contains 1 line", invoice.Lines.Count, 1);

			if (isTaxSummaryTabSelected)
			{
				TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesCount(invoice, 1);
				TaxSummaryTestHelper.AssertTaxSummaryResult(invoice.InvoiceLineTaxSummaries[0], invoice.Lines[0].TaxRate?.AT_Code ?? ZString.Empty, invoice.Lines[0].VATClass?.A9_Code ?? ZString.Empty, 500, 50, 550, invoice.Lines[0].TaxRate?.AT_Description ?? ZString.Empty); // InvoiceLineTaxSummaries should be refresh
			}
			else
			{
				TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoice);
			}
		}

		#endregion
	}
}

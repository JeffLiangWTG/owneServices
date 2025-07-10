using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ExRateOption = Enterprise.Accounting.Business.AccountingConstants.InvoicePostingExchangeRateOption;
using IQRCodeDataProvider = Enterprise.Accounting.Business.AccountingCountryFactory.IQRCodeDataProvider;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocARInvoice))]
	class DocARInvoiceTest : DocARInvoiceCommonTest
	{
		public void TestCrossExchangeRatesBasedOnJobChargeSellCurrency()
		{
			var gbpCurr = TestObjectCreator.GBP;
			var usdCurr = TestObjectCreator.USD;
			var eurCurr = TestObjectCreator.EUR;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			GlbCompany.CurrentCompany.Factory.Save();
			var invoice = Factory.New<ARInvoice>();
			var wrapper = DocARInvoice.New(invoice, Factory);

			var line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge1 = Factory.New<Charge>();

			var line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge2 = Factory.New<Charge>();

			var line3 = (InvoicingLineBase)invoice.Lines.AddNew();
			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge3 = Factory.New<Charge>();

			var line4 = (InvoicingLineBase)invoice.Lines.AddNew();
			var job4 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var charge4 = Factory.New<Charge>();

			charge1.JR_AL_ARLine = line1.PK;
			line1.AL_JH = job1.PK;
			charge1.JR_JH = job1.PK;

			charge2.JR_AL_ARLine = line2.PK;
			line2.AL_JH = job2.PK;
			charge2.JR_JH = job2.PK;

			charge3.JR_AL_ARLine = line3.PK;
			line3.AL_JH = job3.PK;
			charge3.JR_JH = job3.PK;

			charge4.JR_AL_ARLine = line4.PK;
			line4.AL_JH = job4.PK;
			charge4.JR_JH = job4.PK;

			var receivableCharge1 = charge1 as IReceivablesPostingCharge;
			var receivableCharge2 = charge2 as IReceivablesPostingCharge;
			var receivableCharge3 = charge3 as IReceivablesPostingCharge;
			var receivableCharge4 = charge4 as IReceivablesPostingCharge;

			charge1.JR_RX_NKSellCurrency = usdCurr.RX_Code;
			charge1.JR_OSSellAmt = 100M;
			charge1.JR_OSSellExRate = 0.5M;
			AssertEquals("JR_LocalSellAmt", 50m, charge1.JR_LocalSellAmt);

			charge2.JR_RX_NKSellCurrency = usdCurr.RX_Code;
			charge2.JR_OSSellAmt = 151M;
			charge2.JR_OSSellExRate = 0.5M;
			AssertEquals("JR_LocalSellAmt", 75.50m, charge2.JR_LocalSellAmt);

			charge3.JR_RX_NKSellCurrency = gbpCurr.RX_Code;
			charge3.JR_OSSellAmt = 555M;
			charge3.JR_OSSellExRate = 4.7874M;
			AssertEquals("JR_LocalSellAmt", 2657.01m, charge3.JR_LocalSellAmt);

			charge4.JR_RX_NKSellCurrency = gbpCurr.RX_Code;
			charge4.JR_OSSellAmt = 666M;
			charge4.JR_OSSellExRate = 4.7874M;
			AssertEquals("JR_LocalSellAmt", 3188.41m, charge4.JR_LocalSellAmt);

			var eurRate1 = ((IExchangeRateProvider)job1).GetExchangeRate(eurCurr.RX_Code, charge1.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			eurRate1.SetBuyRate_ForTestOnly(0.6m);
			var eurRate2 = ((IExchangeRateProvider)job2).GetExchangeRate(eurCurr.RX_Code, charge2.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			eurRate2.SetBuyRate_ForTestOnly(0.6m);
			var eurRate3 = ((IExchangeRateProvider)job3).GetExchangeRate(eurCurr.RX_Code, charge3.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			eurRate3.SetBuyRate_ForTestOnly(3.685m);
			var eurRate4 = ((IExchangeRateProvider)job4).GetExchangeRate(eurCurr.RX_Code, charge4.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			eurRate4.SetBuyRate_ForTestOnly(3.685m);

			var usdRate1 = ((IExchangeRateProvider)job1).GetExchangeRate(usdCurr.RX_Code, charge1.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			usdRate1.SetBuyRate_ForTestOnly(0.5m);
			var usdRate2 = ((IExchangeRateProvider)job2).GetExchangeRate(usdCurr.RX_Code, charge2.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			usdRate2.SetBuyRate_ForTestOnly(0.5m);
			var gbpRate1 = ((IExchangeRateProvider)job3).GetExchangeRate(gbpCurr.RX_Code, charge3.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			gbpRate1.SetBuyRate_ForTestOnly(4.7874m);
			var gbpRate2 = ((IExchangeRateProvider)job4).GetExchangeRate(gbpCurr.RX_Code, charge4.JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, true);
			gbpRate2.SetBuyRate_ForTestOnly(4.7874m);

			charge1.ClearRevenueLinkOnlyTemporary();
			charge1.JR_RX_NKSellInvoiceCurrency = eurCurr.RX_Code;
			charge2.ClearRevenueLinkOnlyTemporary();
			charge2.JR_RX_NKSellInvoiceCurrency = eurCurr.RX_Code;
			charge3.ClearRevenueLinkOnlyTemporary();
			charge3.JR_RX_NKSellInvoiceCurrency = eurCurr.RX_Code;
			charge4.ClearRevenueLinkOnlyTemporary();
			charge4.JR_RX_NKSellInvoiceCurrency = eurCurr.RX_Code;

			AssertEquals("Sell Inv Amt", 100m, charge1.JR_OSSellAmt);
			AssertEquals("OSSellAmount", 83.33m, receivableCharge1.OSSellAmount);
			AssertEquals("Sell Inv Amt", 151m, charge2.JR_OSSellAmt);
			AssertEquals("OSSellAmount", 125.83m, receivableCharge2.OSSellAmount);
			AssertEquals("Sell Inv Amt", 555m, charge3.JR_OSSellAmt);
			AssertEquals("OSSellAmount", 721.03m, receivableCharge3.OSSellAmount);
			AssertEquals("Sell Inv Amt", 666m, charge4.JR_OSSellAmt);
			AssertEquals("OSSellAmount", 865.24m, receivableCharge4.OSSellAmount);

			charge1.JR_AL_ARLine = line1.PK;
			charge2.JR_AL_ARLine = line2.PK;
			charge3.JR_AL_ARLine = line3.PK;
			charge4.JR_AL_ARLine = line4.PK;
			AssertEquals("2 currencies in dictionary", 2, wrapper.CrossExchangeRatesBasedOnJobChargeSellCurrency.Count);
			AssertEquals("Shows cross ex rate from USD to EUR", 0.833307m, wrapper.CrossExchangeRatesBasedOnJobChargeSellCurrency[usdCurr.RX_Code]);
			AssertEquals("Shows cross ex rate from GBP to EUR", 1.299156m, wrapper.CrossExchangeRatesBasedOnJobChargeSellCurrency[gbpCurr.RX_Code]);
		}

		public void TestEXVDocumentsFallback()
		{
			var orgHeader = TestObjectCreator.AALSHI;

			var documentsWithoutCompanyCodeAttrib = new List<JobRequiredDocument> {
				CreateEXVJobRequiredDocument(orgHeader, "001", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1))
			};

			var documentsWithCompanyCodeAttribForCurrentCompany = new List<JobRequiredDocument> {
				CreateEXVJobRequiredDocument(orgHeader, "002", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), companyCode: GlbCompany.CurrentCompany.GC_Code),
				CreateEXVJobRequiredDocument(orgHeader, "003", ZDateTime.Today.AddDays(-15), ZDateTime.Today.AddDays(15), companyCode: GlbCompany.CurrentCompany.GC_Code)
			};

			var documentsWithCompanyCodeAttribForNonCurrentCompany = new List<JobRequiredDocument> {
				CreateEXVJobRequiredDocument(orgHeader, "004", ZDateTime.Today.AddDays(-20), ZDateTime.Today.AddDays(20), companyCode: TestObjectCreator.NonCurrentCompany.GC_Code)
			};

			var expectedDocuments = new List<JobRequiredDocument>();
			expectedDocuments.AddRange(documentsWithoutCompanyCodeAttrib);
			expectedDocuments.AddRange(documentsWithCompanyCodeAttribForCurrentCompany);

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var wrapper = DocARInvoice.New(invoice, Factory);
			AssertArrayEqualsByElements(expectedDocuments.ToArray(), wrapper.EXVDocuments.ToArray());

			foreach (var document in documentsWithCompanyCodeAttribForCurrentCompany)
			{
				document.Attributes.DeleteAll();
			}

			expectedDocuments.Clear();
			expectedDocuments.AddRange(documentsWithoutCompanyCodeAttrib);
			expectedDocuments.AddRange(documentsWithCompanyCodeAttribForCurrentCompany);

			var wrapper2 = DocARInvoice.New(invoice, Factory);
			AssertArrayEqualsByElements(expectedDocuments.ToArray(), wrapper2.EXVDocuments.ToArray());
		}

		JobRequiredDocument CreateEXVJobRequiredDocument(OrgHeader orgHeader, string docNumber, ZDateTime dateReceived, ZDateTime validToDate, string notes = null, string companyCode = null)
		{
			var document = orgHeader.RequiredDocuments.AddNew();
			document.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			document.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
			document.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			document.EQ_RN_NKRelatedCountry = GlbCompany.CurrentCompany.Country.Code;
			document.EQ_DocNumber = docNumber;
			document.EQ_ParentID = orgHeader.PK;
			document.EQ_ParentTableCode = "OH";
			document.EQ_DateReceived = dateReceived.ToDateTimeOffset(null);
			document.EQ_ValidToDate = validToDate;
			document.EQ_DocumentNotes = notes ?? ZString.Empty;
			if (companyCode != null)
			{
				AddJobRequiredAttrib(document, JobRequiredDocAttribTypeList.Codes.CompanyCode, companyCode);
			}
			return document;
		}

		void AddJobRequiredAttrib(JobRequiredDocument document, string attribName, string attribValue)
		{
			var attrib = document.Attributes.AddNew();
			attrib.D0_AttribName = attribName;
			attrib.D0_AttribDisplayValue = attribValue;
		}

		public void TestHasEXVDocument()
		{
			var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
			taxMessage.A9_IsTriggerExemptionMessage = true;

			var document = TestObjectCreator.AALSHI.RequiredDocuments.AddNew();
			document.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			document.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
			document.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			document.EQ_RN_NKRelatedCountry = GlbCompany.CurrentCompany.Country.Code;
			document.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-1);
			document.EQ_ValidToDate = ZDateTime.Today.AddDays(1);

			//Invoice
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			var line2 = (InvoicingLineBase)invoice.Lines.AddNew();

			var wrapper = DocARInvoice.New(invoice, Factory);
			AssertEquals(false, wrapper.HasEXVDocuments);

			line2.AL_A9_VATClass = taxMessage.PK;
			AssertEquals(true, wrapper.HasEXVDocuments);

			//CreditNote
			var creditNote = Factory.New<ARCreditNote>();
			creditNote.AH_OH = TestObjectCreator.AALSHI.PK;
			var crline1 = (InvoicingLineBase)creditNote.Lines.AddNew();
			var crline2 = (InvoicingLineBase)creditNote.Lines.AddNew();

			wrapper = DocARInvoice.New(creditNote, Factory);
			AssertEquals(false, wrapper.HasEXVDocuments);

			crline2.AL_A9_VATClass = taxMessage.PK;
			AssertEquals(true, wrapper.HasEXVDocuments);

			//AdjustmentNote
			var adjustmentNote = Factory.New<ARAdjustmentNote>();
			adjustmentNote.AH_OH = TestObjectCreator.AALSHI.PK;
			var adjline1 = (InvoicingLineBase)adjustmentNote.Lines.AddNew();
			var adjline2 = (InvoicingLineBase)adjustmentNote.Lines.AddNew();

			wrapper = DocARInvoice.New(adjustmentNote, Factory);
			AssertEquals(false, wrapper.HasEXVDocuments);

			adjline2.AL_A9_VATClass = taxMessage.PK;
			AssertEquals(true, wrapper.HasEXVDocuments);
		}

		public void TestUseDocBuilderForSupplementaryDetails()
		{
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("False by default", false, InvoiceWrapper.UseDocBuilderForSupplementaryDetail);
			Invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;
			DocumentsDataRegistry.Instance.UseNewDocBuilderSupplementaryDetail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, InvoiceWrapper.UseDocBuilderForSupplementaryDetail);
		}

		public void TestUseDocBuilderForSupplementaryDetails_IsAmendingTransactionForPeriodicInvoiceWithMultipleJobs()
		{
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderSupplementaryDetail.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newFactory = Factory.CreateNewFactory();
				var testObjectCreator = new TestObjectCreator(newFactory);
				var invoice = newFactory.NewWithValidTestData<ARInvoice>();
				invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;

				var creditNote = invoice.GenerateAmendingTransaction<ARCreditNote>();
				var line1 = (ARCreditNoteLine)creditNote.Lines.AddNew();
				line1.AL_JH = testObjectCreator.Job1.PK;
				var line2 = (ARCreditNoteLine)creditNote.Lines.AddNew();
				line2.AL_JH = testObjectCreator.Job2.PK;

				InvoiceWrapper = DocARInvoice.New(creditNote, Factory);

				AssertEquals("Credit note is not periodic Invoice", false, InvoiceWrapper.IsPeriodicInvoice);
				AssertEquals("Use DocBuilder For SupplementaryDetail when credit note is amending transaction for periodic invoice with multiple jobs", true, InvoiceWrapper.UseDocBuilderForSupplementaryDetail);
			}
		}

		public void TestLinesForPeriodicInvoice()
		{
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			JobHeader loadListJob = GetInvoiceJob(loadList, Invoice);
			loadListJob.JH_JobNum = "11111111";
			((InvoicingBase)Invoice).Lines.AddNew();
			((InvoicingBase)Invoice).Lines[0].AL_JH = loadListJob.PK;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			JobHeader shipmentJob = GetInvoiceJob(shipment, Invoice);
			shipmentJob.JH_JobNum = "22222222";
			((InvoicingBase)Invoice).Lines.AddNew();
			((InvoicingBase)Invoice).Lines[1].AL_JH = shipmentJob.PK;

			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals(2, InvoiceWrapper.LinesForPeriodicInvoice.Count);
			AssertEquals("11111111", InvoiceWrapper.LinesForPeriodicInvoice[0].JobHeader.JobNumber);
			AssertEquals("22222222", InvoiceWrapper.LinesForPeriodicInvoice[1].JobHeader.JobNumber);
		}

		public void TestSequenceOfMSCLinesForPeriodicInvoice()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			line1.GenericCharge = TestObjectCreator.CC3.PK;
			line1.AL_OSExTaxAmount = 100m;

			InvoicingLineBase line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			line2.GenericCharge = TestObjectCreator.CC2.PK;
			line2.AL_OSExTaxAmount = 200m;

			InvoicingLineBase line3 = (InvoicingLineBase)invoice.Lines.AddNew();
			line3.GenericCharge = TestObjectCreator.CC1.PK;
			line3.AL_OSExTaxAmount = 300m;

			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;

			InvoiceWrapper = DocARInvoice.New(invoice, Factory);
			AssertEquals("Should have 3 lines in MSCLines", InvoiceWrapper.MSCInvoiceLine.Count, 3);
			AssertEquals("Sequence should be based on display sequence", InvoiceWrapper.MSCInvoiceLine[0].ChargeCode.Code, TestObjectCreator.CC3.AC_Code);
			AssertEquals("Sequence should be based on display sequence", InvoiceWrapper.MSCInvoiceLine[1].ChargeCode.Code, TestObjectCreator.CC2.AC_Code);
			AssertEquals("Sequence should be based on display sequence", InvoiceWrapper.MSCInvoiceLine[2].ChargeCode.Code, TestObjectCreator.CC1.AC_Code);
		}

		public void TestEXVDocumentProperties()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			invoice.AH_OH = debtor.PK;
			invoice.AH_PostDate = new DateTime(2011, 5, 6);

			var taxMessage = TestObjectCreator.TaxMsg1;
			taxMessage.A9_IsTriggerExemptionMessage = true;
			((InvoiceLine)invoice.Lines.AddNew()).AL_A9_VATClass = taxMessage.PK;

			var doc1 = CreateEXVJobRequiredDocument(debtor, "1234554321", new DateTime(2010, 12, 10), new DateTime(2011, 3, 5), notes: "hello world");
			AddJobRequiredAttrib(doc1, JobRequiredDocAttribTypeList.Codes.CompanyCode, GlbCompany.CurrentCompany.GC_Code);
			AddJobRequiredAttrib(doc1, JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate, "2016-11-23");
			AddJobRequiredAttrib(doc1, JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference, "12345678901234567-123456");
			AddJobRequiredAttrib(doc1, JobRequiredDocAttribTypeList.Codes.SellerControlNumber, "2016-000005");
			AddJobRequiredAttrib(doc1, JobRequiredDocAttribTypeList.Codes.BuyerIssueDate, "2016-04-04");
			InvalidateEXVDocument(doc1);

			var doc2 = CreateEXVJobRequiredDocument(debtor, "1234567890", new DateTime(2010, 12, 11), new DateTime(2011, 3, 15), notes: "hello world 2");
			AddJobRequiredAttrib(doc2, JobRequiredDocAttribTypeList.Codes.CompanyCode, GlbCompany.CurrentCompany.GC_Code);
			AddJobRequiredAttrib(doc2, JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate, "2016-11-24");
			AddJobRequiredAttrib(doc2, JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference, "12345678901234567-123459");
			AddJobRequiredAttrib(doc2, JobRequiredDocAttribTypeList.Codes.SellerControlNumber, "2016-000005");
			AddJobRequiredAttrib(doc2, JobRequiredDocAttribTypeList.Codes.BuyerIssueDate, "2016-04-05");
			InvalidateEXVDocument(doc2);

			var doc3 = CreateEXVJobRequiredDocument(debtor, "9876543210", new DateTime(2010, 12, 12), new DateTime(2011, 3, 16), notes: "hello world 3");
			AddJobRequiredAttrib(doc3, JobRequiredDocAttribTypeList.Codes.CompanyCode, GlbCompany.CurrentCompany.GC_Code);
			AddJobRequiredAttrib(doc3, JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate, "2016-11-25");
			AddJobRequiredAttrib(doc3, JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference, "12345678901234567-654321");
			AddJobRequiredAttrib(doc3, JobRequiredDocAttribTypeList.Codes.SellerControlNumber, "2016-000005");
			AddJobRequiredAttrib(doc3, JobRequiredDocAttribTypeList.Codes.BuyerIssueDate, "2016-04-06");
			InvalidateEXVDocument(doc3);

			InvoiceWrapper = DocARInvoice.New(invoice, Factory);

			AssertEquals("InvoiceWrapper.HasEXVDocuments", false, InvoiceWrapper.HasEXVDocuments);
			doc1.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			AssertEquals("InvoiceWrapper.HasEXVDocuments", false, InvoiceWrapper.HasEXVDocuments);
			doc1.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
			AssertEquals("InvoiceWrapper.HasEXVDocuments", false, InvoiceWrapper.HasEXVDocuments);
			doc1.EQ_RN_NKRelatedCountry = GlbCompany.CurrentCompany.Country.Code;

			invoice.AH_PostDate = new DateTime(2011, 2, 1);
			AssertEquals("InvoiceWrapper.HasEXVDocuments", true, InvoiceWrapper.HasEXVDocuments);
			AssertEquals("One valid document", 1, InvoiceWrapper.EXVDocuments.Count);

			ValidateEXVDocument(doc2);
			InvoiceWrapper = DocARInvoice.New(invoice, Factory);
			AssertEquals("Two valid documents", 2, InvoiceWrapper.EXVDocuments.Count);

			ValidateEXVDocument(doc3);
			InvoiceWrapper = DocARInvoice.New(invoice, Factory);
			AssertEquals("Three valid documents", 3, InvoiceWrapper.EXVDocuments.Count);

			AssertEquals("hello world\r\nhello world 2\r\nhello world 3\r\n", InvoiceWrapper.EXVNotes);
			AssertEquals("1234554321\r\n1234567890\r\n9876543210\r\n", InvoiceWrapper.EXVDocNumbers);
			AssertEquals("10-Dec-10\r\n11-Dec-10\r\n12-Dec-10\r\n", InvoiceWrapper.EXVReceivedDates);
			AssertEquals("04-Apr-16\r\n05-Apr-16\r\n06-Apr-16\r\n", InvoiceWrapper.EXVBuyerIssueDates);
			AssertEquals("23-Nov-16\r\n24-Nov-16\r\n25-Nov-16\r\n", InvoiceWrapper.EXVValidFromReceivedDates);
			AssertEquals("12345678901234567-123456\r\n12345678901234567-123459\r\n12345678901234567-654321\r\n", InvoiceWrapper.EXVProtocolloNumbers);

			void InvalidateEXVDocument(JobRequiredDocument document)
			{
				document.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				document.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
				document.EQ_RN_NKRelatedCountry = "XX";
			}

			void ValidateEXVDocument(JobRequiredDocument document)
			{
				document.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
				document.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
				document.EQ_RN_NKRelatedCountry = GlbCompany.CurrentCompany.Country.Code;
			}
		}

		public void TestInvoiceGetsSet()
		{
			AssertNotNull("Precondition: Invoice should not be null", Invoice);
			DocARInvoice aRInvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("ARInvoiceWrapper.Invoice", Invoice, aRInvoiceWrapper.Invoice);
		}

		public void TestInvoiceLineByChargeDoesntNegateCreditNoteLines()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			creator.CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			ARCreditNote creditNote = Factory.New<ARCreditNote>();
			creditNote.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			ARCreditNoteLine line1 = (ARCreditNoteLine)creditNote.Lines.AddNew();
			line1.AL_AC = creator.CC1.PK;
			line1.AL_OSExTaxAmount = 100m;
			ARCreditNoteLine line2 = (ARCreditNoteLine)creditNote.Lines.AddNew();
			line2.AL_AC = creator.CC1.PK;
			line2.AL_OSExTaxAmount = -50m;
			Factory.Save();

			DocARInvoice invoiceWrapper = DocARInvoice.New(creditNote, Factory);
			AssertEquals(1, invoiceWrapper.InvoiceLineByCharge.Count);
			AssertEquals(50m, invoiceWrapper.InvoiceLineByCharge[0].OSAmount);
		}

		public void TestInvoiceLineByJobDoesntNegateCreditNoteLines()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ARCreditNote creditNote = Factory.New<ARCreditNote>();
			creditNote.AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			ARCreditNoteLine line1 = (ARCreditNoteLine)creditNote.Lines.AddNew();
			line1.AL_JH = creator.Job1.PK;
			line1.AL_OSExTaxAmount = 100m;
			ARCreditNoteLine line2 = (ARCreditNoteLine)creditNote.Lines.AddNew();
			line2.AL_JH = creator.Job1.PK;
			line2.AL_OSExTaxAmount = -50m;

			DocARInvoice invoiceWrapper = DocARInvoice.New(creditNote, Factory);
			AssertEquals(1, invoiceWrapper.InvoiceLineByJob.Count);
			AssertEquals(50m, invoiceWrapper.InvoiceLineByJob[0].OSAmount);
		}

		[ExpectNoExceptions]
		public void TestInvoiceLineByJobRollupWithBolloLine()
		{
			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code);

			var shipment = TestObjectCreator.CreateShipment("S0001");
			shipment.JS_HouseBill = "99999";
			var job = TestObjectCreator.CreateJob(shipment, false);
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			ARInvoiceLine line1 = (ARInvoiceLine)invoice.Lines.AddNew();
			line1.AL_Desc = "Line1";
			line1.AL_JH = job.PK;
			line1.AL_OSExTaxAmount = 100m;
			line1.AL_AT = TestObjectCreator.GST1.PK;

			//Mimic a bollo line
			ARInvoiceLine line2 = (ARInvoiceLine)invoice.Lines.AddNew();
			line2.AL_Desc = "Line2";
			line2.AL_JH = ZGuid.Empty;
			line2.AL_OSExTaxAmount = 2m;
			line1.AL_AT = TestObjectCreator.GSTFREE1.PK;

			DocARInvoice invoiceWrapper = DocARInvoice.New(invoice, Factory);
			var linesGroupedByJob = invoiceWrapper.InvoiceLineByJob;
			AssertEquals(2, linesGroupedByJob.Count);
			AssertEquals("Bollo Line", 2m, linesGroupedByJob[0].OSAmount);
			AssertEquals("Bollo Line", "Line2", linesGroupedByJob[0].LineDescription);
			AssertEquals("Line with a job", 100m, linesGroupedByJob[1].OSAmount);
			AssertEquals("Line with a job", "99999", linesGroupedByJob[1].LineDescription);

			AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.TaxAmount.Code);
			invoiceWrapper = DocARInvoice.New(invoice, Factory);
			var linesGroupedByJob_AMTRuleType = invoiceWrapper.InvoiceLineByJob;
			AssertEquals(2, linesGroupedByJob_AMTRuleType.Count);
			AssertEquals("Bollo Line", 2m, linesGroupedByJob_AMTRuleType[0].OSAmount);
			AssertEquals("Bollo Line", "Line2", linesGroupedByJob_AMTRuleType[0].LineDescription);
			AssertEquals("Line with a job", 100m, linesGroupedByJob_AMTRuleType[1].OSAmount);
			AssertEquals("Line with a job", "99999", linesGroupedByJob_AMTRuleType[1].LineDescription);
		}

		public void TestBrandNameIsDependingOnRegistry()
		{
			DocARInvoice aRInvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			Guid currentBranchPK = GlbBranch.CurrentBranch.PK.ToGuid();

			AccountingConfigurationRegistry.Instance.PrintBranchAddressInFooter.SetValue(Guid.Empty, currentBranchPK, Guid.Empty, true);
			AssertEquals("Preconditions: Registry Item is set to True", true, AccountingConfigurationRegistry.Instance.PrintBranchAddressInFooter.GetValueWithoutFallback(Guid.Empty, currentBranchPK, Guid.Empty));
			AssertEquals("BrandName should be BranchName only", aRInvoiceWrapper.Branch.MailToAddress.CompanyName, aRInvoiceWrapper.BrandName);

			AccountingConfigurationRegistry.Instance.PrintBranchAddressInFooter.SetValue(Guid.Empty, currentBranchPK, Guid.Empty, false);
			AssertEquals("Preconditions: Registry Item is set to False", false, AccountingConfigurationRegistry.Instance.PrintBranchAddressInFooter.GetValueWithoutFallback(Guid.Empty, currentBranchPK, Guid.Empty));
			string expectedBrandName = GlbCompany.CurrentCompany.GC_Name.ToUpper();
			AssertEquals("BrandName should have the company information or the base behaviour", expectedBrandName, aRInvoiceWrapper.BrandName);
		}

		public void TestBrokerageJobType()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "TEST3";
			debtor.OH_IsDebtor = true;
			debtor.OH_IsConsignor = false;
			debtor.OH_IsConsignee = false;
			debtor.OH_IsBroker = false;
			debtor.OH_IsForwarder = false;

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_RL_NKOrigin = "NZAKL";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.JE_DeclarationReference = "B0001000";
			JobHeader job = GetInvoiceJob(declaration, Invoice);
			job.LocalChargesPK = debtor.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			AssertLines();
		}

		public void TestCFSLoadListJobType()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "TEST3";
			debtor.OH_IsDebtor = true;
			debtor.OH_IsConsignor = false;
			debtor.OH_IsConsignee = false;
			debtor.OH_IsBroker = false;
			debtor.OH_IsForwarder = false;

			CFSLoadListConsol loadListConsol = Factory.New<CFSLoadListConsol>();
			loadListConsol.JK_UniqueConsignRef = "C0001000";
			JobHeader job = GetInvoiceJob(loadListConsol, Invoice);
			job.LocalChargesPK = debtor.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.CFSLoadList.Code;
			AssertLines();
		}

		public void TestCFSShipmentJobType()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "TEST3";
			debtor.OH_IsDebtor = true;
			debtor.OH_IsConsignor = false;
			debtor.OH_IsConsignee = false;
			debtor.OH_IsBroker = false;
			debtor.OH_IsForwarder = false;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001000";
			shipment.JS_IsCFSRegistered = ZBool.True;
			shipment.JS_IsForwardRegistered = ZBool.False;

			JobHeader job = GetInvoiceJob(shipment, Invoice);
			job.LocalChargesPK = debtor.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.CFSShipment.Code;
			AssertLines();
		}

		public void TestCTOCusMAWBJobType()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "TEST3";
			debtor.OH_IsDebtor = true;
			debtor.OH_IsConsignor = false;
			debtor.OH_IsConsignee = false;
			debtor.OH_IsBroker = false;
			debtor.OH_IsForwarder = false;

			// Full namespace reference used tests only as production code for an invoice should never reference AU specific objects.
			// ie: Don't put "using Enterprise.Customs.AU.AirCargo.Business" at the top of the class or tests.
			var cTOMawb = Factory.New<Enterprise.Customs.AU.Declaration.Business.CTOCusMAWB>();
			JobHeader job = GetInvoiceJob(cTOMawb, Invoice);
			job.LocalChargesPK = debtor.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.CTOCusMAWB.Code;
			AssertLines();
		}

		public void TestCusMAWBJobType()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "TEST3";
			debtor.OH_IsDebtor = true;
			debtor.OH_IsConsignor = false;
			debtor.OH_IsConsignee = false;
			debtor.OH_IsBroker = false;
			debtor.OH_IsForwarder = false;

			// Full namespace reference used tests only as production code for an invoice should never reference AU specific objects.
			// ie: Don't put "using Enterprise.Customs.AU.AirCargo.Business" at the top of the class or tests.
			Enterprise.Customs.AU.Declaration.Business.CusMAWB mawb = Factory.New<Enterprise.Customs.AU.Declaration.Business.CusMAWB>();
			JobHeader job = GetInvoiceJob(mawb, Invoice);
			job.LocalChargesPK = debtor.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.CusMAWB.Code;
			AssertLines();
		}

		public void TestCusUnderbondJobType()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "TEST3";
			debtor.OH_IsDebtor = true;
			debtor.OH_IsConsignor = false;
			debtor.OH_IsConsignee = false;
			debtor.OH_IsBroker = false;
			debtor.OH_IsForwarder = false;

			Enterprise.Customs.AU.Declaration.Business.CusUnderbond underbond = Factory.New<Enterprise.Customs.AU.Declaration.Business.CusUnderbond>();
			JobHeader job = GetInvoiceJob(underbond, Invoice);
			job.LocalChargesPK = debtor.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.CusUnderbond.Code;
			AssertLines();
		}

		public void TestHasGSTANDQSTLine()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(ZBool.False, aRInvoiceWrapper.HasGSTANDQSTLine);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(ZBool.False, aRInvoiceWrapper.HasGSTANDQSTLine);

			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			taxRate.SetRateNumerator_ForTestOnly(10);
			taxRate.SetExtraRate_ForTestOnly(10, 1);
			line2.AL_AT = taxRate.PK;

			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.True, aRInvoiceWrapper.HasGSTANDQSTLine);

			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;

			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.True, aRInvoiceWrapper.HasGSTANDQSTLine);

			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;

			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.False, aRInvoiceWrapper.HasGSTANDQSTLine);
		}

		public void TestHasGSTANDQCTLine()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(ZBool.False, aRInvoiceWrapper.HasGSTANDEDULine);
				var line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				var line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;
				line2.AL_AG = TestObjectCreator.GLHeader1.PK;
				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(ZBool.False, aRInvoiceWrapper.HasGSTANDEDULine);

				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate.SetExtraRate_ForTestOnly(3, 1);
				taxRate.AT_Type = AccTaxRate.Types.Rated;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				line2.AL_AT = taxRate.PK;

				aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
				AssertEquals(ZBool.True, aRInvoiceWrapper.HasGSTANDQCTLine);
			}
		}

		public void TestHasGSTANDQCTLine_SER()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(ZBool.False, aRInvoiceWrapper.HasGSTANDEDULine);
				var line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				var line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;
				line2.AL_AG = TestObjectCreator.GLHeader1.PK;
				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(ZBool.False, aRInvoiceWrapper.HasGSTANDEDULine);

				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate.SetExtraRate_ForTestOnly(3, 1);
				taxRate.AT_Type = AccTaxRate.Types.ServiceTax;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				line2.AL_AT = taxRate.PK;

				aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
				AssertEquals(ZBool.True, aRInvoiceWrapper.HasGSTANDQCTLine);
			}
		}

		public void TestHasGSTANDEDULine()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(ZBool.False, aRInvoiceWrapper.HasGSTANDEDULine);
				var line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				var line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;
				line2.AL_AG = TestObjectCreator.GLHeader1.PK;
				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(ZBool.False, aRInvoiceWrapper.HasGSTANDEDULine);

				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate.SetExtraRate_ForTestOnly(3, 1);
				taxRate.AT_Type = AccTaxRate.Types.Rated;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				line2.AL_AT = taxRate.PK;

				aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
				AssertEquals(ZBool.True, aRInvoiceWrapper.HasGSTANDEDULine);
			}
		}

		public void TestHasGSTANDEDULine_SER()
		{
			using (TestObjectCreator.TemporarilyCustomiseTransactionNumberGenerator(GlbCompany.CurrentCompany, TestObjectCreator.CreateTestPrefixAndSequenceNumberCustomisation()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(ZBool.False, aRInvoiceWrapper.HasGSTANDEDULine);
				var line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				var line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;
				line2.AL_AG = TestObjectCreator.GLHeader1.PK;
				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(ZBool.False, aRInvoiceWrapper.HasGSTANDEDULine);

				var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate.SetExtraRate_ForTestOnly(3, 1);
				taxRate.AT_Type = AccTaxRate.Types.ServiceTax;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				line2.AL_AT = taxRate.PK;

				aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
				AssertEquals(ZBool.True, aRInvoiceWrapper.HasGSTANDEDULine);
			}
		}

		public void TestHasSPVLine()
		{
			Action setupAndAssertHasSPVLine = () =>
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(ZBool.False, aRInvoiceWrapper.HasSPVLine);
				var line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				var line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				line1.AL_AG = TestObjectCreator.GLHeader1.PK;
				line2.AL_AG = TestObjectCreator.GLHeader1.PK;
				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(ZBool.False, aRInvoiceWrapper.HasSPVLine);

				AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.SetExtraRate_ForTestOnly(0, 1);
				taxRate.AT_Type = AccTaxRate.Types.Rated;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;
				line2.AL_AT = taxRate.PK;

				aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
				AssertEquals(ZBool.True, aRInvoiceWrapper.HasSPVLine);

				line2.AL_AT = ZGuid.Empty;

				aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
				AssertEquals(ZBool.False, aRInvoiceWrapper.HasSPVLine);
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "ITROM";
				setupAndAssertHasSPVLine();
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.CostaRica))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort = "CRSJO";
				setupAndAssertHasSPVLine();
			}
		}

		public void TestHasRETLine()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(ZBool.False, aRInvoiceWrapper.HasRETLine);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(ZBool.False, aRInvoiceWrapper.HasRETLine);

			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.SetExtraRate_ForTestOnly(3, 1);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;
			line2.AL_AT = taxRate.PK;

			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.True, aRInvoiceWrapper.HasRETLine);
		}

		public void TestHSTLine()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(ZBool.False, aRInvoiceWrapper.HasGSTANDQSTLine);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(ZBool.False, aRInvoiceWrapper.HasGSTANDQSTLine);

			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "HST12";
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Canada;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			taxRate.SetRateNumerator_ForTestOnly(12);
			line2.AL_AT = taxRate.PK;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.True, aRInvoiceWrapper.HasHSTLine);

			line2.AL_AT = ZGuid.Empty;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.False, aRInvoiceWrapper.HasHSTLine);

			taxRate.AT_Code = "HST13";
			taxRate.SetRateNumerator_ForTestOnly(13);
			line2.AL_AT = taxRate.PK;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.True, aRInvoiceWrapper.HasHSTLine);

			line2.AL_AT = ZGuid.Empty;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.False, aRInvoiceWrapper.HasHSTLine);

			taxRate.AT_Code = "HST14";
			taxRate.SetRateNumerator_ForTestOnly(14);
			line2.AL_AT = taxRate.PK;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			Assert("HasHSTLine for HST14", aRInvoiceWrapper.HasHSTLine);

			line2.AL_AT = ZGuid.Empty;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.False, aRInvoiceWrapper.HasHSTLine);

			taxRate.AT_Code = "HST15";
			taxRate.SetRateNumerator_ForTestOnly(15);
			line2.AL_AT = taxRate.PK;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.True, aRInvoiceWrapper.HasHSTLine);

			line2.AL_AT = ZGuid.Empty;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.False, aRInvoiceWrapper.HasHSTLine);

			taxRate.AT_Code = "CAPHST12";
			taxRate.SetRateNumerator_ForTestOnly(12);
			taxRate.AT_Type = AccTaxRate.Types.CapitalRated;
			line2.AL_AT = taxRate.PK;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.True, aRInvoiceWrapper.HasHSTLine);

			line2.AL_AT = ZGuid.Empty;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.False, aRInvoiceWrapper.HasHSTLine);

			taxRate.AT_Code = "CAPHST13";
			taxRate.SetRateNumerator_ForTestOnly(13);
			taxRate.AT_Type = AccTaxRate.Types.CapitalRated;
			line2.AL_AT = taxRate.PK;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.True, aRInvoiceWrapper.HasHSTLine);

			line2.AL_AT = ZGuid.Empty;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.False, aRInvoiceWrapper.HasHSTLine);

			taxRate.AT_Code = "CAPHST15";
			taxRate.SetRateNumerator_ForTestOnly(15);
			taxRate.AT_Type = AccTaxRate.Types.CapitalRated;
			line2.AL_AT = taxRate.PK;
			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(ZBool.True, aRInvoiceWrapper.HasHSTLine);
		}

		public void TestLocalTransportOrCartageJobType()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "TEST3";
			debtor.OH_IsDebtor = true;
			debtor.OH_IsConsignor = false;
			debtor.OH_IsConsignee = false;
			debtor.OH_IsBroker = false;
			debtor.OH_IsForwarder = false;

			CommonCartage transportCartage = Factory.New<CommonCartage>();
			JobHeader job = GetInvoiceJob(transportCartage, Invoice);
			job.LocalChargesPK = debtor.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.LocalCartage.Code;
			AssertLines();
		}

		public void TestMasterAWBJobType()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "TEST3";
			debtor.OH_IsDebtor = true;
			debtor.OH_IsConsignor = false;
			debtor.OH_IsConsignee = false;
			debtor.OH_IsBroker = false;
			debtor.OH_IsForwarder = false;

			JobMawb masterAWB = Factory.New<JobMawb>();
			JobHeader job = GetInvoiceJob(masterAWB, Invoice);
			job.LocalChargesPK = debtor.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.MasterAWB.Code;
			AssertLines();
		}

		public void TestOSTotalFormatted()
		{
			AssertEquals(0M, ARInvoiceWrapper.TotalOSTaxAmount);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

			AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRateNumerator_ForTestOnly(10);

			line1.AL_AT = taxRate.PK;
			line1.AL_OSTaxAmount = 123.4567m;
			line1.AL_OSExTaxAmount = 1234.56789m;
			line1.AL_OSAmount = 10m;

			line3.AL_AT = taxRate.PK;
			line3.AL_OSTaxAmount = 10m;
			line3.AL_OSExTaxAmount = 100m;
			line3.AL_OSAmount = 15m;

			Invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals(1468.03M, ARInvoiceWrapper.OSTotal);
			AssertEquals("1,468.03", ARInvoiceWrapper.OSTotalFormatted);

			((RefCurrency)ARInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 1;
			AssertEquals("1,468", ARInvoiceWrapper.OSTotalFormatted);
		}

		public void TestShipmentJobType()
		{
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "TEST3";
			debtor.OH_IsDebtor = true;
			debtor.OH_IsConsignor = false;
			debtor.OH_IsConsignee = false;
			debtor.OH_IsBroker = false;
			debtor.OH_IsForwarder = false;

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_UniqueConsignRef = "S0001000";
			JobHeader job = GetInvoiceJob(shipment, Invoice);
			job.LocalChargesPK = debtor.PK;
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			Factory.Save();
			AssertLines();
		}

		public void TestTotalOSQSTAmount()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(0M, aRInvoiceWrapper.TotalOSQSTAmount);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line4 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

			AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRateNumerator_ForTestOnly(5);
			taxRate.SetExtraRate_ForTestOnly(75, 10);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;

			AccTaxRate taxRate2 = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate2.SetRateNumerator_ForTestOnly(5);
			taxRate2.SetExtraRate_ForTestOnly(9975, 1000);
			taxRate2.AT_Type = AccTaxRate.Types.Rated;
			taxRate2.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;

			line1.AL_AT = taxRate.PK;
			line1.AL_OSExTaxAmount = 1234.56789m;
			line1.AL_OSTaxAmount = 12.88m;

			line2.AL_AT = new TestObjectCreator(Factory).GST1.PK;
			line2.AL_OSExTaxAmount = 1200m;
			line2.AL_OSTaxAmount = 120m;

			line3.AL_AT = taxRate.PK;
			line3.AL_OSExTaxAmount = 100m;
			line3.AL_OSTaxAmount = 25.76m;

			line4.AL_AT = taxRate2.PK;
			line4.AL_OSExTaxAmount = 100m;
			line4.AL_OSTaxAmount = 14.98m;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(33.62m, aRInvoiceWrapper.TotalOSQSTAmount);
		}

		public void TestTotalOSEDUAmount()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(0M, aRInvoiceWrapper.TotalOSEDUAmount);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

			AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRateNumerator_ForTestOnly(10);
			taxRate.SetExtraRate_ForTestOnly(3, 1);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;

			line1.AL_AT = taxRate.PK;
			line1.AL_OSExTaxAmount = 100m;
			line1.AL_OSTaxAmount = 10.3m;

			line2.AL_AT = new TestObjectCreator(Factory).GST1.PK;
			line2.AL_OSExTaxAmount = 1200m;
			line2.AL_OSTaxAmount = 120m;

			line3.AL_AT = taxRate.PK;
			line3.AL_OSExTaxAmount = 100m;
			line3.AL_OSTaxAmount = 10.3m;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(0.6m, aRInvoiceWrapper.TotalOSEDUAmount);
			AssertEquals(0.4m, aRInvoiceWrapper.TotalOSEDUPrimaryAmount);
			AssertEquals(0.2m, aRInvoiceWrapper.TotalOSEDUSecondaryAmount);
		}

		public void TestTotalOSQSTAmountFormatted()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(0M, aRInvoiceWrapper.TotalOSQSTAmount);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

			AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRateNumerator_ForTestOnly(5);
			taxRate.SetExtraRate_ForTestOnly(75, 10);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;

			line1.AL_AT = taxRate.PK;
			line1.AL_OSExTaxAmount = 1234.56789m;
			line1.AL_OSTaxAmount = 12.88m;

			line2.AL_AT = new TestObjectCreator(Factory).GST1.PK;
			line2.AL_OSExTaxAmount = 1200m;
			line2.AL_OSTaxAmount = 120m;

			line3.AL_AT = taxRate.PK;
			line3.AL_OSExTaxAmount = 100m;
			line3.AL_OSTaxAmount = 25.76m;

			((RefCurrency)aRInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 1;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(23.64m, aRInvoiceWrapper.TotalOSQSTAmount);
			AssertEquals("24", aRInvoiceWrapper.TotalOSQSTAmountFormatted);
		}

		public void TestTotalOSSBCAndKKCAmountFormatted()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(0M, aRInvoiceWrapper.TotalOSQSTAmount);
				var line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				var line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				var line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

				var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate1.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate1.AT_Type = AccTaxRate.Types.Rated;
				taxRate1.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				taxRate1.SetRateNumerator_ForTestOnly(5);
				taxRate1.SetExtraRate_ForTestOnly(1, 1);

				var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate2.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate2.AT_Type = AccTaxRate.Types.Rated;
				taxRate2.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				taxRate2.SetRateNumerator_ForTestOnly(5);
				taxRate2.SetExtraRate_ForTestOnly(5, 10);

				line1.AL_AT = taxRate1.PK;
				line1.AL_OSExTaxAmount = 1000m;

				line2.AL_AT = taxRate2.PK;
				line2.AL_LineType = TransactionLineTypes.Cost;
				line2.AL_OSExTaxAmount = 2000m;

				line3.AL_AT = taxRate2.PK;
				line3.AL_LineType = TransactionLineTypes.Revenue;
				line3.AL_OSExTaxAmount = 1000m;

				((RefCurrency)aRInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 1;

				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(10m, aRInvoiceWrapper.TotalOSSBCAmount);
				AssertEquals("10", aRInvoiceWrapper.TotalOSSBCAmountFormatted);
				AssertEquals(15m, aRInvoiceWrapper.TotalOSKKCAmount);
				AssertEquals("15", aRInvoiceWrapper.TotalOSKKCAmountFormatted);
			}
		}

		public void TestTotalOSSBCAndKKCAmountFormatted_SER()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(0M, aRInvoiceWrapper.TotalOSQSTAmount);
				var line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				var line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				var line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

				var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate1.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate1.AT_Type = AccTaxRate.Types.ServiceTax;
				taxRate1.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				taxRate1.SetRateNumerator_ForTestOnly(5);
				taxRate1.SetExtraRate_ForTestOnly(1, 1);

				var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate2.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate2.AT_Type = AccTaxRate.Types.ServiceTax;
				taxRate2.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
				taxRate2.SetRateNumerator_ForTestOnly(5);
				taxRate2.SetExtraRate_ForTestOnly(5, 10);

				line1.AL_AT = taxRate1.PK;
				line1.AL_OSExTaxAmount = 1000m;

				line2.AL_AT = taxRate2.PK;
				line2.AL_LineType = TransactionLineTypes.Cost;
				line2.AL_OSExTaxAmount = 2000m;

				line3.AL_AT = taxRate2.PK;
				line3.AL_LineType = TransactionLineTypes.Revenue;
				line3.AL_OSExTaxAmount = 1000m;

				((RefCurrency)aRInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 1;

				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(10m, aRInvoiceWrapper.TotalOSSBCAmount);
				AssertEquals("10", aRInvoiceWrapper.TotalOSSBCAmountFormatted);
				AssertEquals(15m, aRInvoiceWrapper.TotalOSKKCAmount);
				AssertEquals("15", aRInvoiceWrapper.TotalOSKKCAmountFormatted);
			}
		}

		public void TestTotalOSEDUAmountFormatted()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(0M, aRInvoiceWrapper.TotalOSEDUAmount);
				ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

				AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
				taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate.SetRateNumerator_ForTestOnly(10);
				taxRate.SetExtraRate_ForTestOnly(3, 1);
				taxRate.AT_Type = AccTaxRate.Types.Rated;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;

				line1.AL_AT = taxRate.PK;
				line1.AL_OSExTaxAmount = 1000m;
				line1.AL_OSTaxAmount = 103m;

				line2.AL_AT = new TestObjectCreator(Factory).GST1.PK;
				line2.AL_OSExTaxAmount = 1200m;
				line2.AL_OSTaxAmount = 120m;

				line3.AL_AT = taxRate.PK;
				line3.AL_OSExTaxAmount = 2000m;
				line3.AL_OSTaxAmount = 206m;

				((RefCurrency)aRInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 1;

				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(9m, aRInvoiceWrapper.TotalOSEDUAmount);
				AssertEquals("9", aRInvoiceWrapper.TotalOSEDUAmountFormatted);
				AssertEquals("6", aRInvoiceWrapper.TotalOSEDUPrimaryAmountFormatted);
				AssertEquals("3", aRInvoiceWrapper.TotalOSEDUSecondaryAmountFormatted);
			}
		}

		public void TestTotalOSEDUAmountFormatted_SER()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(0M, aRInvoiceWrapper.TotalOSEDUAmount);
				ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

				AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
				taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.India;
				taxRate.SetRateNumerator_ForTestOnly(10);
				taxRate.SetExtraRate_ForTestOnly(3, 1);
				taxRate.AT_Type = AccTaxRate.Types.ServiceTax;
				taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;

				line1.AL_AT = taxRate.PK;
				line1.AL_OSExTaxAmount = 1000m;
				line1.AL_OSTaxAmount = 103m;

				line2.AL_AT = new TestObjectCreator(Factory).GST1.PK;
				line2.AL_OSExTaxAmount = 1200m;
				line2.AL_OSTaxAmount = 120m;

				line3.AL_AT = taxRate.PK;
				line3.AL_OSExTaxAmount = 2000m;
				line3.AL_OSTaxAmount = 206m;

				((RefCurrency)aRInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 1;

				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(9m, aRInvoiceWrapper.TotalOSEDUAmount);
				AssertEquals("9", aRInvoiceWrapper.TotalOSEDUAmountFormatted);
				AssertEquals("6", aRInvoiceWrapper.TotalOSEDUPrimaryAmountFormatted);
				AssertEquals("3", aRInvoiceWrapper.TotalOSEDUSecondaryAmountFormatted);
			}
		}

		public void TestTotalOSSPVAmount()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(0M, aRInvoiceWrapper.TotalOSRETAmount);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

			AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.Italy;
			taxRate.SetRateNumerator_ForTestOnly(22);
			taxRate.SetExtraRate_ForTestOnly(0, 1);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRemittedByCustomer;

			line1.AL_AT = taxRate.PK;
			line1.AL_OSExTaxAmount = 100m;

			line2.AL_AT = new TestObjectCreator(Factory).GST1.PK;
			line2.AL_OSExTaxAmount = 200m;

			line3.AL_AT = taxRate.PK;
			line3.AL_OSExTaxAmount = 300m;

			((RefCurrency)aRInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 1;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(-88m, aRInvoiceWrapper.TotalOSSPVAmount);
			AssertEquals("-88", aRInvoiceWrapper.TotalOSSPVAmountFormatted);
			AssertEquals("88", aRInvoiceWrapper.TotalOSVATExcludeSPVAmountFormatted);
			AssertEquals("708", aRInvoiceWrapper.OSTotalExcludeSPVAmountFormatted);
		}

		public void TestTotalOSRETAmount()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(0M, aRInvoiceWrapper.TotalOSRETAmount);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

			AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRateNumerator_ForTestOnly(5);
			taxRate.SetExtraRate_ForTestOnly(75, 10);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;

			line1.AL_AT = taxRate.PK;
			line1.AL_OSExTaxAmount = 1234.56789m;

			line2.AL_AT = new TestObjectCreator(Factory).GST1.PK;
			line2.AL_OSExTaxAmount = 1200m;

			line3.AL_AT = taxRate.PK;
			line3.AL_OSExTaxAmount = 100m;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(100.09m, aRInvoiceWrapper.TotalOSRETAmount);
		}

		public void TestTotalOSREFAmount()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(0M, aRInvoiceWrapper.TotalOSRETAmount);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

			AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRateNumerator_ForTestOnly(5);
			taxRate.SetExtraRate_ForTestOnly(6, 4);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;

			line1.AL_AT = taxRate.PK;
			line1.AL_OSExTaxAmount = 1234.56789m;

			line2.AL_AT = new TestObjectCreator(Factory).GST1.PK;
			line2.AL_OSExTaxAmount = 1200m;

			line3.AL_AT = taxRate.PK;
			line3.AL_OSExTaxAmount = 100m;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(100.09m, aRInvoiceWrapper.TotalOSRETAmount);
		}

		public void TestTotalOSRETAmountFormatted()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(0M, aRInvoiceWrapper.TotalOSRETAmount);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

			AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRateNumerator_ForTestOnly(5);
			taxRate.SetExtraRate_ForTestOnly(75, 10);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetention;

			line1.AL_AT = taxRate.PK;
			line1.AL_OSExTaxAmount = 1234.56789m;

			line2.AL_AT = new TestObjectCreator(Factory).GST1.PK;
			line2.AL_OSExTaxAmount = 1200m;

			line3.AL_AT = taxRate.PK;
			line3.AL_OSExTaxAmount = 100m;

			((RefCurrency)aRInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 1;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(100.09m, aRInvoiceWrapper.TotalOSRETAmount);
			AssertEquals("100", aRInvoiceWrapper.TotalOSRETAmountFormatted);
		}

		public void TestTotalOSREFAmountFormatted()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(0M, aRInvoiceWrapper.TotalOSRETAmount);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

			AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRateNumerator_ForTestOnly(5);
			taxRate.SetExtraRate_ForTestOnly(6, 4);
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.VATRetentionFraction;

			line1.AL_AT = taxRate.PK;
			line1.AL_OSExTaxAmount = 1234.56789m;

			line2.AL_AT = new TestObjectCreator(Factory).GST1.PK;
			line2.AL_OSExTaxAmount = 1200m;

			line3.AL_AT = taxRate.PK;
			line3.AL_OSExTaxAmount = 100m;

			((RefCurrency)aRInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 1;

			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(100.09m, aRInvoiceWrapper.TotalOSRETAmount);
			AssertEquals("100", aRInvoiceWrapper.TotalOSRETAmountFormatted);
		}

		public void TestTotalOSTaxAmount()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(0M, aRInvoiceWrapper.TotalOSTaxAmount);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			line1.AL_AG = TestObjectCreator.GLHeader1.PK;
			line2.AL_AG = TestObjectCreator.GLHeader1.PK;
			line3.AL_AG = TestObjectCreator.GLHeader1.PK;

			AccTaxRate gst = new TestObjectCreator(Factory).GST1;

			line1.AL_AT = gst.PK;
			line1.AL_OSTaxAmount = 123.4567m;
			line1.AL_OSExTaxAmount = 1234.56789m;

			line2.AL_AT = gst.PK;
			line2.AL_OSTaxAmount = 120m;
			line2.AL_OSExTaxAmount = 1200m;

			line3.AL_AT = gst.PK;
			line3.AL_OSTaxAmount = 10m;
			line3.AL_OSExTaxAmount = 100m;

			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals(253.46m, aRInvoiceWrapper.TotalOSTaxAmount);

			AccTaxRate gstAndQst = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			gstAndQst.AT_Type = AccTaxRate.Types.Rated;
			gstAndQst.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			gstAndQst.SetRateNumerator_ForTestOnly(10);
			gstAndQst.SetExtraRate_ForTestOnly(75, 10);
			Factory.Save();
			line1.AL_AT = line3.AL_AT = gstAndQst.PK;
			line1.AL_OSTaxAmount = 123.4567m;
			line2.AL_OSTaxAmount = 120m;
			line3.AL_OSTaxAmount = 10m;

			aRInvoiceWrapper = RecreateTestingInvoiceDocWrapper(InvoicingBase, (invoice, factory) => DocARInvoice.New(invoice, factory));
			AssertEquals("Should still return GST Only", 193.13M, aRInvoiceWrapper.TotalOSTaxAmount);
		}

		public void TestTotalOSTaxAmountFormatted()
		{
			var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(0M, aRInvoiceWrapper.TotalOSTaxAmount);
			ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
			ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRateNumerator_ForTestOnly(10);

			line1.AL_AT = taxRate.PK;
			line1.AL_OSTaxAmount = 123.4567m;
			line1.AL_OSExTaxAmount = 1234.56789m;

			line3.AL_AT = taxRate.PK;
			line3.AL_OSTaxAmount = 10m;
			line3.AL_OSExTaxAmount = 100m;

			Invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
			AssertEquals(133.46M, aRInvoiceWrapper.TotalOSTaxAmount);
			AssertEquals("133.46", aRInvoiceWrapper.TotalOSTaxAmountFormatted);

			((RefCurrency)aRInvoiceWrapper.Currency.WrappedObject).RX_SubUnitRatio = 1;
			AssertEquals("133", aRInvoiceWrapper.TotalOSTaxAmountFormatted);
		}

		public void TestTotalOSTaxAmountFormatted_WithIndiaGSTTaxes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var aRInvoiceWrapper = (DocARInvoiceCommon)DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(0M, aRInvoiceWrapper.TotalOSTaxAmount);
				ARInvoiceLine line1 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				ARInvoiceLine line2 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();
				ARInvoiceLine line3 = (ARInvoiceLine)InvoicingBase.Lines.AddNew();

				line1.AL_AT = TestObjectCreator.IntegratedGST.PK;
				line1.AL_OSTaxAmount = 123.4567m;
				line1.AL_OSExTaxAmount = 1234.56789m;

				line2.AL_AT = TestObjectCreator.SERANDEDU1.PK;
				line2.AL_OSTaxAmount = 10m;
				line2.AL_OSExTaxAmount = 100m;

				line3.AL_AT = TestObjectCreator.IntegratedGST.PK;
				line3.AL_OSTaxAmount = 10m;
				line3.AL_OSExTaxAmount = 100m;

				Invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				aRInvoiceWrapper = DocARInvoice.New(InvoicingBase, Factory);
				AssertEquals(10M, aRInvoiceWrapper.TotalOSTaxAmount);
				AssertEquals("10.00", aRInvoiceWrapper.TotalOSTaxAmountFormatted);
			}
		}

		#region Test JobTypes

		public void TestWarehouseInwardsOrWarehouseDocketJobType()
		{
			WhsReceive warehouse = Factory.NewWithValidTestData<WhsReceive>(TestBusinessObjectKind.MinimumRequiredToSave);
			JobHeader job = GetInvoiceJob(warehouse, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			job.LocalChargesPK = Invoice.Header.PK;
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.WarehouseInwards.Code;
			AssertLines();
		}

		public void TestWarehouseOutwardsOrWarehouseOrderJobType()
		{
			WhsOrder warehouse = Factory.NewWithValidTestData<WhsOrder>(TestBusinessObjectKind.MinimumRequiredToSave);
			JobHeader job = GetInvoiceJob(warehouse, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			job.LocalChargesPK = Invoice.Header.PK;
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.WarehouseOutwards.Code;
			AssertLines();
		}

		public void TestWarehouseStorageOrWarehouseInvoiceJobType()
		{
			WhsInvoice warehouse = Factory.NewWithValidTestData<WhsInvoice>(TestBusinessObjectKind.MinimumRequiredToSave);
			JobHeader job = GetInvoiceJob(warehouse, Invoice);
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			SetupInvoiceLinesAndCharges();
			SetUpOrganisationForRollUpWithCode("OFD");
			job.LocalChargesPK = Invoice.Header.PK;
			OrgInvoiceRollupOrGroup rollupOrGroup = Invoice.Header.CompanyData.InvoiceRollupOrGroups[0];
			rollupOrGroup.PG_JobType = JobInvoicingConsumerTypes.WarehouseStorage.Code;
			AssertLines(true);
		}

		#endregion

		public new void TestRecipientTaxID()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				OrgHeader header = Factory.New<OrgHeader>();
				header.OH_Code = "ORG1";
				header.LocalBusinessRegNo = "ABC123";
				OrgCusCode taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				taxCode.OK_CodeType = Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.UnitedKingdom);
				taxCode.OK_CustomsRegNo = "123456";

				AssertEquals("PreCondition: Local Business Reg Number", "ABC123", header.LocalBusinessRegNo);

				taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				taxCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
				taxCode.OK_CustomsRegNo = "VAT12345";

				ARInvoice.AH_OH = header.PK;
				InvoicingLineBase line2 = (InvoicingLineBase)ARInvoice.Lines.AddNew();
				AccTaxRate rate = base.Factory.LoadTop1<AccTaxRate>(new ZQuery());
				line2.AL_AT = rate.PK;

				InvoiceWrapper = GetInvoiceWrapper();

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading when not a South African Company", ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number when not a South African Company", ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.SouthAfrica);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a South African Company", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a South African Company", header.LocalBusinessRegNo, InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Morocco);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a Moroccan Company", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a Moroccan Company", header.LocalBusinessRegNo, InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Taiwan);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a Taiwan Company", "Client Tax #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a Taiwan Company", header.LocalVATCode, InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a European Union Company", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a European Union Company", "GB123456", InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Bangladesh);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a Bangladesh Company", "Client BIN #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax Id Number for a Bangladesh Company", header.LocalVATCode, InvoiceWrapper.RecipientTaxIDNumber);

				//Philippines

				header = Factory.New<OrgHeader>();
				header.OH_Code = "ORGPH";
				taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = "PH";
				taxCode.OK_CodeType = Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.Philippines);
				taxCode.OK_CustomsRegNo = "789012";

				ARInvoice.AH_OH = header.PK;
				line2 = (InvoicingLineBase)ARInvoice.Lines.AddNew();
				line2.AL_AT = rate.PK;

				InvoiceWrapper = GetInvoiceWrapper();

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Philippines);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a Philippines Company", "TIN:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a Philippines Company", "789012", InvoiceWrapper.RecipientTaxIDNumber);

				// Sri Lanka

				string vATRegNumber = "abc123456";
				header = Factory.New<OrgHeader>();
				header.OH_Code = "ABC862";
				taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SriLanka;
				taxCode.OK_CodeType = Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.SriLanka);
				taxCode.OK_CustomsRegNo = vATRegNumber;

				ARInvoice.AH_OH = header.PK;
				line2 = (InvoicingLineBase)ARInvoice.Lines.AddNew();
				line2.AL_AT = rate.PK;

				InvoiceBatch = Factory.New<InvoiceBatchHeader>();
				InvoiceBatch.AH_OH = header.PK;
				InvoiceBatch.Line.Add(InvoicingBase);

				InvoiceWrapper = GetInvoiceWrapper();

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.SriLanka);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a Sri Lanka Company", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a Sri Lanka Company", vATRegNumber, InvoiceWrapper.RecipientTaxIDNumber);

				//India
				string gstRegNumber = "GST1234";
				header = Factory.New<OrgHeader>();
				header.OH_Code = "OH123";
				header.OH_RL_NKClosestPort = "INBOM";
				taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;
				taxCode.OK_CodeType = OrgCusCode.CodeTypes.GSTCode;
				taxCode.OK_CustomsRegNo = gstRegNumber;

				ARInvoice.AH_OH = header.PK;
				line2 = (InvoicingLineBase)ARInvoice.Lines.AddNew();
				line2.AL_AT = rate.PK;

				InvoiceBatch = Factory.New<InvoiceBatchHeader>();
				InvoiceBatch.AH_OH = header.PK;
				InvoiceBatch.Line.Add(InvoicingBase);

				InvoiceWrapper = GetInvoiceWrapper();

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a India Company", "Client GSTIN #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a India Company", gstRegNumber, InvoiceWrapper.RecipientTaxIDNumber);

				string uinRegNumber = "UIN1234";
				var taxCode1 = header.CustomsCodes.AddNew();
				taxCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;
				taxCode1.OK_CodeType = "UIN";
				taxCode1.OK_CustomsRegNo = uinRegNumber;

				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a India Company - if both GST and UIN is present still the system should retrun GST as RecipientTaxID", "Client GSTIN #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Heading for a India Company - if both GST and UIN is present still the system should retrun GST as RecipientTaxID", gstRegNumber, InvoiceWrapper.RecipientTaxIDNumber);

				taxCode.Delete(); //deleting GST cuscode for India

				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a India Company - in absence of GST, UIN should be retruned as RecipientTaxID", "Client UIN #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Heading for a India Company - in absence of GST, UIN should be retruned as RecipientTaxID", uinRegNumber, InvoiceWrapper.RecipientTaxIDNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public new void TestPapuaNewGuineaRecipientTaxIDBehaviours()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = "ORG1";
			header.LocalBusinessRegNo = "ABC123";
			OrgCusCode taxCode = header.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
			taxCode.OK_CodeType = Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.UnitedKingdom);
			taxCode.OK_CustomsRegNo = "123456";

			AssertEquals("PreCondition: Local Business Reg Number", "ABC123", header.LocalBusinessRegNo);

			taxCode = header.CustomsCodes.AddNew();
			taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			taxCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			taxCode.OK_CustomsRegNo = "VAT12345";

			ARInvoice.AH_OH = header.PK;
			InvoicingLineBase line2 = (InvoicingLineBase)ARInvoice.Lines.AddNew();
			AccTaxRate rate = base.Factory.LoadTop1<AccTaxRate>(new ZQuery());
			line2.AL_AT = rate.PK;

			InvoiceWrapper = GetInvoiceWrapper();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PapuaNewGuinea);
			using (AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a Papua New Guinea Company should be empty by default", ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax Id Number for a Papua New Guinea Company should be empty by default", ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);
			}

			using (AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("Recipient Tax ID Heading for a Papua New Guinea Company", "Client TIN #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax Id Number for a Papua New Guinea Company", header.LocalVATCode, InvoiceWrapper.RecipientTaxIDNumber);
			}
		}

		public new void TestRecipientTaxID_LoginCountryNotEqualRecipientCountryOfRegistration()
		{
			var storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				var header = Factory.New<OrgHeader>();
				header.OH_RL_NKClosestPort = "GBLON";
				header.CustomsCodes.RemoveAll();

				var taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedKingdom;
				taxCode.OK_CodeType = Country.GetConsumptionTaxDescription(Core.Constants.CountryCodes.UnitedKingdom);
				taxCode.OK_CustomsRegNo = "123456";

				ARInvoice.AH_OH = header.PK;
				InvoicingLineBase line2 = (InvoicingLineBase)ARInvoice.Lines.AddNew();
				AccTaxRate rate = base.Factory.LoadTop1<AccTaxRate>(new ZQuery());
				line2.AL_AT = rate.PK;

				InvoiceWrapper = GetInvoiceWrapper();

				//In the Same Country
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AccountingConfigurationRegistry.Instance.DisplayRecipientTaxID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				AssertEquals("Recipient Tax ID Heading for a United Kingdom Company", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a United Kingdom Company", "GB123456", InvoiceWrapper.RecipientTaxIDNumber);

				//In Another ENU
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Germany);
				header.ResetCodeForTaxRegistration_ForTestOnly();

				AssertEquals("Recipient Tax ID Heading for a German Company", "Client VAT ID No:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for another ENU Company", "GB123456", InvoiceWrapper.RecipientTaxIDNumber);

				//Out of ENU
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
				header.ResetCodeForTaxRegistration_ForTestOnly();

				AssertEquals("Recipient Tax ID Heading for a New Zealand Company", "Client GST #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Should be empty because New Zealand is not in ENU", ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public new void TestRecipientTaxIDWithoutTax()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				OrgHeader header = Factory.New<OrgHeader>();
				header.OH_Code = "ORG1";
				header.MiscServ.OM_ARDontShowTaxOnDocs = ZBool.True;

				OrgCusCode taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Netherlands;
				taxCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Netherlands);
				taxCode.OK_CustomsRegNo = "123456";

				taxCode = header.CustomsCodes.AddNew();
				taxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				taxCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(Core.Constants.CountryCodes.Australia);
				taxCode.OK_CustomsRegNo = "654321";

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Netherlands);
				AssertEquals("PreCondition: Local VAT Code", "123456", header.RawTaxRegistrationNumber);
				AssertEquals("PreCondition: is not taxed", ZBool.True, header.MiscServ.OM_ARDontShowTaxOnDocs);

				ARInvoice.AH_OH = header.PK;

				InvoicingLineBase line2 = (InvoicingLineBase)ARInvoice.Lines.AddNew();
				AccTaxRate rate = base.Factory.LoadTop1<AccTaxRate>(new ZQuery());
				line2.AL_AT = rate.PK;

				InvoiceWrapper = GetInvoiceWrapper();

				AssertEquals("Recipient Tax ID Heading when Current Company is Netherlands", "Client VAT #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number when Current Company is Netherlands", "NL123456", InvoiceWrapper.RecipientTaxIDNumber);

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
				header.ResetCodeForTaxRegistration_ForTestOnly();
				AssertEquals("654321", header.RawTaxRegistrationNumber);
				AssertEquals("Recipient Tax ID Heading when Current Company is Australia", ZString.Empty, InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number when Current Company is Australia", ZString.Empty, InvoiceWrapper.RecipientTaxIDNumber);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public void TestRecipientTaxID_DJ()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Djibouti))
			{
				var header = Factory.New<OrgHeader>();
				header.OH_Code = "ORG1";
				header.OH_RL_NKClosestPort = "DJAII";

				var niftaxCode = header.CustomsCodes.AddNew();
				niftaxCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Djibouti;
				niftaxCode.OK_CodeType = "NIF";
				niftaxCode.OK_CustomsRegNo = "NIF1234";

				InvoicingLineBase line2 = (InvoicingLineBase)ARInvoice.Lines.AddNew();
				AccTaxRate rate = base.Factory.LoadTop1<AccTaxRate>(new ZQuery());
				line2.AL_AT = rate.PK;

				ARInvoice.AH_OH = header.PK;
				InvoiceWrapper = GetInvoiceWrapper();
				AssertEquals("Recipient Tax ID Heading for a Taiwan Company", "Client NIF #:", InvoiceWrapper.RecipientTaxIDHeading);
				AssertEquals("Recipient Tax ID Number for a Taiwan Company", "NIF1234", InvoiceWrapper.RecipientTaxIDNumber);
			}
		}

		public void TestDocumentTitle_ProForma()
		{
			ARInvoice previewInvoice = Factory.New<ARInvoice>();
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.CreditNote;
			DocARInvoice previewARInvoiceWrapper = DocARInvoice.New(previewInvoice, Factory);
			AssertEquals("Document Title", "PRO FORMA INVOICE", previewARInvoiceWrapper.DocumentTitle);
		}

		public void TestDocumentTitle_CreditNote_Disbursement()
		{
			ARCreditNote previewInvoice = Factory.New<ARCreditNote>();

			previewInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			DocARInvoice previewARInvoiceWrapper = DocARInvoice.New(previewInvoice, Factory);
			AssertEquals("Document Title", "PRO FORMA CREDIT NOTE", previewARInvoiceWrapper.DocumentTitle);

			previewInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Factory.Save();
			previewARInvoiceWrapper = DocARInvoice.New(previewInvoice, Factory);
			AssertEquals("Document Title", "CREDIT NOTE", previewARInvoiceWrapper.DocumentTitle);

			previewInvoice = Factory.New<ARCreditNote>();
			var line = (AccTransactionLines)previewInvoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;

			previewInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;

			previewARInvoiceWrapper = DocARInvoice.New(previewInvoice, Factory);
			AssertEquals("Document Title", "PRO FORMA TAX CREDIT NOTE", previewARInvoiceWrapper.DocumentTitle);

			previewInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			Factory.Save();
			previewARInvoiceWrapper = DocARInvoice.New(previewInvoice, Factory);
			AssertEquals("Document Title", "TAX CREDIT NOTE", previewARInvoiceWrapper.DocumentTitle);
		}

		public void TestIsIcelandicAndStandardInvoice()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);

			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_Code = "TEST";
			debtor.OH_RL_NKClosestPort = "ISREY";
			debtor.OH_IsDebtor = true;
			Invoice.AH_OH = debtor.PK;
			InvoiceWrapper = (DocARInvoice)GetBaseInvoiceWrapper();

			AssertEquals("Should be Icelandic Invoice", true, InvoiceWrapper.IsIcelandicInvoice);
			AssertEquals("Should not be Standard Invoice", false, InvoiceWrapper.PrintStandard);

			debtor.OH_RL_NKClosestPort = "AUBNE";

			AssertEquals("Should not be Icelandic Invoice", false, InvoiceWrapper.IsIcelandicInvoice);
			AssertEquals("Should be Standard Invoice", true, InvoiceWrapper.PrintStandard);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.HongKong);

			AssertEquals("Should not be Icelandic Invoice", false, InvoiceWrapper.IsIcelandicInvoice);
			AssertEquals("Should be Standard Invoice", true, InvoiceWrapper.PrintStandard);

			debtor.OH_RL_NKClosestPort = "ISREY";

			AssertEquals("Should not be Icelandic Invoice", false, InvoiceWrapper.IsIcelandicInvoice);
			AssertEquals("Should be Standard Invoice", true, InvoiceWrapper.PrintStandard);
		}

		public void TestPrintOrderNumbersFromRelatedShipments()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			JobHeader job = GetInvoiceJob(shipment, Invoice);

			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "DEBTORORG";
			Invoice.AH_OH = debtor.PK;

			InvoiceWrapper = GetInvoiceWrapper();

			debtor.CompanyData.OB_ARBuyersConsolInvoicingStyle = Enterprise.Core.Constants.ConsolInvoicingStyles.Master;
			AssertEquals("True for ConsolInvoicingStyles.Master", true, InvoiceWrapper.PrintOrderNumbersFromRelatedShipments);

			debtor.CompanyData.OB_ARBuyersConsolInvoicingStyle = Enterprise.Core.Constants.ConsolInvoicingStyles.ApportionInvoiceMaster;
			AssertEquals("True for ConsolInvoicingStyles.ApportionInvoiceMaster", true, InvoiceWrapper.PrintOrderNumbersFromRelatedShipments);

			debtor.CompanyData.OB_ARBuyersConsolInvoicingStyle = Enterprise.Core.Constants.ConsolInvoicingStyles.Apportion;
			AssertEquals("False for ConsolInvoicingStyles.Apportion", false, InvoiceWrapper.PrintOrderNumbersFromRelatedShipments);

			debtor.CompanyData.OB_ARBuyersConsolInvoicingStyle = "DEF";
			OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ConsolInvoicingStyles.Master);
			AssertEquals("True for DEFault from registry when registry is ConsolInvoicingStyles.Master", true, InvoiceWrapper.PrintOrderNumbersFromRelatedShipments);

			OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ConsolInvoicingStyles.ApportionInvoiceMaster);
			AssertEquals("True for DEFault from registry when registry is ConsolInvoicingStyles.ApportionInvoiceMaster", true, InvoiceWrapper.PrintOrderNumbersFromRelatedShipments);

			OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ConsolInvoicingStyles.Apportion);
			AssertEquals("False for DEFault from registry when registry is ConsolInvoicingStyles.Apportion", false, InvoiceWrapper.PrintOrderNumbersFromRelatedShipments);

			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			debtor.CompanyData.OB_ARBuyersConsolInvoicingStyle = Enterprise.Core.Constants.ConsolInvoicingStyles.Master;
			AssertEquals("False when shipment is not a buyers consol lead", false, InvoiceWrapper.PrintOrderNumbersFromRelatedShipments);

			debtor.CompanyData.OB_ARBuyersConsolInvoicingStyle = Enterprise.Core.Constants.ConsolInvoicingStyles.ApportionInvoiceMaster;
			AssertEquals("False when shipment is not a buyers consol lead", false, InvoiceWrapper.PrintOrderNumbersFromRelatedShipments);

			debtor.CompanyData.OB_ARBuyersConsolInvoicingStyle = Enterprise.Core.Constants.ConsolInvoicingStyles.Apportion;
			AssertEquals("False when shipment is not a buyers consol lead", false, InvoiceWrapper.PrintOrderNumbersFromRelatedShipments);
		}

		#region Periodic Invoices

		public void TestInvoiceCopyMessage()
		{
			AccountingConfigurationRegistry.Instance.InvoiceTradingTerms.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, null);

			InvoiceWrapper = GetInvoiceWrapper();
			AssertEquals(string.Empty, InvoiceWrapper.InvoiceCopyMessage);

			var collection = (InvoiceCopyCollection)AccountingConfigurationRegistry.Instance.InvoiceCopies.Value.Clone(null, null);

			foreach (InvoiceCopy entry in collection)
			{
				if (entry.IsOriginal)
				{
					entry.Message = "New Message to print";
				}
			}

			AccountingConfigurationRegistry.Instance.InvoiceCopies.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			InvoiceWrapper = GetInvoiceWrapper();
			AssertEquals("New Message to print", InvoiceWrapper.InvoiceCopyMessage);
		}

		public void TestLayout()
		{
			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.Debtor.CompanyData, JobInvoicingConsumerTypes.Brokerage.Code, "ALL", "ALL", ZString.Empty, "INV", "CHG");
			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.Debtor.CompanyData, JobInvoicingConsumerTypes.Shipment.Code, "ALL", "ALL", ZString.Empty, "CHG", "INV");
			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.Debtor.CompanyData, JobInvoicingConsumerTypes.AgencyBooking.Code, "ALL", "ALL", ZString.Empty, "NON", "INV");
			Factory.Save();

			var brkInvoice = CreateJobAndPeriodicInvoice(TestObjectCreator.Debtor, TestObjectCreator.Agent, "T000001", new Tuple<JobInvoicingConsumerType, AccChargeCode>(JobInvoicingConsumerTypes.Brokerage, TestObjectCreator.CC1));
			var shpInvoice = CreateJobAndPeriodicInvoice(TestObjectCreator.Debtor, TestObjectCreator.Agent, "T000002", new Tuple<JobInvoicingConsumerType, AccChargeCode>(JobInvoicingConsumerTypes.Shipment, TestObjectCreator.CC2));
			var cllInvoice = CreateJobAndPeriodicInvoice(TestObjectCreator.Debtor, TestObjectCreator.Agent, "T000003", new Tuple<JobInvoicingConsumerType, AccChargeCode>(JobInvoicingConsumerTypes.CFSLoadList, TestObjectCreator.CC2));
			var agbInvoice = CreateJobAndPeriodicInvoice(TestObjectCreator.Debtor, TestObjectCreator.Agent, "T000004", new Tuple<JobInvoicingConsumerType, AccChargeCode>(JobInvoicingConsumerTypes.AgencyBooking, TestObjectCreator.CC2));

			AssertEquals("Is Periodic Invoice", true, brkInvoice.IsPeriodicInvoice);
			var previewARInvoiceWrapper = DocARInvoice.New(brkInvoice, Factory);
			AssertEquals("HasLinesWithCHGSecondaryLayout:BRK", true, previewARInvoiceWrapper.HasLinesWithCHGSecondaryLayout);
			AssertEquals("HasLinesWithINVSecondaryLayout:BRK", false, previewARInvoiceWrapper.HasLinesWithINVSecondaryLayout);
			AssertEquals("HasLinesWithNONLayout:BRK", false, previewARInvoiceWrapper.HasLinesWithNONLayout);
			AssertEquals("HasLinesWithINVLayout:BRK", true, previewARInvoiceWrapper.HasLinesWithINVLayout);
			AssertEquals("HasLinesWithCHGLayout:BRK", false, previewARInvoiceWrapper.HasLinesWithCHGLayout);

			AssertEquals("Is Periodic Invoice", true, shpInvoice.IsPeriodicInvoice);
			previewARInvoiceWrapper = DocARInvoice.New(shpInvoice, Factory);
			AssertEquals("HasLinesWithCHGSecondaryLayout:SHP", false, previewARInvoiceWrapper.HasLinesWithCHGSecondaryLayout);
			AssertEquals("HasLinesWithINVSecondaryLayout:SHP", true, previewARInvoiceWrapper.HasLinesWithINVSecondaryLayout);
			AssertEquals("HasLinesWithNONLayout:BRK", false, previewARInvoiceWrapper.HasLinesWithNONLayout);
			AssertEquals("HasLinesWithINVLayout:SHP", false, previewARInvoiceWrapper.HasLinesWithINVLayout);
			AssertEquals("HasLinesWithCHGLayout:SHP", true, previewARInvoiceWrapper.HasLinesWithCHGLayout);

			AssertEquals("Is Periodic Invoice", true, cllInvoice.IsPeriodicInvoice);
			previewARInvoiceWrapper = DocARInvoice.New(cllInvoice, Factory);
			AssertEquals("HasLinesWithCHGSecondaryLayout:CLL", true, previewARInvoiceWrapper.HasLinesWithCHGSecondaryLayout);
			AssertEquals("HasLinesWithINVSecondaryLayout:CLL", false, previewARInvoiceWrapper.HasLinesWithINVSecondaryLayout);
			AssertEquals("HasLinesWithNONLayout:BRK", false, previewARInvoiceWrapper.HasLinesWithNONLayout);
			AssertEquals("HasLinesWithINVLayout:CLL", false, previewARInvoiceWrapper.HasLinesWithINVLayout);
			AssertEquals("HasLinesWithCHGLayout:CLL", true, previewARInvoiceWrapper.HasLinesWithCHGLayout);

			AssertEquals("Is Periodic Invoice", true, agbInvoice.IsPeriodicInvoice);
			previewARInvoiceWrapper = DocARInvoice.New(agbInvoice, Factory);
			AssertEquals("HasLinesWithCHGSecondaryLayout:AGB", false, previewARInvoiceWrapper.HasLinesWithCHGSecondaryLayout);
			AssertEquals("HasLinesWithINVSecondaryLayout:AGB", true, previewARInvoiceWrapper.HasLinesWithINVSecondaryLayout);
			AssertEquals("HasLinesWithNONLayout:AGB", true, previewARInvoiceWrapper.HasLinesWithNONLayout);
			AssertEquals("HasLinesWithINVLayout:AGB", false, previewARInvoiceWrapper.HasLinesWithINVLayout);
			AssertEquals("HasLinesWithCHGLayout:AGB", false, previewARInvoiceWrapper.HasLinesWithCHGLayout);
		}

		public void TestDetentionDemurrageStatements()
		{
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Default value", string.Empty, InvoiceWrapper.DetentionDemurrageStatements);
			AccountingConfigurationRegistry.Instance.InvoiceDetentionDemurrageStatements.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "new Statements");
			AssertEquals("new Statements", InvoiceWrapper.DetentionDemurrageStatements);
		}

		public void TestPrintTaxDetailPERRIISLX()
		{
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Default value", true, InvoiceWrapper.PrintTaxDetailPERRIISLX);
			AccountingConfigurationRegistry.Instance.PrintTaxDetailPERRIISLX.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, InvoiceWrapper.PrintTaxDetailPERRIISLX);
		}

		public void TestPrintTaxDetailTRX()
		{
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Default value", true, InvoiceWrapper.PrintTaxDetailTRX);
			AccountingConfigurationRegistry.Instance.PrintTaxDetailTRX.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, InvoiceWrapper.PrintTaxDetailTRX);
		}

		public void TestPrintTaxDetailSPR()
		{
			InvoiceWrapper = DocARInvoice.New(Invoice, Factory);
			AssertEquals("Default value", false, InvoiceWrapper.PrintTaxDetailSPR);
			AccountingConfigurationRegistry.Instance.PrintTaxDetailSPR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, InvoiceWrapper.PrintTaxDetailSPR);
		}

		public void TestInvoiceLineByChargeJobAndNON()
		{
			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.Debtor.CompanyData, JobInvoicingConsumerTypes.Brokerage.Code, "ALL", "ALL", ZString.Empty, "INV", "CHG");
			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.Debtor.CompanyData, JobInvoicingConsumerTypes.Shipment.Code, "ALL", "ALL", ZString.Empty, "CHG", "INV");
			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.Debtor.CompanyData, JobInvoicingConsumerTypes.AgencyBooking.Code, "ALL", "ALL", ZString.Empty, "NON", "INV");
			Factory.Save();

			var tuple1 = new Tuple<JobInvoicingConsumerType, AccChargeCode>(JobInvoicingConsumerTypes.Shipment, TestObjectCreator.CC2);
			var tuple2 = new Tuple<JobInvoicingConsumerType, AccChargeCode>(JobInvoicingConsumerTypes.Brokerage, TestObjectCreator.CC1);
			var tuple3 = new Tuple<JobInvoicingConsumerType, AccChargeCode>(JobInvoicingConsumerTypes.CFSLoadList, TestObjectCreator.CC3);
			var tuple4 = new Tuple<JobInvoicingConsumerType, AccChargeCode>(JobInvoicingConsumerTypes.AgencyBooking, TestObjectCreator.CC2);

			var invoice = CreateJobAndPeriodicInvoice(TestObjectCreator.Debtor, TestObjectCreator.Agent, "T000002", tuple1, tuple2, tuple3, tuple4);
			var previewARInvoiceWrapper = DocARInvoice.New(invoice, Factory);
			AssertEquals("Invoice Line Count", 4, previewARInvoiceWrapper.InvoiceLine.Count);
			AssertEquals("InvoiceLineByCharge Count", 2, previewARInvoiceWrapper.InvoiceLineByCharge.Count);
			AssertEquals("InvoiceLineByCharge Code", TestObjectCreator.CC2.AC_Code, previewARInvoiceWrapper.InvoiceLineByCharge[0].ChargeCode.Code);
			AssertEquals("InvoiceLineByCharge Code", TestObjectCreator.CC3.AC_Code, previewARInvoiceWrapper.InvoiceLineByCharge[1].ChargeCode.Code);

			AssertEquals("InvoiceLineByJob Count", 1, previewARInvoiceWrapper.InvoiceLineByJob.Count);
			AssertEquals("InvoiceLineByJob Code", invoice.Lines[1].Job.JH_JobNum, previewARInvoiceWrapper.InvoiceLineByJob[0].JobNumber);

			AssertEquals("InvoiceLineByNON Count", 1, previewARInvoiceWrapper.InvoiceLineByNON.Count);
			AssertEquals("InvoiceLineByNON Code", TestObjectCreator.CC2.AC_Code, previewARInvoiceWrapper.InvoiceLineByNON[0].ChargeCode.Code);
			AssertEquals("InvoiceLineByNON Code", false, previewARInvoiceWrapper.InvoiceLineByNON[0].IsRollUpLine);
			AssertEquals("InvoiceLineByNON Code", false, previewARInvoiceWrapper.InvoiceLineByNON[0].IsSubTotalLine);

			AssertEquals("LinesForInvoice Count", 4, previewARInvoiceWrapper.LinesForInvoice.Count);
			AssertContainsExactElementsInAnyOrder(previewARInvoiceWrapper.PeriodicInvoiceLinesForDisplay, previewARInvoiceWrapper.LinesForInvoice);
			AssertEquals("PeriodicInvoiceLinesForDisplay Count", 4, previewARInvoiceWrapper.PeriodicInvoiceLinesForDisplay.Count);
		}

		public override void TestInvoiceLineByNON()
		{
			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.Debtor.CompanyData, JobInvoicingConsumerTypes.Brokerage.Code, "ALL", "ALL", ZString.Empty, "INV", "CHG");
			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.Debtor.CompanyData, JobInvoicingConsumerTypes.Shipment.Code, "ALL", "ALL", ZString.Empty, "NON", "INV");
			Factory.Save();

			//Periodic Invoice without any InvoiceLineByNON
			var brkTuple = new Tuple<JobInvoicingConsumerType, AccChargeCode>(JobInvoicingConsumerTypes.Brokerage, TestObjectCreator.CC1);
			var brkInvoice = CreateJobAndPeriodicInvoice(TestObjectCreator.Debtor, TestObjectCreator.Agent, "brk000001", brkTuple);
			var previewARInvoiceWrapper = DocARInvoice.New(brkInvoice, Factory);
			AssertEquals("Invoice Line Count", 1, previewARInvoiceWrapper.InvoiceLine.Count);
			AssertEquals("IsPeriodic Invoice", true, previewARInvoiceWrapper.IsPeriodicInvoice);
			AssertEquals("InvoiceLineByNON count", 0, previewARInvoiceWrapper.InvoiceLineByNON.Count);
			AssertEquals("LinesForInvoice count", 1, previewARInvoiceWrapper.LinesForInvoice.Count);
			AssertEquals("PeriodicInvoiceLinesForDisplay Count", 1, previewARInvoiceWrapper.PeriodicInvoiceLinesForDisplay.Count);

			//Periodic Invoice with one InvoiceLineByNON
			var shpTuple = new Tuple<JobInvoicingConsumerType, AccChargeCode>(JobInvoicingConsumerTypes.Shipment, TestObjectCreator.CC2);
			var shpInvoice = CreateJobAndPeriodicInvoice(TestObjectCreator.Debtor, TestObjectCreator.Agent, "shp000001", shpTuple);
			previewARInvoiceWrapper = DocARInvoice.New(shpInvoice, Factory);
			AssertEquals("Invoice Line Count", 1, previewARInvoiceWrapper.InvoiceLine.Count);
			AssertEquals("IsPeriodic Invoice", true, previewARInvoiceWrapper.IsPeriodicInvoice);
			AssertEquals("InvoiceLineByNON count", 1, previewARInvoiceWrapper.InvoiceLineByNON.Count);
			AssertEquals("LinesForInvoice count", 1, previewARInvoiceWrapper.LinesForInvoice.Count);
			AssertEquals("PeriodicInvoiceLinesForDisplay Count", 1, previewARInvoiceWrapper.PeriodicInvoiceLinesForDisplay.Count);

			//Non Periodic Invoice
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			var line = (InvoiceLine)invoice.Lines.AddNew();
			previewARInvoiceWrapper = DocARInvoice.New(invoice, Factory);
			AssertEquals("IsPeriodic Invoice", false, previewARInvoiceWrapper.IsPeriodicInvoice);
			AssertEquals("InvoiceLineByNON count", 0, previewARInvoiceWrapper.InvoiceLineByNON.Count);
			AssertEquals("LinesForInvoice count", 1, previewARInvoiceWrapper.LinesForInvoice.Count);
		}

		public void TestInvoiceLineByNONIsSortedWhenGroupOrSubtotalIsAlphabetical()
		{
			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.Debtor.CompanyData, JobInvoicingConsumerTypes.Shipment.Code, "ALL", "ALL", ZString.Empty, "NON", "INV");
			TestObjectCreator.CreateOrgInvoiceRollupOrGroup(TestObjectCreator.Debtor,
				jobType: "ALL",
				transportMode: "ALL",
				serviceDirection: "ALL",
				groupOrSubTotal: "ALP",
				groupOrSubtotalStyle: "NOG");

			Factory.Save();

			//Periodic Invoice with one InvoiceLineByNON
			var shpTuple = new[] {
				(JobInvoicingConsumerTypes.Shipment, new AccChargeCode[] { TestObjectCreator.CC2, TestObjectCreator.CC1, TestObjectCreator.CC3, TestObjectCreator.FRT })
			};
			var shpInvoice = CreateJobAndPeriodicInvoice(TestObjectCreator.Debtor, TestObjectCreator.Agent, "shp000001", shpTuple);
			var previewARInvoiceWrapper = DocARInvoice.New(shpInvoice, Factory);
			AssertEquals("Invoice Line Count", 4, previewARInvoiceWrapper.InvoiceLine.Count);
			AssertEquals("IsPeriodic Invoice", true, previewARInvoiceWrapper.IsPeriodicInvoice);
			AssertEquals("InvoiceLineByNON count", 4, previewARInvoiceWrapper.InvoiceLineByNON.Count);

			AssertEquals("FRT", TestObjectCreator.FRT.AC_Code, previewARInvoiceWrapper.InvoiceLineByNON[0].ChargeCode.Code);
			AssertEquals("CC1", TestObjectCreator.CC1.AC_Code, previewARInvoiceWrapper.InvoiceLineByNON[1].ChargeCode.Code);
			AssertEquals("CC2", TestObjectCreator.CC2.AC_Code, previewARInvoiceWrapper.InvoiceLineByNON[2].ChargeCode.Code);
			AssertEquals("CC3", TestObjectCreator.CC3.AC_Code, previewARInvoiceWrapper.InvoiceLineByNON[3].ChargeCode.Code);
		}

		public void TestInvoiceLineByNONIsSortedWhenGroupOrSubtotalIsSequence()
		{
			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.Debtor.CompanyData, JobInvoicingConsumerTypes.Shipment.Code, "ALL", "ALL", ZString.Empty, "NON", "INV");
			TestObjectCreator.CreateOrgInvoiceRollupOrGroup(TestObjectCreator.Debtor,
				jobType: "ALL",
				transportMode: "ALL",
				serviceDirection: "ALL",
				groupOrSubTotal: "SEQ",
				groupOrSubtotalStyle: "NOG");

			TestObjectCreator.CC2.AC_PrintSequence = 1;
			TestObjectCreator.CC3.AC_PrintSequence = 2;
			TestObjectCreator.CC1.AC_PrintSequence = 3;
			TestObjectCreator.FRT.AC_PrintSequence = 4;

			Factory.Save();

			//Periodic Invoice with one InvoiceLineByNON
			var shpTuple = new[] {
				(JobInvoicingConsumerTypes.Shipment, new AccChargeCode[] { TestObjectCreator.CC2, TestObjectCreator.CC1, TestObjectCreator.CC3, TestObjectCreator.FRT })
			};
			var shpInvoice = CreateJobAndPeriodicInvoice(TestObjectCreator.Debtor, TestObjectCreator.Agent, "shp000001", shpTuple);
			var previewARInvoiceWrapper = DocARInvoice.New(shpInvoice, Factory);
			AssertEquals("Invoice Line Count", 4, previewARInvoiceWrapper.InvoiceLine.Count);
			AssertEquals("IsPeriodic Invoice", true, previewARInvoiceWrapper.IsPeriodicInvoice);
			AssertEquals("InvoiceLineByNON count", 4, previewARInvoiceWrapper.InvoiceLineByNON.Count);

			AssertEquals("FRT", TestObjectCreator.CC2.AC_Code, previewARInvoiceWrapper.InvoiceLineByNON[0].ChargeCode.Code);
			AssertEquals("CC1", TestObjectCreator.CC3.AC_Code, previewARInvoiceWrapper.InvoiceLineByNON[1].ChargeCode.Code);
			AssertEquals("CC2", TestObjectCreator.CC1.AC_Code, previewARInvoiceWrapper.InvoiceLineByNON[2].ChargeCode.Code);
			AssertEquals("CC3", TestObjectCreator.FRT.AC_Code, previewARInvoiceWrapper.InvoiceLineByNON[3].ChargeCode.Code);
		}

		public override void TestEnglishTaxMessages()
		{
			SetupCountryForTestingInvoiceTaxMessages();

			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "USLAX";
			TestObjectCreator.CreateOrgInvoiceType(TestObjectCreator.Debtor.CompanyData, "ALL", "ALL", "ALL", ZString.Empty, "NON", "INV");
			var rollupGroupSetup = TestObjectCreator.CreateOrgInvoiceRollupOrGroup(TestObjectCreator.Debtor,
				jobType: "ALL",
				transportMode: "ALL",
				serviceDirection: "ALL",
				groupOrSubTotal: "ALP",
				groupOrSubtotalStyle: "NOG");
			Factory.Save();

			foreach (var isPeriodicInvoice in new[] { false, true })
			{
				InvoicingBase invoiceBase = SetupForTestingInvoiceTaxMessagesCore(TestObjectCreator.Debtor);

				if (isPeriodicInvoice)
				{
					invoiceBase.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;
				}

				SetupForTestingInvoiceTaxMessages(invoiceBase);

				Factory.Save();

				foreach (var groupOrSubTotal in new[] { "DEF", "ALP", "SEQ" })
				{
					rollupGroupSetup.PG_GroupOrSubTotal = groupOrSubTotal;
					Factory.Save();

					IEnumerable<InvoiceLine> orderdLines = null;
					switch (groupOrSubTotal)
					{
						case "ALP":
							orderdLines = invoiceBase.Lines.OfType<InvoiceLine>().OrderBy(l => l.ChargeCode?.AC_Code);
							break;
						case "SEQ":
							orderdLines = invoiceBase.Lines.OfType<InvoiceLine>().OrderBy(l => l.ChargeCode?.AC_PrintSequence);
							break;
						default:
							orderdLines = invoiceBase.Lines.OfType<InvoiceLine>();
							break;
					}

					ZString expectedString = FormattableString.Invariant($@"* {orderdLines.First().VATClass.A9_EnglishMsg}
** {orderdLines.Skip(1).First().VATClass.A9_EnglishMsg}
*** {orderdLines.Skip(2).First().VATClass.A9_EnglishMsg}
**** {orderdLines.Skip(3).First().VATClass.A9_EnglishMsg}
***** {orderdLines.Skip(4).First().VATClass.A9_EnglishMsg}");
					AssertEquals(FormattableString.Invariant($"Category: {invoiceBase.AH_TransactionCategory} - GroupOrSubTotal: {groupOrSubTotal}"), expectedString, GetBaseInvoiceWrapper().EnglishLanguageTaxMessages);

					expectedString = FormattableString.Invariant($@"1. {orderdLines.First().VATClass.A9_EnglishMsg}
2. {orderdLines.Skip(1).First().VATClass.A9_EnglishMsg}
3. {orderdLines.Skip(2).First().VATClass.A9_EnglishMsg}
4. {orderdLines.Skip(3).First().VATClass.A9_EnglishMsg}
5. {orderdLines.Skip(4).First().VATClass.A9_EnglishMsg}");
					AssertEquals(expectedString, GetBaseInvoiceWrapper().EnglishLanguageTaxMessagesWithNumbers);
				}
			}
		}

		ARInvoice CreateJobAndPeriodicInvoice(OrgHeader debtor, OrgHeader agent, ZString transactionNum, params Tuple<JobInvoicingConsumerType, AccChargeCode>[] jobTypeAndchargeCodes)
		{
			return CreateJobAndPeriodicInvoice(debtor, agent, transactionNum, jobTypeAndchargeCodes.Select(i => (i.Item1, new[] { i.Item2 })).ToArray());
		}

		ARInvoice CreateJobAndPeriodicInvoice(OrgHeader debtor, OrgHeader agent, ZString transactionNum, params (JobInvoicingConsumerType JobType, AccChargeCode[] ChargeCodes)[] jobTypeAndchargeCodes)
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>(transactionNum, TestObjectCreator.AUD, 1.0m, debtor);
			arInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;

			foreach ((JobInvoicingConsumerType JobType, AccChargeCode[] ChargeCodes) item in jobTypeAndchargeCodes)
			{
				var plugin = TestObjectCreator.CreateJobPlugIn(item.JobType);
				var job = TestObjectCreator.CreateJob(plugin, debtor, 1.0m, agent, 1.0m);
				foreach (var chargeCode in item.ChargeCodes)
				{
					var arInvoiceLine = TestObjectCreator.CreateARInvoiceLine(arInvoice, job, chargeCode, TestObjectCreator.AUD, 1.0m, "Line 001", 100m);
					var jobCharge = TestObjectCreator.CreateJobCharge(arInvoiceLine, job, chargeCode);
				}
			}

			Factory.Save();

			return arInvoice;
		}

		public void TestIExcludedFromDockPackByDefault()
		{
			var wrapper = GetInvoiceWrapper();
			Assert(!ARInvoice.IsCancelled);
			DocumentsDataRegistry.Instance.IncludeCancelledInvoicesInDocumentPacks.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert("A Non-reversed ARInvoice should be included by default", !((IShouldExcludeFromDocPackByDefault)wrapper).IsExcluded);
			DocumentsDataRegistry.Instance.IncludeCancelledInvoicesInDocumentPacks.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert("Should always be included if the registry item is true", !((IShouldExcludeFromDocPackByDefault)wrapper).IsExcluded);

			var reversing = new ARInvoiceReversing(ARInvoice);
			reversing.Reverse();

			Assert("Should always be included if the registry item is true", !((IShouldExcludeFromDocPackByDefault)wrapper).IsExcluded);
			DocumentsDataRegistry.Instance.IncludeCancelledInvoicesInDocumentPacks.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert("A Reversed ARInvoice should not be included by default", ((IShouldExcludeFromDocPackByDefault)wrapper).IsExcluded);
		}

		#endregion

		#region Digital Signature and Certification

		public void TestRemittanceDataCHSCOR()
		{
			InvoiceWrapper = GetInvoiceWrapper();
			AssertNoExceptionThrown(() => AssertNotEquals(string.Empty, InvoiceWrapper.RemittanceDataCHSCOR));

			SetupRemittanceData();

			InvoiceWrapper = GetInvoiceWrapper();
			AssertEquals(@"SPC
0200
1
CH0300233233102759700
K
Eagle Datamation International
184 Bourke Road
2015 Alexandria


AU







1289.87
CHF
K
ABI GAS & TOOLS
171 ABBOTSFORD ROAD
4006 


AU
SCOR
RF4300000000000000001044
AR INVOICE
EPD", InvoiceWrapper.RemittanceDataCHSCOR);
		}

		public void TestRemittanceDataCHQRR()
		{
			InvoiceWrapper = GetInvoiceWrapper();
			AssertNoExceptionThrown(() => AssertNotEquals(string.Empty, InvoiceWrapper.RemittanceDataCHQRR));

			SetupRemittanceData();

			InvoiceWrapper = GetInvoiceWrapper();
			AssertEquals(@"SPC
0200
1
CH0500334563102759954
K
Eagle Datamation International
184 Bourke Road
2015 Alexandria


AU







1289.87
CHF
K
ABI GAS & TOOLS
171 ABBOTSFORD ROAD
4006 


AU
QRR
RF4300000000000000001044
AR INVOICE
EPD", InvoiceWrapper.RemittanceDataCHQRR);
		}

		void SetupRemittanceData()
		{
			var orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABIGAS"));
			Invoice.AH_OH = orgHeader.PK;

			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Switzerland;
			bankAccount.AB_Code = "Bank1";
			bankAccount.AB_IsDefaultReceiptBankAccount = true;
			bankAccount.IBAN = "CH0300233233102759700";
			bankAccount.AB_FullAccountNumber = "CH0500334563102759954";
			Invoice.AH_AB = bankAccount.PK;

			Invoice.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Switzerland;
			Invoice.AH_OSTotal = 1289.87m;
			Invoice.InvoiceRemittanceReference = "RF4300000000000000001044";
		}

		public void TestFiscalSoftwareCertificateNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				AssertEquals(Core.Constants.CountryCodes.Portugal, Invoice.Company.Country.Code);
				Assert(Invoice.AH_DigitalSignature_COMPRESSED.IsEmpty);

				using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				{
					InvoiceWrapper = GetInvoiceWrapper();
					Assert(InvoiceWrapper.FiscalSoftwareCertificateNumber.IsEmpty);

					Invoice.AH_DigitalSignature_COMPRESSED = new ZBlob(new byte[] { 0, 1, 2, 3, 4 });

					InvoiceWrapper = GetInvoiceWrapper();
					Assert(InvoiceWrapper.FiscalSoftwareCertificateNumber.IsEmpty);
				}
				using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345"))
				{
					AssertEquals("12345/AT", InvoiceWrapper.FiscalSoftwareCertificateNumber);
				}
			}
			InvoiceWrapper = GetInvoiceWrapper();

			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345"))
			{
				AssertNotEquals(Core.Constants.CountryCodes.Portugal, Invoice.Company.Country.Code);
				Assert(InvoiceWrapper.FiscalSoftwareCertificateNumber.IsEmpty);
			}
		}

		public void TestFiscalSoftwareCertificateMessage()
		{
			InvoiceWrapper = GetInvoiceWrapper();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				AssertEquals(Core.Constants.CountryCodes.Portugal, Invoice.Company.Country.Code);

				using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumberMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				{
					Assert(InvoiceWrapper.FiscalSoftwareCertificateMessage.IsEmpty);
				}
				using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumberMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Message"))
				{
					AssertEquals("Test Message", InvoiceWrapper.FiscalSoftwareCertificateMessage);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumberMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Test Message"))
			{
				AssertNotEquals(Core.Constants.CountryCodes.Portugal, Invoice.Company.Country.Code);
				Assert(InvoiceWrapper.FiscalSoftwareCertificateNumber.IsEmpty);
			}
		}

		public void TestTransactionFiscalAuthorization()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				AssertEquals(Core.Constants.CountryCodes.Portugal, Invoice.Company.Country.Code);
				Assert(Invoice.AH_DigitalSignature_COMPRESSED.IsEmpty);

				using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				{
					InvoiceWrapper = GetInvoiceWrapper();
					Assert(InvoiceWrapper.FiscalSoftwareCertificateNumber.IsEmpty);
					Assert(InvoiceWrapper.TransactionFiscalAuthorization.IsEmpty);

					Invoice.AH_DigitalSignature_COMPRESSED = new ZBlob(Convert.FromBase64String("956F75D3840C912E616B39CB04BF22AF8A6FE619CACC1C78C3CDF65955D4A90215AE033FE4AADB81CE82AD43BC579504380AA9D984144BB4867EF99F53A73055CEA33879DF44B264D55C7D639DFCCBAC210C44CCA1F1E25AB10CD09A5C2FD1E83A8DB829CF9227C691958B925219462F54EE15FCAD9BB598603256171CB38946"));

					InvoiceWrapper = GetInvoiceWrapper();
					Assert(InvoiceWrapper.TransactionFiscalAuthorization.IsEmpty);
				}
				using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345"))
				{
					AssertEquals("12345/AT", InvoiceWrapper.FiscalSoftwareCertificateNumber);
					AssertEquals("903A", InvoiceWrapper.TransactionFiscalAuthorization);
				}
			}
			InvoiceWrapper = GetInvoiceWrapper();

			using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345"))
			{
				AssertNotEquals(Core.Constants.CountryCodes.Portugal, Invoice.Company.Country.Code);
				Assert(InvoiceWrapper.TransactionFiscalAuthorization.IsEmpty);
			}
		}

		#region QrCode

		[TestDate(2020, 9, 20)]
		public void TestGenerateInvoiceQRCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Creator.ABIGAS.CompanyData.SetARTaxApplicable(true);
				Creator.AALSHI.CompanyData.SetARTaxApplicable(true);
				Creator.Debtor.CompanyData.SetARTaxApplicable(true);

				var ivaTaxRate = CreateTaxRate("IVA", "RAT", 23, 1, Core.Constants.CountryCodes.Portugal);
				var iva6TaxRate = CreateTaxRate("IVA6", "RAT", 6, 1, Core.Constants.CountryCodes.Portugal);
				var iva13TaxRate = CreateTaxRate("IVA13", "RAT", 13, 1, Core.Constants.CountryCodes.Portugal);
				var iva18TaxRate = CreateTaxRate("IVA18", "RAT", 18, 1, Core.Constants.CountryCodes.Portugal);
				var iva22TaxRate = CreateTaxRate("IVA22", "RAT", 22, 1, Core.Constants.CountryCodes.Portugal);
				var exemptTaxRate = CreateTaxRate("EXEMPT", "EXT", 0, 1, Core.Constants.CountryCodes.Portugal);
				var excludeTaxRate = CreateTaxRate("EXCLUDE", "EXL", 0, 1, Core.Constants.CountryCodes.Portugal);
				var notReportTaxRate = CreateTaxRate("NOTREPORT", "NOT", 0, 1, Core.Constants.CountryCodes.Portugal);
				var freeIvaTaxRate = CreateTaxRate("FREEIVA", "RAT", 0, 1, Core.Constants.CountryCodes.Portugal);
				var freeIvaRevTaxRate = CreateTaxRate("FREEIVAREV", "RVS", 0, 1, Core.Constants.CountryCodes.Portugal);
				var ivaRevTaxRate = CreateTaxRate("IVAREV", "RVS", 0, 1, Core.Constants.CountryCodes.Portugal);
				var ivaRev6TaxRate = CreateTaxRate("IVAREV6", "RVS", 0, 1, Core.Constants.CountryCodes.Portugal);
				var ivaRev13TaxRate = CreateTaxRate("IVAREV13", "RVS", 0, 1, Core.Constants.CountryCodes.Portugal);
				var capIvaTaxRate = CreateTaxRate("CAPIVA", "RAT", 23, 1, Core.Constants.CountryCodes.Portugal);

				GlbCompany.CurrentCompany.GC_BusinessRegNo = "123456"; //A

				//B-C
				var ivaRegistration = Creator.ABIGAS.CustomsCodes.AddNew();
				ivaRegistration.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Portugal;
				ivaRegistration.OK_CodeType = OrgCusCode.CodeTypes.IVA;
				ivaRegistration.OK_CustomsRegNo = "PT123123123";

				Creator.AALSHI.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;
				ivaRegistration = Creator.AALSHI.CustomsCodes.AddNew();
				ivaRegistration.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Mexico;
				ivaRegistration.OK_CodeType = OrgCusCode.CodeTypes.IVA;
				ivaRegistration.OK_CustomsRegNo = "MX231231231";

				Creator.Debtor.CustomsCodes.RemoveAndDeleteAll();

				Factory.Save();

				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				var sequence = Creator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "PT-", 1, 100, 1);
				Factory.Save();

				var invoice1 = Creator.CreateARInvoice<ARInvoice>("INV001", Creator.AUD, 2m, Creator.ABIGAS);
				var invoice2 = Creator.CreateARInvoice<ARInvoice>("INV002", Creator.EUR, 1m, Creator.AALSHI);
				invoice2.AH_PostDate = invoice2.AH_PostDate.AddHours(1);
				var invoice3 = Creator.CreateARInvoice<ARInvoice>("INV003", Creator.EUR, 1m, Creator.Debtor);
				invoice3.AH_PostDate = invoice3.AH_PostDate.AddHours(2);
				var invoice4 = Creator.CreateARInvoice<ARInvoice>("INV004", Creator.EUR, 1m, Creator.Debtor);
				invoice4.AH_PostDate = invoice4.AH_PostDate.AddHours(3);
				var invoice5 = Creator.CreateARInvoice<ARInvoice>("INV005", Creator.EUR, 1m, Creator.Debtor);
				invoice5.AH_PostDate = invoice5.AH_PostDate.AddHours(4);
				var invoice6 = Creator.CreateARInvoice<ARInvoice>("INV006", Creator.EUR, 1m, Creator.Debtor);
				invoice5.AH_PostDate = invoice5.AH_PostDate.AddHours(5);

				invoice1.AH_SystemCreateTimeUtc = new ZDateTime(2020, 09, 11, 05, 30, 59);
				invoice2.AH_SystemCreateTimeUtc = new ZDateTime(2020, 09, 11, 05, 30, 59);
				invoice3.AH_SystemCreateTimeUtc = new ZDateTime(2020, 09, 11, 05, 30, 59);
				invoice4.AH_SystemCreateTimeUtc = new ZDateTime(2020, 09, 11, 05, 30, 59);
				invoice5.AH_SystemCreateTimeUtc = new ZDateTime(2020, 09, 11, 05, 30, 59);
				invoice6.AH_SystemCreateTimeUtc = new ZDateTime(2020, 09, 11, 05, 30, 59);

				invoice4.AH_ComplianceSubType = "TCM"; //D

				invoice1.IsSelfBillingInvoice = true; //E
				invoice1.AH_InvoiceDate = new ZDateTime(2020, 1, 10); //F

				invoice1.AH_TransactionReference = "123456"; //G
				invoice2.AH_TransactionReference = "789101"; //G

				var headerReference1 = Factory.New<AccTransactionHeaderReference>();
				headerReference1.AH1_Reference = "00001234";
				headerReference1.AH1_AH = invoice6.PK;
				headerReference1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ATH;

				//I-J-K
				var line1_1 = Creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 2m, "Desc", 5m);
				line1_1.AL_AT = ivaTaxRate.PK;
				line1_1.AL_LocalTaxAmount = 0.5m;
				var line1_2 = Creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 2m, "Desc", 10m);
				line1_2.AL_AT = iva6TaxRate.PK;
				line1_2.AL_LocalTaxAmount = 1m;
				var line1_3 = Creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 2m, "Desc", 15m);
				line1_3.AL_AT = iva13TaxRate.PK;
				line1_3.AL_LocalTaxAmount = 1.5m;
				var line1_4 = Creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 2m, "Desc", 20m);
				line1_4.AL_AT = iva13TaxRate.PK;
				line1_4.AL_LocalTaxAmount = 2m;
				var line1_5 = Creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 2m, "Desc", 5m);
				line1_5.AL_AT = exemptTaxRate.PK;
				var line1_6 = Creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 2m, "Desc", 6m);
				line1_6.AL_AT = excludeTaxRate.PK;
				var line1_7 = Creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 2m, "Desc", 7m);
				line1_7.AL_AT = notReportTaxRate.PK;
				var line1_8 = Creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 2m, "Desc", 8m);
				line1_8.AL_AT = freeIvaTaxRate.PK;
				var line1_9 = Creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 2m, "Desc", 9m);
				line1_9.AL_AT = freeIvaRevTaxRate.PK;
				var line1_10 = Creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 2m, "Desc", 10m);
				line1_10.AL_AT = ivaRevTaxRate.PK;
				var line1_11 = Creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 2m, "Desc", 11m);
				line1_11.AL_AT = ivaRev6TaxRate.PK;
				var line1_12 = Creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 2m, "Desc", 12m);
				line1_12.AL_AT = ivaRev13TaxRate.PK;

				var line2_1 = Creator.CreateARInvoiceLine(invoice2, null, creator.CC1, creator.EUR, 1.0m, "Desc", 35m);
				line2_1.AL_AT = iva18TaxRate.PK;
				line2_1.AL_LocalTaxAmount = 2.5m;
				var line2_2 = Creator.CreateARInvoiceLine(invoice2, null, creator.CC1, creator.EUR, 1.0m, "Desc", 40m);
				line2_2.AL_AT = iva18TaxRate.PK;
				line2_2.AL_LocalTaxAmount = 3m;
				var line2_3 = Creator.CreateARInvoiceLine(invoice2, null, creator.CC1, creator.EUR, 1.0m, "Desc", 14m);
				line2_3.AL_AT = exemptTaxRate.PK;

				var line3_1 = Creator.CreateARInvoiceLine(invoice3, null, creator.CC1, creator.EUR, 1.0m, "Desc", 35m);
				line3_1.AL_AT = iva22TaxRate.PK;
				line3_1.AL_LocalTaxAmount = 3.5m;
				var line3_2 = Creator.CreateARInvoiceLine(invoice3, null, creator.CC1, creator.EUR, 1.0m, "Desc", 40m);
				line3_2.AL_AT = iva22TaxRate.PK;
				line3_2.AL_LocalTaxAmount = 4m;
				var line3_3 = Creator.CreateARInvoiceLine(invoice3, null, creator.CC1, creator.EUR, 1.0m, "Desc", 13m);
				line3_3.AL_AT = exemptTaxRate.PK;

				var line4_1 = Creator.CreateARInvoiceLine(invoice4, null, creator.CC1, creator.EUR, 1.0m, "Desc", 45.05m);
				line4_1.AL_AT = ivaTaxRate.PK;
				line4_1.AL_LocalTaxAmount = 4.5m;
				var line4_2 = Creator.CreateARInvoiceLine(invoice4, null, creator.CC1, creator.EUR, 1.0m, "Desc", 50.07m);
				line4_2.AL_AT = iva18TaxRate.PK;
				line4_2.AL_LocalTaxAmount = 5m;
				var line4_3 = Creator.CreateARInvoiceLine(invoice4, null, creator.CC1, creator.EUR, 1.0m, "Desc", 55.11m);
				line4_3.AL_AT = iva22TaxRate.PK;
				line4_3.AL_LocalTaxAmount = 5.5m;
				var line4_4 = Creator.CreateARInvoiceLine(invoice4, null, creator.CC1, creator.EUR, 1.0m, "Desc", 60m);
				line4_4.AL_AT = capIvaTaxRate.PK;
				line4_4.AL_LocalTaxAmount = 12m;

				var line5_1 = Creator.CreateARInvoiceLine(invoice5, null, creator.CC1, creator.AUD, 2m, "Desc", 8m);
				line5_1.AL_AT = freeIvaTaxRate.PK;

				var line6_1 = Creator.CreateARInvoiceLine(invoice6, null, creator.CC1, creator.AUD, 2m, "Desc", 33m);
				line6_1.AL_AT = excludeTaxRate.PK;
				var line6_2 = Creator.CreateARInvoiceLine(invoice6, null, creator.CC1, creator.AUD, 2m, "Desc", 11m);
				line6_2.AL_AT = notReportTaxRate.PK;

				Factory.Save();

				using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty)) //R
				{
					var expectedResult = "A:41065894724*B:123123123*C:AU*D:FT*E:N*F:20200110*G:TXI PT-/000000001*H:0*I1:PT*I2:27.50*I3:5.00*I4:1.00*I5:17.50*I6:3.50*I7:2.50*I8:0.50*L:6.50*N:5.00*O:64.00*Q:ZWda*R:";

					var result = DocARInvoice.New(invoice1, Factory).QRCodeData;
					AssertEquals(expectedResult, result);
				}

				using (AccountingMasterFilesRegistry.Instance.PTBillingSoftwareCertificateNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "789654")) //R
				{
					var expectedResult = "A:41065894724*B:*C:AU*D:FT*E:N*F:20200920*G:TXI PT-/000000002*H:0*I1:PT*J1:PT-AC*J2:14.00*J7:75.00*J8:5.50*N:5.50*O:94.50*Q:mQZX*R:789654";
					var result = DocARInvoice.New(invoice2, Factory).QRCodeData;
					AssertEquals(expectedResult, result);

					expectedResult = "A:41065894724*B:*C:*D:FT*E:N*F:20200920*G:TXI PT-/000000003*H:0*I1:PT*K1:PT-MA*K2:13.00*K7:75.00*K8:7.50*N:7.50*O:95.50*Q:kfbY*R:789654";
					result = DocARInvoice.New(invoice3, Factory).QRCodeData;
					AssertEquals(expectedResult, result);

					expectedResult = "A:41065894724*B:*C:*D:NC*E:N*F:20200920*G:*H:0*I1:PT*I7:105.05*I8:16.50*J1:PT-AC*J7:50.07*J8:5.00*K1:PT-MA*K7:55.11*K8:5.50*N:27.00*O:237.23*R:789654";
					result = DocARInvoice.New(invoice4, Factory).QRCodeData;
					AssertEquals(expectedResult, result);

					expectedResult = "A:41065894724*B:*C:*D:FT*E:N*F:20200920*G:TXI PT-/000000004*H:0*I1:PT*I2:4.00*N:0.00*O:4.00*Q:bxT8*R:789654";
					result = DocARInvoice.New(invoice5, Factory).QRCodeData;
					AssertEquals(expectedResult, result);

					expectedResult = "A:41065894724*B:*C:*D:FT*E:N*F:20200920*G:TXI PT-/000000005*H:0*I1:0*L:22.00*N:0.00*O:22.00*Q:A8PP*R:789654";
					result = DocARInvoice.New(invoice6, Factory).QRCodeData;
					AssertEquals(expectedResult, result);

					using (AccountingMasterFilesRegistry.Instance.TransactionAuthorizationNumberDate.SetTemporaryValue(invoice6.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2020, 9, 20))) //H
					{
						expectedResult = "A:41065894724*B:*C:*D:FT*E:N*F:20200920*G:TXI PT-/000000005*H:00001234*I1:0*L:22.00*N:0.00*O:22.00*Q:A8PP*R:789654";
						result = DocARInvoice.New(invoice6, Factory).QRCodeData;
						AssertEquals(expectedResult, result);
					}
				}
			}
		}

		[TestDate(2020, 9, 20)]
		public void TestGenerateInvoiceQRCodeWithEmptyComplianceSubType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Creator.ABIGAS.CompanyData.SetARTaxApplicable(true);
				var ivaTaxRate = CreateTaxRate("IVA", "RAT", 23, 1, Core.Constants.CountryCodes.Portugal);

				var ivaRegistration = Creator.ABIGAS.CustomsCodes.AddNew();
				ivaRegistration.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Portugal;
				ivaRegistration.OK_CodeType = OrgCusCode.CodeTypes.IVA;
				ivaRegistration.OK_CustomsRegNo = "PT123123123";
				Factory.Save();

				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
				Creator.SetupComplianceSequence(menuPK, PortugalComplianceInfo.ComplianceSubTypeCodes.TXI, "PT-", 1, 100, 1);
				Factory.Save();

				var invoice1 = Creator.CreateARInvoice<ARInvoice>("INV001", Creator.AUD, 2m, Creator.ABIGAS);
				var line1_1 = Creator.CreateARInvoiceLine(invoice1, null, creator.CC1, creator.AUD, 2m, "Desc", 5m);
				line1_1.AL_AT = ivaTaxRate.PK;
				line1_1.AL_LocalTaxAmount = 0.5m;
				Factory.Save();

				var expectedResult = "A:41065894724*B:123123123*C:AU*D:FT*E:N*F:20200920*G:TXI PT-/000000001*H:0*I1:PT*I7:2.50*I8:0.50*N:0.50*O:3.00*Q:M7lb*R:0";
				var result = DocARInvoice.New(invoice1, Factory).QRCodeData;
				AssertEquals(expectedResult, result);

				invoice1.AH_ComplianceSubType = string.Empty;
				Factory.Save();

				result = DocARInvoice.New(invoice1, Factory).QRCodeData;
				AssertEquals(string.Empty, result);
			}
		}

		AccTaxRate CreateTaxRate(string code, string type, int rateNum, int rateDenom, string country)
		{
			var query = new ZQuery(AccTaxRateSchema.AT_Code, code);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, country);
			var result = Factory.LoadTop1<AccTaxRate>(query);

			if (result == null)
			{
				result = new BusinessObjectFactory().New<AccTaxRate>();
				result.AT_Code = code;
				result.AT_Description = code + " Desc";
				result.AT_IsActive = true;
				result.AT_RN_NKCountry = country;
				result.AT_Type = type;
				result.SetRate_ForTestOnly(rateNum, rateDenom);
				result.Factory.Save();
				result = Factory.Load<AccTaxRate>(result.PK);
			}
			return result;
		}

		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		#endregion

		#endregion

		#region QRCodeData Any Country

		public void TestQRCodeDataAnyCountry()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();

			var docARInvoice = DocARInvoice.New(invoice, Factory);

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			var expectedQRCodeData = "TXIINV20210614EUR160";

			mockIAccountingCountryFactory.As<IQRCodeDataProvider>().Setup(x => x.GetTransactionQRCodeString(It.IsAny<InvoicingBase>())).Returns(expectedQRCodeData);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			var actualQRCodeData = docARInvoice.QRCodeData;

			AssertEquals("QRCodeData has value", expectedQRCodeData, actualQRCodeData);

			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Once);
			mockIAccountingCountryFactory.As<IQRCodeDataProvider>().Verify(x => x.GetTransactionQRCodeString(invoice), Times.Once);

			mockIAccountingCountryFactory.Reset();
			mockIGlobalAccountingCountryFactory.Reset();
			mockIAccountingCountryFactory.As<IQRCodeDataProvider>().Setup(x => x.GetTransactionQRCodeString(It.IsAny<InvoicingBase>())).Returns(expectedQRCodeData);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			actualQRCodeData = docARInvoice.QRCodeData;

			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Never);
			mockIAccountingCountryFactory.As<IQRCodeDataProvider>().Verify(x => x.GetTransactionQRCodeString(invoice), Times.Once);

			AssertEquals("QRCodeData must no change", expectedQRCodeData, actualQRCodeData);

			mockIAccountingCountryFactory.Reset();
			mockIGlobalAccountingCountryFactory.Reset();

			docARInvoice = DocARInvoice.New(invoice, Factory);

			actualQRCodeData = docARInvoice.QRCodeData;

			mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), Times.Once);
			mockIAccountingCountryFactory.As<IQRCodeDataProvider>().Verify(x => x.GetTransactionQRCodeString(invoice), Times.Never);

			AssertEquals("IQRCodeDataProvider is null", ZString.Empty, actualQRCodeData);
		}

		#endregion

		public void TestTransactionSourceReference()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				AssertEquals(Core.Constants.CountryCodes.Portugal, Invoice.Company.Country.Code);
				Assert(Invoice.SourceReference.IsEmpty);

				InvoiceWrapper = GetInvoiceWrapper();
				Assert(InvoiceWrapper.SourceReference.IsEmpty);

				Invoice.SourceReference = "FTM a/189";
				AssertEquals("FTM a/189", Invoice.SourceReference);

				InvoiceWrapper = GetInvoiceWrapper();
				AssertEquals("FTM a/189", InvoiceWrapper.SourceReference);
			}
		}

		public void TestOriginalReferenceComplianceNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				AssertEquals(Core.Constants.CountryCodes.Portugal, Invoice.Company.Country.Code);
				InvoiceWrapper = GetInvoiceWrapper();
				AssertNull(InvoiceWrapper.ParentTransaction);
				AssertEquals("if there is no parent invoice, we return nothing", string.Empty, InvoiceWrapper.OriginalReferenceComplianceNumber);

				var parentInvoice = Factory.NewWithValidTestData<ARInvoice>();
				parentInvoice.AH_TransactionNum = "0001001";
				parentInvoice.AH_TransactionReference = "123456";
				parentInvoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;
				Invoice.AH_TransactionBelongsToGroup = parentInvoice.PK;
				AssertEquals("PreCond: Parent transaction SourceReference is empty", string.Empty, parentInvoice.SourceReference);
				AssertOriginalReferenceComplianceNumber("when parent is set and source ref is empty, we return the parent compliance number", "123456");

				parentInvoice.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.LCD;
				parentInvoice.SourceReference = "NDD a/465";
				AssertOriginalReferenceComplianceNumber("if the source ref starts by the compliance sub type prefix, we should return the sourceRef without the prefix", "a/465");

				parentInvoice.SourceReference = "NDDa/465";
				AssertOriginalReferenceComplianceNumber("if the prefix doesn't match with the compliance sub type then we don't remove it", "NDDa/465");

				parentInvoice.SourceReference = "FTD a/465";
				AssertOriginalReferenceComplianceNumber("if the prefix doesn't match with the compliance sub type then we don't remove it", "FTD a/465");

				void AssertOriginalReferenceComplianceNumber(string message, string expectedResult)
				{
					InvoiceWrapper = GetInvoiceWrapper();
					AssertNotNull(InvoiceWrapper.ParentTransaction);
					AssertEquals(message, expectedResult, InvoiceWrapper.OriginalReferenceComplianceNumber);
				}
			}
		}

		public void TestDocWrapperTaxRegimeInformation()
		{
			var message = "601 - bla bla";
			var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
			var globalFactoryMockAsRegistryInformation = new Mock<ICountryEInvoicingRegistryInformationProvider>();

			globalFactoryMock.Setup(x => x.GetCountryEInvoicingRegistryInformationProvider(Invoice.Company.GC_RN_NKCountryCode)).Returns(globalFactoryMockAsRegistryInformation.Object);
			globalFactoryMockAsRegistryInformation.Setup(x => x.GetTaxRegimeInformation()).Returns(message);

			ObjectFactory.Substitute(globalFactoryMock.Object);
			ObjectFactory.Substitute(globalFactoryMockAsRegistryInformation.Object);

			InvoiceWrapper = GetInvoiceWrapper();
			var result = InvoiceWrapper.TaxRegimeInformation;
			AssertEquals(message, result);

			globalFactoryMock.Verify(x => x.GetCountryEInvoicingRegistryInformationProvider(Invoice.Company.GC_RN_NKCountryCode), Times.Once);
			globalFactoryMockAsRegistryInformation.Verify(x => x.GetTaxRegimeInformation(), Times.Once);
		}

		public void TestDocWrapperTaxRegimeInformation_GetCountryEInvoicingRegistryInformationProvider_IsNull()
		{
			var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
			var globalFactoryMockAsRegistryInformation = new Mock<ICountryEInvoicingRegistryInformationProvider>();

			globalFactoryMock.Setup(x => x.GetCountryEInvoicingRegistryInformationProvider(Invoice.Company.GC_RN_NKCountryCode)).Returns(null as ICountryEInvoicingRegistryInformationProvider);

			ObjectFactory.Substitute(globalFactoryMock.Object);
			ObjectFactory.Substitute(globalFactoryMockAsRegistryInformation.Object);

			InvoiceWrapper = GetInvoiceWrapper();
			var result = InvoiceWrapper.TaxRegimeInformation;
			globalFactoryMockAsRegistryInformation.Verify(x => x.GetTaxRegimeInformation(), Times.Never);
			AssertEquals(ZString.Empty, result);
		}

		#region TestDebtorTaxRegime
		public void TestDebtorTaxRegime_IAccountingCountryComplianceGlobalFactory_GetsInvoiceCountryAsParameter()
		{
			var invoice = ARInvoiceWrapper;

			var mockIAccountingCountryComplianceGlobalFactory = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory();
			var debtorTaxRegime = invoice.DebtorTaxRegime;
			mockIAccountingCountryComplianceGlobalFactory.Verify(x => x.GetFeatureInterface<IDebtorTaxRegime>("AU"));

			invoice.Invoice.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;
			debtorTaxRegime = invoice.DebtorTaxRegime;
			mockIAccountingCountryComplianceGlobalFactory.Verify(x => x.GetFeatureInterface<IDebtorTaxRegime>("MX"));
		}

		public void TestDebtorTaxRegime_Empty_WhenCountryDoesNotImplement_IDebtorTaxRegime()
		{
			var invoice = ARInvoiceWrapper;
			var mockIAccountingCountryComplianceGlobalFactory = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory();

			var debtorTaxRegime = invoice.DebtorTaxRegime;

			Assert(debtorTaxRegime.IsEmpty);
		}

		public void TestDebtorTaxRegime_Empty_WhenInvoiceHeaderIsNull()
		{
			var invoice = ARInvoiceWrapper;
			var mockIDebtorTaxRegime = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory().SetupFeatureInterface<IDebtorTaxRegime>();
			mockIDebtorTaxRegime.Setup(x => x.GetOrgCusCode()).Returns("REG");
			mockIDebtorTaxRegime.Setup(x => x.GetTaxRegimeIdTypes()).Returns(CreateCodeDescriptionPairListWithOnePair("100", "TEST"));
			AssertNull("Precondition: Invoice.Header", invoice.InvoicingBase.Header);

			var debtorTaxRegime = invoice.DebtorTaxRegime;

			Assert(debtorTaxRegime.IsEmpty);
		}

		public void TestDebtorTaxRegime_Empty_WhenIDebtorTaxRegime_GetOrgCusCode_ReturnsCodeTypeNotAddedToInvoiceOrg()
		{
			var regNo = "100";
			var invoice = SetupInvoiceWithOrgHeader("XXX", regNo, Core.Constants.CountryCodes.Mexico);
			var mockIDebtorTaxRegime = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory().SetupFeatureInterface<IDebtorTaxRegime>();
			mockIDebtorTaxRegime.Setup(x => x.GetOrgCusCode()).Returns("REG");
			mockIDebtorTaxRegime.Setup(x => x.GetTaxRegimeIdTypes()).Returns(CreateCodeDescriptionPairListWithOnePair(regNo, "TEST"));

			var debtorTaxRegime = invoice.DebtorTaxRegime;

			Assert(debtorTaxRegime.IsEmpty);
		}

		public void TestDebtorTaxRegime_Empty_WhenIDebtorTaxRegime_GetTaxRegimeIdTypes_ReturnsRegNoNotAddedToInvoiceOrg()
		{
			var orgCusCode = "REG";
			var invoice = SetupInvoiceWithOrgHeader(orgCusCode, "200", Core.Constants.CountryCodes.Mexico);
			var mockIDebtorTaxRegime = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory().SetupFeatureInterface<IDebtorTaxRegime>();
			mockIDebtorTaxRegime.Setup(x => x.GetOrgCusCode()).Returns(orgCusCode);
			mockIDebtorTaxRegime.Setup(x => x.GetTaxRegimeIdTypes()).Returns(CreateCodeDescriptionPairListWithOnePair("100", "TEST"));

			var debtorTaxRegime = invoice.DebtorTaxRegime;

			Assert(debtorTaxRegime.IsEmpty);
		}

		public void TestDebtorTaxRegime_ReturnsRegNoValue_WhenIDebtorTaxRegime_GetTaxRegimeIdTypes_ReturnsRegNoAddedToInvoiceOrg()
		{
			var orgCusCode = "REG";
			var regNo = "100";
			var invoice = SetupInvoiceWithOrgHeader(orgCusCode, regNo, Core.Constants.CountryCodes.Mexico);
			var mockIDebtorTaxRegime = TestMockObjectCreator.CreateAndRegisterIAccountingCountryComplianceGlobalFactory().SetupFeatureInterface<IDebtorTaxRegime>();
			mockIDebtorTaxRegime.Setup(x => x.GetOrgCusCode()).Returns(orgCusCode);
			mockIDebtorTaxRegime.Setup(x => x.GetTaxRegimeIdTypes()).Returns(CreateCodeDescriptionPairListWithOnePair(regNo, "TEST"));

			var debtorTaxRegime = invoice.DebtorTaxRegime;

			AssertEquals((ZString)("100 - TEST"), debtorTaxRegime);

			orgCusCode = "REF";
			regNo = "200";
			invoice = SetupInvoiceWithOrgHeader(orgCusCode, regNo, Core.Constants.CountryCodes.Australia);
			mockIDebtorTaxRegime.Setup(x => x.GetOrgCusCode()).Returns(orgCusCode);
			mockIDebtorTaxRegime.Setup(x => x.GetTaxRegimeIdTypes()).Returns(CreateCodeDescriptionPairListWithOnePair(regNo, "NEW TEST"));

			debtorTaxRegime = invoice.DebtorTaxRegime;

			AssertEquals((ZString)("200 - NEW TEST"), debtorTaxRegime);
		}
		#endregion

		#region GovernmentAgreedPaymentMethod Wrapper

		[TestDate(2020, 11, 8)]
		public void TestGovernmentAgreedPaymentMethod()
		{
			var expectedCountryCode = "XX";
			var expectedInvoiceTerm = "COD";
			var expectedAgreedPaymentMethodOverride = "XXX";

			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_RN_NKCountryCode = expectedCountryCode;

			Invoice.AH_GC = glbCompany.PK;
			Invoice.AH_InvoiceTerm = expectedInvoiceTerm;
			Invoice.AH_InvoiceDate = ZDateTime.Today;
			Invoice.AH_DueDate = ZDateTime.Today;
			Invoice.AH_AgreedPaymentMethodOverride = expectedAgreedPaymentMethodOverride;

			var expectedInvoiceTermPaymentMethodCode = new CodeDescriptionPair("", "");

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInvoicePaymentMethodProvider>().Setup(x => x.GetInvoicePaymentMethodProvider().GetInvoicePaymentMethod(It.IsAny<ZString>(), It.IsAny<ZDateTime>(), It.IsAny<ZDateTime>())).Returns(() => null);
			mockIAccountingCountryFactory.As<IEquivalentAgreedPaymentMethodProvider>().Setup(x => x.GetEquivalentAgreedPaymentMethodProvider().GetEquivalentAgreedPaymentMethod(It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(() => null);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			AssertGovernmentAgreedPaymentMethod(ZString.Empty, Times.Once());

			AssertGovernmentAgreedPaymentMethodIsImplemented(new CodeDescriptionPair("XYZ", "XXXXXXXXXXX"), "XYZ - XXXXXXXXXXX");

			AssertGovernmentAgreedPaymentMethodIsImplemented(new CodeDescriptionPair("", ""), ZString.Empty);

			mockIAccountingCountryFactory.Reset();
			mockIGlobalAccountingCountryFactory.Reset();
			AssertGovernmentAgreedPaymentMethod(ZString.Empty, Times.Never());

			void AssertGovernmentAgreedPaymentMethodIsImplemented(CodeDescriptionPair expectedCodeDescriptionPair, ZString expectedResult)
			{
				mockIAccountingCountryFactory.Reset();
				mockIGlobalAccountingCountryFactory.Reset();

				expectedInvoiceTermPaymentMethodCode = new CodeDescriptionPair("XXX", "xxxx xxx x xxx xxx");

				mockIAccountingCountryFactory.As<IInvoicePaymentMethodProvider>().Setup(x => x.GetInvoicePaymentMethodProvider().GetInvoicePaymentMethod(It.IsAny<ZString>(), It.IsAny<ZDateTime>(), It.IsAny<ZDateTime>())).Returns(expectedInvoiceTermPaymentMethodCode);
				mockIAccountingCountryFactory.As<IEquivalentAgreedPaymentMethodProvider>().Setup(x => x.GetEquivalentAgreedPaymentMethodProvider().GetEquivalentAgreedPaymentMethod(It.IsAny<ZString>(), It.IsAny<ZString>())).Returns(expectedCodeDescriptionPair);
				mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

				AssertGovernmentAgreedPaymentMethod(expectedResult, Times.Once());
			}

			void AssertGovernmentAgreedPaymentMethod(ZString expectedResult, Times times)
			{
				InvoiceWrapper = GetInvoiceWrapper();
				var actualResult = InvoiceWrapper.GovernmentAgreedPaymentMethod;

				AssertEquals("GovernmentAgreedPaymentMethod Wrapper", expectedResult, actualResult);
				mockIAccountingCountryFactory.As<IEquivalentAgreedPaymentMethodProvider>().Verify(x => x.GetEquivalentAgreedPaymentMethodProvider().GetEquivalentAgreedPaymentMethod(expectedAgreedPaymentMethodOverride, expectedInvoiceTermPaymentMethodCode.Code), times);
				mockIAccountingCountryFactory.As<IInvoicePaymentMethodProvider>().Verify(x => x.GetInvoicePaymentMethodProvider().GetInvoicePaymentMethod(expectedInvoiceTerm, ZDateTime.Today, ZDateTime.Today), times);
				mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(expectedCountryCode), Times.Once);
			}
		}

		#endregion

		public void TestDocWrapperInvoiceTermPaymentMethodCode()
		{
			var expectedCountry = "XX";
			var expectedInvoiceTerm = "COD";
			var invoiceDate = new ZDateTime(2022, 2, 1);
			var dueDate = new ZDateTime(2022, 2, 5);

			Invoice.AH_InvoiceTerm = expectedInvoiceTerm;
			Invoice.AH_InvoiceDate = invoiceDate;
			Invoice.AH_DueDate = dueDate;
			Invoice.Company.GC_RN_NKCountryCode = expectedCountry;

			var expectedInvoiceTermPaymentMethodCode = new CodeDescriptionPair("XXX", "xxxx xxx x xxx xxx");

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			mockIAccountingCountryFactory.As<IInvoicePaymentMethodProvider>().Setup(x => x.GetInvoicePaymentMethodProvider().GetInvoicePaymentMethod(Invoice.AH_InvoiceTerm, Invoice.AH_InvoiceDate, Invoice.AH_DueDate)).Returns(expectedInvoiceTermPaymentMethodCode);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
			{
				InvoiceWrapper = GetInvoiceWrapper();

				var result = InvoiceWrapper.GovernmentCreditTerms;
				AssertEquals(expectedInvoiceTermPaymentMethodCode.CodeAndDescription, result);

				mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(expectedCountry), Times.Once);
				mockIAccountingCountryFactory.As<IInvoicePaymentMethodProvider>().Verify(x => x.GetInvoicePaymentMethodProvider().GetInvoicePaymentMethod(expectedInvoiceTerm, invoiceDate, dueDate), Times.Once);

				mockIAccountingCountryFactory.Reset();
				mockIGlobalAccountingCountryFactory.Reset();
				mockIAccountingCountryFactory.As<IInvoicePaymentMethodProvider>().Setup(x => x.GetInvoicePaymentMethodProvider().GetInvoicePaymentMethod(Invoice.AH_InvoiceTerm, Invoice.AH_InvoiceDate, Invoice.AH_DueDate)).Returns(new CodeDescriptionPair("", ""));
				mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

				InvoiceWrapper = GetInvoiceWrapper();

				AssertEquals(string.Empty, InvoiceWrapper.GovernmentCreditTerms);

				mockIAccountingCountryFactory.As<IInvoicePaymentMethodProvider>().Verify(x => x.GetInvoicePaymentMethodProvider().GetInvoicePaymentMethod(expectedInvoiceTerm, invoiceDate, dueDate), Times.Once);
				mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(expectedCountry), Times.Once);

				mockIAccountingCountryFactory.Reset();
				mockIGlobalAccountingCountryFactory.Reset();
				mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns((IAccountingCountryFactory)null);

				InvoiceWrapper = GetInvoiceWrapper();

				AssertEquals(string.Empty, InvoiceWrapper.GovernmentCreditTerms);

				mockIAccountingCountryFactory.As<IInvoicePaymentMethodProvider>().Verify(x => x.GetInvoicePaymentMethodProvider().GetInvoicePaymentMethod(expectedInvoiceTerm, invoiceDate, dueDate), Times.Never());
				mockIGlobalAccountingCountryFactory.Verify(x => x.GetCountryFactory(expectedCountry), Times.Once);
			}
		}

		#region HasDetentionDemurrageChargeSubGroup

		public void TestHasDetentionDemurrageChargeSubGroupForOSRA_ReturnsFalse_WhenNoInvoiceLines()
		{
			var invoice = CreateInvoiceWithSingleLineCharge();
			invoice.Lines.RemoveAndDeleteAll();

			var wrapper = DocARInvoice.New(invoice, Factory);
			AssertEquals(false, wrapper.HasDetentionDemurrageChargeSubGroupForOSRA);
		}

		public void TestHasDetentionDemurrageChargeSubGroupForOSRA_ReturnsFalse_WhenNoInvoiceLinesWithChargeCode()
		{
			var invoice = CreateInvoiceWithSingleLineCharge();
			invoice.Lines[0].AL_AC = ZGuid.Empty;

			var wrapper = DocARInvoice.New(invoice, Factory);
			AssertEquals(false, wrapper.HasDetentionDemurrageChargeSubGroupForOSRA);
		}

		public void TestHasDetentionDemurrageChargeSubGroupForOSRA_ReturnsTrue_ForMatchingSubTypes()
		{
			var matchingSubGroups = GetDetentionDemurrageChargeSubGroupsForOSRA();
			foreach (var subGroup in matchingSubGroups)
			{
				var invoice = CreateInvoiceWithSingleLineCharge();
				invoice.Lines[0].ChargeCode.AC_ChargeSubGroup = subGroup;

				var wrapper = DocARInvoice.New(invoice, Factory);
				AssertEquals($"SubGroup '{subGroup}' should be classified as Detention / Demurrage", true, wrapper.HasDetentionDemurrageChargeSubGroupForOSRA);
			}
		}

		public void TestHasDetentionDemurrageChargeSubGroupForOSRA_ReturnsFalse_ForNonMatchingSubGroups()
		{
			var nonMatchingSubGroup = GetNonDetentionDemurrageChargeSubGroupsForOSRA();
			foreach (var subGroup in nonMatchingSubGroup)
			{
				var invoice = CreateInvoiceWithSingleLineCharge();
				invoice.Lines[0].ChargeCode.AC_ChargeSubGroup = subGroup;

				var wrapper = DocARInvoice.New(invoice, Factory);
				AssertEquals($"SubGroup '{subGroup}' should not be classified as Detention / Demurrage", false, wrapper.HasDetentionDemurrageChargeSubGroupForOSRA);
			}
		}

		public void TestHasDetentionDemurrageChargeSubGroupForOSRA_ReturnsTrue_WithMultipleLines()
		{
			var invoice = CreateInvoiceWithSingleLineCharge();
			var matchingLine = invoice.Lines[0];
			matchingLine.ChargeCode.AC_ChargeSubGroup = GetDetentionDemurrageChargeSubGroupsForOSRA().First();
			var nullLine = (InvoicingLineBase)invoice.Lines.AddNew();
			nullLine.AL_AC = ZGuid.Empty;
			var glAccountLine = (InvoicingLineBase)invoice.Lines.AddNew();
			glAccountLine.AL_AC = ZGuid.Empty;
			glAccountLine.AL_AG = Factory.New<AccGLHeader>().PK;

			invoice.Lines.Sort(nameof(InvoicingLineBase.AL_Sequence), ListSortDirection.Ascending);
			var wrapper = DocARInvoice.New(invoice, Factory);
			AssertEquals(true, wrapper.HasDetentionDemurrageChargeSubGroupForOSRA);

			invoice.Lines.Sort(nameof(InvoicingLineBase.AL_Sequence), ListSortDirection.Descending);
			AssertEquals(true, wrapper.HasDetentionDemurrageChargeSubGroupForOSRA);
		}

		public void TestHasDetentionDemurrageChargeSubGroupForOSRA_ReturnsFalse_WithMultipleLines()
		{
			var invoice = CreateInvoiceWithSingleLineCharge();
			var nonMatchingLine = invoice.Lines[0];
			nonMatchingLine.ChargeCode.AC_ChargeSubGroup = GetNonDetentionDemurrageChargeSubGroupsForOSRA().First();
			var nullLine = (InvoicingLineBase)invoice.Lines.AddNew();
			nullLine.AL_AC = ZGuid.Empty;
			var glAccountLine = (InvoicingLineBase)invoice.Lines.AddNew();
			glAccountLine.AL_AC = ZGuid.Empty;
			glAccountLine.AL_AG = Factory.New<AccGLHeader>().PK;

			invoice.Lines.Sort(nameof(InvoicingLineBase.AL_Sequence), ListSortDirection.Ascending);
			var wrapper = DocARInvoice.New(invoice, Factory);
			AssertEquals(false, wrapper.HasDetentionDemurrageChargeSubGroupForOSRA);

			invoice.Lines.Sort(nameof(InvoicingLineBase.AL_Sequence), ListSortDirection.Descending);
			AssertEquals(false, wrapper.HasDetentionDemurrageChargeSubGroupForOSRA);
		}

		#region EnableEInvoicingQRCode

		public void TestEnableEInvoicingQRCode()
		{
			var docArInvoice = DocARInvoice.New(Invoice, Factory);

			using (AccountingElectronicMessagingRegistry.Instance.EnableEInvoicingQRCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, docArInvoice.EnableEInvoicingQRCode);
			}

			using (AccountingElectronicMessagingRegistry.Instance.EnableEInvoicingQRCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, docArInvoice.EnableEInvoicingQRCode);
			}
		}

		#endregion

		InvoicingBase CreateInvoiceWithSingleLineCharge()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			var line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			line1.AL_AC = chargeCode.PK;

			return invoice;
		}

		IEnumerable<string> GetDetentionDemurrageChargeSubGroupsForOSRA()
			=> new[]
			{
				ChargeCodeSubGroupList.Storage,
				ChargeCodeSubGroupList.CarrierStorage,
				ChargeCodeSubGroupList.ContainerDetention,
			};
		IEnumerable<string> GetNonDetentionDemurrageChargeSubGroupsForOSRA()
			=> typeof(ChargeCodeSubGroupList).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(fi => fi.IsLiteral && !fi.IsInitOnly)
				.Select(fi => (string)fi.GetValue(null))
				.Where(x => !GetDetentionDemurrageChargeSubGroupsForOSRA().Contains(x));

		#endregion

		[TestDate(2024, 09, 27)]
		public void TestGSTVATConversionExchangeRate()
		{
			var companyAU = GlbCompany.CurrentCompany;
			companyAU.GC_RX_NKLocalCurrency = TestObjectCreator.USD.RX_Code;

			AssertEquals("PreCond: companyAU use currency USD", TestObjectCreator.USD.RX_Code, companyAU.GC_RX_NKLocalCurrency);
			AssertEquals("PreCond: Today's date is 27/09/2024", new ZDateTime(2024, 09, 27), ZDateTime.Today);

			ExchangeRateReader.GetReaderInstance().ClearCache();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.AUD, "SEL", 4m, new ZDateTime(2024, 09, 27), new ZDateTime(2024, 09, 27));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.AUD, "SEL", 5m, new ZDateTime(2024, 09, 28), new ZDateTime(2024, 09, 28));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.AUD, "SEL", 6m, new ZDateTime(2024, 09, 29), new ZDateTime(2024, 09, 29));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.AUD, "SEL", 7m, new ZDateTime(2024, 09, 30), new ZDateTime(2024, 09, 30));

			ARInvoice.AH_ExchangeRate = 1;
			ARInvoice.AH_PostDate = new ZDateTime(2024, 09, 28);
			ARInvoice.AH_InvoiceDate = new ZDateTime(2024, 10, 01);

			var line = ARInvoice.Lines.AddNew();
			line.AL_AT = TestObjectCreator.FREEVAT.PK;
			line.AL_TaxDate = new ZDate(2024, 09, 26);

			var docARInvoice = GetInvoiceWrapper();

			SetRegistryExRateOptionRegistry(ExRateOption.TodayExchangeRate.Code);
			AssertGetOverrideExchangeRateResult(0.2m, 0.2m);

			SetRegistryExRateOptionRegistry(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			AssertGetOverrideExchangeRateResult(0m, 0.142857m);

			SetRegistryExRateOptionRegistry(ExRateOption.ExchangeRateBasedOnPostDate.Code);
			AssertGetOverrideExchangeRateResult(0.166667m, 0.166667m);

			SetRegistryExRateOptionRegistry(ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
			AssertEquals(ARInvoice.InvoiceTaxDate, new ZDate(2024, 09, 26));
			AssertGetOverrideExchangeRateResult(0.25m, 0.25m);

			ARInvoice.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
			ARInvoice.AH_ExchangeRate = 10m;

			docARInvoice = GetInvoiceWrapper();
			AssertEquals(GlbCompany.CurrentCompany.Country.LocalCurrency.Code, ARInvoice.AH_RX_NKTransactionCurrency);
			AssertGetOverrideExchangeRateResult(1m, 1m);

			void SetRegistryExRateOptionRegistry(string exRateOptionValue)
			{
				var collectionAR = new InvoicePostingExRateOptionCollection()
				{
					new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, exRateOptionValue, offSet: 1),
					new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, exRateOptionValue, offSet: 0)
				};

				AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAR);
			}

			void AssertGetOverrideExchangeRateResult(decimal expextedConversionExchangeRateWithoutFallBack, decimal expextedConversionExchangeRateWithFallBack)
			{
				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				AssertEquals(expextedConversionExchangeRateWithoutFallBack, docARInvoice.GSTVATConversionExchangeRate);

				AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AssertEquals(expextedConversionExchangeRateWithFallBack, docARInvoice.GSTVATConversionExchangeRate);
			}
		}

		#region Implementation

		protected override DocARBaseInvoice GetBaseInvoiceWrapper()
		{
			return DocARInvoice.New(base.Invoice, base.Factory);
		}

		DocARInvoice GetInvoiceWrapper()
		{
			return DocARInvoice.New(ARInvoice, Factory);
		}

		protected DocARInvoice ARInvoiceWrapper
		{
			get { return (DocARInvoice)base.InvoiceWrapper; }
		}

		protected override TransactionHeader GetWrappedInvoice()
		{
			return Factory.New<ARInvoice>();
		}

		protected override void SetUp()
		{
			Invoice = (InvoicingBase)GetWrappedInvoice();
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocARInvoice.New(Invoice, Factory), };
		}

		ARInvoice ARInvoice
		{
			get { return Invoice as ARInvoice; }
		}

		new DocARInvoice InvoiceWrapper;

		#endregion

		DocARInvoice SetupInvoiceWithOrgHeader(ZString orgCodeType, ZString customsRegNo, ZString country)
		{
			var invoice = ARInvoiceWrapper;
			invoice.TransactionHeader.Company.GC_RN_NKCountryCode = country;
			var orgHeader = Factory.New<OrgHeader>();
			InvoicingBase.AH_OH = orgHeader.PK;
			orgHeader.CustomsCodes.AddNew(orgCodeType, customsRegNo, country);
			return invoice;
		}

		CodeDescriptionPairList CreateCodeDescriptionPairListWithOnePair(ZString code, ZString description)
		{
			var codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair(code, description);
			return codeDescriptionPairList;
		}
	}
}

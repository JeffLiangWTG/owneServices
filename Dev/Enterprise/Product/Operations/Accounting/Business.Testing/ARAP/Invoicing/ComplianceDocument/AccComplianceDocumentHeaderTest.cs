using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccComplianceDocumentHeader))]
	public abstract class AccComplianceDocumentHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = GetComplianceDocumentHeader();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestIHandleDeleteError()
		{
			var complianceDocument = GetComplianceDocumentHeader();
			Assert(!complianceDocument.IsInDatabase);
			Assert(!complianceDocument.IsDeleted);
			Assert(!(complianceDocument as IHandleDeleteError).RollbackAfterDeleteError);
			Assert((complianceDocument as IHandleDeleteError).DisableFormOnDeleteConcurrencyError);

			Factory.Save();
			Assert(complianceDocument.IsInDatabase);
			Assert(!complianceDocument.IsDeleted);
			Assert(!(complianceDocument as IHandleDeleteError).RollbackAfterDeleteError);
			Assert((complianceDocument as IHandleDeleteError).DisableFormOnDeleteConcurrencyError);

			complianceDocument.Delete();
			Assert(complianceDocument.IsInDatabase);
			Assert(complianceDocument.IsDeleted);
			Assert((complianceDocument as IHandleDeleteError).RollbackAfterDeleteError);
			Assert(!(complianceDocument as IHandleDeleteError).DisableFormOnDeleteConcurrencyError);

			Factory.Save();
			Assert(!complianceDocument.IsInDatabase);
			Assert(complianceDocument.IsDeleted);
			Assert(!(complianceDocument as IHandleDeleteError).RollbackAfterDeleteError);
			Assert((complianceDocument as IHandleDeleteError).DisableFormOnDeleteConcurrencyError);

			Assert(!(complianceDocument as IHandleDeleteError).RebindAfterDeleteError);
		}

		public virtual void TestCheckCanVoid()
		{
			var mockIComplianceDocumentVoidingProvider = new Mock<IComplianceDocumentVoidingProvider>();
			mockIComplianceDocumentVoidingProvider.Setup(x => x.ShouldPreventVoidAmendingInvoiceWithCreditNote(It.IsAny<InvoicingBase[]>())).Returns(false);

			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceDocumentVoidingProvider>>().Setup(x => x.Get()).Returns(mockIComplianceDocumentVoidingProvider.Object);

			var mockAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
			mockAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			using (ObjectFactory.Substitute(mockAccountingCountryFactory.Object))
			{
				var complianceDocument = GetComplianceDocumentHeader();
				using (AccountingMasterFilesRegistry.Instance.EnableFinalisedComplianceDocumentToBeSpecialVoided.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.Finalised;
					Assert(complianceDocument.IsFinalised);
					AssertEquals("This compliance document is already finalized.", complianceDocument.CheckCanVoid());
				}

				using (AccountingMasterFilesRegistry.Instance.EnableFinalisedComplianceDocumentToBeSpecialVoided.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.Finalised;
					Assert(complianceDocument.IsFinalised);
					if (complianceDocument is APComplianceDocumentHeader)
					{
						AssertEquals("This compliance document is already finalized.", complianceDocument.CheckCanVoid());
					}
					else
					{
						AssertEquals(ZString.Empty, complianceDocument.CheckCanVoid());
					}
				}

				complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
				AssertEquals(ZString.Empty, complianceDocument.CheckCanVoid());

				complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.NumberSet;
				AssertEquals(ZString.Empty, complianceDocument.CheckCanVoid());

				complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;
				Assert(complianceDocument.IsVoided);
				AssertEquals("This compliance document is already voided.", complianceDocument.CheckCanVoid());
			}

			mockIComplianceDocumentVoidingProvider.Setup(x => x.ShouldPreventVoidAmendingInvoiceWithCreditNote(It.IsAny<InvoicingBase[]>())).Returns(true);

			using (ObjectFactory.Substitute(mockAccountingCountryFactory.Object))
			{
				var complianceDocument = GetComplianceDocumentHeader();
				complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
				AssertEquals("This record cannot be voided as a credit note has been posted.", complianceDocument.CheckCanVoid());
			}
		}

		public void TestIsPreventPrintElectronic()
		{
			StmMenuItem menu = Factory.NewWithValidTestData<StmMenuItem>();
			menu.SU_MenuName = "test";
			menu.SU_BusinessContext = "ARComplianceDocument";
			menu.SU_MenuPath = ZString.Empty;
			menu.SU_IsSystemDefined = true;
			menu.SU_MenuType = "DOC";
			menu.SU_GS_NKStaffCode = ZString.Empty;
			menu.SU_ContactType = "NCT";
			menu.SU_IsPublished = true;

			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence.XD_Code = "AAA";
			complianceSequence.XD_SequenceClass = "NTC";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_IsActive = true;
			complianceSequence.XD_ExpiryDate = new ZDateTime(2012, 11, 11);
			complianceSequence.XD_SU_MenuItem = menu.PK;
			Factory.Save();

			TestObjectCreator.ABIGAS.OH_Category = OrgConstants.Category.NonGovernmentOrganisation;
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);

			new ComplianceDocumentCreator(new InvoicingBase[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();

			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupSinglePeriod(201211, new ZDateTime(2012, 11, 1), new ZDateTime(2012, 11, 30));
			Factory.Save();

			var header = Factory.Load<ARComplianceDocumentHeader>(new ZQuery())[0];
			header.ADH_DocumentDate = new ZDateTime(2012, 11, 11, 1, 1, 1);
			header.ADH_ComplianceSubType = "NTC";
			header.ADH_DocumentNumber = ZString.Empty;
			header.ADH_XD_ComplianceBook = complianceSequence.PK;
			header.ADH_OH_Organisation = TestObjectCreator.ABIGAS.PK;
			Factory.Save();

			Assert(!header.ShouldPreventPrintDocument);

			header.ADH_ComplianceSubType = "TXE";
			Assert(!header.ShouldPreventPrintDocument);

			TestObjectCreator.ABIGAS.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			Assert(!header.ShouldPreventPrintDocument);

			var customsCode = TestObjectCreator.ABIGAS.CustomsCodes.AddNew("MCI ", "123");

			Assert(header.ShouldPreventPrintDocument);

			TestObjectCreator.ABIGAS.CustomsCodes.Remove(customsCode.PK);
			Assert(!header.ShouldPreventPrintDocument);

			TestObjectCreator.ABIGAS.CustomsCodes.AddNew("PIG", "/1234567");
			Assert(header.ShouldPreventPrintDocument);
		}

		public void TestEInvoicingMembers()
		{
			AssertNullOrEmpty(ComplianceDocumentHeader.EInvoicingBatchStatus);
			AssertNullOrEmpty(ComplianceDocumentHeader.EInvoicingBatchNumber);
			AssertNullOrEmpty(ComplianceDocumentHeader.EInvoicingStatus);
			AssertNullOrEmpty(ComplianceDocumentHeader.EInvoicingError);
			AssertEquals(ZDateTime.Empty, ComplianceDocumentHeader.EInvoicingLastSentTimeUtc);
			AssertEquals(ZDateTime.Empty, ComplianceDocumentHeader.EInvoicingLastResponseReceivedUtc);

			var now = ZDateTime.Now;
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ParentID = ComplianceDocumentHeader.PK;
			pivot.AIP_ParentTableCode = AccComplianceDocumentHeaderSchema.Constants.Prefix;
			pivot.AIP_Status = "QUE";
			pivot.AIP_ErrorDescription = "Failed";
			pivot.AIP_LastResponseReceivedUtc = now;
			pivot.AIP_LastSentTimeUtc = now;
			Factory.Save();

			AssertEquals("QUE", ComplianceDocumentHeader.EInvoicingStatus);
			AssertEquals("Failed", ComplianceDocumentHeader.EInvoicingError);
			AssertEquals(now, ComplianceDocumentHeader.EInvoicingLastSentTimeUtc);
			AssertEquals(now, ComplianceDocumentHeader.EInvoicingLastResponseReceivedUtc);

			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			pivot.AIP_AIB = batch.PK;
			batch.AIB_BatchNumber = 123;
			batch.AIB_Status = EInvoicingBatchState.Ready;
			Factory.Save();

			AssertEquals("123", ComplianceDocumentHeader.EInvoicingBatchNumber);
			AssertEquals(EInvoicingBatchState.Ready, ComplianceDocumentHeader.EInvoicingBatchStatus);
		}

		public void TestVATRegistrationNum()
		{
			ComplianceDocumentHeader.ADH_VATRegistrationNumberOverride = ZString.Empty;
			ComplianceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.AALSHI.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = "RCC";

			var cusCode = TestObjectCreator.AALSHI.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "VAT";
			cusCode.OK_CustomsRegNo = "VATREG123";
			Assert(ComplianceDocumentHeader.ADH_VATRegistrationNumberOverride.IsEmpty);
			AssertEquals("VATREG123", ComplianceDocumentHeader.VATRegistrationNum);

			ComplianceDocumentHeader.ADH_VATRegistrationNumberOverride = "Override123";
			Assert(!ComplianceDocumentHeader.ADH_VATRegistrationNumberOverride.IsEmpty);
			AssertEquals("Override123", ComplianceDocumentHeader.VATRegistrationNum);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}

		public void TestIsSequenceBookExpired()
		{
			var book = Factory.NewWithValidTestData<AccComplianceSequence>();
			book.XD_StartDate = new ZDate(2019, 4, 10);
			book.XD_ExpiryDate = new ZDate(2019, 4, 15);
			Factory.Save();

			ComplianceDocumentHeader.ADH_DocumentDate = new ZDate(2019, 4, 9);
			ComplianceDocumentHeader.ADH_XD_ComplianceBook = book.PK;

			Assert(ComplianceDocumentHeader.IsSequenceBookExpired);

			ComplianceDocumentHeader.ADH_DocumentDate = new ZDate(2019, 4, 15);
			Assert(!ComplianceDocumentHeader.IsSequenceBookExpired);

			ComplianceDocumentHeader.ADH_DocumentDate = new ZDate(2019, 4, 16);
			Assert(ComplianceDocumentHeader.IsSequenceBookExpired);
		}

		public void TestAmount()
		{
			AssertEquals(120m, complianceDocumentLine.Amount);
		}

		public void TestTaxAmount()
		{
			AssertEquals(12m, complianceDocumentLine.TaxAmount);
		}

		public void TestTotalAmount()
		{
			AssertEquals(132m, complianceDocumentLine.TotalAmount);
		}

		public void TestADH_DocumentDate()
		{
			var helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupSinglePeriod(ZDateTime.Today.Year * 100 + ZDateTime.Today.Month, ZDateTime.Today, ZDateTime.Today.AddDays(30));

			ComplianceDocumentHeader.ADH_DocumentDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today.Year * 100 + ZDateTime.Today.Month, ComplianceDocumentHeader.ADH_ReportingPeriod);
		}

		public void TestDocumentStatus()
		{
			ComplianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			Assert(ComplianceDocumentHeader.IsAdded);
			Assert(!ComplianceDocumentHeader.IsAllocated);
			Assert(!ComplianceDocumentHeader.IsVoided);
			Assert(!ComplianceDocumentHeader.IsFinalised);

			ComplianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.NumberSet;
			Assert(!ComplianceDocumentHeader.IsAdded);
			Assert(ComplianceDocumentHeader.IsAllocated);
			Assert(!ComplianceDocumentHeader.IsVoided);
			Assert(!ComplianceDocumentHeader.IsFinalised);

			ComplianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Finalised;
			Assert(!ComplianceDocumentHeader.IsAdded);
			Assert(!ComplianceDocumentHeader.IsAllocated);
			Assert(!ComplianceDocumentHeader.IsVoided);
			Assert(ComplianceDocumentHeader.IsFinalised);

			ComplianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;
			Assert(!ComplianceDocumentHeader.IsAdded);
			Assert(!ComplianceDocumentHeader.IsAllocated);
			Assert(ComplianceDocumentHeader.IsVoided);
			Assert(!ComplianceDocumentHeader.IsFinalised);
		}

		public void TestDelete()
		{
			var arInv = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor);
			var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine.AL_AG = testObjectCreator.GLHeader1.PK;
			var invoiceDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine);
			invoiceDocumentHeader.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			invoiceDocumentHeader.ADH_ReportingPeriod = 201802;
			Factory.Save();

			invoiceDocumentHeader.Delete();
			Assert(invoiceDocumentHeader.IsDeleted);

			var arInv1 = testObjectCreator.CreateARInvoice<ARInvoice>("INV002", testObjectCreator.AUD, 1m, testObjectCreator.Debtor);
			var arInvLine1 = (ARInvoiceLine)arInv1.Lines.AddNew();
			arInvLine1.AL_AG = testObjectCreator.GLHeader1.PK;
			var invoiceDocumentHeader1 = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "AA00000001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine1);
			invoiceDocumentHeader1.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			invoiceDocumentHeader1.ADH_ReportingPeriod = 201802;
			Factory.Save();

			AssertExceptionThrown<CannotDeleteException>(invoiceDocumentHeader1.Delete);
			Assert(!invoiceDocumentHeader1.IsDeleted);
		}

		public void TestTransactionType()
		{
			var arInv = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor);
			var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine.AL_AG = testObjectCreator.GLHeader1.PK;
			var invoiceDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "TX00010001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine);
			invoiceDocumentHeader.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			invoiceDocumentHeader.ADH_ReportingPeriod = 201802;
			Factory.Save();

			AssertEquals(TransactionTypes.Invoice, invoiceDocumentHeader.ADH_TransactionType);

			var arCrd = testObjectCreator.CreateARCreditNote("CRD001", testObjectCreator.Debtor, testObjectCreator.TWD, 1m, "");
			var arCrdLine = (ARCreditNoteLine)arCrd.Lines.AddNew();
			arCrdLine.AL_AG = testObjectCreator.GLHeader1.PK;
			var creditNoteDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "", TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE, "desc", arCrdLine);
			creditNoteDocumentHeader.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			creditNoteDocumentHeader.ADH_ReportingPeriod = 201802;
			Factory.Save();

			AssertEquals(TransactionTypes.CreditNote, creditNoteDocumentHeader.ADH_TransactionType);
		}

		public void TestINVComplianceDocumentHeaderForCRD()
		{
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "0001", TestObjectCreator.AUD, 1.0m, 100m, 0m, 100m, 0m);
			var creditNote = TestObjectCreator.CreateARCreditNoteWithLine("0002", TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1m, "Desc", null, TestObjectCreator.CC1, 100m, ZDateTime.Today, false);
			creditNote.OriginalTransactionReference = invoice.PK;
			Factory.Save();

			var invoiceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "Des", "ABC");
			var invoicecomplianceDocumentLine = TestObjectCreator.CreateComplianceDocumentLine(invoiceDocumentHeader, "Test");
			TestObjectCreator.CreateComplianceDocumentPivot(invoicecomplianceDocumentLine, invoice.Lines[0]);
			invoiceDocumentHeader.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			invoiceDocumentHeader.ADH_DocumentNumber = "D00001";

			var creditNoteDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "Des", "DEF");
			var creditNotecomplianceDocumentLine = TestObjectCreator.CreateComplianceDocumentLine(creditNoteDocumentHeader, "Test");
			TestObjectCreator.CreateComplianceDocumentPivot(creditNotecomplianceDocumentLine, creditNote.Lines[0]);
			creditNoteDocumentHeader.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			creditNoteDocumentHeader.ADH_DocumentNumber = "D00002";
			creditNoteDocumentHeader.ADH_DocumentStatus = "VOD";
			Factory.Save();
			AssertNull(creditNoteDocumentHeader.INVComplianceDocumentHeaderForCRD);

			creditNoteDocumentHeader.ADH_DocumentStatus = "SET";
			creditNoteDocumentHeader.ADH_DocumentNumber = "D00001";
			Factory.Save();
			AssertNotNull(creditNoteDocumentHeader.INVComplianceDocumentHeaderForCRD);
		}

		public void TestComplianceDocumentLinesWithVODStatus()
		{
			var arInv = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor);
			var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine.AL_AG = testObjectCreator.GLHeader1.PK;
			var invoiceDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine);
			invoiceDocumentHeader.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			invoiceDocumentHeader.ADH_ReportingPeriod = 201802;
			Factory.Save();

			AssertEquals("compliance document has lines", 1, invoiceDocumentHeader.ComplianceDocumentLines.Count);
			AssertEquals("compliance document has lines", 1, invoiceDocumentHeader.OriginalComplianceDocumentLines.Count);

			invoiceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;
			Factory.Save();

			var complianceDocumentHeader = new BusinessObjectFactory().Load<ARComplianceDocumentHeader>(invoiceDocumentHeader.PK);
			AssertEquals("compliance document has no lines to display after void", 0, complianceDocumentHeader.ComplianceDocumentLines.Count);
			AssertEquals("compliance document has original line after void", 1, complianceDocumentHeader.OriginalComplianceDocumentLines.Count);
		}

		public void TestComplianceDocument_ContactChangedLog()
		{
			var arInv = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor);
			var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			var contact1 = testObjectCreator.CreateContact(testObjectCreator.Debtor, "John1", "email1@example.com");
			var contact2 = testObjectCreator.CreateContact(testObjectCreator.Debtor, "John2", "email2@example.com");
			arInvLine.AL_AG = testObjectCreator.GLHeader1.PK;
			var complianceDocument = GetComplianceDocumentHeader();
			complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.NumberSet;
			complianceDocument.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			complianceDocument.DisplayInvoiceContactOverride = contact1.PK;
			Factory.Save();

			complianceDocument.DisplayInvoiceContactOverride = contact2.PK;
			Factory.Save();
			var occLogs = complianceDocument.Logs.Find(x => x.SL_SE_NKEvent == Events.InvoiceContactOverride.Code);
			AssertEquals("compliance document should have 1 OCC log", 1, occLogs.Count());
			AssertEquals("Invoice contact override from 'John1' to 'John2'", occLogs.First().SL_Reference);
		}

		public void TestComplianceDocument_ContactReadOnly()
		{
			var arInv = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor);
			var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine.AL_AG = testObjectCreator.GLHeader1.PK;
			var complianceDocument = GetComplianceDocumentHeader();
			complianceDocument.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			complianceDocument.ADH_OA_AddressOverride = testObjectCreator.Debtor.Addresses[0].PK;

			complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.NumberSet;
			AssertEquals("Contact should not be read-only when ADH_DocumentStatus is NumberSet", false, complianceDocument.OrganisationAddressWithContact.ContactFK_ReadOnly);

			complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			AssertEquals("Contact should not be read-only when ADH_DocumentStatus is Added", false, complianceDocument.OrganisationAddressWithContact.ContactFK_ReadOnly);

			complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.Finalised;
			AssertEquals("Contact should be read-only when ADH_DocumentStatus is Finalised", true, complianceDocument.OrganisationAddressWithContact.ContactFK_ReadOnly);

			complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;
			AssertEquals("Contact should not be read-only when ADH_DocumentStatus is Voided", false, complianceDocument.OrganisationAddressWithContact.ContactFK_ReadOnly);

			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = "TXT";
			report.ACR_DateFrom = new ZDate(2018, 7, 1);
			report.ACR_DateTo = new ZDate(2018, 7, 31);

			complianceDocument.Finalise(report);

			AssertEquals("Contact should be read-only when ADH_DocumentStatus is Finalised after Voided", true, complianceDocument.OrganisationAddressWithContact.ContactFK_ReadOnly);
		}

		public virtual void TestInternalReferenceConcurrencyPolicy()
		{
			var complianceDocument = GetComplianceDocumentHeader();
			complianceDocument.ADH_DocumentNumber = ZString.Empty;
			complianceDocument.ADH_InternalReference = ZString.Empty;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var newComplianceDocument = newFactory.Load<AccComplianceDocumentHeader>(complianceDocument.PK);
			newComplianceDocument.ADH_DocumentNumber = "AA001";
			newFactory.Save();

			complianceDocument.ADH_DocumentNumber = "AA001";
			AssertExceptionThrown<ZSaveConcurrencyException>(Factory.Save);
		}

		public void TestComplianceDocumentHeaderWithOrgAddressCompanyName()
		{
			var companyOrg = testObjectCreator.CreateOrgHeader("COMPROXY", true, true);
			var companyOrgAddress = testObjectCreator.CreateAddress(companyOrg, OrgAddressType.Office, true);
			companyOrgAddress.CompanyName = "Test change company name";

			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.TWD, 1m, companyOrg);
			var arInvLine = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine.AL_AG = TestObjectCreator.GLHeader1.PK;

			var invoiceDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "TX00010001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arInvLine);
			invoiceDocumentHeader.ADH_OH_Organisation = companyOrg.PK;
			invoiceDocumentHeader.ADH_OA_AddressOverride = companyOrgAddress.PK;

			AssertEquals("OrgAddressCompanyName", "Test change company name", invoiceDocumentHeader.OrgAddressCompanyName);

			companyOrgAddress.CompanyName = ZString.Empty;
			companyOrg.OH_FullName = "Org Name";
			AssertEquals("OrgAddressCompanyName", "Org Name", invoiceDocumentHeader.OrgAddressCompanyName);
		}

		public void TestCompanyVATRegistrationNum()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			cusCode.OK_CustomsRegNo = "32323329";
			Factory.Save();

			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_OH_OrgProxy = org.PK;
			currentCompany.GC_Code = "TXX";
			currentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			currentCompany.GC_IsActive = true;

			var companyBranch = Factory.NewWithValidTestData<GlbBranch>();
			companyBranch.GB_GC = currentCompany.PK;
			companyBranch.GB_Code = "TXX";
			companyBranch.GB_OH_OrgProxy = currentCompany.GC_OH_OrgProxy;
			Factory.Save();

			AssertEquals("32323329", ComplianceDocumentHeader.CompanyVATRegistrationNum);
		}

		protected abstract AccComplianceDocumentHeader GetComplianceDocumentHeader();

		AccComplianceDocumentLine complianceDocumentLine;
		protected AccComplianceDocumentHeader ComplianceDocumentHeader;

		protected override void SetUp()
		{
			base.SetUp();

			var frt = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			var caf = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CAF"));

			var accInvMsg = Factory.NewWithValidTestData<AccInvMsg>();
			accInvMsg.A9_Code = "MSG";

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "TX1";
			taxRate.AT_PostingGroupId = 0;
			taxRate.AT_A9_DefaultVatClass = accInvMsg.PK;

			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TX2";
			taxRate1.AT_PostingGroupId = 1;

			var arInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			arInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			arInvoice.AH_TransactionType = TransactionTypes.Invoice;
			arInvoice.AH_InvoiceAmount = 256;
			arInvoice.AH_OutstandingAmount = 256;
			arInvoice.AH_ExchangeRate = 1;

			var arInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			arInvoiceLine.AL_AH = arInvoice.PK;
			arInvoiceLine.AL_AT = taxRate.PK;
			arInvoiceLine.AL_AC = frt.PK;
			arInvoiceLine.AL_LineAmount = 44;
			arInvoiceLine.AL_GSTVAT = 4;
			arInvoiceLine.AL_OSAmount = 48;
			arInvoiceLine.AL_ExchangeRate = 1;

			var arInvoiceLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			arInvoiceLine1.AL_AH = arInvoice.PK;
			arInvoiceLine1.AL_AT = taxRate.PK;
			arInvoiceLine1.AL_AC = caf.PK;
			arInvoiceLine1.AL_LineAmount = 50;
			arInvoiceLine1.AL_GSTVAT = 5;
			arInvoiceLine1.AL_OSAmount = 55;
			arInvoiceLine1.AL_ExchangeRate = 1;

			var arInvoiceLine2 = Factory.NewWithValidTestData<AccTransactionLines>();
			arInvoiceLine2.AL_AH = arInvoice.PK;
			arInvoiceLine2.AL_AT = taxRate.PK;
			arInvoiceLine2.AL_AC = caf.PK;
			arInvoiceLine2.AL_LineAmount = 70;
			arInvoiceLine2.AL_GSTVAT = 7;
			arInvoiceLine2.AL_OSAmount = 77;
			arInvoiceLine2.AL_ExchangeRate = 1;

			var arInvoiceLine3 = Factory.NewWithValidTestData<AccTransactionLines>();
			arInvoiceLine3.AL_AH = arInvoice.PK;
			arInvoiceLine3.AL_AT = taxRate1.PK;
			arInvoiceLine3.AL_AC = frt.PK;
			arInvoiceLine3.AL_LineAmount = 92;
			arInvoiceLine3.AL_GSTVAT = 5;
			arInvoiceLine3.AL_OSAmount = 97;
			arInvoiceLine3.AL_ExchangeRate = 1;

			ComplianceDocumentHeader = GetComplianceDocumentHeader();
			ComplianceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			complianceDocumentLine = TestObjectCreator.CreateComplianceDocumentLine(ComplianceDocumentHeader, "Test");
			TestObjectCreator.CreateComplianceDocumentPivot(complianceDocumentLine, arInvoiceLine1);
			TestObjectCreator.CreateComplianceDocumentPivot(complianceDocumentLine, arInvoiceLine2);

			var complianceDocumentLine1 = TestObjectCreator.CreateComplianceDocumentLine(ComplianceDocumentHeader, "Test", 2);
			TestObjectCreator.CreateComplianceDocumentPivot(complianceDocumentLine1, arInvoiceLine);

			var complianceDocumentHeader1 = GetComplianceDocumentHeader();
			var complianceDocumentLine2 = TestObjectCreator.CreateComplianceDocumentLine(complianceDocumentHeader1, "Test");
			TestObjectCreator.CreateComplianceDocumentPivot(complianceDocumentLine2, arInvoiceLine3);

			Factory.Save();
		}

		TestObjectCreator testObjectCreator;
		public TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}

		#region ComplianceDocumentHeaderTest

		#region Compliance Reports

		public void TestNonQueueForAPComplianceReportsAfterVoided()
		{
			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);

			var reportConfigurations = SetupConfigurationForComplianceDocument(newFactory, "TW1", ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber);
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);
			newFactory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var invoice1 = creator.CreateInvoice(typeof(APInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line1 = creator.CreateInvoiceLine(invoice1, TestObjectCreator.TWD, 1M, 100M);
				line1.AL_AT = creator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice1 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocuments = newFactory.Load<APComplianceDocumentHeader>(query);
				AssertEquals("created compliance document records count: ", 1, complianceDocuments.Length);
				var complianceDocumentHeader = complianceDocuments[0];
				complianceDocumentHeader.ADH_ComplianceSubType = "TXC";
				complianceDocumentHeader.ADH_DocumentNumber = "001";
				complianceDocumentHeader.ComplianceDocumentLines[0].ADL_Description = "desc";
				newFactory.Save();

				var queueEntries = new DynamicBusinessObjectCollection(newFactory);
				queueEntries.Load("select * from dbo.AccTransactionComplianceReportQueue");
				AssertEquals("only one compliance document is created in AccTransactionComplianceReportQueue table", 1, queueEntries.Count);
				AssertEquals("TW1", (ZString)queueEntries[0]["ACQ_ReportType"]);
				AssertEquals(complianceDocuments[0].PK, (ZGuid)queueEntries[0]["ACQ_ParentID"]);

				var clusteredIndexFieldsForAcq = ComplianceReportTransactionQueuerTest.Helpers.GetClusteredIndexFieldsForAcqTable();
				using (Db.Connection.TrackExecutedCommands())
				{
					complianceDocumentHeader.Void();

					var deleteCommands = ComplianceReportTransactionQueuerTest.Helpers.GetDeleteCommandsCoveredByIndexFields(
											Db.Connection.ExecutedCommands,
											clusteredIndexFieldsForAcq
										);
					Assert("DELETE from dbo.AccTransactionComplianceReportQueue must run", deleteCommands.Any());
					var nonCoveredCommands = deleteCommands.Where(x => !x.isCovered);
					Assert("DELETE from dbo.AccTransactionComplianceReportQueue must cover clustered index fields to minimise locking", !nonCoveredCommands.Any());
				}
				newFactory.Save();

				queueEntries.Load("select * from dbo.AccTransactionComplianceReportQueue");
				AssertEquals("There is no records after voided", 0, queueEntries.Count);
			}
		}

		public void TestOnLoad()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var complianceDocument = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
				complianceDocument.ADH_DocumentDate = new ZDate(2018, 7, 5);
				complianceDocument.ADH_ComplianceSubType = "TXI";
				complianceDocument.ADH_DocumentNumber = "001";
				complianceDocument.ADH_ReportingPeriod = 201807;
				complianceDocument.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
				Factory.Save();

				complianceDocument.OnLoaded();
				Assert(!complianceDocument.ReadOnly);
				Assert(!complianceDocument.ComplianceDocumentLines.ReadOnly);

				complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.Finalised;
				Factory.Save();

				Assert(complianceDocument.IsFinalised);

				complianceDocument.OnLoaded();
				Assert(complianceDocument.ReadOnly);
				Assert(complianceDocument.ComplianceDocumentLines.ReadOnly);

				complianceDocument.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;
				Factory.Save();

				Assert(complianceDocument.IsVoided);

				complianceDocument.OnLoaded();
				Assert(complianceDocument.ReadOnly);
				Assert(complianceDocument.ComplianceDocumentLines.ReadOnly);
			}
		}

		public void TestQueueForComplianceReports()
		{
			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);

			var reportConfigurations = SetupConfigurationForComplianceDocument(newFactory, "TW1", ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber);
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);
			newFactory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var invoice1 = creator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line1 = creator.CreateInvoiceLine(invoice1, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				line1.AL_AT = creator.GST1.PK;

				var creditNote1 = creator.CreateInvoice(typeof(ARCreditNote), "INV002", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line2 = creator.CreateInvoiceLine(creditNote1, TestObjectCreator.TWD, 1M, 200M, 20M, 0M, TestObjectCreator.CC1.PK);
				line2.AL_AT = creator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice1, creditNote1 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocuments = newFactory.Load<ARComplianceDocumentHeader>(query);
				AssertEquals("created compliance document records count: ", 2, complianceDocuments.Length);

				complianceDocuments[0].ADH_ComplianceSubType = "TXC";
				complianceDocuments[1].ADH_ComplianceSubType = "TDP";
				complianceDocuments[0].ADH_DocumentNumber = "001";
				complianceDocuments[1].ADH_DocumentNumber = "002";
				newFactory.Save();

				var queueEntries = new DynamicBusinessObjectCollection(newFactory);
				queueEntries.Load("select * from dbo.AccTransactionComplianceReportQueue");
				AssertEquals("only one compliance document is created in AccTransactionComplianceReportQueue table", 1, queueEntries.Count);
				AssertEquals("TW1", (ZString)queueEntries[0]["ACQ_ReportType"]);
				AssertEquals(complianceDocuments[0].PK, (ZGuid)queueEntries[0]["ACQ_ParentID"]);
			}
		}

		public abstract void TestQueueForComplianceReports_ACQDate();

		public void TestDeleteWithComplianceReport()
		{
			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var report = newFactory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = "TW1";
				report.ACR_DateFrom = ZDate.Today;
				report.ACR_DateTo = ZDate.Today.AddDays(1);
				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				var reportConfigurations = SetupConfigurationForComplianceDocument(newFactory, "TW1", ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber);
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);
				newFactory.Save();

				var invoice1 = creator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line1 = creator.CreateInvoiceLine(invoice1, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				line1.AL_AT = creator.GST1.PK;
				var creditNote1 = creator.CreateInvoice(typeof(ARCreditNote), "INV002", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line2 = creator.CreateInvoiceLine(creditNote1, TestObjectCreator.TWD, 1M, 200M, 20M, 0M, TestObjectCreator.CC1.PK);
				line2.AL_AT = creator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice1, creditNote1 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocuments = newFactory.Load<ARComplianceDocumentHeader>(query);
				AssertEquals("created compliance document records count: ", 2, complianceDocuments.Length);
				var complianceDocument1 = complianceDocuments[0];
				var complianceDocument2 = complianceDocuments[1];

				complianceDocument1.ADH_ComplianceSubType = "TXC";
				complianceDocument2.ADH_ComplianceSubType = "TXI";
				complianceDocument1.ADH_DocumentNumber = "001";
				complianceDocument2.ADH_DocumentNumber = "002";
				newFactory.Save();

				report.GenerateFromQueue();
				AssertEquals("Report is Generated", AccComplianceReport.Status.ReportGenerated, report.ACR_Status);
				AssertComplianceDocumentExists(complianceDocument1.PK, true, true, true);
				AssertComplianceDocumentExists(complianceDocument2.PK, true, true, true);

				AssertEquals("compliance document can not be deleted", false, complianceDocument1.CanDelete);
				AssertExceptionThrown<CannotDeleteException>(complianceDocument1.Delete);
				newFactory.Save();
				AssertEquals("Report is Invalidated", AccComplianceReport.Status.ReportGenerated, report.ACR_Status);
				AssertComplianceDocumentExists(complianceDocument1.PK, true, true, true);

				report.GenerateFromQueue();
				report.Finalise();
				var complianceDocument = newFactory.Load<AccComplianceDocumentHeader>(complianceDocument2.PK);
				AssertEquals("Report is Finalised", AccComplianceReport.Status.ReportFinalised, report.ACR_Status);
				AssertEquals("compliance document can not be deleted", false, complianceDocument.CanDelete);
				AssertEquals("compliance document can not be deleted message", "This record can not be deleted, because it's document status is not ADD.", complianceDocument.ReasonForNotAbleToDelete);
				AssertExceptionThrown<CannotDeleteException>("delete should fail because report is finalised", complianceDocument.Delete);
				AssertComplianceDocumentExists(complianceDocument.PK, false, true, true);
			}
		}

		public void TestDocumentStatusUpdatedToBeVAF()
		{
			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var report = newFactory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = "TW1";
				report.ACR_DateFrom = ZDate.Today;
				report.ACR_DateTo = ZDate.Today.AddDays(1);
				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				var reportConfigurations = SetupConfigurationForComplianceDocument(newFactory, "TW1", ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber);
				SetupConfigurationForComplianceDocument(newFactory, "TW2", ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.ComplianceDocumentNumber, reportConfigurations);
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);
				newFactory.Save();

				var invoice = creator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line = creator.CreateInvoiceLine(invoice, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				line.AL_AT = creator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocuments = newFactory.Load<ARComplianceDocumentHeader>(query);
				AssertEquals("created compliance document records count: ", 1, complianceDocuments.Length);
				var complianceDocument = complianceDocuments[0];

				complianceDocument.ADH_ComplianceSubType = "TXC";
				complianceDocument.ADH_DocumentNumber = "001";
				newFactory.Save();

				report.GenerateFromQueue();
				report.Finalise();
				complianceDocument.Void();

				AssertEquals("Report is Finalised", AccComplianceReport.Status.ReportFinalised, report.ACR_Status);
				AssertEquals(complianceDocument.ADH_DocumentStatus, ComplianceDocumentStatus.FinalisedSpecialVoided);
			}
		}

		public void TestVoidWithComplianceReport()
		{
			var newFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(newFactory);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var report1 = newFactory.NewWithValidTestData<AccComplianceReport>();
				report1.ACR_ReportType = "TW1";
				report1.ACR_DateFrom = ZDate.Today;
				report1.ACR_DateTo = ZDate.Today.AddDays(1);
				report1.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				var report2 = newFactory.NewWithValidTestData<AccComplianceReport>();
				report2.ACR_ReportType = "TW2";
				report2.ACR_DateFrom = ZDate.Today;
				report2.ACR_DateTo = ZDate.Today.AddDays(1);
				report2.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				var reportConfigurations = SetupConfigurationForComplianceDocument(newFactory, "TW1", ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber);
				SetupConfigurationForComplianceDocument(newFactory, "TW2", ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.ComplianceDocumentNumber, reportConfigurations);
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);
				newFactory.Save();

				var invoice1 = creator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.TWD, 1M, TestObjectCreator.ABIGAS);
				var line1 = creator.CreateInvoiceLine(invoice1, TestObjectCreator.TWD, 1M, 100M, 10M, 0M, TestObjectCreator.CC1.PK);
				line1.AL_AT = creator.GST1.PK;

				new ComplianceDocumentCreator(new[] { invoice1 }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var complianceDocuments = newFactory.Load<ARComplianceDocumentHeader>(query);
				AssertEquals("created compliance document records count: ", 1, complianceDocuments.Length);
				var complianceDocument1 = complianceDocuments[0];

				complianceDocument1.ADH_ComplianceSubType = "TXC";
				complianceDocument1.ADH_DocumentNumber = "001";
				newFactory.Save();

				report1.GenerateFromQueue();
				report2.GenerateFromQueue();
				AssertEquals("Report1 is Generated", AccComplianceReport.Status.ReportGenerated, report1.ACR_Status);
				AssertEquals("Report2 is Generated", AccComplianceReport.Status.ReportGenerated, report2.ACR_Status);
				AssertComplianceDocumentExists(complianceDocument1.PK, true, true, true, 2);

				AssertNoExceptionThrown(complianceDocument1.Void);
				newFactory.Save();
				AssertEquals("Report1 is Invalidated", AccComplianceReport.Status.ReportInvalidated, report1.ACR_Status);
				AssertEquals("Report2 is Invalidated", AccComplianceReport.Status.ReportInvalidated, report2.ACR_Status);
				AssertComplianceDocumentExists(complianceDocument1.PK, true, false, true, 1);

				report1.GenerateFromQueue();
				report1.Finalise();
				AssertEquals("Report is Finalised", AccComplianceReport.Status.ReportFinalised, report1.ACR_Status);
				AssertNoExceptionThrown(complianceDocument1.Void);
				AssertEquals("Report is Finalised", AccComplianceReport.Status.ReportFinalised, report1.ACR_Status);
				AssertComplianceDocumentExists(complianceDocument1.PK, false, true, true, 1);
			}
		}

		void AssertComplianceDocumentExists(ZGuid pk, bool isInQueueTable, bool isInPivotTable, bool isInDocumentTable, int count = 1)
		{
			var sql = $"SELECT * FROM dbo.AccTransactionComplianceReportQueue WHERE ACQ_ParentID = '{pk}'";
			var rows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();
			AssertEquals("Exists in ReportQueue table:", isInQueueTable, rows.Count() == count);

			sql = $"SELECT * FROM dbo.AccComplianceReportTransactionPivot WHERE ACL_ParentID = '{pk}'";
			rows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();
			AssertEquals("Exists in ReportPivot table:", isInPivotTable, rows.Count() == count);

			sql = $"SELECT * FROM dbo.AccComplianceDocumentHeader WHERE ADH_PK = '{pk}'";
			rows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();
			AssertEquals("Exists in AccComplianceDocumentHeader table:", isInDocumentTable, rows.Count() == 1);
		}

		protected ComplianceReportConfigurationCollection SetupConfigurationForComplianceDocument(BusinessObjectFactory newFactory, ZString reportCode, ZString orderBy, ComplianceReportConfigurationCollection reportConfigurations = null)
		{
			if (reportConfigurations == null)
			{
				reportConfigurations = new ComplianceReportConfigurationCollection(newFactory);
			}
			var reportConfig = reportConfigurations.AddNew();
			reportConfig.Country = CountryCodes.Taiwan;
			reportConfig.ReportCode = reportCode;
			reportConfig.ReportBaseTablePrefix = ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;
			reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange;
			reportConfig.TaxRegistrationType = reportConfig.Lookups.TaxRegistrationTypeList[0].Code;
			reportConfig.ReportLineOrdering = orderBy;

			var setting1 = reportConfig.Settings.AddNew();
			setting1.ComplianceSubType = "TXC";
			setting1.LedgerType = "AR";
			var setting2 = reportConfig.Settings.AddNew();
			setting2.ComplianceSubType = "TXI";
			setting2.LedgerType = "AR";
			var setting3 = reportConfig.Settings.AddNew();
			setting3.ComplianceSubType = "TXC";
			setting3.LedgerType = "AP";

			return reportConfigurations;
		}

		#endregion

		public void TestCanDelete()
		{
			ComplianceDocumentHeader.ADH_DocumentStatus = Core.Constants.ComplianceDocumentStatus.Added;
			Assert("Only ADD record can be deleted.", ComplianceDocumentHeader.CanDelete);

			ComplianceDocumentHeader.ADH_DocumentNumber = "AA000010001";
			Factory.Save();
			AssertEquals(Core.Constants.ComplianceDocumentStatus.NumberSet, ComplianceDocumentHeader.ADH_DocumentStatus);
			Assert("Only ADD record can be deleted.", !ComplianceDocumentHeader.CanDelete);
		}

		public virtual void TestSetComplianceDocumentNumber()
		{
			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence.XD_Code = "AAA";
			complianceSequence.XD_SequenceClass = "NTC";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 1;
			complianceSequence.XD_EndNumber = 99;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_IsActive = true;
			complianceSequence.XD_ExpiryDate = new ZDateTime(2012, 11, 11);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			new ComplianceDocumentCreator(new InvoicingBase[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.NotRollup).CreateComplianceDocumentRecords();

			var header = Factory.Load<ARComplianceDocumentHeader>(new ZQuery())[0];
			header.ADH_DocumentDate = new ZDateTime(2012, 11, 11, 1, 1, 1);
			header.ADH_ComplianceSubType = "NTC";
			header.ADH_DocumentNumber = ZString.Empty;
			header.ADH_XD_ComplianceBook = complianceSequence.PK;
			header.ADH_ReportingPeriod = 201211;

			header.SetComplianceDocumentNumber();
			Factory.Save();

			AssertEquals("abc-0010025", header.ADH_DocumentNumber);
		}

		public void TestLedger()
		{
			AssertEquals("Ledger", ComplianceDocumentHeader.ADH_Ledger, ComplianceRule.Ledger);
		}

		public void TestComplianceRuleTransactionType()
		{
			AssertEquals("TransactionType", TransactionTypes.Invoice, ComplianceRule.TransactionType);
		}

		public void TestIsAmendingTransaction()
		{
			AssertEquals("IsAmendingTransaction", ZBool.False, ComplianceRule.IsAmendingTransaction);
		}

		public void TestIsReversalTransaction()
		{
			AssertEquals("IsReversalTransaction", ZBool.False, ComplianceRule.IsReversalTransaction);
		}

		public void TestIsDisbursementOrFinal()
		{
			AssertEquals("IsDisbursementOrFinal", ZBool.False, ComplianceRule.IsDisbursementOrFinal);
		}

		public void TestIsSelfBillingInvoice()
		{
			AssertEquals("IsSelfBillingInvoice", ZBool.False, ComplianceRule.IsSelfBillingInvoice);
		}

		public void TestHeader()
		{
			AssertEquals("Header", ZBool.True, ComplianceRule.Header.PK.IsValid);
		}

		public void TestCompany()
		{
			AssertEquals("Company", GlbCompany.CurrentCompany.PK, ComplianceRule.Company.PK);
		}

		public void TestLines()
		{
			AssertEquals("Lines", 3, ComplianceRule.Lines.Count());
		}

		public void TestEmptyLedgerMatchesAll()
		{
			var complianceDocumentHeader = Factory.NewWithValidTestData(typeof(ARComplianceDocumentHeader)) as IEvaluateComplianceRule;
			Assert(complianceDocumentHeader.EmptyLedgerMatchesAll);
		}

		public void TestEmptyTransactionTypeMatchesAll()
		{
			var complianceDocumentHeader = Factory.NewWithValidTestData(typeof(ARComplianceDocumentHeader)) as IEvaluateComplianceRule;
			Assert(complianceDocumentHeader.EmptyTransactionTypeMatchesAll);
		}

		public void TestComplianceSubType()
		{
			var complianceDocumentHeader = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			var supporter = complianceDocumentHeader as IEvaluateComplianceRule;
			AssertEquals(complianceDocumentHeader.ADH_ComplianceSubType, supporter.ComplianceSubType);
		}

		public void TestLocalTotalAmount()
		{
			AssertEquals("LocalTotalAmount", 180M, ComplianceRule.LocalTotalAmount);
		}

		public void TestIEvaluateComplianceRuleTaxTransactions()
		{
			AssertNotNull(ComplianceRule.TaxTransactions);
			Assert(!ComplianceRule.TaxTransactions.Any());

			var creator = new TaxFrameworkTestObjectCreator(Factory);
			creator.CreateTaxSystem("TS");
			var taxTransaction1 = creator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters() { TransactionHeader = ComplianceDocumentHeader.TransactionHeaders[0] });
			var taxTransaction2 = creator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters() { TransactionHeader = ComplianceDocumentHeader.TransactionHeaders[0] });

			AssertNotNull(ComplianceRule.TaxTransactions);
			Assert("Transaction level Tax Transactions are currently ignored", !ComplianceRule.TaxTransactions.Any());
		}

		public void TestIComplianceRuleParentTransaction()
		{
			var arInv = testObjectCreator.CreateARInvoice<ARInvoice>("INV001", testObjectCreator.AUD, 1m, testObjectCreator.Debtor);
			var arInvLine1 = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine1.AL_AG = testObjectCreator.GLHeader1.PK;
			var arInvLine2 = (ARInvoiceLine)arInv.Lines.AddNew();
			arInvLine2.AL_AG = testObjectCreator.GLHeader2.PK;
			var invoiceDocumentHeader1 = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "header1", "TXE0001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "line1", arInvLine1, testObjectCreator.Debtor);
			var invoiceDocumentHeader2 = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "header2", "TXC0002", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, "line2", arInvLine2, testObjectCreator.Debtor);
			Factory.Save();

			var arCreditNote = (ARCreditNote)(testObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInv)).amendTransaction;
			var line = arCreditNote.Lines.Cast<InvoicingLineBase>().First(x => x.AL_AG == testObjectCreator.GLHeader2.PK);
			var arCreditNoteDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "header3", "TXC0002", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "line3", line, testObjectCreator.Debtor);

			var complianceRuleParentTransaction = arCreditNoteDocumentHeader as IComplianceRuleParentTransaction;
			AssertNotNull("Should implement IComplianceRuleParentTransaction interface.", complianceRuleParentTransaction);

			var parentTransaction = complianceRuleParentTransaction.ParentTransaction;
			AssertNotNull("Should use compliance number to determine its parent compliance document.", parentTransaction);
			AssertEquals(TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, parentTransaction.ComplianceSubType);

			var parentTransactionAsDocument = parentTransaction as AccComplianceDocumentHeader;
			AssertEquals(invoiceDocumentHeader2.PK, parentTransactionAsDocument.PK);
		}

		public void TestVoid()
		{
			var complianceDocumentHeader = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			AssertEquals(ComplianceDocumentStatus.Added, complianceDocumentHeader.ADH_DocumentStatus);

			complianceDocumentHeader.ADH_Ledger = LedgerTypes.AccountsReceivable;
			complianceDocumentHeader.ADH_DocumentNumber = "TX00010001";
			Factory.Save();
			AssertEquals(ComplianceDocumentStatus.NumberSet, complianceDocumentHeader.ADH_DocumentStatus);

			complianceDocumentHeader.Void();
			AssertEquals(ComplianceDocumentStatus.Voided, complianceDocumentHeader.ADH_DocumentStatus);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			Assert(complianceDocumentHeader.Logs.HasLogWith(query));
		}

		public void TestFinalise()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = "TXT";
			report.ACR_DateFrom = new ZDate(2018, 7, 1);
			report.ACR_DateTo = new ZDate(2018, 7, 31);
			Factory.Save();

			var complianceDocumentHeader = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			complianceDocumentHeader.ADH_Ledger = LedgerTypes.AccountsReceivable;
			complianceDocumentHeader.ADH_DocumentNumber = "TX00010001";
			complianceDocumentHeader.Finalise(report);
			Factory.Save();

			AssertEquals(ComplianceDocumentStatus.Finalised, complianceDocumentHeader.ADH_DocumentStatus);
			AssertEquals(2, complianceDocumentHeader.Logs.GetAllLogs().Count);
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			Assert(complianceDocumentHeader.Logs.HasLogWith(query));
			var msg = complianceDocumentHeader.Logs.GetAllLogs().First(x => (x as StmALog).SL_SE_NKEvent == Events.EditedARecord.Code) as StmALog;
			AssertEquals("Compliance Document Finalized with Compliance Report <TXT>", msg.SL_Reference);

			complianceDocumentHeader.Finalise(report);
			Factory.Save();

			AssertEquals("Should not add finalised log when already finalised.", 2, complianceDocumentHeader.Logs.GetAllLogs().Count);
		}

		public void TestFinalise_IsVoided()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = "TXT";
			report.ACR_DateFrom = new ZDate(2018, 7, 1);
			report.ACR_DateTo = new ZDate(2018, 7, 31);
			Factory.Save();

			var complianceDocumentHeader = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			complianceDocumentHeader.ADH_Ledger = LedgerTypes.AccountsReceivable;
			complianceDocumentHeader.ADH_DocumentNumber = "TX00010001";
			Factory.Save();

			complianceDocumentHeader.Void();
			Factory.Save();

			AssertEquals(ComplianceDocumentStatus.Voided, complianceDocumentHeader.ADH_DocumentStatus);

			complianceDocumentHeader.Finalise(report);
			Factory.Save();

			AssertEquals(ComplianceDocumentStatus.Voided, complianceDocumentHeader.ADH_DocumentStatus);
		}

		public void TestRelatedBusinessObjectsAfterVoid()
		{
			var complianceDocumentHeader = Factory.Load<AccComplianceDocumentHeader>(ComplianceDocumentHeader.PK);
			Assert(complianceDocumentHeader.ComplianceDocumentLines.Count > 0);
			var complianceDocumentLine = complianceDocumentHeader.ComplianceDocumentLines[0];

			complianceDocumentHeader.Void();
			AssertEquals(ComplianceDocumentStatus.Voided, complianceDocumentHeader.ADH_DocumentStatus);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			complianceDocumentLine = factory.Load<AccComplianceDocumentLine>(complianceDocumentLine.PK);
			AssertNotNull("compliance document line is not deleted after void", complianceDocumentLine);

			var pivotQuery = new ZQuery(AccComplianceDocumentPivotSchema.ADP_ADL, complianceDocumentLine.PK);
			var complianceDocumentPivot = factory.LoadTop1<AccComplianceDocumentPivot>(pivotQuery);
			AssertNotNull("compliance document line pivot is not deleted after void", complianceDocumentPivot);
		}

		public void TestADH_BarCode()
		{
			var header = Factory.New<ARComplianceDocumentHeader>();
			AssertNullOrEmpty(header.ADH_BarCode);

			header.ADH_DocumentNumber = "00001";
			AssertNullOrEmpty(header.ADH_BarCode);

			header.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
			header.ADH_DocumentNumber = "00001";
			AssertNullOrEmpty(header.ADH_BarCode);

			header.ADH_Ledger = LedgerTypes.AccountsReceivable;
			header.ADH_DocumentNumber = "00001";
			AssertNullOrEmpty(header.ADH_BarCode);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				header.ADH_DocumentNumber = "00001";
				AssertNotNullOrEmpty(header.ADH_BarCode);
				var regex = new Regex("[0-9]{4}");
				Assert("ADH_BarCode should contain 4 digit number.", regex.IsMatch(header.ADH_BarCode));
			}
		}

		public void TestADH_DocumentStatus_CannotChangeStatusForVoidStatus()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = "TXT";
			report.ACR_DateFrom = new ZDate(2018, 7, 1);
			report.ACR_DateTo = new ZDate(2018, 7, 31);
			Factory.Save();

			var complianceDocumentHeader = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			complianceDocumentHeader.ADH_Ledger = LedgerTypes.AccountsReceivable;
			complianceDocumentHeader.ADH_DocumentNumber = "TX00010001";
			Factory.Save();

			complianceDocumentHeader.Void();
			Factory.Save();

			AssertEquals("VOD", complianceDocumentHeader.ADH_DocumentStatus);

			complianceDocumentHeader.ADH_DocumentStatus = "ADD";

			AssertNotEquals("ADD", complianceDocumentHeader.ADH_DocumentStatus);
			AssertEquals("VOD", complianceDocumentHeader.ADH_DocumentStatus);
		}

		IEvaluateComplianceRule ComplianceRule => Factory.Load<AccComplianceDocumentHeader>(ComplianceDocumentHeader.PK);
		#endregion
	}
}

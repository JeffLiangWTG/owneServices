using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccComplianceDocumentHeaderValidationTest : BusinessObjectValidationTestCase
	{
		protected AccComplianceDocumentHeader complianceDocumentHeader;
		protected AccComplianceDocumentHeader complianceDocumentHeader2;
		protected ARComplianceDocumentHeader arComplianceDocumentHeader;
		protected APComplianceDocumentHeader apComplianceDocumentHeader;
		protected TestObjectCreator testObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();
			testObjectCreator = new TestObjectCreator(Factory);

			var helper = new AccountingPeriodTestHelper(Factory);
			var period = helper.SetupSinglePeriod(201802, new ZDateTime(2018, 2, 1), new ZDateTime(2018, 2, 28));
			helper.SetupSinglePeriod(201803, new ZDateTime(2018, 3, 1), new ZDateTime(2018, 3, 31));
			period.AM_IsSubLedgerClosed = true;

			var arCrd = testObjectCreator.CreateARCreditNote("CRD001", testObjectCreator.Debtor, testObjectCreator.AUD, 1m, "");
			arCrd.AH_OH = testObjectCreator.Debtor.PK;
			var arCrdLine = (ARCreditNoteLine)arCrd.Lines.AddNew();
			arCrdLine.AL_AG = testObjectCreator.GLHeader1.PK;
			complianceDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "desc", "TX00010001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", arCrdLine);
			complianceDocumentHeader.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			complianceDocumentHeader.ADH_ReportingPeriod = 201802;

			var apCrd = testObjectCreator.CreateAPCreditNote("CRD002", testObjectCreator.Debtor, testObjectCreator.AUD, 1m, "");
			apCrd.AH_OH = testObjectCreator.Debtor.PK;
			var apCrdLine = (APCreditNoteLine)apCrd.Lines.AddNew();
			apCrdLine.AL_AG = testObjectCreator.GLHeader1.PK;
			complianceDocumentHeader2 = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00010002", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", apCrdLine);
			complianceDocumentHeader2.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			complianceDocumentHeader2.ADH_ReportingPeriod = 201802;

			Factory.Save();

			arComplianceDocumentHeader = Factory.Load<ARComplianceDocumentHeader>(complianceDocumentHeader.PK);
			apComplianceDocumentHeader = Factory.Load<APComplianceDocumentHeader>(complianceDocumentHeader2.PK);
		}

		public void TestCheckADH_SupportingDocumentNumber()
		{
			complianceDocumentHeader.ADH_SupportingDocumentNumber = "123";
			Assert(complianceDocumentHeader.ADH_SupportingDocumentNumberInfo.HasError("The Supporting Document Number should have a Supporting Document Type."));

			complianceDocumentHeader.ADH_SupportingDocumentType = "xx";
			complianceDocumentHeader.ADH_SupportingDocumentNumber = "123";
			AssertNoErrors(complianceDocumentHeader.ADH_SupportingDocumentNumberInfo);
		}

		public void TestCheckADH_XD_ComplianceBook()
		{
			var book = Factory.NewWithValidTestData<AccComplianceSequence>();
			book.XD_StartDate = new ZDate(2019, 4, 10);
			book.XD_ExpiryDate = new ZDate(2019, 4, 15);
			Factory.Save();

			complianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			complianceDocumentHeader.ADH_DocumentDate = new ZDate(2019, 4, 9);
			complianceDocumentHeader.ADH_XD_ComplianceBook = book.PK;

			Assert(complianceDocumentHeader.ADH_XD_ComplianceBookInfo.HasError("The compliance book is inactive or expired."));

			complianceDocumentHeader.ADH_DocumentDate = new ZDate(2019, 4, 15);
			complianceDocumentHeader.ADH_XD_ComplianceBook = book.PK;
			Assert(!complianceDocumentHeader.ADH_XD_ComplianceBookInfo.HasError("The compliance book is inactive or expired."));

			complianceDocumentHeader.ADH_DocumentDate = new ZDate(2019, 4, 16);
			complianceDocumentHeader.ADH_XD_ComplianceBook = book.PK;
			Assert(complianceDocumentHeader.ADH_XD_ComplianceBookInfo.HasError("The compliance book is inactive or expired."));
		}

		public void TestADH_ReportingPeriod()
		{
			complianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			complianceDocumentHeader.ADH_ReportingPeriod = 201803;

			AssertNoErrors(complianceDocumentHeader.ADH_ReportingPeriodInfo);
			complianceDocumentHeader.ADH_ReportingPeriod = 201805;
			Assert(complianceDocumentHeader.ADH_ReportingPeriodInfo.HasError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(201805)));

			complianceDocumentHeader.ADH_Ledger = LedgerTypes.AccountsPayable;
			complianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			complianceDocumentHeader.ADH_ReportingPeriod = 201802;
			Assert(complianceDocumentHeader.ADH_ReportingPeriodInfo.HasError("This date falls into a period where the sub-ledger is closed"));
		}

		public void TestCheckADH_Description()
		{
			complianceDocumentHeader.ADH_Description = "Dummy Desc";
			AssertNoErrors(complianceDocumentHeader.ADH_DescriptionInfo);

			complianceDocumentHeader.ADH_Description = string.Empty;
			Assert(complianceDocumentHeader.ADH_DescriptionInfo.HasError("Please enter a value."));
		}

		public void TestCheckADH_ReportingPeriodFallsInFalisedReport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				complianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
				complianceDocumentHeader.ADH_ReportingPeriod = 201803;
				AssertNoErrors(complianceDocumentHeader.ADH_ReportingPeriodInfo);

				var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				var reportConfig = complianceConfig.AddNew();
				reportConfig.ReportCode = "TST";
				reportConfig.ReportTitle = "Test Tax Report";
				reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange;
				reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				reportConfig.TaxRegistrationType = "APC";
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;

				var setting1 = reportConfig.Settings.AddNew();
				setting1.ComplianceSubType = "TCR";
				setting1.LedgerType = LedgerTypes.AccountsReceivable;

				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

				var report = Factory.New<AccComplianceReport>();
				report.ACR_ReportType = "TST";
				report.ACR_DateFrom = new ZDate(2018, 3, 1);
				report.ACR_DateTo = new ZDate(2018, 3, 31);
				report.ACR_Status = AccComplianceReport.Status.ReportFinalised;

				complianceDocumentHeader.ADH_ComplianceSubType = "TCR";
				complianceDocumentHeader.ADH_ReportingPeriod = 201803;
				Assert(complianceDocumentHeader.ADH_ReportingPeriodInfo.HasError("The reporting period falls in a compliance report that has been finalized."));
			}
		}

		public void TestCheckADH_DocumentDate()
		{
			arComplianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			arComplianceDocumentHeader.ADH_Ledger = LedgerTypes.AccountsReceivable;
			var future = ZDateTime.Today.AddDays(1);
			arComplianceDocumentHeader.ADH_DocumentDate = future;
			Assert(arComplianceDocumentHeader.ADH_DocumentDateInfo.HasError("This date cannot be in the future."));

			apComplianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			apComplianceDocumentHeader.ADH_Ledger = LedgerTypes.AccountsPayable;
			apComplianceDocumentHeader.ADH_DocumentDate = future;
			Assert(apComplianceDocumentHeader.ADH_DocumentDateInfo.HasError("This date cannot be in the future."));

			arComplianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			arComplianceDocumentHeader.ADH_Ledger = LedgerTypes.AccountsReceivable;
			arComplianceDocumentHeader.ADH_DocumentDate = new ZDateTime(2018, 2, 5);
			Assert(arComplianceDocumentHeader.ADH_DocumentDateInfo.HasError("This date falls into a period where the sub-ledger is closed"));

			apComplianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			apComplianceDocumentHeader.ADH_Ledger = LedgerTypes.AccountsPayable;
			var lastYear = ZDateTime.Today.AddYears(-1);
			apComplianceDocumentHeader.ADH_DocumentDate = lastYear;
			apComplianceDocumentHeader.ADH_DocumentDateInfo.HasWarning($"The date {lastYear} is more than 1 year old");
		}

		public void TestCheckADH_OH_Organisation()
		{
			complianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Voided;
			complianceDocumentHeader.ADH_OH_Organisation = ZGuid.Empty;
			Assert(!complianceDocumentHeader.ADH_DescriptionInfo.HasError("Please enter a value."));

			complianceDocumentHeader.ADH_DocumentStatus = ComplianceDocumentStatus.Added;
			complianceDocumentHeader.ADH_OH_Organisation = ZGuid.Empty;
			Assert(!complianceDocumentHeader.ADH_DescriptionInfo.HasError("Please enter a value."));
		}

		public void TestCheckADH_DocumentNumber()
		{
			var oldComplianceHeaders = Factory.Load<ARComplianceDocumentHeader>(new ZQuery());
			var query = new ZQuery();
			foreach (var header in oldComplianceHeaders)
			{
				query.AddToFilter(AccComplianceDocumentHeaderSchema.PK, SQLComparisonOperator.NotEqual, header.PK);
			}

			var testObjectCreater = new TestObjectCreator(Factory);
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_OH = testObjectCreater.AALSHI.PK;
			var line = arInvoice.Lines.AddNew() as InvoicingLineBase;
			line.AL_AC = testObjectCreater.CC1.PK;
			line.AL_OSExTaxAmount = 100m;
			line.AL_AT = testObjectCreater.GST1.PK;
			arInvoice.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "12345675", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();

			new ComplianceDocumentCreator(new[] { arInvoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();

			var invComplianceDocumentHeaders = Factory.Load<ARComplianceDocumentHeader>(query);
			AssertEquals("Prevalidation", 1, invComplianceDocumentHeaders.Length);
			invComplianceDocumentHeaders[0].ADH_DocumentNumber = "TX00000010";

			Factory.Save();

			var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			arCreditNote.AH_OH = arInvoice.AH_OH;
			arCreditNote.OriginalTransactionReference = arInvoice.PK;

			new ComplianceDocumentCreator(new[] { arCreditNote }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
			query.AddToFilter(AccComplianceDocumentHeaderSchema.PK, SQLComparisonOperator.NotEqual, invComplianceDocumentHeaders[0].PK);
			query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.CreditNote);
			var creditComplianceDocumentHeaders = Factory.Load<ARComplianceDocumentHeader>(query);

			AssertEquals("Prevalidation", 1, creditComplianceDocumentHeaders.Length);

			AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			creditComplianceDocumentHeaders[0].ADH_DocumentNumber = "TX00000020";
			Factory.Save();

			AssertHasError(creditComplianceDocumentHeaders[0].ADH_DocumentNumberInfo, "Document Number does not match any 'INV' Compliance Document recorded against the Debtor.");

			creditComplianceDocumentHeaders[0].ADH_DocumentNumber = "TX00000010";
			AssertNoErrors(creditComplianceDocumentHeaders[0].ADH_DocumentNumberInfo);

			var apInv = testObjectCreator.CreateAPInvoice<APInvoice>("INV001", testObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, testObjectCreator.Debtor);
			var apInvLine = (APInvoiceLine)apInv.Lines.AddNew();
			apInvLine.AL_AG = testObjectCreator.GLHeader1.PK;
			var invoiceDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00090001", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", apInvLine);
			invoiceDocumentHeader.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			invoiceDocumentHeader.ADH_ReportingPeriod = 201802;

			Factory.Save();

			var apInv1 = testObjectCreator.CreateAPInvoice<APInvoice>("INV002", testObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, testObjectCreator.Debtor);
			var apInvLine1 = (APInvoiceLine)apInv1.Lines.AddNew();
			apInvLine1.AL_AG = testObjectCreator.GLHeader1.PK;
			var invoiceDocumentHeader1 = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "", TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, "desc", apInvLine1);
			invoiceDocumentHeader1.ADH_OH_Organisation = testObjectCreator.Debtor.PK;
			invoiceDocumentHeader1.ADH_ReportingPeriod = 201802;

			Factory.Save();

			var apInvoiceDocumentHeader = Factory.Load<APComplianceDocumentHeader>(invoiceDocumentHeader1.PK);

			Assert(!apInvoiceDocumentHeader.ADH_DocumentNumberInfo.HasError("This Compliance Document Number is already in use. Please enter another number."));
			apInvoiceDocumentHeader.ADH_DocumentNumber = "TX00090001";
			Assert(apInvoiceDocumentHeader.ADH_DocumentNumberInfo.HasError("This Compliance Document Number is already in use. Please enter another number."));

			var apCrd = testObjectCreator.CreateAPCreditNote("CRD001", testObjectCreator.AALSHI, testObjectCreator.AUD, 1m, "desc");
			var apCrdLine = (APCreditNoteLine)apCrd.Lines.AddNew();
			apCrdLine.AL_AG = testObjectCreator.GLHeader1.PK;

			var crdDocumentHeader = testObjectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsPayable, "desc", "TX00010002", TaiwanComplianceInfo.ComplianceSubTypeCodes.NTI, "desc", apCrdLine);
			crdDocumentHeader.ADH_OH_Organisation = testObjectCreator.AALSHI.PK;
			crdDocumentHeader.ADH_ReportingPeriod = 201802;
			crdDocumentHeader.ADH_DocumentDate = new ZDateTime(2018, 2, 2);
			Factory.Save();

			var crdDocumentHeader1 = Factory.Load<APComplianceDocumentHeader>(invoiceDocumentHeader1.PK);
			AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			crdDocumentHeader1.ADH_DocumentNumber = "TX00090001";
			Assert(crdDocumentHeader1.ADH_DocumentNumberInfo.HasError("This Compliance Document Number is already in use. Please enter another number."));
		}

		public void TestCheckADH_DocumentNumber_DifferentOrganization()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var testObjectCreater = new TestObjectCreator(Factory);
				var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
				arInvoice.AH_OH = testObjectCreater.AALSHI.PK;
				var line = arInvoice.Lines.AddNew() as InvoicingLineBase;
				line.AL_AC = testObjectCreater.CC1.PK;
				line.AL_OSExTaxAmount = 100m;
				line.AL_AT = testObjectCreater.GST1.PK;
				arInvoice.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "12345675", Core.Constants.CountryCodes.Taiwan);

				var arInvoiceWithDifferentOrg = Factory.NewWithValidTestData<ARInvoice>();
				arInvoiceWithDifferentOrg.AH_OH = testObjectCreater.ABIGAS.PK;
				line = arInvoiceWithDifferentOrg.Lines.AddNew() as InvoicingLineBase;
				line.AL_AC = testObjectCreater.CC1.PK;
				line.AL_OSExTaxAmount = 100m;
				line.AL_AT = testObjectCreater.GST1.PK;
				arInvoiceWithDifferentOrg.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "12345675", Core.Constants.CountryCodes.Taiwan);
				Factory.Save();

				var invComplianceDocumentHeader = (new ComplianceDocumentCreator(new[] { arInvoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords())[0];
				invComplianceDocumentHeader.ADH_DocumentNumber = "TX00000010";

				var invComplianceDocumentHeaderWithDifferentOrg = (new ComplianceDocumentCreator(new[] { arInvoiceWithDifferentOrg }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords())[0];
				invComplianceDocumentHeaderWithDifferentOrg.ADH_DocumentNumber = "TX00000011";

				Factory.Save();

				var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
				arCreditNote.AH_OH = arInvoice.AH_OH;
				arCreditNote.OriginalTransactionReference = arInvoice.PK;

				var creditComplianceDocumentHeader = (new ComplianceDocumentCreator(new[] { arCreditNote }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords())[0];

				AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				creditComplianceDocumentHeader.ADH_DocumentNumber = "TX00000011";
				AssertNotEquals(invComplianceDocumentHeaderWithDifferentOrg.ADH_OH_Organisation, creditComplianceDocumentHeader.ADH_OH_Organisation);
				AssertHasError(creditComplianceDocumentHeader.ADH_DocumentNumberInfo, "Document Number does not match any 'INV' Compliance Document recorded against the Debtor.");

				creditComplianceDocumentHeader.ADH_DocumentNumber = "TX00000010";
				AssertEquals(invComplianceDocumentHeader.ADH_OH_Organisation, creditComplianceDocumentHeader.ADH_OH_Organisation);
				AssertNoErrors(creditComplianceDocumentHeader.ADH_DocumentNumberInfo);
			}
		}

		public void TestCheckADH_VoidingReason()
		{
			complianceDocumentHeader.IsSpecialVoiding = true;
			complianceDocumentHeader.ADH_VoidingReason = "Test";
			AssertNoErrors(complianceDocumentHeader.ADH_VoidingReasonInfo);

			complianceDocumentHeader.ADH_VoidingReason = string.Empty;
			Assert(complianceDocumentHeader.ADH_VoidingReasonInfo.HasError("Please enter a value."));

			complianceDocumentHeader.ADH_VoidingReason = "Test";
			AssertNoErrors(complianceDocumentHeader.ADH_VoidingReasonInfo);

			complianceDocumentHeader.IsSpecialVoiding = false;
			complianceDocumentHeader.ADH_VoidingReason = "";
			AssertNoErrors(complianceDocumentHeader.ADH_VoidingReasonInfo);
		}
	}
}

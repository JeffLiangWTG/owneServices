using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	class KoreaSouthEInvoicingValidationTest : TestCaseWithFactory
	{
		[TestDate(2023, 3, 11)]
		public void TestValidateInvoiceLines()
		{
			var validation = new KoreaSouthEInvoicingValidation();

			var invoiceInDB = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV_INDB", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			Factory.Save();

			var invoiceNotEligible = GetValidateReverseDateCases().First(x => !x.ExpectedIsEligible).TransactionHeader;
			var invoiceWith100Lines = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var invoiceWith99Lines = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV2", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);

			for (var i = 0; i < 98; i++)
			{
				TestObjectCreator.CreateInvoiceLine(invoiceWith100Lines, TestObjectCreator.KRW, 1, 100m);
				TestObjectCreator.CreateInvoiceLine(invoiceWith99Lines, TestObjectCreator.KRW, 1, 100m);
				TestObjectCreator.CreateInvoiceLine(invoiceInDB, TestObjectCreator.KRW, 1, 100m);
				TestObjectCreator.CreateInvoiceLine((InvoicingBase)invoiceNotEligible, TestObjectCreator.KRW, 1, 100m);
			}
			TestObjectCreator.CreateInvoiceLine(invoiceWith100Lines, TestObjectCreator.KRW, 1, 100m);
			TestObjectCreator.CreateInvoiceLine(invoiceInDB, TestObjectCreator.KRW, 1, 100m);
			TestObjectCreator.CreateInvoiceLine((InvoicingBase)invoiceNotEligible, TestObjectCreator.KRW, 1, 100m);

			AssertEquals("Pre-condition", 100, invoiceWith100Lines.Lines.Count);
			AssertEquals("Pre-condition", 99, invoiceWith99Lines.Lines.Count);
			AssertEquals("Pre-condition", 100, invoiceInDB.Lines.Count);
			AssertEquals("Pre-condition", 100, invoiceNotEligible.Lines.Count);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
			{
				AssertEquals("Pre-condition", true, ElectronicInvoicingEligibilityDecider.IsEligible(invoiceWith100Lines));
				AssertEquals("Pre-condition", true, ElectronicInvoicingEligibilityDecider.IsEligible(invoiceWith99Lines));
				AssertEquals("Pre-condition", true, ElectronicInvoicingEligibilityDecider.IsEligible(invoiceInDB));
				AssertEquals("Pre-condition", false, ElectronicInvoicingEligibilityDecider.IsEligible(invoiceNotEligible));

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertNotNull("Should have errors", validation.ValidateInvoiceLines(invoiceWith100Lines.Lines.OfType<AccTransactionLines>()));
					AssertNull("Should have no errors when there is no more than 99 lines.", validation.ValidateInvoiceLines(invoiceWith99Lines.Lines.OfType<AccTransactionLines>()));
					AssertNull("Should have no errors when header is in DB.", validation.ValidateInvoiceLines(invoiceInDB.Lines.OfType<AccTransactionLines>()));
					AssertNull("Should have no errors when header is not eligible.", validation.ValidateInvoiceLines(invoiceNotEligible.Lines.OfType<AccTransactionLines>()));

					var linesOfDifferentHeader = new List<AccTransactionLines>();
					linesOfDifferentHeader.AddRange(invoiceWith100Lines.Lines.OfType<AccTransactionLines>());
					linesOfDifferentHeader.AddRange(invoiceWith99Lines.Lines.OfType<AccTransactionLines>());
					AssertNull("Should have no errors when lines are not all same header.", validation.ValidateInvoiceLines(linesOfDifferentHeader));
				}

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertNull("Should have no errors when E-Invoicing is not enabled.", validation.ValidateInvoiceLines(invoiceWith100Lines.Lines.OfType<AccTransactionLines>()));
					AssertNull("Should have no errors when E-Invoicing is not enabled.", validation.ValidateInvoiceLines(invoiceWith99Lines.Lines.OfType<AccTransactionLines>()));
				}
			}
		}

		[TestDate(2023, 3, 3)]
		public void TestValidateReverseDate()
		{
			TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true);
			AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddMonths(-2).ToDateTime());
			AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued);

			var testCases = GetValidateReverseDateCases();
			var validation = new KoreaSouthEInvoicingValidation();

			foreach (var testCase in testCases)
			{
				CombineAssertions(testCase.AssertComment, () => {
					AssertEquals(testCase.ExpectedIsEligible, ElectronicInvoicingEligibilityDecider.IsEligible(testCase.TransactionHeader));
					AssertEquals(testCase.ExpectedErrorMsg, validation.ValidateReverseDate(testCase.TransactionHeader.Lines[0]));
				});
			}

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, false))
			{
				foreach (var testCase in testCases)
				{
					AssertNull("Validation should not work when EInvoicing is disabled.", validation.ValidateReverseDate(testCase.TransactionHeader.Lines[0]));
				}
			}
		}

		IEnumerable<ValidateReverseDateScenario> GetValidateReverseDateCases()
		{
			var futureInvoiceDate = new ZDateTime(2023, 4, 11);
			var futureSameMonthInvoiceDate = new ZDateTime(2023, 3, 11);
			var today = new ZDateTime(2023, 3, 03);

			var invoiceAR_EReporting = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			testObjectCreator.CreateEInvoicingTransactionPivot(invoiceAR_EReporting);
			UpdateInvoiceDateAndLineRecognitionDate(invoiceAR_EReporting,
				invoiceDate: futureInvoiceDate,
				recognitionDate: today
			);
			invoiceAR_EReporting.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", invoiceAR_EReporting.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
			AssertEquals("Precondition", true, invoiceAR_EReporting is InvoicingBase);

			var invoiceAR_DateInSameMonth = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			UpdateInvoiceDateAndLineRecognitionDate(invoiceAR_DateInSameMonth,
				invoiceDate: futureSameMonthInvoiceDate,
				recognitionDate: today
			);
			invoiceAR_DateInSameMonth.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", invoiceAR_DateInSameMonth.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
			AssertEquals("Precondition", true, invoiceAR_EReporting is InvoicingBase);

			var creditNoteWithEReportingOriginalReference = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "AR_CRD1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			UpdateInvoiceDateAndLineRecognitionDate(creditNoteWithEReportingOriginalReference,
				invoiceDate: futureInvoiceDate,
				recognitionDate: today
			);
			creditNoteWithEReportingOriginalReference.AH_TransactionBelongsToGroup = invoiceAR_EReporting.PK;
			creditNoteWithEReportingOriginalReference.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", creditNoteWithEReportingOriginalReference.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var creditNoteWithoutEReportingOriginaOriginalReference = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "AR_CRD2", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			UpdateInvoiceDateAndLineRecognitionDate(creditNoteWithoutEReportingOriginaOriginalReference,
				invoiceDate: futureInvoiceDate,
				recognitionDate: today
			);
			creditNoteWithoutEReportingOriginaOriginalReference.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", creditNoteWithoutEReportingOriginaOriginalReference.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var reveredEReportingInvoiceAR = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV2", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			testObjectCreator.CreateEInvoicingTransactionPivot(reveredEReportingInvoiceAR);
			UpdateInvoiceDateAndLineRecognitionDate(reveredEReportingInvoiceAR,
				invoiceDate: futureInvoiceDate,
				recognitionDate: today
			);
			reveredEReportingInvoiceAR.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", reveredEReportingInvoiceAR.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var reveringEReportingInvoiceAR = TestObjectCreator.ReverseTransaction(reveredEReportingInvoiceAR, out var errorMsgReveringEReportingInvoiceAR) as TransactionHeaderWithLines;
			AssertNullOrEmpty("Precondition", errorMsgReveringEReportingInvoiceAR);
			AssertNotNull("Precondition", reveringEReportingInvoiceAR);
			UpdateInvoiceDateAndLineRecognitionDate(reveringEReportingInvoiceAR,
				invoiceDate: futureInvoiceDate,
				recognitionDate: today
			);

			var reveredInvoiceAR = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV3", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			UpdateInvoiceDateAndLineRecognitionDate(reveredInvoiceAR,
				invoiceDate: futureInvoiceDate,
				recognitionDate: today
			);
			reveredInvoiceAR.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", reveredInvoiceAR.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var reveringInvoiceAR = TestObjectCreator.ReverseTransaction(reveredInvoiceAR, out var errorMsgReveringInvoiceAR) as TransactionHeaderWithLines;
			AssertNullOrEmpty("Precondition", errorMsgReveringInvoiceAR);
			AssertNotNull("Precondition", reveringInvoiceAR);
			UpdateInvoiceDateAndLineRecognitionDate(reveringInvoiceAR,
				invoiceDate: futureInvoiceDate,
				recognitionDate: today
			);
			reveringInvoiceAR.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", reveringInvoiceAR.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var amendingEReportingARInvoiceResult = TestObjectCreator.AmendARTransaction(TransactionTypes.Invoice, invoiceAR_EReporting);
			AssertNullOrEmpty("Precondition", amendingEReportingARInvoiceResult.errorMessage);
			AssertNotNull("Precondition", amendingEReportingARInvoiceResult.amendTransaction is TransactionHeaderWithLines);
			var amendingEReportingARInvoice = amendingEReportingARInvoiceResult.amendTransaction as TransactionHeaderWithLines;
			UpdateInvoiceDateAndLineRecognitionDate(amendingEReportingARInvoice,
				invoiceDate: futureInvoiceDate,
				recognitionDate: today
			);
			amendingEReportingARInvoice.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", amendingEReportingARInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var amendingEReportingARCreditNoteResult = TestObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, invoiceAR_EReporting);
			AssertNullOrEmpty("Precondition", amendingEReportingARCreditNoteResult.errorMessage);
			AssertNotNull("Precondition", amendingEReportingARCreditNoteResult.amendTransaction is TransactionHeaderWithLines);
			var amendingEReportingARCreditNote = amendingEReportingARCreditNoteResult.amendTransaction as TransactionHeaderWithLines;
			UpdateInvoiceDateAndLineRecognitionDate(amendingEReportingARCreditNote,
				invoiceDate: futureInvoiceDate,
				recognitionDate: today
			);
			amendingEReportingARCreditNote.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", amendingEReportingARCreditNote.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var invoiceAR_NonEReporting = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV4", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			UpdateInvoiceDateAndLineRecognitionDate(invoiceAR_NonEReporting,
				invoiceDate: futureInvoiceDate,
				recognitionDate: today
			);
			invoiceAR_NonEReporting.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", invoiceAR_NonEReporting.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
			AssertEquals("Precondition", true, invoiceAR_NonEReporting is InvoicingBase);

			var amendingNonEReportingARInvoiceResult = TestObjectCreator.AmendARTransaction(TransactionTypes.Invoice, invoiceAR_NonEReporting);
			AssertNullOrEmpty("Precondition", amendingNonEReportingARInvoiceResult.errorMessage);
			AssertNotNull("Precondition", amendingNonEReportingARInvoiceResult.amendTransaction is TransactionHeaderWithLines);
			var amendingNonEReportingARInvoice = amendingNonEReportingARInvoiceResult.amendTransaction as TransactionHeaderWithLines;
			UpdateInvoiceDateAndLineRecognitionDate(amendingNonEReportingARInvoice,
				invoiceDate: futureInvoiceDate,
				recognitionDate: today
			);
			amendingNonEReportingARInvoice.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", amendingNonEReportingARInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var amendingNonEReportingARCreditNoteResult = TestObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, invoiceAR_NonEReporting);
			AssertNullOrEmpty("Precondition", amendingNonEReportingARCreditNoteResult.errorMessage);
			AssertNotNull("Precondition", amendingNonEReportingARCreditNoteResult.amendTransaction is TransactionHeaderWithLines);
			var amendingNonEReportingARCreditNote = amendingNonEReportingARCreditNoteResult.amendTransaction as TransactionHeaderWithLines;
			UpdateInvoiceDateAndLineRecognitionDate(amendingNonEReportingARCreditNote,
				invoiceDate: futureInvoiceDate,
				recognitionDate: today
			);
			amendingNonEReportingARCreditNote.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", amendingNonEReportingARCreditNote.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			return new[] {
				new ValidateReverseDateScenario {
					AssertComment = "Should run validation When AR invoice.(is eligible)",
					TransactionHeader = invoiceAR_EReporting,
					ExpectedErrorMsg = "The invoice date and revenue recognition date must be in the same calendar month. Current Invoice Date: 11-Apr-23. Current Revenue Recognition Date: 03-Mar-23",
					ExpectedIsEligible = true
				},
				new ValidateReverseDateScenario {
					AssertComment = "Should run and pass validation When AR invoice have invoice date & recognition date in same month.(is eligible)",
					TransactionHeader = invoiceAR_DateInSameMonth,
					ExpectedErrorMsg = null,
					ExpectedIsEligible = true
				},
				new ValidateReverseDateScenario {
					AssertComment = "Should run validation When AR invoice.(is eligible)",
					TransactionHeader = invoiceAR_NonEReporting,
					ExpectedErrorMsg = "The invoice date and revenue recognition date must be in the same calendar month. Current Invoice Date: 11-Apr-23. Current Revenue Recognition Date: 03-Mar-23",
					ExpectedIsEligible = true
				},
				new ValidateReverseDateScenario {
					AssertComment = "Should run validation When AR invoice.(is eligible)",
					TransactionHeader = reveredEReportingInvoiceAR,
					ExpectedErrorMsg = "The invoice date and revenue recognition date must be in the same calendar month. Current Invoice Date: 11-Apr-23. Current Revenue Recognition Date: 03-Mar-23",
					ExpectedIsEligible = true
				},
				new ValidateReverseDateScenario {
					AssertComment = "Should run validation When AR invoice.(is eligible)",
					TransactionHeader = reveredInvoiceAR,
					ExpectedErrorMsg = "The invoice date and revenue recognition date must be in the same calendar month. Current Invoice Date: 11-Apr-23. Current Revenue Recognition Date: 03-Mar-23",
					ExpectedIsEligible = true
				},
				new ValidateReverseDateScenario {
					AssertComment = "Should not run validation When AR invoice is revering E-Reporting Invoice.(is eligible)",
					TransactionHeader = reveringEReportingInvoiceAR,
					ExpectedErrorMsg = null,
					ExpectedIsEligible = true
				},
				new ValidateReverseDateScenario {
					AssertComment = "Should not run validation When AR invoice is revering non E-Reporting Invoice.(not eligible)",
					TransactionHeader = reveringInvoiceAR,
					ExpectedErrorMsg = null,
					ExpectedIsEligible = false
				},
				new ValidateReverseDateScenario {
					AssertComment = "Should not run validation When creditNote is related to original transaction header.(is eligible)",
					TransactionHeader = creditNoteWithEReportingOriginalReference,
					ExpectedErrorMsg = null,
					ExpectedIsEligible = true
				},
				new ValidateReverseDateScenario {
					AssertComment = "Should not run validation When creditNote is not related to original transaction header.(not eligible)",
					TransactionHeader = creditNoteWithoutEReportingOriginaOriginalReference,
					ExpectedErrorMsg = null,
					ExpectedIsEligible = false
				},
				new ValidateReverseDateScenario {
					AssertComment = "Should not run validation When amending AR Invoice which the original reference is E-Reporting enabled.(is eligible)",
					TransactionHeader = amendingEReportingARInvoice,
					ExpectedErrorMsg = null,
					ExpectedIsEligible = true
				},
				new ValidateReverseDateScenario {
					AssertComment = "Should not run validation When amending AR creditNote which the original reference is E-Reporting enabled.(is eligible)",
					TransactionHeader = amendingEReportingARCreditNote,
					ExpectedErrorMsg = null,
					ExpectedIsEligible = true
				},
				new ValidateReverseDateScenario {
					AssertComment = "Should not run validation When amending AR Invoice which the original reference is E-Reporting disabled.(is eligible)",
					TransactionHeader = amendingNonEReportingARInvoice,
					ExpectedErrorMsg = null,
					ExpectedIsEligible = true
				},
				new ValidateReverseDateScenario {
					AssertComment = "Should not run validation when amending AR creditNote which the original reference is E-Reporting disabled.(not eligible)",
					TransactionHeader = amendingNonEReportingARCreditNote,
					ExpectedErrorMsg = null,
					ExpectedIsEligible = false
				}
			};
		}

		[TestDate(2023, 3, 11)]
		public void TestValidateReverseDate_SkipWhenReverseDateNotChangeAndInDB()
		{
			var invoiceDateInNextMonth = new ZDateTime(2023, 4, 11);
			var recognitionDate = new ZDateTime(2023, 3, 11);

			var invoiceInDB = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			TestObjectCreator.CreateEInvoicingTransactionPivot(invoiceInDB);
			UpdateInvoiceDateAndLineRecognitionDate(invoiceInDB,
				invoiceDate: invoiceDateInNextMonth,
				recognitionDate: recognitionDate
			);
			invoiceInDB.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", invoiceInDB.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
			Factory.Save();

			var invoiceReadyToPost = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			TestObjectCreator.CreateEInvoicingTransactionPivot(invoiceReadyToPost);
			UpdateInvoiceDateAndLineRecognitionDate(invoiceReadyToPost,
				invoiceDate: invoiceDateInNextMonth,
				recognitionDate: recognitionDate
			);
			invoiceReadyToPost.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", invoiceReadyToPost.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today.AddDays(-10)))
			{
				var validation = new KoreaSouthEInvoicingValidation();

				AssertEquals("Precondition", false, invoiceReadyToPost.IsInDatabase);
				foreach (AccTransactionLines line in invoiceReadyToPost.Lines)
				{
					AssertEquals("Precondition", false, invoiceReadyToPost.AH_InvoiceDateInfo.HasChanges);
					AssertEquals("Precondition", false, line.AL_ReverseDateInfo.HasChanges);
					AssertEquals("Validation should be triggered when posting transaction.(transaction is not in DB).",
						"The invoice date and revenue recognition date must be in the same calendar month. Current Invoice Date: 11-Apr-23. Current Revenue Recognition Date: 11-Mar-23",
						validation.ValidateReverseDate(line)
					);
				}

				Assert("Precondition", ElectronicInvoicingEligibilityDecider.IsEligible(invoiceInDB));
				foreach (AccTransactionLines line in invoiceInDB.Lines)
				{
					AssertEquals("Precondition", false, invoiceInDB.AH_InvoiceDateInfo.HasChanges);
					AssertEquals("Precondition", false, line.AL_ReverseDateInfo.HasChanges);
					AssertNullOrEmpty("Validation should not be triggered when transaction line had been posted.", validation.ValidateReverseDate(line));
				}

				UpdateInvoiceDateAndLineRecognitionDate(invoiceInDB,
					invoiceDate: invoiceDateInNextMonth.AddDays(1),
					recognitionDate: recognitionDate
				);
				foreach (AccTransactionLines line in invoiceInDB.Lines)
				{
					AssertEquals("Precondition", true, invoiceInDB.AH_InvoiceDateInfo.HasChanges);
					AssertEquals("Precondition", false, line.AL_ReverseDateInfo.HasChanges);
					AssertEquals("Validation should be triggered even transaction line had been posted when header invoice date is changed.",
						"The invoice date and revenue recognition date must be in the same calendar month. Current Invoice Date: 12-Apr-23. Current Revenue Recognition Date: 11-Mar-23",
						validation.ValidateReverseDate(line)
					);
				}

				UpdateInvoiceDateAndLineRecognitionDate(invoiceInDB,
					invoiceDate: invoiceDateInNextMonth,
					recognitionDate: recognitionDate.AddDays(-1)
				);
				foreach (AccTransactionLines line in invoiceInDB.Lines)
				{
					AssertEquals("Precondition", false, invoiceInDB.AH_InvoiceDateInfo.HasChanges);
					AssertEquals("Precondition", true, line.AL_ReverseDateInfo.HasChanges);
					AssertEquals("Validation should be triggered even transaction line had been posted when line reverse date is changed.",
						"The invoice date and revenue recognition date must be in the same calendar month. Current Invoice Date: 11-Apr-23. Current Revenue Recognition Date: 10-Mar-23",
						validation.ValidateReverseDate(line)
					);
				}
			}
		}

		void UpdateInvoiceDateAndLineRecognitionDate(TransactionHeaderWithLines transactionHeader, ZDateTime invoiceDate, ZDateTime recognitionDate)
		{
			transactionHeader.AH_InvoiceDate = invoiceDate;
			transactionHeader.AH_PostDate = recognitionDate;
			foreach (DependentTransactionLine line in transactionHeader.Lines)
			{
				using (line.SetTempContext(BusinessContext.WipAccrualReversing))
				{
					line.AL_ReverseDate = recognitionDate;
				}
			}
		}

		class ValidateReverseDateScenario
		{
			public string AssertComment { get; set; }
			public TransactionHeaderWithLines TransactionHeader { get; set; }
			public string ExpectedErrorMsg { get; set; }
			public bool ExpectedIsEligible { get; set; }
		}

		public void TestValidateReverseDate_InvalidParameter()
		{
			var validation = new KoreaSouthEInvoicingValidation();
			AssertNull(validation.ValidateReverseDate(null));

			var wip = TestObjectCreator.CreateWIP(TestObjectCreator.Job1, TestObjectCreator.CC1, 1M, "Desc1", 10M);
			AssertNull(validation.ValidateReverseDate(wip));

			wip.AL_AH = ZGuid.BrettsGuid;
			AssertNull(validation.ValidateReverseDate(wip));

			wip.AL_AH = ZGuid.Invalid;
			AssertNull(validation.ValidateReverseDate(wip));
		}

		[TestDate(2023, 3, 11)]
		public void TestValidateInvoiceDate_SkipWhenInvoiceDateNotChangeAndInDB()
		{
			var backDateInvoiceDateDeadline = ZDateTime.Today.Day - 1;
			var expectedMessage = GetExpectedMessageWhenInvoiceDateExceedsDeadLine(backDateInvoiceDateDeadline);

			var invoiceInDB = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			invoiceInDB.AH_InvoiceDate = ZDateTime.Today.AddMonths(-1);
			invoiceInDB.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", invoiceInDB.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
			Factory.Save();

			var invoiceReadyToPost = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			invoiceReadyToPost.AH_InvoiceDate = ZDateTime.Today.AddMonths(-1);
			invoiceReadyToPost.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", invoiceReadyToPost.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today.AddDays(-10)))
			using (AccountingConfigurationRegistry.Instance.BackDateInvoiceDateDeadline.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, backDateInvoiceDateDeadline))
			{
				var validation = new KoreaSouthEInvoicingValidation();

				Assert("Precondition", ElectronicInvoicingEligibilityDecider.IsEligible(invoiceInDB));
				Assert("Precondition", invoiceInDB is InvoicingBase);
				AssertEquals("Precondition", false, invoiceInDB.AH_InvoiceDateInfo.HasChanges);
				AssertNullOrEmpty("Validation should not be triggered when transaction had been posted.", validation.ValidateInvoiceDate(invoiceInDB));

				invoiceInDB.AH_InvoiceDate = invoiceInDB.AH_InvoiceDate.AddDays(-1);
				AssertEquals("Precondition", true, invoiceInDB.AH_InvoiceDateInfo.HasChanges);
				AssertEquals("Validation should be triggered even transaction had been posted when invoice date is changed.", expectedMessage, validation.ValidateInvoiceDate(invoiceInDB));

				Assert("Precondition", ElectronicInvoicingEligibilityDecider.IsEligible(invoiceReadyToPost));
				Assert("Precondition", invoiceReadyToPost is InvoicingBase);
				AssertEquals("Precondition", false, invoiceReadyToPost.IsInDatabase);
				AssertEquals("Validation should be triggered when posting transaction.(transaction is not in DB)", expectedMessage, validation.ValidateInvoiceDate(invoiceReadyToPost));
			}
		}

		[TestDate(2023, 3, 11)]
		public void TestValidateInvoiceDate()
		{
			var dayOfDeadLine = 10;
			var complianceDate = ZDateTime.Today.AddDays(-10);
			var expectedMessage = GetExpectedMessageWhenInvoiceDateExceedsDeadLine(dayOfDeadLine);

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			invoice.AH_InvoiceDate = new ZDateTime(2023, 2, 11);
			invoice.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			Assert("PreCondition", invoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate != null && x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

			var creditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			creditNote.AH_InvoiceDate = new ZDateTime(2023, 2, 11);

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, EInvoicingPivotState.Queued))
			using (AccountingConfigurationRegistry.Instance.BackDateInvoiceDateDeadline.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dayOfDeadLine))
			{
				var validation = new KoreaSouthEInvoicingValidation();

				using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
				{
					AssertEquals("Precondition", true, ElectronicInvoicingEligibilityDecider.IsEligible(invoice));
					AssertEquals("Precondition", true, invoice is InvoicingBase);
					AssertEquals("When invoice is eligible and line is InvoicingLineBase", expectedMessage, validation.ValidateInvoiceDate(invoice));

					var header = Factory.Load<AccTransactionHeader>(invoice.PK);
					AssertEquals("Precondition", true, invoice is AccTransactionHeader);
					AssertEquals("When invoice is eligible and line is AccTransactionLines", expectedMessage, validation.ValidateInvoiceDate(header));

					AssertEquals("Precondition", false, ElectronicInvoicingEligibilityDecider.IsEligible(creditNote));
					AssertNull("When creditNote is not eligible", validation.ValidateInvoiceDate(creditNote));
				}

				using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, false))
				{
					AssertNull(validation.ValidateInvoiceDate(invoice));
				}
			}
		}

		public void TestValidateInvoiceDate_InvalidParameter()
		{
			var validation = new KoreaSouthEInvoicingValidation();
			AssertNull(validation.ValidateInvoiceDate(null));

			var header = Factory.NewWithPrimaryKey<AccTransactionHeader>(Guid.Empty);
			AssertNull(validation.ValidateInvoiceDate(header));
		}

		[TestDate]
		public void TestValidateInvoiceDate_WhenEInvoicingFunctionalityIsDisabled()
		{
			const int dayOfDeadLine = 10;
			var expectedMessage = GetExpectedMessageWhenInvoiceDateExceedsDeadLine(dayOfDeadLine);

			using (AccountingConfigurationRegistry.Instance.BackDateInvoiceDateDeadline.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dayOfDeadLine))
			{
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertValidateInvoiceDate(expectedMessage, new ZDateTime(2023, 2, 15), new ZDateTime(2023, 3, 11));
				}

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertValidateInvoiceDate(null, new ZDateTime(2023, 2, 15), new ZDateTime(2023, 3, 11));
				}
			}
		}

		[TestDate]
		public void TestValidateInvoiceDate_WhenBackDateInvoiceDateDeadlineIsZero()
		{
			var dayOfDeadLine = 0;

			using (AccountingConfigurationRegistry.Instance.BackDateInvoiceDateDeadline.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dayOfDeadLine))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertValidateInvoiceDate(null, new ZDateTime(2023, 2, 15), new ZDateTime(2023, 3, 11));
			}
		}

		[TestDate]
		public void TestValidateInvoiceDate_WhenInvoiceDateInTheMonthBeforeLast()
		{
			var dayOfDeadLine = 10;

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.BackDateInvoiceDateDeadline.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dayOfDeadLine))
			{
				AssertValidateInvoiceDate(ExpectedMessageWhenInvoiceDateIsTheMonthBeforeLast, new ZDateTime(2023, 1, 1), new ZDateTime(2023, 3, 11));
			}
		}

		[TestDate]
		public void TestValidateInvoiceDate_WhenWorkdayIsDeadline()
		{
			var dayOfDeadLine = 10;

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.BackDateInvoiceDateDeadline.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dayOfDeadLine))
			{
				var fridayToday = new ZDateTime(2023, 3, 10);
				AssertEquals("Precondition", DayOfWeek.Friday, fridayToday.DayOfWeek);

				AssertGeneralDates(dayOfDeadLine, fridayToday);
			}
		}

		[TestDate]
		public void TestValidateInvoiceDate_WhenSaturdayIsDeadline()
		{
			var dayOfDeadLine = 11;
			var expectedMessage = GetExpectedMessageWhenInvoiceDateExceedsDeadLine(dayOfDeadLine);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.BackDateInvoiceDateDeadline.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dayOfDeadLine))
			{
				var saturdayToday = new ZDateTime(2023, 3, dayOfDeadLine);
				AssertEquals("Precondition", DayOfWeek.Saturday, saturdayToday.DayOfWeek);
				var fallbackDeadline = new ZDateTime(2023, 3, 13);
				AssertEquals("Precondition", DayOfWeek.Monday, fallbackDeadline.DayOfWeek);

				AssertGeneralDates(dayOfDeadLine, fallbackDeadline);

				AssertValidateInvoiceDate(null, new ZDateTime(2023, 2, 10), saturdayToday.AddDays(-1), "Deadline is (2023, 3, 11) Saturday, fallbacks to (2023, 3, 13) Monday. Today is before the fallback Deadline.");
				AssertValidateInvoiceDate(null, new ZDateTime(2023, 2, 10), saturdayToday, "Deadline is (2023, 3, 11) Saturday, fallbacks to (2023, 3, 13) Monday. Today is before the fallback Deadline.");
				AssertValidateInvoiceDate(null, new ZDateTime(2023, 2, 10), saturdayToday.AddDays(1), "Deadline is (2023, 3, 11) Saturday, fallbacks to (2023, 3, 13) Monday. Today is before the fallback Deadline.");
				AssertValidateInvoiceDate(null, new ZDateTime(2023, 2, 10), saturdayToday.AddDays(2), "Deadline is (2023, 3, 11) Saturday, fallbacks to (2023, 3, 13) Monday. Today is the same as fallback Deadline.");
				AssertValidateInvoiceDate(expectedMessage, new ZDateTime(2023, 2, 10), saturdayToday.AddDays(3), "Deadline is (2023, 3, 11) Saturday, fallbacks to (2023, 3, 13) Monday. Today is later than the fallback Deadline.");
			}
		}

		[TestDate]
		public void TestValidateInvoiceDate_WhenSundayIsDeadline()
		{
			var dayOfDeadLine = 12;
			var expectedMessage = GetExpectedMessageWhenInvoiceDateExceedsDeadLine(dayOfDeadLine);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.BackDateInvoiceDateDeadline.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dayOfDeadLine))
			{
				var deadline = new ZDateTime(2023, 3, dayOfDeadLine);
				AssertEquals("Precondition", DayOfWeek.Sunday, deadline.DayOfWeek);
				var fallbackDeadline = new ZDateTime(2023, 3, 13);
				AssertEquals("Precondition", DayOfWeek.Monday, fallbackDeadline.DayOfWeek);

				AssertGeneralDates(dayOfDeadLine, fallbackDeadline);

				AssertValidateInvoiceDate(null, new ZDateTime(2023, 2, 10), deadline.AddDays(-1), "Deadline is (2023, 3, 12) Sunday, fallbacks to (2023, 3, 13) Monday. Today is before the fallback Deadline.");
				AssertValidateInvoiceDate(null, new ZDateTime(2023, 2, 10), deadline, "Deadline is (2023, 3, 12) Sunday, fallbacks to (2023, 3, 13) Monday. Today is before the fallback Deadline.");
				AssertValidateInvoiceDate(null, new ZDateTime(2023, 2, 10), deadline.AddDays(1), "Deadline is (2023, 3, 12) Sunday, fallbacks to (2023, 3, 13) Monday. Today is the same as fallback Deadline.");
				AssertValidateInvoiceDate(expectedMessage, new ZDateTime(2023, 2, 10), deadline.AddDays(2), "Deadline is (2023, 3, 12) Sunday, fallbacks to (2023, 3, 13) Monday. Today is later than the fallback Deadline.");
			}
		}

		[TestDate]
		public void TestValidateInvoiceDate_WhenInvoiceDateAndDateTimeTodayAreInDifferentYear()
		{
			var dayOfDeadLine = 10;

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.BackDateInvoiceDateDeadline.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, dayOfDeadLine))
			{
				var deadline = new ZDateTime(2023, 1, dayOfDeadLine);
				AssertEquals("Precondition", DayOfWeek.Tuesday, deadline.DayOfWeek);

				AssertValidateInvoiceDate((invoiceDate, dateTimeToday) => null,
				dayOfDeadLine,
				new ZDateTime(2022, 12, 1), new ZDateTime(2022, 12, 30),
				new ZDateTime(2022, 12, 1), new ZDateTime(2022, 12, 30),
				"Invoice date and today's date are in the same year");

				AssertValidateInvoiceDate((invoiceDate, dateTimeToday) => dateTimeToday > deadline ? GetExpectedMessageWhenInvoiceDateExceedsDeadLine(dayOfDeadLine) : null,
				dayOfDeadLine,
				new ZDateTime(2022, 12, 1), new ZDateTime(2022, 12, 30),
				new ZDateTime(2023, 1, 1), new ZDateTime(2023, 1, 31),
				"Invoice date and today's date are in different years. The date difference is less than a month");

				AssertValidateInvoiceDate((invoiceDate, dateTimeToday) => ExpectedMessageWhenInvoiceDateIsTheMonthBeforeLast,
				dayOfDeadLine,
				new ZDateTime(2022, 12, 1), new ZDateTime(2022, 12, 30),
				new ZDateTime(2023, 2, 1), new ZDateTime(2023, 2, 28),
				"Invoice date and today's date are in different years. The date difference is more than a month");
			}
		}

		void AssertValidateInvoiceDate(Func<ZDateTime, ZDateTime, string> getExpectedErrorMessage, int dayOfDeadLine, ZDateTime invoiceDateFrom, ZDateTime invoiceDateTo, ZDateTime dateTimeTodayFrom, ZDateTime dateTimeTodayTo, string assertMsg = null)
		{
			for (var invoiceDate = invoiceDateFrom; invoiceDate <= invoiceDateTo; invoiceDate = invoiceDate.AddDays(1))
			{
				for (var dateTimeToday = dateTimeTodayFrom; dateTimeToday <= dateTimeTodayTo; dateTimeToday = dateTimeToday.AddDays(1))
				{
					AssertValidateInvoiceDate(getExpectedErrorMessage(invoiceDate, dateTimeToday), invoiceDate, dateTimeToday, assertMsg);
				}
			}
		}

		void AssertValidateInvoiceDate(string expectedErrorMessage, ZDateTime invoiceDate, ZDateTime dateTimeToday, string assertMsg = null)
		{
			TestDateAttribute.Date = dateTimeToday.ToDateTime();
			AssertEquals("Precondition", dateTimeToday, ZDateTime.Today);

			var validation = new KoreaSouthEInvoicingValidation();
			AssertEquals(assertMsg, expectedErrorMessage, validation.ValidateInvoiceDate(invoiceDate));
		}

		void AssertGeneralDates(int dayOfDeadLine, ZDateTime today)
		{
			AssertValidateInvoiceDate((invoiceDate, dateTimeToday) => null,
				dayOfDeadLine,
				new ZDateTime(2023, 2, 1), new ZDateTime(2023, 2, 28),
				new ZDateTime(2023, 2, 1), new ZDateTime(2023, 2, 28),
				"Invoice date and today's date are in the same month");

			AssertValidateInvoiceDate((invoiceDate, dateTimeToday) => dateTimeToday > today ? GetExpectedMessageWhenInvoiceDateExceedsDeadLine(dayOfDeadLine) : null,
				dayOfDeadLine,
				new ZDateTime(2023, 2, 1), new ZDateTime(2023, 2, 28),
				new ZDateTime(2023, 3, 1), new ZDateTime(2023, 3, 31),
				"Invoice date month is 1 month ahead of today's month");

			AssertValidateInvoiceDate((invoiceDate, dateTimeToday) => ExpectedMessageWhenInvoiceDateIsTheMonthBeforeLast,
				dayOfDeadLine,
				new ZDateTime(2023, 2, 1), new ZDateTime(2023, 2, 28),
				new ZDateTime(2023, 4, 1), new ZDateTime(2023, 4, 30),
				"Invoice date month is 2 months ahead of today's month");
		}

		const string ExpectedMessageWhenInvoiceDateIsTheMonthBeforeLast = "The invoice date can only be back dated to the previous month only, not earlier than previous month.";

		string GetExpectedMessageWhenInvoiceDateExceedsDeadLine(int dayOfDeadLine)
		{
			return $@"The invoice date can not be back dated to the previous month after day {dayOfDeadLine} of the current month. 
Should day {dayOfDeadLine} falls on a Saturday or Sunday, back dating to previous month is allowed on the immediate Monday only.
However, back dating to current month is allowed.";
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}

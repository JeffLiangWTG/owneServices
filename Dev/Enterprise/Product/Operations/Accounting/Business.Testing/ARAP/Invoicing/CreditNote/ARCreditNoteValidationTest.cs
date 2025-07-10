using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class ARCreditNoteValidationTest : CrediteNoteValidationTest<ARCreditNote>
	{
		protected override InvoiceBaseValidation GetValidation(TransactionHeader parent)
		{
			return parent.Validation as ARCreditNoteValidation;
		}

		protected override BooleanRegistryItem OriginalInvoiceDetailsMandatoryRegistryItem => AccountingConfigurationRegistry.Instance.OriginalInvoiceDetailsMandatoryOnARCreditNotes;

		protected override Type OriginalInvoiceType => typeof(ARInvoice);

		protected override InvoicingBase GetCorrectOriginalInvoice(OrgHeader header) => TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: header);

		protected override InvoicingBase GetWrongOrgOriginalInvoice(OrgHeader header) => TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: header);

		protected override InvoicingBase GetWrongLedgerOriginalInvoice(OrgHeader header) => TestObjectCreator.CreateInvoice(typeof(ARInvoice), organisation: header);

		public override void TestCheckTransactionNumForUAInvNumExcludingItselfAlreadyExist_Standard()
		{
			Assert(true);
		}

		public override void TestCheckTransactionNumForUAInvNumExcludingItselfAlreadyExist_Calendar()
		{
			Assert(true);
		}

		public override void TestCheckTransactionNumForUAInvNumAlreadyExist_Standard()
		{
			Assert(true);
		}

		public override void TestCheckTransactionNumForUAInvNumAlreadyExist_Calendar()
		{
			Assert(true);
		}

		public override void TestUAInvoiceNumberDoesntCheckInvoiceNumbersForOtherCompanies()
		{
			Assert(true);
		}

		public override void TestCheckOriginalTransactionReference()
		{
			base.TestCheckOriginalTransactionReference();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				var aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
				var aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
				var testValidation = GetValidation(aRCreditNote) as ARCreditNoteValidation;

				aRCreditNote.AH_OriginalTransactionNum = "1122";
				aRCreditNote.OriginalTransactionReference = ZGuid.Empty;
				testValidation.ValidateOriginalTransactionReference();
				AssertHasError(aRCreditNote.OriginalTransactionReferenceInfo, "Original Reference must be filled in if Original Invoice number or date have been filled in.");

				aRCreditNote.AH_OriginalTransactionNum = ZString.Empty;
				aRCreditNote.AH_OriginalInvoiceDate = ZDate.Today;
				aRCreditNote.OriginalTransactionReference = ZGuid.Empty;
				testValidation.ValidateOriginalTransactionReference();
				AssertHasError(aRCreditNote.OriginalTransactionReferenceInfo, "Original Reference must be filled in if Original Invoice number or date have been filled in.");

				aRCreditNote.AH_OriginalInvoiceDate = ZDate.Empty;
				aRCreditNote.AH_OriginalReferenceStartDate = ZDate.Empty;
				aRCreditNote.AH_OriginalReferenceEndDate = ZDate.Empty;
				aRCreditNote.OriginalTransactionReference = ZGuid.Empty;
				testValidation.ValidateOriginalTransactionReference();
				AssertHasError(aRCreditNote.OriginalTransactionReferenceInfo, "Must identify an original reference or date range in this transaction.");

				aRCreditNote.AH_OriginalReferenceStartDate = ZDate.Today;
				aRCreditNote.AH_OriginalReferenceEndDate = ZDate.Today;
				aRCreditNote.OriginalTransactionReference = aRInvoice.PK;
				AssertHasError(aRCreditNote.OriginalTransactionReferenceInfo, "Must identify only one of the following fields, original reference or date range.");
			}
		}

		public void TestCheckReasonCode()
		{
			AssertReasonCodeEmptyValueValidation(false, false);
			AssertReasonCodeEmptyValueValidation(true, true);

			void AssertReasonCodeEmptyValueValidation(bool areOriginalTransactionReferenceFieldsMandatory, bool isValidationErrorExpected)
			{
				using (InitaliseComplianceFactory(areOriginalTransactionReferenceFieldsMandatory: areOriginalTransactionReferenceFieldsMandatory))
				{
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
					var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);

					Assert(!creditNote.OriginalTransactionIsSet);
					AssertNoErrors("no error when Original Transaction is empty", creditNote.ReasonCodeInfo);

					creditNote.OriginalTransactionReference = invoice.PK;
					Assert(creditNote.OriginalTransactionIsSet);
					creditNote.ReasonCode = "";
					if (isValidationErrorExpected)
					{
						AssertHasError(creditNote.ReasonCodeInfo, "Please enter a value.");
					}
					else
					{
						AssertNoErrors(creditNote.ReasonCodeInfo);
					}

					creditNote.ReasonCode = "UKN";
					AssertHasError(creditNote.ReasonCodeInfo, "Enter a valid selection.");

					creditNote.ReasonCode = "TXT";
					AssertNoErrors(creditNote.ReasonCodeInfo);

					var creditNote2 = TestObjectCreator.CreateARCreditNote("ARCRD002", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);

					Assert(!creditNote2.OriginalTransactionIsSet);
					AssertNoErrors("no error when Original Transaction is empty", creditNote2.ReasonCodeInfo);

					creditNote2.AH_OriginalReferenceStartDate = ZDate.Today.AddDays(-2);
					creditNote2.AH_OriginalReferenceEndDate = ZDate.Today;
					creditNote2.ReasonCode = string.Empty;
					Assert(!creditNote2.OriginalTransactionIsSet);
					AssertNoErrors(creditNote2.ReasonCodeInfo);
					AssertNoErrors(creditNote2.ReasonDescriptionInfo);
				}
			}
		}

		public void TestCheckReasonDescription()
		{
			AssertReasonDescriptionEmptyValueValidation(false, false);
			AssertReasonDescriptionEmptyValueValidation(true, true);

			void AssertReasonDescriptionEmptyValueValidation(bool areOriginalTransactionReferenceFieldsMandatory, bool isValidationErrorExpected)
			{
				using (InitaliseComplianceFactory(areOriginalTransactionReferenceFieldsMandatory: areOriginalTransactionReferenceFieldsMandatory))
				{
					var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV001", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);
					var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD001", TestObjectCreator.AALSHI, TestObjectCreator.EUR, 1m);

					Assert(!creditNote.OriginalTransactionIsSet);
					AssertNoErrors("no error when Original Transaction is empty", creditNote.ReasonDescriptionInfo);

					creditNote.OriginalTransactionReference = invoice.PK;
					Assert(creditNote.OriginalTransactionIsSet);
					creditNote.ReasonDescription = "";
					if (isValidationErrorExpected)
					{
						AssertHasError(creditNote.ReasonDescriptionInfo, "Please enter a value.");
					}
					else
					{
						AssertNoErrors(creditNote.ReasonDescriptionInfo);
					}

					creditNote.ReasonDescription = "new DESC";
					AssertNoErrors(creditNote.ReasonDescriptionInfo);

					creditNote.ReasonDescription = "TXT|new DESC";
					AssertHasError(creditNote.ReasonDescriptionInfo, "This field should never contain the '|' character.");
				}
			}
		}

		public void TestCheckAH_OriginalReferenceEndDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				var aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
				var aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();

				aRCreditNote.AH_OriginalReferenceStartDate = ZDate.Empty;
				aRCreditNote.AH_OriginalReferenceEndDate = ZDate.Empty;
				AssertHasError(aRCreditNote.AH_OriginalReferenceEndDateInfo, "Must identify an original reference or date range in this transaction.");

				aRCreditNote.OriginalTransactionReference = aRInvoice.PK;
				aRCreditNote.AH_OriginalReferenceStartDate = ZDate.Today;
				aRCreditNote.AH_OriginalReferenceEndDate = ZDate.Today;
				AssertHasError(aRCreditNote.AH_OriginalReferenceEndDateInfo, "Must identify only one of the following fields, original reference or date range.");

				aRCreditNote.AH_OriginalReferenceEndDate = ZDate.Empty;
				AssertHasError(aRCreditNote.AH_OriginalReferenceEndDateInfo, "Please enter a value.");

				aRCreditNote.OriginalTransactionReference = ZGuid.Empty;
				aRCreditNote.AH_OriginalReferenceEndDate = ZDate.Today.AddDays(1);
				AssertHasError(aRCreditNote.AH_OriginalReferenceEndDateInfo, "'Reference Date From' and 'Reference Date To' must be filled with dates less than or equal to current date.");

				aRCreditNote.AH_OriginalReferenceEndDate = ZDate.Today.AddDays(-1);
				AssertHasError(aRCreditNote.AH_OriginalReferenceEndDateInfo, "'Reference Date From' should be less than or equal to 'Reference Date To'.");
			}
		}

		public override void TestValidateAH_Calc_AmendStatusCode()
		{
			var mockIAmendStatusCodeProvider = new Mock<IAmendStatusCodeProvider>();
			var mockIAmendStatusCodeValidationProvider = new Mock<AmendStatusCodeValidationProvider>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			var amendStatusCodeList = new CodeDescriptionPairList();
			amendStatusCodeList.AddPair("01", "Test List Value");

			mockIAmendStatusCodeProvider.Setup(x => x.AmendStatusCodeList).Returns(amendStatusCodeList);
			mockIAmendStatusCodeProvider.Setup(x => x.ShouldShowAmendStatusCode()).Returns(true);
			mockIAmendStatusCodeProvider.Setup(x => x.AmendStatusCodeReferenceType).Returns("KRE");
			mockIAccountingCountryFactory.As<IInstanceProvider<IAmendStatusCodeProvider>>().Setup(x => x.Get()).Returns(mockIAmendStatusCodeProvider.Object);
			mockIAccountingCountryFactory.As<IInstanceProvider<IAmendStatusCodeValidationProvider>>().Setup(x => x.Get()).Returns(mockIAmendStatusCodeValidationProvider.Object);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
			mockIAmendStatusCodeValidationProvider.Protected().Setup<bool>("ShouldValidateAmendStatusCode", ItExpr.IsAny<InvoicingBase>()).Returns(true);

			var invoicingBase = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			var invoiceWithOriginal = Factory.NewWithValidTestData(InvoiceType) as InvoicingBase;
			invoiceWithOriginal.OriginalTransactionReference = Factory.New(InvoiceType).PK;

			AssertValidateAH_Calc_AmendStatusCode(invoicingBase, false, false);
			AssertValidateAH_Calc_AmendStatusCode(invoiceWithOriginal, false, false);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			AssertValidateAH_Calc_AmendStatusCode(invoicingBase, true, false);
			AssertValidateAH_Calc_AmendStatusCode(invoiceWithOriginal, true, true);
		}

		public void TestCheckAH_OriginalReferenceStartDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				var aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
				var aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();

				aRCreditNote.AH_OriginalReferenceEndDate = ZDate.Empty;
				aRCreditNote.AH_OriginalReferenceStartDate = ZDate.Empty;
				AssertHasError(aRCreditNote.AH_OriginalReferenceStartDateInfo, "Must identify an original reference or date range in this transaction.");

				aRCreditNote.OriginalTransactionReference = aRInvoice.PK;
				aRCreditNote.AH_OriginalReferenceEndDate = ZDate.Today;
				aRCreditNote.AH_OriginalReferenceStartDate = ZDate.Today;
				AssertHasError(aRCreditNote.AH_OriginalReferenceStartDateInfo, "Must identify only one of the following fields, original reference or date range.");

				aRCreditNote.AH_OriginalReferenceStartDate = ZDate.Empty;
				AssertHasError(aRCreditNote.AH_OriginalReferenceStartDateInfo, "Please enter a value.");

				aRCreditNote.OriginalTransactionReference = ZGuid.Empty;
				aRCreditNote.AH_OriginalReferenceStartDate = ZDate.Today.AddDays(1);
				AssertHasError(aRCreditNote.AH_OriginalReferenceStartDateInfo, "'Reference Date From' and 'Reference Date To' must be filled with dates less than or equal to current date.");

				aRCreditNote.AH_OriginalReferenceEndDate = ZDate.Today.AddDays(-1);
				aRCreditNote.AH_OriginalReferenceStartDate = ZDate.Today;
				AssertHasError(aRCreditNote.AH_OriginalReferenceStartDateInfo, "'Reference Date From' should be less than or equal to 'Reference Date To'.");
			}
		}
	}
}

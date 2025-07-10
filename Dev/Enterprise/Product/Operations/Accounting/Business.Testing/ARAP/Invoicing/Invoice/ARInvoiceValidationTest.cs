using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class ARInvoiceValidationTest : InvoiceValidationTest
	{
		public override void TestCheckTransactionNumForAPInvNumAlreadyExist_Standard()
		{
			Assert(true);
		}

		public override void TestCheckTransactionNumForAPInvNumAlreadyExist_Calendar()
		{
			Assert(true);
		}

		public override void TestAPInvoiceNumberDoesntCheckInvoiceNumbersForOtherCompanies()
		{
			Assert(true);
		}

		public override void TestJobInvoicingExist()
		{
			Assert(true);
		}

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

					Assert(!invoice.OriginalTransactionIsSet);
					AssertNoErrors("no error when Original Transaction is empty", invoice.ReasonCodeInfo);

					invoice.OriginalTransactionReference = creditNote.PK;
					Assert(invoice.OriginalTransactionIsSet);
					invoice.ReasonCode = "";
					if (isValidationErrorExpected)
					{
						AssertHasError(invoice.ReasonCodeInfo, "Please enter a value.");
					}
					else
					{
						AssertNoErrors(invoice.ReasonCodeInfo);
					}

					invoice.ReasonCode = "UKN";
					AssertHasError(invoice.ReasonCodeInfo, "Enter a valid selection.");

					invoice.ReasonCode = "TXT";
					AssertNoErrors(invoice.ReasonCodeInfo);

					var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV002", TestObjectCreator.EUR, 1m, TestObjectCreator.AALSHI);

					Assert(!invoice2.OriginalTransactionIsSet);
					AssertNoErrors("no error when Original Transaction is empty", invoice2.ReasonCodeInfo);

					invoice2.AH_OriginalReferenceStartDate = ZDate.Today.AddDays(-2);
					invoice2.AH_OriginalReferenceEndDate = ZDate.Today;
					invoice2.ReasonCode = string.Empty;
					Assert(!invoice2.OriginalTransactionIsSet);
					AssertNoErrors(invoice2.ReasonCodeInfo);
					AssertNoErrors(invoice2.ReasonDescriptionInfo);
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

					Assert(!invoice.OriginalTransactionIsSet);
					AssertNoErrors("no error when Original Transaction is empty", invoice.ReasonDescriptionInfo);

					invoice.OriginalTransactionReference = creditNote.PK;
					Assert(invoice.OriginalTransactionIsSet);
					invoice.ReasonDescription = "";
					if (isValidationErrorExpected)
					{
						AssertHasError(invoice.ReasonDescriptionInfo, "Please enter a value.");
					}
					else
					{
						AssertNoErrors(invoice.ReasonDescriptionInfo);
					}

					invoice.ReasonDescription = "new DESC";
					AssertNoErrors(invoice.ReasonDescriptionInfo);

					invoice.ReasonDescription = "TXT|new DESC";
					AssertHasError(invoice.ReasonDescriptionInfo, "This field should never contain the '|' character.");
				}
			}
		}

		public void TestSourceReferenceMutexValidation()
		{
			var expectedError = "is in the process of allocating the same source reference value. We cannot create duplicate source reference.";
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			using (factory1.AddDisposableService())
			using (factory2.AddDisposableService())
			{
				var invoice1 = factory1.NewWithValidTestData<ARInvoice>();
				invoice1.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;
				var invoice2 = factory2.NewWithValidTestData<ARInvoice>();
				invoice2.AH_ComplianceSubType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXM;

				SetupSourceReferenceMutexConflict("FTM a124");

				invoice1.SourceReference = "FTM a125";
				new TransactionHeaderValidation(invoice1).ValidateSourceReference();
				new TransactionHeaderValidation(invoice2).ValidateSourceReference();
				AssertNoErrors(invoice1.SourceReferenceInfo);
				AssertNoErrors("second user doesn't have error anymore if first user update the source reference to another value", invoice2.SourceReferenceInfo);

				SetupSourceReferenceMutexConflict("FTM a126");
				invoice1.SourceReference = string.Empty;
				new TransactionHeaderValidation(invoice1).ValidateSourceReference();
				new TransactionHeaderValidation(invoice2).ValidateSourceReference();
				AssertNoErrors("second user doesn't have error anymore if first user update the source reference to empty", invoice2.SourceReferenceInfo);

				SetupSourceReferenceMutexConflict("FTM a127");
				factory1.Save();
				new TransactionHeaderValidation(invoice2).ValidateSourceReference();
				AssertHasError(invoice2.SourceReferenceInfo, $"The entered Source Reference value is already recorded against transaction {invoice1.AH_TransactionNum}.");

				invoice2.SourceReference = string.Empty;

				void SetupSourceReferenceMutexConflict(string sourceReference)
				{
					invoice1.SourceReference = sourceReference;
					invoice2.SourceReference = sourceReference;
					new TransactionHeaderValidation(invoice1).ValidateSourceReference();
					new TransactionHeaderValidation(invoice2).ValidateSourceReference();
					AssertNoErrors("user1 doesn't have error because he was the first to set source reference", invoice1.SourceReferenceInfo);
					AssertHasErrorContaining("user2 have error because user1 already uses this source reference value", invoice2.SourceReferenceInfo, expectedError);
				}
			}
		}

		public override void TestValidateAH_Calc_AmendStatusCode()
		{
			var invoicingBase = Factory.NewWithValidTestData<ARInvoice>();

			var originalAR = Factory.NewWithValidTestData<ARInvoice>();
			IAmending original = originalAR;
			var amendingAR = original.GenerateAmendingTransaction(originalAR.AH_TransactionType) as ARInvoice;

			var originalAR2 = Factory.NewWithValidTestData<ARInvoice>();
			IAmending original2 = originalAR2;
			var amendingAR2 = original2.GenerateAmendingTransaction(originalAR2.AH_TransactionType) as ARInvoice;

			var mockIAmendStatusCodeProvider = new Mock<IAmendStatusCodeProvider>();
			var mockIAmendStatusCodeValidationProvider = new Mock<AmendStatusCodeValidationProvider>();
			var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
			var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();

			var amendStatusCodeList = new CodeDescriptionPairList();
			amendStatusCodeList.AddPair("01", "Test List Value");

			mockIAmendStatusCodeProvider.Setup(x => x.AmendStatusCodeList).Returns(amendStatusCodeList);
			mockIAmendStatusCodeProvider.Setup(x => x.ShouldShowAmendStatusCode()).Returns(true);
			mockIAmendStatusCodeProvider.Setup(x => x.AmendStatusCodeReferenceType).Returns("KRE");
			mockIAmendStatusCodeValidationProvider.Setup(x => x.ValidateAmendStatusCodeForInvoice(amendingAR2)).Verifiable();
			mockIAmendStatusCodeValidationProvider.CallBase = true;
			mockIAccountingCountryFactory.As<IInstanceProvider<IAmendStatusCodeProvider>>().Setup(x => x.Get()).Returns(mockIAmendStatusCodeProvider.Object);
			mockIAccountingCountryFactory.As<IInstanceProvider<IAmendStatusCodeValidationProvider>>().Setup(x => x.Get()).Returns(mockIAmendStatusCodeValidationProvider.Object);
			mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

			AssertValidateAH_Calc_AmendStatusCode(invoicingBase, false, false);
			AssertValidateAH_Calc_AmendStatusCode(amendingAR, false, false);

			ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object);

			(amendingAR2.Validation as InvoiceValidation).ValidateAH_Calc_AmendStatusCode();
			mockIAmendStatusCodeValidationProvider.Verify(x => x.ValidateAmendStatusCodeForInvoice(amendingAR2), Times.Once);

			amendingAR2.AH_Calc_AmendStatusCode = "01";
			mockIAmendStatusCodeValidationProvider.Verify(x => x.ValidateAmendStatusCodeForInvoice(amendingAR2), Times.Exactly(2));

			amendingAR2.AH_Calc_AmendStatusCode = "";
			mockIAmendStatusCodeValidationProvider.Verify(x => x.ValidateAmendStatusCodeForInvoice(amendingAR2), Times.Exactly(3));

			AssertValidateAH_Calc_AmendStatusCode(invoicingBase, true, false);
			AssertValidateAH_Calc_AmendStatusCode(amendingAR, true, true);
		}

		protected override InvoiceBaseValidation GetValidation(TransactionHeader parent)
		{
			return new InvoiceValidation((Invoice)parent);
		}

		protected override Type InvoiceType
		{
			get { return typeof(ARInvoice); }
		}

		protected override BooleanRegistryItem OriginalInvoiceDetailsMandatoryRegistryItem => AccountingConfigurationRegistry.Instance.OriginalInvoiceDetailsMandatoryOnARDebitNotes;
		protected override Type OriginalInvoiceType => typeof(ARCreditNote);
	}
}

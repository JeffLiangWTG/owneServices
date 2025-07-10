using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class WithholdingAPJournalCreatorTest : TestCaseWithFactory
	{
		[TestDate(2020, 01, 15)]
		public void TestWithHoldingJournalCreation()
		{
			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			var expectedCategory = Enterprise.Core.Constants.TransactionCategory.Codes.PaymentBasisWithholding;
			var expectedTranasctionNumber = "112233";
			var expectedOrg = TestObjectCreator.Creditor1;
			var expectedInvoiceDate = ZDate.Today;
			var expectedBranchPK = GlbBranch.CurrentBranch.PK;
			var expectedDepartmentPK = GlbDepartment.CurrentDepartment.PK;
			var expectedCurrency = TestObjectCreator.AUD;
			var expectedOSAmount = 10M;
			var expectedLocalAmount = 20M;
			var expectedGLHeaderPK = TestObjectCreator.GLHeader1.PK;
			var expectedTaxRecordPKs1 = new[] { ZGuid.NewZGuid(), ZGuid.NewZGuid() };
			var expectedTaxRecordPKs2 = new[] { ZGuid.NewZGuid() };

			var matchTransactionDetails1 = new MatchTransactionDetails(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, expectedCurrency.RX_Code, expectedLocalAmount, expectedOSAmount, expectedGLHeaderPK, expectedTaxRecordPKs1);
			var matchTransactionDetails2 = new MatchTransactionDetails(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, expectedCurrency.RX_Code, -1 * expectedLocalAmount, -1 * expectedOSAmount, expectedGLHeaderPK, expectedTaxRecordPKs2);
			IMatchTransactionDetails[] expectedOutput = new[] { matchTransactionDetails1, matchTransactionDetails2 };

			IWithholdingAPJournalCreator journalCreator = new WithholdingAPJournalCreator();
			var futureDate = ZDate.Today.AddDays(2);
			var expectedPostDate = ZDate.Today;

			ActionAndAssertJournalProperties(typeof(APInvoice));

			ActionAndAssertJournalProperties(typeof(APCreditNote));

			void ActionAndAssertJournalProperties(Type invoiceType)
			{
				var invoice = TestObjectCreator.CreateInvoice(invoiceType, expectedTranasctionNumber, expectedCurrency, 2M, expectedOrg);
				var taxParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
				taxProcessorMock.Setup(x => x.GetPaymentRetentionMatchTransactionDetails(taxParent)).Returns((0M, 0M, expectedOutput));

				var result = journalCreator.Create(Factory, invoice, futureDate);

				taxProcessorMock.Verify(x => x.GetPaymentRetentionMatchTransactionDetails(taxParent), Times.Once);

				AssertEquals("2 withholding journals will be created", 2, result.JournalDetails.Count);

				var expectedDescription = $"PBW Withholding AP Journal - {invoice.AH_TransactionType} 112233";
				var withholdingAPjournal = result.JournalDetails.First();
				AssertJournalProperties(matchTransactionDetails1);

				withholdingAPjournal = result.JournalDetails.Skip(1).First();
				AssertJournalProperties(matchTransactionDetails2);

				void AssertJournalProperties(IMatchTransactionDetails expectedMatchTransactionDetails)
				{
					var journal = withholdingAPjournal.Key;
					var sign = withholdingAPjournal.Value.OSAmount > 0 ? 1 : -1;
					AssertEquals("Category", expectedCategory, journal.AH_TransactionCategory);
					AssertEquals("Org", expectedOrg.PK, journal.AH_OH);
					AssertEquals("GL Account", expectedGLHeaderPK, journal.AH_AG);
					AssertEquals("Currency", expectedCurrency.RX_Code, journal.AH_RX_NKTransactionCurrency);
					AssertEquals("OS Partial Payment Amount", sign * expectedOSAmount, ((IMatching)journal).OSPartialPaymentAmount);
					AssertEquals("OS ex tax amount", expectedOSAmount, journal.AH_OSExTaxAmount);
					AssertEquals("Local ex tax amount", expectedLocalAmount, journal.AH_LocalExTaxAmount);
					AssertEquals("Debit Credit sign", withholdingAPjournal.Value.OSAmount > 0 ? DebitCreditDataEntry.CR : DebitCreditDataEntry.DR, journal.DebitCreditSign);
					AssertEquals("Description", expectedDescription, journal.AH_Desc);
					AssertEquals("Branch", expectedBranchPK, journal.AH_GB);
					AssertEquals("Department", expectedDepartmentPK, journal.AH_GE);
					AssertEquals("Post Date", expectedPostDate, journal.AH_PostDate.Date);
					AssertEquals("Invoice Date", futureDate, journal.AH_InvoiceDate);
					AssertEquals("TransactionCreatedByMatching", false, journal.AH_TransactionCreatedByMatching);
					AssertEquals("EnableCheckSubAccountsForGLHeader", false, journal.EnableCheckSubAccountsForGLHeader);

					var expectedJournalDetails = withholdingAPjournal.Value;
					AssertEquals("Journal details", expectedMatchTransactionDetails, expectedJournalDetails);
					AssertEquals("Journal PK", journal.PK, expectedJournalDetails.PK);
					AssertEquals("Journal Realisation Date", journal.AH_PostDate.Date, expectedJournalDetails.RealisationDate);
				}
			}
		}

		public void TestWithHoldingJournalCreation_PaymentRetentionMatchTransactionDetailsListEmpty()
		{
			AssertCreateReturnsNull("When MatchTransactionDetails list is empty", (0M, 0M, Array.Empty<MatchTransactionDetails>()));
		}

		public void TestWithHoldingJournalCreation_GetPaymentRetentionMatchTransactionDetailsReturnsDefault()
		{
			AssertCreateReturnsNull("When GetPaymentRetentionMatchTransactionDetails return default", default);
		}

		public void TestWithHoldingJournalCreation_PaymentRetentionMatchTransactionDetailsListNull()
		{
			AssertCreateReturnsNull("When MatchTransactionDetails list is null but RealizedWHT is not 0", (0M, 15M, null));
		}

		void AssertCreateReturnsNull(ZString message, (ZDecimal, ZDecimal, IMatchTransactionDetails[]) valueToReturnInMockSetup)
		{
			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var taxParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);

			taxProcessorMock.Setup(x => x.GetPaymentRetentionMatchTransactionDetails(taxParent)).Returns(valueToReturnInMockSetup);

			IWithholdingAPJournalCreator journalCreator = new WithholdingAPJournalCreator();

			AssertNull(message, journalCreator.Create(Factory, invoice, ZDate.Today.AddDays(1)));
			taxProcessorMock.Verify(x => x.GetPaymentRetentionMatchTransactionDetails(taxParent));
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}

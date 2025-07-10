using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class WithholdingJournalCreatorForMultipleInvoicesTest : TestCaseWithFactory
	{
		public void TestConstructorExceptionWhenWithholdingJournalCreatorIsNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: withholdingJournalCreator", () => new WithholdingJournalCreatorForMultipleInvoices(null));
			AssertNoExceptionThrown(() => new WithholdingJournalCreatorForMultipleInvoices(new Mock<IWithholdingJournalCreatorForSingleInvoice>().Object));
		}

		[ExpectNoExceptions]
		public void TestCreate_PassesSingleInvoiceToCreator_CorrectInvoiceIsPassedToSingleCreator()
		{
			var apInvoice = Factory.New<APInvoice>();
			var mockCreator = new Mock<IWithholdingJournalCreatorForSingleInvoice>();
			mockCreator.Setup(c => c.Create(It.IsAny<InvoicingBase>())).Returns((new WithholdingJournalsPerInvoice(apInvoice, Array.Empty<WithholdingJournalForDisplay>()), "error"));
			var withholdingJournals = new WithholdingJournalCreatorForMultipleInvoices(mockCreator.Object);

			withholdingJournals.Create(new[] { apInvoice });

			mockCreator.Verify(c => c.Create(apInvoice));
		}

		[ExpectNoExceptions]
		public void TestCreate_PassesMultipleInvoicesToCreator_CorrectInvoicesArePassedToSingleCreator()
		{
			var apInvoice1 = Factory.New<APInvoice>();
			var apInvoice2 = Factory.New<APInvoice>();
			var mockCreator = new Mock<IWithholdingJournalCreatorForSingleInvoice>();
			mockCreator.Setup(c => c.Create(It.IsAny<InvoicingBase>())).Returns((new WithholdingJournalsPerInvoice(apInvoice1, Array.Empty<WithholdingJournalForDisplay>()), "error"));
			var withholdingJournals = new WithholdingJournalCreatorForMultipleInvoices(mockCreator.Object);

			withholdingJournals.Create(new[] { apInvoice1, apInvoice2 });

			mockCreator.Verify(c => c.Create(apInvoice1));
			mockCreator.Verify(c => c.Create(apInvoice2));
		}

		[ExpectNoExceptions]
		public void TestCreate_PassesEmptyInvoiceCollectionToCreator_SingleCreatorIsNotCalled()
		{
			var mockCreator = new Mock<IWithholdingJournalCreatorForSingleInvoice>();
			var withholdingJournals = new WithholdingJournalCreatorForMultipleInvoices(mockCreator.Object);

			var result = withholdingJournals.Create(Array.Empty<InvoicingBase>());

			mockCreator.Verify(c => c.Create(It.IsAny<InvoicingBase>()), Times.Never);
		}

		public void TestCreate_SingleInvoiceWithoutErrors_ReturnsSingleCreatorResult()
		{
			var apInvoice = Factory.New<APInvoice>();
			var withholdingJournalsPerInvoice = new WithholdingJournalsPerInvoice(apInvoice, new[] { new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty) });
			var mockCreator = new Mock<IWithholdingJournalCreatorForSingleInvoice>();
			mockCreator.Setup(c => c.Create(It.IsAny<InvoicingBase>())).Returns((withholdingJournalsPerInvoice, string.Empty));
			var withholdingJournals = new WithholdingJournalCreatorForMultipleInvoices(mockCreator.Object);

			var result = withholdingJournals.Create(new[] { apInvoice });

			AssertContainsExactElementsInAnyOrder("result.journalCollectionPerInvoice", new[] { withholdingJournalsPerInvoice }, result.journalCollectionPerInvoice);
			AssertEquals("result.errorMessage", string.Empty, result.errorMessage);
		}

		public void TestCreate_MultipleInvoicesWithoutErrors_ReturnsSingleCreatorResultForAllInvoice()
		{
			var apInvoice1 = Factory.New<APInvoice>();
			var apInvoice2 = Factory.New<APInvoice>();
			var mockCreator = new Mock<IWithholdingJournalCreatorForSingleInvoice>();
			var withholdingJournalsPerInvoice1 = new WithholdingJournalsPerInvoice(apInvoice1, new[] { new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty) });
			var withholdingJournalsPerInvoice2 = new WithholdingJournalsPerInvoice(apInvoice2, new[] { new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty) });
			mockCreator.Setup(c => c.Create(apInvoice1)).Returns((withholdingJournalsPerInvoice1, string.Empty));
			mockCreator.Setup(c => c.Create(apInvoice2)).Returns((withholdingJournalsPerInvoice2, string.Empty));
			var withholdingJournals = new WithholdingJournalCreatorForMultipleInvoices(mockCreator.Object);

			var result = withholdingJournals.Create(new[] { apInvoice1, apInvoice2 });

			AssertContainsExactElementsInAnyOrder("result.journalCollectionPerInvoice", new[] { withholdingJournalsPerInvoice1, withholdingJournalsPerInvoice2 }, result.journalCollectionPerInvoice);
			AssertEquals("result.errorMessage", string.Empty, result.errorMessage);
		}

		public void TestCreate_SingleInvoiceWithError_ReturnsErrorMessageAndEmptyJournalCollection()
		{
			var expectedErrorMessage = "Some error";
			var apInvoice = Factory.New<APInvoice>();
			apInvoice.AH_TransactionNum = "INV123";
			var withholdingJournalsPerInvoice = new WithholdingJournalsPerInvoice(apInvoice, Array.Empty<WithholdingJournalForDisplay>());
			var mockCreator = new Mock<IWithholdingJournalCreatorForSingleInvoice>();
			mockCreator.Setup(c => c.Create(It.IsAny<InvoicingBase>())).Returns((withholdingJournalsPerInvoice, expectedErrorMessage));
			var withholdingJournals = new WithholdingJournalCreatorForMultipleInvoices(mockCreator.Object);

			var result = withholdingJournals.Create(new[] { apInvoice });

			AssertEquals("result.journalCollectionPerInvoice.Count", 0, result.journalCollectionPerInvoice.Count);
			AssertEquals("result.errorMessage", "INV123: Some error", result.errorMessage);
		}

		public void TestCreate_MultipleInvoicesWithMixedResults_ReturnsConcatenatedErrorMessagesAndCreatedJournals()
		{
			var errorMessage1 = "Error 1.";
			var errorMessage2 = "Error 2.";
			var apInvoice1 = Factory.New<APInvoice>();
			apInvoice1.AH_TransactionNum = "INV1";
			var apInvoice2 = Factory.New<APInvoice>();
			apInvoice2.AH_TransactionNum = "INV2";
			var apInvoice3 = Factory.New<APInvoice>();
			apInvoice3.AH_TransactionNum = "INV3";
			var withholdingJournalsPerInvoice1 = new WithholdingJournalsPerInvoice(apInvoice1, Array.Empty<WithholdingJournalForDisplay>());
			var withholdingJournalsPerInvoice2 = new WithholdingJournalsPerInvoice(apInvoice2, new[] { new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty) });
			var withholdingJournalsPerInvoice3 = new WithholdingJournalsPerInvoice(apInvoice3, Array.Empty<WithholdingJournalForDisplay>());
			var mockCreator = new Mock<IWithholdingJournalCreatorForSingleInvoice>();
			mockCreator.Setup(c => c.Create(apInvoice1)).Returns((withholdingJournalsPerInvoice1, errorMessage1));
			mockCreator.Setup(c => c.Create(apInvoice2)).Returns((withholdingJournalsPerInvoice2, string.Empty));
			mockCreator.Setup(c => c.Create(apInvoice3)).Returns((withholdingJournalsPerInvoice3, errorMessage2));
			var withholdingJournals = new WithholdingJournalCreatorForMultipleInvoices(mockCreator.Object);

			var result = withholdingJournals.Create(new[] { apInvoice1, apInvoice2, apInvoice3 });

			var expectedErrorMessage =
@"INV1: Error 1.
INV3: Error 2.";
			AssertContainsExactElementsInAnyOrder("result.journalCollectionPerInvoice", new[] { withholdingJournalsPerInvoice2 }, result.journalCollectionPerInvoice);
			AssertEquals("result.errorMessage", expectedErrorMessage, result.errorMessage);
		}

		public void TestCreate_EmptyInvoiceCollection_ReturnsEmptyResult()
		{
			var mockCreator = new Mock<IWithholdingJournalCreatorForSingleInvoice>();
			var withholdingJournals = new WithholdingJournalCreatorForMultipleInvoices(mockCreator.Object);

			var result = withholdingJournals.Create(new List<InvoicingBase>());

			AssertEquals("result.journalCollectionPerInvoice.Count", 0, result.journalCollectionPerInvoice.Count);
			AssertEquals("result.errorMessage", string.Empty, result.errorMessage);
		}
	}
}

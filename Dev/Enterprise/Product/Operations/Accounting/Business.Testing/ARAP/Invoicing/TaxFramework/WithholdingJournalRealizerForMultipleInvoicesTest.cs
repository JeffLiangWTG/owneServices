using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class WithholdingJournalRealizerForMultipleInvoicesTest : TestCaseWithFactory
	{
		public void TestConstructorExceptionWhenWithholdingJournalRealizerIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>("Value cannot be null.\r\nParameter name: withholdingJournalRealizer", () => new WithholdingJournalRealizerForMultipleInvoices(null));
		}

		[ExpectNoExceptions]
		public void TestRealize_PassesSingleJournalToRealizer_CorrectJournalIsPassedToSingleRealizer()
		{
			var withholdingJournalsPerInvoice = new WithholdingJournalsPerInvoice(Factory.New<APInvoice>(), new[] { new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty) });
			var mockRealizer = new Mock<IWithholdingJournalRealizerForSingleInvoice>();
			mockRealizer.Setup(r => r.Realize(It.IsAny<WithholdingJournalsPerInvoice>())).Returns(string.Empty);
			var withholdingJournals = new WithholdingJournalRealizerForMultipleInvoices(mockRealizer.Object);

			withholdingJournals.Realize(new[] { withholdingJournalsPerInvoice });

			mockRealizer.Verify(r => r.Realize(withholdingJournalsPerInvoice));
		}

		[ExpectNoExceptions]
		public void TestRealize_PassesMultipleJournalsToRealizer_CorrectJournalsArePassedToSingleRealizer()
		{
			var withholdingJournalsPerInvoice1 = new WithholdingJournalsPerInvoice(Factory.New<APInvoice>(), new[] { new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty) });
			var withholdingJournalsPerInvoice2 = new WithholdingJournalsPerInvoice(Factory.New<APInvoice>(), new[] { new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty) });
			var mockRealizer = new Mock<IWithholdingJournalRealizerForSingleInvoice>();
			mockRealizer.Setup(r => r.Realize(It.IsAny<WithholdingJournalsPerInvoice>())).Returns(string.Empty);
			var withholdingJournals = new WithholdingJournalRealizerForMultipleInvoices(mockRealizer.Object);

			withholdingJournals.Realize(new[] { withholdingJournalsPerInvoice1, withholdingJournalsPerInvoice2 });

			mockRealizer.Verify(r => r.Realize(withholdingJournalsPerInvoice1));
			mockRealizer.Verify(r => r.Realize(withholdingJournalsPerInvoice1));
		}

		[ExpectNoExceptions]
		public void TestRealize_PassesEmptyJournalCollectionToRealizer_SingleRealizerIsNotCalled()
		{
			var mockRealizer = new Mock<IWithholdingJournalRealizerForSingleInvoice>();
			var withholdingJournals = new WithholdingJournalRealizerForMultipleInvoices(mockRealizer.Object);

			withholdingJournals.Realize(Array.Empty<WithholdingJournalsPerInvoice>());

			mockRealizer.Verify(r => r.Realize(It.IsAny<WithholdingJournalsPerInvoice>()), Times.Never);
		}

		public void TestRealize_SingleJournalWithoutErrors_ReturnsNoErrorMessages()
		{
			var withholdingJournalsPerInvoice = new WithholdingJournalsPerInvoice(Factory.New<APInvoice>(), new[] { new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty) });
			var mockRealizer = new Mock<IWithholdingJournalRealizerForSingleInvoice>();
			mockRealizer.Setup(r => r.Realize(It.IsAny<WithholdingJournalsPerInvoice>())).Returns(string.Empty);
			var withholdingJournals = new WithholdingJournalRealizerForMultipleInvoices(mockRealizer.Object);

			var result = withholdingJournals.Realize(new[] { withholdingJournalsPerInvoice });

			AssertEquals(string.Empty, result);
		}

		public void TestRealize_MultipleJournalsWithoutErrors_ReturnsNoErrorMessages()
		{
			var withholdingJournalsPerInvoice1 = new WithholdingJournalsPerInvoice(Factory.New<APInvoice>(), new[] { new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty) });
			var withholdingJournalsPerInvoice2 = new WithholdingJournalsPerInvoice(Factory.New<APInvoice>(), new[] { new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty) });
			var mockRealizer = new Mock<IWithholdingJournalRealizerForSingleInvoice>();
			mockRealizer.Setup(r => r.Realize(It.IsAny<WithholdingJournalsPerInvoice>())).Returns(string.Empty);
			var withholdingJournals = new WithholdingJournalRealizerForMultipleInvoices(mockRealizer.Object);

			var result = withholdingJournals.Realize(new[] { withholdingJournalsPerInvoice1, withholdingJournalsPerInvoice2 });

			AssertEquals(string.Empty, result);
		}

		public void TestRealize_SingleJournalWithError_ReturnsErrorMessage()
		{
			var invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = "INV123";
			var withholdingJournalsPerInvoice = new WithholdingJournalsPerInvoice(invoice, new[] { new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty) });
			var mockRealizer = new Mock<IWithholdingJournalRealizerForSingleInvoice>();
			mockRealizer.Setup(r => r.Realize(It.IsAny<WithholdingJournalsPerInvoice>())).Returns("Some error");
			var withholdingJournals = new WithholdingJournalRealizerForMultipleInvoices(mockRealizer.Object);

			var result = withholdingJournals.Realize(new[] { withholdingJournalsPerInvoice });

			AssertEquals("INV123: Some error", result);
		}

		public void TestRealize_MultipleJournalsWithMixedResults_ReturnsConcatenatedErrorMessages()
		{
			var errorMessage1 = $"Error for Invoice1";
			var errorMessage2 = $"Error for Invoice2";
			var invoice1 = Factory.New<APInvoice>();
			invoice1.AH_TransactionNum = "INV1";
			var invoice2 = Factory.New<APInvoice>();
			invoice2.AH_TransactionNum = "INV2";
			var withholdingJournalsPerInvoice1 = new WithholdingJournalsPerInvoice(invoice1, new[] { new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty) });
			var withholdingJournalsPerInvoice2 = new WithholdingJournalsPerInvoice(invoice2, new[] { new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty) });
			var mockRealizer = new Mock<IWithholdingJournalRealizerForSingleInvoice>();
			mockRealizer.Setup(r => r.Realize(withholdingJournalsPerInvoice1)).Returns(errorMessage1);
			mockRealizer.Setup(r => r.Realize(withholdingJournalsPerInvoice2)).Returns(errorMessage2);
			var withholdingJournals = new WithholdingJournalRealizerForMultipleInvoices(mockRealizer.Object);

			var result = withholdingJournals.Realize(new[] { withholdingJournalsPerInvoice1, withholdingJournalsPerInvoice2 });

			var expectedError =
@"INV1: Error for Invoice1
INV2: Error for Invoice2";
			AssertEquals(expectedError, result);
		}

		public void TestRealize_EmptyJournalCollection_ReturnsNoErrorMessages()
		{
			var mockRealizer = new Mock<IWithholdingJournalRealizerForSingleInvoice>();
			var withholdingJournals = new WithholdingJournalRealizerForMultipleInvoices(mockRealizer.Object);

			var result = withholdingJournals.Realize(Array.Empty<WithholdingJournalsPerInvoice>());

			AssertEquals(string.Empty, result);
		}
	}
}

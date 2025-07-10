using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.TaxFramework.Business;
using Moq;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class WithholdingJournalRealizerForSingleInvoiceTest : TestCaseWithFactory
	{
		public void TestJournalRealization_NoMatchTransactionDetailsFound()
		{
			var withholdingJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			var withholdingJournalProcessor = new WithholdingJournalRealizerForSingleInvoice(withholdingJournalCreationManagerMock.Object);
			var whtAmountLoaderMock = new Mock<IWHTAmountLoader>();
			TaxFrameworkObjectFactory.SubstituteWHTAmountLoader_ForTestOnly(Factory, whtAmountLoaderMock.Object);

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			whtAmountLoaderMock.Setup(x => x.GetNotionalWHT(apInvoice.PK)).Returns(10M);

			var journalForDisplayCollection = new WithholdingJournalsPerInvoice(apInvoice, Array.Empty<WithholdingJournalForDisplay>());
			InvoicingBase passedInvoice = null;
			withholdingJournalCreationManagerMock.Setup(x => x.GetAssociatedJournalDetails(It.IsAny<IMatching>()))
				.Callback<IMatching>((invoice) => passedInvoice = (InvoicingBase)invoice)
				.Returns(new List<IMatchTransactionDetails>());

			IWithholdingJournalRealizerForSingleInvoice withholdingJournalRealizer = withholdingJournalProcessor;
			var result = withholdingJournalRealizer.Realize(journalForDisplayCollection);
			withholdingJournalCreationManagerMock.Verify(x => x.GetAssociatedJournalDetails(passedInvoice), Times.Once);

			var expectedMessage = "An unexpected error has occurred. Please try again.";
			AssertEquals(expectedMessage, result);

			var expectedDeveloperErrorMessage = "No MatchTranasctionDetails is found in the dictionary inside WithholdingJournalCreationManager for the AP tranaction.";
			AssertEquals("LastExceptionReported.Message", expectedDeveloperErrorMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestJournalRealization_RealizesSuccesfully()
		{
			var taxProcessor = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessor.Object);

			var withholdingJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			var withholdingJournalProcessor = new WithholdingJournalRealizerForSingleInvoice(withholdingJournalCreationManagerMock.Object);

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();

			Factory.Save();

			var apJournal1 = Factory.NewWithValidTestData<APJournal>();
			var apJournal2 = Factory.NewWithValidTestData<APJournal>();

			var postDate1 = ZDate.Today.AddDays(-1);
			var description1 = "description 1";

			var postDate2 = ZDate.Today.AddDays(-10);
			var description2 = "description 2";

			var journalForDisplay1 = new WithholdingJournalForDisplay(apJournal1, ZDate.Empty);
			journalForDisplay1.PostDate = postDate1;
			journalForDisplay1.Description = description1;

			var journalForDisplay2 = new WithholdingJournalForDisplay(apJournal2, ZDate.Empty);
			journalForDisplay2.PostDate = postDate2;
			journalForDisplay2.Description = description2;

			var journalForDisplayCollection = new WithholdingJournalsPerInvoice(apInvoice, new[] { journalForDisplay1, journalForDisplay2 });

			var journalDetails1 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new ZGuid[] { ZGuid.NewZGuid() });
			journalDetails1.PK = apJournal1.PK;

			var journalDetails2 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new ZGuid[] { ZGuid.NewZGuid() });
			journalDetails2.PK = apJournal2.PK;

			var matchTransactionDetailsList = new List<IMatchTransactionDetails>() { journalDetails1, journalDetails2 };

			InvoicingBase passedInvoice = null;
			withholdingJournalCreationManagerMock.Setup(x => x.GetAssociatedJournalDetails(It.IsAny<IMatching>()))
				.Callback<IMatching>((invoice) => passedInvoice = (InvoicingBase)invoice)
				.Returns(matchTransactionDetailsList);

			ITaxRecordParent passedTaxParent = null;
			IEnumerable<IMatchTransactionDetails> passedMatchTransactionDetailsList = null;
			taxProcessor.Setup(x => x.ProcessPaymentRetentionTaxes(It.IsAny<ITaxRecordParent>(), It.IsAny<IEnumerable<IMatchTransactionDetails>>()))
				.Callback<ITaxRecordParent, IEnumerable<IMatchTransactionDetails>>((taxParent, listOfmatchTransactionDetails) => { passedTaxParent = taxParent; passedMatchTransactionDetailsList = listOfmatchTransactionDetails; })
				.Returns(string.Empty);

			IWithholdingJournalRealizerForSingleInvoice withholdingJournalRealizer = withholdingJournalProcessor;
			var result = withholdingJournalRealizer.Realize(journalForDisplayCollection);
			AssertEquals(string.Empty, result);

			taxProcessor.Verify(x => x.ProcessPaymentRetentionTaxes(passedTaxParent, passedMatchTransactionDetailsList), Times.Once);

			var passedFirstTransactionDetails = passedMatchTransactionDetailsList.First();
			AssertEquals("Passed transaction details 1 has correct post date", postDate1, passedFirstTransactionDetails.RealisationDate);

			var passedSecondTransactionDetails = passedMatchTransactionDetailsList.Skip(1).First();
			AssertEquals("Passed transaction details 2 has correct post date", postDate2, passedSecondTransactionDetails.RealisationDate);
		}

		public void TestJournalRealization_RealizationProcessReturnsError()
		{
			var taxProcessor = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessor.Object);

			var withholdingJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			var withholdingJournalProcessor = new WithholdingJournalRealizerForSingleInvoice(withholdingJournalCreationManagerMock.Object);

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();

			Factory.Save();

			var apJournal1 = Factory.NewWithValidTestData<APJournal>();
			var apJournal2 = Factory.NewWithValidTestData<APJournal>();

			var postDate1 = ZDate.Today.AddDays(-1);
			var description1 = "description 1";

			var postDate2 = ZDate.Today.AddDays(-10);
			var description2 = "description 2";

			var journalForDisplay1 = new WithholdingJournalForDisplay(apJournal1, ZDate.Empty);
			journalForDisplay1.PostDate = postDate1;
			journalForDisplay1.Description = description1;

			var journalForDisplay2 = new WithholdingJournalForDisplay(apJournal2, ZDate.Empty);
			journalForDisplay2.PostDate = postDate2;
			journalForDisplay2.Description = description2;

			var journalForDisplayCollection = new WithholdingJournalsPerInvoice(apInvoice, new[] { journalForDisplay1, journalForDisplay2 });

			var journalDetails1 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new ZGuid[] { ZGuid.NewZGuid() });
			journalDetails1.PK = apJournal1.PK;

			var journalDetails2 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new ZGuid[] { ZGuid.NewZGuid() });
			journalDetails2.PK = apJournal2.PK;

			var matchTransactionDetailsList = new List<IMatchTransactionDetails>() { journalDetails1, journalDetails2 };

			InvoicingBase passedInvoice = null;
			withholdingJournalCreationManagerMock.Setup(x => x.GetAssociatedJournalDetails(It.IsAny<IMatching>()))
				.Callback<IMatching>((invoice) => passedInvoice = (InvoicingBase)invoice)
				.Returns(matchTransactionDetailsList);

			var expectedErrorMessage = "Some error";
			ITaxRecordParent passedTaxParent = null;
			IEnumerable<IMatchTransactionDetails> passedMatchTransactionDetailsList = null;
			taxProcessor.Setup(x => x.ProcessPaymentRetentionTaxes(It.IsAny<ITaxRecordParent>(), It.IsAny<IEnumerable<IMatchTransactionDetails>>()))
				.Callback<ITaxRecordParent, IEnumerable<IMatchTransactionDetails>>((taxParent, listOfmatchTransactionDetails) => { passedTaxParent = taxParent; passedMatchTransactionDetailsList = listOfmatchTransactionDetails; })
				.Returns(expectedErrorMessage);

			IWithholdingJournalRealizerForSingleInvoice withholdingJournalRealizer = withholdingJournalProcessor;
			var result = withholdingJournalRealizer.Realize(journalForDisplayCollection);
			AssertEquals(expectedErrorMessage, result);

			taxProcessor.Verify(x => x.ProcessPaymentRetentionTaxes(passedTaxParent, passedMatchTransactionDetailsList), Times.Once);
		}
	}
}

using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.TaxFramework.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class TaxRealisationEnablerTest : TestCaseWithFactory
	{
		[TestDate(2020, 01, 15)]
		[ExpectNoExceptions]
		public void TestRealiseTaxIfApplicable_CalledForCorrectTypes()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice));
			var creditNote = testObjectCreator.CreateInvoice(typeof(APCreditNote));
			var adjustmentNote = testObjectCreator.CreateInvoice(typeof(ARAdjustmentNote));

			var payment = testObjectCreator.CreateAPPayment(1M, 100M, ZDate.Today, ZDate.Today, testObjectCreator.Creditor1.PK, testObjectCreator.AUDBankAccount.PK);

			var matchingCollection = new IMatchingCollection(Factory);

			matchingCollection.Add(invoice);
			matchingCollection.Add(creditNote);
			matchingCollection.Add(adjustmentNote);
			matchingCollection.Add(payment);

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			var expectedMatchDate = ZDate.Today;
			taxProcessorMock.Setup(x => x.ProcessTaxesOnMatching(It.IsAny<ITaxRecordParent>(), expectedMatchDate));
			taxProcessorMock.Setup(x => x.ProcessPaymentRetentionTaxes(It.IsAny<ITaxRecordParent>(), It.IsAny<IEnumerable<IMatchTransactionDetails>>())).Returns(string.Empty);

			ITaxRealisationEnabler taxRealisationEnabler = new TaxRealisationEnabler();
			var matchTranasctionDetails1 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new[] { ZGuid.NewZGuid() });

			var withholdingJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			withholdingJournalCreationManagerMock.Setup(x => x.GetAssociatedJournalDetails(It.IsAny<InvoicingBase>())).Returns(new List<IMatchTransactionDetails>() { matchTranasctionDetails1 });
			taxRealisationEnabler.RealiseTaxIfApplicable(matchingCollection, withholdingJournalCreationManagerMock.Object, expectedMatchDate);
			taxProcessorMock.Verify(x => x.ProcessTaxesOnMatching(It.IsAny<ITaxRecordParent>(), expectedMatchDate), Times.Exactly(3));
			taxProcessorMock.Verify(x => x.ProcessPaymentRetentionTaxes(It.IsAny<ITaxRecordParent>(), It.IsAny<IEnumerable<IMatchTransactionDetails>>()), Times.Exactly(3));
		}

		[TestDate(2020, 01, 15)]
		[ExpectNoExceptions]
		public void TestRealiseTaxIfApplicable()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice));
			var matchingCollection = new IMatchingCollection(Factory);

			matchingCollection.Add(invoice);

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			var expectedMatchDate = ZDate.Today;
			taxProcessorMock.Setup(x => x.ProcessTaxesOnMatching(It.IsAny<ITaxRecordParent>(), expectedMatchDate));
			taxProcessorMock.Setup(x => x.ProcessPaymentRetentionTaxes(It.IsAny<ITaxRecordParent>(), It.IsAny<IEnumerable<IMatchTransactionDetails>>())).Returns(string.Empty);

			ITaxRealisationEnabler taxRealisationEnabler = new TaxRealisationEnabler();
			taxRealisationEnabler.RealiseTaxIfApplicable(matchingCollection, null, expectedMatchDate);

			taxProcessorMock.Verify(x => x.ProcessTaxesOnMatching(It.IsAny<ITaxRecordParent>(), expectedMatchDate), Times.Exactly(1));
			taxProcessorMock.Verify(x => x.ProcessPaymentRetentionTaxes(It.IsAny<ITaxRecordParent>(), It.IsAny<IEnumerable<IMatchTransactionDetails>>()), Times.Never);

			taxProcessorMock.Invocations.Clear();

			var withholdingJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			withholdingJournalCreationManagerMock.Setup(x => x.GetAssociatedJournalDetails(It.IsAny<InvoicingBase>())).Returns(new List<IMatchTransactionDetails>() { });

			taxRealisationEnabler.RealiseTaxIfApplicable(matchingCollection, withholdingJournalCreationManagerMock.Object, expectedMatchDate);
			taxProcessorMock.Verify(x => x.ProcessTaxesOnMatching(It.IsAny<ITaxRecordParent>(), expectedMatchDate), Times.Exactly(1));
			taxProcessorMock.Verify(x => x.ProcessPaymentRetentionTaxes(It.IsAny<ITaxRecordParent>(), It.IsAny<IEnumerable<IMatchTransactionDetails>>()), Times.Never);

			taxProcessorMock.Invocations.Clear();

			taxProcessorMock.Setup(x => x.ProcessTaxesOnMatching(It.IsAny<ITaxRecordParent>(), expectedMatchDate));

			var matchTranasctionDetails1 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new[] { ZGuid.NewZGuid() });
			withholdingJournalCreationManagerMock.Setup(x => x.GetAssociatedJournalDetails(It.IsAny<InvoicingBase>())).Returns(new List<IMatchTransactionDetails>() { matchTranasctionDetails1 });

			taxRealisationEnabler.RealiseTaxIfApplicable(matchingCollection, withholdingJournalCreationManagerMock.Object, expectedMatchDate);

			taxProcessorMock.Verify(x => x.ProcessTaxesOnMatching(It.IsAny<ITaxRecordParent>(), expectedMatchDate), Times.Exactly(1));
			taxProcessorMock.Verify(x => x.ProcessPaymentRetentionTaxes(It.IsAny<ITaxRecordParent>(), It.IsAny<IEnumerable<IMatchTransactionDetails>>()), Times.Exactly(1));
		}
	}
}

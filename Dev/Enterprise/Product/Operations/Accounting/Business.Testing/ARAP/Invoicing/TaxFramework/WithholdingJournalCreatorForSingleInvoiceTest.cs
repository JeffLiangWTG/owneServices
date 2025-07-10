using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class WithholdingJournalCreatorForSingleInvoiceTest : TestCaseWithFactory
	{
		public void TestConstructorExceptionWhenWithholdingJournalCreationManagerIsNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: withholdingJournalCreationManager", () => new WithholdingJournalCreatorForSingleInvoice(null));
			AssertNoExceptionThrown(() => new WithholdingJournalCreatorForSingleInvoice(new Mock<IWithholdingJournalCreationManager>().Object));
		}

		[TestDate(2020, 05, 15)]
		public void TestJournalCreation_NoNotionalWHT()
		{
			var withholdingJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			var withholdingJournalProcessor = new WithholdingJournalCreatorForSingleInvoice(withholdingJournalCreationManagerMock.Object);
			var whtAmountLoaderMock = new Mock<IWHTAmountLoader>();
			TaxFrameworkObjectFactory.SubstituteWHTAmountLoader_ForTestOnly(Factory, whtAmountLoaderMock.Object);

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			whtAmountLoaderMock.Setup(x => x.GetNotionalWHT(apInvoice.PK)).Returns(0M);
			InvoicingBase passedInvoice = null;
			BusinessObjectFactory passedFactory = null;
			var today = ZDate.Today;
			withholdingJournalCreationManagerMock.Setup(x => x.CreateAPWithholdingJournalsIfApplicable(It.IsAny<IMatching>(), It.IsAny<BusinessObjectFactory>(), today))
				.Callback<IMatching, BusinessObjectFactory, ZDate>((invoice, factory, date) => { passedInvoice = (InvoicingBase)invoice; passedFactory = factory; })
				.Returns(new List<APJournal>());

			var expectedMessage = "No Journal can be created. There are no Notional Withholding Tax records to be realized.";

			IWithholdingJournalCreatorForSingleInvoice withholdingJournalCreator = withholdingJournalProcessor;
			var result = withholdingJournalCreator.Create(apInvoice);
			withholdingJournalCreationManagerMock.Verify(x => x.CreateAPWithholdingJournalsIfApplicable(passedInvoice, passedFactory, today), Times.Never);

			AssertEquals("Journal for display returned", 0, result.journalsPerInvoice.Journals.Count);
			AssertEquals("Error message", expectedMessage, result.errorMessage);
		}

		[TestDate(2020, 05, 15)]
		public void TestJournalCreation_NoJournalsCreated()
		{
			var withholdingJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			var withholdingJournalProcessor = new WithholdingJournalCreatorForSingleInvoice(withholdingJournalCreationManagerMock.Object);
			var whtAmountLoaderMock = new Mock<IWHTAmountLoader>();
			TaxFrameworkObjectFactory.SubstituteWHTAmountLoader_ForTestOnly(Factory, whtAmountLoaderMock.Object);

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			whtAmountLoaderMock.Setup(x => x.GetNotionalWHT(apInvoice.PK)).Returns(10M);
			InvoicingBase passedInvoice = null;
			BusinessObjectFactory passedFactory = null;
			var today = ZDate.Today;
			withholdingJournalCreationManagerMock.Setup(x => x.CreateAPWithholdingJournalsIfApplicable(It.IsAny<IMatching>(), It.IsAny<BusinessObjectFactory>(), today))
				.Callback<IMatching, BusinessObjectFactory, ZDate>((invoice, factory, date) => { passedInvoice = (InvoicingBase)invoice; passedFactory = factory; })
				.Returns(new List<APJournal>());

			var expectedMessage = "No Journal can be created. There are no Notional Withholding Tax records to be realized.";

			IWithholdingJournalCreatorForSingleInvoice withholdingJournalCreator = withholdingJournalProcessor;
			var result = withholdingJournalCreator.Create(apInvoice);
			withholdingJournalCreationManagerMock.Verify(x => x.CreateAPWithholdingJournalsIfApplicable(passedInvoice, passedFactory, today), Times.Once);

			AssertEquals("Journal for display returned", 0, result.journalsPerInvoice.Journals.Count);
			AssertEquals("Error message", expectedMessage, result.errorMessage);
		}

		[TestDate(2020, 05, 15)]
		public void TestJournalCreation_HasNotionalWHT()
		{
			var withholdingJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();
			var withholdingJournalProcessor = new WithholdingJournalCreatorForSingleInvoice(withholdingJournalCreationManagerMock.Object);
			var whtAmountLoaderMock = new Mock<IWHTAmountLoader>();
			TaxFrameworkObjectFactory.SubstituteWHTAmountLoader_ForTestOnly(Factory, whtAmountLoaderMock.Object);

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var apJournal1 = Factory.NewWithValidTestData<APJournal>();
			var apJournal2 = Factory.NewWithValidTestData<APJournal>();
			var returnValue = new List<APJournal>() { apJournal1, apJournal2 };
			whtAmountLoaderMock.Setup(x => x.GetNotionalWHT(apInvoice.PK)).Returns(10M);

			InvoicingBase passedInvoice = null;
			BusinessObjectFactory passedFactory = null;
			var today = ZDate.Today;
			withholdingJournalCreationManagerMock.Setup(x => x.CreateAPWithholdingJournalsIfApplicable(It.IsAny<IMatching>(), It.IsAny<BusinessObjectFactory>(), today))
				.Callback<IMatching, BusinessObjectFactory, ZDate>((invoice, factory, date) => { passedInvoice = (InvoicingBase)invoice; passedFactory = factory; })
				.Returns(returnValue);

			IWithholdingJournalCreatorForSingleInvoice withholdingJournalCreator = withholdingJournalProcessor;
			var result = withholdingJournalCreator.Create(apInvoice);
			withholdingJournalCreationManagerMock.Verify(x => x.CreateAPWithholdingJournalsIfApplicable(passedInvoice, passedFactory, today), Times.Once);

			AssertEquals("Journals for display returned", 2, result.journalsPerInvoice.Journals.Count);
			AssertEquals("Error message", string.Empty, result.errorMessage);
		}
	}
}

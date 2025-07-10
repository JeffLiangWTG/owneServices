using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class WithholdingJournalCreationManagerTest : TestCaseWithFactory
	{
		public void TestDependencies()
		{
			var whtJournalCreationManager = new WithholdingJournalCreationManager();
			AssertType(typeof(WithholdingAPJournalCreator), whtJournalCreationManager.WithholdingAPJournalCreator_ExposedForTestOnly);
		}

		public void TestShouldAPWithholdingJournalsBeCreated()
		{
			var withholdingJournalCreationManager = new WithholdingJournalCreationManager();
			var iWithholdingJournalCreationManager = withholdingJournalCreationManager as IWithholdingJournalCreationManager;

			Factory.SetContext(BusinessContext.RemittanceFileImport);
			Assert(!iWithholdingJournalCreationManager.ShouldAPWithholdingJournalsBeCreated(Factory));

			Factory.RemoveContext(BusinessContext.RemittanceFileImport);
			Assert(iWithholdingJournalCreationManager.ShouldAPWithholdingJournalsBeCreated(Factory));

			Factory.SetContext(BusinessContext.UniversalTransactionBatchImport);
			Assert(!iWithholdingJournalCreationManager.ShouldAPWithholdingJournalsBeCreated(Factory));

			Factory.RemoveContext(BusinessContext.UniversalTransactionBatchImport);
			Assert(iWithholdingJournalCreationManager.ShouldAPWithholdingJournalsBeCreated(Factory));
		}

		[TestDate(2020, 05, 15)]
		public void TestCreateAPWithholdingJournalsIfApplicable_JournalCreatorCalledForCorrectInvoiceTypes()
		{
			var whtJournalCreationManager = new WithholdingJournalCreationManager();
			var whtJournalCreatorMock = new Mock<IWithholdingAPJournalCreator>();
			whtJournalCreationManager.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreatorMock.Object);

			var matchDate = ZDate.Today;
			var returnTaxDetailsMock = new Mock<ITaxDetails>();
			returnTaxDetailsMock.SetupGet(t => t.JournalDetails).Returns(new Dictionary<APJournal, IMatchTransactionDetails>());
			whtJournalCreatorMock.Setup(x => x.Create(Factory, It.IsAny<InvoicingBase>(), matchDate)).Returns(returnTaxDetailsMock.Object);

			var iwhtJournalCreationManager = whtJournalCreationManager as IWithholdingJournalCreationManager;

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apInvoice, Factory, matchDate);
			whtJournalCreatorMock.Verify(x => x.Create(Factory, apInvoice, matchDate), Times.Once);

			var apCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apCreditNote, Factory, matchDate);
			whtJournalCreatorMock.Verify(x => x.Create(Factory, apCreditNote, matchDate), Times.Once);

			var arInvoice = Factory.NewWithValidTestData<ARCreditNote>();
			iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(arInvoice, Factory, matchDate);
			whtJournalCreatorMock.Verify(x => x.Create(Factory, arInvoice, matchDate), Times.Never);

			var apJournal = Factory.NewWithValidTestData<APJournal>();
			var result = iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apJournal, Factory, matchDate);
			AssertEquals(0, result.Count());
		}

		public void TestCreateAPWithholdingJournalsIfApplicable_WhenCreateCallReturnsNull()
		{
			var whtJournalCreationManager = new WithholdingJournalCreationManager();
			var whtJournalCreatorMock = new Mock<IWithholdingAPJournalCreator>();
			whtJournalCreationManager.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreatorMock.Object);
			var iwhtJournalCreationManager = whtJournalCreationManager as IWithholdingJournalCreationManager;

			var matchDate = ZDate.Today;
			whtJournalCreatorMock.Setup(x => x.Create(Factory, It.IsAny<InvoicingBase>(), matchDate)).Returns<ITaxDetails>(null);

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			var apJournalList = iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apInvoice, Factory, matchDate);

			whtJournalCreatorMock.Verify(x => x.Create(Factory, apInvoice, matchDate));
			AssertEquals("AP Journal count", 0, apJournalList.Count());
		}

		[TestDate(2020, 05, 15)]
		public void TestCreateAPWithholdingJournalsIfApplicable_ReturnsResultCorrectly()
		{
			var whtJournalCreationManager = new WithholdingJournalCreationManager();
			var whtJournalCreatorMock = new Mock<IWithholdingAPJournalCreator>();
			whtJournalCreationManager.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreatorMock.Object);
			var iwhtJournalCreationManager = whtJournalCreationManager as IWithholdingJournalCreationManager;
			var matchDate = ZDate.Today;

			var apJournal1 = Factory.NewWithValidTestData<APJournal>();
			var apJournal2 = Factory.NewWithValidTestData<APJournal>();
			var journalDetails = new Dictionary<APJournal, IMatchTransactionDetails>();
			journalDetails.Add(apJournal1, null);
			journalDetails.Add(apJournal2, null);

			var returnTaxDetailsMock = new Mock<ITaxDetails>();
			returnTaxDetailsMock.SetupGet(t => t.JournalDetails).Returns(journalDetails);

			var apInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			InvoicingBase passedInvoice = null;
			whtJournalCreatorMock.Setup(x => x.Create(Factory, It.IsAny<InvoicingBase>(), matchDate))
				.Callback<BusinessObjectFactory, InvoicingBase, ZDate>((factory, invoice, date) => passedInvoice = invoice)
				.Returns(returnTaxDetailsMock.Object);

			var result = iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apInvoice1, Factory, matchDate);
			whtJournalCreatorMock.Verify(x => x.Create(Factory, apInvoice1, matchDate), Times.Once);
			AssertEquals("Passed invoice", apInvoice1, passedInvoice);
			AssertEquals(2, result.Count());
			AssertContainsExactElementsInAnyOrder(new[] { apJournal1, apJournal2 }, result);

			var apJournal3 = Factory.NewWithValidTestData<APJournal>();
			journalDetails = new Dictionary<APJournal, IMatchTransactionDetails>();
			journalDetails.Add(apJournal3, null);

			returnTaxDetailsMock = new Mock<ITaxDetails>();
			returnTaxDetailsMock.SetupGet(t => t.JournalDetails).Returns(journalDetails);

			var apInvoice2 = Factory.NewWithValidTestData<APInvoice>();
			passedInvoice = null;
			whtJournalCreatorMock.Setup(x => x.Create(Factory, It.IsAny<InvoicingBase>(), matchDate))
				.Callback<BusinessObjectFactory, InvoicingBase, ZDate>((factory, invoice, date) => passedInvoice = invoice)
				.Returns(returnTaxDetailsMock.Object);

			result = iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apInvoice2, Factory, matchDate);
			whtJournalCreatorMock.Verify(x => x.Create(Factory, apInvoice2, matchDate), Times.Once);
			AssertEquals("Passed invoice", apInvoice2, passedInvoice);
			AssertEquals(1, result.Count());
			AssertContainsExactElementsInAnyOrder(new[] { apJournal3 }, result);
		}

		public void TestGetWithholdingJournalsToDelete_ReturnsCorrectType()
		{
			var whtJournalCreationManager = new WithholdingJournalCreationManager();

			var apJournal = Factory.NewWithValidTestData<APJournal>();
			var iwhtJournalCreationManager = whtJournalCreationManager as IWithholdingJournalCreationManager;
			var result = iwhtJournalCreationManager.GetWithholdingJournalsToDelete(apJournal);
			AssertEquals(typeof(List<APJournal>), result.GetType());
		}

		[TestDate(2020, 05, 15)]
		public void TestGetWithholdingJournalsToDelete_ProcessesCorrectTypesAsExpected()
		{
			var whtJournalCreationManager = new WithholdingJournalCreationManager();
			var whtJournalCreatorMock = new Mock<IWithholdingAPJournalCreator>();
			whtJournalCreationManager.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreatorMock.Object);

			var apJournal1 = Factory.NewWithValidTestData<APJournal>();
			var apJournal2 = Factory.NewWithValidTestData<APJournal>();
			var apJournal3 = Factory.NewWithValidTestData<APJournal>();
			var apJournal4 = Factory.NewWithValidTestData<APJournal>(); //not created as withholding journal
			var arJournal = Factory.NewWithValidTestData<ARJournal>(); //random other type of transaction

			var journalDetails = new Dictionary<APJournal, IMatchTransactionDetails>();
			journalDetails.Add(apJournal1, null);
			journalDetails.Add(apJournal2, null);
			journalDetails.Add(apJournal3, null);

			var returnTaxDetailsMock = new Mock<ITaxDetails>();
			returnTaxDetailsMock.SetupGet(t => t.JournalDetails).Returns(journalDetails);

			var matchDate = ZDate.Today;
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			whtJournalCreatorMock.Setup(x => x.Create(Factory, It.IsAny<InvoicingBase>(), matchDate)).Returns(returnTaxDetailsMock.Object);

			var iwhtJournalCreationManager = whtJournalCreationManager as IWithholdingJournalCreationManager;
			iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apInvoice, Factory, matchDate);

			var result = iwhtJournalCreationManager.GetWithholdingJournalsToDelete(arJournal);
			AssertEquals("Empty list returned as arJournal has unsupported type", 0, result.Count());
			Assert("arJournal is not deleted as well", !arJournal.IsDeleted);

			result = iwhtJournalCreationManager.GetWithholdingJournalsToDelete(apJournal4);
			AssertEquals("Empty list returned as apJournal3 is not linked to any invoice, hence is not a withholding journal", 0, result.Count());
			Assert("apJournal3 is not deleted as it is not treated as withholding journal", !apJournal4.IsDeleted);

			result = iwhtJournalCreationManager.GetWithholdingJournalsToDelete(apJournal3);
			AssertEquals("1 item returned", 1, result.Count());
			AssertEquals(apJournal3, result.First());
			Assert("apJournal3 is deleted", apJournal3.IsDeleted);

			result = iwhtJournalCreationManager.GetWithholdingJournalsToDelete(apJournal3);
			AssertEquals("Empty list returned as apJournal3 is not linked to invoice anymore as it has been removed from dictionary already", 0, result.Count());

			result = iwhtJournalCreationManager.GetWithholdingJournalsToDelete(apInvoice);
			AssertEquals("2 item returned", 2, result.Count());
			var expectedResult = new List<APJournal> { apJournal1, apJournal2 };
			AssertContainsExactElementsInAnyOrder(expectedResult, result);
			Assert("apJournal1 is deleted", apJournal1.IsDeleted);
			Assert("apJournal2 is deleted", apJournal2.IsDeleted);
		}

		[TestDate(2020, 05, 15)]
		public void TestGetWithholdingJournalsToDelete_AddingSameInvoiceSecondTimeDoesNotThrowException()
		{
			var whtJournalCreationManager = new WithholdingJournalCreationManager();
			var whtJournalCreatorMock = new Mock<IWithholdingAPJournalCreator>();
			whtJournalCreationManager.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreatorMock.Object);

			var apJournal = Factory.NewWithValidTestData<APJournal>();
			var journalDetails = new Dictionary<APJournal, IMatchTransactionDetails>();
			journalDetails.Add(apJournal, null);
			var returnTaxDetailsMock = new Mock<ITaxDetails>();
			returnTaxDetailsMock.SetupGet(t => t.JournalDetails).Returns(journalDetails);

			var matchDate = ZDate.Today;
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			whtJournalCreatorMock.Setup(x => x.Create(Factory, It.IsAny<InvoicingBase>(), matchDate)).Returns(returnTaxDetailsMock.Object);

			var iwhtJournalCreationManager = whtJournalCreationManager as IWithholdingJournalCreationManager;
			iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apInvoice, Factory, matchDate);

			var result = iwhtJournalCreationManager.GetWithholdingJournalsToDelete(apJournal);
			AssertEquals("1 item returned", 1, result.Count());
			AssertEquals(apJournal, result.First());
			Assert("apJournal is deleted", apJournal.IsDeleted);

			AssertNoExceptionThrown("Adding the same invoice second time", () => iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apInvoice, Factory, matchDate));
		}

		[TestDate(2020, 05, 15)]
		public void TestCheckIfAnyTransactionIsLinkedToWithholdingJournal_ProcessesCorrectTypesAsExpected()
		{
			var whtJournalCreationManager = new WithholdingJournalCreationManager();
			var whtJournalCreatorMock = new Mock<IWithholdingAPJournalCreator>();
			whtJournalCreationManager.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreatorMock.Object);

			var apInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			var apInvoice2 = Factory.NewWithValidTestData<APInvoice>();

			var apJournal1 = Factory.NewWithValidTestData<APJournal>();
			var apJournal2 = Factory.NewWithValidTestData<APJournal>();
			var apJournal3 = Factory.NewWithValidTestData<APJournal>(); //not created as withholding journal
			var arJournal = Factory.NewWithValidTestData<ARJournal>(); //random other type of transaction

			var journalDetails = new Dictionary<APJournal, IMatchTransactionDetails>();
			journalDetails.Add(apJournal1, null);
			journalDetails.Add(apJournal2, null);

			var returnTaxDetailsMock = new Mock<ITaxDetails>();
			returnTaxDetailsMock.SetupGet(t => t.JournalDetails).Returns(journalDetails);

			var matchDate = ZDate.Today;
			whtJournalCreatorMock.Setup(x => x.Create(Factory, It.IsAny<InvoicingBase>(), matchDate)).Returns(returnTaxDetailsMock.Object);

			var iwhtJournalCreationManager = whtJournalCreationManager as IWithholdingJournalCreationManager;
			iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apInvoice1, Factory, matchDate);

			var result = iwhtJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(new List<IMatching>() { apInvoice1 });
			Assert("apInvoice1 is linked to 2 withholding journals", result);

			result = iwhtJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(new List<IMatching>() { apInvoice2 });
			Assert("apInvoice2 is not linked to any withholding journals", !result);

			result = iwhtJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(new List<IMatching>() { apInvoice1, apInvoice2 });
			Assert("apInvoice1 is linked to 2 withholding journals", result);

			result = iwhtJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(new List<IMatching>() { apJournal1 });
			Assert("apJournal1 is in the dictionary", result);

			result = iwhtJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(new List<IMatching>() { apJournal2 });
			Assert("apJournal2 is in the dictionary", result);

			result = iwhtJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(new List<IMatching>() { apJournal3 });
			Assert("apJournal3 is not in the dictionary", !result);

			result = iwhtJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(new List<IMatching>() { apJournal1, apJournal2, apJournal3 });
			Assert("apJournal1 and apJournal2 is in the dictionary", result);

			result = iwhtJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(new List<IMatching>() { arJournal });
			Assert("arJournal is not the correct type", !result);
		}

		[TestDate(2020, 05, 15)]
		public void TestGetAssociatedJournalDetails()
		{
			var whtJournalCreationManager = new WithholdingJournalCreationManager();
			var whtJournalCreatorMock = new Mock<IWithholdingAPJournalCreator>();
			whtJournalCreationManager.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreatorMock.Object);

			var apJournal = Factory.NewWithValidTestData<APJournal>();

			var iwhtJournalCreationManager = whtJournalCreationManager as IWithholdingJournalCreationManager;
			var result = iwhtJournalCreationManager.GetAssociatedJournalDetails(apJournal);
			AssertEquals("Incorrect transaciton type", 0, result.Count());

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			result = iwhtJournalCreationManager.GetAssociatedJournalDetails(apJournal);
			AssertEquals("Transaction type is valid but the invoice does not have any associated entry in the dictionary", 0, result.Count());

			var apJournal1 = Factory.NewWithValidTestData<APJournal>();
			var jounalDetails1 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new ZGuid[] { ZGuid.NewZGuid() });

			var apJournal2 = Factory.NewWithValidTestData<APJournal>();
			var jounalDetails2 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new ZGuid[] { ZGuid.NewZGuid() });

			var journalDetails = new Dictionary<APJournal, IMatchTransactionDetails>();
			journalDetails.Add(apJournal1, jounalDetails1);
			journalDetails.Add(apJournal2, jounalDetails2);
			var returnTaxDetailsMock = new Mock<ITaxDetails>();
			returnTaxDetailsMock.SetupGet(t => t.JournalDetails).Returns(journalDetails);

			var matchDate = ZDate.Today;
			whtJournalCreatorMock.Setup(x => x.Create(Factory, It.IsAny<InvoicingBase>(), matchDate)).Returns(returnTaxDetailsMock.Object);

			var apInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apInvoice1, Factory, matchDate);
			result = iwhtJournalCreationManager.GetAssociatedJournalDetails(apInvoice1);

			var expectedResult = new List<IMatchTransactionDetails> { jounalDetails1, jounalDetails2 };
			AssertEquals(2, result.Count());
			AssertContainsExactElementsInAnyOrder(expectedResult, result);
		}

		[TestDate(2020, 05, 15)]
		public void TestDeleteAllWithholdingJournals()
		{
			var whtJournalCreationManager = new WithholdingJournalCreationManager();
			var whtJournalCreatorMock = new Mock<IWithholdingAPJournalCreator>();
			whtJournalCreationManager.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreatorMock.Object);

			var iwhtJournalCreationManager = whtJournalCreationManager as IWithholdingJournalCreationManager;

			var apJournal1 = Factory.NewWithValidTestData<APJournal>();
			var jounalDetails1 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new ZGuid[] { ZGuid.NewZGuid() });

			var apJournal2 = Factory.NewWithValidTestData<APJournal>();
			var jounalDetails2 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new ZGuid[] { ZGuid.NewZGuid() });

			var journalDetails = new Dictionary<APJournal, IMatchTransactionDetails>();
			journalDetails.Add(apJournal1, jounalDetails1);
			journalDetails.Add(apJournal2, jounalDetails2);
			var returnTaxDetailsMock = new Mock<ITaxDetails>();
			returnTaxDetailsMock.SetupGet(t => t.JournalDetails).Returns(journalDetails);

			var matchDate = ZDate.Today;
			whtJournalCreatorMock.Setup(x => x.Create(Factory, It.IsAny<InvoicingBase>(), matchDate)).Returns(returnTaxDetailsMock.Object);
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();

			iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apInvoice, Factory, matchDate);

			var result = iwhtJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(new List<IMatching>() { apInvoice });
			Assert("Precondition: apInvoice is linked to 2 withholding journals", result);

			iwhtJournalCreationManager.DeleteAllWithholdingJournals();

			result = iwhtJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(new List<IMatching>() { apInvoice });
			Assert("After calling DeleteAllWithholdingJournals() there is no item in the dictionary", !result);
			Assert("apJournal1 is deleted", apJournal1.IsDeleted);
			Assert("apJournal2 is deleted", apJournal2.IsDeleted);
		}

		[TestDate(2020, 05, 15)]
		public void TestResetCache()
		{
			var whtJournalCreationManager = new WithholdingJournalCreationManager();
			var whtJournalCreatorMock = new Mock<IWithholdingAPJournalCreator>();
			whtJournalCreationManager.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreatorMock.Object);

			var iwhtJournalCreationManager = whtJournalCreationManager as IWithholdingJournalCreationManager;

			var apJournal1 = Factory.NewWithValidTestData<APJournal>();
			var jounalDetails1 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new ZGuid[] { ZGuid.NewZGuid() });

			var apJournal2 = Factory.NewWithValidTestData<APJournal>();
			var jounalDetails2 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new ZGuid[] { ZGuid.NewZGuid() });

			var journalDetails = new Dictionary<APJournal, IMatchTransactionDetails>();
			journalDetails.Add(apJournal1, jounalDetails1);
			journalDetails.Add(apJournal2, jounalDetails2);
			var returnTaxDetailsMock = new Mock<ITaxDetails>();
			returnTaxDetailsMock.SetupGet(t => t.JournalDetails).Returns(journalDetails);

			var matchDate = ZDate.Today;
			whtJournalCreatorMock.Setup(x => x.Create(Factory, It.IsAny<InvoicingBase>(), matchDate)).Returns(returnTaxDetailsMock.Object);
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();

			iwhtJournalCreationManager.CreateAPWithholdingJournalsIfApplicable(apInvoice, Factory, matchDate);

			var result = iwhtJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(new List<IMatching>() { apInvoice });
			Assert("Precondition: apInvoice is linked to 2 withholding journals", result);

			iwhtJournalCreationManager.ResetCache();

			result = iwhtJournalCreationManager.CheckIfAnyTransactionIsLinkedToWithholdingJournal(new List<IMatching>() { apInvoice });
			Assert("After calling ResetCache() there is no item in the dictionary", !result);
		}

		public void TestCalculate()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var journalsWithMatchDetails = new Dictionary<APJournal, IMatchTransactionDetails>();
			journalsWithMatchDetails.Add(GetAPJournal(testObjectCreator, 20M), GetMatchTransactionDetails(20M));
			journalsWithMatchDetails.Add(GetAPJournal(testObjectCreator, 30M), GetMatchTransactionDetails(30M));
			journalsWithMatchDetails.Add(GetAPJournal(testObjectCreator, 40M), GetMatchTransactionDetails(40M));
			var taxDetails = GetTaxDetails(20M, 250M, journalsWithMatchDetails);

			var whtJournalCreatorMock = new Mock<IWithholdingAPJournalCreator>();
			var matchDate = ZDate.Today;
			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			whtJournalCreatorMock
				.Setup(x => x.Create(Factory, It.IsAny<InvoicingBase>(), matchDate))
				.Returns(taxDetails);
			var whtJournalCreationManager = new WithholdingJournalCreationManager();
			whtJournalCreationManager.SubstituteWithholdingJournalCreationManager_ForTestOnly(whtJournalCreatorMock.Object);
			(whtJournalCreationManager as IWithholdingJournalCreationManager).CreateAPWithholdingJournalsIfApplicable(apInvoice, Factory, matchDate);
			var notionalAmount = (whtJournalCreationManager as IWithholdingJournalCreationManager).CalculateNotionalWHT(apInvoice.PK);
			var realizeAmount = (whtJournalCreationManager as IWithholdingJournalCreationManager).CalculateRealizedWHT(apInvoice.PK);
			AssertEquals("Realized WHT (DB + In Memory)", 110M, realizeAmount);
			AssertEquals("Notional WHT (Current DB Notional - In Memory WHT", 160M, notionalAmount);

			(whtJournalCreationManager as IWithholdingJournalCreationManager).GetWithholdingJournalsToDelete(apInvoice);
			notionalAmount = (whtJournalCreationManager as IWithholdingJournalCreationManager).CalculateNotionalWHT(apInvoice.PK);
			realizeAmount = (whtJournalCreationManager as IWithholdingJournalCreationManager).CalculateRealizedWHT(apInvoice.PK);
			AssertNull(realizeAmount);
			AssertNull(notionalAmount);
		}

		ITaxDetails GetTaxDetails(ZDecimal realizedWHT, ZDecimal notionalWHT, Dictionary<APJournal, IMatchTransactionDetails> journalDetails)
		{
			var taxDetailsMock = new Mock<ITaxDetails>();
			taxDetailsMock.Setup(g => g.NotionalWHT).Returns(notionalWHT);
			taxDetailsMock.Setup(g => g.RealizedWHT).Returns(realizedWHT);
			taxDetailsMock.Setup(g => g.JournalDetails).Returns(journalDetails);
			return taxDetailsMock.Object;
		}

		APJournal GetAPJournal(TestObjectCreator objectCreator, ZDecimal localAmount)
		{
			var apJournal1 = objectCreator.CreateJournal<APJournal>(localAmount, ZDateTime.Today, objectCreator.ABIGAS.PK);
			apJournal1.DebitCreditSign = localAmount > 0 ? DebitCreditDataEntry.CR : DebitCreditDataEntry.DR;
			apJournal1.AH_LocalExTaxAmount = localAmount;
			return apJournal1;
		}
		IMatchTransactionDetails GetMatchTransactionDetails(ZDecimal localAmount)
		{
			var matchDetailsMock = new Mock<IMatchTransactionDetails>();
			matchDetailsMock.SetupGet(m => m.LocalAmount).Returns(localAmount);
			return matchDetailsMock.Object;
		}
	}
}

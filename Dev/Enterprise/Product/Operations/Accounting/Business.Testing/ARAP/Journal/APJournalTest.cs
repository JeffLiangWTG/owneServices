using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.ARAP.Journal.Testing
{
	[TestedType(typeof(APJournal))]
	public class APJournalTest : JournalTest
	{
		#region Implementation

		Guid WhtAccountPK;
		Guid MatchingAccountPK;

		protected override void SetUp()
		{
			base.SetUp();

			WhtAccountPK = Factory.NewWithValidTestData<AccGLHeader>().PK.ToGuid();
			MatchingAccountPK = Factory.NewWithValidTestData<AccGLHeader>().PK.ToGuid();

			AccountingConfigurationRegistry.Instance.WHTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WhtAccountPK);
			AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MatchingAccountPK);
		}

		#endregion

		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<APJournal>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestDefaults()
		{
			AssertEquals("description should default to AP JOURNAL",
				"AP JOURNAL", Header.AH_Desc);
			AssertEquals("AP Journal should default to DR",
				DebitCreditDataEntry.DR, ((APJournal)Header).DebitCreditSign);
		}

		public void TestWHTAccount()
		{
			APJournal aPJnl = Factory.NewWithValidTestData<APJournal>();
			AssertEquals(WhtAccountPK, aPJnl.WHTAccount_ForTestOnly);
		}

		public void TestMatchingAccount()
		{
			APJournal aPJnl = Factory.NewWithValidTestData<APJournal>();
			AssertEquals(MatchingAccountPK, aPJnl.MatchingAccount_ForTestOnly);
		}

		public void TestReverseAPJournal()
		{
			APJournal aPJnl = Factory.NewWithValidTestData<APJournal>();
			aPJnl.DebitCreditSign = DebitCreditDataEntry.CR;
			aPJnl.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			aPJnl.AH_ExchangeRate = 0.5M;
			aPJnl.AH_OSExTaxAmount = 70M;
			aPJnl.AH_OSTaxAmount = 7M;
			aPJnl.AH_OutstandingAmount = 154m;

			var taxProcessorMock = new Mock<ITaxProcessor>(MockBehavior.Strict);
			ObjectFactory.Substitute(taxProcessorMock.Object);

			Factory.Save();

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			APJournal loadedAPJnl = loadingFactory.Load<APJournal>(aPJnl.PK);

			ReversingBase journalReverser = new ReversingFactory().NewReversing(loadedAPJnl);
			journalReverser.Reverse();
			APJournal revJnl = loadedAPJnl.ReverseTransaction as APJournal;
			revJnl.AH_PostDate.AddDays(1);
			AssertNotEquals(loadedAPJnl.AH_PostDate, revJnl.AH_PostDate);

			taxProcessorMock.Setup(t => t.ProcessRealisedSPRAPTaxRecordsOnReversing(loadingFactory, aPJnl.PK, revJnl.PK, revJnl.AH_PostDate.Date));
			loadingFactory.Save();
			taxProcessorMock.Verify(t => t.ProcessRealisedSPRAPTaxRecordsOnReversing(loadingFactory, aPJnl.PK, revJnl.PK, revJnl.AH_PostDate.Date), Times.Once());

			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, aPJnl.PK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, aPJnl.AH_GC);
			revJnl = Factory.LoadTop1<APJournal>(filter);

			AssertNotNull("The reversing journal should be in the DB", revJnl);
			AssertEquals("Reversing APJournal should have AH_OSTotal = -77", -77M, revJnl.AH_OSTotal);
			AssertEquals("Reversing APJournal should have AH_InvoiceAmount = -140", -140M, revJnl.AH_InvoiceAmount);
			AssertEquals("Reversing APJournal should have AH_GSTAmount = -14", -14M, revJnl.AH_GSTAmount);
		}

		[ExpectNoExceptions]
		public void TestReverseAPJournal_ProcessRealisedSPRAPTaxRecordsOnReversingIsCalledBeforeGLMovementProcessor()
		{
			var org = TestObjectCreator.Creditor1;
			var journal = TestObjectCreator.CreateJournal<APJournal>(10, ZDateTime.Today, org.PK);
			journal.AH_TransactionCategory = TransactionCategory.Codes.PaymentBasisWithholding;

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: org);
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.OverheadChargeCode.PK, 100);

			var taxTransactionsParams = new CreateTaxTransactionParameters
			{
				TransactionHeader = apInvoice,
				OsTaxAmount = 10,
				LocalTaxAmount = 10,
				TaxBasis = TaxBasisList.PostingOnMatching.Code,
				AffectsSourceTransactionTotal = false,
				MatchTransaction = journal,
			};

			var taxTransaction = TFObjectCreator.CreateTaxTransaction(taxTransactionsParams);
			TFObjectCreator.CreateTaxTransactionLinePivot(taxTransaction.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(invoiceLine));

			Factory.Save();

			var loadingFactory = new BusinessObjectFactory();
			var loadedTaxTransaction = loadingFactory.Load<AccTaxTransaction>(taxTransaction.PK); //must be loaded before journal to be earlier in the list of objects in a Factory.
			var loadedJournal = loadingFactory.Load<APJournal>(journal.PK);

			var journalReverser = new ReversingFactory().NewReversing(loadedJournal);
			journalReverser.Reverse();

			var sequence = new MockSequence();
			var taxProcessorMock = new Mock<ITaxProcessor>(MockBehavior.Strict);
			taxProcessorMock.InSequence(sequence).Setup(t => t.ProcessRealisedSPRAPTaxRecordsOnReversing(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDate>())).Callback(() => loadedTaxTransaction.ATT_RealisationDate = ZDate.Today);
			ObjectFactory.Substitute(taxProcessorMock.Object);
			var glMovementProcessor = new Mock<IGLMovementProcessor>(MockBehavior.Strict); //it must be Strict to fail the sequence, otherwise it uses default method setup
			glMovementProcessor.InSequence(sequence).Setup(x => x.CreateGLMovements(It.IsAny<AccTaxTransaction>()));
			loadedTaxTransaction.SubstituteGLMovementProcessor_ForTestOnly(glMovementProcessor.Object);
			var tfDependencyFactory = new Mock<ITaxFrameworkDependencyFactory>();
			var emptyValidatorMock = new Mock<IAccTaxTransactionCriticalValidator>();
			tfDependencyFactory.Setup(x => x.GetAccTaxTransactionCriticalValidator(loadedTaxTransaction)).Returns(emptyValidatorMock.Object);
			ObjectFactory.Substitute(tfDependencyFactory.Object);
			loadingFactory.Save();
			taxProcessorMock.Verify(t => t.ProcessRealisedSPRAPTaxRecordsOnReversing(It.IsAny<BusinessObjectFactory>(), It.IsAny<ZGuid>(), It.IsAny<ZGuid>(), It.IsAny<ZDate>()), Times.Once());
			glMovementProcessor.Verify(x => x.CreateGLMovements(It.IsAny<AccTaxTransaction>()), Times.Once);
		}

		public void TestDocManagerCode()
		{
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "PJN", ((IDocManagerSupport)Header).DocManagerInfo.DocManagerCode);
		}

		TaxFrameworkTestObjectCreator TFObjectCreator => tfObjectCreator ?? (tfObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator tfObjectCreator;
	}
}

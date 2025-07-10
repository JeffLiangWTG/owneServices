using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	class TaxProcessorTest : TestCaseWithFactory
	{
		[UseSnapshotProtection(skipTransaction: true)]
		[TestDate(2022, 05, 25)]
		public void TestRecalculatePeriodForAllGLMovementRecords_NewPeriodsAreSet()
		{
			var taxConfigForCurrentCompany = TaxFrameworkObjectCreator.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany);
			var taxConfigForNonCurrentCompany = TaxFrameworkObjectCreator.ConfigureTaxFrameworkAtCompanyLevel(TestObjectCreator.NonCurrentCompany);

			TestObjectCreator.CreateTestPeriods(new ZDateTime(2022, 1, 1));
			InvoicingBase invoiceForCurrentCompany = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			InvoicingLineBase invoiceLineForCurrentCompany = TestObjectCreator.CreateInvoiceLine(invoiceForCurrentCompany, TestObjectCreator.CC1.PK, 100);
			InvoicingBase invoiceForNonCurrentCompany;
			InvoicingLineBase invoiceLineForNonCurrentCompany;
			using (SetTemporaryNonCurrentCompanyUserContext())
			{
				TestObjectCreator.CreateTestPeriods(new ZDateTime(2022, 1, 1));
				invoiceForNonCurrentCompany = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
				invoiceLineForNonCurrentCompany = TestObjectCreator.CreateInvoiceLine(invoiceForNonCurrentCompany, TestObjectCreator.CC1.PK, 100);
			}

			var taxTransactionForCurrentCompany = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters
			{
				Company = GlbCompany.CurrentCompany,
				TaxConfiguration = taxConfigForCurrentCompany,
				TransactionHeader = invoiceForCurrentCompany,
				OsTaxAmount = 10,
				LocalTaxAmount = 10,
				DoesNotCreateGLMovemetsOnSaving = true
			});
			TaxFrameworkObjectCreator.CreateTaxTransactionLinePivot(taxTransactionForCurrentCompany.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(invoiceLineForCurrentCompany));
			var glMovementForCurrentCompany = TaxFrameworkObjectCreator.CreateAccTaxGLMovement(taxTransactionForCurrentCompany.PK);

			var taxTransactionForNonCurrentCompany = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters
			{
				Company = TestObjectCreator.NonCurrentCompany,
				TaxConfiguration = taxConfigForNonCurrentCompany,
				TransactionHeader = invoiceForNonCurrentCompany,
				TaxBasis = TaxBasisList.Matching.Code,
				OsTaxAmount = 10,
				LocalTaxAmount = 10,
				DoesNotCreateGLMovemetsOnSaving = true
			});
			TaxFrameworkObjectCreator.CreateTaxTransactionLinePivot(taxTransactionForNonCurrentCompany.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(invoiceLineForNonCurrentCompany));
			var glMovementForNonCurrentCompany1 = TaxFrameworkObjectCreator.CreateAccTaxGLMovement(taxTransactionForNonCurrentCompany.PK, type: TaxGLMovementTypeList.Pending.Code);
			var expectedNonTodayDate = ZDate.Today.AddMonths(-2);
			var glMovementForNonCurrentCompany2 = TaxFrameworkObjectCreator.CreateAccTaxGLMovement(taxTransactionForNonCurrentCompany.PK, date: expectedNonTodayDate, type: TaxGLMovementTypeList.Realised.Code);

			Factory.Save();

			AssertEquals("Precondition: glMovementForCurrentCompany.ATM_Date", ZDate.Today, glMovementForCurrentCompany.ATM_Date);
			AssertEquals("Precondition: glMovementForCurrentCompany.ATM_Period", 202205, glMovementForCurrentCompany.ATM_Period);
			AssertEquals("Precondition: glMovementForNonCurrentCompany1.ATM_Date", ZDate.Today, glMovementForNonCurrentCompany1.ATM_Date);
			AssertEquals("Precondition: glMovementForNonCurrentCompany1.ATM_Period", 202205, glMovementForNonCurrentCompany1.ATM_Period);
			AssertEquals("Precondition: glMovementForNonCurrentCompany2.ATM_Date", expectedNonTodayDate, glMovementForNonCurrentCompany2.ATM_Date);
			AssertEquals("Precondition: glMovementForNonCurrentCompany2.ATM_Period", 202203, glMovementForNonCurrentCompany2.ATM_Period);

			TestObjectCreator.DeleteAllPeriodsForCurrentCompany();
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2021, 7, 1));
			using (SetTemporaryNonCurrentCompanyUserContext())
			{
				TestObjectCreator.DeleteAllPeriodsForCurrentCompany();
				TestObjectCreator.CreateTestPeriods(new ZDateTime(2021, 7, 1));
			}
			Factory.Save();

			var taxProcessor = new TaxProcessor();
			((ITaxProcessor)taxProcessor).RecalculatePeriodForAllGLMovementRecords(TestObjectCreator.NonCurrentCompany.PK);

			glMovementForCurrentCompany.Reload();
			glMovementForNonCurrentCompany1.Reload();
			glMovementForNonCurrentCompany2.Reload();
			AssertEquals("glMovementForCurrentCompany.ATM_Date", ZDate.Today, glMovementForCurrentCompany.ATM_Date);
			AssertEquals("glMovementForCurrentCompany.ATM_Period", 202205, glMovementForCurrentCompany.ATM_Period);
			AssertEquals("glMovementForNonCurrentCompany1.ATM_Date", ZDate.Today, glMovementForNonCurrentCompany1.ATM_Date);
			AssertEquals("glMovementForNonCurrentCompany1.ATM_Period", 202211, glMovementForNonCurrentCompany1.ATM_Period);
			AssertEquals("glMovementForNonCurrentCompany2.ATM_Date", expectedNonTodayDate, glMovementForNonCurrentCompany2.ATM_Date);
			AssertEquals("glMovementForNonCurrentCompany2.ATM_Period", 202209, glMovementForNonCurrentCompany2.ATM_Period);

			((ITaxProcessor)taxProcessor).RecalculatePeriodForAllGLMovementRecords(GlbCompany.CurrentCompany.PK);

			glMovementForCurrentCompany.Reload();
			glMovementForNonCurrentCompany1.Reload();
			glMovementForNonCurrentCompany2.Reload();
			AssertEquals("glMovementForCurrentCompany.ATM_Date", ZDate.Today, glMovementForCurrentCompany.ATM_Date);
			AssertEquals("glMovementForCurrentCompany.ATM_Period", 202211, glMovementForCurrentCompany.ATM_Period);
			AssertEquals("glMovementForNonCurrentCompany1.ATM_Date", ZDate.Today, glMovementForNonCurrentCompany1.ATM_Date);
			AssertEquals("glMovementForNonCurrentCompany1.ATM_Period", 202211, glMovementForNonCurrentCompany1.ATM_Period);
			AssertEquals("glMovementForNonCurrentCompany2.ATM_Date", expectedNonTodayDate, glMovementForNonCurrentCompany2.ATM_Date);
			AssertEquals("glMovementForNonCurrentCompany2.ATM_Period", 202209, glMovementForNonCurrentCompany2.ATM_Period);

			IDisposable SetTemporaryNonCurrentCompanyUserContext() => Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), TestObjectCreator.NonCurrentCompany.ActiveBranches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
		}

		[UseSnapshotProtection(skipTransaction: true)]
		[TestDate(2022, 05, 25)]
		public void TestRecalculatePeriodForAllGLMovementRecords_NoPeriodSetup_NoException_NoDataChange()
		{
			var taxConfigForCurrentCompany = TaxFrameworkObjectCreator.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany);

			TestObjectCreator.CreateTestPeriods(new ZDateTime(2022, 1, 1));
			InvoicingBase invoiceForCurrentCompany = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			InvoicingLineBase invoiceLineForCurrentCompany = TestObjectCreator.CreateInvoiceLine(invoiceForCurrentCompany, TestObjectCreator.CC1.PK, 100);

			var taxTransactionForCurrentCompany = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters
			{
				Company = GlbCompany.CurrentCompany,
				TaxConfiguration = taxConfigForCurrentCompany,
				TransactionHeader = invoiceForCurrentCompany,
				OsTaxAmount = 10,
				LocalTaxAmount = 10,
				DoesNotCreateGLMovemetsOnSaving = true
			});
			TaxFrameworkObjectCreator.CreateTaxTransactionLinePivot(taxTransactionForCurrentCompany.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(invoiceLineForCurrentCompany));
			var glMovementForCurrentCompany = TaxFrameworkObjectCreator.CreateAccTaxGLMovement(taxTransactionForCurrentCompany.PK);

			Factory.Save();

			AssertEquals("Precondition: glMovementForCurrentCompany.ATM_Date", ZDate.Today, glMovementForCurrentCompany.ATM_Date);
			AssertEquals("Precondition: glMovementForCurrentCompany.ATM_Period", 202205, glMovementForCurrentCompany.ATM_Period);

			TestObjectCreator.DeleteAllPeriodsForCurrentCompany();
			Factory.Save();

			var taxProcessor = new TaxProcessor();
			((ITaxProcessor)taxProcessor).RecalculatePeriodForAllGLMovementRecords(GlbCompany.CurrentCompany.PK);

			glMovementForCurrentCompany.Reload();
			AssertEquals("glMovementForCurrentCompany.ATM_Date", ZDate.Today, glMovementForCurrentCompany.ATM_Date);
			AssertEquals("glMovementForCurrentCompany.ATM_Period", 202205, glMovementForCurrentCompany.ATM_Period);
		}

		public void TestGetTaxExpenses()
		{
			var taxProcessor = new TaxProcessor();
			var taxLoaderMock = new Mock<ITaxRecordLoader>();
			var linePK = ZGuid.NewZGuid();
			taxLoaderMock.Setup(l => l.GetTaxExpenses(Factory, linePK)).Returns(new[] { (ZDate.Today.AddDays(-10), new ZDecimal(0.23m)) });
			taxProcessor.SubstituteTaxRecordLoader_ForTestOnly(taxLoaderMock.Object);
			taxLoaderMock.Verify(l => l.GetTaxExpenses(Factory, linePK), Times.Never());
			var taxExpenses = ((ITaxProcessor)taxProcessor).GetTaxExpenses(Factory, linePK);
			taxLoaderMock.Verify(l => l.GetTaxExpenses(Factory, linePK), Times.Once());
			AssertEquals(1, taxExpenses.Length);
			AssertEquals(ZDate.Today.AddDays(-10), taxExpenses[0].TaxExpenseDate);
			AssertEquals(0.23m, taxExpenses[0].TaxExpenseAmount);
		}

		[ExpectNoExceptions]
		public void TestUpdatePostDate()
		{
			var taxProcessor = new TaxProcessor();
			var taxCreatorMock = new Mock<ITaxRecordCreator>();
			taxProcessor.SubstituteTaxRecordCreator_ForTestOnly(taxCreatorMock.Object);
			var taxParentMock = new Mock<ITaxRecordParent>();

			((ITaxProcessor)taxProcessor).UpdatePostDate(taxParentMock.Object);
			taxCreatorMock.Verify(x => x.UpdatePostDate(taxParentMock.Object), Times.Once);
		}

		public void TestGetEstimatedTaxRecordsByTaxSystemCodes()
		{
			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var linePivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			var taxRecord2 = Factory.New<AccTaxTransaction>();
			var linePivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			var taxRecord3 = Factory.New<AccTaxTransaction>();
			var linePivot3 = Factory.New<AccTaxRecordTransactionLinePivot>();
			var taxRecord4 = Factory.New<AccTaxTransaction>();
			var linePivot4 = Factory.New<AccTaxRecordTransactionLinePivot>();

			var taxProcessor = new TaxProcessor();
			var taxCreatorMock = new Mock<ITaxRecordCreator>();
			taxProcessor.SubstituteTaxRecordCreator_ForTestOnly(taxCreatorMock.Object);

			var taxRecordsWithLinePivots = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			taxRecordsWithLinePivots.Add((taxRecord1, linePivot1));
			taxRecordsWithLinePivots.Add((taxRecord2, linePivot2));

			var taxParentMock = new Mock<ITaxRecordParent>();

			taxCreatorMock.Setup(x => x.GetEstimatedTaxRecords(taxParentMock.Object, null)).Returns(taxRecordsWithLinePivots);

			var result = ((ITaxProcessor)taxProcessor).GetEstimatedTaxRecordsByTaxSystemCodes(taxParentMock.Object, null);

			taxCreatorMock.Verify(x => x.GetEstimatedTaxRecords(taxParentMock.Object, null), Times.Once);
			AssertEquals(taxRecordsWithLinePivots, result.result);
			AssertEquals(string.Empty, result.errorMessage);

			taxRecordsWithLinePivots = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			taxRecordsWithLinePivots.Add((taxRecord3, linePivot3));

			var taxSystemsToFilterby1 = new List<ZString> { "TAX" };
			taxCreatorMock.Setup(x => x.GetEstimatedTaxRecords(taxParentMock.Object, taxSystemsToFilterby1)).Returns(taxRecordsWithLinePivots);
			result = ((ITaxProcessor)taxProcessor).GetEstimatedTaxRecordsByTaxSystemCodes(taxParentMock.Object, taxSystemsToFilterby1);

			taxCreatorMock.Verify(x => x.GetEstimatedTaxRecords(taxParentMock.Object, taxSystemsToFilterby1), Times.Once);
			AssertEquals(taxRecordsWithLinePivots, result.result);
			AssertEquals(string.Empty, result.errorMessage);

			taxRecordsWithLinePivots = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			taxRecordsWithLinePivots.Add((taxRecord4, linePivot4));

			var taxSystemsToFilterby2 = new List<ZString> { "TAX1" };
			taxCreatorMock.Setup(x => x.GetEstimatedTaxRecords(taxParentMock.Object, taxSystemsToFilterby2)).Returns(taxRecordsWithLinePivots);
			result = ((ITaxProcessor)taxProcessor).GetEstimatedTaxRecordsByTaxSystemCodes(taxParentMock.Object, taxSystemsToFilterby2);

			taxCreatorMock.Verify(x => x.GetEstimatedTaxRecords(taxParentMock.Object, taxSystemsToFilterby2), Times.Once);
			AssertEquals(taxRecordsWithLinePivots, result.result);
			AssertEquals(string.Empty, result.errorMessage);
		}

		public void TestGetEstimatedTaxRecords_CatchesTaxFrameworkInvalidDataException()
		{
			var taxProcessor = new TaxProcessor();
			var taxCreatorMock = new Mock<ITaxRecordCreator>();
			taxProcessor.SubstituteTaxRecordCreator_ForTestOnly(taxCreatorMock.Object);

			var expectedError = "Some Error";
			var taxRecordsWithLinePivots = new List<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)>();
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupProperty(x => x.IsTaxTransactionsCalculatedBeforePosting);

			taxCreatorMock.Setup(x => x.GetEstimatedTaxRecords(taxParentMock.Object, null)).Throws(new TaxFrameworkUnknownConfigurationValueException((NoResString)expectedError));

			IReadOnlyCollection<(AccTaxTransaction, AccTaxRecordTransactionLinePivot)> result;
			string errorMessage = string.Empty;
			AssertNoExceptionThrown(() => (result, errorMessage) = ((ITaxProcessor)taxProcessor).GetEstimatedTaxRecordsByTaxSystemCodes(taxParentMock.Object, null));
			AssertEquals("Returned error message", expectedError, errorMessage);

			AssertType("LastExceptionReported", typeof(TaxFrameworkUnknownConfigurationValueException), ErrorReporter.LastExceptionReported);
			AssertEquals("LastExceptionReported.Message", expectedError, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestGetEstimatedTaxRecordsByTaxSystemCodes_CatchesGeneralException()
		{
			var taxProcessor = new TaxProcessor();
			var taxCreatorMock = new Mock<ITaxRecordCreator>();
			taxProcessor.SubstituteTaxRecordCreator_ForTestOnly(taxCreatorMock.Object);

			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupProperty(x => x.IsTaxTransactionsCalculatedBeforePosting);

			taxCreatorMock.Setup(x => x.GetEstimatedTaxRecords(taxParentMock.Object, null)).Throws(new NullReferenceException());

			AssertExceptionThrown<NullReferenceException>(() => ((ITaxProcessor)taxProcessor).GetEstimatedTaxRecordsByTaxSystemCodes(taxParentMock.Object, null));

			AssertNull("LastExceptionReported", ErrorReporter.LastExceptionReported);
		}

		public void TestGetEstimatedTaxRecordsByTaxSystemCodes_DoesNotCatchCriticalException()
		{
			var taxProcessor = new TaxProcessor();
			var taxCreatorMock = new Mock<ITaxRecordCreator>();
			taxProcessor.SubstituteTaxRecordCreator_ForTestOnly(taxCreatorMock.Object);

			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupProperty(x => x.IsTaxTransactionsCalculatedBeforePosting);

			taxCreatorMock.Setup(x => x.GetEstimatedTaxRecords(taxParentMock.Object, null)).Throws(new OutOfMemoryException());

			AssertExceptionThrown<OutOfMemoryException>(() => ((ITaxProcessor)taxProcessor).GetEstimatedTaxRecordsByTaxSystemCodes(taxParentMock.Object, null));

			taxParentMock.VerifySet(x => x.IsTaxTransactionsCalculatedBeforePosting = It.IsAny<bool>(), Times.Never);
			taxCreatorMock.Verify(x => x.DeleteTaxRecordsNotInDB(taxParentMock.Object), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestProcessOnParentReversing()
		{
			var taxProcessor = new TaxProcessor();
			var taxReverserMock = new Mock<ITaxRecordReverser>();
			taxProcessor.SubstituteTaxRecordReverser_ForTestOnly(taxReverserMock.Object);
			var taxParentMock = new Mock<ITaxRecordParent>();
			var reversedTaxParentMock = new Mock<ITaxRecordParent>();

			var realisationDate = ZDate.Today.AddDays(-10);

			((ITaxProcessor)taxProcessor).ProcessOnParentReversing(taxParentMock.Object, reversedTaxParentMock.Object, realisationDate);
			taxReverserMock.Verify(x => x.ReverseNotRealisedSPRAPRecords(taxParentMock.Object));
			taxReverserMock.Verify(x => x.ReverseNonSPRAPTaxRecords(taxParentMock.Object, reversedTaxParentMock.Object, realisationDate), Times.Once);
		}

		public void TestUpdatePostDateOnParentReversing()
		{
			var taxProcessor = new TaxProcessor();
			var taxReverserMock = new Mock<ITaxRecordReverser>();
			taxProcessor.SubstituteTaxRecordReverser_ForTestOnly(taxReverserMock.Object);

			var taxParentMock = new Mock<ITaxRecordParent>();
			var reversedTaxParentMock = new Mock<ITaxRecordParent>();
			var realisationDate = ZDate.Empty;

#if NETFRAMEWORK
			AssertExceptionThrown<ArgumentNullException>("originalTaxParentParameter is null", "Value cannot be null.\r\nParameter name: originalTaxParent", () => ((ITaxProcessor)taxProcessor).UpdatePostDateOnParentReversing(null, reversedTaxParentMock.Object, realisationDate));
			taxReverserMock.Verify(x => x.UpdatePostDateOnNonSPRAPTaxRecords(It.IsAny<ITaxRecordParent>(), reversedTaxParentMock.Object, realisationDate), Times.Never);

			AssertExceptionThrown<ArgumentNullException>("reversedTaxParent parameter is null", "Value cannot be null.\r\nParameter name: reversedTaxParent", () => ((ITaxProcessor)taxProcessor).UpdatePostDateOnParentReversing(taxParentMock.Object, null, realisationDate));
			taxReverserMock.Verify(x => x.UpdatePostDateOnNonSPRAPTaxRecords(taxParentMock.Object, It.IsAny<ITaxRecordParent>(), realisationDate), Times.Never);
#else
			var ex = AssertExceptionThrown<ArgumentNullException>(() => ((ITaxProcessor)taxProcessor).UpdatePostDateOnParentReversing(null, reversedTaxParentMock.Object, realisationDate));
			Assert("originalTaxParentParameter is null message", ex.Message.Equals("Value cannot be null. (Parameter 'originalTaxParent')"));
			Assert("originalTaxParentParameter is null", ex.ParamName.Equals("originalTaxParent"));

			var ex0 = AssertExceptionThrown<ArgumentNullException>(() => ((ITaxProcessor)taxProcessor).UpdatePostDateOnParentReversing(taxParentMock.Object, null, realisationDate));
			Assert("reversedTaxParent parameter is null message", ex0.Message.Equals("Value cannot be null. (Parameter 'reversedTaxParent')"));
			Assert("reversedTaxParent parameter is null", ex0.ParamName.Equals("reversedTaxParent"));
#endif
			((ITaxProcessor)taxProcessor).UpdatePostDateOnParentReversing(taxParentMock.Object, reversedTaxParentMock.Object, realisationDate);
			taxReverserMock.Verify(x => x.UpdatePostDateOnNonSPRAPTaxRecords(taxParentMock.Object, reversedTaxParentMock.Object, realisationDate), Times.Once);

			realisationDate = ZDate.Today.AddDays(-10);
			((ITaxProcessor)taxProcessor).UpdatePostDateOnParentReversing(taxParentMock.Object, reversedTaxParentMock.Object, realisationDate);
			taxReverserMock.Verify(x => x.UpdatePostDateOnNonSPRAPTaxRecords(taxParentMock.Object, reversedTaxParentMock.Object, realisationDate), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestHasRealisedAPPaymentRetentionRecords()
		{
			var taxProcessor = new TaxProcessor();
			var taxLoaderMock = new Mock<ITaxRecordLoader>();
			taxProcessor.SubstituteTaxRecordLoader_ForTestOnly(taxLoaderMock.Object);
			var taxParentMock = new Mock<ITaxRecordParent>();

			((ITaxProcessor)taxProcessor).HasRealisedAPPaymentRetentionRecords(taxParentMock.Object);
			taxLoaderMock.Verify(x => x.HasRealisedSPRAPTaxRecordsInDB(taxParentMock.Object));
		}

		[SuspendCriticalValidation, ExpectNoExceptions]
		public void TestDeleteTaxRecordNotInDB()
		{
			var taxProcessor = new TaxProcessor();
			var taxCreatorMock = new Mock<ITaxRecordCreator>();
			taxProcessor.SubstituteTaxRecordCreator_ForTestOnly(taxCreatorMock.Object);
			var taxParentMock = new Mock<ITaxRecordParent>();

			var iTaxProcessor = taxProcessor as ITaxProcessor;
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			iTaxProcessor.DeleteTaxRecordNotInDB(taxParentMock.Object, taxRecord);
			taxCreatorMock.Verify(t => t.DeleteTaxRecordNotInDB(taxParentMock.Object, taxRecord), Times.Once);

			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "INV1");
			taxRecord.ATT_AH = invoice.PK;
			Factory.Save();

			taxCreatorMock.Reset();
			iTaxProcessor.DeleteTaxRecordNotInDB(taxParentMock.Object, taxRecord);
			taxCreatorMock.Verify(t => t.DeleteTaxRecordNotInDB(It.IsAny<ITaxRecordParent>(), It.IsAny<AccTaxTransaction>()), Times.Never);
		}

		public void TestITaxProcessorInObjectFactory()
		{
			AssertEquals(typeof(TaxProcessor), ObjectFactory.GetType<ITaxProcessor>());
		}

		public void TestDependencies()
		{
			var taxProcessor = new TaxProcessor();
			AssertType<TaxRecordCreator>(taxProcessor.TaxRecordCreator_ExposedForTestOnly);
			AssertType<TaxRecordRealiser>(taxProcessor.TaxRecordRealiser_ExposedForTestOnly);
			AssertType<TaxRecordReverser>(taxProcessor.TaxRecordReverser_ExposedForTestOnly);
		}

		public void TestProcessTaxesOnPosting()
		{
			var taxProcessor = new TaxProcessor();
			var taxCreatorMock = new Mock<ITaxRecordCreator>(MockBehavior.Strict);
			taxProcessor.SubstituteTaxRecordCreator_ForTestOnly(taxCreatorMock.Object);

			var taxParent = new Mock<ITaxRecordParent>();
			taxParent.SetupProperty(x => x.IsTaxTransactionsCalculatedBeforePosting);
			taxCreatorMock.Setup(x => x.CreateTaxRecords(taxParent.Object));

			var iTaxProcessor = (ITaxProcessor)taxProcessor;
			var errorMessage = iTaxProcessor.ProcessTaxesOnPosting(taxParent.Object);
			AssertEquals(nameof(errorMessage), "", errorMessage);
			taxCreatorMock.Verify(x => x.CreateTaxRecords(taxParent.Object), Times.Once);
			taxParent.VerifySet(x => x.IsTaxTransactionsCalculatedBeforePosting = true);
		}

		public void TestProcessTaxesOnPosting_CriticalException()
		{
			var taxProcessor = new TaxProcessor();
			var taxCreatorMock = new Mock<ITaxRecordCreator>();
			taxProcessor.SubstituteTaxRecordCreator_ForTestOnly(taxCreatorMock.Object);

			var taxParent = new Mock<ITaxRecordParent>();
			taxParent.SetupProperty(x => x.IsTaxTransactionsCalculatedBeforePosting);
			taxCreatorMock.Setup(x => x.CreateTaxRecords(taxParent.Object)).Throws<OutOfMemoryException>();

			var iTaxProcessor = (ITaxProcessor)taxProcessor;
			AssertExceptionThrown<OutOfMemoryException>(() => iTaxProcessor.ProcessTaxesOnPosting(taxParent.Object));

			taxParent.VerifySet(x => x.IsTaxTransactionsCalculatedBeforePosting = It.IsAny<bool>(), Times.Never);
			taxCreatorMock.Verify(x => x.DeleteTaxRecordsNotInDB(It.IsAny<ITaxRecordParent>()), Times.Never);
		}

		public void TestProcessTaxesOnPosting_Exception_InCreateTaxRecords()
		{
			var taxProcessor = new TaxProcessor();
			var taxCreatorMock = new Mock<ITaxRecordCreator>();
			taxProcessor.SubstituteTaxRecordCreator_ForTestOnly(taxCreatorMock.Object);

			var taxParent = new Mock<ITaxRecordParent>();
			taxParent.SetupProperty(x => x.IsTaxTransactionsCalculatedBeforePosting);
			taxCreatorMock.Setup(x => x.CreateTaxRecords(It.IsAny<ITaxRecordParent>())).Throws<NullReferenceException>();

			var iTaxProcessor = (ITaxProcessor)taxProcessor;
			AssertExceptionThrown<NullReferenceException>(() => iTaxProcessor.ProcessTaxesOnPosting(taxParent.Object));

			taxParent.VerifySet(x => x.IsTaxTransactionsCalculatedBeforePosting = false);
			taxCreatorMock.Verify(x => x.DeleteTaxRecordsNotInDB(taxParent.Object), Times.Once);
		}

		public void TestProcessTaxesOnPosting_TaxFrameworkUnknownConfigurationValueException_InCreateTaxRecords()
		{
			var taxProcessor = new TaxProcessor();
			var taxCreatorMock = new Mock<ITaxRecordCreator>();
			taxProcessor.SubstituteTaxRecordCreator_ForTestOnly(taxCreatorMock.Object);

			var taxParent = new Mock<ITaxRecordParent>();
			taxParent.SetupProperty(x => x.IsTaxTransactionsCalculatedBeforePosting);
			var expectedError = "Some error";
			taxCreatorMock.Setup(x => x.CreateTaxRecords(It.IsAny<ITaxRecordParent>())).Throws(new TaxFrameworkUnknownConfigurationValueException((NoResString)expectedError));

			var iTaxProcessor = (ITaxProcessor)taxProcessor;
			var errorMessage = iTaxProcessor.ProcessTaxesOnPosting(taxParent.Object);
			AssertEquals(nameof(errorMessage), expectedError, errorMessage);
			AssertType("LastExceptionReported", typeof(TaxFrameworkUnknownConfigurationValueException), ErrorReporter.LastExceptionReported);
			AssertEquals("LastExceptionReported.Message", expectedError, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();

			taxParent.VerifySet(x => x.IsTaxTransactionsCalculatedBeforePosting = false);
			taxCreatorMock.Verify(x => x.DeleteTaxRecordsNotInDB(taxParent.Object), Times.Once);
		}

		public void TestProcessTaxesOnPosting_TaxFrameworkInvalidDataException_InCreateTaxRecords()
		{
			var taxProcessor = new TaxProcessor();
			var taxCreatorMock = new Mock<ITaxRecordCreator>();
			taxProcessor.SubstituteTaxRecordCreator_ForTestOnly(taxCreatorMock.Object);

			var taxParent = new Mock<ITaxRecordParent>();
			taxParent.SetupProperty(x => x.IsTaxTransactionsCalculatedBeforePosting);
			var expectedError = "Some error";
			taxCreatorMock.Setup(x => x.CreateTaxRecords(It.IsAny<ITaxRecordParent>())).Throws(new TaxFrameworkInvalidDataException((NoResString)expectedError));

			var iTaxProcessor = (ITaxProcessor)taxProcessor;
			var errorMessage = iTaxProcessor.ProcessTaxesOnPosting(taxParent.Object);
			AssertEquals(nameof(errorMessage), expectedError, errorMessage);
			AssertNull("LastExceptionReported", ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();

			taxParent.VerifySet(x => x.IsTaxTransactionsCalculatedBeforePosting = false);
			taxCreatorMock.Verify(x => x.DeleteTaxRecordsNotInDB(taxParent.Object), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestDeleteTaxesNoInDB()
		{
			var taxProcessor = new TaxProcessor();
			var taxCreatorMock = new Mock<ITaxRecordCreator>(MockBehavior.Strict);
			taxProcessor.SubstituteTaxRecordCreator_ForTestOnly(taxCreatorMock.Object);

			var taxParent = new Mock<ITaxRecordParent>();
			taxParent.SetupProperty(x => x.IsTaxTransactionsCalculatedBeforePosting);
			taxCreatorMock.Setup(x => x.DeleteTaxRecordsNotInDB(taxParent.Object));

			var iTaxProcessor = (ITaxProcessor)taxProcessor;
			iTaxProcessor.DeleteTaxesNotInDB(taxParent.Object);

			taxParent.VerifySet(x => x.IsTaxTransactionsCalculatedBeforePosting = false);
			taxCreatorMock.Verify(x => x.DeleteTaxRecordsNotInDB(taxParent.Object), Times.Once);
		}

		public void TestDeleteTaxesNoInDB_WhenTaxRecordCreatorThrowsInvalidOperationException()
		{
			var taxProcessor = new TaxProcessor();
			var taxCreatorMock = new Mock<ITaxRecordCreator>(MockBehavior.Strict);
			taxProcessor.SubstituteTaxRecordCreator_ForTestOnly(taxCreatorMock.Object);

			var taxParent = new Mock<ITaxRecordParent>();
			taxCreatorMock.Setup(x => x.DeleteTaxRecordsNotInDB(taxParent.Object)).Throws(new InvalidOperationException("Tax records for posted transaction cannot be deleted."));

			var iTaxProcessor = (ITaxProcessor)taxProcessor;
			AssertNoExceptionThrown(() => iTaxProcessor.DeleteTaxesNotInDB(taxParent.Object));

			taxCreatorMock.Verify(x => x.DeleteTaxRecordsNotInDB(taxParent.Object), Times.Once);
			AssertType("LastExceptionReported", typeof(InvalidOperationException), ErrorReporter.LastExceptionReported);
			AssertEquals("LastExceptionReported.Message", "Tax records for posted transaction cannot be deleted.", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestDeleteTaxesNoInDB_AllDependentObjectsCanBeDeleted()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: testObjectCreator.TestOrganisation);
			var line = testObjectCreator.CreateInvoiceLine(invoice, 100);

			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			var taxLinePivot = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord.PK))[0];
			taxLinePivot.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(line));

			var taxProcessor = new TaxProcessor();
			var iTaxProcessor = (ITaxProcessor)taxProcessor;
			iTaxProcessor.DeleteTaxesNotInDB(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice));

			Assert("taxRecord.IsDeleted", taxRecord.IsDeleted);
			Assert("taxLinePivot.IsDeleted", taxLinePivot.IsDeleted);
			Assert("invoice.IsDeleted", !invoice.IsDeleted);
			Assert("line.IsDeleted", !line.IsDeleted);

			AssertNoExceptionThrown("All dependent objects are deleted, nothing left in factory and it can be saved", () => Factory.Save());
			Assert("invoice.IsInDatabase", invoice.IsInDatabase);
			Assert("line.IsInDatabase", line.IsInDatabase);
		}

		[TestDate(2020, 01, 15)]
		public void TestProcessTaxesOnMatching()
		{
			var taxProcessor = new TaxProcessor();

			var taxRealiserMock = new Mock<ITaxRecordRealiser>();
			taxProcessor.SubstituteTaxRecordRealiser_ForTestOnly(taxRealiserMock.Object);

			var realisationDate = ZDate.Today;
			var taxParentMock = new Mock<ITaxRecordParent>();

			var itaxProcessor = (ITaxProcessor)taxProcessor;
			AssertNoExceptionThrown(() => itaxProcessor.ProcessTaxesOnMatching(taxParentMock.Object, realisationDate));

			taxRealiserMock.Verify(x => x.RealiseTaxRecord(taxParentMock.Object, realisationDate), Times.Once);
		}

		[TestDate(2020, 01, 15)]
		public void TestProcessPaymentRetentionTaxes()
		{
			var taxProcessor = new TaxProcessor();

			var taxRealiserMock = new Mock<ITaxRecordRealiser>();
			taxProcessor.SubstituteTaxRecordRealiser_ForTestOnly(taxRealiserMock.Object);

			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var taxRecord2 = Factory.New<AccTaxTransaction>();
			var taxRecords = new[] { taxRecord1, taxRecord2 };

			var matchTranasctionDetails1 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new[] { taxRecord1.PK, taxRecord2.PK });

			var taxParentMock = new Mock<ITaxRecordParent>();
			taxRealiserMock.Setup(x => x.RealisePaymentRetentionTaxRecords(taxParentMock.Object, It.IsAny<IEnumerable<IMatchTransactionDetails>>())).Returns(string.Empty);

			var matchTransactionDetailsList = new List<IMatchTransactionDetails>() { matchTranasctionDetails1 };
			var itaxProcessor = (ITaxProcessor)taxProcessor;
			AssertNoExceptionThrown(() => itaxProcessor.ProcessPaymentRetentionTaxes(taxParentMock.Object, matchTransactionDetailsList));

			taxRealiserMock.Verify(x => x.RealisePaymentRetentionTaxRecords(taxParentMock.Object, matchTransactionDetailsList), Times.Once);
		}

		[TestDate(2020, 01, 15)]
		public void TestProcessPaymentRetentionTaxes_ReturnsErrorMessage()
		{
			var taxProcessor = new TaxProcessor();

			var taxRealiserMock = new Mock<ITaxRecordRealiser>();
			taxProcessor.SubstituteTaxRecordRealiser_ForTestOnly(taxRealiserMock.Object);

			var matchTranasctionDetails1 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, Array.Empty<ZGuid>());

			var taxParentMock = new Mock<ITaxRecordParent>();
			taxRealiserMock.Setup(x => x.RealisePaymentRetentionTaxRecords(taxParentMock.Object, It.IsAny<IEnumerable<IMatchTransactionDetails>>())).Returns("Some error");

			var matchTransactionDetailsList = new List<IMatchTransactionDetails>() { matchTranasctionDetails1 };
			var itaxProcessor = (ITaxProcessor)taxProcessor;
			AssertNoExceptionThrown(() => itaxProcessor.ProcessPaymentRetentionTaxes(taxParentMock.Object, matchTransactionDetailsList));

			taxRealiserMock.Verify(x => x.RealisePaymentRetentionTaxRecords(taxParentMock.Object, matchTransactionDetailsList), Times.Once);
		}

		public void TestProcessPaymentRetentionTaxesOnMatching_CatchesTaxFrameworkUnknownConfigurationValueException()
		{
			var taxProcessor = new TaxProcessor();

			var taxRealiserMock = new Mock<ITaxRecordRealiser>();
			taxProcessor.SubstituteTaxRecordRealiser_ForTestOnly(taxRealiserMock.Object);

			var expectedError = "Some error";
			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var taxRecord2 = Factory.New<AccTaxTransaction>();
			var taxRecords = new[] { taxRecord1, taxRecord2 };

			var matchTranasctionDetails1 = new MatchTransactionDetails(ZGuid.Empty, ZGuid.Empty, ZString.Empty, 0M, 0M, ZGuid.Empty, new[] { taxRecord1.PK, taxRecord2.PK });

			var taxParentMock = new Mock<ITaxRecordParent>();
			taxRealiserMock.Setup(x => x.RealisePaymentRetentionTaxRecords(taxParentMock.Object, It.IsAny<IEnumerable<IMatchTransactionDetails>>())).Throws(new TaxFrameworkUnknownConfigurationValueException((NoResString)expectedError));

			var matchTransactionDetailsList = new List<IMatchTransactionDetails>() { matchTranasctionDetails1 };
			var iTaxProcessor = (ITaxProcessor)taxProcessor;
			var errorMessage = iTaxProcessor.ProcessPaymentRetentionTaxes(taxParentMock.Object, matchTransactionDetailsList);

			AssertEquals(nameof(errorMessage), expectedError, errorMessage);
			AssertType("LastExceptionReported", typeof(TaxFrameworkUnknownConfigurationValueException), ErrorReporter.LastExceptionReported);
			AssertEquals("LastExceptionReported.Message", expectedError, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestGetPaymentRetentionMatchTransactionDetails()
		{
			var company = GlbCompany.CurrentCompany;
			var localCurrency = TestObjectCreator.AUD;
			var osCurrency = TestObjectCreator.USD;

			var glAccount = TestObjectCreator.GLHeader1;
			var branch1 = TestObjectCreator.CreateBranch("BR1", company);
			var branch2 = TestObjectCreator.CreateBranch("BR2", company);

			var department1 = TestObjectCreator.CreateDepartment("DE1");
			var department2 = TestObjectCreator.CreateDepartment("DE2");

			var serviceCode1 = "SER1";
			var serviceCode2 = "SER2";

			int rateNumerator1 = 10;
			int rateNumerator2 = 20;

			int rateDenominator = 1;

			var taxSystemSPR = TaxFrameworkObjectCreator.CreateTaxSystem("SPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			var taxSystemNonSPR = TaxFrameworkObjectCreator.CreateTaxSystem("NSPR", taxSuperType: TaxSuperTypeList.Perceptions.Code);

			var taxConfigurationSPR1 = TaxFrameworkObjectCreator.CreateTaxConfiguration(company, null, taxSystemSPR, LedgerTypes.AccountsPayable);
			taxConfigurationSPR1.ETC_AG_TaxControlAccount = glAccount.PK;
			var taxConfigurationSPR2 = TaxFrameworkObjectCreator.CreateTaxConfiguration(company, null, taxSystemSPR, LedgerTypes.AccountsPayable);
			taxConfigurationSPR2.ETC_AG_TaxControlAccount = glAccount.PK;

			var taxConfigurationNonSPR = TaxFrameworkObjectCreator.CreateTaxConfiguration(company, null, taxSystemNonSPR, LedgerTypes.AccountsPayable);

			var header1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1001", localCurrency, 1M, TestObjectCreator.Creditor1);
			var taxParentMock1 = new Mock<ITaxRecordParent>();
			var taxRecordParent1 = taxParentMock1.Object;
			taxParentMock1.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock1.SetupGet(x => x.PK).Returns(header1.PK);
			taxParentMock1.SetupGet(x => x.Company).Returns(GlbCompany.CurrentCompany);

			var taxRecord_1_10 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationNonSPR, TransactionHeader = header1, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.Perceptions.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = localCurrency, LocalTaxAmount = 10M, OsTaxAmount = 10M });
			var taxRecord_1_20 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header1, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = localCurrency, LocalTaxAmount = 20M, OsTaxAmount = 20M });
			var taxRecord_1_30 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR2, TransactionHeader = header1, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = localCurrency, LocalTaxAmount = 30M, OsTaxAmount = 30M });
			var taxRecord_1_40 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header1, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch2, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = localCurrency, LocalTaxAmount = 40M, OsTaxAmount = 40M });
			var taxRecord_1_50 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header1, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department2, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = localCurrency, LocalTaxAmount = 50M, OsTaxAmount = 50M });
			var taxRecord_1_60 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header1, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator2, rateDenominator), ServiceCode = serviceCode1, Currency = localCurrency, LocalTaxAmount = 60M, OsTaxAmount = 60M });
			var taxRecord_1_70 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header1, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode2, Currency = localCurrency, LocalTaxAmount = 70M, OsTaxAmount = 70M });
			var taxRecord_1_80 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header1, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode2, Currency = osCurrency, LocalTaxAmount = 80M, OsTaxAmount = 160M });
			var taxRecord_1_90 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header1, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode2, Currency = localCurrency, LocalTaxAmount = 90M, OsTaxAmount = 90M, RealisationDate = ZDate.Today });
			var taxRecord_1_100 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header1, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode2, Currency = localCurrency, LocalTaxAmount = 100M, OsTaxAmount = 100M, IsCancelled = true });
			var taxRecord_1_110 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header1, Ledger = TaxConfigurationLedgers.AccountsReceivable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = localCurrency, LocalTaxAmount = 110M, OsTaxAmount = 110M });

			var header2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1002", localCurrency, 1M, TestObjectCreator.Creditor1);
			var taxParentMock2 = new Mock<ITaxRecordParent>();
			var taxRecordParent2 = taxParentMock2.Object;
			taxParentMock2.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock2.SetupGet(x => x.PK).Returns(header2.PK);
			taxParentMock2.SetupGet(x => x.Company).Returns(GlbCompany.CurrentCompany);

			var taxRecord_2_10 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationNonSPR, TransactionHeader = header2, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.Perceptions.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = osCurrency, LocalTaxAmount = 10M, OsTaxAmount = 15M });
			var taxRecord_2_20 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header2, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = osCurrency, LocalTaxAmount = 20M, OsTaxAmount = 0M });
			var taxRecord_2_30 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR2, TransactionHeader = header2, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = osCurrency, LocalTaxAmount = 30M, OsTaxAmount = 35M });
			var taxRecord_2_40 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header2, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch2, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = osCurrency, LocalTaxAmount = 0M, OsTaxAmount = 45M });
			var taxRecord_2_50 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header2, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department2, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = osCurrency, LocalTaxAmount = 50M, OsTaxAmount = 55M });
			var taxRecord_2_60 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header2, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator2, rateDenominator), ServiceCode = serviceCode1, Currency = osCurrency, LocalTaxAmount = 60M, OsTaxAmount = 65M });
			var taxRecord_2_70 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header2, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode2, Currency = osCurrency, LocalTaxAmount = 70M, OsTaxAmount = 75M });
			var taxRecord_2_80 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header2, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = osCurrency, LocalTaxAmount = 80M, OsTaxAmount = 85M, RealisationDate = ZDate.Today });
			var taxRecord_2_90 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header2, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = osCurrency, LocalTaxAmount = 80M, OsTaxAmount = 85M, IsCancelled = true });
			var taxRecord_2_100 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header2, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode2, Currency = osCurrency, LocalTaxAmount = 100M, OsTaxAmount = 150M });

			var header3 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "1003", localCurrency, 1M, TestObjectCreator.Creditor1);
			var taxParentMock3 = new Mock<ITaxRecordParent>();
			var taxRecordParent3 = taxParentMock3.Object;
			taxParentMock3.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock3.SetupGet(x => x.PK).Returns(header3.PK);
			taxParentMock3.SetupGet(x => x.Company).Returns(GlbCompany.CurrentCompany);

			var taxRecord_3_10 = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header3, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = branch1, Department = department1, TaxRate = (rateNumerator1, rateDenominator), ServiceCode = serviceCode1, Currency = osCurrency, LocalTaxAmount = 0M, OsTaxAmount = 0M });

			var header4 = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "1004", localCurrency, 1M, TestObjectCreator.Creditor1);
			var taxParentMock4 = new Mock<ITaxRecordParent>();
			var taxRecordParent4 = taxParentMock4.Object;
			taxParentMock4.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock4.SetupGet(x => x.PK).Returns(header4.PK);
			taxParentMock4.SetupGet(x => x.Company).Returns(GlbCompany.CurrentCompany);

			ITaxProcessor taxProcessor = new TaxProcessor();
			var result = taxProcessor.GetPaymentRetentionMatchTransactionDetails(taxRecordParent1);
			AssertEquals("Realized Amount", 90M, result.RealizedWHT);
			AssertEquals("Notional Amount", 350M, result.NotionalWHT);
			AssertEquals(6, result.MatchDetails.Length);
			var expectedResultForHeader = new[]
			{
				new MatchTransactionDetails(branch1.PK, department1.PK, localCurrency.RX_Code, 150M, 230M, glAccount.PK, new[] { taxRecord_1_70.PK, taxRecord_1_80.PK }), //different service code
				new MatchTransactionDetails(branch1.PK, department1.PK, localCurrency.RX_Code, 60M, 60M, glAccount.PK, new[] { taxRecord_1_60.PK }), //different rate
				new MatchTransactionDetails(branch1.PK, department2.PK, localCurrency.RX_Code, 50M, 50M, glAccount.PK, new[] { taxRecord_1_50.PK }), //different department
				new MatchTransactionDetails(branch2.PK, department1.PK, localCurrency.RX_Code, 40M, 40M, glAccount.PK, new[] { taxRecord_1_40.PK }), //different branch
				new MatchTransactionDetails(branch1.PK, department1.PK, localCurrency.RX_Code, 30M, 30M, glAccount.PK, new[] { taxRecord_1_30.PK }), //different tax config
				new MatchTransactionDetails(branch1.PK, department1.PK, localCurrency.RX_Code, 20M, 20M, glAccount.PK, new[] { taxRecord_1_20.PK }), //base line
			};

			AssertMatchTransactionDetails("Header 1");

			result = taxProcessor.GetPaymentRetentionMatchTransactionDetails(taxRecordParent2);
			AssertEquals("Realized Amount", 80M, result.RealizedWHT);
			AssertEquals("Notional Amount", 330M, result.NotionalWHT);
			AssertEquals(6, result.MatchDetails.Length);
			expectedResultForHeader = new[]
			{
				new MatchTransactionDetails(branch1.PK, department1.PK, osCurrency.RX_Code, 170M, 225M, glAccount.PK, new[] { taxRecord_2_70.PK, taxRecord_2_100.PK }), //different service code
				new MatchTransactionDetails(branch1.PK, department1.PK, osCurrency.RX_Code, 60M, 65M, glAccount.PK, new[] { taxRecord_2_60.PK }), //different rate
				new MatchTransactionDetails(branch1.PK, department2.PK, osCurrency.RX_Code, 50M, 55M, glAccount.PK, new[] { taxRecord_2_50.PK }), //different department
				new MatchTransactionDetails(branch1.PK, department1.PK, osCurrency.RX_Code, 30M, 35M, glAccount.PK, new[] { taxRecord_2_30.PK }), //different tax config
				new MatchTransactionDetails(branch1.PK, department1.PK, osCurrency.RX_Code, 20M, 0M, glAccount.PK, new[] { taxRecord_2_20.PK }), //base line
				new MatchTransactionDetails(branch2.PK, department1.PK, osCurrency.RX_Code, 0M, 45M, glAccount.PK, new[] { taxRecord_2_40.PK }), //different branch
			};

			AssertMatchTransactionDetails("Header 2");

			result = taxProcessor.GetPaymentRetentionMatchTransactionDetails(taxRecordParent3);
			AssertEquals("Realized Amount", 0M, result.RealizedWHT);
			AssertEquals("Notional Amount", 0M, result.NotionalWHT);
			AssertEquals(1, result.MatchDetails.Length);

			expectedResultForHeader = new[]
			{
				new MatchTransactionDetails(branch1.PK, department1.PK, osCurrency.RX_Code, 0M, 0M, glAccount.PK, new[] { taxRecord_3_10.PK }),
			};

			AssertMatchTransactionDetails("Header 3");

			result = taxProcessor.GetPaymentRetentionMatchTransactionDetails(taxRecordParent4);
			AssertNull(result.MatchDetails);

			void AssertMatchTransactionDetails(string headerString)
			{
				var details = result.MatchDetails.OrderByDescending(x => x.LocalAmount).ToArray();
				for (int i = 0; i < details.Length; i++)
				{
					AssertEquals(headerString + "-Branch", expectedResultForHeader[i].BranchPK, details[i].BranchPK);
					AssertEquals(headerString + "-Department", expectedResultForHeader[i].DepartmenPK, details[i].DepartmenPK);
					AssertEquals(headerString + "-Currency", expectedResultForHeader[i].Currency, details[i].Currency);
					AssertEquals(headerString + "-Local Amount", expectedResultForHeader[i].LocalAmount, details[i].LocalAmount);
					AssertEquals(headerString + "-OS Amount", expectedResultForHeader[i].OSAmount, details[i].OSAmount);
					AssertEquals(headerString + "-GL account PK", expectedResultForHeader[i].GLAccountPK, details[i].GLAccountPK);
					AssertContainsExactElementsInAnyOrder(headerString + "-Tax record PKS", expectedResultForHeader[i].GetTaxRecordPKs(), details[i].GetTaxRecordPKs());
				}
			}
		}

		public void TestGetPaymentRetentionMatchTransactionDetails_WhenMatchDetailsListEmpty()
		{
			var taxSystemSPR = TaxFrameworkObjectCreator.CreateTaxSystem("SPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);

			var taxConfigurationSPR1 = TaxFrameworkObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, null, taxSystemSPR, LedgerTypes.AccountsPayable);
			taxConfigurationSPR1.ETC_AG_TaxControlAccount = TestObjectCreator.GLHeader1.PK;

			var header = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "1000", TestObjectCreator.AUD, 1M, TestObjectCreator.Creditor1);
			var taxParentMock = new Mock<ITaxRecordParent>();
			var taxRecordParent = taxParentMock.Object;
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.PK).Returns(header.PK);
			taxParentMock.SetupGet(x => x.Company).Returns(GlbCompany.CurrentCompany);

			var taxRecord = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfigurationSPR1, TransactionHeader = header, Ledger = TaxConfigurationLedgers.AccountsPayable.Code, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, Branch = TestObjectCreator.NonCurrentBranch, Department = TestObjectCreator.NonCurrentDepartment, TaxRate = (10, 1), ServiceCode = "SER1", Currency = TestObjectCreator.USD, LocalTaxAmount = 15M, OsTaxAmount = 10M, RealisationDate = ZDate.Today });

			ITaxProcessor taxProcessor = new TaxProcessor();
			var (notionalWHT, realizedWHT, matchDetails) = taxProcessor.GetPaymentRetentionMatchTransactionDetails(taxRecordParent);
			AssertEquals("Realized Amount", 15M, realizedWHT);
			AssertEquals("Notional Amount", 0M, notionalWHT);
			AssertEquals(0, matchDetails.Length);
		}

		[ExpectNoExceptions]
		public void TestGetTaxTransactions()
		{
			var taxProcessor = new TaxProcessor();
			var taxLoaderMock = new Mock<ITaxRecordLoader>();
			taxProcessor.SubstituteTaxRecordLoader_ForTestOnly(taxLoaderMock.Object);
			var taxParentMock = new Mock<ITaxRecordParent>();

			((ITaxProcessor)taxProcessor).GetTaxTransactions(taxParentMock.Object);
			taxLoaderMock.Verify(x => x.LoadAllReportableTaxRecords(taxParentMock.Object), Times.Once);
		}

		public void TestGetTaxDetailsForAccountingJournal()
		{
			var taxProcessor = new TaxProcessor();
			var taxLoaderMock = new Mock<ITaxRecordLoader>();
			taxProcessor.SubstituteTaxRecordLoader_ForTestOnly(taxLoaderMock.Object);
			var taxParentMock = new Mock<ITaxRecordParent>();
			var taxParent = taxParentMock.Object;

			var result = new List<IGLMovementDetails>();
			taxLoaderMock.Setup(x => x.LoadGLMovementDetails(taxParent.Factory, taxParent.PK)).Returns(result);
			var actualResult = ((ITaxProcessor)taxProcessor).GetTaxDetailsForAccountingJournal(taxParent.Factory, taxParent.PK);
			taxLoaderMock.Verify(x => x.LoadGLMovementDetails(taxParent.Factory, taxParent.PK), Times.Once);

			AssertEquals(result, actualResult);
		}

		#region LoadTaxRecordPivots

		public void TestLoadTaxRecordPivots_ReturnsPivotsReturnedByTaxRecordLoader()
		{
			var taxProcessor = new TaxProcessor();
			var mockTaxRecordLoader = new Mock<ITaxRecordLoader>();
			taxProcessor.SubstituteTaxRecordLoader_ForTestOnly(mockTaxRecordLoader.Object);

			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();

			var expectedPivot = new AccTaxRecordTransactionLinePivot[] { pivot1 };
			mockTaxRecordLoader.Setup(x => x.LoadTaxRecordPivots(It.IsAny<bool>(), It.IsAny<AccTaxTransaction[]>())).Returns(expectedPivot);

			var pivots = ((ITaxProcessor)taxProcessor).LoadTaxRecordPivots(It.IsAny<AccTaxTransaction[]>());
			AssertContainsExactElementsInAnyOrder("Setup with 1 pivot", expectedPivot, pivots);

			var expected2Pivots = new AccTaxRecordTransactionLinePivot[] { pivot1, pivot2 };
			mockTaxRecordLoader.Setup(x => x.LoadTaxRecordPivots(It.IsAny<bool>(), It.IsAny<AccTaxTransaction[]>())).Returns(expected2Pivots);

			pivots = ((ITaxProcessor)taxProcessor).LoadTaxRecordPivots(It.IsAny<AccTaxTransaction[]>());
			AssertContainsExactElementsInAnyOrder("Setup with 2 pivots", expected2Pivots, pivots);
		}

		public void TestLoadTaxRecordPivots_WhenTaxRecordLoaderReturnsEmptyPivots()
		{
			var taxProcessor = new TaxProcessor();
			var mockTaxRecordLoader = new Mock<ITaxRecordLoader>();
			taxProcessor.SubstituteTaxRecordLoader_ForTestOnly(mockTaxRecordLoader.Object);

			var emptyPivotsArray = Array.Empty<AccTaxRecordTransactionLinePivot>();
			mockTaxRecordLoader.Setup(x => x.LoadTaxRecordPivots(It.IsAny<bool>(), It.IsAny<AccTaxTransaction[]>())).Returns(emptyPivotsArray);

			var pivots = ((ITaxProcessor)taxProcessor).LoadTaxRecordPivots(It.IsAny<AccTaxTransaction[]>());
			AssertContainsExactElementsInAnyOrder(emptyPivotsArray, pivots);
		}

		[ExpectNoExceptions]
		public void TestLoadTaxRecordPivots_InvokesTaxRecordLoader_WithProvidedInputTaxRecords()
		{
			var taxProcessor = new TaxProcessor();
			var mockTaxRecordLoader = new Mock<ITaxRecordLoader>();
			taxProcessor.SubstituteTaxRecordLoader_ForTestOnly(mockTaxRecordLoader.Object);

			var pivots = ((ITaxProcessor)taxProcessor).LoadTaxRecordPivots();
			mockTaxRecordLoader.Verify(x => x.LoadTaxRecordPivots(false));

			var taxRecord1 = Factory.New<AccTaxTransaction>();

			pivots = ((ITaxProcessor)taxProcessor).LoadTaxRecordPivots(taxRecord1);
			mockTaxRecordLoader.Verify(x => x.LoadTaxRecordPivots(false, taxRecord1));

			var taxRecord2 = Factory.New<AccTaxTransaction>();

			pivots = ((ITaxProcessor)taxProcessor).LoadTaxRecordPivots(taxRecord1, taxRecord2);
			mockTaxRecordLoader.Verify(x => x.LoadTaxRecordPivots(false, taxRecord1, taxRecord2));
		}

		#endregion

		#region GetTaxTransactionDataForDataTransfer

		public void TestGetTaxTransactionDataForDataTransfer_ReturnsEmptyList_WhenTFDataTransferReturnsEmptyListOfDataObjects()
		{
			var taxFrameworkDataTransferMock = new Mock<ITaxFrameworkDataTransfer>();
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();
			var taxProcessor = CreateTestTaxProcessor(taxRecordLoader: taxRecordLoaderMock.Object, taxFrameworkDataTransfer: taxFrameworkDataTransferMock.Object);

			taxFrameworkDataTransferMock.Setup(x => x.CreateTaxRecordDataFromTaxTransactions(It.IsAny<ITaxRecordParent>(), It.IsAny<AccTaxTransaction[]>(), It.IsAny<AccTaxRecordTransactionLinePivot[]>())).Returns(Array.Empty<IReadOnlyTaxRecordData>());

			var taxParentMock = new Mock<ITaxRecordParent>();

			var taxRecordData = taxProcessor.GetTaxRecordDataForDataTransfer(taxParentMock.Object);
			AssertEquals("When TaxFrameworkDataTransfer returns empty list of tax record data", Array.Empty<IReadOnlyTaxRecordData>(), taxRecordData);
		}

		public void TestGetTaxTransactionDataForDataTransfer_ReturnsNull_WhenTFDataTransferReturnsNullDataObjectsList()
		{
			var taxFrameworkDataTransferMock = new Mock<ITaxFrameworkDataTransfer>();
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();
			var taxProcessor = CreateTestTaxProcessor(taxRecordLoader: taxRecordLoaderMock.Object, taxFrameworkDataTransfer: taxFrameworkDataTransferMock.Object);

			taxFrameworkDataTransferMock.Setup(x => x.CreateTaxRecordDataFromTaxTransactions(It.IsAny<ITaxRecordParent>(), It.IsAny<AccTaxTransaction[]>(), It.IsAny<AccTaxRecordTransactionLinePivot[]>())).Returns((IReadOnlyTaxRecordData[])null);

			var taxParentMock = new Mock<ITaxRecordParent>();

			var taxRecordData = taxProcessor.GetTaxRecordDataForDataTransfer(taxParentMock.Object);
			AssertEquals("When TaxFrameworkDataTransfer returns null list of tax record data", null, taxRecordData);
		}

		public void TestGetTaxTransactionDataForDataTransfer_ReturnsSameDataObjectList_AsReturnedByTFDataTransfer()
		{
			var expectedTaxRecordData = Array.Empty<IReadOnlyTaxRecordData>();
			var taxFrameworkDataTransferMock = new Mock<ITaxFrameworkDataTransfer>();
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();
			var taxProcessor = CreateTestTaxProcessor(taxRecordLoader: taxRecordLoaderMock.Object, taxFrameworkDataTransfer: taxFrameworkDataTransferMock.Object);

			taxFrameworkDataTransferMock.Setup(x => x.CreateTaxRecordDataFromTaxTransactions(It.IsAny<ITaxRecordParent>(), It.IsAny<AccTaxTransaction[]>(), It.IsAny<AccTaxRecordTransactionLinePivot[]>())).Returns(expectedTaxRecordData);

			var taxRecordParentMock = new Mock<ITaxRecordParent>();
			var taxRecordData1 = new TaxRecordData();
			var taxRecordData2 = new TaxRecordData();
			expectedTaxRecordData.Append(taxRecordData2);

			var taxRecordData = taxProcessor.GetTaxRecordDataForDataTransfer(taxRecordParentMock.Object);
			AssertContainsExactElementsInExactOrder(expectedTaxRecordData, taxRecordData.ToArray());
		}

		[ExpectNoExceptions]
		public void TestGetTaxTransactionDataForDataTransfer_InvokesLoadAllReportableTaxRecords_WithTaxParentParam()
		{
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();
			var taxProcessor = CreateTestTaxProcessor(taxRecordLoader: taxRecordLoaderMock.Object);

			var taxRecordParentMock1 = new Mock<ITaxRecordParent>();
			var taxRecordData = taxProcessor.GetTaxRecordDataForDataTransfer(taxRecordParentMock1.Object);

			taxRecordLoaderMock.Verify(x => x.LoadAllReportableTaxRecords(taxRecordParentMock1.Object));

			var taxRecordParentMock2 = new Mock<ITaxRecordParent>();
			taxRecordData = taxProcessor.GetTaxRecordDataForDataTransfer(taxRecordParentMock2.Object);

			taxRecordLoaderMock.Verify(x => x.LoadAllReportableTaxRecords(taxRecordParentMock2.Object));
		}

		[ExpectNoExceptions]
		public void TestGetTaxTransactionDataForDataTransfer_InvokesLoadTaxRecordPivotsWithValidTaxRecords_WhenTaxTransactionListIsValid()
		{
			var taxRecords = Array.Empty<AccTaxTransaction>();

			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();
			var taxProcessor = CreateTestTaxProcessor(taxRecordLoader: taxRecordLoaderMock.Object);
			var taxRecordParentMock = new Mock<ITaxRecordParent>();

			taxRecordLoaderMock.Setup(x => x.LoadAllReportableTaxRecords(It.IsAny<ITaxRecordParent>())).Returns(taxRecords); // This dependency cannot be avoided.

			var taxRecord1 = Factory.New<AccTaxTransaction>();
			taxRecords.Append(taxRecord1);
			var taxRecordData = taxProcessor.GetTaxRecordDataForDataTransfer(taxRecordParentMock.Object);
			taxRecordLoaderMock.Verify(x => x.LoadTaxRecordPivots(false, taxRecords));

			var taxRecord2 = Factory.New<AccTaxTransaction>();
			taxRecords.Append(taxRecord2);
			taxRecordLoaderMock.Setup(x => x.LoadAllReportableTaxRecords(It.IsAny<ITaxRecordParent>())).Returns(taxRecords);

			taxRecordData = taxProcessor.GetTaxRecordDataForDataTransfer(taxRecordParentMock.Object);
			taxRecordLoaderMock.Verify(x => x.LoadTaxRecordPivots(false, taxRecords));
		}

		[ExpectNoExceptions]
		public void TestGetTaxTransactionDataForDataTransfer_InvokesLoadTaxRecordPivotsWithEmptyTaxRecords_WhenTaxTransactionListIsEmpty()
		{
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();
			var taxProcessor = CreateTestTaxProcessor(taxRecordLoader: taxRecordLoaderMock.Object);

			var taxRecordParentMock = new Mock<ITaxRecordParent>();
			taxRecordLoaderMock.Setup(x => x.LoadAllReportableTaxRecords(It.IsAny<ITaxRecordParent>())).Returns(Array.Empty<AccTaxTransaction>());

			var taxRecordData = taxProcessor.GetTaxRecordDataForDataTransfer(taxRecordParentMock.Object);
			taxRecordLoaderMock.Verify(x => x.LoadTaxRecordPivots(false, Array.Empty<AccTaxTransaction>()));
		}

		public void TestGetTaxTransactionDataForDataTransfer_ThrowsNullException_WhenInputTaxRecordParentIsNull()
		{
			var taxProcessor = CreateTestTaxProcessor();
#if NETFRAMEWORK
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: taxParent", () => taxProcessor.GetTaxRecordDataForDataTransfer(null));
#else
			var ex = AssertExceptionThrown<ArgumentNullException>(() => taxProcessor.GetTaxRecordDataForDataTransfer(null));
			Assert(ex.Message.Equals("Value cannot be null. (Parameter 'taxParent')"));
			Assert(ex.ParamName.Equals("taxParent"));
#endif
		}

		[ExpectNoExceptions]
		public void TestGetTaxTransactionDataForDataTransfer_InvokesCreateTaxRecordsFromTaxTransactions_WithTaxParentParam()
		{
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();
			var taxFrameworkDataTransferMock = new Mock<ITaxFrameworkDataTransfer>();
			var taxProcessor = CreateTestTaxProcessor(taxRecordLoader: taxRecordLoaderMock.Object, taxFrameworkDataTransfer: taxFrameworkDataTransferMock.Object);
			var taxRecordParentMock = new Mock<ITaxRecordParent>();

			taxProcessor.GetTaxRecordDataForDataTransfer(taxRecordParentMock.Object);
			taxFrameworkDataTransferMock.Verify(x => x.CreateTaxRecordDataFromTaxTransactions(taxRecordParentMock.Object, It.IsAny<AccTaxTransaction[]>(), It.IsAny<AccTaxRecordTransactionLinePivot[]>()));
		}

		[ExpectNoExceptions]
		public void TestGetTaxTransactionDataForDataTransfer_InvokesCreateTaxRecordsFromTaxTransactions_WithTaxRecordsAndPivots() // Don't want to have separate tests for tax records and pivots as there cannot be a pivot without a tax transaction.
		{
			var taxRecords = Array.Empty<AccTaxTransaction>();
			var taxRecordPivots = Array.Empty<AccTaxRecordTransactionLinePivot>();

			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();
			var taxFrameworkDataTransferMock = new Mock<ITaxFrameworkDataTransfer>();
			var taxProcessor = CreateTestTaxProcessor(taxRecordLoader: taxRecordLoaderMock.Object, taxFrameworkDataTransfer: taxFrameworkDataTransferMock.Object);
			var taxRecordParentMock = new Mock<ITaxRecordParent>();

			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			taxRecords.Append(taxRecord1);
			taxRecordPivots.Append(pivot1);

			var taxRecord2 = Factory.New<AccTaxTransaction>();
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			taxRecords.Append(taxRecord2);
			taxRecordPivots.Append(pivot2);

			taxRecordLoaderMock.Setup(x => x.LoadAllReportableTaxRecords(It.IsAny<ITaxRecordParent>())).Returns(taxRecords);
			taxRecordLoaderMock.Setup(x => x.LoadTaxRecordPivots(It.IsAny<bool>(), It.IsAny<AccTaxTransaction[]>())).Returns(taxRecordPivots);

			taxProcessor.GetTaxRecordDataForDataTransfer(taxRecordParentMock.Object);
			taxFrameworkDataTransferMock.Verify(x => x.CreateTaxRecordDataFromTaxTransactions(It.IsAny<ITaxRecordParent>(), taxRecords, taxRecordPivots));
		}

		[ExpectNoExceptions]
		public void TestGetTaxTransactionDataForDataTransfer_CreateTaxRecordsFromTaxTransactions_InvokedWithEmptyTaxRecordsAndPivots()
		{
			var taxRecordLoaderMock = new Mock<ITaxRecordLoader>();
			var taxFrameworkDataTransferMock = new Mock<ITaxFrameworkDataTransfer>();
			var taxProcessor = CreateTestTaxProcessor(taxRecordLoader: taxRecordLoaderMock.Object, taxFrameworkDataTransfer: taxFrameworkDataTransferMock.Object);
			var taxRecordParentMock = new Mock<ITaxRecordParent>();

			taxRecordLoaderMock.Setup(x => x.LoadAllReportableTaxRecords(It.IsAny<ITaxRecordParent>())).Returns(Array.Empty<AccTaxTransaction>());
			taxRecordLoaderMock.Setup(x => x.LoadTaxRecordPivots(It.IsAny<bool>(), It.IsAny<AccTaxTransaction[]>())).Returns(Array.Empty<AccTaxRecordTransactionLinePivot>());

			taxProcessor.GetTaxRecordDataForDataTransfer(taxRecordParentMock.Object);
			taxFrameworkDataTransferMock.Verify(x => x.CreateTaxRecordDataFromTaxTransactions(It.IsAny<ITaxRecordParent>(), Array.Empty<AccTaxTransaction>(), Array.Empty<AccTaxRecordTransactionLinePivot>()));
		}

		#endregion

		#region RestoreFromTaxRecordData

		[ExpectNoExceptions]
		public void TestRestoreFromTaxRecordData_InvokesCreateTaxTransactionFromTaxRecordData_WithTaxParentParam()
		{
			var taxFrameworkDataTransferMock = new Mock<ITaxFrameworkDataTransfer>();
			var taxProcessor = CreateTestTaxProcessor(taxFrameworkDataTransfer: taxFrameworkDataTransferMock.Object);
			var taxRecordParentMock = new Mock<ITaxRecordParent>();

			taxProcessor.RestoreFromTaxRecordData(taxRecordParentMock.Object, Array.Empty<IReadOnlyTaxRecordData>());
			taxFrameworkDataTransferMock.Verify(x => x.CreateTaxTransactionFromTaxRecordData(taxRecordParentMock.Object, It.IsAny<IReadOnlyCollection<IReadOnlyTaxRecordData>>()));
		}

		[ExpectNoExceptions]
		public void TestRestoreFromTaxRecordData_InvokesCreateTaxTransactionFromTaxRecordData_WithEmptyDataObjectListParam_WhenInputDataObjectListIsEmpty()
		{
			var taxFrameworkDataTransferMock = new Mock<ITaxFrameworkDataTransfer>();
			var taxProcessor = CreateTestTaxProcessor(taxFrameworkDataTransfer: taxFrameworkDataTransferMock.Object);
			var taxRecordParentMock = new Mock<ITaxRecordParent>();

			taxProcessor.RestoreFromTaxRecordData(taxRecordParentMock.Object, Array.Empty<IReadOnlyTaxRecordData>());
			taxFrameworkDataTransferMock.Verify(x => x.CreateTaxTransactionFromTaxRecordData(It.IsAny<ITaxRecordParent>(), Array.Empty<IReadOnlyTaxRecordData>()));
		}

		[ExpectNoExceptions]
		public void TestRestoreFromTaxRecordData_WhenInputDataObjectListIsNull()
		{
			var taxFrameworkDataTransferMock = new Mock<ITaxFrameworkDataTransfer>();
			var taxProcessor = CreateTestTaxProcessor(taxFrameworkDataTransfer: taxFrameworkDataTransferMock.Object);
			var taxRecordParentMock = new Mock<ITaxRecordParent>();

			taxProcessor.RestoreFromTaxRecordData(taxRecordParentMock.Object, null);
			taxFrameworkDataTransferMock.Verify(x => x.CreateTaxTransactionFromTaxRecordData(taxRecordParentMock.Object, null));
		}

		[ExpectNoExceptions]
		public void TestRestoreFromTaxRecordData_InvokesCreateTaxTransactionFromTaxRecordData_WithSameDataObjectListAsInput()
		{
			var taxFrameworkDataTransferMock = new Mock<ITaxFrameworkDataTransfer>();
			var taxProcessor = CreateTestTaxProcessor(taxFrameworkDataTransfer: taxFrameworkDataTransferMock.Object);
			var taxRecordParentMock = new Mock<ITaxRecordParent>();

			var taxRecordData1 = new TaxRecordData();
			var taxRecordData2 = new TaxRecordData();
			var taxRecordDataArray = new IReadOnlyTaxRecordData[] { taxRecordData1 , taxRecordData2 };

			taxProcessor.RestoreFromTaxRecordData(taxRecordParentMock.Object, taxRecordDataArray);
			taxFrameworkDataTransferMock.Verify(x => x.CreateTaxTransactionFromTaxRecordData(It.IsAny<ITaxRecordParent>(), taxRecordDataArray));
		}

		public void TestRestoreFromTaxRecordData_ReturnsNull_WhenInputTaxRecordParentIsNull()
		{
			var taxRecordData1 = new TaxRecordData();
			var taxRecordDataArray = new IReadOnlyTaxRecordData[] { taxRecordData1 };

			var taxProcessor = CreateTestTaxProcessor();
#if NETFRAMEWORK
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: taxParent", () => taxProcessor.RestoreFromTaxRecordData(null, taxRecordDataArray));
#else
			var ex = AssertExceptionThrown<ArgumentNullException>(() => taxProcessor.RestoreFromTaxRecordData(null, taxRecordDataArray));
			Assert(ex.Message.Equals("Value cannot be null. (Parameter 'taxParent')"));
			Assert(ex.ParamName.Equals("taxParent"));
#endif
		}

		ITaxProcessor CreateTestTaxProcessor(ITaxRecordCreator taxRecordCreator = null, ITaxRecordRealiser taxRecordRealiser = null, ITaxRecordLoader taxRecordLoader = null, ITaxRecordReverser taxRecordReverser = null, ITaxFrameworkDataTransfer taxFrameworkDataTransfer = null)
		{
			return new TaxProcessor(
				taxRecordCreator ?? new TaxRecordCreator(),
				taxRecordRealiser ?? new TaxRecordRealiser(),
				taxRecordLoader ?? new TaxRecordLoader(),
				taxRecordReverser ?? new TaxRecordReverser(),
				taxFrameworkDataTransfer ?? new TaxFrameworkDataTransfer(new TaxRecordPivotProcessor()));
		}

		#endregion

		TaxFrameworkTestObjectCreator TaxFrameworkObjectCreator => taxFrameworkObjectCreator ?? (taxFrameworkObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator taxFrameworkObjectCreator;
		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}

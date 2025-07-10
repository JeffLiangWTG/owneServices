using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	class GLMovementProcessorTest : TestCaseWithFactory
	{
		#region CreateGLMovements

		public void TestCreateGLMovements_Posting()
		{
			var glMovementCreatorMock = new Mock<IGLMovementCreator>(MockBehavior.Strict);

			var glMovementCreatorClass = new GLMovementProcessor();
			glMovementCreatorClass.SubstituteGLMovementCreator_ForTestOnly(glMovementCreatorMock.Object);

			var taxRecord1 = CreateTaxRecord(TaxBasisList.Posting.Code, ZDate.Today, ZDate.Today, 0);
			var taxRecord2 = CreateTaxRecord(TaxBasisList.Posting.Code, ZDate.Today, ZDate.Today, 0);
			var glMovementCreator = (IGLMovementProcessor)glMovementCreatorClass;
			glMovementCreator.CreateGLMovements(taxRecord1, taxRecord2);

			var expectedParameterValues = new List<AccTaxTransaction>();
			glMovementCreatorMock.Setup(x => x.CreateNormalRecord(It.IsAny<AccTaxTransaction>(), GLMovementDateSource.TaxRecordPostDate)).Callback<AccTaxTransaction, GLMovementDateSource>((x, y) => expectedParameterValues.Add(x));
			taxRecord1.ATT_LocalTaxAmount = 10;
			glMovementCreator.CreateGLMovements(taxRecord1, taxRecord2);
			glMovementCreatorMock.Verify(x => x.CreateNormalRecord(It.IsAny<AccTaxTransaction>(), GLMovementDateSource.TaxRecordPostDate), Times.Once);
			AssertArrayEqualsByElements(new[] { taxRecord1 }, expectedParameterValues.ToArray());

			glMovementCreatorMock = new Mock<IGLMovementCreator>(MockBehavior.Strict);
			expectedParameterValues = new List<AccTaxTransaction>();
			glMovementCreatorMock.Setup(x => x.CreateNormalRecord(It.IsAny<AccTaxTransaction>(), GLMovementDateSource.TaxRecordPostDate)).Callback<AccTaxTransaction, GLMovementDateSource>((x, y) => expectedParameterValues.Add(x));
			glMovementCreatorClass.SubstituteGLMovementCreator_ForTestOnly(glMovementCreatorMock.Object);
			taxRecord2.ATT_LocalTaxAmount = 10;
			glMovementCreator.CreateGLMovements(taxRecord1, taxRecord2);
			glMovementCreatorMock.Verify(x => x.CreateNormalRecord(It.IsAny<AccTaxTransaction>(), GLMovementDateSource.TaxRecordPostDate), Times.Exactly(2));
			AssertArrayEqualsByElements(new[] { taxRecord1, taxRecord2 }, expectedParameterValues.ToArray());
		}

		public void TestCreateGLMovements_PostingOnMatching_LocalTaxAmount()
		{
			var glMovementCreatorMock = new Mock<IGLMovementCreator>(MockBehavior.Strict);

			var glMovementCreatorClass = new GLMovementProcessor();
			glMovementCreatorClass.SubstituteGLMovementCreator_ForTestOnly(glMovementCreatorMock.Object);

			var taxRecord1 = CreateTaxRecord(TaxBasisList.PostingOnMatching.Code, ZDate.Today, ZDate.Today, 0);
			var taxRecord2 = CreateTaxRecord(TaxBasisList.PostingOnMatching.Code, ZDate.Today, ZDate.Today, 0);
			var glMovementCreator = (IGLMovementProcessor)glMovementCreatorClass;
			glMovementCreator.CreateGLMovements(taxRecord1, taxRecord2);

			var expectedParameterValues = new List<AccTaxTransaction>();
			glMovementCreatorMock.Setup(x => x.CreateNormalRecord(It.IsAny<AccTaxTransaction>(), GLMovementDateSource.TaxRecordRealisationDate)).Callback<AccTaxTransaction, GLMovementDateSource>((x, y) => expectedParameterValues.Add(x));
			taxRecord2.ATT_LocalTaxAmount = 10;
			glMovementCreator.CreateGLMovements(taxRecord1, taxRecord2);
			glMovementCreatorMock.Verify(x => x.CreateNormalRecord(It.IsAny<AccTaxTransaction>(), GLMovementDateSource.TaxRecordRealisationDate), Times.Once);
			AssertArrayEqualsByElements(new[] { taxRecord2 }, expectedParameterValues.ToArray());

			glMovementCreatorMock = new Mock<IGLMovementCreator>(MockBehavior.Strict);
			expectedParameterValues = new List<AccTaxTransaction>();
			glMovementCreatorMock.Setup(x => x.CreateNormalRecord(It.IsAny<AccTaxTransaction>(), GLMovementDateSource.TaxRecordRealisationDate)).Callback<AccTaxTransaction, GLMovementDateSource>((x, y) => expectedParameterValues.Add(x));
			glMovementCreatorClass.SubstituteGLMovementCreator_ForTestOnly(glMovementCreatorMock.Object);
			taxRecord1.ATT_LocalTaxAmount = 10;
			glMovementCreator.CreateGLMovements(taxRecord1, taxRecord2);
			glMovementCreatorMock.Verify(x => x.CreateNormalRecord(It.IsAny<AccTaxTransaction>(), GLMovementDateSource.TaxRecordRealisationDate), Times.Exactly(2));
			AssertArrayEqualsByElements(new[] { taxRecord1, taxRecord2 }, expectedParameterValues.ToArray());
		}

		public void TestCreateGLMovements_PostingOnMatching_RealisationDate()
		{
			var glMovementCreatorMock = new Mock<IGLMovementCreator>(MockBehavior.Strict);

			var glMovementCreatorClass = new GLMovementProcessor();
			glMovementCreatorClass.SubstituteGLMovementCreator_ForTestOnly(glMovementCreatorMock.Object);

			var taxRecord1 = CreateTaxRecord(TaxBasisList.PostingOnMatching.Code, ZDate.Today, ZDate.Empty, 10);
			var taxRecord2 = CreateTaxRecord(TaxBasisList.PostingOnMatching.Code, ZDate.Today, ZDate.Empty, 10);
			var glMovementCreator = (IGLMovementProcessor)glMovementCreatorClass;
			glMovementCreator.CreateGLMovements(taxRecord1, taxRecord2);

			var expectedParameterValues = new List<AccTaxTransaction>();
			glMovementCreatorMock.Setup(x => x.CreateNormalRecord(It.IsAny<AccTaxTransaction>(), GLMovementDateSource.TaxRecordRealisationDate)).Callback<AccTaxTransaction, GLMovementDateSource>((x, y) => expectedParameterValues.Add(x));
			taxRecord2.ATT_RealisationDate = ZDate.Today;
			glMovementCreator.CreateGLMovements(taxRecord1, taxRecord2);
			glMovementCreatorMock.Verify(x => x.CreateNormalRecord(It.IsAny<AccTaxTransaction>(), GLMovementDateSource.TaxRecordRealisationDate), Times.Once);
			AssertArrayEqualsByElements(new[] { taxRecord2 }, expectedParameterValues.ToArray());

			glMovementCreatorMock = new Mock<IGLMovementCreator>(MockBehavior.Strict);
			expectedParameterValues = new List<AccTaxTransaction>();
			glMovementCreatorMock.Setup(x => x.CreateNormalRecord(It.IsAny<AccTaxTransaction>(), GLMovementDateSource.TaxRecordRealisationDate)).Callback<AccTaxTransaction, GLMovementDateSource>((x, y) => expectedParameterValues.Add(x));
			glMovementCreatorClass.SubstituteGLMovementCreator_ForTestOnly(glMovementCreatorMock.Object);
			taxRecord1.ATT_RealisationDate = ZDate.Today;
			glMovementCreator.CreateGLMovements(taxRecord1, taxRecord2);
			glMovementCreatorMock.Verify(x => x.CreateNormalRecord(It.IsAny<AccTaxTransaction>(), GLMovementDateSource.TaxRecordRealisationDate), Times.Exactly(2));
			AssertArrayEqualsByElements(new[] { taxRecord1, taxRecord2 }, expectedParameterValues.ToArray());
		}

		public void TestCreateGLMovements_Matching()
		{
			var glMovementCreatorMock = new Mock<IGLMovementCreator>(MockBehavior.Strict);

			var glMovementCreatorClass = new GLMovementProcessor();
			glMovementCreatorClass.SubstituteGLMovementCreator_ForTestOnly(glMovementCreatorMock.Object);

			var taxRecord1 = CreateTaxRecord(TaxBasisList.Matching.Code, ZDate.Today, ZDate.Empty, 0);
			var taxRecord2 = CreateTaxRecord(TaxBasisList.Matching.Code, ZDate.Today, ZDate.Empty, 0);
			var glMovementCreator = (IGLMovementProcessor)glMovementCreatorClass;
			glMovementCreator.CreateGLMovements(taxRecord1, taxRecord2);

			var expectedPendingParameterValues = new List<AccTaxTransaction>();
			var expectedRealisedParameterValues = new List<AccTaxTransaction>();
			glMovementCreatorMock.Setup(x => x.CreatePendingRecord(It.IsAny<AccTaxTransaction>())).Callback<AccTaxTransaction>(x => expectedPendingParameterValues.Add(x));
			glMovementCreatorMock.Setup(x => x.CreateRealisedRecord(It.IsAny<AccTaxTransaction>())).Callback<AccTaxTransaction>(x => expectedRealisedParameterValues.Add(x));
			taxRecord1.ATT_LocalTaxAmount = 10;
			taxRecord2.ATT_LocalTaxAmount = 10;
			glMovementCreator.CreateGLMovements(taxRecord1, taxRecord2);
			glMovementCreatorMock.Verify(x => x.CreatePendingRecord(It.IsAny<AccTaxTransaction>()), Times.Exactly(2));
			glMovementCreatorMock.Verify(x => x.CreateRealisedRecord(It.IsAny<AccTaxTransaction>()), Times.Never);
			AssertArrayEqualsByElements(new[] { taxRecord1, taxRecord2 }, expectedPendingParameterValues.ToArray());
			AssertArrayEqualsByElements(System.Array.Empty<AccTaxTransaction>(), expectedRealisedParameterValues.ToArray());

			expectedPendingParameterValues.Clear();
			expectedRealisedParameterValues.Clear();
			glMovementCreatorMock.Invocations.Clear();
			taxRecord1.ATT_RealisationDate = ZDate.Today;
			glMovementCreator.CreateGLMovements(taxRecord1, taxRecord2);
			glMovementCreatorMock.Verify(x => x.CreatePendingRecord(It.IsAny<AccTaxTransaction>()), Times.Exactly(2));
			glMovementCreatorMock.Verify(x => x.CreateRealisedRecord(It.IsAny<AccTaxTransaction>()), Times.Once);
			AssertArrayEqualsByElements(new[] { taxRecord1, taxRecord2 }, expectedPendingParameterValues.ToArray());
			AssertArrayEqualsByElements(new[] { taxRecord1 }, expectedRealisedParameterValues.ToArray());

			expectedPendingParameterValues.Clear();
			expectedRealisedParameterValues.Clear();
			glMovementCreatorMock.Invocations.Clear();
			((INeedRow)taxRecord1).Row.AcceptChanges();
			((INeedRow)taxRecord2).Row.AcceptChanges();
			taxRecord2.ATT_RealisationDate = ZDate.Today;
			glMovementCreator.CreateGLMovements(taxRecord1, taxRecord2);
			glMovementCreatorMock.Verify(x => x.CreatePendingRecord(It.IsAny<AccTaxTransaction>()), Times.Never);
			glMovementCreatorMock.Verify(x => x.CreateRealisedRecord(It.IsAny<AccTaxTransaction>()), Times.Once);
			AssertArrayEqualsByElements(System.Array.Empty<AccTaxTransaction>(), expectedPendingParameterValues.ToArray());
			AssertArrayEqualsByElements(new[] { taxRecord2 }, expectedRealisedParameterValues.ToArray());
		}

		[SuspendCriticalValidation]
		public void TestCreateGLMovementsZeroAmount_Matching_InDB()
		{
			var glMovementCreatorMock = new Mock<IGLMovementCreator>(MockBehavior.Strict);

			var glMovementCreatorClass = new GLMovementProcessor();
			glMovementCreatorClass.SubstituteGLMovementCreator_ForTestOnly(glMovementCreatorMock.Object);
			AccTaxTransaction expectedParameterValue = null;
			glMovementCreatorMock.Setup(x => x.CreatePendingRecord(It.IsAny<AccTaxTransaction>())).Callback<AccTaxTransaction>(x => expectedParameterValue = x);

			var taxRecord = CreateTaxRecord(TaxBasisList.Matching.Code, ZDate.Today, ZDate.Empty, 100);
			var glMovementCreator = (IGLMovementProcessor)glMovementCreatorClass;
			glMovementCreator.CreateGLMovements(taxRecord);
			glMovementCreatorMock.Verify(x => x.CreatePendingRecord(It.IsAny<AccTaxTransaction>()), Times.Once);
			AssertEquals(taxRecord, expectedParameterValue);

			glMovementCreatorMock = new Mock<IGLMovementCreator>(MockBehavior.Strict);
			glMovementCreatorClass.SubstituteGLMovementCreator_ForTestOnly(glMovementCreatorMock.Object);
			expectedParameterValue = null;
			glMovementCreatorMock.Setup(x => x.CreateRealisedRecord(It.IsAny<AccTaxTransaction>())).Callback<AccTaxTransaction>(x => expectedParameterValue = x);
			((INeedRow)taxRecord).Row.AcceptChanges();
			taxRecord.ATT_RealisationDate = ZDate.Today;
			glMovementCreator.CreateGLMovements(taxRecord);
			glMovementCreatorMock.Verify(x => x.CreateRealisedRecord(It.IsAny<AccTaxTransaction>()), Times.Once);
			AssertEquals(taxRecord, expectedParameterValue);
		}

		#endregion

		public void TestDeleteGLMovementsNotInDB()
		{
			var taxRecord1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			var taxRecord2 = Factory.NewWithValidTestData<AccTaxTransaction>();

			var glMovement22 = Factory.New<AccTaxGLMovement>();
			glMovement22.ATM_ATT_TaxTransaction = taxRecord2.PK;
			glMovement22.FillWithValidTestData();

			var tfDependencyFactory = new Mock<ITaxFrameworkDependencyFactory>();
			var emptyValidatorMock = new Mock<IAccTaxTransactionCriticalValidator>();
			tfDependencyFactory.Setup(x => x.GetAccTaxTransactionCriticalValidator(taxRecord1)).Returns(emptyValidatorMock.Object);
			tfDependencyFactory.Setup(x => x.GetAccTaxTransactionCriticalValidator(taxRecord2)).Returns(emptyValidatorMock.Object);
			ObjectFactory.Substitute(tfDependencyFactory.Object);

			Factory.Save();

			var glMovement11 = Factory.New<AccTaxGLMovement>();
			glMovement11.ATM_ATT_TaxTransaction = taxRecord1.PK;
			var glMovement12 = Factory.New<AccTaxGLMovement>();
			glMovement12.ATM_ATT_TaxTransaction = taxRecord1.PK;
			var glMovement2 = Factory.New<AccTaxGLMovement>();
			glMovement2.ATM_ATT_TaxTransaction = taxRecord2.PK;

			var glMovementCreator = (IGLMovementProcessor)new GLMovementProcessor();
			glMovementCreator.DeleteGLMovementsNotInDB(new[] { taxRecord1, taxRecord2 });

			Assert(nameof(glMovement11), glMovement11.IsDeleted);
			Assert(nameof(glMovement12), glMovement12.IsDeleted);
			Assert(nameof(glMovement2), glMovement2.IsDeleted);
			Assert(nameof(glMovement22), !glMovement22.IsDeleted);
		}

		#region Setup and helpers

		AccTaxTransaction CreateTaxRecord(string basis, ZDate postDate, ZDate realisationDate, ZDecimal amount)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;
			taxRecord.TaxConfiguration.ETC_Code = "TTT";
			taxRecord.ATT_Basis = basis;
			taxRecord.ATT_PostDate = postDate;
			taxRecord.ATT_RealisationDate = realisationDate;
			taxRecord.ATT_LocalTaxAmount = amount;

			return taxRecord;
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-6));
		}

		#endregion
	}
}

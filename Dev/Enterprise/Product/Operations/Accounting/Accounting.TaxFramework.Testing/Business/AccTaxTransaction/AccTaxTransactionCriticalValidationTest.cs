using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	public class AccTaxTransactionCriticalValidationTest : CriticalValidationTest<AccTaxTransaction>
	{
		public void TestConsistencyWithPivotsData_NoPivots()
		{
			var pivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, TaxTransaction.PK) { FetchOnlyFromLocalCache = true });
			if (!pivots.IsNullOrEmpty())
			{
				pivots.ForEach(p => p.Delete());
			}

			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("TaxTransactionWithoutPivots", true, CriticalValidationErrorType.TaxTransactionWithoutPivots, CriticalValidationMessageTemplate.TaxTransactionWithoutPivots, "Type = AccTaxTransaction", "Properties:"));
		}

		public void TestConsistencyWithPivotsData_LocalTaxAmount()
		{
			var pivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, TaxTransaction.PK) { FetchOnlyFromLocalCache = true });
			if (!pivots.IsNullOrEmpty())
			{
				pivots.ForEach(p => p.Delete());
			}

			var pivot1 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = TaxTransaction.PK;
			var pivot2 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = TaxTransaction.PK;

			TaxTransaction.ATT_LocalTaxAmount = 2m;
			pivot1.ATP_LocalTaxAmount = 0.5m;
			pivot2.ATP_LocalTaxAmount = 0.8m;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("TaxTransactionWithSumOfPivotsLocalTaxAmountsMismatch", true, CriticalValidationErrorType.TaxTransactionWithSumOfPivotsLocalTaxAmountsMismatch, CriticalValidationMessageTemplate.TaxTransactionWithSumOfPivotsLocalTaxAmountsMismatch, "Type = AccTaxTransaction", "Properties:", "Type = AccTaxRecordTransactionLinePivot", "Properties:", "Type = AccTaxRecordTransactionLinePivot", "Properties:"));

			pivot2.ATP_LocalTaxAmount = 1.5m;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}

			TaxTransaction.ATT_LocalTaxAmount = 4m;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("TaxTransactionWithLocalTaxAmountChangedAfterSaving", true, CriticalValidationErrorType.TaxTransactionWithLocalTaxAmountChangedAfterSaving, CriticalValidationMessageTemplate.TaxTransactionWithLocalTaxAmountChangedAfterSaving, "Type = AccTaxTransaction", "Properties:"));

			TaxTransaction.ATT_LocalTaxAmount = (TaxTransaction.ATT_LocalTaxAmountInfo.OriginalValue as ZDecimal?).Value;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			var newFactory = new BusinessObjectFactory();
			var taxTaransactionInNewFactory = newFactory.Load<AccTaxTransaction>(TaxTransaction.PK);
			taxTaransactionInNewFactory.ATT_LocalTaxAmount = 4m;
			AssertOnSavingCheck(taxTaransactionInNewFactory, new TestCaseDefinition_ForSeparateTestsMethods("TaxTransactionWithLocalTaxAmountChangedAfterSaving", true, CriticalValidationErrorType.TaxTransactionWithLocalTaxAmountChangedAfterSaving, CriticalValidationMessageTemplate.TaxTransactionWithLocalTaxAmountChangedAfterSaving, "Type = AccTaxTransaction", "Properties:"));

			taxTaransactionInNewFactory.ATT_LocalTaxAmount = (taxTaransactionInNewFactory.ATT_LocalTaxAmountInfo.OriginalValue as ZDecimal?).Value;
			AssertOnSavingCheck(taxTaransactionInNewFactory, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));
		}

		public void TestConsistencyWithPivotsData_TaxExpense()
		{
			var pivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, TaxTransaction.PK) { FetchOnlyFromLocalCache = true });
			if (!pivots.IsNullOrEmpty())
			{
				pivots.ForEach(p => p.Delete());
			}

			var pivot1 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = TaxTransaction.PK;
			var pivot2 = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = TaxTransaction.PK;

			TaxTransaction.ATT_LocalTaxAmount = 2m;
			pivot1.ATP_LocalTaxAmount = 0.5m;
			pivot2.ATP_LocalTaxAmount = 1.5m;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			pivot1.ATP_IsTaxExpense = true;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch", true, CriticalValidationErrorType.TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch, CriticalValidationMessageTemplate.TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch, "Type = AccTaxTransaction", "Properties:", "Type = AccTaxRecordTransactionLinePivot", "Properties:", "Type = AccTaxRecordTransactionLinePivot", "Properties:"));

			pivot1.ATP_IsTaxExpense = false;
			pivot2.ATP_IsTaxExpense = true;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch", true, CriticalValidationErrorType.TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch, CriticalValidationMessageTemplate.TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch, "Type = AccTaxTransaction", "Properties:", "Type = AccTaxRecordTransactionLinePivot", "Properties:", "Type = AccTaxRecordTransactionLinePivot", "Properties:"));

			var taxExpenseAccount = Factory.NewWithValidTestData<AccGLHeader>();
			taxExpenseAccount.AG_AccountNum = "TaxExpense";
			TaxTransaction.ATT_AG_TaxExpenseAccount = taxExpenseAccount.PK;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch", true, CriticalValidationErrorType.TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch, CriticalValidationMessageTemplate.TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch, "Type = AccTaxTransaction", "Properties:", "Type = AccTaxRecordTransactionLinePivot", "Properties:", "Type = AccTaxRecordTransactionLinePivot", "Properties:"));

			pivot1.ATP_IsTaxExpense = true;
			pivot2.ATP_IsTaxExpense = false;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch", true, CriticalValidationErrorType.TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch, CriticalValidationMessageTemplate.TaxTransactionWithSomePivotHavingTaxExpenseDataMismatch, "Type = AccTaxTransaction", "Properties:", "Type = AccTaxRecordTransactionLinePivot", "Properties:", "Type = AccTaxRecordTransactionLinePivot", "Properties:"));

			pivot2.ATP_IsTaxExpense = true;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}

			TaxTransaction.ATT_AG_TaxExpenseAccount = ZGuid.NewZGuid();
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("TaxTransactionWithTaxExpenseDataChangedAfterSaving", true, CriticalValidationErrorType.TaxTransactionWithTaxExpenseDataChangedAfterSaving, CriticalValidationMessageTemplate.TaxTransactionWithTaxExpenseDataChangedAfterSaving, "Type = AccTaxTransaction", "Properties:"));

			TaxTransaction.ATT_AG_TaxExpenseAccount = (TaxTransaction.ATT_AG_TaxExpenseAccountInfo.OriginalValue as ZGuid?).Value;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			var newFactory = new BusinessObjectFactory();
			var taxTaransactionInNewFactory = newFactory.Load<AccTaxTransaction>(TaxTransaction.PK);
			taxTaransactionInNewFactory.ATT_AG_TaxExpenseAccount = ZGuid.NewZGuid();
			AssertOnSavingCheck(taxTaransactionInNewFactory, new TestCaseDefinition_ForSeparateTestsMethods("TaxTransactionWithTaxExpenseDataChangedAfterSaving", true, CriticalValidationErrorType.TaxTransactionWithTaxExpenseDataChangedAfterSaving, CriticalValidationMessageTemplate.TaxTransactionWithTaxExpenseDataChangedAfterSaving, "Type = AccTaxTransaction", "Properties:"));

			taxTaransactionInNewFactory.ATT_AG_TaxExpenseAccount = (taxTaransactionInNewFactory.ATT_AG_TaxExpenseAccountInfo.OriginalValue as ZGuid?).Value;
			AssertOnSavingCheck(taxTaransactionInNewFactory, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));
		}

		public void TestHasSkippedDataRefreshBusUpdate()
		{
			var dataRefreshBusUpdateDeciderMock = new Mock<IDataRefreshBusUpdateActionDecider>();
			AccTaxTransaction passedObject = null;
			dataRefreshBusUpdateDeciderMock
				.Setup(x => x.HasSkippedDataRefreshBusUpdate(It.IsAny<AccTaxTransaction>()))
				.Returns(true)
				.Callback<BusinessObject>(x => passedObject = (AccTaxTransaction)x);
			ObjectFactory.Substitute(dataRefreshBusUpdateDeciderMock.Object);

			AssertAfterSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("", true, CriticalValidationErrorType.TaxRecordSkippedDataRefreshBusUpdateButWasSavedSuccessfully, CriticalValidationMessageTemplate.TaxRecordSkippedDataRefreshBusUpdateButWasSavedSuccessfully, "Properties:"));
			dataRefreshBusUpdateDeciderMock.Verify(x => x.HasSkippedDataRefreshBusUpdate(It.IsAny<AccTaxTransaction>()));
			AssertEquals("Correct parameter passed into HasSkippedDataRefreshBusUpdate method.", TaxTransaction, passedObject);

			dataRefreshBusUpdateDeciderMock
				.Setup(x => x.HasSkippedDataRefreshBusUpdate(It.IsAny<AccTaxTransaction>()))
				.Returns(false);
			AssertAfterSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods(""));
		}

		public void TestLinkedGLMovements_PTMBasis()
		{
			TaxTransaction.ATT_Basis = TaxBasisList.PostingOnMatching.Code;

			GLMovement.ATM_Type = "";
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("InvalidGLMovementForNotionalTax", true, CriticalValidationErrorType.InvalidGLMovementForNotionalTax, "GL Movement record must not be created for notional tax record.", "Properties:"));

			GLMovement.ATM_Type = TaxGLMovementTypeList.Normal.Code;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("InvalidGLMovementForNotionalTax", true, CriticalValidationErrorType.InvalidGLMovementForNotionalTax, "GL Movement record must not be created for notional tax record.", "Properties:"));

			GLMovement.ATM_ATT_TaxTransaction = ZGuid.Empty;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			TaxTransaction.ATT_RealisationDate = ZDate.Today;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("GLMovementsRecordsNotFound", true, CriticalValidationErrorType.GLMovementsRecordsNotFound, "Could not find GL Movements record of type NRM when tax transaction basis is PTM", "Properties:"));

			GLMovement.ATM_ATT_TaxTransaction = TaxTransaction.PK;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			GLMovement.ATM_Type = TaxGLMovementTypeList.Realised.Code;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("GLMovementsRecordsNotFound", true, CriticalValidationErrorType.GLMovementsRecordsNotFound, "Could not find GL Movements record of type NRM when tax transaction basis is PTM", "Properties:"));

			SetUp();
			TaxTransaction.ATT_Basis = TaxBasisList.PostingOnMatching.Code;
			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}

			GLMovement = TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(TaxTransaction.PK);
			GLMovement.ATM_Type = "";
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			GLMovement.ATM_Type = TaxGLMovementTypeList.Normal.Code;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			GLMovement.ATM_ATT_TaxTransaction = ZGuid.Empty;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			TaxTransaction.ATT_RealisationDate = ZDate.Today;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("GLMovementsRecordsNotFound", true, CriticalValidationErrorType.GLMovementsRecordsNotFound, "Could not find GL Movements record of type NRM when tax transaction basis is PTM", "Properties:"));

			GLMovement.ATM_ATT_TaxTransaction = TaxTransaction.PK;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			GLMovement.ATM_Type = TaxGLMovementTypeList.Realised.Code;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("GLMovementsRecordsNotFound", true, CriticalValidationErrorType.GLMovementsRecordsNotFound, "Could not find GL Movements record of type NRM when tax transaction basis is PTM", "Properties:"));

			GLMovement.ATM_Type = TaxGLMovementTypeList.Normal.Code;
			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}

			GLMovement.ATM_ATT_TaxTransaction = ZGuid.Empty;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			GLMovement.ATM_ATT_TaxTransaction = TaxTransaction.PK;
			GLMovement.ATM_Type = TaxGLMovementTypeList.Realised.Code;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));
		}

		public void TestIsCancelledIfAllTaxAmountsZero()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_AH = invoice.PK;
			var pivot = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransaction.PK) { FetchOnlyFromLocalCache = true })[0];
			taxTransaction.ATT_OSTaxBaseAmount = 0m;
			taxTransaction.ATT_LocalTaxBaseAmount = 0m;
			taxTransaction.ATT_OSTaxAmount = 0m;
			taxTransaction.ATT_LocalTaxAmount = 0m;
			taxTransaction.ATT_IsCancelled = false;
			AssertOnSavingCheck(taxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("TaxTransactionShouldBeCancelledIfAllAmountsAreZero", true, CriticalValidationErrorType.TaxTransactionShouldBeCancelledIfAllAmountsAreZero, CriticalValidationMessageTemplate.TaxTransactionShouldBeCancelledIfAllAmountsAreZeroErrorMessage, "Properties:"));

			taxTransaction.ATT_IsCancelled = true;
			taxTransaction.ATT_RealisationDate = ZDate.Today;
			AssertOnSavingCheck(taxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			taxTransaction.ATT_IsCancelled = false;
			taxTransaction.ATT_OSTaxBaseAmount = 10m;
			AssertOnSavingCheck(taxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			taxTransaction.ATT_OSTaxBaseAmount = 0m;
			taxTransaction.ATT_LocalTaxBaseAmount = 10m;
			AssertOnSavingCheck(taxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			taxTransaction.ATT_LocalTaxBaseAmount = 0m;
			taxTransaction.ATT_OSTaxAmount = 2m;
			AssertEquals("Precondition: ATT_LocalTaxAmount is set when ATT_OSTaxAmount is set", 2m, taxTransaction.ATT_LocalTaxAmount);
			pivot.ATP_LocalTaxAmount = 2m;

			var glMovement = Factory.NewWithValidTestData<AccTaxGLMovement>();
			glMovement.ATM_ATT_TaxTransaction = taxTransaction.PK;
			glMovement.ATM_Type = TaxGLMovementTypeList.Normal.Code;
			AssertOnSavingCheck(taxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			taxTransaction.ATT_OSTaxAmount = 0m;
			taxTransaction.ATT_LocalTaxAmount = 4m;
			pivot.ATP_LocalTaxAmount = 4m;
			AssertOnSavingCheck(taxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));
		}

		public void TestLinkedGLMovementsWhenLocalTaxAmountZero()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_AH = invoice.PK;
			taxTransaction.ATT_LocalTaxAmount = 0m;
			AssertOnSavingCheck(taxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			var glMovement = Factory.NewWithValidTestData<AccTaxGLMovement>();
			glMovement.ATM_ATT_TaxTransaction = taxTransaction.PK;
			AssertOnSavingCheck(taxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("GLMovementExistsForLocalTaxAmountZero", true, CriticalValidationErrorType.GLMovementExistsForLocalTaxAmountZero, "GL Movement record must not be created when Local Tax Amount is zero.", "Properties:"));
		}

		public void TestLinkedGLMovements_MATBasis_MissingPNDType()
		{
			TaxTransaction.ATT_Basis = TaxBasisList.Matching.Code;

			GLMovement.ATM_Type = TaxGLMovementTypeList.Normal.Code;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("GLMovementsRecordsNotFound", true, CriticalValidationErrorType.GLMovementsRecordsNotFound, "Could not find GL Movements record of type PND when tax transaction basis is MAT", "Properties:"));

			GLMovement.ATM_Type = TaxGLMovementTypeList.Pending.Code;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}

			Assert(TaxTransaction.IsInDatabase);
			GLMovement.ATM_Type = TaxGLMovementTypeList.Normal.Code;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));
		}

		public void TestLinkedGLMovements_MATBasis_MissingRLSType()
		{
			TaxTransaction.ATT_Basis = TaxBasisList.Matching.Code;
			GLMovement.ATM_Type = TaxGLMovementTypeList.Pending.Code;

			TaxTransaction.ATT_RealisationDate = ZDate.Empty;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			TaxTransaction.ATT_RealisationDate = new ZDate(2020, 3, 3);
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("GLMovementsRecordsNotFound", true, CriticalValidationErrorType.GLMovementsRecordsNotFound, "Could not find GL Movements record of type RLS when tax transaction basis is MAT", "Properties:"));

			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}

			Assert(TaxTransaction.IsInDatabase);
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			SetUp();
			Assert(!TaxTransaction.IsInDatabase);
			TaxTransaction.ATT_Basis = TaxBasisList.Matching.Code;
			GLMovement.ATM_Type = TaxGLMovementTypeList.Pending.Code;

			TaxTransaction.ATT_RealisationDate = ZDate.Empty;
			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}

			Assert(TaxTransaction.IsInDatabase);
			TaxTransaction.ATT_RealisationDate = new ZDate(2020, 3, 5);
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("GLMovementsRecordsNotFound", true, CriticalValidationErrorType.GLMovementsRecordsNotFound, "Could not find GL Movements record of type RLS when tax transaction basis is MAT", "Properties:"));
		}

		public void TestLinkedGLMovements_PSTBasis_MissingNRMType()
		{
			TaxTransaction.ATT_Basis = TaxBasisList.Posting.Code;

			GLMovement.ATM_Type = TaxGLMovementTypeList.Pending.Code;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("GLMovementsRecordsNotFound", true, CriticalValidationErrorType.GLMovementsRecordsNotFound, "Could not find GL Movements record of type NRM when tax transaction basis is PST", "Properties:"));

			GLMovement.ATM_Type = TaxGLMovementTypeList.Normal.Code;
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			GLMovement.ATM_Type = TaxGLMovementTypeList.Pending.Code;
			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));
		}

		public void TestInvalidTaxTransactionBasis()
		{
			TaxTransaction.ATT_Basis = "INV";
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("InvalidTaxTransactionBasis", true, CriticalValidationErrorType.InvalidTaxTransactionBasis, "Invalid Tax Transaction basis INV", "Properties:"));
		}

		public void TestCriticalValidationCancelledNonSPRTransactionRealisationDate()
		{
			var taxFrameworkDependencyFactory = new Mock<ITaxFrameworkDependencyFactory>();
			ObjectFactory.Substitute(taxFrameworkDependencyFactory.Object);

			var accTaxTransactionCriticalValidator = new Mock<IAccTaxTransactionCriticalValidator>();
			taxFrameworkDependencyFactory.Setup(x => x.GetAccTaxTransactionCriticalValidator(TaxTransaction)).Returns(accTaxTransactionCriticalValidator.Object);

			accTaxTransactionCriticalValidator.Setup(x => x.CheckCancelledNonSPRTransactionRealisationDate()).Returns((CriticalValidationResult)null);
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			accTaxTransactionCriticalValidator.Setup(x => x.CheckCancelledNonSPRTransactionRealisationDate()).Returns(new CriticalValidationResult(CriticalValidationErrorType.CancelledTaxTransactionRealisationDateShouldNotBeEmpty,
																																				   CriticalValidationMessageTemplate.CancelledTaxTransactionRealisationDateShouldNotBeEmpty,
																																				   "additional info"));
			AssertOnSavingCheck(TaxTransaction, new TestCaseDefinition_ForSeparateTestsMethods("CancelledTaxTransactionRealisationDateShouldNotBeEmpty", true, CriticalValidationErrorType.CancelledTaxTransactionRealisationDateShouldNotBeEmpty, "Canceled transaction has empty realization date.", "additional info"));
		}

		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			Assert(true);

			return new List<TestCaseDefinitionWithDelegate_Obsolete>();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var linkedTxn = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			linkedTxn.AH_OutstandingAmount = 10;
			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, TaxConfigurationLedgers.AccountsPayable.Code);
			var taxID = TestObjectCreator.CreateTaxRate("TID", "TID Desc", 6);
			TaxTransaction = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxConfiguration = taxConfig, TransactionHeader = linkedTxn, PostDate = new ZDate(2020, 3, 5), TaxId = taxID, TaxRate = (9, 8), OsTaxAmount = 4m, LocalTaxAmount = 2m, OsTaxBaseAmount = 40m, LocalTaxBaseAmount = 20m, TaxBasis = TaxBasisList.Matching.Code, CurrencyCode = TestObjectCreator.USD.Code, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot = Factory.NewWithValidTestData<AccTaxRecordTransactionLinePivot>();
			pivot.ATP_ATT = TaxTransaction.PK;
			pivot.ATP_LocalTaxAmount = 2m;

			GLMovement = TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(TaxTransaction.PK);
			GLMovement.ATM_Type = TaxGLMovementTypeList.Pending.Code;
			GLMovement.TaxTransaction.SubstituteGLMovementProcessor_ForTestOnly(new Mock<IGLMovementProcessor>().Object);
		}

		AccTaxTransaction TaxTransaction;
		AccTaxGLMovement GLMovement;

		TestObjectCreator TestObjectCreator
		{
			get
			{
				return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
			}
		}

		TestObjectCreator testObjectCreator;

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator
		{
			get
			{
				return taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
			}
		}

		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
	}

	[TestedType(typeof(AccTaxTransaction))]
	class AccTaxTransactionSupportCriticalValidationTest : SupportCriticalValidationTestBase
	{
		protected override IConflictWithCriticalFields GetNewBusinessObject()
		{
			return Factory.New<AccTaxTransaction>();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	class ThresholdAmountProcessorTest : TestCaseWithFactory
	{
		public void TestIsUsedAsDependecy()
		{
			AssertType<ThresholdAmountProcessor>(new TaxRecordCreator().ThresholdAmountProcessor_ExposedForTestOnly);
		}

		public void TestGetTaxRecordsOutsideOfThreshold_AR()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.SetupGet(x => x.Org).Returns(TestObjectCreator.TestOrganisation);
			taxParentMock.SetupGet(x => x.Ledger).Returns(LedgerTypes.AccountsReceivable);

			var taxConfigs = SetupTaxConfigs();

			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = -3,EffectiveRate = -1 });
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = -7,EffectiveRate = -1 });
			var taxRecord3 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[1], LocalTaxAmount = -10,EffectiveRate = -1 });
			var taxRecord4 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[2], LocalTaxAmount = -10,EffectiveRate = -1 });
			var taxRecord5 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[3], LocalTaxAmount = -9.99,EffectiveRate = -1 });
			var taxRecord6 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[4], LocalTaxAmount = 10,EffectiveRate = -1 });

			var taxRecords = new List<AccTaxTransaction> { taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5, taxRecord6 };

			IThresholdProcessor processor = new ThresholdAmountProcessor();
			var result = processor.GetTaxRecordsOutsideOfThreshold(taxParentMock.Object, taxRecords, new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code });
			AssertEquals(2, result.Count);

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecord5, taxRecord6 };
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, result);
		}

		public void TestGetTaxRecordsOutsideOfThreshold_AP()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.SetupGet(x => x.Org).Returns(TestObjectCreator.TestOrganisation);
			taxParentMock.SetupGet(x => x.Ledger).Returns(LedgerTypes.AccountsPayable);

			var taxConfigs = SetupTaxConfigs();

			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = 3, EffectiveRate = -1 });
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = 7, EffectiveRate = -1 });
			var taxRecord3 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[1], LocalTaxAmount = 10, EffectiveRate = -1 });
			var taxRecord4 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[2], LocalTaxAmount = 10, EffectiveRate = -1 });
			var taxRecord5 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[3], LocalTaxAmount = 9.99, EffectiveRate = -1 });
			var taxRecord6 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[4], LocalTaxAmount = -10, EffectiveRate = -1 });

			var taxRecords = new List<AccTaxTransaction> { taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5, taxRecord6 };

			IThresholdProcessor processor = new ThresholdAmountProcessor();
			var result = processor.GetTaxRecordsOutsideOfThreshold(taxParentMock.Object, taxRecords, new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code });
			AssertEquals(2, result.Count);

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecord5, taxRecord6 };
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, result);
		}

		public void TestProcess_AR()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.SetupGet(x => x.Org).Returns(TestObjectCreator.TestOrganisation);
			taxParentMock.SetupGet(x => x.Ledger).Returns(LedgerTypes.AccountsReceivable);

			var taxConfigs = SetupTaxConfigs();

			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = -3, EffectiveRate = -1 });
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = -7, EffectiveRate = -1 });
			var taxRecord3 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[1], LocalTaxAmount = -10, EffectiveRate = -1 });
			var taxRecord4 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[2], LocalTaxAmount = -10, EffectiveRate = -1 });
			var taxRecord5 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[3], LocalTaxAmount = -9.99, EffectiveRate = -1 });
			var taxRecord6 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[4], LocalTaxAmount = 10, EffectiveRate = -1 });

			AccTaxTransaction[] passedTaxRecordToDelete = null;
			var pivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();
			pivotProcessorMock.Setup(x => x.DeleteTaxRecordsWithPivots(It.IsAny<AccTaxTransaction[]>())).Callback((AccTaxTransaction[] recordsToDelete) => passedTaxRecordToDelete = recordsToDelete);
			var taxRecords = new List<AccTaxTransaction> { taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5, taxRecord6 };

			IThresholdProcessor processor = new ThresholdAmountProcessor();
			processor.Process(pivotProcessorMock.Object, taxParentMock.Object, taxRecords, new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code });
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord5, taxRecord6 }, passedTaxRecordToDelete);
		}

		public void TestProcess_AP()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.SetupGet(x => x.Org).Returns(TestObjectCreator.TestOrganisation);
			taxParentMock.SetupGet(x => x.Ledger).Returns(LedgerTypes.AccountsPayable);

			var taxConfigs = SetupTaxConfigs();

			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = 3, EffectiveRate = -1 });
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = 7, EffectiveRate = -1 });
			var taxRecord3 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[1], LocalTaxAmount = 10, EffectiveRate = -1 });
			var taxRecord4 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[2], LocalTaxAmount = 10, EffectiveRate = -1 });
			var taxRecord5 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[3], LocalTaxAmount = 9.99, EffectiveRate = -1 });
			var taxRecord6 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[4], LocalTaxAmount = -10, EffectiveRate = -1 });

			AccTaxTransaction[] passedTaxRecordToDelete = null;
			var pivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();
			pivotProcessorMock.Setup(x => x.DeleteTaxRecordsWithPivots(It.IsAny<AccTaxTransaction[]>())).Callback((AccTaxTransaction[] recordsToDelete) => passedTaxRecordToDelete = recordsToDelete);
			var taxRecords = new List<AccTaxTransaction> { taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5, taxRecord6 };

			IThresholdProcessor processor = new ThresholdAmountProcessor();
			processor.Process(pivotProcessorMock.Object, taxParentMock.Object, taxRecords, new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code });
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord5, taxRecord6 }, passedTaxRecordToDelete);
		}

		public void TestThresholdGRP_GetTaxRecordsOutsideOfThreshold_AR()
		{
			var taxConfigs = SetupTaxConfigs();

			var taxTransactions = new AccTaxTransaction[]
			{
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[5], LocalTaxAmount = -7, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[6], LocalTaxAmount = -2.99, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[7], LocalTaxAmount = -5, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[8], LocalTaxAmount = -5, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[9], LocalTaxAmount = 6, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[9], LocalTaxAmount = 4, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[10], LocalTaxAmount = -10, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[11], LocalTaxAmount = -10, EffectiveRate = -1  })
			};
			AssertThresholdGRP_GetTaxRecordsOutsideOfThreshold(taxTransactions.ToList(), LedgerTypes.AccountsReceivable);
		}

		public void TestThresholdGRP_GetTaxRecordsOutsideOfThreshold_AP()
		{
			var taxConfigs = SetupTaxConfigs();

			var taxTransactions = new AccTaxTransaction[]
			{
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[5], LocalTaxAmount = 7, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[6], LocalTaxAmount = 2.99, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[7], LocalTaxAmount = 5, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[8], LocalTaxAmount = 5, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[9], LocalTaxAmount = -6, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[9], LocalTaxAmount = -4, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[10], LocalTaxAmount = 10, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[11], LocalTaxAmount = 10, EffectiveRate = -1  })
			};
			AssertThresholdGRP_GetTaxRecordsOutsideOfThreshold(taxTransactions.ToList(), LedgerTypes.AccountsPayable);
		}

		void AssertThresholdGRP_GetTaxRecordsOutsideOfThreshold(List<AccTaxTransaction> taxRecords, string ledgerType)
		{
			var taxParentMock = CreateITaxRecordParentMock(ledgerType);
			var thresholdMethodCodes = new HashSet<string>()  { ETC_ThresholdMethods.TransactionLevel.Code, ETC_ThresholdMethods.TransactionLevelGroup.Code };

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[0], taxRecords[1], taxRecords[4], taxRecords[5] };

			IThresholdProcessor processor = new ThresholdAmountProcessor();
			var result = processor.GetTaxRecordsOutsideOfThreshold(taxParentMock.Object, taxRecords, thresholdMethodCodes);
			AssertEquals(4, result.Count);
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, result);

			AccTaxTransaction[] passedTaxRecordToDelete = null;
			var pivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();
			pivotProcessorMock.Setup(x => x.DeleteTaxRecordsWithPivots(It.IsAny<AccTaxTransaction[]>())).Callback((AccTaxTransaction[] recordsToDelete) => passedTaxRecordToDelete = recordsToDelete);

			processor.Process(pivotProcessorMock.Object, taxParentMock.Object, taxRecords, thresholdMethodCodes);
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, passedTaxRecordToDelete);
		}

		public void TestAllThresholdMethods_GetTaxRecordsOutsideOfThreshold_AR()
		{
			var taxConfigs = SetupTaxConfigs();

			var taxTransactions = new AccTaxTransaction[]
			{
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = -3, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = -7, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[1], LocalTaxAmount = -10, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[5], LocalTaxAmount = -7, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[6], LocalTaxAmount = -2.99, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[7], LocalTaxAmount = -10, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[9], LocalTaxAmount = 6, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[9], LocalTaxAmount = 4, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[10], LocalTaxAmount = -10, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[11], LocalTaxAmount = -10, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[12], LocalTaxBaseAmount = 90 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[13], LocalTaxBaseAmount = 100 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[14], LocalTaxBaseAmount = 150 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[15], LocalTaxBaseAmount = 200 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[16], LocalTaxBaseAmount = -300 }),
			};
			AssertAllThresholdMethods_GetTaxRecordsOutsideOfThreshold(taxTransactions.ToList(), LedgerTypes.AccountsReceivable);
		}

		public void TestAllThresholdMethods_GetTaxRecordsOutsideOfThreshold_AP()
		{
			var taxConfigs = SetupTaxConfigs();

			var taxTransactions = new AccTaxTransaction[]
			{
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = 3, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = 7, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[1], LocalTaxAmount = 10, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[5], LocalTaxAmount = 7, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[6], LocalTaxAmount = 2.99, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[7], LocalTaxAmount = 10, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[9], LocalTaxAmount = -6, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[9], LocalTaxAmount = -4, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[10], LocalTaxAmount = 10, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[11], LocalTaxAmount = 10, EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[12], LocalTaxBaseAmount = -90 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[13], LocalTaxBaseAmount = -100 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[14], LocalTaxBaseAmount = -150 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[15], LocalTaxBaseAmount = -200 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[16], LocalTaxBaseAmount = 300 }),
			};

			AssertAllThresholdMethods_GetTaxRecordsOutsideOfThreshold(taxTransactions.ToList(), LedgerTypes.AccountsPayable);
		}

		void AssertAllThresholdMethods_GetTaxRecordsOutsideOfThreshold(List<AccTaxTransaction> taxRecords, string ledgerType)
		{
			var taxParentMock = CreateITaxRecordParentMock(ledgerType);

			var thresholdMethodCodes = new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code, ETC_ThresholdMethods.TransactionLevelGroup.Code, ETC_ThresholdMethods.TransactionLevelTaxBase.Code };

			IThresholdProcessor processor = new ThresholdAmountProcessor();
			var result = processor.GetTaxRecordsOutsideOfThreshold(taxParentMock.Object, taxRecords, thresholdMethodCodes);
			AssertEquals(7, result.Count);

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[3], taxRecords[4], taxRecords[6], taxRecords[7], taxRecords[10], taxRecords[12], taxRecords[14] };
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, result);

			AccTaxTransaction[] passedTaxRecordToDelete = null;
			var pivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();
			pivotProcessorMock.Setup(x => x.DeleteTaxRecordsWithPivots(It.IsAny<AccTaxTransaction[]>())).Callback((AccTaxTransaction[] recordsToDelete) => passedTaxRecordToDelete = recordsToDelete);

			processor.Process(pivotProcessorMock.Object, taxParentMock.Object, taxRecords, thresholdMethodCodes);
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, passedTaxRecordToDelete);
		}

		public void TestTRNProcessAPWithDifferentThresholdAmount()
		{
			var taxRecords = CreateTaxRecordsForTRN();
			var taxParentMock = CreateITaxRecordParentMock(LedgerTypes.AccountsPayable);
			var thresholdMethodCodes = new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code, ETC_ThresholdMethods.TransactionLevelGroup.Code };

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[0], taxRecords[1], taxRecords[5] };

			IThresholdProcessor processor = new ThresholdAmountProcessor();

			AccTaxTransaction[] passedTaxRecordToDelete = null;
			var pivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();
			pivotProcessorMock.Setup(x => x.DeleteTaxRecordsWithPivots(It.IsAny<AccTaxTransaction[]>())).Callback((AccTaxTransaction[] recordsToDelete) => passedTaxRecordToDelete = recordsToDelete);

			processor.Process(pivotProcessorMock.Object, taxParentMock.Object, taxRecords, thresholdMethodCodes);
			AssertEquals(3, passedTaxRecordToDelete.Length);
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, passedTaxRecordToDelete);
		}

		public void TestTRNProcessARWithDifferentThresholdAmount()
		{
			var taxRecords = CreateTaxRecordsForTRN();
			var taxParentMock = CreateITaxRecordParentMock(LedgerTypes.AccountsReceivable);
			var thresholdMethodCodes = new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code, ETC_ThresholdMethods.TransactionLevelGroup.Code };

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[0], taxRecords[1], taxRecords[4] };

			IThresholdProcessor processor = new ThresholdAmountProcessor();

			AccTaxTransaction[] passedTaxRecordToDelete = null;
			var pivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();
			pivotProcessorMock.Setup(x => x.DeleteTaxRecordsWithPivots(It.IsAny<AccTaxTransaction[]>())).Callback((AccTaxTransaction[] recordsToDelete) => passedTaxRecordToDelete = recordsToDelete);

			processor.Process(pivotProcessorMock.Object, taxParentMock.Object, taxRecords, thresholdMethodCodes);
			AssertEquals(3, passedTaxRecordToDelete.Length);
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, passedTaxRecordToDelete);
		}

		public void TestTRNGetTaxRecordsOutsideOfThreshold_APWithDifferentThresholdAmount()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.SetupGet(x => x.Org).Returns(TestObjectCreator.TestOrganisation);
			taxParentMock.SetupGet(x => x.Ledger).Returns(LedgerTypes.AccountsPayable);

			var taxRecords = CreateTaxRecordsForTRN();

			IThresholdProcessor processor = new ThresholdAmountProcessor();
			var result = processor.GetTaxRecordsOutsideOfThreshold(taxParentMock.Object, taxRecords, new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code });
			AssertEquals(3, result.Count);

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[0], taxRecords[1], taxRecords[5] };
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, result);
		}

		public void TestTRNGetTaxRecordsOutsideOfThreshold_ARWithDifferentThresholdAmount()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.SetupGet(x => x.Org).Returns(TestObjectCreator.TestOrganisation);
			taxParentMock.SetupGet(x => x.Ledger).Returns(LedgerTypes.AccountsReceivable);

			var taxRecords = CreateTaxRecordsForTRN();

			IThresholdProcessor processor = new ThresholdAmountProcessor();
			var result = processor.GetTaxRecordsOutsideOfThreshold(taxParentMock.Object, taxRecords, new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code });
			AssertEquals(3, result.Count);

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[0], taxRecords[1], taxRecords[4] };
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, result);
		}

		public void TestGRPProcessAPWithDifferentThresholdAmount()
		{
			var taxRecords = CreateTaxRecordsForGRP();
			var taxParentMock = CreateITaxRecordParentMock(LedgerTypes.AccountsPayable);
			var thresholdMethodCodes = new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code, ETC_ThresholdMethods.TransactionLevelGroup.Code };

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[0], taxRecords[1], taxRecords[2], taxRecords[3] };

			IThresholdProcessor processor = new ThresholdAmountProcessor();

			AccTaxTransaction[] passedTaxRecordToDelete = null;
			var pivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();
			pivotProcessorMock.Setup(x => x.DeleteTaxRecordsWithPivots(It.IsAny<AccTaxTransaction[]>())).Callback((AccTaxTransaction[] recordsToDelete) => passedTaxRecordToDelete = recordsToDelete);

			processor.Process(pivotProcessorMock.Object, taxParentMock.Object, taxRecords,thresholdMethodCodes );
			AssertEquals(4, passedTaxRecordToDelete.Length);
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, passedTaxRecordToDelete);
		}

		public void TestGRPProcessARWithDifferentThresholdAmount()
		{
			var taxRecords = CreateTaxRecordsForGRP();
			var taxParentMock = CreateITaxRecordParentMock(LedgerTypes.AccountsReceivable);
			var thresholdMethodCodes = new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code, ETC_ThresholdMethods.TransactionLevelGroup.Code };

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[2], taxRecords[3], taxRecords[4], taxRecords[5] };
			IThresholdProcessor processor = new ThresholdAmountProcessor();

			AccTaxTransaction[] passedTaxRecordToDelete = null;
			var pivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();
			pivotProcessorMock.Setup(x => x.DeleteTaxRecordsWithPivots(It.IsAny<AccTaxTransaction[]>())).Callback((AccTaxTransaction[] recordsToDelete) => passedTaxRecordToDelete = recordsToDelete);

			processor.Process(pivotProcessorMock.Object, taxParentMock.Object, taxRecords, thresholdMethodCodes);
			AssertEquals(4, passedTaxRecordToDelete.Length);
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, passedTaxRecordToDelete);
		}

		public void TestGRPGetTaxRecordsOutsideOfThreshold_APWithDifferentThresholdAmount()
		{
			var taxRecords = CreateTaxRecordsForGRP();
			var taxParentMock = CreateITaxRecordParentMock(LedgerTypes.AccountsPayable);
			var thresholdMethodCodes = new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code, ETC_ThresholdMethods.TransactionLevelGroup.Code };

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[0], taxRecords[1], taxRecords[2], taxRecords[3] };

			IThresholdProcessor processor = new ThresholdAmountProcessor();
			var result = processor.GetTaxRecordsOutsideOfThreshold(taxParentMock.Object, taxRecords.ToList(), thresholdMethodCodes);
			AssertEquals(4, result.Count);
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, result);
		}

		public void TestGRPGetTaxRecordsOutsideOfThreshold_ARWithDifferentThresholdAmount()
		{
			var taxRecords = CreateTaxRecordsForGRP();
			var taxParentMock = CreateITaxRecordParentMock(LedgerTypes.AccountsReceivable);
			var thresholdMethodCodes = new HashSet<string>() { ETC_ThresholdMethods.TransactionLevel.Code, ETC_ThresholdMethods.TransactionLevelGroup.Code };

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[2], taxRecords[3] , taxRecords[4], taxRecords[5] };

			IThresholdProcessor processor = new ThresholdAmountProcessor();
			var result = processor.GetTaxRecordsOutsideOfThreshold(taxParentMock.Object, taxRecords.ToList(), thresholdMethodCodes);
			AssertEquals(4, result.Count);
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, result);
		}

		public void TestTRBProcessAPWithDifferentThresholdAmount()
		{
			var taxRecords = CreateTaxRecordsForTRB();
			var taxParentMock = CreateITaxRecordParentMock(LedgerTypes.AccountsPayable);
			var thresholdMethodCodes = new HashSet<string>() { ETC_ThresholdMethods.TransactionLevelTaxBase.Code };

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[0], taxRecords[1], taxRecords[2], taxRecords[4], taxRecords[5] };

			IThresholdProcessor processor = new ThresholdAmountProcessor();

			AccTaxTransaction[] passedTaxRecordToDelete = null;
			var pivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();
			pivotProcessorMock.Setup(x => x.DeleteTaxRecordsWithPivots(It.IsAny<AccTaxTransaction[]>())).Callback((AccTaxTransaction[] recordsToDelete) => passedTaxRecordToDelete = recordsToDelete);

			processor.Process(pivotProcessorMock.Object, taxParentMock.Object, taxRecords, thresholdMethodCodes);
			AssertEquals(5, passedTaxRecordToDelete.Length);
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, passedTaxRecordToDelete);
		}

		public void TestTRBProcessARWithDifferentThresholdAmount()
		{
			var taxRecords = CreateTaxRecordsForTRB();
			var taxParentMock = CreateITaxRecordParentMock(LedgerTypes.AccountsReceivable);
			var thresholdMethodCodes = new HashSet<string>() { ETC_ThresholdMethods.TransactionLevelTaxBase.Code };

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[0], taxRecords[2], taxRecords[3], taxRecords[5], taxRecords[6] };

			IThresholdProcessor processor = new ThresholdAmountProcessor();

			AccTaxTransaction[] passedTaxRecordToDelete = null;
			var pivotProcessorMock = new Mock<ITaxRecordPivotProcessor>();
			pivotProcessorMock.Setup(x => x.DeleteTaxRecordsWithPivots(It.IsAny<AccTaxTransaction[]>())).Callback((AccTaxTransaction[] recordsToDelete) => passedTaxRecordToDelete = recordsToDelete);

			processor.Process(pivotProcessorMock.Object, taxParentMock.Object, taxRecords, thresholdMethodCodes);
			AssertEquals(5, passedTaxRecordToDelete.Length);
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, passedTaxRecordToDelete);
		}

		public void TestTRBGetTaxRecordsOutsideOfThreshold_APWithDifferentThresholdAmount()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.SetupGet(x => x.Org).Returns(TestObjectCreator.TestOrganisation);
			taxParentMock.SetupGet(x => x.Ledger).Returns(LedgerTypes.AccountsPayable);

			var taxRecords = CreateTaxRecordsForTRB();

			IThresholdProcessor processor = new ThresholdAmountProcessor();
			var result = processor.GetTaxRecordsOutsideOfThreshold(taxParentMock.Object, taxRecords, new HashSet<string>() { ETC_ThresholdMethods.TransactionLevelTaxBase.Code });
			AssertEquals(5, result.Count);

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[0], taxRecords[1], taxRecords[2], taxRecords[4], taxRecords[5] };
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, result);
		}

		public void TestTRBGetTaxRecordsOutsideOfThreshold_ARWithDifferentThresholdAmount()
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.SetupGet(x => x.Org).Returns(TestObjectCreator.TestOrganisation);
			taxParentMock.SetupGet(x => x.Ledger).Returns(LedgerTypes.AccountsReceivable);

			var taxRecords = CreateTaxRecordsForTRB();

			IThresholdProcessor processor = new ThresholdAmountProcessor();
			var result = processor.GetTaxRecordsOutsideOfThreshold(taxParentMock.Object, taxRecords, new HashSet<string>() { ETC_ThresholdMethods.TransactionLevelTaxBase.Code });
			AssertEquals(5, result.Count);

			var expectedTaxRecords = new HashSet<AccTaxTransaction> { taxRecords[0], taxRecords[2], taxRecords[3], taxRecords[5], taxRecords[6] };
			AssertContainsExactElementsInAnyOrder(expectedTaxRecords, result);
		}

		List<AccTaxTransaction> CreateTaxRecordsForGRP()
		{
			var taxConfigs = SetupTaxConfigs();
			UpdateTaxConfigs(taxConfigs);
			var taxRecords = new List<AccTaxTransaction>()
			{
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[5], LocalTaxAmount = -60,EffectiveRate = -1 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[6], LocalTaxAmount = -50,EffectiveRate = -1 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[7], LocalTaxAmount = 100,EffectiveRate = -1 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[8], LocalTaxAmount = 99.99,EffectiveRate = -1  }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[9], LocalTaxAmount = -150,EffectiveRate = 1 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[9], LocalTaxAmount = -150,EffectiveRate = 1 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[10], LocalTaxAmount = -300,EffectiveRate = 1 }),
				TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[11], LocalTaxAmount = 300,EffectiveRate = 1 })
			};
			return taxRecords;
		}

		List<AccTaxTransaction> CreateTaxRecordsForTRN()
		{
			var taxConfigs = SetupTaxConfigs();
			UpdateTaxConfigs(taxConfigs);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = 60, EffectiveRate = -1 });
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[0], LocalTaxAmount = 30, EffectiveRate = -1 });
			var taxRecord3 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[1], LocalTaxAmount = -100,EffectiveRate = 1 });
			var taxRecord4 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[2], LocalTaxAmount = -100, EffectiveRate = 1 });
			var taxRecord5 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[3], LocalTaxAmount = -130, EffectiveRate = 1 });
			var taxRecord6 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[4], LocalTaxAmount = -11, EffectiveRate = -1 });

			var taxRecords = new List<AccTaxTransaction> { taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5, taxRecord6 };
			return taxRecords;
		}

		List<AccTaxTransaction> CreateTaxRecordsForTRB()
		{
			var taxConfigs = SetupTaxConfigs();
			UpdateTaxConfigs(taxConfigs);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[12], LocalTaxBaseAmount = 90 });
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[12], LocalTaxBaseAmount = 100 });
			var taxRecord3 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[13], LocalTaxBaseAmount = -90 });
			var taxRecord4 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[13], LocalTaxBaseAmount = -100 });
			var taxRecord5 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[14], LocalTaxBaseAmount = 200 });
			var taxRecord6 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[15], LocalTaxBaseAmount = -190 });
			var taxRecord7 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TaxConfiguration = taxConfigs[16], LocalTaxBaseAmount = -300 });

			var taxRecords = new List<AccTaxTransaction> { taxRecord1, taxRecord2, taxRecord3, taxRecord4, taxRecord5, taxRecord6, taxRecord7 };
			return taxRecords;
		}

		Mock<ITaxRecordParent> CreateITaxRecordParentMock(string ledger)
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(x => x.Factory).Returns(Factory);
			taxParentMock.SetupGet(x => x.Company).Returns(TestObjectCreator.NonCurrentCompany);
			taxParentMock.SetupGet(x => x.Org).Returns(TestObjectCreator.TestOrganisation);
			taxParentMock.SetupGet(x => x.Ledger).Returns(ledger);

			return taxParentMock;
		}
		AccTaxConfiguration[] SetupTaxConfigs()
		{
			var company = TestObjectCreator.NonCurrentCompany;
			var branch1 = TestObjectCreator.CreateNewBranch(company, "BR1");
			var branch2 = TestObjectCreator.CreateNewBranch(company, "BR2");

			AccountingMasterFilesRegistry.Instance.RevenueTaxExpenseRecoveryChargeCode.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC1.PK.ToGuid());

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("TA");
			var taxSystem1 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS1", registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem2 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS2", registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem3 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS3", registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem4 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS4", registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem5 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS5", registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem6 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS6", registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem7 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS7", registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem8 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS8", registrationLevel: TaxSystemRegistrationLevels.Branch.Code);
			var taxSystem9 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS9", registrationLevel: TaxSystemRegistrationLevels.Branch.Code);
			var taxSystem10 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS10", registrationLevel: TaxSystemRegistrationLevels.Branch.Code);
			var taxSystem11 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS11", registrationLevel: TaxSystemRegistrationLevels.Branch.Code);
			var taxSystem12 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS12", registrationLevel: TaxSystemRegistrationLevels.Branch.Code);
			var taxSystem13 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS13", registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem14 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS14", registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem15 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS15", registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem16 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS16", registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem17 = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS17", registrationLevel: TaxSystemRegistrationLevels.Company.Code);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection
			{
				taxSystem1,
				taxSystem2,
				taxSystem3,
				taxSystem4,
				taxSystem5,
				taxSystem6,
				taxSystem7,
				taxSystem8,
				taxSystem9,
				taxSystem10,
				taxSystem11,
				taxSystem12,
				taxSystem13,
				taxSystem14,
				taxSystem15,
				taxSystem16,
				taxSystem17,
			};

			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);

			var org = TestObjectCreator.TestOrganisation;
			var ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			var companyData = org.GetCompanyDataForGlbCompany(company);

			var taxConfig1 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem1, ledger);
			taxConfig1.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevel.Code;
			taxConfig1.ETC_ThresholdAmount = 10;

			var taxConfig2 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem2, ledger);

			var taxConfig3 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem3, ledger);
			taxConfig3.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevel.Code;
			taxConfig3.ETC_ThresholdAmount = 10;

			var taxConfig4 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch2, taxAuthority, taxSystem4, ledger);
			taxConfig4.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevel.Code;
			taxConfig4.ETC_ThresholdAmount = 10;

			var taxConfig5 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch1, taxAuthority, taxSystem5, ledger);
			taxConfig5.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevel.Code;
			taxConfig5.ETC_ThresholdAmount = 10;

			var taxConfig6 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem6, ledger);
			taxConfig6.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelGroup.Code;
			taxConfig6.ETC_ThresholdAmount = 10;

			var taxConfig7 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem7, ledger);
			taxConfig7.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelGroup.Code;
			taxConfig7.ETC_ThresholdAmount = 10;

			var taxConfig8 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch1, taxAuthority, taxSystem8, ledger);
			taxConfig8.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelGroup.Code;
			taxConfig8.ETC_ThresholdAmount = 10;

			var taxConfig9 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch1, taxAuthority, taxSystem9, ledger);
			taxConfig9.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelGroup.Code;
			taxConfig9.ETC_ThresholdAmount = 10;

			var taxConfig10 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch2, taxAuthority, taxSystem10, ledger);
			taxConfig10.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelGroup.Code;
			taxConfig10.ETC_ThresholdAmount = 10;

			var taxConfig11 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(branch2, taxAuthority, taxSystem11, ledger);
			taxConfig11.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelGroup.Code;
			taxConfig11.ETC_ThresholdAmount = 10;

			var taxConfig12 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem12, ledger);

			var taxConfig13 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem13, ledger);
			taxConfig13.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelTaxBase.Code;
			taxConfig13.ETC_ThresholdAmount = 100;

			var taxConfig14 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem14, ledger);
			taxConfig14.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelTaxBase.Code;
			taxConfig14.ETC_ThresholdAmount = 100;

			var taxConfig15 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem15, ledger);
			taxConfig15.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelTaxBase.Code;
			taxConfig15.ETC_ThresholdAmount = 200;

			var taxConfig16 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem16, ledger);
			taxConfig16.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelTaxBase.Code;
			taxConfig16.ETC_ThresholdAmount = 200;

			var taxConfig17 = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem17, ledger);
			taxConfig17.ETC_ThresholdMethod = ETC_ThresholdMethods.TransactionLevelTaxBase.Code;
			taxConfig17.ETC_ThresholdAmount = 300;

			Factory.Save();

			var orgTaxConfig1 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig1, companyData);
			orgTaxConfig1.OTC_IsThresholdUsed = true;

			var orgTaxConfig2 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig2, companyData);
			orgTaxConfig2.OTC_IsThresholdUsed = true;

			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig3, companyData);

			var orgTaxConfig4 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig4, companyData);
			orgTaxConfig4.OTC_IsThresholdUsed = true;

			var orgTaxConfig5 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig5, companyData);
			orgTaxConfig5.OTC_IsThresholdUsed = true;

			var orgTaxConfig6 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig6, companyData);
			orgTaxConfig6.OTC_IsThresholdUsed = true;

			var orgTaxConfig7 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig7, companyData);
			orgTaxConfig7.OTC_IsThresholdUsed = true;

			var orgTaxConfig8 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig8, companyData);
			orgTaxConfig8.OTC_IsThresholdUsed = true;

			var orgTaxConfig9 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig9, companyData);
			orgTaxConfig9.OTC_IsThresholdUsed = true;

			var orgTaxConfig10 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig10, companyData);
			orgTaxConfig10.OTC_IsThresholdUsed = true;

			TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig11, companyData);

			var orgTaxConfig12 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig12, companyData);
			orgTaxConfig12.OTC_IsThresholdUsed = true;

			var orgTaxConfig13 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig13, companyData);
			orgTaxConfig13.OTC_IsThresholdUsed = true;

			var orgTaxConfig14 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig14, companyData);
			orgTaxConfig14.OTC_IsThresholdUsed = true;

			var orgTaxConfig15 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig15, companyData);
			orgTaxConfig15.OTC_IsThresholdUsed = true;

			var orgTaxConfig16 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig16, companyData);
			orgTaxConfig16.OTC_IsThresholdUsed = true;

			var orgTaxConfig17 = TaxFrameworkTestObjectCreator.CreateOrgTaxConfiguration(taxConfig17, companyData);
			orgTaxConfig17.OTC_IsThresholdUsed = true;
			Factory.Save();

			return new[] { taxConfig1, taxConfig2, taxConfig3, taxConfig4, taxConfig5, taxConfig6, taxConfig7, taxConfig8, taxConfig9, taxConfig10, taxConfig11, taxConfig12, taxConfig13, taxConfig14, taxConfig15, taxConfig16, taxConfig17 };
		}

		void UpdateTaxConfigs(AccTaxConfiguration[] taxConfigurations)
		{
			taxConfigurations[0].ETC_ThresholdAmount = 100;
			taxConfigurations[2].ETC_ThresholdAmount = 110;
			taxConfigurations[3].ETC_ThresholdAmount = 120;
			taxConfigurations[4].ETC_ThresholdAmount = 10;

			taxConfigurations[5].ETC_ThresholdAmount = 100;
			taxConfigurations[6].ETC_ThresholdAmount = 100;

			taxConfigurations[7].ETC_ThresholdAmount = 200;
			taxConfigurations[8].ETC_ThresholdAmount = 200;

			taxConfigurations[9].ETC_ThresholdAmount = 300;
			taxConfigurations[10].ETC_ThresholdAmount = 300;

			taxConfigurations[12].ETC_ThresholdAmount = 100;
			taxConfigurations[13].ETC_ThresholdAmount = 100;
			taxConfigurations[14].ETC_ThresholdAmount = 200;
			taxConfigurations[15].ETC_ThresholdAmount = 200;
			taxConfigurations[16].ETC_ThresholdAmount = 300;
		}

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}

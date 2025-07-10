using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	class TaxRecordLoaderTest : TestCaseWithFactory
	{
		[SuspendCriticalValidation]
		public void TestLoadAccountingTaxJournalDetails()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment = TestObjectCreator.CreateShipment("S001001", consol);
			var job = TestObjectCreator.CreateJob(shipment, createWithMutex: false);

			var currencyCode = TestObjectCreator.USD.Code;

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV10010", TestObjectCreator.USD, 1M, 150M, 0M, 150M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);
			var revenueLine = arInvoice.Lines[0];
			revenueLine.AL_JH = job.PK;
			TestObjectCreator.CreateJobCharge(revenueLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, CurrencyCode = currencyCode, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(revenueLine));
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.Matching.Code, BranchPK = GlbBranch.CurrentBranch.PK, DepartmentPK = GlbDepartment.CurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, CurrencyCode = currencyCode, DoesNotCreateGLMovemetsOnSaving = true });
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(revenueLine));
			pivot2.ATP_IsTaxExpense = true;

			var apControlAccount = TestObjectCreator.CreateAPControlAccount();
			var taxPrepaidPendingControlAccount = TestObjectCreator.CreatePendingPrepaidTaxControlAccount();
			var taxRealisedControlAccount = TestObjectCreator.CreatePrepaidAssetTaxControlAccount();

			var glMovement1 = TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord1.PK, debitAccountPK: taxPrepaidPendingControlAccount.PK, creditAccountPK: apControlAccount.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Pending.Code);
			var glMovement2 = TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord2.PK, debitAccountPK: taxRealisedControlAccount.PK, creditAccountPK: taxPrepaidPendingControlAccount.PK, amount: 8M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Realised.Code);

			Factory.Save();

			var loader = (ITaxRecordLoader)new TaxRecordLoader();
			var results = loader.LoadGLMovementDetails(arInvoice.Factory, arInvoice.PK);
			AssertEquals(4, results.Count);

			CombineAssertions("Results from LoadAccountingTaxJournalDetails()", () =>
			{
				var result = results.First();
				Assert(taxConfig.ETC_Code, taxRealisedControlAccount.AG_AccountNum, taxRealisedControlAccount.AG_Description, glMovement2.ATM_Period, glMovement2.ATM_Date, 8M, TaxBasisList.Matching.Code
					, GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code, 45M, result);

				result = results.Skip(1).First();
				Assert(taxConfig.ETC_Code, taxPrepaidPendingControlAccount.AG_AccountNum, taxPrepaidPendingControlAccount.AG_Description, glMovement2.ATM_Period, glMovement2.ATM_Date, -8M, TaxBasisList.Matching.Code
					, GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code, -45M, result);

				result = results.Skip(2).First();
				Assert(taxConfig.ETC_Code, apControlAccount.AG_AccountNum, apControlAccount.AG_Description, glMovement1.ATM_Period, glMovement1.ATM_Date, -4M, TaxBasisList.PostingOnMatching.Code
					, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code, -45M, result);

				result = results.Skip(3).First();
				Assert(taxConfig.ETC_Code, taxPrepaidPendingControlAccount.AG_AccountNum, taxPrepaidPendingControlAccount.AG_Description, glMovement1.ATM_Period, glMovement1.ATM_Date, 4M, TaxBasisList.PostingOnMatching.Code
					, TestObjectCreator.NonCurrentBranch.GB_Code, TestObjectCreator.NonCurrentDepartment.GE_Code, 45M, result);
			});

			void Assert(string taxConfigCode, string accountNumber, string accountDescription, int postPeriod, ZDate postDate, decimal amount, string basis, string branchCode, string deptCode, decimal osAmount, IGLMovementDetails result)
			{
				AssertEquals("TaxConfiguration", taxConfig.ETC_Code, result.TaxConfiguration);
				AssertEquals("GLAccount", accountNumber, result.GLAccount);
				AssertEquals("GLAccountDesc", accountDescription, result.GLAccountDesc);
				AssertEquals("PostPeriod", postPeriod.ToString(), result.PostPeriod);
				AssertEquals("PostDate", postDate, result.PostDate);
				AssertEquals("Amount", amount, result.Amount);
				AssertEquals("Basis", basis, result.Basis);
				AssertEquals("BranchCode", branchCode, result.BranchCode);
				AssertEquals("DepartmentCode", deptCode, result.DepartmentCode);
				AssertEquals("OSAmount", osAmount, result.OSAmount);
				AssertEquals("CurrencyCode", currencyCode, result.CurrencyCode);
			}
		}

		public void TestUsesGeneralLedgerDataToLoadGLMovementDetails()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment = TestObjectCreator.CreateShipment("S001001", consol);
			var job = TestObjectCreator.CreateJob(shipment, createWithMutex: false);

			var currencyCode = TestObjectCreator.USD.Code;
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV10010", TestObjectCreator.USD, 1M, 150M, 0M, 150M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK);
			var revenueLine = arInvoice.Lines[0];
			revenueLine.AL_JH = job.PK;
			TestObjectCreator.CreateJobCharge(revenueLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var taxRecord = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = ZDate.Today.AddDays(1), RealisationDate = ZDate.Today.AddDays(2), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, CurrencyCode = currencyCode, DoesNotCreateGLMovemetsOnSaving = true, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, });

			var pivot = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot.ATP_ATT = taxRecord.PK;
			pivot.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(revenueLine));
			pivot.ATP_IsTaxExpense = true;

			var apControlAccount = TestObjectCreator.CreateAPControlAccount();
			var taxPrepaidPendingControlAccount = TestObjectCreator.CreatePendingPrepaidTaxControlAccount();
			var taxRealisedControlAccount = TestObjectCreator.CreatePrepaidAssetTaxControlAccount();

			var glMovement = TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord.PK, debitAccountPK: taxPrepaidPendingControlAccount.PK, creditAccountPK: apControlAccount.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);

			Factory.Save();

			var loader = (ITaxRecordLoader)new TaxRecordLoader();
			var results = loader.LoadGLMovementDetails(arInvoice.Factory, arInvoice.PK);
			AssertEquals(2, results.Count);

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			var processor = new GeneralLedgerDataProcessor();
			var processorAsInterface = processor as IGeneralLedgerDataProcessor;
			processorAsInterface.ProcessData(new[] { ((INeedRow)arInvoice.Lines[0]).Row, ((INeedRow)glMovement).Row });

			var resultsWithUsingGeneralLedgerData = loader.LoadGLMovementDetailsWithGeneralLedgerData(arInvoice.Factory, arInvoice.PK);

			AssertEquals(2, resultsWithUsingGeneralLedgerData.Count);
		}

		public void TestGetTaxExpenses()
		{
			var loader = (ITaxRecordLoader)new TaxRecordLoader();
			var linePK = ZGuid.NewZGuid();
			var anotherLinePK = ZGuid.NewZGuid();
			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(linePK);

			AssertEquals(0, loader.GetTaxExpenses(Factory, linePK).Length);

			var taxRecord1 = Factory.New<AccTaxTransaction>();
			taxRecord1.ATT_GC = GlbCompany.CurrentCompany.PK;
			taxRecord1.ATT_RealisationDate = ZDate.Today;
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(lineMock.Object);
			pivot1.ATP_LocalTaxAmount = 2m;
			pivot1.ATP_IsTaxExpense = false;

			var taxRecord2 = Factory.New<AccTaxTransaction>();
			taxRecord2.ATT_GC = GlbCompany.CurrentCompany.PK;
			taxRecord2.ATT_RealisationDate = ZDate.Today;
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(lineMock.Object);
			pivot2.ATP_LocalTaxAmount = 3m;
			pivot2.ATP_IsTaxExpense = true;

			var taxRecord3 = Factory.New<AccTaxTransaction>();
			taxRecord3.ATT_GC = GlbCompany.CurrentCompany.PK;
			taxRecord3.ATT_RealisationDate = ZDate.Today.AddDays(-1);
			var pivot31 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot31.ATP_ATT = taxRecord3.PK;
			pivot31.LinkLine(lineMock.Object);
			pivot31.ATP_LocalTaxAmount = 5m;
			pivot31.ATP_IsTaxExpense = true;

			lineMock.Setup(l => l.PK).Returns(anotherLinePK);
			var pivot32 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot32.ATP_ATT = taxRecord3.PK;
			pivot32.LinkLine(lineMock.Object);
			pivot32.ATP_LocalTaxAmount = 13m;
			pivot32.ATP_IsTaxExpense = true;

			lineMock.Setup(l => l.PK).Returns(linePK);
			var taxRecord4 = Factory.New<AccTaxTransaction>();
			taxRecord4.ATT_GC = GlbCompany.CurrentCompany.PK;
			taxRecord4.ATT_RealisationDate = ZDate.Today;
			var pivot4 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot4.ATP_ATT = taxRecord4.PK;
			pivot4.LinkLine(lineMock.Object);
			pivot4.ATP_LocalTaxAmount = 19m;
			pivot4.ATP_IsTaxExpense = true;

			var taxExpenses = loader.GetTaxExpenses(Factory, linePK);
			AssertEquals(2, taxExpenses.Length);
			AssertEquals(ZDate.Today, taxExpenses[0].TaxExpenseDate);
			AssertEquals(22m, taxExpenses[0].TaxExpenseAmount);
			AssertEquals(ZDate.Today.AddDays(-1), taxExpenses[1].TaxExpenseDate);
			AssertEquals(5m, taxExpenses[1].TaxExpenseAmount);

			taxExpenses = loader.GetTaxExpenses(Factory, anotherLinePK);
			AssertEquals(1, taxExpenses.Length);
			AssertEquals(ZDate.Today.AddDays(-1), taxExpenses[0].TaxExpenseDate);
			AssertEquals(13m, taxExpenses[0].TaxExpenseAmount);
		}

		public void TestLoadNonSPRAPTaxRecords()
		{
			var taxParentMock = CreateITaxRecordParentMock();
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_AH = taxParentMock.Object.PK;
			taxRecord.ATT_TaxSuperType = TaxSuperTypeList.SalesTax.Code;
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxParentMock.Setup(t => t.Ledger).Returns(TaxConfigurationLedgers.AccountsPayable.Code);
			taxRecord.ATT_IsCancelled = false;

			var loader = (ITaxRecordLoader)new TaxRecordLoader();
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadNonSPRAPTaxRecords(taxParentMock.Object));

			taxParentMock.SetupGet(f => f.Factory).Returns(new BusinessObjectFactory());
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadNonSPRAPTaxRecords(taxParentMock.Object));

			taxParentMock.SetupGet(f => f.Factory).Returns(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadNonSPRAPTaxRecords(taxParentMock.Object));

			taxRecord.ATT_AH = ZGuid.NewZGuid();
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadNonSPRAPTaxRecords(taxParentMock.Object));

			taxRecord.ATT_AH = taxParentMock.Object.PK;
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadNonSPRAPTaxRecords(taxParentMock.Object));

			foreach (var superType in new TaxSuperTypeList().GetAllCodes())
			{
				foreach (var ledger in new TaxConfigurationLedgers().GetAllCodes())
				{
					taxRecord.ATT_TaxSuperType = superType;
					taxRecord.ATT_Ledger = ledger;
					taxParentMock.Setup(t => t.Ledger).Returns(ledger);

					taxRecord.ATT_IsCancelled = true;
					if (superType == TaxSuperTypeList.StandardPaymentRetention.Code && ledger == TaxConfigurationLedgers.AccountsPayable.Code)
					{
						AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadNonSPRAPTaxRecords(taxParentMock.Object));
					}
					else
					{
						AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadNonSPRAPTaxRecords(taxParentMock.Object));
					}

					taxRecord.ATT_IsCancelled = false;
					if (superType == TaxSuperTypeList.StandardPaymentRetention.Code && ledger == TaxConfigurationLedgers.AccountsPayable.Code)
					{
						AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadNonSPRAPTaxRecords(taxParentMock.Object));
					}
					else
					{
						AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadNonSPRAPTaxRecords(taxParentMock.Object));
					}
				}
			}
		}

		[SuspendCriticalValidation]
		public void TestHasRealisedSPRAPTaxRecords()
		{
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxRecord.ATT_IsCancelled = false;
			taxRecord.ATT_RealisationDate = ZDate.Empty;
			taxRecord.SubstituteGLMovementProcessor_ForTestOnly(new Mock<IGLMovementProcessor>().Object);
			Factory.Save();

			var taxParentMock = CreateITaxRecordParentMock(taxRecord.ATT_AH);

			var loader = (ITaxRecordLoader)new TaxRecordLoader();
			Assert(!loader.HasRealisedSPRAPTaxRecordsInDB(taxParentMock.Object));

			taxRecord.ATT_RealisationDate = ZDate.BrettsBirthday;
			Factory.Save();
			Assert(loader.HasRealisedSPRAPTaxRecordsInDB(taxParentMock.Object));

			taxRecord.ATT_TaxSuperType = TaxSuperTypeList.RetentionInInvoice.Code;
			Factory.Save();
			Assert(!loader.HasRealisedSPRAPTaxRecordsInDB(taxParentMock.Object));

			taxRecord.ATT_TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			Factory.Save();
			Assert(loader.HasRealisedSPRAPTaxRecordsInDB(taxParentMock.Object));

			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			Factory.Save();
			Assert(!loader.HasRealisedSPRAPTaxRecordsInDB(taxParentMock.Object));

			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			Factory.Save();
			Assert(loader.HasRealisedSPRAPTaxRecordsInDB(taxParentMock.Object));

			taxRecord.ATT_IsCancelled = true;
			Factory.Save();
			Assert(!loader.HasRealisedSPRAPTaxRecordsInDB(taxParentMock.Object));

			taxRecord.ATT_IsCancelled = false;
			Factory.Save();
			Assert(loader.HasRealisedSPRAPTaxRecordsInDB(taxParentMock.Object));
		}

		public void TestIsUsedAsDependency()
		{
			AssertType<TaxRecordLoader>(new TaxProcessor().TaxRecordLoader_ExposedForTestOnly);
			AssertType<TaxRecordLoader>(new TaxRecordReverser().TaxRecordLoader_ExposedForTestOnly);
		}

		[SuspendCriticalValidation]
		public void TestLoadSPRAPTaxRecords_UseLocalCacheOnly()
		{
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxRecord.ATT_IsCancelled = false;
			taxRecord.ATT_RealisationDate = ZDate.Empty;
			Factory.Save();

			var taxParentMock = CreateITaxRecordParentMock(taxRecord.ATT_AH, new BusinessObjectFactory());
			taxParentMock.SetupGet(x => x.IsPosted).Returns(true);
			var loader = (ITaxRecordLoader)new TaxRecordLoader();
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadSPRAPTaxRecords(taxParentMock.Object, useLocalCacheOnly: true));
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord.PK }, loader.LoadSPRAPTaxRecords(taxParentMock.Object).Select(x => x.PK));
		}

		public void TestLoadSPRAPTaxRecords()
		{
			var taxParentMock = CreateITaxRecordParentMock();
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_AH = taxParentMock.Object.PK;
			taxRecord.ATT_TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxRecord.ATT_IsCancelled = false;
			taxRecord.ATT_RealisationDate = ZDate.Empty;

			var loader = (ITaxRecordLoader)new TaxRecordLoader();
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecords(taxParentMock.Object));

			taxParentMock.SetupGet(f => f.Factory).Returns(new BusinessObjectFactory());
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadSPRAPTaxRecords(taxParentMock.Object));

			taxParentMock.SetupGet(f => f.Factory).Returns(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecords(taxParentMock.Object));

			taxRecord.ATT_AH = ZGuid.NewZGuid();
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadSPRAPTaxRecords(taxParentMock.Object));

			taxRecord.ATT_AH = taxParentMock.Object.PK;
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecords(taxParentMock.Object));

			taxRecord.ATT_TaxSuperType = TaxSuperTypeList.RetentionInInvoice.Code;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadSPRAPTaxRecords(taxParentMock.Object));

			taxRecord.ATT_TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecords(taxParentMock.Object));

			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadSPRAPTaxRecords(taxParentMock.Object));

			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecords(taxParentMock.Object));

			taxRecord.ATT_IsCancelled = true;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadSPRAPTaxRecords(taxParentMock.Object));

			taxRecord.ATT_IsCancelled = false;
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecords(taxParentMock.Object));

			taxRecord.ATT_RealisationDate = ZDate.BrettsBirthday;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadSPRAPTaxRecords(taxParentMock.Object));
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecords(taxParentMock.Object, false));

			var taxRecord2 = Factory.New<AccTaxTransaction>();
			taxRecord2.ATT_AH = taxParentMock.Object.PK;
			taxRecord2.ATT_TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			taxRecord2.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxRecord2.ATT_IsCancelled = false;
			taxRecord2.ATT_RealisationDate = ZDate.Empty;

			AssertContainsExactElementsInAnyOrder(new[] { taxRecord, taxRecord2 }, loader.LoadSPRAPTaxRecords(taxParentMock.Object, false));
		}

		public void TestLoadSPRAPTaxRecordsLinkedToMatchTransaction()
		{
			var taxParentMock = CreateITaxRecordParentMock();
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_AH = taxParentMock.Object.PK;
			taxRecord.ATT_TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxRecord.ATT_IsCancelled = false;
			var matchTrancationPK = ZGuid.NewZGuid();
			taxRecord.ATT_AH_MatchTransaction = matchTrancationPK;

			var loader = (ITaxRecordLoader)new TaxRecordLoader();
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));

			taxParentMock.SetupGet(f => f.Factory).Returns(new BusinessObjectFactory());
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));

			taxParentMock.SetupGet(f => f.Factory).Returns(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));

			taxRecord.ATT_AH = ZGuid.NewZGuid();
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));

			taxRecord.ATT_AH = taxParentMock.Object.PK;
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));

			taxRecord.ATT_TaxSuperType = TaxSuperTypeList.RetentionInInvoice.Code;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));

			taxRecord.ATT_TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));

			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));

			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));

			taxRecord.ATT_IsCancelled = true;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));

			taxRecord.ATT_IsCancelled = false;
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));

			taxRecord.ATT_AH_MatchTransaction = ZGuid.NewZGuid();
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));

			taxRecord.ATT_AH_MatchTransaction = matchTrancationPK;
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));

			var taxRecord2 = Factory.New<AccTaxTransaction>();
			taxRecord2.ATT_AH = taxParentMock.Object.PK;
			taxRecord2.ATT_TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code;
			taxRecord2.ATT_Ledger = TaxConfigurationLedgers.AccountsPayable.Code;
			taxRecord2.ATT_IsCancelled = false;
			taxRecord2.ATT_AH_MatchTransaction = matchTrancationPK;

			AssertContainsExactElementsInAnyOrder(new[] { taxRecord, taxRecord2 }, loader.LoadSPRAPTaxRecordsLinkedToMatchTransaction(taxParentMock.Object.Factory, matchTrancationPK));
		}

		[TestDate(2020, 05, 15)]
		public void TestLoadMatchingBasisTaxRecords()
		{
			var taxParentMock = CreateITaxRecordParentMock();
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_AH = taxParentMock.Object.PK;
			taxRecord.ATT_Basis = TaxBasisList.Matching.Code;
			taxRecord.ATT_RealisationDate = ZDate.Empty;

			var loader = (ITaxRecordLoader)new TaxRecordLoader();
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadMatchingBasisTaxRecords(taxParentMock.Object));

			taxParentMock.SetupGet(f => f.Factory).Returns(new BusinessObjectFactory());
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadMatchingBasisTaxRecords(taxParentMock.Object));

			taxParentMock.SetupGet(f => f.Factory).Returns(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadMatchingBasisTaxRecords(taxParentMock.Object));

			taxRecord.ATT_AH = ZGuid.NewZGuid();
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadMatchingBasisTaxRecords(taxParentMock.Object));

			taxRecord.ATT_AH = taxParentMock.Object.PK;
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadMatchingBasisTaxRecords(taxParentMock.Object));

			taxRecord.ATT_Basis = TaxBasisList.Posting.Code;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadMatchingBasisTaxRecords(taxParentMock.Object));

			taxRecord.ATT_Basis = TaxBasisList.Matching.Code;
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord }, loader.LoadMatchingBasisTaxRecords(taxParentMock.Object));

			taxRecord.ATT_RealisationDate = ZDate.Today;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadMatchingBasisTaxRecords(taxParentMock.Object));
		}

		[TestDate(2020, 06, 30)]
		public void TestLoadAllReportableTaxRecords()
		{
			var testObjectCreator = new TaxFrameworkTestObjectCreator(Factory);
			var taxParentMock1 = CreateITaxRecordParentMock();
			var taxParentMock2 = CreateITaxRecordParentMock();
			var taxParentMock3 = CreateITaxRecordParentMock();

			var taxRecord1_1 = testObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeaderPK = taxParentMock1.Object.PK, IsCancelled = false });
			var taxRecord1_2 = testObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeaderPK = taxParentMock1.Object.PK, IsCancelled = true });
			var taxRecord1_3 = testObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeaderPK = taxParentMock1.Object.PK, IsCancelled = false, RealisationDate = ZDate.Today });
			var taxRecord1_4 = testObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeaderPK = taxParentMock1.Object.PK, IsCancelled = true, RealisationDate = ZDate.Today });

			var taxRecord2_1 = testObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeaderPK = taxParentMock2.Object.PK, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, IsCancelled = false });
			var taxRecord2_2 = testObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeaderPK = taxParentMock2.Object.PK, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, IsCancelled = true });
			var taxRecord2_3 = testObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeaderPK = taxParentMock2.Object.PK, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, IsCancelled = false, RealisationDate = ZDate.Today });
			var taxRecord2_4 = testObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TransactionHeaderPK = taxParentMock2.Object.PK, TaxSuperType = TaxSuperTypeList.StandardPaymentRetention.Code, IsCancelled = true, RealisationDate = ZDate.Today });

			var loader = (ITaxRecordLoader)new TaxRecordLoader();
			AssertContainsExactElementsInAnyOrder(new[] { taxRecord1_1, taxRecord1_2, taxRecord1_3, taxRecord1_4 }, loader.LoadAllReportableTaxRecords(taxParentMock1.Object));

			AssertContainsExactElementsInAnyOrder(new[] { taxRecord2_1, taxRecord2_3, taxRecord2_4 }, loader.LoadAllReportableTaxRecords(taxParentMock2.Object));

			AssertContainsExactElementsInAnyOrder(System.Array.Empty<AccTaxTransaction>(), loader.LoadAllReportableTaxRecords(taxParentMock3.Object));
		}

		Mock<ITaxRecordParent> CreateITaxRecordParentMock(ZGuid? pk = null, BusinessObjectFactory factory = null)
		{
			var taxParentMock = new Mock<ITaxRecordParent>();
			taxParentMock.SetupGet(f => f.Factory).Returns(factory ?? Factory);
			taxParentMock.SetupGet(f => f.PK).Returns(pk ?? ZGuid.NewZGuid());

			return taxParentMock;
		}

		public void TestGetAccTaxTransactionBasedOnAPAndARLine()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var loader = (ITaxRecordLoader)new TaxRecordLoader();
			var arInvoice = testObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", organisation: testObjectCreator.TestOrganisation);
			var arInvoiceLine = testObjectCreator.CreateInvoiceLine(arInvoice, 100);

			var apInvoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "INV2", organisation: testObjectCreator.TestOrganisation);
			var apInvoiceLine = testObjectCreator.CreateInvoiceLine(apInvoice, 100);

			var databaseLoadCount = Factory.DatabaseLoadCount;

			var accTaxTransactionPivot = loader.LoadTaxRecordsAndPivotsLinkedToTaxableLines(Factory, arInvoiceLine.PK, apInvoiceLine.PK);
			AssertEquals(databaseLoadCount + 1, Factory.DatabaseLoadCount);

			var taxRecord1 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord1.ATT_AH = arInvoice.PK;
			var taxRecord2 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord2.ATT_AH = arInvoice.PK;
			var taxLinePivot1 = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord1.PK))[0];
			taxLinePivot1.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(arInvoiceLine));
			var taxLinePivot2 = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord2.PK))[0];
			taxLinePivot2.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(arInvoiceLine));

			var taxRecord3 = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord3.ATT_AH = apInvoice.PK;
			var taxLinePivot3 = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord3.PK))[0];
			taxLinePivot3.LinkLine(TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(apInvoiceLine));

			Factory.Save();
			databaseLoadCount = Factory.DatabaseLoadCount;
			accTaxTransactionPivot = loader.LoadTaxRecordsAndPivotsLinkedToTaxableLines(Factory, arInvoiceLine.PK, apInvoiceLine.PK);
			AssertList(3, new[] { taxRecord1, taxRecord2, taxRecord3 }, new[] { taxLinePivot1, taxLinePivot2, taxLinePivot3 });
			AssertEquals(databaseLoadCount + 2, Factory.DatabaseLoadCount);

			accTaxTransactionPivot = loader.LoadTaxRecordsAndPivotsLinkedToTaxableLines(Factory, arInvoiceLine.PK, ZGuid.Empty);
			AssertList(2, new[] { taxRecord1, taxRecord2 }, new[] { taxLinePivot1, taxLinePivot2 });

			accTaxTransactionPivot = loader.LoadTaxRecordsAndPivotsLinkedToTaxableLines(Factory, ZGuid.Empty, apInvoiceLine.PK);
			AssertList(1, new[] { taxRecord3 }, new[] { taxLinePivot3 });

			accTaxTransactionPivot = loader.LoadTaxRecordsAndPivotsLinkedToTaxableLines(Factory, ZGuid.Empty, ZGuid.Empty);
			Assert(accTaxTransactionPivot.Count == 0);

			void AssertList(int count, AccTaxTransaction[] expactedAccTaxTransactions, AccTaxRecordTransactionLinePivot[] expactedAccTaxRecordTransactionLinePivots)
			{
				AssertEquals(count, accTaxTransactionPivot.Count);
				AssertContainsExactElementsInAnyOrder("AccTaxTransaction should be same", expactedAccTaxTransactions, accTaxTransactionPivot.Select(item => item.accTaxTransaction).ToArray());
				AssertContainsExactElementsInAnyOrder("AccTaxRecordTransactionLinePivot should be same", expactedAccTaxRecordTransactionLinePivots, accTaxTransactionPivot.Select(item => item.accTaxRecordTransactionLinePivot).ToArray());
			}
		}

		#region LoadTaxRecordPivots

		public void TestLoadTaxRecordPivots_ReturnsEmptyPivots_WhenInputTaxRecordsAreEmpty()
		{
			var taxRecordLoader = (ITaxRecordLoader)new TaxRecordLoader();
			var pivots = taxRecordLoader.LoadTaxRecordPivots(false);

			AssertContainsExactElementsInAnyOrder("When input taxRecords are empty", System.Array.Empty<AccTaxRecordTransactionLinePivot>(), pivots);
		}

		public void TestLoadTaxRecordPivots_ReturnsEmptyPivots_WhenNoMatchingPivotsAreFoundForInputTaxRecords()
		{
			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var taxRecord2 = Factory.New<AccTaxTransaction>();

			var taxRecordLoader = (ITaxRecordLoader)new TaxRecordLoader();
			var pivots = taxRecordLoader.LoadTaxRecordPivots(false, taxRecord1, taxRecord2);

			AssertContainsExactElementsInAnyOrder("When query returns an empty taxRecordPivots array", System.Array.Empty<AccTaxRecordTransactionLinePivot>(), pivots);
		}

		public void TestLoadTaxRecordPivots_ReturnsPivots_WhenValidTaxRecordsArePassed()
		{
			var taxRecord1 = Factory.New<AccTaxTransaction>();
			var taxRecord2 = Factory.New<AccTaxTransaction>();
			var taxRecord3 = Factory.New<AccTaxTransaction>();

			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			var pivot3 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot3.ATP_ATT = taxRecord3.PK;

			var taxRecordLoader = (ITaxRecordLoader)new TaxRecordLoader();

			var pivots = taxRecordLoader.LoadTaxRecordPivots(true, taxRecord1, taxRecord2);
			AssertContainsExactElementsInAnyOrder("Returns taxReocrds", new AccTaxRecordTransactionLinePivot[] { pivot1, pivot2 }, pivots);

			pivots = taxRecordLoader.LoadTaxRecordPivots(true, taxRecord1, taxRecord2, taxRecord3);
			AssertContainsExactElementsInAnyOrder("Returns taxRecords", new AccTaxRecordTransactionLinePivot[] { pivot1, pivot2, pivot3 }, pivots);
		}

		public void TestLoadTaxRecordPivots_WhenInputParameterUseLocalCacheOnlyIsTrue()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.CC1.PK, 10);

			var taxTransactionsParams = new CreateTaxTransactionParameters
			{
				TransactionHeader = invoice,
				OsTaxAmount = 10,
				LocalTaxAmount = 10,
				TaxBasis = TaxBasisList.PostingOnMatching.Code,
				AffectsSourceTransactionTotal = false,
			};
			var taxTransaction1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(taxTransactionsParams);
			var taxTransaction2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(taxTransactionsParams);
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransaction1.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(invoiceLine));
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransaction2.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(invoiceLine));

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var taxRecords = newFactory.Load<AccTaxTransaction>(new ZQuery());
			var taxRecordLoader = (ITaxRecordLoader)new TaxRecordLoader();

			var pivots = taxRecordLoader.LoadTaxRecordPivots(true, taxRecords);
			AssertContainsExactElementsInAnyOrder("When pivots are not present in local cache", System.Array.Empty<AccTaxRecordTransactionLinePivot>(), pivots);

			var expectedPivots = newFactory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery());
			pivots = taxRecordLoader.LoadTaxRecordPivots(true, taxRecords);
			AssertContainsExactElementsInAnyOrder("When pivots are present in local cache", expectedPivots, pivots);
		}

		public void TestLoadTaxRecordPivots_WhenInputParameterUseLocalCacheOnlyIsFalse()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice));
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.CC1.PK, 10);

			var taxTransactionsParams = new CreateTaxTransactionParameters
			{
				TransactionHeader = invoice,
				OsTaxAmount = 10,
				LocalTaxAmount = 10,
				TaxBasis = TaxBasisList.PostingOnMatching.Code,
				AffectsSourceTransactionTotal = false,
			};
			var taxTransaction = TaxFrameworkTestObjectCreator.CreateTaxTransaction(taxTransactionsParams);
			var pivot1 = TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransaction.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(invoiceLine));

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var taxRecords = newFactory.Load<AccTaxTransaction>(new ZQuery());
			var taxRecordLoader = (ITaxRecordLoader)new TaxRecordLoader();

			var pivots = taxRecordLoader.LoadTaxRecordPivots(false, taxRecords);
			AssertEquals("When pivots are loaded from DB", pivots[0].PK, pivot1.PK);
		}

		#endregion

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator
		{
			get { return taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory)); }
		}
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}

using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.WipsAndAccruals.Testing
{
	public class WIPAccrualPostingTransactionExportFilterTest : WIPAccrualTransactionExportFilterBaseTest
	{
		public void TestTransactionFilterPicksUpCorrectJobRelatedTransactions()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.Jobs.Add(ObjectCreator.JobHeader1);

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			Assert("WIP Accrual Collection should contain WIP1", BusinessObjectIsInCollectionByPK(wIPs, WIP1));
			Assert("WIP Accrual Collection should not contain WIP2", !BusinessObjectIsInCollectionByPK(wIPs, WIP2Reversed));
			Assert("WIP Accrual Collection should contain Accrual1", BusinessObjectIsInCollectionByPK(wIPs, Accrual1));
			Assert("WIP Accrual Collection should not contain Accrual2", !BusinessObjectIsInCollectionByPK(wIPs, Accrual2Reversed));

			Assert("WIP Accrual Collection should not contain WIPFromOtherCompany", !BusinessObjectIsInCollectionByPK(wIPs, WIPFromAnotherCompany));

			AssertEquals("WIPAcc Posted Collection Count", 2, wIPs.Count);

			FakeABatch(wIPs);
			FilterProvider.CurrentBatchNo = 1;

			wIPs.Load(Filter.Filter);

			Assert("WIP Accrual Collection should contain WIP1", BusinessObjectIsInCollectionByPK(wIPs, WIP1));
			Assert("WIP Accrual Collection should contain Accrual1", BusinessObjectIsInCollectionByPK(wIPs, Accrual1));

			Assert("WIP Accrual Collection should not contain WIPFromOtherCompany", !BusinessObjectIsInCollectionByPK(wIPs, WIPFromAnotherCompany));

			AssertEquals("WIPAcc Posted Collection Count", 2, wIPs.Count);
		}

		public void TestTransactionFilterPicksUpNoTransactionsWhenExcludingEverything()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;

			AssertEquals("WIPAccruals Posted Count", 0, Filter.NumberOfObjects);
		}

		public void TestTransactionFilterForJobRelatedTransactionsForAR()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = false;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			Assert("WIP Accrual Post Collection should contain WIP 1", BusinessObjectIsInCollectionByPK(wIPs, WIP1));
			Assert("WIP Accrual Post Collection should contain WIP 2", BusinessObjectIsInCollectionByPK(wIPs, WIP2Reversed));
			Assert("WIP Accrual Post Collection should not contain Accrual 1", !BusinessObjectIsInCollectionByPK(wIPs, Accrual1));
			Assert("WIP Accrual Post Collection should not contain Accrual 2", !BusinessObjectIsInCollectionByPK(wIPs, Accrual2Reversed));

			AssertEquals("WIPAcc Posted Collection Count", 2, wIPs.Count);
		}

		public void TestTransactionFilterForNonJobRelatedTransactionsForAR()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = false;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			AssertEquals("WIPAcc Posted Collection Count", 0, wIPs.Count);
		}

		public void TestTransactionFilterForJobRelatedTransactionsForAP()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = false;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			Assert("WIP Accrual Post Collection should not contain WIP 1", !BusinessObjectIsInCollectionByPK(wIPs, WIP1));
			Assert("WIP Accrual Post Collection should not contain WIP 2", !BusinessObjectIsInCollectionByPK(wIPs, WIP2Reversed));
			Assert("WIP Accrual Post Collection should contain Accrual 1", BusinessObjectIsInCollectionByPK(wIPs, Accrual1));
			Assert("WIP Accrual Post Collection should contain Accrual 2", BusinessObjectIsInCollectionByPK(wIPs, Accrual2Reversed));

			AssertEquals("WIPAcc Posted Collection Count", 2, wIPs.Count);
		}

		public void TestTransactionFilterForNonJobRelatedTransactionsForAP()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = false;

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			AssertEquals("WIPAcc Posted Collection Count", 0, wIPs.Count);
		}

		public void TestTransactionFilterForToAndFromDates()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.DateFrom = ZDateTime.Today.AddDays(-1);
			FilterProvider.DateTo = ZDateTime.Today.AddDays(1);

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			Assert("WIP Accrual Post Collection should contain Accrual 1", BusinessObjectIsInCollectionByPK(wIPs, Accrual1));

			AssertEquals("WIP Accrual in Post Collection", 1, wIPs.Count);
		}

		public void TestTransactionFilterForToAndFromPeriods()
		{
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();
			Factory.Save();

			IncludeAllTransactionTypes(true);

			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(Factory);
			ZInt currentPeriod = periodCalc.GetPeriodFromDate(ZDateTime.Today);

			FilterProvider.PeriodFrom = periodTestHelper.CurrentPeriodInt;
			FilterProvider.PeriodTo = periodTestHelper.CurrentPeriodInt;

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			Assert("WIP Accrual Post Collection should contain Accrual 1", BusinessObjectIsInCollectionByPK(wIPs, Accrual1));

			AssertEquals("WIP Accrual in Post Collection", 1, wIPs.Count);
		}

		public void TestTransactionFilterOnlyPicksUpTransactionsFromCurrentCompany()
		{
			FilterProvider.IncludeWIPsPosting = true;
			FilterProvider.IncludeAccrualsPosting = true;

			Filter = new WIPAccrualPostingTransactionExportFilter(Factory, FilterProvider);

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			AssertEquals("Collection should contain WIP1 (Posting)", true, wIPs.Contains(WIP1.PK));
			AssertEquals("Collection should contain WIP 2 (Posting)", true, wIPs.Contains(WIP2Reversed.PK));
			AssertEquals("Collection should contain Accrual 1 (Posting)", true, wIPs.Contains(Accrual1.PK));
			AssertEquals("Collection should contain Accrual 2(Posting)", true, wIPs.Contains(WIP2Reversed.PK));
			AssertEquals("Collection should not contain WIP from other Company (Posting)", false, wIPs.Contains(WIPFromAnotherCompany.PK));

			ObjectCreator.CreateGenExportBatchSequencePostLine(1, WIPFromAnotherCompany.PK, 1);
			FakeABatch(wIPs);

			FilterProvider.CurrentBatchNo = 1;
			Filter = new WIPAccrualPostingTransactionExportFilter(Factory, FilterProvider);

			wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			AssertEquals("Collection should contain WIP1 (Posting)", true, wIPs.Contains(WIP1.PK));
			AssertEquals("Collection should contain WIP 2 (Posting)", true, wIPs.Contains(WIP2Reversed.PK));
			AssertEquals("Collection should contain Accrual 1 (Posting)", true, wIPs.Contains(Accrual1.PK));
			AssertEquals("Collection should contain Accrual 2(Posting)", true, wIPs.Contains(WIP2Reversed.PK));
			AssertEquals("Collection should not contain WIP from other Company (Posting)", false, wIPs.Contains(WIPFromAnotherCompany.PK));
		}

		public void TestTransactionFilterForOrganisations()
		{
			IncludeAllTransactionTypes(true);

			ObjectCreator.AALSHI.CompanyData.OB_IsDebtor = true;
			WIP1.RelatedJobCharge.JR_OH_SellAccount = ObjectCreator.AALSHI.PK;
			WIP1.AL_OH = ObjectCreator.AALSHI.PK;

			WIP2Reversed.AL_OH = ZGuid.Empty;

			ObjectCreator.LocalClient.CompanyData.OB_IsCreditor = true;
			Accrual1.RelatedJobCharge.JR_OH_CostAccount = ObjectCreator.LocalClient.PK;
			Accrual1.AL_OH = ObjectCreator.LocalClient.PK;

			Accrual2Reversed.AL_OH = ObjectCreator.ZECTRA.PK;

			WIPFromAnotherCompany.AL_OH = ObjectCreator.ZECTRA.PK;

			Factory.Save();

			FilterProvider.Organisations.Add(ObjectCreator.AALSHI);
			FilterProvider.Organisations.Add(ObjectCreator.ABIGAS);
			FilterProvider.Organisations.Add(ObjectCreator.ZECTRA);

			AssertEquals("PreCondition: FilterProvider.Organisations has 3 Organisations", 3, FilterProvider.Organisations.Count);

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			Assert("WIP Acc Posted Collection should contain WIP1", BusinessObjectIsInCollectionByPK(wIPs, WIP1));
			Assert("WIP Acc Posted Collection should contain Accrual2", BusinessObjectIsInCollectionByPK(wIPs, Accrual2Reversed));

			AssertEquals("WIPAcc Posted Collection Count", 2, wIPs.Count);
		}

		public void TestTransactionTypeFilterIncludeWIPsPosting()
		{
			AssertEquals("Batch Number", 0, FilterProvider.CurrentBatchNo);
			FilterProvider.IncludeWIPsPosting = true;

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			AssertEquals("WIPAcc Posted Collection Count", 2, wIPs.Count);
			Assert("WIPacc Posted Collection contains WIP1", BusinessObjectIsInCollectionByPK(wIPs, WIP1));
			Assert("WIPacc Posted Collection contains WIP2", BusinessObjectIsInCollectionByPK(wIPs, WIP2Reversed));

			FakeABatch(wIPs);
			FilterProvider.CurrentBatchNo = 1;

			WIPAccrualCollection wIPs2 = new WIPAccrualCollection(Factory);
			wIPs2.Load(Filter.Filter);

			AssertEquals("WIPAcc Posted Collection Count", 2, wIPs2.Count);
			Assert("WIPacc Posted Collection contains WIP1", BusinessObjectIsInCollectionByPK(wIPs2, WIP1));
			Assert("WIPacc Posted Collection contains WIP2", BusinessObjectIsInCollectionByPK(wIPs2, WIP2Reversed));
		}

		public void TestTransactionTypeFilterIncludeAccrualPosting()
		{
			AssertEquals("Batch Number", 0, FilterProvider.CurrentBatchNo);
			FilterProvider.IncludeAccrualsPosting = true;

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			AssertEquals("WIPAcc Posted Collection Count", 2, wIPs.Count);
			Assert("WIPacc Posted Collection contains Accrual1", BusinessObjectIsInCollectionByPK(wIPs, Accrual1));
			Assert("WIPacc Posted Collection contains Accrual2Reversed", BusinessObjectIsInCollectionByPK(wIPs, Accrual2Reversed));

			FakeABatch(wIPs);
			FilterProvider.CurrentBatchNo = 1;

			WIPAccrualCollection wIPs2 = new WIPAccrualCollection(Factory);
			wIPs2.Load(Filter.Filter);

			AssertEquals("WIPAcc Posted Collection Count", 2, wIPs2.Count);
			Assert("WIPacc Posted Collection contains Accrual1", BusinessObjectIsInCollectionByPK(wIPs2, Accrual1));
			Assert("WIPacc Posted Collection contains Accrual2Reversed", BusinessObjectIsInCollectionByPK(wIPs2, Accrual2Reversed));
		}

		void FakeABatch(BusinessObjectCollection collectionOfBizObjsToFakeBatch)
		{
			foreach (BusinessObject bizObj in collectionOfBizObjsToFakeBatch)
			{
				ObjectCreator.CreateGenExportBatchSequencePostLine(1, ((BaseWIPAccrual)bizObj).PK, 1);
			}

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Filter = new WIPAccrualPostingTransactionExportFilter(Factory, FilterProvider);
		}

		WIPAccrualPostingTransactionExportFilter Filter;

		protected override TransactionExportFilter GetNewExportFilter()
		{
			return new WIPAccrualPostingTransactionExportFilter(Factory, FilterProvider);
		}

		protected override SchemaDateTimeColumn ExpectedSystemLastEditTimeColumn
		{
			get { return AccTransactionLinesSchema.AL_SystemLastEditTimeUtc; }
		}
	}

	public class WIPAccrualPostingTransactionExportFilterForBranchAndDepartmentTest : WIPAccrualTransactionExportFilterBaseTest
	{
		public void TestTransactionFilterForBranches()
		{
			IncludeAllTransactionTypes(true);

			#region Make 6 New Test Branches

			GlbBranch newBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch1.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch newBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch2.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch newBranch3 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch3.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch newBranch4 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch4.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch newBranch5 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch5.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch newBranch6 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch6.GB_GC = GlbCompany.CurrentCompany.PK;

			#endregion

			WIP1.AL_GB = newBranch1.PK;
			WIP2Reversed.AL_GB = newBranch2.PK;
			Accrual1.AL_GB = newBranch3.PK;
			Accrual2Reversed.AL_GB = newBranch4.PK;

			Factory.SuspendValidation();
			Factory.Save();
			Factory.ResumeValidation();

			FilterProvider.Branches.Add(newBranch1);
			FilterProvider.Branches.Add(newBranch3);
			FilterProvider.Branches.Add(newBranch5);

			AssertEquals("PreCondition: FilterProvider.Branches has 3 Branches", 3, FilterProvider.Branches.Count);

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			Assert("WIP Acc Posted Collection should contain WIP1", BusinessObjectIsInCollectionByPK(wIPs, WIP1));
			Assert("WIP Acc Posted Collection should contain Accrual", BusinessObjectIsInCollectionByPK(wIPs, Accrual1));

			AssertEquals("WIPAcc Posted Collection Count", 2, wIPs.Count);
		}

		public void TestTransactionFilterForDepartments()
		{
			IncludeAllTransactionTypes(true);

			GlbDepartment newDepartment1 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment newDepartment2 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment newDepartment3 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment newDepartment4 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment newDepartment5 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment newDepartment6 = Factory.NewWithValidTestData<GlbDepartment>();

			ARInvoice1.AH_GE = newDepartment1.PK;
			ARCreditNote1.AH_GE = newDepartment2.PK;
			ARAdjustmentNote1.AH_GE = newDepartment3.PK;

			APInvoice1.AH_GE = newDepartment4.PK;
			APCreditNote1.AH_GE = newDepartment5.PK;
			APAdjustmentNote1.AH_GE = newDepartment6.PK;

			ARInvoiceFromAnotherCompany.AH_GE = newDepartment1.PK;

			WIP1.AL_GE = newDepartment1.PK;
			WIP2Reversed.AL_GE = newDepartment2.PK;
			Accrual1.AL_GE = newDepartment3.PK;
			Accrual2Reversed.AL_GE = newDepartment4.PK;

			Factory.SuspendValidation();
			Factory.Save();
			Factory.ResumeValidation();

			FilterProvider.Departments.Add(newDepartment2);
			FilterProvider.Departments.Add(newDepartment4);
			FilterProvider.Departments.Add(newDepartment6);
			AssertEquals("PreCondition: FilterProvider.Departmentes has 3 Departments", 3, FilterProvider.Departments.Count);

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			Assert("WIP Accrual Collection should not contain WIP from other company", !BusinessObjectIsInCollectionByPK(wIPs, WIPFromAnotherCompany));
			AssertEquals("WIPAcc Posted Collection Count", 2, wIPs.Count);
		}

		public override void TestGetFilterPks()
			=> Assert($"Tested in {nameof(WIPAccrualPostingTransactionExportFilterTest)} but doesn't work when IsForceSetupSave is true", true);

		protected override void SetUp()
		{
			base.SetUp();
			Filter = new WIPAccrualPostingTransactionExportFilter(Factory, FilterProvider);
		}

		protected override TransactionExportFilter GetNewExportFilter()
		{
			return new WIPAccrualPostingTransactionExportFilter(Factory, FilterProvider);
		}

		protected override SchemaDateTimeColumn ExpectedSystemLastEditTimeColumn
		{
			get { return AccTransactionLinesSchema.AL_SystemLastEditTimeUtc; }
		}

		protected override bool IsForceSetupSave => false;

		WIPAccrualPostingTransactionExportFilter Filter;
	}
}

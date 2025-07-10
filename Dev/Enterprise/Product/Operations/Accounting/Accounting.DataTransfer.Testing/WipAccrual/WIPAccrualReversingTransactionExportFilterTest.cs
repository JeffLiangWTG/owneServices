using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.WipsAndAccruals.Testing
{
	public class WIPAccrualReversingTransactionExportFilterTest : WIPAccrualTransactionExportFilterBaseTest
	{
		public void TestTransactionFilterPicksUpCorrectJobRelatedTransactions()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.Jobs.Add(ObjectCreator.Job1);

			WIPAccrualCollection reversedWIPs = new WIPAccrualCollection(Factory);
			reversedWIPs.Load(Filter.Filter);

			Assert("WIP Accrual Collection should not contain WIP2", !BusinessObjectIsInCollectionByPK(reversedWIPs, WIP2Reversed));
			Assert("WIP Accrual Collection should not contain Accrual2", !BusinessObjectIsInCollectionByPK(reversedWIPs, Accrual2Reversed));

			Assert("TransactionHeader Collection does not contain TransactionHeader from other company", !BusinessObjectIsInCollectionByPK(reversedWIPs, ARInvoiceFromAnotherCompany));
			Assert("WIP Accrual Collection should not contain WIPFromOtherCompany", !BusinessObjectIsInCollectionByPK(reversedWIPs, WIPFromAnotherCompany));

			AssertEquals("WIPAcc Reversed Collection Count", 0, reversedWIPs.Count);

			FakeABatch(reversedWIPs);
			FilterProvider.CurrentBatchNo = 1;

			Assert("WIP Accrual Collection should not contain WIP2", !BusinessObjectIsInCollectionByPK(reversedWIPs, WIP2Reversed));
			Assert("WIP Accrual Collection should not contain Accrual2", !BusinessObjectIsInCollectionByPK(reversedWIPs, Accrual2Reversed));
			Assert("WIP Accrual Collection should not contain WIPFromOtherCompany", !BusinessObjectIsInCollectionByPK(reversedWIPs, WIPFromAnotherCompany));

			AssertEquals("WIPAcc Reversed Collection Count", 0, reversedWIPs.Count);
		}

		public void TestTransactionFilterPicksUpNoTransactionsWhenExcludingEverything()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;

			WIPAccrualCollection reversedWIPs = new WIPAccrualCollection(Factory);
			reversedWIPs.Load(Filter.Filter);

			AssertEquals("WIPAccruals Reversed Count", 0, reversedWIPs.Count);
		}

		public void TestTransactionFilterForJobRelatedTransactionsForAR()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = false;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;

			WIPAccrualCollection reversedWIPs = new WIPAccrualCollection(Factory);
			reversedWIPs.Load(Filter.Filter);

			Assert("WIP Accrual Reverse Collection should contain WIP 2", BusinessObjectIsInCollectionByPK(reversedWIPs, WIP2Reversed));
			Assert("WIP Accrual Reverse Collection should not contain Accrual 2", !BusinessObjectIsInCollectionByPK(reversedWIPs, Accrual2Reversed));

			AssertEquals("WIPAcc Reversed Collection Count", 1, reversedWIPs.Count);
		}

		public void TestTransactionFilterForNonJobRelatedTransactionsForAR()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = false;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;

			WIPAccrualCollection reversedWIPs = new WIPAccrualCollection(Factory);
			reversedWIPs.Load(Filter.Filter);

			AssertEquals("WIPAcc Reversed Collection Count", 0, reversedWIPs.Count);
		}

		public void TestTransactionFilterForJobRelatedTransactionsForAP()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = false;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = true;

			WIPAccrualCollection reversedWIPs = new WIPAccrualCollection(Factory);
			reversedWIPs.Load(Filter.Filter);

			Assert("WIP Accrual Reverse Collection should not contain WIP 2", !BusinessObjectIsInCollectionByPK(reversedWIPs, WIP2Reversed));
			Assert("WIP Accrual Reverse Collection should contain Accrual 2", BusinessObjectIsInCollectionByPK(reversedWIPs, Accrual2Reversed));

			AssertEquals("WIPAcc Reversed Collection Count", 1, reversedWIPs.Count);
		}

		public void TestTransactionFilterForNonJobRelatedTransactionsForAP()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.ExcludeJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAR = true;
			FilterProvider.ExcludeJobRelatedTransactionsForAP = true;
			FilterProvider.ExcludeNonJobRelatedTransactionsForAP = false;

			WIPAccrualCollection reversedWIPs = new WIPAccrualCollection(Factory);
			reversedWIPs.Load(Filter.Filter);

			AssertEquals("WIPAcc Reversed Collection Count", 0, reversedWIPs.Count);
		}

		public void TestTransactionFilterForToAndFromDates()
		{
			IncludeAllTransactionTypes(true);

			FilterProvider.DateFrom = ZDateTime.Today.AddDays(-1);
			FilterProvider.DateTo = ZDateTime.Today.AddDays(1);

			WIPAccrualCollection reversedWIPs = new WIPAccrualCollection(Factory);
			reversedWIPs.Load(Filter.Filter);

			Assert("WIP Accrual Reverse Collection should contain WIP 2", BusinessObjectIsInCollectionByPK(reversedWIPs, WIP2Reversed));

			AssertEquals("WIP Accrual in Reverse Collection", 1, reversedWIPs.Count);
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

			WIPAccrualCollection reversedWIPs = new WIPAccrualCollection(Factory);
			reversedWIPs.Load(Filter.Filter);

			Assert("WIP Accrual Reverse Collection should contain WIP 2", BusinessObjectIsInCollectionByPK(reversedWIPs, WIP2Reversed));

			AssertEquals("WIP Accrual in Reverse Collection", 1, reversedWIPs.Count);
		}

		public void TestTransactionFilterOnlyPicksUpTransactionsFromCurrentCompany()
		{
			FilterProvider.IncludeWIPsReversing = true;
			FilterProvider.IncludeAccrualsReversing = true;

			Filter = new WIPAccrualReversingTransactionExportFilter(Factory, FilterProvider);

			WIPAccrualCollection wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			AssertEquals("Collection should contain WIP 2 (Reversing)", true, wIPs.Contains(WIP2Reversed.PK));
			AssertEquals("Collection should contain Accrual 2(Reversing)", true, wIPs.Contains(WIP2Reversed.PK));
			AssertEquals("Collection should not contain WIP from other Company (Reversing)", false, wIPs.Contains(WIPFromAnotherCompany.PK));

			ObjectCreator.CreateGenExportBatchSequenceReverseLine(1, WIPFromAnotherCompany.PK, 1);
			FakeABatch(wIPs);

			FilterProvider.CurrentBatchNo = 1;
			Filter = new WIPAccrualReversingTransactionExportFilter(Factory, FilterProvider);

			wIPs = new WIPAccrualCollection(Factory);
			wIPs.Load(Filter.Filter);

			AssertEquals("Collection should contain WIP 2 (Reversing)", true, wIPs.Contains(WIP2Reversed.PK));
			AssertEquals("Collection should contain Accrual 2(Reversing)", true, wIPs.Contains(WIP2Reversed.PK));
			AssertEquals("Collection should not contain WIP from other Company (Reversing)", false, wIPs.Contains(WIPFromAnotherCompany.PK));
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

			ObjectCreator.ZECTRA.GetCompanyDataForGlbCompany(WIPFromAnotherCompany.Branch.Company).OB_IsDebtor = true;
			//WIPFromAnotherCompany.RelatedJobCharge.JR_OH_SellAccount = ObjectCreator.ZECTRA.PK;
			WIPFromAnotherCompany.AL_OH = ObjectCreator.ZECTRA.PK;

			Factory.Save();

			FilterProvider.Organisations.Add(ObjectCreator.AALSHI);
			FilterProvider.Organisations.Add(ObjectCreator.ABIGAS);
			FilterProvider.Organisations.Add(ObjectCreator.ZECTRA);

			AssertEquals("PreCondition: FilterProvider.Organisations has 3 Organisations", 3, FilterProvider.Organisations.Count);

			WIPAccrualCollection reversedWIPs = new WIPAccrualCollection(Factory);
			reversedWIPs.Load(Filter.Filter);

			AssertNull("Batch Should Not Exist for TransactionHeader from other company", ARInvoiceFromAnotherCompany.ExportedBatchSequence);

			Assert("WIP Acc Reversed Collection should contain Accrual2", BusinessObjectIsInCollectionByPK(reversedWIPs, Accrual2Reversed));

			AssertEquals("WIPAcc Reversed Collection Count", 1, reversedWIPs.Count);
		}

		public void TestTransactionTypeFilterIncludeWIPsReversing()
		{
			FilterProvider.IncludeWIPsReversing = true;

			WIPAccrualCollection reversedWIPs = new WIPAccrualCollection(Factory);
			reversedWIPs.Load(Filter.Filter);

			AssertEquals("WIPAcc Reversed Collection Count = 1", 1, reversedWIPs.Count);
			Assert("WIPAcc Reversed Collection contains WIP2", BusinessObjectIsInCollectionByPK(reversedWIPs, WIP2Reversed));

			FakeABatch(reversedWIPs);
			FilterProvider.CurrentBatchNo = 1;

			WIPAccrualCollection wIPs2 = new WIPAccrualCollection(Factory);
			wIPs2.Load(Filter.Filter);

			AssertEquals("WIPAcc Reversed Collection Count = 1", 1, wIPs2.Count);
			AssertEquals("WIP2: Reverse Batch Number", 1, WIP2Reversed.ExportBatchSequenceReversedObject.XB_BatchNumber);
			Assert("WIPAcc Reversed Collection contains WIP2", BusinessObjectIsInCollectionByPK(wIPs2, WIP2Reversed));
		}

		public void TestTransactionTypeFilterIncludeAccrualsReversing()
		{
			FilterProvider.IncludeAccrualsReversing = true;

			WIPAccrualCollection reversedWIPs = new WIPAccrualCollection(Factory);
			reversedWIPs.Load(Filter.Filter);

			AssertEquals("WIPAcc Reversed Collection Count = 1", 1, reversedWIPs.Count);

			FakeABatch(reversedWIPs);
			FilterProvider.CurrentBatchNo = 1;

			WIPAccrualCollection wIPs2 = new WIPAccrualCollection(Factory);
			wIPs2.Load(Filter.Filter);

			AssertEquals("WIPAcc Reversed Collection Count = 1", 1, wIPs2.Count);

			AssertEquals("Accrual2: Reverse Batch Number", 1, Accrual2Reversed.ExportBatchSequenceReversedObject.XB_BatchNumber);
		}

		void FakeABatch(BusinessObjectCollection collectionOfBizObjsToFakeBatch)
		{
			foreach (BusinessObject bizObj in collectionOfBizObjsToFakeBatch)
			{
				BaseWIPAccrual wIP = (BaseWIPAccrual)bizObj;

				if (wIP.AL_ReverseDate.IsValid)
				{
					ObjectCreator.CreateGenExportBatchSequenceReverseLine(1, wIP.PK, 1);
				}
			}

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Filter = new WIPAccrualReversingTransactionExportFilter(Factory, FilterProvider);

			foreach (var organisation in new OrgHeader[] { ObjectCreator.AALSHI, ObjectCreator.LocalClient, ObjectCreator.ZECTRA })
			{
				foreach (OrgCompanyData companyData in organisation.CompanyDataCollection)
				{
					companyData.OB_IsDebtor = true;
					companyData.OB_IsCreditor = true;
				}
			}

			ObjectCreator.ZECTRA.GetCompanyDataForGlbCompany(WIPFromAnotherCompany.Branch.Company).OB_IsDebtor = true;
		}

		WIPAccrualReversingTransactionExportFilter Filter;

		protected override TransactionExportFilter GetNewExportFilter()
		{
			return new WIPAccrualReversingTransactionExportFilter(Factory, FilterProvider);
		}

		protected override int ExpectedNumberOfHighWaterMarkParams
		{
			get
			{
				return FilterProvider.AtLeastOneTypeOfWIPAccrualReversingIsSelected ? 1 : 0;
			}
		}

		protected override SchemaDateTimeColumn ExpectedSystemLastEditTimeColumn
		{
			get { return AccTransactionLinesSchema.AL_SystemLastEditTimeUtc; }
		}
	}

	public class WIPAccrualReversingTransactionExportFilterForBranchAndDepartmentTest : WIPAccrualTransactionExportFilterBaseTest
	{
		public void TestTransactionFilterForBranches()
		{
			IncludeAllTransactionTypes(true);

			#region Create 6 new branches for testing

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
			WIP1.RelatedJobCharge.JR_GB = WIP1.AL_GB;
			WIP2Reversed.AL_GB = newBranch2.PK;
			Accrual1.AL_GB = newBranch3.PK;
			Accrual2Reversed.AL_GB = newBranch4.PK;

			Factory.SuspendValidation();
			Factory.Save();
			Factory.ResumeValidation();

			FilterProvider.Branches.Add(newBranch1);
			FilterProvider.Branches.Add(newBranch2);
			FilterProvider.Branches.Add(newBranch5);

			AssertEquals("PreCondition: FilterProvider.Branches has 3 Branches", 3, FilterProvider.Branches.Count);

			WIPAccrualCollection reversedWIPs = new WIPAccrualCollection(Factory);
			reversedWIPs.Load(Filter.Filter);

			Assert("WIP Acc Posted Collection should NOT contain WIP1", !BusinessObjectIsInCollectionByPK(reversedWIPs, WIP1));
			Assert("WIP Acc Posted Collection should contain WIP2", BusinessObjectIsInCollectionByPK(reversedWIPs, WIP2Reversed));
			Assert("WIP Acc Posted Collection should NOT contain Accrual1", !BusinessObjectIsInCollectionByPK(reversedWIPs, Accrual1));
			Assert("WIP Acc Posted Collection should NOT contain Accrual2", !BusinessObjectIsInCollectionByPK(reversedWIPs, Accrual2Reversed));

			AssertEquals("WIPAcc Reversed Collection Count", 1, reversedWIPs.Count);
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

			var depFilter = new ZQuery(FilterProvider.Departments.AdditionalFilter);
			depFilter.AddToFilter(JoinCondition.Or, GlbDepartmentSchema.PK, newDepartment2.PK);
			depFilter.AddToFilter(JoinCondition.Or, GlbDepartmentSchema.PK, newDepartment4.PK);
			depFilter.AddToFilter(JoinCondition.Or, GlbDepartmentSchema.PK, newDepartment6.PK);
			FilterProvider.Departments.Add(newDepartment2);
			FilterProvider.Departments.Add(newDepartment4);
			FilterProvider.Departments.Add(newDepartment6);

			AssertEquals("PreCondition: FilterProvider.Departmentes has 3 Departments", 3, FilterProvider.Departments.Count);

			WIPAccrualCollection reversedWIPs = new WIPAccrualCollection(Factory);
			reversedWIPs.Load(Filter.Filter);

			Assert("WIP Accrual Collection should contain WIP2", BusinessObjectIsInCollectionByPK(reversedWIPs, WIP2Reversed));
			Assert("WIP Accrual Collection should contain Accrual2", BusinessObjectIsInCollectionByPK(reversedWIPs, Accrual2Reversed));
			AssertEquals("WIPAcc Reversed Collection Count", 2, reversedWIPs.Count);

			Assert("WIP Accrual Collection should not contain WIP from other company", !BusinessObjectIsInCollectionByPK(reversedWIPs, WIPFromAnotherCompany));
		}

		public override void TestGetFilterPks()
			=> Assert($"Tested in {nameof(WIPAccrualReversingTransactionExportFilterTest)} but doesn't work when IsForceSetupSave is false", true);

		protected override void SetUp()
		{
			base.SetUp();
			Filter = new WIPAccrualReversingTransactionExportFilter(Factory, FilterProvider);

			foreach (var organisation in new OrgHeader[] { ObjectCreator.AALSHI, ObjectCreator.LocalClient, ObjectCreator.ZECTRA })
			{
				foreach (OrgCompanyData companyData in organisation.CompanyDataCollection)
				{
					companyData.OB_IsDebtor = true;
					companyData.OB_IsCreditor = true;
				}
			}

			ObjectCreator.ZECTRA.GetCompanyDataForGlbCompany(WIPFromAnotherCompany.Branch.Company).OB_IsDebtor = true;
		}

		WIPAccrualReversingTransactionExportFilter Filter;

		protected override TransactionExportFilter GetNewExportFilter()
		{
			return new WIPAccrualReversingTransactionExportFilter(Factory, FilterProvider);
		}

		protected override bool IsForceSetupSave => false;

		protected override SchemaDateTimeColumn ExpectedSystemLastEditTimeColumn
		{
			get { return AccTransactionLinesSchema.AL_SystemLastEditTimeUtc; }
		}
	}
}

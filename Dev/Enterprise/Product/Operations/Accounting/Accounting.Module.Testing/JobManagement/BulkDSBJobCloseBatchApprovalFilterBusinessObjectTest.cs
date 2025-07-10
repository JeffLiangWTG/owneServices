using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(BulkDSBJobCloseBatchApprovalFilterBusinessObject))]
	public class BulkDSBJobCloseBatchApprovalFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BulkDSBJobCloseBatchApprovalFilterBusinessObject();
		}

		BulkDSBJobCloseBatchApprovalFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();
			FilterBO = (BulkDSBJobCloseBatchApprovalFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		public void TestFilterApprovalDate()
		{
			var batch1 = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			var batch2 = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			batch1.JBB_ApprovalTimeUtc = new ZDateTime(2020, 1, 1, 0, 0, 0);
			batch2.JBB_ApprovalTimeUtc = new ZDateTime(2020, 2, 2, 0, 0, 0);
			Factory.Save();

			var filter = (ModuleDateFilter)FilterBO[AccountingUtils.DsbJobCloseBatchFilterTypes.ApprovalDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2020, 1, 1);
			filter.Property2 = new ZDateTime(2020, 1, 1);
			filter.IsActive = true;
			var filterCollection = new DsbJobCloseBatchCollection(Factory, FilterBO.Filter);
			Assert(filterCollection.Contains(batch1));
			Assert(!filterCollection.Contains(batch2));

			filter = (ModuleDateFilter)FilterBO[AccountingUtils.DsbJobCloseBatchFilterTypes.ApprovalDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2020, 2, 2);
			filter.Property2 = new ZDateTime(2020, 2, 2);
			filter.IsActive = true;
			filterCollection = new DsbJobCloseBatchCollection(Factory, FilterBO.Filter);
			Assert(!filterCollection.Contains(batch1));
			Assert(filterCollection.Contains(batch2));
		}

		public void TestFilterApprovingUser()
		{
			var staff1 = TestObjectCreator.CreateStaff("TS1");
			var staff2 = TestObjectCreator.CreateStaff("TS2");
			var batch1 = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			var batch2 = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			batch1.JBB_GS_NKApprovingUser = staff1.GS_Code;
			batch2.JBB_GS_NKApprovingUser = staff2.GS_Code;
			Factory.Save();

			var filter = (ModuleNkFilter)FilterBO[AccountingUtils.DsbJobCloseBatchFilterTypes.ApprovingUser];
			filter.Property = "TS1";
			filter.IsActive = true;
			var filterCollection = new DsbJobCloseBatchCollection(Factory, FilterBO.Filter);
			Assert(filterCollection.Contains(batch1));
			Assert(!filterCollection.Contains(batch2));

			filter = (ModuleNkFilter)FilterBO[AccountingUtils.DsbJobCloseBatchFilterTypes.ApprovingUser];
			filter.Property = "TS2";
			filter.IsActive = true;
			filterCollection = new DsbJobCloseBatchCollection(Factory, FilterBO.Filter);
			Assert(!filterCollection.Contains(batch1));
			Assert(filterCollection.Contains(batch2));
		}

		public void TestFilterBatchNumber()
		{
			var batch1 = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			var batch2 = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			batch1.JBB_BatchNumber = "B001";
			batch2.JBB_BatchNumber = "B002";
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[AccountingUtils.DsbJobCloseBatchFilterTypes.BatchNumber];
			filter.Property = "B001";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			var filterCollection = new DsbJobCloseBatchCollection(Factory, FilterBO.Filter);
			Assert(filterCollection.Contains(batch1));
			Assert(!filterCollection.Contains(batch2));

			filter = (ModuleTextFilter)FilterBO[AccountingUtils.DsbJobCloseBatchFilterTypes.BatchNumber];
			filter.Property = "B002";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filterCollection = new DsbJobCloseBatchCollection(Factory, FilterBO.Filter);
			Assert(!filterCollection.Contains(batch1));
			Assert(filterCollection.Contains(batch2));

			filter = (ModuleTextFilter)FilterBO[AccountingUtils.DsbJobCloseBatchFilterTypes.BatchNumber];
			filter.Property = "B001, B002";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filterCollection = new DsbJobCloseBatchCollection(Factory, FilterBO.Filter);
			Assert(filterCollection.Contains(batch1));
			Assert(filterCollection.Contains(batch2));
		}

		public void TestFilterApprovalStatus()
		{
			var batch1 = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			var batch2 = Factory.NewWithValidTestData<DsbJobCloseBatch>();
			batch1.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Approve;
			batch2.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.Open;
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBO[AccountingUtils.DsbJobCloseBatchFilterTypes.ApprovalStatus];
			filter.Property = AccountingConstants.DsbJobBatchStatus.Approve;
			filter.IsActive = true;
			var filterCollection = new DsbJobCloseBatchCollection(Factory, FilterBO.Filter);
			Assert(filterCollection.Contains(batch1));
			Assert(!filterCollection.Contains(batch2));

			filter = (ModuleTextFilter)FilterBO[AccountingUtils.DsbJobCloseBatchFilterTypes.ApprovalStatus];
			filter.Property = AccountingConstants.DsbJobBatchStatus.Open;
			filter.IsActive = true;
			filterCollection = new DsbJobCloseBatchCollection(Factory, FilterBO.Filter);
			Assert(!filterCollection.Contains(batch1));
			Assert(filterCollection.Contains(batch2));
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;
	}
}

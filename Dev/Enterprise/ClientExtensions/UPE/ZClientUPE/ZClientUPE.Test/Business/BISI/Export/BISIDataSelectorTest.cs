using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class BISIDataSelectorTest : TestCaseWithFactory
	{
		public void TestTimeStampFilter()
		{
			var selector = new BISIDataSelectorForTest(Factory, StartDate, StartDate.AddSeconds(9));
			var collection = selector.GetLogs();
			AssertEquals(9, collection.Count);
			for (int i = 0; i < 9; i++)
			{
				AssertEquals(i.ToString(), collection[i].SL_Reference);
			}
		}

		public void TestTimeStampFilter_UpperBoundIsNotIncluded()
		{
			var selector = new BISIDataSelectorForTest(Factory, StartDate, StartDate.AddSeconds(8));
			var collection = selector.GetLogs();
			AssertEquals("Upper Bound should not be included", 8, collection.Count);
			for (int i = 0; i < 8; i++)
			{
				AssertEquals(i.ToString(), collection[i].SL_Reference);
			}
		}

		public void TestTimeStampFilter_NotWithinDateRange()
		{
			var selector = new BISIDataSelectorForTest(Factory, StartDate.AddSeconds(9), StartDate.AddSeconds(40));
			var collection = selector.GetLogs();
			AssertEquals(0, collection.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			StartDate = new ZDateTimeOffset(2004, 1, 1, 10, 10, 00);
			InsertStmALog("0", StartDate);
			InsertStmALog("1", StartDate.AddSeconds(1));
			InsertStmALog("2", StartDate.AddSeconds(2));
			InsertStmALog("3", StartDate.AddSeconds(3));
			InsertStmALog("4", StartDate.AddSeconds(4));
			InsertStmALog("5", StartDate.AddSeconds(5));
			InsertStmALog("6", StartDate.AddSeconds(6));
			InsertStmALog("7", StartDate.AddSeconds(7));
			InsertStmALog("8", StartDate.AddSeconds(8));
		}

		void InsertStmALog(ZString reference, ZDateTimeOffset eventTimeStamp)
		{
			var parent = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			parent.GetLogs().AddNew(Events.AddedARecordToTheSystem, reference, eventTimeStamp);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
		}

		ZDateTimeOffset StartDate;
		#region BISIDataSelectorForTest
		class BISIDataSelectorForTest : BISIDataSelector
		{
			public BISIDataSelectorForTest(BusinessObjectFactory factory, ZDateTimeOffset startDate, ZDateTimeOffset endDate) : base(factory, startDate.ToZDateTime(), endDate.ToZDateTime())
			{
			}

			public StmALogCollection GetLogs()
			{
				var collection = new StmALogCollection(Factory, TimeStampFilter);
				collection.Load();
				collection.Sort(StmALogSchema.Constants.SL_Reference, ListSortDirection.Ascending);
				return collection;
			}
		}
		#endregion
	}
}

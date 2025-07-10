using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public class ABLEntryNumFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForDeleteCore()
		{
			FetchStrategyTestHelper.AssertFetchForDelete<ABLEntryNum>(GetType(), Factory, TestFetchForDeleteCoreTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
		}

		protected virtual string[] GetTablesToCollectQueriesFor() => null;

		protected virtual Dictionary<string, int> FetchForDeleteUnconsumedExpectedHitCounts => new Dictionary<string, int>
		{
			{ GenAddOnColumnSchema.Constants.TableName, 1 },
			{ GenCustomAddOnValueSchema.Constants.TableName, 1 }
		};

		protected virtual Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts => new Dictionary<string, int> { };

		protected virtual ZString Message => "ASYCUDA Bill Entry Num (ABLEntryNum)";

		protected virtual Type AsycudaManifestHeaderTypeForTest => ObjectFactory.GetType<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();

		protected ABLEntryNum ablEntryNumber;

		protected override void SetUp()
		{
			base.SetUp();
			var header1 = (AsycudaManifestHeader)Factory.NewWithValidTestData(AsycudaManifestHeaderTypeForTest);
			var bill = header1.Bills.AddNew();
			ablEntryNumber = bill.CustomsEntryNumbers.AddNew();
			ablEntryNumber.CE_EntryType = "REG";
			ablEntryNumber.CE_EntryNum = "REGO2";
			Factory.Save();
		}

		void TestFetchForDeleteCoreTestCases(out IList<FetchStrategyTestHelper.TestCase<ABLEntryNum>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<ABLEntryNum>>
			{
				new FetchStrategyTestHelper.TestCase<ABLEntryNum>
				{
					Message = Message,
					BusinessObject = ablEntryNumber,
					ExecuteActionExpectedHitCounts = FetchForDeleteExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts  = FetchForDeleteUnconsumedExpectedHitCounts
				}
			};
		}
	}
}

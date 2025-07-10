using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public class AsycudaPackedItemEntryNumFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForDeleteCore()
		{
			FetchStrategyTestHelper.AssertFetchForDelete<AsycudaPackedItemEntryNum>(GetType(), Factory, TestFetchForDeleteCoreTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
		}

		protected virtual string[] GetTablesToCollectQueriesFor() => null;

		protected virtual Dictionary<string, int> FetchForDeleteUnconsumedExpectedHitCounts => new Dictionary<string, int>()
		{
			{ GenAddOnColumnSchema.Constants.TableName, 1 },
			{ GenCustomAddOnValueSchema.Constants.TableName, 1 },
			{ UNDGDataItemSchema.Constants.TableName, 1 }
		};

		protected virtual Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts => new Dictionary<string, int> { };

		protected virtual ZString Message => "ASYCUDA Packed Item Entry Num (AsycudaPackedItemEntryNum)";

		protected virtual Type AsycudaManifestHeaderTypeForTest => ObjectFactory.GetType<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();

		protected override void SetUp()
		{
			base.SetUp();
			var header1 = (AsycudaManifestHeader)Factory.NewWithValidTestData(AsycudaManifestHeaderTypeForTest);
			var cont1 = header1.Containers.AddNew();
			var bill = header1.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont1.PK;
			var packedItem = pack.PackedItem;
			packedItemEntryNumber = packedItem.CustomsEntryNumbers.AddNew();
			packedItemEntryNumber.CE_EntryType = "REG";
			packedItemEntryNumber.CE_EntryNum = "REGO2";
			Factory.Save();
		}

		protected AsycudaPackedItemEntryNum packedItemEntryNumber;

		void TestFetchForDeleteCoreTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaPackedItemEntryNum>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaPackedItemEntryNum>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaPackedItemEntryNum>
				{
					Message = Message,
					BusinessObject = packedItemEntryNumber,
					ExecuteActionExpectedHitCounts = FetchForDeleteExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts = FetchForDeleteUnconsumedExpectedHitCounts
				}
			};
		}
	}
}

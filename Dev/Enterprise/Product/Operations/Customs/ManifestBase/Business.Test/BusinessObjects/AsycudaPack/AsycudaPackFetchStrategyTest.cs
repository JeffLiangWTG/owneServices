using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase.Testing
{
	public class AsycudaPackFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForDeleteCore()
		{
			FetchStrategyTestHelper.AssertFetchForDelete<AsycudaPack>(GetType(), Factory, SetupFetchForDeleteCoreTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
		}

		protected virtual string[] GetTablesToCollectQueriesFor() => null;

		void SetupFetchForDeleteCoreTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaPack>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaPack>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaPack>
				{
					Message = Message,
					BusinessObject = pack,
					FetchStrategyExpectedHitCounts = FetchForDeleteFetchStrategyExpectedHitCounts,
					ExecuteActionExpectedHitCounts = FetchForDeleteExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts = FetchForDeleteUnconsumedExpectedHitCounts
				}
			};
		}

		protected virtual Dictionary<string, int> FetchForDeleteFetchStrategyExpectedHitCounts => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts =>
			new Dictionary<string, int>
			{
				{ StmDocDataOverrideSchema.Constants.TableName, 2 },
				{ StmUniversalCopySchema.Constants.TableName, 2 }
			};

		protected virtual Dictionary<string, int> FetchForDeleteUnconsumedExpectedHitCounts => new Dictionary<string, int>();

		public void TestFetchForLoadChildEditableObjectsCore()
		{
			ReleaseFactory();
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				FetchStrategyTestHelper.AssertFetchForLoadChildEditableObjects<AsycudaPack>(GetType(), Factory, SetupFetchForLoadChildEditableObjectsCoreTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		void SetupFetchForLoadChildEditableObjectsCoreTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaPack>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaPack>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaPack>
				{
					Message = Message,
					BusinessObject = pack,
					FetchStrategyExpectedHitCounts = FetchForLoadChildEditableObjectsFetchStrategyExpectedHitCounts,
					ExecuteActionExpectedHitCounts = FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts = FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts
				}
			};
		}

		protected virtual Dictionary<string, int> FetchForLoadChildEditableObjectsFetchStrategyExpectedHitCounts => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts => new Dictionary<string, int>();

		public void TestFetchForValidateCore()
		{
			FetchStrategyTestHelper.AssertFetchForValidate<AsycudaPack>(GetType(), Factory, SetupFetchForValidateTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
		}

		void SetupFetchForValidateTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaPack>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaPack>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaPack>
				{
					Message = Message,
					BusinessObject = pack,
					FetchStrategyExpectedHitCounts = FetchForValidateFetchStrategyExpectedHitCounts,
					ExecuteActionExpectedHitCounts = FetchForValidateExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts = FetchForValidateUnconsumedExpectedHitCounts
				}
			};
		}

		protected virtual Dictionary<string, int> FetchForValidateFetchStrategyExpectedHitCounts => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> FetchForValidateUnconsumedExpectedHitCounts => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> FetchForValidateExecuteActionExpectedHitCounts => new Dictionary<string, int>();

		protected virtual ZString Message => "ManifestBase Pack (AsycudaPack)";

		protected virtual string ApplicationCode => ApplicationCodeTypeList.Constants.ManifestBase;

		protected virtual Type AsycudaManifestHeaderTypeForTest => typeof(AsycudaManifestHeader);

		protected override void SetUp()
		{
			base.SetUp();
			var header = (AsycudaManifestHeader)Factory.NewWithValidTestData(AsycudaManifestHeaderTypeForTest);
			header.AMA_ApplicationCode = ApplicationCode;
			var bill = header.Bills.AddNew();
			var container1 = header.Containers.AddNew();
			var container2 = header.Containers.AddNew();
			pack = bill.Packs.AddNew();
			_ = pack.IsOnePackedItemRelationship ? pack.PackedItem : pack.PackedItems.AddNewPackedItem();
			_ = pack.PackedItems.AddNewPackedItem();
			pack.ContainerPK = container1.PK;
			var packContainerLink2 = Factory.New<AsycudaContainerBillOrPackageLink>();
			packContainerLink2.APC_APA_Pack = pack.PK;
			packContainerLink2.APC_ACN_Container = container2.PK;
			Factory.Save();
		}
		protected AsycudaPack pack;
	}
}

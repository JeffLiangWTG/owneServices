using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase.Testing
{
	public class AsycudaBillFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForDeleteCore()
		{
			FetchStrategyTestHelper.AssertFetchForDelete<AsycudaBill>(GetType(), Factory, TestFetchForDeleteCoreTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
		}

		void TestFetchForDeleteCoreTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaBill>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaBill>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaBill>
				{
					Message = Message,
					BusinessObject = bill,
					FetchStrategyExpectedHitCounts = FetchForDeleteFetchStrategyExpectedHitCounts,
					ExecuteActionExpectedHitCounts = FetchForDeleteExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts = FetchForDeleteUnconsumedExpectedHitCounts
				}
			};
		}

		protected virtual Dictionary<string, int> FetchForDeleteFetchStrategyExpectedHitCounts =>
			new Dictionary<string, int>
			{
				{ AsycudaPackPackedItemPivotSchema.Constants.TableName, 2 },
			};

		protected virtual Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts =>
			new Dictionary<string, int>
			{
				{ StmDocDataOverrideSchema.Constants.TableName, 2 },
				{ StmUniversalCopySchema.Constants.TableName, 3 },
			};

		protected virtual Dictionary<string, int> FetchForDeleteUnconsumedExpectedHitCounts =>
			new Dictionary<string, int>
			{
			};

		public void TestFetchForLoadChildEditableObjectsCore()
		{
			ReleaseFactory();
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				FetchStrategyTestHelper.AssertFetchForLoadChildEditableObjects<AsycudaBill>(GetType(), Factory, SetupFetchForLoadChildEditableObjectsCoreTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		void SetupFetchForLoadChildEditableObjectsCoreTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaBill>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaBill>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaBill>
				{
					Message = Message,
					BusinessObject = bill,
					ExecuteActionExpectedHitCounts = FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts = FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts
				}
			};
		}

		protected virtual Dictionary<string, int> FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts => new Dictionary<string, int>()
		{
			{ AsycudaPackedItemSchema.Constants.TableName, 1 }
		};

		protected virtual Dictionary<string, int> FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts => new Dictionary<string, int>();

		protected virtual string[] GetTablesToCollectQueriesFor() => null;

		public void TestFetchForValidateCore()
		{
			FetchStrategyTestHelper.AssertFetchForValidate<AsycudaBill>(GetType(), Factory, SetupFetchForValidateTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
		}

		void SetupFetchForValidateTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaBill>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaBill>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaBill>
				{
					Message = Message,
					BusinessObject = bill,
					ExecuteActionExpectedHitCounts = FetchForValidateExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts = FetchForValidateUnconsumedExpectedHitCounts
				}
			};
		}

		protected virtual Dictionary<string, int> FetchForValidateExecuteActionExpectedHitCounts => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> FetchForValidateUnconsumedExpectedHitCounts => new Dictionary<string, int>()
		{
			{ AsycudaPackedItemSchema.Constants.TableName, 1 },
		};

		protected virtual ZString Message => "ManifestBase Bill (AsycudaBill)";

		protected virtual string ApplicationCode => ApplicationCodeTypeList.Constants.ManifestBase;

		protected virtual Type AsycudaManifestHeaderTypeForTest => typeof(AsycudaManifestHeader);

		protected override void SetUp()
		{
			base.SetUp();
			var header = (AsycudaManifestHeader)Factory.NewWithValidTestData(AsycudaManifestHeaderTypeForTest);
			header.AMA_ApplicationCode = ApplicationCode;
			var container1PK = header.Containers.AddNew().PK;
			var container2PK = header.Containers.AddNew().PK;
			bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			pack1.ContainerPK = container1PK;
			_ = pack1.IsOnePackedItemRelationship ? pack1.PackedItem : pack1.PackedItems.AddNewPackedItem();
			_ = pack1.PackedItems.AddNewPackedItem();
			var pack2 = bill.Packs.AddNew();
			pack2.ContainerPK = container2PK;
			_ = pack2.IsOnePackedItemRelationship ? pack2.PackedItem : pack2.PackedItems.AddNewPackedItem();
			_ = pack2.PackedItems.AddNewPackedItem();
			Factory.Save();
			if (bill is IAsycudaTaxTypeSupporter taxTypeSupporter)
			{
				var clusterKey = header.AMA_ClusterKey;
				var asycudaTaxType = taxTypeSupporter.GetAsycudaTaxType();
				var tax1 = (AsycudaTax)Factory.New(asycudaTaxType);
				tax1.AET_ABL = bill.PK;
				tax1.AET_ClusterKey = clusterKey;
				tax1.AET_MethodOfCalculation = "%";
				var tax2 = (AsycudaTax)Factory.New(asycudaTaxType);
				tax2.AET_ABL = bill.PK;
				tax2.AET_ClusterKey = clusterKey;
				tax2.AET_MethodOfCalculation = "%";
			}
		}
		protected AsycudaBill bill;
	}
}

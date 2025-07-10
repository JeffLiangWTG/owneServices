using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public class AsycudaPackedItemFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForDeleteCore()
		{
			FetchStrategyTestHelper.AssertFetchForDelete<AsycudaPackedItem>(GetType(), Factory, SetupFetchForDeleteCoreTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
		}

		public void TestFetchForLoadChildEditableObjectsCore()
		{
			ReleaseFactory();
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				FetchStrategyTestHelper.AssertFetchForLoadChildEditableObjects<AsycudaPackedItem>(GetType(), Factory, SetupFetchForLoadChildEditableObjectsCoreTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		public void TestFetchForValidateCore()
		{
			FetchStrategyTestHelper.AssertFetchForValidate<AsycudaPackedItem>(GetType(), Factory, SetupFetchForValidateTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
		}

		protected virtual string[] GetTablesToCollectQueriesFor() => null;

		protected virtual Dictionary<string, int> FetchForDeleteUnconsumedExpectedHitCounts => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts => new Dictionary<string, int>
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 4 },
			{ StmUniversalCopySchema.Constants.TableName, 4 },
			{ StmNoteSchema.Constants.TableName, 2 }
		};

		protected virtual Dictionary<string, int> FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> FetchForValidateExecuteActionExpectedHitCounts => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> FetchForValidateUnconsumedExpectedHitCounts => new Dictionary<string, int>
		{
			{ GenAddOnColumnSchema.Constants.TableName, 1 },
			{ GenCustomAddOnValueSchema.Constants.TableName, 1 },
			{ UNDGDataItemSchema.Constants.TableName, 1 }
		};

		protected virtual ZString Message => "ASYCUDA Packed Item (AsycudaPackedItem)";

		protected virtual Type AsycudaManifestHeaderTypeForTest => ObjectFactory.GetType<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();

		protected override void SetUp()
		{
			base.SetUp();
			var header1 = (AsycudaManifestHeader)Factory.NewWithValidTestData(AsycudaManifestHeaderTypeForTest);
			var cont1 = header1.Containers.AddNew();
			var bill = header1.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont1.PK;
			packedItem = pack.PackedItem;
			var packedItemRegistrationNumber = packedItem.CustomsEntryNumbers.AddNew();
			packedItemRegistrationNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			packedItemRegistrationNumber.CE_EntryNum = "REGO1";
			var packedItemEntryNumber = packedItem.CustomsEntryNumbers.AddNew();
			packedItemRegistrationNumber.CE_EntryType = "REG";
			packedItemRegistrationNumber.CE_EntryNum = "REGO2";
			Factory.Save();
		}

		protected AsycudaPackedItem packedItem;

		void SetupFetchForDeleteCoreTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaPackedItem>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaPackedItem>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaPackedItem>
				{
					Message = Message,
					BusinessObject = packedItem,
					ExecuteActionExpectedHitCounts = FetchForDeleteExecuteActionExpectedHitCounts
				}
			};
		}

		void SetupFetchForLoadChildEditableObjectsCoreTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaPackedItem>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaPackedItem>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaPackedItem>
				{
					Message = Message,
					BusinessObject = packedItem,
					ExecuteActionExpectedHitCounts = FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts
				}
			};
		}

		void SetupFetchForValidateTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaPackedItem>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaPackedItem>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaPackedItem>
				{
					Message = Message,
					BusinessObject = packedItem,
					ExecuteActionExpectedHitCounts = FetchForValidateExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts = FetchForValidateUnconsumedExpectedHitCounts
				}
			};
		}
	}
}

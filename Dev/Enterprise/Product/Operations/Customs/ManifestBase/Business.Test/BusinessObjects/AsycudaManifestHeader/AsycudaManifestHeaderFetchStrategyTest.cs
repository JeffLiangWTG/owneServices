using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Common.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase.Testing
{
	public class AsycudaManifestHeaderFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForDeleteCore()
		{
			FetchStrategyTestHelper.AssertFetchForDelete<AsycudaManifestHeader>(GetType(), Factory, TestFetchForDeleteCoreTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
		}

		protected virtual string[] GetTablesToCollectQueriesFor() => null;

		void TestFetchForDeleteCoreTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>
				{
					Message = Message,
					BusinessObject = header,
					FetchStrategyExpectedHitCounts = FetchForDeleteFetchStrategyExpectedHitCounts,
					ExecuteActionExpectedHitCounts = FetchForDeleteExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts = FetchForDeleteUnconsumedExpectedHitCounts
				}
			};
		}

		protected virtual Dictionary<string, int> FetchForDeleteFetchStrategyExpectedHitCounts => new Dictionary<string, int>
		{
			{ AsycudaContainerBillOrPackageLinkSchema.Constants.TableName, 2 },
			{ AsycudaPackPackedItemPivotSchema.Constants.TableName, 1 },
		};

		protected virtual Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts => new Dictionary<string, int>
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 2 },
			{ StmUniversalCopySchema.Constants.TableName, 5 },
		};

		protected virtual Dictionary<string, int> FetchForDeleteUnconsumedExpectedHitCounts => new Dictionary<string, int>()
		{
			{ AsycudaBillScreeningSchema.Constants.TableName, 1 },
			{ AsycudaTaxSchema.Constants.TableName, 1 }
		};

		public void TestFetchForLoadChildEditableObjectsCore()
		{
			ReleaseFactory();
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				FetchStrategyTestHelper.AssertFetchForLoadChildEditableObjects<AsycudaManifestHeader>(GetType(), Factory, SetupFetchForLoadChildEditableObjectsCoreTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		void SetupFetchForLoadChildEditableObjectsCoreTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>
				{
					Message = Message,
					BusinessObject = header,
					FetchStrategyExpectedHitCounts = FetchForLoadChildEditableObjectsFetchStrategyExpectedHitCounts,
					ExecuteActionExpectedHitCounts = FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts = FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts
				}
			};
		}

		protected virtual Dictionary<string, int> FetchForLoadChildEditableObjectsFetchStrategyExpectedHitCounts => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> FetchForLoadChildEditableObjectsExecuteActionExpectedHitCounts => new Dictionary<string, int>()
		{
			{ AsycudaPackedItemSchema.Constants.TableName,	1 }
		};

		protected virtual Dictionary<string, int> FetchForLoadChildEditableObjectsUnconsumedExpectedHitCounts => new Dictionary<string, int>()
		{
			{ AsycudaBillScreeningSchema.Constants.TableName, 1 },
			{ AsycudaPackPackedItemPivotSchema.Constants.TableName, 1 },
			{ AsycudaTaxSchema.Constants.TableName, 1 }
		};

		public void TestFetchForValidateCore()
		{
			FetchStrategyTestHelper.AssertFetchForValidate<AsycudaManifestHeader>(GetType(), Factory, SetupFetchForValidateTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
		}

		void SetupFetchForValidateTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaManifestHeader>
				{
					Message = Message,
					BusinessObject = header,
					FetchStrategyExpectedHitCounts = FetchForValidateFetchStrategyExpectedHitCounts,
					ExecuteActionExpectedHitCounts = FetchForValidateExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts = FetchForValidateUnconsumedExpectedHitCounts
				}
			};
		}

		protected virtual Dictionary<string, int> FetchForValidateFetchStrategyExpectedHitCounts => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> FetchForValidateExecuteActionExpectedHitCounts => new Dictionary<string, int>();

		protected virtual Dictionary<string, int> FetchForValidateUnconsumedExpectedHitCounts => new Dictionary<string, int>()
		{
			{ AsycudaBillScreeningSchema.Constants.TableName, 1 },
			{ AsycudaTaxSchema.Constants.TableName, 1 },
			{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
			{ GlbCompanySchema.Constants.TableName, 1 },
			{ AsycudaPackedItemSchema.Constants.TableName, 1 },
			{ AsycudaPackPackedItemPivotSchema.Constants.TableName, 1 }
		};

		protected virtual ZString Message => "ManifestBase Header (AsycudaManifestHeader)";

		protected virtual string ApplicationCode => ApplicationCodeTypeList.Constants.ManifestBase;

		protected virtual Type AsycudaManifestHeaderTypeForTest => typeof(AsycudaManifestHeader);

		protected override void SetUp()
		{
			base.SetUp();
			header = (AsycudaManifestHeader)Factory.NewWithValidTestData(AsycudaManifestHeaderTypeForTest);
			header.AMA_ApplicationCode = ApplicationCode;
			var container1 = header.Containers.AddNew();
			var container2 = header.Containers.AddNew();
			var container3 = header.Containers.AddNew();

			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmTestHelper.EnableBMSInRegistry();
			bmTestHelper.CreateSystem(Factory, WorkflowDescriptors.AsycudaManifestWorkflowDescriptorCode);

			SetupBillAndChildren(header, container1, container2);
			SetupBillAndChildren(header, container2, container3);
			Factory.Save();
		}

		void SetupBillAndChildren(AsycudaManifestHeader header, AsycudaContainer container1, AsycudaContainer container2)
		{
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			_ = pack1.IsOnePackedItemRelationship ? pack1.PackedItem : pack1.PackedItems.AddNewPackedItem();
			_ = pack1.PackedItems.AddNewPackedItem();
			var pack2 = bill.Packs.AddNew();
			_ = pack2.IsOnePackedItemRelationship ? pack2.PackedItem : pack2.PackedItems.AddNewPackedItem();
			_ = pack2.PackedItems.AddNewPackedItem();
			var pack3 = bill.Packs.AddNew();
			pack3.ContainerPK = container1.PK;
			var pack4 = bill.Packs.AddNew();
			pack4.ContainerPK = container2.PK;
			if (bill is IAsycudaTaxTypeSupporter taxTypeSupporter)
			{
				var factory = bill.Factory;
				var asycudaTaxType = taxTypeSupporter.GetAsycudaTaxType();
				var clusterKey = bill.ABL_ClusterKey;
				var tax1 = (AsycudaTax)factory.New(asycudaTaxType);
				tax1.AET_ClusterKey = clusterKey;
				tax1.AET_ABL = bill.PK;
				tax1.AET_MethodOfCalculation = "%";
				var tax2 = (AsycudaTax)factory.New(asycudaTaxType);
				tax2.AET_ClusterKey = clusterKey;
				tax2.AET_ABL = bill.PK;
				tax2.AET_MethodOfCalculation = "%";
			}
		}
		protected AsycudaManifestHeader header;
	}
}

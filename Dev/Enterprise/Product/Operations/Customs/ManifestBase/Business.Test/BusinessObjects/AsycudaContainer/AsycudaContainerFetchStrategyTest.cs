using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase.Testing
{
	public class AsycudaContainerFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForDeleteCore()
		{
			FetchStrategyTestHelper.AssertFetchForDelete<AsycudaContainer>(GetType(), Factory, TestFetchForDeleteCoreTestCases, tablesToCollectQueriesFor: GetTablesToCollectQueriesFor());
		}

		protected virtual string[] GetTablesToCollectQueriesFor() => null;
		void TestFetchForDeleteCoreTestCases(out IList<FetchStrategyTestHelper.TestCase<AsycudaContainer>> testCases)
		{
			testCases = new List<FetchStrategyTestHelper.TestCase<AsycudaContainer>>
			{
				new FetchStrategyTestHelper.TestCase<AsycudaContainer>
				{
					Message = Message,
					BusinessObject = container,
					ExecuteActionExpectedHitCounts = FetchForDeleteExecuteActionExpectedHitCounts,
					UnconsumedExpectedHitCounts = FetchForDeleteUnconsumedExpectedHitCounts
				}
			};
		}

		protected virtual Dictionary<string, int> FetchForDeleteExecuteActionExpectedHitCounts => new Dictionary<string, int>
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 6 },
			{ StmUniversalCopySchema.Constants.TableName, 4 }
		};

		protected virtual Dictionary<string, int> FetchForDeleteUnconsumedExpectedHitCounts => new Dictionary<string, int>();

		protected virtual ZString Message => "ManifestBase Container (AsycudaContainer)";

		protected virtual string ApplicationCode => ApplicationCodeTypeList.Constants.ManifestBase;

		protected virtual Type AsycudaManifestHeaderTypeForTest => typeof(AsycudaManifestHeader);

		protected override void SetUp()
		{
			base.SetUp();
			var header1 = (AsycudaManifestHeader)Factory.NewWithValidTestData(AsycudaManifestHeaderTypeForTest);
			header1.AMA_ApplicationCode = ApplicationCode;
			var bill1 = header1.Bills.AddNew();
			container = header1.Containers.AddNew();
			var pack = bill1.Packs.AddNew();
			pack.ContainerPK = container.PK;
			var pack2 = bill1.Packs.AddNew();
			pack2.ContainerPK = container.PK;
			var pack3 = bill1.Packs.AddNew();
			pack3.ContainerPK = container.PK;
			Factory.Save();
		}
		protected AsycudaContainer container;
	}
}

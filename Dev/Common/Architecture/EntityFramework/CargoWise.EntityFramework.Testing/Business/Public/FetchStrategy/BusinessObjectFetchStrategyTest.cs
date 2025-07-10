using System.Linq;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoadWithIAddInfoChildSupporter()
		{
			try
			{
				RowFactory.LoadedFetchHintRecordingEnabled = true;
				var bizObj = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				newFactory.Load<DummyBizObjWithAddInfoChildSupporter>(bizObj.PK);
				newFactory.ClearLoadedFetchHintCountForTable(DummyBizoSchema.Constants.TableName);
				AssertEquals(1, newFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
				using (newFactory.EnableTableHitQueryCollection(new[] { DummyBizoSchema.Constants.TableName }))
				{
					newFactory.ExecuteAllFetchHints();
					var tableHitCount = ((IBusinessObjectFactoryInternals)newFactory).RowFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName);
					var queries = tableHitCount.Queries.ToArray();
					AssertEquals(1, queries.Length);
					AssertContains($"WHERE Z0_Guid = CONVERT('{bizObj.PK.ToString()}', 'System.Guid')", queries[0].Query);
				}
			}
			finally
			{
				RowFactory.LoadedFetchHintRecordingEnabled = false;
			}
		}

		public void TestFetchForLoadChildEditableObjectsWithIAddInfoChildSupporter()
		{
			var bizObj = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			var strategy = (BusinessObjectFetchStrategyForTest)bizObj.AddInfoChild.FetchStrategy;
			AssertEquals(0, strategy.FetchForLoadChildEditableObjectsCoreCount);
			bizObj.FetchStrategy.FetchForLoadChildEditableObjects();
			AssertEquals(1, strategy.FetchForLoadChildEditableObjectsCoreCount);
		}

		public void TestFetchForValidateWithIAddInfoChildSupporter()
		{
			var bizObj = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			var strategy = (BusinessObjectFetchStrategyForTest)bizObj.AddInfoChild.FetchStrategy;
			AssertEquals(0, strategy.FetchForValidateCoreCount);
			bizObj.FetchStrategy.FetchForValidate();
			AssertEquals(1, strategy.FetchForValidateCoreCount);
		}

		public void TestFetchForDeleteWithIAddInfoChildSupporter()
		{
			var bizObj = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			var strategy = (BusinessObjectFetchStrategyForTest)bizObj.AddInfoChild.FetchStrategy;
			AssertEquals(0, strategy.FetchForDeleteCoreCount);
			bizObj.FetchStrategy.FetchForDelete();
			AssertEquals(1, strategy.FetchForDeleteCoreCount);
		}

		public void TestFetchForViewWithIAddInfoChildSupporter()
		{
			var bizObj = Factory.New<DummyBizObjWithAddInfoChildSupporter>();
			var strategy = (BusinessObjectFetchStrategyForTest)bizObj.AddInfoChild.FetchStrategy;
			AssertEquals(0, strategy.FetchForViewCoreCount);
			var tableColumns = new TableColumn[]
			{
				new TableColumn(DummyBizoSchema.Constants.TableName, DummyDependentBizoSchema.ZD1_Code.Name)
			};
			bizObj.FetchStrategy.FetchForView(tableColumns);
			AssertEquals(1, strategy.FetchForViewCoreCount);
		}
	}
}

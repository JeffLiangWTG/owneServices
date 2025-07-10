using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class VisualizerNoteDeleterTest : TestCaseWithFactory
	{
		public void TestFetchHintQueryDoesNotAddMultipleTimesAndQueryResultInAnInStatement()
		{
			var bo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			bo1.Z0_Description = "JOE";
			var data1 = Factory.NewWithValidTestData<VisualizerNote>();
			data1.DD_ParentID = bo1.PK;
			data1.DD_ParentTableCode = bo1.TablePrefix;
			data1.DD_ParentRelatedID = bo1.PK;
			data1.DD_DocumentData = ZBlob.FromAscii("abc");
			var bo2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			bo2.Z0_Description = "BOB";
			var data2 = Factory.NewWithValidTestData<VisualizerNote>();
			data2.DD_ParentID = bo2.PK;
			data2.DD_ParentTableCode = bo2.TablePrefix;
			data2.DD_ParentRelatedID = bo2.PK;
			data2.DD_DocumentData = ZBlob.FromAscii("def");
			var data3 = Factory.NewWithValidTestData<VisualizerNote>();
			data3.DD_ParentID = bo1.PK;
			data3.DD_ParentTableCode = bo2.TablePrefix;
			data3.DD_ParentRelatedID = bo2.PK;
			data3.DD_DocumentData = ZBlob.FromAscii("ghi");

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			using (newFactory.EnableTableHitQueryCollection(new[] { StmDocDataOverrideSchema.Constants.TableName }))
			{
				var rowFactory = ((IBusinessObjectFactoryInternals)newFactory).RowFactory;
				var bo1InOtherFactory = newFactory.Load<DummyBusinessObject>(bo1.PK);
				var bo2InOtherFactory = newFactory.Load<DummyBusinessObject>(bo2.PK);

				for (var i = 0; i < 3; i++)
				{
					bo1InOtherFactory.FetchStrategy.FetchForDelete();
					bo2InOtherFactory.FetchStrategy.FetchForDelete();
				}
				rowFactory.ExecuteFetchHintsForTable(StmDocDataOverrideSchema.Constants.TableName);
				var tableSelect = newFactory.TableSelects.First(x => x.TableName == StmDocDataOverrideSchema.Constants.TableName);
				AssertEquals(2, tableSelect.Value);
				var queries = tableSelect.Queries.ToArray();
				AssertEquals(2, queries.Length);
				_ = queries.First(x => x.Query.Contains("WHERE (DD_ParentRelatedID in (CONVERT("));
				_ = queries.First(x => x.Query.Contains("WHERE (DD_ParentID in (CONVERT("));
			}
		}

		public void TestVisualizerNoteDeleter()
		{
			var newFactory = new BusinessObjectFactory();

			var dummyBusinessObject = newFactory.NewWithValidTestData<DummyBusinessObject>();
			var stmOverrideDocData = newFactory.NewWithValidTestData<VisualizerNote>();
			stmOverrideDocData.DD_ParentID = dummyBusinessObject.PK;
			stmOverrideDocData.DD_ParentTableCode = dummyBusinessObject.TablePrefix;
			stmOverrideDocData.DD_ParentRelatedID = dummyBusinessObject.PK;
			stmOverrideDocData.DD_DocumentData = ZBlob.FromAscii("abc");

			var dummy1 = newFactory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = newFactory.NewWithValidTestData<DummyBusinessObject>();
			var dummy3 = newFactory.NewWithValidTestData<DummyBusinessObject>();
			var dummy4 = newFactory.NewWithValidTestData<DummyBusinessObject>();
			var dummy5 = newFactory.NewWithValidTestData<DummyBusinessObject>();

			var dummyBusinessObjectCollection = new DummyBusinessObjectCollection(newFactory);
			dummyBusinessObjectCollection.Add(dummyBusinessObject);
			dummyBusinessObjectCollection.Add(dummy1);
			dummyBusinessObjectCollection.Add(dummy2);
			dummyBusinessObjectCollection.Add(dummy3);
			dummyBusinessObjectCollection.Add(dummy4);
			dummyBusinessObjectCollection.Add(dummy5);

			var anotherStmOverrideDocData = newFactory.NewWithValidTestData<VisualizerNote>();
			anotherStmOverrideDocData.DD_ParentRelatedID = dummyBusinessObject.PK;
			anotherStmOverrideDocData.DD_DocumentData = ZBlob.FromAscii("abc");
			anotherStmOverrideDocData.DD_ParentTableCode = "JS";

			newFactory.Save();

			dummyBusinessObjectCollection.RemoveAndDeleteAll();

			var expectedHitCounts = new Dictionary<string, int> {
				{ StmUniversalCopySchema.Constants.TableName, 3 },
				{ StmDocDataOverrideSchema.Constants.TableName, 2 },
				{ StmNoteSchema.Constants.TableName, 2 },
			};

			CombineAssertions(() =>
			{
				AssertDbHits(expectedHitCounts, newFactory);
				AssertEquals(0, newFactory.Load<VisualizerNote>(new ZQuery()).Length);
			});
		}
	}
}

using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews
{
	class ModelViewTest : TransactionedTestCase
	{
		public void TestIndexFilterDefinitionSameAsStoredInDatabase()
		{
			foreach (var modelView in CargoWise.DbUpgrader.Scripts.Definitions.ModelViews.GetViews().OfType<IIndexedViewWithTemporaryIndexesOrColumns>())
			{
				var type = modelView.GetType();
				CombineAssertions($@"{type.Name}, {type.Namespace}
-------------------------------------------------------------------------------------------------
Index filter definition created in the database is different from IndexInfo.Filter as provided in the code,
please correct the code with that created in the database so that they are exactly the same.
-------------------------------------------------------------------------------------------------",
				() =>
				{
					TestConnection.ExecuteNonQuery(string.Join("\r\n", modelView.TemporaryComputedColumns.Select(c => c.AddDefinition)));

					foreach (var indexInfo in modelView.TemporaryIndexes)
					{
						if (indexInfo.HasFilter)
						{
							indexInfo.Drop(TestConnection);
							_ = indexInfo.Create(TestConnection);
							var indexInfoCreatedInTheDatabase = IndexLoader.LoadTop1(TestConnection, indexInfo.SchemaName, indexInfo.TableName, indexInfo.IndexName);
							AssertEquals(indexInfoCreatedInTheDatabase.Filter, indexInfo.Filter);
						}
					}
				});
			}
		}
	}
}

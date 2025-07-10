using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews
{
	abstract class BaseModelViewScriptTest : DbCreateScriptTest
	{
		protected abstract string ViewName { get; }

		protected abstract string UnderlyingTableName { get; }

		protected abstract TestDbViewHelper.DbColumn[] ExpectedViewColumns { get; }

		protected abstract bool HasIndexes { get; }

		protected virtual string IndexedViewName => $"{ViewName}_Idx";

		protected virtual TestDbViewHelper.DbColumn[] ExpectedIndexedViewColumns =>
			Array.Empty<TestDbViewHelper.DbColumn>();

		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				ViewName,
				UnderlyingTableName,
				ExpectedViewColumns
			);
		}

		public void TestIndexedViewColumns()
		{
			var indexedViewInfoExists = ExpectedIndexedViewColumns.Length > 0;
			Assert($"{ViewName} either should have '{nameof(HasIndexes)}' set to true and '{nameof(ExpectedIndexedViewColumns)}' properties defined or set to false and properties not defined.", HasIndexes == indexedViewInfoExists);

			if (HasIndexes)
			{
				TestDbViewHelper.AssertViewColumns(
					Db.Connection,
					IndexedViewName,
					tableName: null,
					ExpectedIndexedViewColumns
				);
			}
		}

		public void TestViewIndexes()
		{
			if (!HasIndexes)
			{
				AssertEquals($"{ViewName} doesn't require any indexes.", expected: false, DbObjectCreator.ViewExists(Db.Connection, IndexedViewName));
			}
			else
			{
				var expectedIndexes = new List<TestDbViewHelper.DbIndex>(ExpectedIndexedViewColumns.Length);
				var expectedClusteredIndexColumns = new List<string>();

				foreach (var indexedViewColumn in ExpectedIndexedViewColumns)
				{
					if (indexedViewColumn.ColumnName.EndsWith("_PK") ||
						indexedViewColumn.ColumnName.EndsWith("_ClusterKey"))
					{
						expectedClusteredIndexColumns.Add(indexedViewColumn.ColumnName);
						continue;
					}

					var indexName = $"NR_UX__{indexedViewColumn.ColumnName}";
					var columns = indexedViewColumn.ColumnName;
					expectedIndexes.Add(new(indexName, columns));
				}

				if (expectedClusteredIndexColumns.Count > 0)
				{
					expectedClusteredIndexColumns.Sort();
					var indexName = $"NR_UC__{string.Join("_", expectedClusteredIndexColumns)}";
					var columns = string.Join(",", expectedClusteredIndexColumns);
					expectedIndexes.Insert(0, new(indexName, columns));
				}

				TestDbViewHelper.AssertViewIndexes(
					Db.Connection,
					IndexedViewName,
					expectedIndexes.ToArray()
				);
			}
		}
	}
}

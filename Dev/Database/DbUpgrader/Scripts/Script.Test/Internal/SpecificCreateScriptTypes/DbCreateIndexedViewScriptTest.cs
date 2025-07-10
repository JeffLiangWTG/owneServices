using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script
{
	[TestsSubclassesOf(typeof(DbCreateIndexedViewScript))]
	abstract class DbCreateIndexedViewScriptTest : DbCreateScriptTest
	{
		#region Base Tests (no subclass test implementation required)

		[ExpectNoExceptions()]
		public void TestCreateViewAndIndex()
		{
			DbColumnDependencyRemover.DropSchemaBoundReferencingObjects(TestConnection, Db.SqlDbOwnerSchema, ScriptToTest.Name);

			var sqlText = string.Format(
				"IF exists(SELECT null FROM sys.objects WHERE name = '{0}') DROP VIEW [{0}];",
				ScriptToTest.Name);
			TestConnection.ExecuteNonQuery(sqlText);
			TestConnection.ExecuteNonQuery(ScriptToTest.Text);
			TestConnection.ExecuteNonQuery(((DbCreateIndexedViewScript)ScriptToTest).IndexCreateScript);
		}

		public void TestViewAndIndexExistInDatabase()
		{
			var viewName = ScriptToTest.Name;
			var clusteredIndexName = $"NR_UC__{viewName}";
			AssertViewAndIndexExist(viewName, clusteredIndexName);
		}

		#endregion

		public void TestNonClusteredIndexesExistInDatabase()
		{
			var nonClusteredIndexes = GetNonClusteredIndexes();
			if (nonClusteredIndexes.Any())
			{
				var viewName = ScriptToTest.Name;
				foreach (var indexInfo in nonClusteredIndexes)
				{
					AssertViewAndIndexExist(viewName, indexInfo.IndexName);
				}
			}
			else
			{
				Assert(true);
			}
		}

		void AssertViewAndIndexExist(string viewName, string indexName)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DECLARE @ViewId int;
				SET @ViewId = (SELECT object_id FROM sys.views WHERE name = '{0}');

				IF exists(SELECT null FROM sys.indexes WHERE object_id = @ViewId AND name = '{1}')
					SELECT 1;
				ELSE
					SELECT 0;
				",
				viewName, indexName);

			bool indexCreated = (Convert.ToInt32(TestConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture) == 1);
			Assert(string.Format(CultureInfo.InvariantCulture, @"View [{0}] and Index [{1}] exist?", viewName, indexName), indexCreated);
		}

		protected virtual IEnumerable<IndexInfo> GetNonClusteredIndexes() => Enumerable.Empty<IndexInfo>();
	}
}


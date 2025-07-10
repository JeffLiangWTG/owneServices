using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Data.Testing;

sealed class SqlSynonymNameResolverTest : TestCase
{
	public void TestGetReferencedDatabasesFromSynonyms_NullSql_ReturnsEmptyHashSet()
	{
		string sql = null;
		var result = SqlSynonymNameResolver.GetReferencedDatabasesFromSynonyms(sql);

		AssertNotNull(result);
		AssertEquals(0, result.Count);
	}

	public void TestGetReferencedDatabasesFromSynonyms_EmptySql_ReturnsEmptyHashSet()
	{
		var sql = string.Empty;
		var result = SqlSynonymNameResolver.GetReferencedDatabasesFromSynonyms(sql);

		AssertNotNull(result);
		AssertEquals(0, result.Count);
	}

	public void TestGetReferencedDatabasesFromSynonyms_EmptyString()
	{
		var synonyms = SqlSynonymNameResolver.GetReferencedDatabasesFromSynonyms("");
		AssertContainsExactElementsInAnyOrder("Empty SQL should return empty set", [], synonyms);
	}

	public void TestGetReferencedDatabasesFromSynonyms_NoSynonyms()
	{
		const string sql = "SELECT * FROM dbo.DirectTable WHERE Column = 1";
		var synonyms = SqlSynonymNameResolver.GetReferencedDatabasesFromSynonyms(sql);
		AssertContainsExactElementsInAnyOrder("SQL without synonyms should return empty set", [], synonyms);
	}

	public void TestGetReferencedDatabasesFromSynonyms_MultipleSynonyms()
	{
		const string sql = @"
		SELECT a.* FROM dbo.RefDatabase_RefCusTariff a
		INNER JOIN dbo.RefDbCmrAU_CMRRefundReason b ON a.Id = b.RefId";

		var synonyms = SqlSynonymNameResolver.GetReferencedDatabasesFromSynonyms(sql);

		var expectedAuRefDbCmr = RefDbTableNameResolver.GetExclusiveRefDbName(Db.Connection.CurrentDatabase, RefDbTypeEnum.Customs, "AU");
		AssertContainsExactElementsInAnyOrder(
			"Should find both synonym tables",
			[RefDbTableNameResolver.SingleRefDatabaseName, expectedAuRefDbCmr],
			synonyms);
	}

	public void TestGetReferencedDatabasesFromSynonyms_CaseInsensitive()
	{
		const string sql = "SELECT * FROM DBO.refdatabase_refcustariff";

		var synonyms = SqlSynonymNameResolver.GetReferencedDatabasesFromSynonyms(sql);
		AssertContainsExactElementsInAnyOrder(
			"Should find synonym regardless of case",
			[RefDbTableNameResolver.SingleRefDatabaseName],
			synonyms);
	}

	public void TestGetReferencedDatabasesFromSynonyms_PartialMatches()
	{
		const string sql = "SELECT * FROM dbo.RefDatabase_RefCusTariffFakeTable";

		var synonyms = SqlSynonymNameResolver.GetReferencedDatabasesFromSynonyms(sql);
		AssertContainsExactElementsInAnyOrder(
			"Should not match partial synonym names",
			[],
			synonyms);
	}

	public void TestGetReferencedDatabasesFromSynonyms_RepeatedSynonyms()
	{
		const string sql = @"
		SELECT t1.* FROM dbo.RefDatabase_RefCusTariff t1
		INNER JOIN dbo.RefDatabase_RefCusTariff t2 ON t1.Id = t2.ParentId";

		var synonyms = SqlSynonymNameResolver.GetReferencedDatabasesFromSynonyms(sql);
		AssertContainsExactElementsInAnyOrder(
			"Should return unique synonyms even when repeated in SQL",
			[RefDbTableNameResolver.SingleRefDatabaseName],
			synonyms);
	}

	public void TestGetReferencedDatabasesFromSynonyms_ThreadSafety_ReturnsCorrectResults()
	{
		const int threadCount = 10;
		var barrier = new Barrier(threadCount);
		var synonyms = new HashSet<string>[threadCount];
		const string sql = @"
		SELECT a.* FROM dbo.RefDatabase_RefCusTariff a
		INNER JOIN dbo.RefDbCmrAU_CMRRefundReason b ON a.Id = b.RefId";

		var threads = new Thread[threadCount];
		for (var i = 0; i < threadCount; i++)
		{
			var threadId = i;
			threads[i] = new Thread(() =>
			{
				barrier.SignalAndWait();
				synonyms[threadId] = SqlSynonymNameResolver.GetReferencedDatabasesFromSynonyms(sql).ToHashSet();
			});
			threads[i].Start();
		}

		foreach (var thread in threads)
		{
			thread.Join();
		}

		var expectedAuRefDbCmr = RefDbTableNameResolver.GetExclusiveRefDbName(Db.Connection.CurrentDatabase, RefDbTypeEnum.Customs, "AU");
		var singleSharedRefDb = RefDbTableNameResolver.SingleRefDatabaseName;

		for (var i = 0; i < threadCount; i++)
		{
			CombineAssertions("Enter a specific assertion message here.", () =>
			{
				var result = synonyms[i];
				AssertContainsExactElementsInAnyOrder(
					"Should find synonym regardless of case",
					[singleSharedRefDb, expectedAuRefDbCmr],
					result);
			});
		}
	}
}

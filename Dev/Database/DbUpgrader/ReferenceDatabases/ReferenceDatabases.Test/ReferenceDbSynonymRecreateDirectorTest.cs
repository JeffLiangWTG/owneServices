using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	sealed class ReferenceDbSynonymRecreateDirectorTest : TransactionedTestCase
	{
		public void TestCleanupUnusedRefDbSynonyms()
		{
			var upgradeContext = new Mock<IUpgradeContext>().Object;
			var initialSynonymCount = GetRefDbSynonymCount();
			var synonymPrefixesToKeep = new List<string>();
			synonymPrefixesToKeep.AddRange(ReferenceDbUpgradeDirector.ReferenceDbUpgraderFactories.Select(f => f.New(upgradeContext, TestConnection, null).SynonymPrefix));

			ReferenceDbSynonymRecreateDirector.DropOldRefDbSynonyms(TestConnection, synonymPrefixesToKeep);
			AssertEquals("Initial RefDb synonym count", initialSynonymCount, GetRefDbSynonymCount());
			AssertSynonymsExist("RefDbCmrAU", expected: true);
			AssertSynonymsExist("RefDbEntCA", expected: true);
			AssertSynonymsExist("RefDbTrfNZ", expected: true);

			int referenceSynonymCount = initialSynonymCount;
			synonymPrefixesToKeep.Remove("RefDbCmrAU_");
			ReferenceDbSynonymRecreateDirector.DropOldRefDbSynonyms(TestConnection, synonymPrefixesToKeep);
			int currentSynonymCount = GetRefDbSynonymCount();
			AssertEquals("RefDb synonym count decreased?", true, currentSynonymCount < referenceSynonymCount);
			AssertSynonymsExist("RefDbCmrAU", expected: false);
			AssertSynonymsExist("RefDbEntCA", expected: true);
			AssertSynonymsExist("RefDbTrfNZ", expected: true);

			referenceSynonymCount = currentSynonymCount;
			synonymPrefixesToKeep.Remove("RefDbTrfNZ_");
			ReferenceDbSynonymRecreateDirector.DropOldRefDbSynonyms(TestConnection, synonymPrefixesToKeep);
			currentSynonymCount = GetRefDbSynonymCount();
			AssertEquals("RefDb synonym count decreased again?", true, currentSynonymCount < referenceSynonymCount);
			AssertSynonymsExist("RefDbCmrAU", expected: false);
			AssertSynonymsExist("RefDbEntCA", expected: true);
			AssertSynonymsExist("RefDbTrfNZ", expected: false);

			ReferenceDbSynonymRecreateDirector.DropOldRefDbSynonyms(TestConnection, synonymPrefixesToKeep);
			AssertEquals("RefDb synonym count", currentSynonymCount, GetRefDbSynonymCount());

			TestConnection.ExecuteNonQuery(@"CREATE SCHEMA Test;");
			TestConnection.ExecuteNonQuery(@"CREATE SYNONYM [Test].RefDbCmrAU_JobDeclaration FOR dbo.JobDeclaration");
			ReferenceDbSynonymRecreateDirector.DropOldRefDbSynonyms(TestConnection, synonymPrefixesToKeep);
			AssertEquals("RefDb synonym count", currentSynonymCount, GetRefDbSynonymCount());
		}

		int GetRefDbSynonymCount()
		{
			string sqlText = "SELECT count(*) FROM sys.synonyms WHERE name like 'RefDb%'";
			return Convert.ToInt32(TestConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
		}

		void AssertSynonymsExist(string prefix, bool expected)
		{
			string sqlText = "IF EXISTS (SELECT null FROM sys.synonyms WHERE name like '" + prefix + "%') SELECT 1 ELSE SELECT 0";
			AssertEquals(prefix + " synonyms exist", expected, Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
		}
	}
}

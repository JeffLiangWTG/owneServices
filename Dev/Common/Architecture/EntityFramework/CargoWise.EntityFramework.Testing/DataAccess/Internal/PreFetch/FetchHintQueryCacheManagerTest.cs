using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class FetchHintQueryCacheManagerTest : TestCaseWithFactory
	{
		public void TestIsShortestSQLScriptFetchHint()
		{
			ZString code = new ZString("123");
			ZQuery mainQuery = new ZQuery(DummyBizoSchema.Z0_Code, code);
			ZQuery secondQuery = new ZQuery(DummyBizoSchema.Z0_Number, 56);
			ZQuery mixQuery = new ZQuery(mainQuery);
			mixQuery.AddToFilter(secondQuery);
			List<IFetchHint> list = new List<IFetchHint>();
			ZQueryFetchHint zMixQuery = new ZQueryFetchHint(DummyBizoSchema.Instance, mixQuery);
			list.Add(zMixQuery);
			ImmediateZMultiQueryFetchHint immediateZMultiQuery = new ImmediateZMultiQueryFetchHint(typeof(DummyBusinessObject), mainQuery, secondQuery);
			list.Add(immediateZMultiQuery);
			ZMultiQueryFetchHint zMultiQuery = new ZMultiQueryFetchHint(DummyBizoSchema.Instance, mainQuery, secondQuery);
			list.Add(zMultiQuery);
			ImmediateZQueryFetchHint immediateZQuery = new ImmediateZQueryFetchHint(typeof(DummyBusinessObject), mainQuery);
			list.Add(immediateZQuery);
			ZQueryFetchHint zQuery = new ZQueryFetchHint(DummyBizoSchema.Instance, mainQuery);
			list.Add(zQuery);
			ImmediateFetchHint immediateFetch = new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, code);
			list.Add(immediateFetch);
			FetchHint fetch = new FetchHint(DummyBizoSchema.Z0_Code, code);
			list.Add(fetch);

			ZQuery mainQueryWithBlob = new ZQuery(mainQuery);
			mainQueryWithBlob.IncludeBlob(DummyBizoSchema.Z0_VarCharMax);
			ZMultiQueryFetchHint zMultiQueryWithBlobs = new ZMultiQueryFetchHint(DummyBizoSchema.Instance, mainQueryWithBlob, secondQuery);
			list.Add(zMultiQueryWithBlobs);

			FetchHintQueryCacheManager manager = new FetchHintQueryCacheManager(list);
			AssertEquals(false, manager.IsShortestSQLScriptFetchHint(zMixQuery));
			AssertEquals(false, manager.IsShortestSQLScriptFetchHint(immediateZMultiQuery));
			AssertEquals(false, manager.IsShortestSQLScriptFetchHint(zMultiQuery));
			AssertEquals(true, manager.IsShortestSQLScriptFetchHint(immediateZQuery));
			AssertEquals(false, manager.IsShortestSQLScriptFetchHint(zQuery));
			AssertEquals(false, manager.IsShortestSQLScriptFetchHint(immediateFetch));
			AssertEquals(true, manager.IsShortestSQLScriptFetchHint(zMultiQueryWithBlobs));

			list.Clear();
			list.Add(immediateZQuery);
			list.Add(zQuery);
			manager = new FetchHintQueryCacheManager(list);
			AssertEquals(false, manager.IsShortestSQLScriptFetchHint(zQuery));
			AssertEquals(true, manager.IsShortestSQLScriptFetchHint(immediateZQuery));

			list.Clear();
			list.Add(immediateZQuery);
			list.Add(zQuery);
			manager = new FetchHintQueryCacheManager(list);
			AssertEquals(true, manager.IsShortestSQLScriptFetchHint(immediateZQuery));
			AssertEquals(false, manager.IsShortestSQLScriptFetchHint(zQuery));

			list.Clear();
			list.Add(zMultiQueryWithBlobs);
			FetchHint fetchWithBlobs = new FetchHint(DummyBizoSchema.Z0_Code, code, DummyBizoSchema.Z0_VarCharMax);
			list.Add(fetchWithBlobs);
			manager = new FetchHintQueryCacheManager(list);
			AssertEquals(false, manager.IsShortestSQLScriptFetchHint(zMultiQueryWithBlobs));
			AssertEquals(true, manager.IsShortestSQLScriptFetchHint(fetchWithBlobs));
		}
	}
}

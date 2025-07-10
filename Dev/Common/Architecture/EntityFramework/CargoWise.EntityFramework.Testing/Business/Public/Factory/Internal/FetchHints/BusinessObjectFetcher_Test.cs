using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectFetcher_Test : NUnit.Framework.TestCase
	{
		public void TestGroupSimpleHintsDoesNotExceedParameterLimitForZSqlInFilter()
		{
			var fetcher = new BusinessObjectFetcher();
			for (int i = 0; i < ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION + 1; i++)
			{
				fetcher.AddFetchHint(new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, (ZString)i.ToString()) { IsDataHintLoaded = true });
			}
			AssertEquals(ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION + 1, fetcher.FetchHints.Count);

			fetcher.GroupSimpleHints();
			AssertEquals(2, fetcher.FetchHints.Count);
		}
		public void TestGroupSimpleHints()
		{
			BusinessObjectFetcher fetcher = new BusinessObjectFetcher();

			fetcher.AddFetchHint(new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, new ZString("A")) { IsDataHintLoaded = true });
			fetcher.AddFetchHint(new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, new ZString("B")) { IsDataHintLoaded = true });
			fetcher.AddFetchHint(new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, new ZString("C")) { IsDataHintLoaded = false });

			fetcher.AddFetchHint(new ImmediateFetchHint(typeof(DummyChildBusinessObject), DummyBizoSchema.Z0_Code, new ZString("D")) { IsDataHintLoaded = true });
			fetcher.AddFetchHint(new ImmediateFetchHint(typeof(DummyChildBusinessObject), DummyBizoSchema.Z0_Code, new ZString("E")) { IsDataHintLoaded = true });

			fetcher.AddFetchHint(new ImmediateFetchHint(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Code, new ZString("F")) { IsDataHintLoaded = true });
			fetcher.AddFetchHint(new ImmediateFetchHint(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Code, new ZString("G")) { IsDataHintLoaded = true });
			fetcher.AddFetchHint(new ImmediateFetchHint(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Code, new ZString("H")) { IsDataHintLoaded = true });

			fetcher.AddFetchHint(new ImmediateZQueryFetchHint(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, new ZString("I"))) { IsDataHintLoaded = true });
			fetcher.AddFetchHint(new ImmediateZQueryFetchHint(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, new ZString("J"))) { IsDataHintLoaded = true });

			AssertEquals(10, fetcher.FetchHints.Count);

			fetcher.GroupSimpleHints();

			AssertEquals(6, fetcher.FetchHints.Count);

			List<string> queries = fetcher.FetchHints.Values.Select(hint => hint.GetQuery().LiteralTextADO).ToList();

			AssertCollectionContains("(Z0_Code in ('A', 'B'))", queries);
			AssertCollectionContains("Z0_Code = 'C'", queries);
			AssertCollectionContains("(Z0_Code in ('D', 'E'))", queries);
			AssertCollectionContains("(ZD1_Code in ('F', 'G', 'H'))", queries);
			AssertCollectionContains("Z0_Code = 'I'", queries);
			AssertCollectionContains("Z0_Code = 'J'", queries);
		}
	}
}

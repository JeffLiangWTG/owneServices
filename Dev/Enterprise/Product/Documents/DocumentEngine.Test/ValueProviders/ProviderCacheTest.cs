using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ValueReplacers.Testing
{
	sealed class ProviderCacheTest : TestCase
	{
		public void TestGetProviderResponsibleForIncludingDBProvider()
		{
			var cache = new ProviderCache();
			AssertEquals(typeof(DBOrBOValueProvider), cache.GetProviderResponsibleForIncludingDBProvider("<Table.Column>", Passes.FirstPass, null).GetType());
			AssertNull(cache.GetProviderResponsibleForIncludingDBProvider("<Blah>", Passes.FirstPass, null));
		}

		public void TestIsCached()
		{
			var cache = new ProviderCache();
			cache.GetProviderResponsibleForIncludingDBProvider("<Table.Column>", Passes.FirstPass, null);
			AssertEquals(true, cache.IsCached("<Table.Column>"));

			cache.GetProviderResponsibleForIncludingDBProvider("<CompanyName>", Passes.SecondPass, null);
			AssertEquals(true, cache.IsCached("<CompanyName>"));
		}

		public void TestIsntCachedtwice()
		{
			var cache = new ProviderCache();
			var providerToAdd = new FixedValueProvider("blah", "");
			cache.AddProvider(providerToAdd);
			AssertEquals(1, cache.Count);
			cache.AddProvider(providerToAdd);
			AssertEquals(1, cache.Count);
		}

		public void TestCount()
		{
			var cache = new ProviderCache();
			cache.AddProvider(new FixedValueProvider("test", "empty"));
			AssertEquals("Number of providers in cache is not correct: " + cache.Count, 1, cache.Count);
		}

		public void TestInitialValueProviders()
		{
			var providers = ProviderCache.InitialValueProvidersCache;
			var providers2 = ProviderCache.InitialValueProvidersCache;
			Assert("Should be the same instance.", object.ReferenceEquals(providers, providers2));
			AssertEquals(providers.Count, providers2.Count);
		}

		public void TestInitialValueProvidersCacheCanBeReset()
		{
			var providers = ProviderCache.InitialValueProvidersCache;
			Overridable.ResetAll();
			var providers2 = ProviderCache.InitialValueProvidersCache;
			Assert("Should be a different instance.", !object.ReferenceEquals(providers, providers2));
			AssertEquals(providers.Count, providers2.Count);
		}

		public void TestClone()
		{
			var cache = new ValueProviderCollector().ValueProviders;
			var clone = cache.Clone();
			AssertEquals(cache.Providers.Count, clone.Providers.Count);
			foreach (var provider in cache.Providers)
			{
				Assert(clone.Providers.Contains(provider));
			}
			AssertEquals(cache.AddedProviderIDs.Count, clone.AddedProviderIDs.Count);
			foreach (var id in cache.AddedProviderIDs)
			{
				Assert(clone.AddedProviderIDs.Contains(id));
			}
		}

		public void TestMultiThreadNoExceptionThrown()
		{
			var result = string.Empty;
			var threads = new List<Thread>();
			var startEvent = new ManualResetEventSlim(false);
			var provider = new FixedValueProvider("test", "empty");
			var cache = new ProviderCache();
			cache.AddProvider(provider);

			for (var i = 0; i < 20; i++)
			{
				var thread = new Thread(() =>
				{
					startEvent.Wait();
					try
					{
						cache.GetProviderResponsibleFor("<test>", Passes.FirstPass);
					}
					catch (Exception e)
					{
						result += e.ToString() + System.Environment.NewLine;
					}
				});

				thread.Start();
				threads.Add(thread);
			}

			try
			{
				startEvent.Set();
				WaitAll(threads);
				AssertEquals(string.Empty, result);
			}
			catch (Exception ex)
			{
				HtmlAssert("Error: " + ex.Message + "Stack: " + ex.StackTrace, false);
			}
		}

		void WaitAll(IEnumerable<Thread> threads)
		{
			if (threads != null)
			{
				foreach (var thread in threads)
				{
					if (thread.IsAlive)
					{
						thread.Join(1000);
					}
				}
			}
		}

		public void TestAddProvider()
		{
			var provider = new FixedValueProvider("test", "empty");
			var cache = new ProviderCache();
			cache.AddProvider(provider);
			AssertEquals(provider, cache.GetProviderResponsibleFor("<test>", Passes.FirstPass));
		}

		public void TestAddSameMacroNameProvider()
		{
			var cache = new ProviderCache();

			var provider = new FixedValueProvider("Consignor - Shipper", "aa");
			cache.AddProvider(provider);
			AssertEquals(1, cache.Count);

			var provider2 = new DelegateValueProvider("Consignor - Shipper", new ReplacementProviderMethod(GetFromDateReplacement));
			cache.AddProvider(provider2);
			AssertEquals(1, cache.Count);

			AssertEquals(provider, cache.GetProviderResponsibleFor("<Consignor - Shipper>", Passes.FirstPass));
		}

		object GetFromDateReplacement(string macro, Report report)
		{
			return "bb";
		}

		public void TestGetProviderResponsibleCachesResult()
		{
			var mock = new Mock<ValueProvider>();
			mock.Setup(m => m.PassToStartReplacingOn).Returns(Passes.FirstPass);
			mock.Setup(m => m.Regex).Returns(RegexProvider.InnermostMacrosRegex);
			mock.Setup(m => m.PassToStartReplacingOn).Returns(Passes.FirstPass);
			var cache = new ProviderCache();
			cache.AddProvider(mock.Object);
			mock.Protected().Setup<bool>("IsResponsibleForReplacingCore", ItExpr.IsAny<string>(), ItExpr.IsAny<Passes>()).Returns(true);
			AssertEquals(mock.Object, cache.GetProviderResponsibleFor("Blah", Passes.FirstPass));
			mock.Protected().Setup<bool>("IsResponsibleForReplacingCore", ItExpr.IsAny<string>(), ItExpr.IsAny<Passes>()).Returns(false);
			AssertEquals(mock.Object, cache.GetProviderResponsibleFor("Blah", Passes.FirstPass));
		}
	}
}

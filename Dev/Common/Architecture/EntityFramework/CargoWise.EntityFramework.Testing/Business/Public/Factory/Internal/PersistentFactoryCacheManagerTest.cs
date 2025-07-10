using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Moq;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class PersistentFactoryCacheManagerTest : TestCase
	{
		public void TestClearAllCaches()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			PersistentFactoryCacheManager.Instance.ClearAllQueryCaches();
			AssertEquals(1, factory1.RowFactory.QueryCacheClearCount);
			AssertEquals(1, factory2.RowFactory.QueryCacheClearCount);
		}

		public void TestClearAllCaches_WhatIfNull()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			PersistentFactoryCacheManager.Instance.ClearAllQueryCaches(new string[] { null });
			AssertEquals(0, factory1.RowFactory.QueryCacheClearCount);
		}

		public void TestWeakReferencesRelease()
		{
			GC.Collect();

			var initialInstances = RowFactory.GetActiveRowFactories().Select(rowFactory => rowFactory._Instance).ToArray();
			var currentInstances = RowFactory.GetActiveRowFactories().Select(rowFactory => rowFactory._Instance).ToArray();
			AssertFactoryInstances("PreCondition: RowFactory instances stable (no new factories yet).", initialInstances, currentInstances, null, null);

			long newFactoryInstance = AssertNewFactory(ref currentInstances); // Test new factory in a separate method to keep its local variable in a separate stack

			initialInstances = currentInstances;
			GC.Collect();
			currentInstances = RowFactory.GetActiveRowFactories().Select(rowFactory => rowFactory._Instance).ToArray();
			AssertFactoryInstances("Count after RowFactory release, new allocation.", initialInstances, currentInstances, null, new[] { newFactoryInstance });
		}

		public void TestTrackFactoriesCreatedOnCurrentThread()
		{
			using (var tracked = PersistentFactoryCacheManager.Instance.TrackFactoriesCreatedOnCurrentThread())
			{
				AssertNotNull(tracked);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
				IDisposable secodeTrackedFactories = null;
				AssertNoExceptionThrown(() => secodeTrackedFactories = PersistentFactoryCacheManager.Instance.TrackFactoriesCreatedOnCurrentThread());
				AssertNotNull(secodeTrackedFactories);
				AssertEquals("Expected an error", 1, ErrorReporter.TotalErrorCount);
				ErrorReporter.Clear();
			}
		}

		public void TestTrackFactories_DoesNotTrackNonAliveFactories()
		{
			using (var tracked = PersistentFactoryCacheManager.Instance.TrackFactoriesCreatedOnCurrentThread())
			{
				var factory1 = new BusinessObjectFactory();

				RequestProcessor();

				GC.Collect();
				GC.WaitForPendingFinalizers();
				var trackedFactories = PersistentFactoryCacheManager.Instance.GetFactoriesCreatedInTrackedRegion();

				AssertEquals(1, trackedFactories.Count());
				AssertEquals(factory1, trackedFactories.First());

				void RequestProcessor()
				{
					var factory2 = new BusinessObjectFactory();
				}
			}
		}

		public void TestGetFactoriesCreatedInTrackedRegionWeakReferenceRaceCondition()
		{
			var mockManager = new Mock<PersistentFactoryCacheManager>();

			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory { NameForDebugging = "Factory to be garbage collected" };

			mockManager
				.Setup(m => m.GetFactoriesFromWeakReferences())
				.Returns(new List<BusinessObjectFactory> { factory1, null });

			var manager = mockManager.Object;

			using (manager.TrackFactoriesCreatedOnCurrentThread())
			{
				manager.Add(factory1);
				manager.Add(factory2);

				var factories = manager.GetFactoriesCreatedInTrackedRegion();

				AssertNoExceptionThrown("Should be able to call DeactivateActiveCollectionsAndCaches on all factories returned", () =>
				{
					foreach (var factory in factories)
					{
						factory.DeactivateActiveCollectionsAndCaches();
					}
				});
				AssertEquals("Expected 1 factory", 1, factories.Count());
				AssertEquals("Only the alive factory is returned", factory1, factories.First());
			}
		}

		long AssertNewFactory(ref long[] initialInstances)
		{
			var factory = new BusinessObjectFactory();
			long factoryInstance = factory.RowFactory._Instance;
			var currentInstances = RowFactory.GetActiveRowFactories().Select(rowFactory => rowFactory._Instance).ToArray();
			AssertFactoryInstances("PreCondition: RowFactory count before Load, new allocation.", initialInstances, currentInstances, new[] { factoryInstance }, null);

			initialInstances = currentInstances;
			factory.LoadTop1(typeof(DummyBusinessObject), new ZQuery());
			currentInstances = RowFactory.GetActiveRowFactories().Select(rowFactory => rowFactory._Instance).ToArray();
			AssertFactoryInstances("PreCondition: RowFactory count after Load, new allocation.", initialInstances, currentInstances, new[] { factoryInstance }, null);

			return factoryInstance;
		}

		void AssertFactoryInstances(string message, long[] initialInstances, long[] finalInstances, long[] expectedInstances, long[] notExpectedInstances)
		{
			var errors = new StringBuilder();

			var newUnexpectedInstances = finalInstances.Where(instance => !initialInstances.Contains(instance) && (expectedInstances == null || !expectedInstances.Contains(instance))).ToArray();
			if (newUnexpectedInstances.Length > 0)
			{
				errors.AppendLine("New unexpected instances: " + string.Join(" ", newUnexpectedInstances));
			}

			if (expectedInstances != null)
			{
				var souldNotBeReleasedInstances = expectedInstances.Where(instance => !finalInstances.Contains(instance)).ToArray();
				if (souldNotBeReleasedInstances.Length > 0)
				{
					errors.AppendLine("Should not be released: " + string.Join(" ", souldNotBeReleasedInstances));
				}
			}

			if (notExpectedInstances != null)
			{
				var shouldBeReleasedInstances = notExpectedInstances.Where(finalInstances.Contains).ToArray();
				if (shouldBeReleasedInstances.Length > 0)
				{
					errors.AppendLine("Should be released: " + string.Join(" ", shouldBeReleasedInstances));
				}
			}

			if (errors.Length > 0)
			{
				Fail(message +
					"\r\nSome RowFactory instances were not released as expected." +
					"\r\nBefore: " + string.Join(" ", initialInstances) +
					"\r\nAfter: " + string.Join(" ", finalInstances) +
					"\r\n" + errors.ToString());
			}
			else
			{
				Assert(true);
			}
		}
	}
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class PropertyCacheTests : TestCase
	{
		public void TestGetTopProperty()
		{
			var p = PropertyCache.GetTopProperty(typeof(AutoDummyBizo), nameof(AutoDummyBizo.Z0_Number));
			AssertEquals(nameof(AutoDummyBizo.Z0_Number), p?.Name);
			AssertEquals(nameof(AutoDummyBizo), p?.DeclaringType?.Name);
		}

		public void TestGetTopProperty_Inherited()
		{
			var p = PropertyCache.GetTopProperty(typeof(DummyBusinessObjectWithHidingProperty), nameof(DummyBusinessObjectWithHidingProperty.Z0_Bool));
			AssertEquals(nameof(AutoDummyBizo.Z0_Bool), p?.Name);
			AssertEquals(nameof(AutoDummyBizo), p?.DeclaringType?.Name);
		}

		public void TestGetTopProperty_NonUnique()
		{
			var p = PropertyCache.GetTopProperty(typeof(DummyBusinessObjectWithHidingProperty), nameof(DummyBusinessObjectWithHidingProperty.Z0_Number));
			AssertEquals(nameof(DummyBusinessObjectWithHidingProperty.Z0_Number), p?.Name);
			AssertEquals(nameof(DummyBusinessObjectWithHidingProperty), p?.DeclaringType?.Name);
		}

		public void TestGetTopProperty_Missing()
		{
			var p = PropertyCache.GetTopProperty(typeof(DummyBusinessObjectWithHidingProperty), "Z0_IDoNotExist");
			AssertNull(p);
		}

		public void TestGetTopProperty_CacheSize()
		{
			PropertyCache.ClearTopPropertiesCache();

			var propertyNamePart1 = nameof(DummyBusinessObjectWithHidingProperty.Z0_Number).Substring(0, 3);
			var propertyNamePart2 = nameof(DummyBusinessObjectWithHidingProperty.Z0_Number).Substring(3);

			for (var i = 0; i < 10000; i++)
			{
				// Creating new string instance on every iteration to make sure that property names are not compared by reference.
				var propertyName = propertyNamePart1 + propertyNamePart2;

				var p1 = PropertyCache.GetTopProperty(typeof(DummyBusinessObject), propertyName);
				AssertEquals(nameof(AutoDummyBizo), p1?.DeclaringType?.Name);
				AssertEquals(nameof(AutoDummyBizo.Z0_Number), p1?.Name);

				var p2 = PropertyCache.GetTopProperty(typeof(DummyBusinessObjectWithHidingProperty), propertyName);
				AssertEquals(nameof(DummyBusinessObjectWithHidingProperty), p2?.DeclaringType?.Name);
				AssertEquals(nameof(DummyBusinessObjectWithHidingProperty.Z0_Number), p2?.Name);
			}

			var cacheSize = PropertyCache.ClearTopPropertiesCache();

			Assert($"If cache size ({cacheSize}) == 0, then we are not storing keys in the cache.", cacheSize > 0);

			// 1000 is big enough number to practically guarantee that no other thread will create so many records during execution of this test.
			Assert($"If cache size ({cacheSize}) >= 1000, then we are storing duplicate keys in the cache.", cacheSize > 0);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "Testing")]
		public void TestGetTopProperty_Concurrency()
		{
			var errors = new ConcurrentBag<Exception>();
			using (var startSignal = new ManualResetEvent(false))
			{
				var threads = new List<Thread>();
				for (var tn = 0; tn < 10; tn++)
				{
					var thread = new Thread(() =>
					{
						try
						{
							startSignal.WaitOne(TimeSpan.FromSeconds(60));

							for (var it = 0; it < 1000; it++)
							{
								if (it % 100 == 0)
								{
									PropertyCache.ClearTopPropertiesCache();
								}

								var d = PropertyCache.GetTopProperty(typeof(DummyBusinessObject), nameof(DummyBusinessObject.Z0_Decimal));
								AssertEquals(nameof(AutoDummyBizo.Z0_Decimal), d?.Name);
								AssertEquals(nameof(AutoDummyBizo), d?.DeclaringType?.Name);

								var d2 = PropertyCache.GetTopProperty(typeof(DummyBusinessObject), nameof(DummyBusinessObject.Z0_AnotherDecimal));
								AssertEquals(nameof(AutoDummyBizo.Z0_AnotherDecimal), d2?.Name);
								AssertEquals(nameof(AutoDummyBizo), d2?.DeclaringType?.Name);

								var d3 = PropertyCache.GetTopProperty(typeof(DummyBusinessObjectWithHidingProperty), nameof(DummyBusinessObjectWithHidingProperty.Z0_AnotherDecimal));
								AssertEquals(nameof(AutoDummyBizo.Z0_AnotherDecimal), d3?.Name);
								AssertEquals(nameof(AutoDummyBizo), d3?.DeclaringType?.Name);

								var n = PropertyCache.GetTopProperty(typeof(DummyBusinessObjectWithHidingProperty), nameof(DummyBusinessObjectWithHidingProperty.Z0_Number));
								AssertEquals(nameof(DummyBusinessObjectWithHidingProperty.Z0_Number), n?.Name);
								AssertEquals(nameof(DummyBusinessObjectWithHidingProperty), n?.DeclaringType?.Name);

								var m = PropertyCache.GetTopProperty(typeof(DummyBusinessObject), "I do not exist");
								AssertNull(m);
							}
						}
						catch (Exception ex)
						{
							errors.Add(ex);
						}
					});
					threads.Add(thread);
					thread.IsBackground = true;
					thread.Name = $"{nameof(TestGetTopProperty_Concurrency)}:{tn}";
				}

				foreach (var thread in threads)
				{
					thread.Start();
				}

				startSignal.Set();

				foreach (var thread in threads)
				{
					Assert($"{thread.Name} stopped", thread.Join(TimeSpan.FromSeconds(60)));
				}
			}

			if (errors.Count > 0)
			{
				throw new AggregateException("Exception occured in a background thread.", errors);
			}
		}

		class DummyBusinessObjectWithHidingProperty : DummyBusinessObject
		{
			public DummyBusinessObjectWithHidingProperty(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new ZString Z0_Number { get; set; }
		}
	}
}

#if DEBUG
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class PropertyDescriptorCollectionFactoryTests : TestCase
	{
		public void TestFromType_UsesLRUCache()
		{
			var propertiesRef = CreateUnrefrencedPropertiesCollection();

			Type[] typeList = typeof(int).Assembly.GetTypes();
			for (int i = 0; i < 100; i++)
			{
				// invalidate the LRU cache
				KPropertyDescriptorCollection.FromType(typeList[i]);
			}
			GC.Collect();
			AssertEquals("The collection should be collected when it is not referenced by anything", false, propertiesRef.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateUnrefrencedPropertiesCollection()
		{
			var properties = KPropertyDescriptorCollection.FromType(typeof(TestComponent));
			var propertiesRef = new WeakReference(properties);
			return propertiesRef;
		}

		public void TestTypeDescriptor_Refresh()
		{
			KPropertyDescriptorCollection properties = KPropertyDescriptorCollection.FromType(typeof(TestComponent));
			AssertEquals("Collection cached", true, properties == KPropertyDescriptorCollection.FromType(typeof(TestComponent)));

			TypeDescriptor.Refresh(typeof(TestComponent));
			AssertEquals("Cache invalidated by Refresh(Type)", false, properties == KPropertyDescriptorCollection.FromType(typeof(TestComponent)));
		}

		public void TestFromType_IsThreadSafe()
		{
			ManualResetEvent start = new ManualResetEvent(false);
			const int n = 10;
			Task[] tasks = new Task[n];
			for (int i = 0; i < n; ++i)
			{
				tasks[i] = Task.Factory.StartNew(() =>
				{
					start.WaitOne();
					KPropertyDescriptorCollection.FromType(typeof(KPropertyDescriptorCollectionTests.TestComponent));
				});
			}

			start.Set();
			Task.WaitAll(tasks);
			var props = KPropertyDescriptorCollection.FromType(typeof(KPropertyDescriptorCollectionTests.TestComponent));
			for (int i = 0; i < props.Count; ++i)
			{
				AssertNotNull(props[i]);
			}

			AssertEquals(101, props.Count);
		}

		#region Test no duplicate property descriptors

		public void TestNoDuplicatePropertyDescriptors()
		{
			AssertPopulateFromType(false, false);
			AssertPopulateFromType(false, true);
			AssertPopulateFromType(true, false);
			AssertPopulateFromType(true, true);
		}

		void AssertPopulateFromType(bool includePrivate1, bool includePrivate2)
		{
			TestDuplicatesPropertyDescriptorCollection.PopulatedCount = 0;

			var factory = new PropertyDescriptorCollectionFactory<TestDuplicatesPropertyDescriptorCollection>(t => new TestDuplicatesPropertyDescriptorCollection(t, includePrivate1));
			AssertEquals(0, TestDuplicatesPropertyDescriptorCollection.PopulatedCount);

			factory.FromType(typeof(TestComponent), includePrivate2);
			AssertEquals(1, TestDuplicatesPropertyDescriptorCollection.PopulatedCount);
		}

		class TestDuplicatesPropertyDescriptorCollection : KPropertyDescriptorCollection
		{
			public TestDuplicatesPropertyDescriptorCollection(Type componentType, bool includePrivate)
				: base(componentType, includePrivate)
			{
			}

			public static int PopulatedCount { get; set; }

			protected override void PopulatePropertyDescriptorsCore()
			{
				base.PopulatePropertyDescriptorsCore();
				PopulatedCount++;
			}

			protected override KPropertyDescriptorCollection NewEmptyPropertyDescriptorCollectionCore(Type componentType, bool includePrivate)
			{
				return new TestDuplicatesPropertyDescriptorCollection(componentType, includePrivate);
			}
		}

		#endregion

		#region Test Classes

		class TestComponent
		{
			public TestRelatedComponent Related
			{
				get
				{
					return null;
				}
			}

			public string Property
			{
				get
				{
					return null;
				}
			}
		}

		class TestRelatedComponent
		{
			public string RelatedProperty
			{
				get
				{
					return null;
				}
			}
		}

		#endregion
	}
}
#endif

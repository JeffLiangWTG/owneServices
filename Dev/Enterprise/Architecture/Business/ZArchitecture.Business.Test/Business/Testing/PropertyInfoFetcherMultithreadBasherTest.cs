using System;
using System.Reflection;
using System.Threading;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class PropertyInfoFetcherMultithreadBasherTest : MultiThreadBasherBaseTest
	{
		protected override void ThreadBashingMethod()
		{
			Type declaringType;

			for (int i = 0; i < 100; i++)
			{
				PropertyInfoFetcher.RemoveAll();
				Thread.Sleep(0);
				declaringType = PropertyInfoFetcher.GetFromLowestSubclass(typeof(Child), "Item2").DeclaringType;
				Thread.Sleep(0);
				PropertyInfoFetcher.RemoveType(declaringType);
				Thread.Sleep(0);
				PropertyInfoFetcher.RemoveType(typeof(Child));
				Thread.Sleep(1);
			}
		}

		protected override void SetupThreadSafeInstances()
		{
			ClearCachedPropertyInfos();
		}

		protected override void RestoreSingleThreadInstances()
		{
			ClearCachedPropertyInfos();
		}

		protected override void SetUp()
		{
			ClearCachedPropertyInfos();
		}

		void ClearCachedPropertyInfos()
		{
			((ILRUCache)typeof(PropertyInfoFetcher).InvokeMember("cachedPropertyInfos", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField, null, null, null)).Clear();
		}

		#region Test Classes

		class GrandParent
		{
			public GrandParent Item
			{
				get { return null; }
			}

			public Child Item2
			{
				get { return null; }
			}
		}

		class Parent : GrandParent
		{
			public new Parent Item
			{
				get { return null; }
			}
		}

		class Child : Parent
		{
			public new Child Item
			{
				get { return null; }
			}

			public new GrandParent Item2
			{
				get { return null; }
			}
		}

		#endregion
	}
}

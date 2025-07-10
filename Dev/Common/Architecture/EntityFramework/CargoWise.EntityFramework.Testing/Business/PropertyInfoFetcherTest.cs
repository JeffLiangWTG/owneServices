using System.Reflection;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class PropertyInfoFetcherTest : TestCase
	{
		public void TestGetFromLowestSubclassWhenPropertyDoesNotExist()
		{
			AssertNull(PropertyInfoFetcher.GetFromLowestSubclass(typeof(string), "Cotton"));
		}

		public void TestGetFromLowestSubclassWhenPropertyDeclaredMoreSpecificallyInChild()
		{
			AssertEquals(typeof(Child), PropertyInfoFetcher.GetFromLowestSubclass(typeof(Child), "Item").DeclaringType);
		}

		public void TestGetFromLowestSubclassWhenPropertyDeclaredMoreSpecificallyInGrandParent()
		{
			AssertEquals(typeof(Child), PropertyInfoFetcher.GetFromLowestSubclass(typeof(Child), "Item2").DeclaringType);
		}

		public void TestGetFromLowestSubclassCachesProperly()
		{
			PropertyInfo info = PropertyInfoFetcher.GetFromLowestSubclass(typeof(GrandParent), "Item");
			AssertEquals("GetFromLowestSubclassCache(typeof(GrandParent), \"Item\")", info, PropertyInfoFetcher.GetFromLowestSubclassCache(typeof(GrandParent), "Item"));
		}
	}

	#region Implementation
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

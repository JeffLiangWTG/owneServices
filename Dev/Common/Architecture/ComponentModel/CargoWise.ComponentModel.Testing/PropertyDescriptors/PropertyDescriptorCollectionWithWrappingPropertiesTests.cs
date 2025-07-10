#if DEBUG
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class PropertyDescriptorCollectionWithWrappingPropertiesTests : TestCase
	{
		public void TestIndexerReturnsWrappedProperties()
		{
			WrappingPropertyDescriptor prop = (WrappingPropertyDescriptor)PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(TestComponent))["Related+RelatedProperty"];
			AssertEquals("Related", prop.Outer.Name);
			AssertEquals("RelatedProperty", prop.Inner.Name);
		}

		public void TestIndexerWithInvalidWrappedPropertyReturnsNull()
		{
			AssertNull(PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(TestComponent))["Related,aha"]);
			AssertNull(PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(TestComponent))["splaty,aha"]);
		}

		public void TestForeachingPropertiesDoesntReturnWrappedProperties()
		{
			KPropertyDescriptorCollection properties =
				PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(TestComponent));

			// access a wrapped property so it is lazy created for test
			object x = properties["Related+RelatedProperty"];

			bool wrapped_property_found = false;
			foreach (PropertyDescriptor property in (PropertyDescriptorCollection)properties)
			{
				if (property is WrappingPropertyDescriptor)
				{
					wrapped_property_found = true;
				}
			}
			AssertEquals("Should find a wrapped property when the DataGrid enumerates the properties", true, wrapped_property_found);

			foreach (PropertyDescriptor property in properties)
			{
				AssertEquals(
					"Shouldnt return a wrapped property when the developer is enumerating the properties",
					false, property is WrappingPropertyDescriptor);
				AssertEquals(
					"Shouldnt return a wrapped property when the developer is enumerating the properties",
					false, property.Name.IndexOf("+") != -1);
			}
		}

		public void TestFromType_UsesLRUCache()
		{
			var propertiesRef = CreateUnrefrencedPropertiesCollection();

			Type[] typeList = typeof(int).Assembly.GetTypes();
			for (int i = 0; i < 100; i++)
			{
				// invalidate the LRU cache
				PropertyDescriptorCollectionWithWrappingProperties.FromType(typeList[i]);
			}
			GC.Collect();
			AssertEquals("The property collection should be collected when it is not referenced by anything", false, propertiesRef.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateUnrefrencedPropertiesCollection()
		{
			var properties = PropertyDescriptorCollectionWithWrappingProperties.FromType(typeof(TestComponent));
			var propertiesRef = new WeakReference(properties);
			return propertiesRef;
		}

		abstract class TestComponent
		{
			public abstract TestRelatedComponent Related { get; }
			public abstract int Property { get; }
		}

		abstract class TestRelatedComponent
		{
			public abstract string RelatedProperty { get; }
		}
	}
}
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZCustomTypeDescriptorTest : TestCaseWithFactory
	{
		#region RemoveFromCache

		public void TestRemoveFromCache()
		{
			var a = ZCustomTypeDescriptor.GetProperties(typeof(TestCustomTypeDescriptorBO)).OfType<KPropertyDescriptor>().Select(x => x.Name);
			ZCustomTypeDescriptor.RemoveFromCache(typeof(TestCustomTypeDescriptorBO));
			ZCustomTypeDescriptor.RemoveFromCache(typeof(DummyBusinessObject));
			ZCustomTypeDescriptor.RemoveFromCache(typeof(string));
			ZCustomTypeDescriptor.RemoveFromCache(null);
			var b = ZCustomTypeDescriptor.GetProperties(typeof(TestCustomTypeDescriptorBO)).OfType<KPropertyDescriptor>().Select(x => x.Name);
			AssertContainsExactElementsInAnyOrder("Properties should be invariant over clearing", a, b);
		}

		#endregion

		#region GetProperties

		public void TestGetProperties()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			_ = dummy.GetProperties()["Self+Z0_Code"];

			bool foundWrapped = false;
			foreach (PropertyDescriptor next in dummy.GetProperties())
			{
				if (next.Name.IndexOf("+") != -1)
				{
					foundWrapped = true;
					break;
				}
			}
			AssertEquals("Should not contain flattened properties in GetProperties", false, foundWrapped);
		}

		public void TestGetProperties_IncludesFlattenedProperties()
		{
			DummyBusinessObject dummy = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			_ = dummy.GetProperties()["Self+Z0_Code"];

			bool foundWrapped = false;
			foreach (PropertyDescriptor next in (PropertyDescriptorCollection)dummy.GetProperties())
			{
				if (next.Name.IndexOf("+") != -1)
				{
					foundWrapped = true;
					break;
				}
			}
			AssertEquals("Should find flattened properties in GetPropertiesIncludingFlattened", true, foundWrapped);
		}

		public void TestInstanceGetPropertiesUseSameCollectionInstanceAsStaticGetProperties()
		{
			TestCustomTypeDescriptorBO customTypeDescInstance = (TestCustomTypeDescriptorBO)Factory.New(typeof(TestCustomTypeDescriptorBO));

			PropertyDescriptorCollection collection = ZCustomTypeDescriptor.GetProperties(typeof(TestCustomTypeDescriptorBO));
			PropertyDescriptorCollection collectionIncludingFlattened = ZCustomTypeDescriptor.GetProperties(typeof(TestCustomTypeDescriptorBO));
			PropertyDescriptorCollection instanceCollection = customTypeDescInstance.GetProperties();
			PropertyDescriptorCollection instanceCollectionIncludingFlattened = customTypeDescInstance.GetProperties().IncludingFlattened;

			AssertEquals(
				"Instance properties should be consistent with static properties",
				true, collection == instanceCollection);
			AssertEquals(
				"Instance properties inc. flattened should be consistent with static properties",
				true, collectionIncludingFlattened == instanceCollectionIncludingFlattened);
		}

		public void TestRefreshProperties()
		{
			var businessObject = new TestDynamicBusinessObject(Factory);
			businessObject.AddProperty("_property_A_", new DynamicBusinessObjectProperty(typeof(string), false));
			var properties = businessObject.GetProperties().AllProperties;

			AssertNotNull(properties["_property_A_"]);

			businessObject.AddProperty("_property_B_", new DynamicBusinessObjectProperty(typeof(string), false));

			AssertNull(properties["_property_B_"]);

			businessObject.RefreshDynamicBusinessObjectPropertyDescriptorCollection();

			AssertNotNull(properties["_property_B_"]);
		}

		#endregion

		#region GetProperties with TypeDecider

		public void TestGetProperties_DoesntUseTypeDeciderOnInstance()
		{
			TestDummyBusinessObject.TypeDecider.TypeToReturn = typeof(TestDummyBusinessObjectSubclass1);
			TestDummyBusinessObject dummy = Factory.New<TestDummyBusinessObject>();
			AssertEquals("TypeDecider should not be used for instance property because we already know the concrete type", true, ZCustomTypeDescriptor.GetProperties(typeof(TestDummyBusinessObject)) == dummy.GetProperties());
		}

		public void TestGetProperties_FromSubclasses()
		{
			TestDummyBusinessObject.TypeDecider.TypeToReturn = typeof(TestDummyBusinessObjectSubclass1);
			PropertyDescriptorCollection properties1 = ZCustomTypeDescriptor.GetProperties(typeof(TestDummyBusinessObject));
			AssertNotNull("Should contain subclass 1 property", properties1["Subclass1Property"]);
			AssertNull("Should not contain subclass 2 property", properties1["Subclass2Property"]);

			TestDummyBusinessObject.TypeDecider.TypeToReturn = typeof(TestDummyBusinessObjectSubclass2);
			PropertyDescriptorCollection properties2 = ZCustomTypeDescriptor.GetProperties(typeof(TestDummyBusinessObject));
			AssertNotNull("Should contain subclass 2 property", properties2["Subclass2Property"]);
			AssertNull("Should not contain subclass 1 property", properties2["Subclass1Property"]);
		}

		#endregion

		#region ICustomTypeDescriptor

		public void TestICustomTypeDescriptor_GetProperties()
		{
			PropertyDescriptor returnedProperty = ((ICustomTypeDescriptor)Component).GetProperties(null)["SomeProp"];
			AssertEquals(typeof(TestComponent2), returnedProperty.PropertyType);
		}

		#endregion

		#region ITypedList

		public void TestITypedList_GetItemProperties_WithNull()
		{
			PropertyDescriptor returnedProperty = ((ITypedList)Component).GetItemProperties(null)["SomeProp"];
			AssertEquals(typeof(TestComponent2), returnedProperty.PropertyType);
		}

		public void TestITypedList_GetItemProperties_WithProperty()
		{
			PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(TestComponent))["SomeProp"];
			PropertyDescriptor[] propertyPath = new PropertyDescriptor[] { property };
			PropertyDescriptor returnedProperty = ((ITypedList)Component).GetItemProperties(propertyPath)["SomeProp2"];
			AssertEquals(typeof(int), returnedProperty.PropertyType);
		}

		public void TestITypedList_GetItemProperties_WithRelatedCollection()
		{
			TestCustomTypeDescriptorBO bO = (TestCustomTypeDescriptorBO)Factory.New(typeof(TestCustomTypeDescriptorBO));

			PropertyDescriptor property = ((ITypedList)bO).GetItemProperties(null)["Z0_Number"];
			AssertNotNull("Should find a property on the business object", property);

			PropertyDescriptor selfProperty = bO.GetProperties()["Self"];
			PropertyDescriptor collectionProperty = bO.GetProperties()["Collection"];
			PropertyDescriptor propertyOnCollectionItem = ((ITypedList)bO).GetItemProperties(new PropertyDescriptor[] { selfProperty, collectionProperty })["Z0_ChildOnly"];
			AssertNotNull("Should find a property on the item of a related business object collection", propertyOnCollectionItem);
		}

		#endregion

		#region Test Classes

		class TestComponent : ZCustomTypeDescriptor
		{
			public TestComponent2 SomeProp
			{
				get { return null; }
			}
		}

		class TestComponent2
		{
			public int SomeProp2
			{
				get { return 0; }
			}
		}

		class TestCustomTypeDescriptorBO : DummyBusinessObject
		{
			public TestCustomTypeDescriptorBO(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		class TestDynamicBusinessObject : NonPersistentBusinessObject, IDynamicBusinessObject
		{
			public TestDynamicBusinessObject(BusinessObjectFactory factory) : base(factory)
			{
			}

			readonly Dictionary<string, DynamicBusinessObjectProperty> properties = new Dictionary<string, DynamicBusinessObjectProperty>();

			public string[] PropertyNames => properties.Keys.ToArray();

			public DynamicBusinessObjectProperty GetProperty(string propertyName)
			{
				return properties[propertyName];
			}

			public void AddProperty(string propertyName, DynamicBusinessObjectProperty newProperty)
			{
				properties.Add(propertyName, newProperty);
			}
		}

		#endregion

		#region BusinessObjects with Country TypeDeciders

		class TestDummyBusinessObject : AutoDummyBizo
		{
			public TestDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public static TestDummyBusinessObjectTypeDecider TypeDecider = new TestDummyBusinessObjectTypeDecider();
		}

		class TestDummyBusinessObjectSubclass1 : TestDummyBusinessObject
		{
			public TestDummyBusinessObjectSubclass1(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString Subclass1Property
			{
				get { return ""; }
			}
		}

		class TestDummyBusinessObjectSubclass2 : TestDummyBusinessObject
		{
			public TestDummyBusinessObjectSubclass2(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString Subclass2Property
			{
				get { return ""; }
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Testing")]
		class TestDummyBusinessObjectTypeDecider : TypeDecider
		{
			public Type TypeToReturn = typeof(TestDummyBusinessObject);

			public override Type GetTypeForBinding()
			{
				if (TypeToReturn == typeof(TestDummyBusinessObjectSubclass1))
				{
					return typeof(TestDummyBusinessObjectSubclass1);
				}
				else if (TypeToReturn == typeof(TestDummyBusinessObjectSubclass2))
				{
					return typeof(TestDummyBusinessObjectSubclass2);
				}
				else
				{
					throw new InvalidOperationException();
				}
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				return GetTypeForBinding();
			}

			public override Type GetTypeForNew()
			{
				return GetTypeForBinding();
			}
		}

		#endregion

		#region Implementation

		TestComponent Component
		{
			get { return component ?? (component = new TestComponent()); }
		}
		TestComponent component;

		#endregion
	}
}

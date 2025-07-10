using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CustomPropertyContainerExtensionsTest : TestCase
	{
		public void TestFindPropertyByIdentifier()
		{
			CustomPropertyContainerForTest container = new CustomPropertyContainerForTest();
			CustomPropertyForTest field1 = new CustomPropertyForTest { Identifier = "abc" };
			CustomPropertyForTest field2 = new CustomPropertyForTest { Identifier = "xyz" };

			AssertNull(container.FindPropertyByIdentifier("abc"));
			AssertNull(container.FindPropertyByIdentifier("xyz"));
			AssertNull(container.FindPropertyByIdentifier("Abc"));

			container.InnerCustomPropertyList.Add(field1);
			container.InnerCustomPropertyList.Add(field2);

			AssertEquals(field1, container.FindPropertyByIdentifier("abc"));
			AssertEquals(field2, container.FindPropertyByIdentifier("xyz"));
			AssertNull(container.FindPropertyByIdentifier("Abc"));
		}

		public void TestHasPropertyWithIdentifier()
		{
			CustomPropertyContainerForTest container = new CustomPropertyContainerForTest();
			CustomPropertyForTest field1 = new CustomPropertyForTest { Identifier = "abc" };
			CustomPropertyForTest field2 = new CustomPropertyForTest { Identifier = "xyz" };

			Assert(!container.HasPropertyWithIdentifier("abc"));
			Assert(!container.HasPropertyWithIdentifier("xyz"));
			Assert(!container.HasPropertyWithIdentifier("Abc"));

			container.InnerCustomPropertyList.Add(field1);
			container.InnerCustomPropertyList.Add(field2);

			Assert(container.HasPropertyWithIdentifier("abc"));
			Assert(container.HasPropertyWithIdentifier("xyz"));
			Assert(!container.HasPropertyWithIdentifier("Abc"));
		}

		public void TestHasPropertyWithIdentifierAndType()
		{
			CustomPropertyContainerForTest container = new CustomPropertyContainerForTest();
			CustomPropertyForTest field1 = new CustomPropertyForTest { Identifier = "abc", Info = new DynamicBusinessObjectProperty(typeof(int), false) };
			CustomPropertyForTest field2 = new CustomPropertyForTest { Identifier = "abc", Info = new DynamicBusinessObjectProperty(typeof(string), false) };

			Assert(!container.HasPropertyWithIdentifier("abc"));
			Assert(!container.HasPropertyWithIdentifier("abc"));

			container.InnerCustomPropertyList.Add(field1);
			container.InnerCustomPropertyList.Add(field2);

			Assert(container.HasPropertyWithIdentifier("abc"));
			Assert(container.HasPropertyWithIdentifier("abc"));
		}

		public void TestGetValue()
		{
			CustomPropertyContainerForTest container = new CustomPropertyContainerForTest();
			CustomPropertyForTest field1 = new CustomPropertyForTest { Identifier = "abc" };
			CustomPropertyForTest field2 = new CustomPropertyForTest { Identifier = "xyz" };

			field1.TrySetValue(null, "111");
			field2.TrySetValue(null, "222");

			AssertNull(container.GetValue(null, "abc"));
			AssertNull(container.GetValue(null, "xyz"));
			AssertNull(container.GetValue(null, "Abc"));

			container.InnerCustomPropertyList.Add(field1);
			container.InnerCustomPropertyList.Add(field2);

			AssertEquals("111", container.GetValue(null, "abc"));
			AssertEquals("222", container.GetValue(null, "xyz"));
			AssertNull(container.GetValue(null, "Abc"));
		}

		public void TestSetValue()
		{
			CustomPropertyContainerForTest container = new CustomPropertyContainerForTest();
			CustomPropertyForTest field1 = new CustomPropertyForTest { Identifier = "abc", Info = new DynamicBusinessObjectProperty(typeof(string), false) };
			CustomPropertyForTest field2 = new CustomPropertyForTest { Identifier = "xyz", Info = new DynamicBusinessObjectProperty(typeof(string), false) };

			container.InnerCustomPropertyList.Add(field1);
			container.InnerCustomPropertyList.Add(field2);

			AssertNull(field1.GetValue(null));
			AssertNull(field2.GetValue(null));

			Assert(container.TrySetValue(null, "abc", "111"));
			Assert(container.TrySetValue(null, "xyz", "222"));
			AssertEquals(container.TrySetValue(null, "Abc", "333"), false);

			AssertEquals("111", field1.GetValue(null));
			AssertEquals("222", field2.GetValue(null));
		}

		public void TestIsReadonly()
		{
			CustomPropertyContainerForTest container = new CustomPropertyContainerForTest();
			CustomPropertyForTest field1 = new CustomPropertyForTest { Identifier = "abc" };
			CustomPropertyForTest field2 = new CustomPropertyForTest { Identifier = "xyz" };

			Assert(container.IsReadonly("abc"));
			Assert(container.IsReadonly("xyz"));
			Assert(container.IsReadonly("Abc"));

			container.InnerCustomPropertyList.Add(field1);
			container.InnerCustomPropertyList.Add(field2);

			Assert(container.IsReadonly("abc"));
			Assert(container.IsReadonly("xyz"));
			Assert(container.IsReadonly("Abc"));

			field1.Info = new DynamicBusinessObjectProperty(typeof(object), false, metaData: Array.Empty<DynamicMetaData>());
			field2.Info = new DynamicBusinessObjectProperty(typeof(object), true, metaData: Array.Empty<DynamicMetaData>());

			Assert(!container.IsReadonly("abc"));
			Assert(container.IsReadonly("xyz"));
			Assert(container.IsReadonly("Abc"));
		}

		public void TestIsReadonly_CustomBusinessObject()
		{
			var customValuesStorage = new Dictionary<string, object>();

			Func<string, object> customPropertyGetter = propertyIdentifier =>
				{
					object value;
					return customValuesStorage.TryGetValue(propertyIdentifier, out value) ? value : null;
				};

			Func<string, object, bool> customPropertySetter = (propertyIdentifier, value) =>
				{
					customValuesStorage[propertyIdentifier] = value;
					return true;
				};

			var propertyCollection = new CustomPropertyCollectionImpl(customPropertyGetter, customPropertySetter);

			propertyCollection.Add(typeof(ZString), "ZZZ_String");
			propertyCollection.Add(typeof(ZBool), "ZZZ_Bool", true);
			propertyCollection.Add(typeof(ZDateTime), "ZZZ_DateTime");

			var customBusinessObject = new CustomBusinessObjectForTest(new BusinessObjectFactory(), propertyCollection);
			customBusinessObject.ReadOnlyPropertyIdentifiers.Add("ZZZ_DateTime");

			AssertEquals("ZZZ_String", false, customBusinessObject.IsReadonly("ZZZ_String"));
			AssertEquals("ZZZ_Bool is readonly due to metadata", true, customBusinessObject.IsReadonly("ZZZ_Bool"));
			AssertEquals("ZZZ_DateTime is readonly due to CustomBusinessObject", true, customBusinessObject.IsReadonly("ZZZ_DateTime"));
		}
	}
}

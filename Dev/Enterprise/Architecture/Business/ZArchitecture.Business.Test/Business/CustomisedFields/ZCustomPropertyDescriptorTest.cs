using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZCustomPropertyDescriptorTest : TestCase
	{
		public void TestGetValue()
		{
			ZCustomPropertyDescriptor propertyDescriptor1 = new ZCustomPropertyDescriptor("abc", typeof(string));
			ZCustomPropertyDescriptor propertyDescriptor2 = new ZCustomPropertyDescriptor("xyz", typeof(string));

			CustomPropertyContainerForTest container1 = new CustomPropertyContainerForTest();
			CustomPropertyForTest field1 = new CustomPropertyForTest { Identifier = "abc", Info = new DynamicBusinessObjectProperty(typeof(string), false) };

			CustomPropertyContainerForTest container2 = new CustomPropertyContainerForTest();
			CustomPropertyForTest field2 = new CustomPropertyForTest { Identifier = "xyz", Info = new DynamicBusinessObjectProperty(typeof(string), false) };

			field1.TrySetValue(null, "111");
			field2.TrySetValue(null, "222");

			AssertNull(propertyDescriptor1.GetValue(container1));
			AssertNull(propertyDescriptor2.GetValue(container1));

			AssertNull(propertyDescriptor1.GetValue(container2));
			AssertNull(propertyDescriptor2.GetValue(container2));

			container1.InnerCustomPropertyList.Add(field1);
			container2.InnerCustomPropertyList.Add(field2);

			AssertEquals("111", propertyDescriptor1.GetValue(container1));
			AssertNull(propertyDescriptor2.GetValue(container1));

			AssertNull(propertyDescriptor1.GetValue(container2));
			AssertEquals("222", propertyDescriptor2.GetValue(container2));
		}

		public void TestSetValue()
		{
			ZCustomPropertyDescriptor propertyDescriptor1 = new ZCustomPropertyDescriptor("abc", typeof(string));
			ZCustomPropertyDescriptor propertyDescriptor2 = new ZCustomPropertyDescriptor("xyz", typeof(string));

			CustomPropertyContainerForTest container1 = new CustomPropertyContainerForTest();
			CustomPropertyForTest field1 = new CustomPropertyForTest { Identifier = "abc", Info = new DynamicBusinessObjectProperty(typeof(string), false) };

			CustomPropertyContainerForTest container2 = new CustomPropertyContainerForTest();
			CustomPropertyForTest field2 = new CustomPropertyForTest { Identifier = "xyz", Info = new DynamicBusinessObjectProperty(typeof(string), false) };

			AssertNull(field1.GetValue(null));
			AssertNull(field2.GetValue(null));

			propertyDescriptor1.SetValue(container1, "111");
			propertyDescriptor2.SetValue(container1, "222");

			propertyDescriptor1.SetValue(container2, "333");
			propertyDescriptor2.SetValue(container2, "444");

			AssertNull(field1.GetValue(null));
			AssertNull(field2.GetValue(null));

			container1.InnerCustomPropertyList.Add(field1);
			container2.InnerCustomPropertyList.Add(field2);

			propertyDescriptor1.SetValue(container1, "111");
			propertyDescriptor2.SetValue(container1, "222");

			propertyDescriptor1.SetValue(container2, "333");
			propertyDescriptor2.SetValue(container2, "444");

			AssertEquals("111", field1.GetValue(null));
			AssertEquals("444", field2.GetValue(null));
		}

		public void TestIsReadOnlyOnComponent()
		{
			ZCustomPropertyDescriptor propertyDescriptor1 = new ZCustomPropertyDescriptor("abc", typeof(string));
			ZCustomPropertyDescriptor propertyDescriptor2 = new ZCustomPropertyDescriptor("xyz", typeof(string));

			CustomPropertyContainerForTest container1 = new CustomPropertyContainerForTest();
			CustomPropertyForTest field1 = new CustomPropertyForTest { Identifier = "abc" };

			CustomPropertyContainerForTest container2 = new CustomPropertyContainerForTest();
			CustomPropertyForTest field2 = new CustomPropertyForTest { Identifier = "xyz" };

			Assert(propertyDescriptor1.IsReadOnlyOnComponent(container1));
			Assert(propertyDescriptor2.IsReadOnlyOnComponent(container1));

			Assert(propertyDescriptor1.IsReadOnlyOnComponent(container2));
			Assert(propertyDescriptor2.IsReadOnlyOnComponent(container2));

			container1.InnerCustomPropertyList.Add(field1);
			container2.InnerCustomPropertyList.Add(field2);

			Assert(propertyDescriptor1.IsReadOnlyOnComponent(container1));
			Assert(propertyDescriptor2.IsReadOnlyOnComponent(container1));

			Assert(propertyDescriptor1.IsReadOnlyOnComponent(container2));
			Assert(propertyDescriptor2.IsReadOnlyOnComponent(container2));

			field1.Info = new DynamicBusinessObjectProperty(typeof(string), false, metaData: System.Array.Empty<DynamicMetaData>());

			Assert(!propertyDescriptor1.IsReadOnlyOnComponent(container1));
			Assert(propertyDescriptor1.IsReadOnlyOnComponent(container2));

			Assert(propertyDescriptor2.IsReadOnlyOnComponent(container1));
			Assert(propertyDescriptor2.IsReadOnlyOnComponent(container2));

			field2.Info = new DynamicBusinessObjectProperty(typeof(string), false, metaData: System.Array.Empty<DynamicMetaData>());

			Assert(!propertyDescriptor1.IsReadOnlyOnComponent(container1));
			Assert(propertyDescriptor1.IsReadOnlyOnComponent(container2));

			Assert(propertyDescriptor2.IsReadOnlyOnComponent(container1));
			Assert(!propertyDescriptor2.IsReadOnlyOnComponent(container2));

			field1.Info = new DynamicBusinessObjectProperty(typeof(string), true, metaData: System.Array.Empty<DynamicMetaData>());

			Assert(propertyDescriptor1.IsReadOnlyOnComponent(container1));
			Assert(propertyDescriptor1.IsReadOnlyOnComponent(container2));

			Assert(propertyDescriptor2.IsReadOnlyOnComponent(container1));
			Assert(!propertyDescriptor2.IsReadOnlyOnComponent(container2));
		}

		public void TestPropertyType()
		{
			ZCustomPropertyDescriptor propertyDescriptor1 = new ZCustomPropertyDescriptor("abc", typeof(ZString));
			ZCustomPropertyDescriptor propertyDescriptor2 = new ZCustomPropertyDescriptor("abc", typeof(ZInt));

			AssertEquals(typeof(ZString), propertyDescriptor1.PropertyType);
			AssertEquals(typeof(ZInt), propertyDescriptor2.PropertyType);
		}

		public void TestConverter()
		{
			ZCustomPropertyDescriptor propertyDescriptor1 = new ZCustomPropertyDescriptor("abc", typeof(ZString));
			ZCustomPropertyDescriptor propertyDescriptor2 = new ZCustomPropertyDescriptor("abc", typeof(ZInt));

			AssertEquals(typeof(ZStringTypeConverter), propertyDescriptor1.Converter.GetType());
			AssertEquals(typeof(ZIntTypeConverter), propertyDescriptor2.Converter.GetType());
		}
	}
}

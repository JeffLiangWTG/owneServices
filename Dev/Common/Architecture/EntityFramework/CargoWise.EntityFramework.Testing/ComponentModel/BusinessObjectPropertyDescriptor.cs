using System.ComponentModel;
using System.Data;
using System.Reflection;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectPropertyDescriptorTest : TestCaseWithDummy
	{
		public void TestGetValueWhenPropertyTypeIsBusinessObject()
		{
			var collection = BusinessObjectPropertyDescriptorCollection.FromType(typeof(TestBusinessObjectForPropertyType));
			var property = (BusinessObjectPropertyDescriptor)collection["TestBizo"];
			var testObject = Factory.New<TestBusinessObjectForPropertyType>();
			AssertNull(property.GetValue(testObject));
		}

		public void TestGetValueForMetaDataWhenPropertyTypeIsZType()
		{
			var collection = BusinessObjectPropertyDescriptorCollection.FromType(typeof(TestBusinessObjectForPropertyType));
			var property = (BusinessObjectPropertyDescriptor)collection["TestZTypeMemeber"];
			var testObject = Factory.New<TestBusinessObjectForPropertyType>();
			testObject.TestZTypeMemeber = 3;

			var dataValue = property.GetValue(testObject);
			AssertType<ZInt>(dataValue);
			AssertEquals(3, dataValue);

			var method = typeof(BusinessObjectPropertyDescriptor).GetMethod("GetValueForMetaData", BindingFlags.NonPublic | BindingFlags.Instance);
			var metaDataValue = method.Invoke(property, new object[] { testObject });
			AssertType<int>(metaDataValue);
			AssertEquals(3, metaDataValue);
		}

		public void TestGettingPropertyFromDeletedObject()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Delete();
			BusinessObjectPropertyDescriptor property = (BusinessObjectPropertyDescriptor)TypeDescriptor.GetProperties(dummy)[DummyBizoSchema.Z0_Code.Name];
			ZString deletedValue = (ZString)property.GetValue(dummy);
			AssertEquals("Getting a value from a deleted business object should produce no error and return an empty value", ZString.Empty, deletedValue);
		}

		class TestBusinessObjectForPropertyType : DummyBusinessObject
		{
			public TestBusinessObjectForPropertyType(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
			public BusinessObject TestBizo { get; set; }
			public ZInt TestZTypeMemeber { get; set; }
		}
	}
}

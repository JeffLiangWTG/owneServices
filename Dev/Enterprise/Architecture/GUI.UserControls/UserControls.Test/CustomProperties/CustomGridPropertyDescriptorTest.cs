using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class CustomGridPropertyDescriptorTest : TestCase
	{
		public void TestSetGet()
		{
			var descriptor = new CustomGridPropertyDescriptor(x => ((BusinessObjectWithCustomBusinessObject)x).CustomBusinessObject, "ZZZ_String1", typeof(ZString));
			IZPropertyInfoRetriever infoRetreiver = descriptor;

			var bizObj = new BusinessObjectWithCustomBusinessObject();

			descriptor.SetValue(bizObj, "Hello");
			AssertEquals("Hello", descriptor.GetValue(bizObj));
		}

		public void TestZPropertyInfoRetrieverImplementation()
		{
			var descriptor = new CustomGridPropertyDescriptor(x => ((BusinessObjectWithCustomBusinessObject)x).CustomBusinessObject, "ZZZ_String1", typeof(ZString));
			IZPropertyInfoRetriever infoRetreiver = descriptor;

			var bizObj = new BusinessObjectWithCustomBusinessObject();

			var propertyInfo = infoRetreiver.GetZPropertyInfo(bizObj);
			AssertNotNull(propertyInfo);
			AssertEquals("ZZZ_String1", propertyInfo.Name);
		}
	}
}

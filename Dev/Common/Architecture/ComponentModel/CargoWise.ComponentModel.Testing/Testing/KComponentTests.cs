#if DEBUG
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class KComponentTests : TestCase
	{
		public void TestGetPropertiesFromObject()
		{
			MockDComponent component = new MockDComponent();
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
			Assert(properties.Count > 0);
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(typeof(MockDComponent)))
			{
				Assert(properties[property.Name] is KPropertyDescriptor);
			}
		}

		class MockDComponent : KComponent
		{
			public int SomeProperty
			{
				get { return 3; }
			}
		}
	}
}
#endif

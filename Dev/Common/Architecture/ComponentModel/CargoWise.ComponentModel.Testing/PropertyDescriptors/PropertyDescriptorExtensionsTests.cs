#if DEBUG
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class PropertyDescriptorExtensionsTests : TestCase
	{
		#region HasSetter

		public void TestHasSetter()
		{
			AssertEquals("Property with getter and setter", true, ClassWithHasSetterProperties.Properties["PropertyWithGetterAndSetter"].HasSetter());
			AssertEquals("Property without setter", false, ClassWithHasSetterProperties.Properties["PropertyWithoutSetter"].HasSetter());
			AssertEquals("Property with protected setter", false, ClassWithHasSetterProperties.Properties["PropertyWithProtectedSetter"].HasSetter());
		}

		class ClassWithHasSetterProperties
		{
			public static KPropertyDescriptorCollection Properties
			{
				get { return KPropertyDescriptorCollection.FromType(typeof(ClassWithHasSetterProperties)); }
			}

			public string PropertyWithGetterAndSetter
			{
				get { return null; }
				set { }
			}

			public string PropertyWithoutSetter
			{
				get { return null; }
			}

			public string PropertyWithProtectedSetter
			{
				get { return null; }
				protected set { }
			}
		}

		#endregion
	}
}
#endif

#if DEBUG
using System;
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class CalculatedFromAttributeBaseTests : TestCase
	{
		public void TestGetAttributeOnOuterProperty()
		{
			KPropertyDescriptor property = (KPropertyDescriptor)TypeDescriptor.GetProperties(new TestComponent())["Related+RelatedProperty"];
			CalculatedFromAttribute attr = (CalculatedFromAttribute)property.GetAttributeFromMostSpecificComponentType(typeof(CalculatedFromAttribute));
			AssertEquals("Should prepend the property path", "Related.x", attr.PropertyNames[0]);
		}

		class TestComponent : KComponent
		{
			public TestRelatedComponent Related
			{ get { throw new NotSupportedException(); } }
		}

		abstract class TestRelatedComponent : KComponent
		{
			[CalculatedFrom("x")]
			public abstract int RelatedProperty { get; }
		}
	}
}
#endif

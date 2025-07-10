using System;
using System.ComponentModel;
using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoWrappedPropertyDescriptorTest : TestCaseWithFactory
	{
		public void TestGetValueWhenInnerThrowException()
		{
			DummyWithBadSelf badDummy = Factory.New<DummyWithBadSelf>();

			PropertyDescriptorCollection properties = badDummy.GetProperties();
			ZPropertyInfoWrappingPropertyDescriptor wpd = properties["Self+Z0_DateInfo"] as ZPropertyInfoWrappingPropertyDescriptor;
			AssertNotNull(wpd);

			badDummy.ThrowExceptionOnSelf = true;
			try
			{
				ZPropertyInfo propertyInfo = wpd.GetValue(badDummy) as ZPropertyInfo;
			}
			catch (Exception ex)
			{
				AssertEquals("ThrowExceptionOnSelf", ex.Message);
			}
		}

		class DummyWithBadSelf : DummyBusinessObject
		{
			public DummyWithBadSelf(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool ThrowExceptionOnSelf;

			public override DummyBaseBusinessObject Self
			{
				get
				{
					if (ThrowExceptionOnSelf)
					{
						throw new Exception("ThrowExceptionOnSelf");
					}

					return base.Self;
				}
			}
		}
	}
}

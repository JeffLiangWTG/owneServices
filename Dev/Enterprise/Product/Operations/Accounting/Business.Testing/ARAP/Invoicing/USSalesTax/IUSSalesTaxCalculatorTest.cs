using CargoWise.Application;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax.Implementation.Testing
{
	public class IUSSalesTaxCalculatorTest : TestCase
	{
		public void TestObjectFactory_ReturnsOverridableUSSalesTaxCalculator()
		{
			var calculator = ObjectFactory.Get<IUSSalesTaxCalculator>();
			AssertType<OverridableUSSalesTaxCalculator>("ObjectFactory should return OverridableUSSalesTaxCalculator", calculator);
		}

		public void TestObjectFactory_IUSSalesTaxCalculator_HasTransientScope()
		{
			var calculator1 = ObjectFactory.Get<IUSSalesTaxCalculator>();
			var calculator2 = ObjectFactory.Get<IUSSalesTaxCalculator>();
			var areSame = object.ReferenceEquals(calculator1, calculator2);
			Assert("ObjectFactory IUSSalesTaxCalculator should not be defined as singleton", !areSame);
		}

		public void TestObjectFactory_ReturnsEmpty_ForClientSpecific()
		{
			var list = ObjectFactory.Get<ListObject>("IUSSalesTaxCalculator_ClientSpecific");
			AssertNotNull("ObjectFactory should not return null", list);
			AssertEquals("ObjectFactory should return empty list", 0, list.Count);
		}

		public void TestObjectFactory_ForClientSpecific_HasSingletonScope()
		{
			var list1 = ObjectFactory.Get<ListObject>("IUSSalesTaxCalculator_ClientSpecific");
			var list2 = ObjectFactory.Get<ListObject>("IUSSalesTaxCalculator_ClientSpecific");
			AssertSame("ObjectFactory client specific IUSSalesTaxCalculator list should be defined as singleton", list1, list2);
		}
	}
}

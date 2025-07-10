using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Billing.Business.USSalesTax.Test
{
	public class AvalaraUSSalesTaxCalculatorFactoryTest : TestCaseWithFactory
	{
		public void TestGet_ReturnsNewInstance()
		{
			var factory = new AvalaraUSSalesTaxCalculatorFactory();
			var instance1 = factory.Get();
			var instance2 = factory.Get();

			var areSame = object.ReferenceEquals(instance1, instance2);
			Assert("Factory should return new instance on every call", !areSame);

			AssertType<AvalaraUSSalesTaxCalculator>(instance1);
			AssertType<AvalaraUSSalesTaxCalculator>(instance2);
		}
	}
}

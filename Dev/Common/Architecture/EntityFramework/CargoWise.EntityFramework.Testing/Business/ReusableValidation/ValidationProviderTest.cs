using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ValidationProviderTest : TestCaseWithDummy
	{
		class DummyValidation : ValidationProvider
		{
			public DummyValidation(BusinessObject @object) : base(@object)
			{
			}
		}

		[ExpectNoExceptions()]
		public void TestValidationProvider()
		{
			DummyValidation dummyValidation = new DummyValidation(Dummy);
		}
	}
}

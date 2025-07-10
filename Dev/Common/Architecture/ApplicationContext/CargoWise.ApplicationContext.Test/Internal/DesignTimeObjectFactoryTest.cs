using NUnit.Framework;

namespace CargoWise.Application.Testing
{
	class DesignTimeObjectFactoryTest : TestCase
	{
		public void TestGetDesignerSafe_WhenUsingConstructor()
		{
			DesignTimeObjectFactory.IsVisualStudio = true;
			try
			{
				ITestInterface reference = ObjectFactory.GetDesignerSafe<ITestInterface>();
				AssertNotNull("Should be able to manually load simple object configurations within the designer using AssemblyLoader", reference);
			}
			finally
			{
				DesignTimeObjectFactory.IsVisualStudio = false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Configure(IocConfigurationTests.TestConfigurationLocation);
		}

		protected override void TearDown()
		{
			ObjectFactory.Unconfigure(IocConfigurationTests.TestConfigurationLocation);
		}
	}
}

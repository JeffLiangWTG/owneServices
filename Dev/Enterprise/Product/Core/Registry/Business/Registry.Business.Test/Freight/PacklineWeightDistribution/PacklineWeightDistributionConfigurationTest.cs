using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Freight.PacklineWeightDistribution
{
	[TestedType(typeof(PacklineWeightDistributionConfiguration))]
	sealed class PacklineWeightDistributionConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultValue()
		{
			var config = new PacklineWeightDistributionConfiguration();
			AssertEquals(false, config.EnablePacklineWeightDistribution);
			AssertEquals(false, config.EnableActualWeightDistribution);
			AssertEquals(false, config.EnableVolumetricWeightDistribution);
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new PacklineWeightDistributionConfiguration();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new PacklineWeightDistributionConfiguration();
		}
	}
}

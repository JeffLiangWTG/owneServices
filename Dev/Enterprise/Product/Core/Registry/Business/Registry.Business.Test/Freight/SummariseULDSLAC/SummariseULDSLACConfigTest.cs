using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SummarizeULDSLACConfig))]
	sealed class SummariseULDSLACConfigTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		public void TestDefaultValues()
		{
			var config = new SummarizeULDSLACConfig();
			AssertEquals(ZBool.True, config.TotalSLAC);
			AssertEquals(ZBool.False, config.UseShipmentInners);
			AssertEquals(ZBool.False, config.TotalSLACInfo.ReadOnly);

			config.TotalSLAC = false;
			config.UseShipmentInners = true;
			AssertEquals(ZBool.True, config.TotalSLAC);
			AssertEquals(ZBool.True, config.TotalSLACInfo.ReadOnly);
		}

		public void TestCheckIdenticalConfigurationExists()
		{
			var configurations = new SummarizeULDSLACConfigCollection();
			var config = new SummarizeULDSLACConfig();
			configurations.Add(config);

			config.DestinationCountry = "AU";
			AssertNoRowError(config, SummarizeULDSLACConfig.IdenticalConfigurationExistsMessage);

			config = new SummarizeULDSLACConfig();
			configurations.Add(config);
			config.DestinationCountry = "AU";
			AssertHasRowError(config, SummarizeULDSLACConfig.IdenticalConfigurationExistsMessage);
		}

		public void TestDestinationCountry()
		{
			var config = new SummarizeULDSLACConfig();
			config.DestinationCountry = ZString.Empty;
			config.RunPreSaveValidation();
			AssertHasError(config.DestinationCountryInfo, "Please enter a value.");
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new SummarizeULDSLACConfig();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}

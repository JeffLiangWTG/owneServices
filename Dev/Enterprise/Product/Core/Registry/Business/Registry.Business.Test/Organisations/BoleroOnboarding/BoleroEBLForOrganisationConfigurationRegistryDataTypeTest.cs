using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Organisations
{
	[TestedType(typeof(BoleroEBLForOrganisationConfigurationRegistryDataType))]
	sealed class BoleroEBLForOrganisationConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BoleroEBLForOrganisationConfigurationRegistryDataType>
	{
		protected override BoleroEBLForOrganisationConfigurationRegistryDataType GetNewDataType()
		{
			return new BoleroEBLForOrganisationConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "BoleroEBLForOrganisationConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var boleroEBLForOrganisationConfiguration = new BoleroEBLForOrganisationConfiguration();
			boleroEBLForOrganisationConfiguration.EnableEBLIntegration = true;
			boleroEBLForOrganisationConfiguration.GalileoEndPointUrl = "http://test.test";
			boleroEBLForOrganisationConfiguration.GalileoAudience = "962E67D4-2A75-4404-BE21-43FF50ED5159";
			boleroEBLForOrganisationConfiguration.GalileoTestEndPointUrl = "http://test.test";
			boleroEBLForOrganisationConfiguration.GalileoTestAudience = "BE607423-ED7E-407C-9F8C-BDC4CA6EE9B1";
			boleroEBLForOrganisationConfiguration.Timeout = 60;

			return new[] { new ValidSampleAndBinaryValueInDB(boleroEBLForOrganisationConfiguration, DataType.Serialise(boleroEBLForOrganisationConfiguration)) };
		}
	}
}

using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Freight.Consol
{
	[TestedType(typeof(BoleroEBLConfigurationRegistryDataType))]
	sealed class BoleroEBLConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BoleroEBLConfigurationRegistryDataType>
	{
		protected override BoleroEBLConfigurationRegistryDataType GetNewDataType()
		{
			return new BoleroEBLConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "BoleroEBLConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var boleroEBLConfiguration = new BoleroEBLConfiguration();
			boleroEBLConfiguration.EnableEBLIntegration = true;
			boleroEBLConfiguration.GalileoEndPointUrl = "http://test.test";
			boleroEBLConfiguration.GalileoAudience = "962E67D4-2A75-4404-BE21-43FF50ED5159";
			boleroEBLConfiguration.GalileoTestEndPointUrl = "http://test.test";
			boleroEBLConfiguration.GalileoTestAudience = "BE607423-ED7E-407C-9F8C-BDC4CA6EE9B1";

			return new[] { new ValidSampleAndBinaryValueInDB(boleroEBLConfiguration, DataType.Serialise(boleroEBLConfiguration)) };
		}
	}
}

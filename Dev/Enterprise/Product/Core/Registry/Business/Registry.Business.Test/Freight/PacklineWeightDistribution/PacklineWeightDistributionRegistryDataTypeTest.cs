using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Freight.PacklineWeightDistribution
{
	[TestedType(typeof(PacklineWeightDistributionRegistryDataType))]
	sealed class PacklineWeightDistributionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PacklineWeightDistributionRegistryDataType>
	{
		protected override PacklineWeightDistributionRegistryDataType GetNewDataType()
		{
			return new PacklineWeightDistributionRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "PacklineWeightDistributionRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var config1 = new PacklineWeightDistributionConfiguration();
			config1.EnablePacklineWeightDistribution = false;
			config1.EnableActualWeightDistribution = false;
			config1.EnableVolumetricWeightDistribution = false;

			var config2 = new PacklineWeightDistributionConfiguration();
			config2.EnablePacklineWeightDistribution = true;
			config2.EnableActualWeightDistribution = true;
			config2.EnableVolumetricWeightDistribution = true;

			return new[] {
				new ValidSampleAndBinaryValueInDB(config1, GetNewDataType().Serialise(config1)),
				new ValidSampleAndBinaryValueInDB(config2, GetNewDataType().Serialise(config2))
			};
		}
	}
}

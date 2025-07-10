using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DpsMatchingConfigurationRegistryDataType))]
	sealed class DpsMatchingConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DpsMatchingConfigurationRegistryDataType>
	{
		protected override DpsMatchingConfigurationRegistryDataType GetNewDataType() => new DpsMatchingConfigurationRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var matchingConfiguration1 = new DpsMatchingConfigurationBusinessObject();
			var matchingConfiguration2 = new DpsMatchingConfigurationBusinessObject
			{
				MatchingConfiguration = nameof(MatchingRules.Balanced)
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(matchingConfiguration1, DataType.Serialise(matchingConfiguration1)),
				new ValidSampleAndBinaryValueInDB(matchingConfiguration2, DataType.Serialise(matchingConfiguration2)),
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "DpsMatchingConfigurationRegistryItemEditor"; }
		}
	}
}

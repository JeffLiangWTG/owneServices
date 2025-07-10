using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DpsConfidenceThresholdsRegistryDataType))]
	sealed class DpsConfidenceThresholdsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DpsConfidenceThresholdsRegistryDataType>
	{
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var thresholds1 = new DpsConfidenceThresholdsBusinessObject();
			var thresholds2 = new DpsConfidenceThresholdsBusinessObject { MediumThreshold = 75, HighThreshold = 90 };
			return new[]
			{
				new ValidSampleAndBinaryValueInDB(thresholds1, DataType.Serialise(thresholds1)),
				new ValidSampleAndBinaryValueInDB(thresholds2, DataType.Serialise(thresholds2)),
			};
		}
		protected override DpsConfidenceThresholdsRegistryDataType GetNewDataType() => new DpsConfidenceThresholdsRegistryDataType();

		protected override string ExpectedEditorName => "DpsConfidenceThresholdsRegistryItemEditor";
	}
}

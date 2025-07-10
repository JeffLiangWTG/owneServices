using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(Enterprise.Registry.Business.FeeChargeLevelsRegistryItem.FeeChargeLevelsRegistryDataType))]
	sealed class FeeChargeLevelsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<Enterprise.Registry.Business.FeeChargeLevelsRegistryItem.FeeChargeLevelsRegistryDataType>
	{
		protected override FeeChargeLevelsRegistryItem.FeeChargeLevelsRegistryDataType GetNewDataType()
		{
			return new FeeChargeLevelsRegistryItem.FeeChargeLevelsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "FeeChargeLevelsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var sample = new FeeChargeLevelsSection();
			byte[] byteArrayValue = new FeeChargeLevelsRegistryItem.FeeChargeLevelsRegistryDataType().Serialise(sample);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample, byteArrayValue)
			};
		}
	}
}

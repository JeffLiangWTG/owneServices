using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVEnablePartyScreeningRegistryDataType))]
	sealed class HVLVEnablePartyScreeningRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<HVLVEnablePartyScreeningRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "HVLVEnablePartyScreeningRegistryItemEditor"; }
		}

		protected override HVLVEnablePartyScreeningRegistryDataType GetNewDataType()
		{
			return new HVLVEnablePartyScreeningRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var result1 = new HVLVEnablePartyScreening();
			var result2 = new HVLVEnablePartyScreening() { EnableNewDPSResultForm = false };

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(result1, DataType.Serialise(result1)),
				new ValidSampleAndBinaryValueInDB(result2, DataType.Serialise(result2))
			};
		}
	}
}

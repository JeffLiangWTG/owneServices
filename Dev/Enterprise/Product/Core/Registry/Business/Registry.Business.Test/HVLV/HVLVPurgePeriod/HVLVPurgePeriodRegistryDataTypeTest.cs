using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVPurgePeriodRegistryDataType))]
	sealed class HVLVPurgePeriodRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<HVLVPurgePeriodRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "HVLVPurgePeriodRegistryItemEditor"; }
		}

		protected override HVLVPurgePeriodRegistryDataType GetNewDataType()
		{
			return new HVLVPurgePeriodRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var copy = new HVLVPurgePeriod();
			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(copy,DataType.Serialise(copy))
			};
		}
	}
}

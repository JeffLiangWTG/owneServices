using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVPreScreeningRuleRegistryDataType))]
	sealed class HVLVPreScreeningRuleRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<HVLVPreScreeningRuleRegistryDataType>
	{
		#region Implementation

		protected override HVLVPreScreeningRuleRegistryDataType GetNewDataType()
		{
			return new HVLVPreScreeningRuleRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "HVLVPreScreeningRuleRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var copy = new HVLVDetailsPreScreeningConfiguration();

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(copy, DataType.Serialise(copy))
			};
		}

		#endregion
	}
}

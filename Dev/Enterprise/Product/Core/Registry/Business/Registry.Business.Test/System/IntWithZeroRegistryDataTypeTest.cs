using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Registry.Business.SystemDataRegistry;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(IntWithZeroRegistryDataType))]
	sealed class IntWithZeroRegistryDataTypeTest : RegistryDataTypeTestCase<IntWithZeroRegistryDataType>
	{
		protected override IntWithZeroRegistryDataType GetNewDataType()
		{
			return new IntWithZeroRegistryDataType(0, 10);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var result = 10;
			var result2 = 5;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(result, new IntWithZeroRegistryDataType(0, 10).Serialise(result)),
				new ValidSampleAndBinaryValueInDB(result2, new IntWithZeroRegistryDataType(0, 10).Serialise(result2))
			};
		}
	}
}

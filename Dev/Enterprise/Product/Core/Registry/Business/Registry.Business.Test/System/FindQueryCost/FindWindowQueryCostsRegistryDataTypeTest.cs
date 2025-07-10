using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FindWindowQueryCostsRegistryDataType))]
	sealed class FindWindowQueryCostsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<FindWindowQueryCostsRegistryDataType>
	{
		protected override string ExpectedEditorName => "FindWindowQueryCostsRegistryItemEditor";
		protected override FindWindowQueryCostsRegistryDataType GetNewDataType() => new FindWindowQueryCostsRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var first = new FindWindowQueryCosts();
			var second = new FindWindowQueryCosts { AllowedCost = 123, MaximalCost = 456 };
			return new[]
			{
				new ValidSampleAndBinaryValueInDB(first, DataType.Serialise(first)),
				new ValidSampleAndBinaryValueInDB(second, DataType.Serialise(second))
			};
		}
	}
}

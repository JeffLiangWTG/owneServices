using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AddressMatchingLevelRegistryDataType))]
	sealed class AddressMatchingLevelRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AddressMatchingLevelRegistryDataType>
	{
		protected override AddressMatchingLevelRegistryDataType GetNewDataType() => new AddressMatchingLevelRegistryDataType();

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var matchingLevel1 = new AddressMatchingLevelBusinessObject();
			var matchingLevel2 = new AddressMatchingLevelBusinessObject
			{
				MatchingLevel = nameof(MatchingRules.Strict)
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(matchingLevel1, DataType.Serialise(matchingLevel1)),
				new ValidSampleAndBinaryValueInDB(matchingLevel2, DataType.Serialise(matchingLevel2)),
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "AddressMatchingLevelRegistryItemEditor"; }
		}
	}
}

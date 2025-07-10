using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CreateMissingProductsRegistryDataType))]
	sealed class CreateMissingProductsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CreateMissingProductsRegistryDataType>
	{
		protected override CreateMissingProductsRegistryDataType GetNewDataType()
		{
			return new CreateMissingProductsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "CreateMissingProductsItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var createMissingProductsInfo = new CreateMissingProductsInfo();
			createMissingProductsInfo.IsOverrideToYes = true;
			createMissingProductsInfo.DefaultRelationship = DefaultRalationshipCodes.Owner;

			var createMissingProductsInfo2 = new CreateMissingProductsInfo();
			createMissingProductsInfo2.IsOverrideToYes = true;
			createMissingProductsInfo2.DefaultRelationship = DefaultRalationshipCodes.Supplier;
			return new[]
			{
				new ValidSampleAndBinaryValueInDB(createMissingProductsInfo, DataType.Serialise(createMissingProductsInfo)),
				new ValidSampleAndBinaryValueInDB(createMissingProductsInfo2, DataType.Serialise(createMissingProductsInfo2))
			};
		}
	}
}

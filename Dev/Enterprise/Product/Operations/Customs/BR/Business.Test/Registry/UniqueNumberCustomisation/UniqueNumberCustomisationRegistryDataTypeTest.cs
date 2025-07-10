using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Registry.Testing
{
	[TestedType(typeof(UniqueNumberCustomisationRegistryDataType))]
	sealed class UniqueNumberCustomisationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<UniqueNumberCustomisationRegistryDataType>
	{
		protected override string ExpectedEditorName => "BillOfLadingNumberCustomisationRegistryItemEditor";

		protected override UniqueNumberCustomisationRegistryDataType GetNewDataType()
		{
			return new UniqueNumberCustomisationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var customisation = new UniqueNumberCustomisation() { RemoveFountainPrefix = true };
			var customisation2 = new UniqueNumberCustomisation();
			return new[]
			{
				new ValidSampleAndBinaryValueInDB(customisation, new UniqueNumberCustomisationRegistryDataType().Serialise(customisation)),
				new ValidSampleAndBinaryValueInDB(new UniqueNumberCustomisation(), new UniqueNumberCustomisationRegistryDataType().Serialise(customisation2))
			};
		}

		public void TestSupportsDirection()
		{
			var uniqueNumberCustomisationRegistryDataType = new UniqueNumberCustomisationRegistryDataType();
			AssertEquals("UniqueNumberCustomisationRegistryDataType should be true by default", true, uniqueNumberCustomisationRegistryDataType.SupportsDirection);
			AssertEquals("UniqueNumberCustomisationRegistryDataType should be true by default", true, uniqueNumberCustomisationRegistryDataType.DefaultValue.SupportsDirection);

			uniqueNumberCustomisationRegistryDataType = new UniqueNumberCustomisationRegistryDataType(false);
			AssertEquals("UniqueNumberCustomisationRegistryDataType should be true by default", false, uniqueNumberCustomisationRegistryDataType.SupportsDirection);
			AssertEquals("UniqueNumberCustomisationRegistryDataType should be true by default", false, uniqueNumberCustomisationRegistryDataType.DefaultValue.SupportsDirection);

			uniqueNumberCustomisationRegistryDataType.SupportsDirection = true;
			AssertEquals("UniqueNumberCustomisationRegistryDataType should be true by default", true, uniqueNumberCustomisationRegistryDataType.DefaultValue.SupportsDirection);
		}
	}
}

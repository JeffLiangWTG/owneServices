using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(DefaultFreightPercentagesRegistryDataType))]
	sealed class DefaultFreightPercentagesRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DefaultFreightPercentagesRegistryDataType>
	{
		protected override DefaultFreightPercentagesRegistryDataType GetNewDataType()
		{
			return new DefaultFreightPercentagesRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new DefaultFreightPercentageCollection();
			var item = collection.AddNew();
			item.ModeofTransport = "ROA";
			item.FreightPercentage = 0.12m;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new DefaultFreightPercentagesRegistryDataType().Serialise(collection))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "DefaultFreightPercentagesRegistryItemEditor"; }
		}
	}
}

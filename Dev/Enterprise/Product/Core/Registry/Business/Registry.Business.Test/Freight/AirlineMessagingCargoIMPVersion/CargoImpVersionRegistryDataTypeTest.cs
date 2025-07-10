using Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Freight.AirlineMessagingCargoIMPVersion
{
	[TestedType(typeof(CargoImpVersionRegistryDataType))]

	sealed class CargoImpVersionRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CargoImpVersionRegistryDataType>
	{
		protected override CargoImpVersionRegistryDataType GetNewDataType()
		{
			return new CargoImpVersionRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var config1 = new CargoImpVersionConfiguration();
			config1.DefaultImpVersion = "V16";
			var config2 = new CargoImpVersionConfiguration();
			config2.DefaultImpVersion = "V17";

			return new[] {
				new ValidSampleAndBinaryValueInDB(config1, GetNewDataType().Serialise(config1)),
				new ValidSampleAndBinaryValueInDB(config2, GetNewDataType().Serialise(config2))
			};
		}

		protected override string ExpectedEditorName
		{
			get { return "CargoImpVersionRegistryItemEditor"; }
		}
	}
}

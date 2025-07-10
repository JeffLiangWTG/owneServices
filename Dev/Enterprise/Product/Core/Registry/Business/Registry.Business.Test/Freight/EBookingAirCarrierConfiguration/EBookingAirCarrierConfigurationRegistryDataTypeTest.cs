using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EBookingAirCarrierConfigurationRegistryDataType))]
	sealed class EBookingAirCarrierConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<EBookingAirCarrierConfigurationRegistryDataType>
	{
		protected override bool HasEditor => true;

		protected override string ExpectedEditorName => "EBookingAirCarrierConfigurationRegistryItemEditor";

		protected override EBookingAirCarrierConfigurationRegistryDataType GetNewDataType()
		{
			return new EBookingAirCarrierConfigurationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var eBookingCarrierConfiguration1 = new EBookingCarrierConfiguration
			{
				LastUpdatedTime = new ZDateTime(2020, 6, 12),
				LastResponse = @"{
	""Test1"": 1
}"
			};

			var eBookingCarrierConfiguration2 = new EBookingCarrierConfiguration
			{
				LastUpdatedTime = new ZDateTime(2020, 6, 13),
				LastResponse = @"[""Test2"", ""Test 2""]"
			};
			return new[]
			{
				new ValidSampleAndBinaryValueInDB(eBookingCarrierConfiguration1, new EBookingAirCarrierConfigurationRegistryDataType().Serialise(eBookingCarrierConfiguration1)),
				new ValidSampleAndBinaryValueInDB(eBookingCarrierConfiguration2, new EBookingAirCarrierConfigurationRegistryDataType().Serialise(eBookingCarrierConfiguration2))
			};
		}
	}
}

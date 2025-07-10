using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.EU.H7.Business.UniversalDataTransfer.Test
{
	sealed class EUH7AsycudaManifestDataObjectReaderHelperTest : TestCaseWithFactory
	{
		public void TestManifestHeaderDataObjectReaderType()
		{
			var shipment = new Shipment();

			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvShipment.DataContext = DataContextFactory.New();
			hvlvShipment.DataContext.AddDataSource(DataContextType.HVLVConsignment, "Key");

			var manifestHeader = Factory.New<AsycudaManifestHeader>();

			var helper = new EUH7AsycudaManifestDataObjectReaderHelper("EU", Factory);

			var reader = helper.GetBillDataObjectReader(shipment, new DummyLogger(), new UniversalObjectFactory(), manifestHeader, helper, isUpdateEnabled: true);
			AssertEquals("Return normal reader for a non-hvlv shipment", true, typeof(AsycudaBillDataObjectReader).IsAssignableFrom(reader.GetType()));

			var hvlvReader = helper.GetBillDataObjectReader(hvlvShipment, new DummyLogger(), new UniversalObjectFactory(), manifestHeader, helper, isUpdateEnabled: true);
			Assert("Return EU H7 HVLV reader for a hvlv shipment", typeof(EUH7HVLVAsycudaBillDataObjectReader).IsAssignableFrom(hvlvReader.GetType()));
		}

		public void TestVehicleRegistrationMapped()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvShipment.TransportMode = new CodeDescriptionPair { Code = TransportTypeList.Codes.Road };
			hvlvShipment.VoyageFlightNo = "123";

			var helper = new EUH7AsycudaManifestDataObjectReaderHelper("EU", Factory);
			helper.FillManifestSpecificData(hvlvShipment, new DummyLogger(), header, new UniversalObjectFactory());

			AssertEquals("Vehicle Registration should be set", hvlvShipment.VoyageFlightNo, header.AMA_VehicleRegistration);
		}

		public void TestAgentTypeEmpty()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var hvlvShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvShipment.TransportMode = new CodeDescriptionPair { Code = TransportTypeList.Codes.Road };

			var helper = new EUH7AsycudaManifestDataObjectReaderHelper("EU", Factory);
			helper.FillManifestSpecificData(hvlvShipment, new DummyLogger(), header, new UniversalObjectFactory());

			AssertNullOrEmpty("Agent Type should be empty", header.AMA_AgentType);
		}
	}
}

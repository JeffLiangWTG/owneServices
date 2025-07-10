using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class TemporaryStorageHeaderValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestSettingAMA_MessageType()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.IsENSReuse = true;

			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			Assert(temporaryStorageHeader.IsENSReuse);

			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			Assert(temporaryStorageHeader.IsENSReuse);

			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			Assert(!temporaryStorageHeader.IsENSReuse);
		}

		public void TestChangeTransportType()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.Air;
			AssertEquals("When set AMA_TransportMode eqauls Air , the type is 41", MeansOfTransportList.Codes.RegistrationNumberOfTheAircraft, temporaryStorageHeader.TransportType);
			temporaryStorageHeader.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			AssertEquals("When set AMA_TransportMode eqauls Sea , the type is 10", MeansOfTransportList.Codes.ImoShipIdentificationNumber, temporaryStorageHeader.TransportType);
			temporaryStorageHeader.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.Rail;
			AssertEquals("When the length of TransportTypeList more than 1,the value of TransportType keep the last value", MeansOfTransportList.Codes.ImoShipIdentificationNumber, temporaryStorageHeader.TransportType);
		}
	}
}

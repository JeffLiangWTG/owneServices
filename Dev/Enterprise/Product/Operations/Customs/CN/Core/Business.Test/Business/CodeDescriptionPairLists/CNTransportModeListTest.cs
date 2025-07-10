using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNTransportModeListTest : TestCaseWithFactory
	{
		public void TestGetCorrespondingTransportCode()
		{
			AssertEquals(CNTransportModeList.Codes.Air, CNTransportModeList.GetCorrespondingTransportCode(Core.Constants.TransportModes.Air));
			AssertEquals(CNTransportModeList.Codes.Sea, CNTransportModeList.GetCorrespondingTransportCode(Core.Constants.TransportModes.Sea));
			AssertEquals(CNTransportModeList.Codes.Mail, CNTransportModeList.GetCorrespondingTransportCode(Core.Constants.TransportModes.Mail));
			AssertEquals(CNTransportModeList.Codes.Road, CNTransportModeList.GetCorrespondingTransportCode(Core.Constants.TransportModes.Road));
			AssertEquals(CNTransportModeList.Codes.Rail, CNTransportModeList.GetCorrespondingTransportCode(Core.Constants.TransportModes.Rail));
			AssertEquals(CNTransportModeList.Codes.FixedTransportInstallations, CNTransportModeList.GetCorrespondingTransportCode(Core.Constants.TransportModes.FixedTransportInstallations));
			AssertEquals(CNTransportModeList.Codes.PassengerCarried, CNTransportModeList.GetCorrespondingTransportCode(TransportTypeList.Codes.PassengerCarried));
		}
	}
}

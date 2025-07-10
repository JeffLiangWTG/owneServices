using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TransportTypeListTest : TestCaseWithFactory
	{
		public void TestGetTransportModeNumber()
		{
			AssertEquals("1", TransportTypeList.GetTransportModeNumber(TransportTypeList.Codes.Air));
			AssertEquals("2", TransportTypeList.GetTransportModeNumber(TransportTypeList.Codes.Road));
			AssertEquals("5", TransportTypeList.GetTransportModeNumber(TransportTypeList.Codes.Mail));
			AssertEquals("6", TransportTypeList.GetTransportModeNumber(TransportTypeList.Codes.Rail));
			AssertEquals("7", TransportTypeList.GetTransportModeNumber(TransportTypeList.Codes.FixedTransportInstallations));
			AssertEquals("8", TransportTypeList.GetTransportModeNumber(TransportTypeList.Codes.NoCarrier));
			AssertEquals("9", TransportTypeList.GetTransportModeNumber(TransportTypeList.Codes.Sea));
			AssertEquals("0", TransportTypeList.GetTransportModeNumber("TEST"));
		}
	}
}

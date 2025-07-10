using CargoWise.Types;
using Enterprise.Customs.Business.CustomsLists;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing.Business
{
	internal class BRTransportModeListTest : TestCase
	{
		public void TestMapBRTransportModeToCW1Code()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty Transport Mode to CW1Code", ZString.Empty, BRTransportModeList.MapBRTransportModeToCW1Code(""));
				AssertEquals("Mapping AIR to CW1Code", BRTransportModeList.Codes.AIR, BRTransportModeList.MapBRTransportModeToCW1Code(BRTransportModeList.Codes.AIR));
				AssertEquals("Mapping OTH to CW1Code", TransportTypeGenericList.Codes.Other, BRTransportModeList.MapBRTransportModeToCW1Code(BRTransportModeList.Codes.OTH));
				AssertEquals("Mapping FIC to CW1Code", TransportTypeGenericList.Codes.Other, BRTransportModeList.MapBRTransportModeToCW1Code(BRTransportModeList.Codes.FIC));
			});
		}

		public void TestGetTransportMeansForBRTransportMode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Get TransportMeans for Empty", ZString.Empty, BRTransportModeList.GetTransportMeansForBRTransportMode(""));
				AssertEquals("Get TransportMeans for AIR", ZString.Empty, BRTransportModeList.GetTransportMeansForBRTransportMode(BRTransportModeList.Codes.AIR));
				AssertEquals("Get TransportMeans for OTH", "O", BRTransportModeList.GetTransportMeansForBRTransportMode(BRTransportModeList.Codes.OTH));
				AssertEquals("Get TransportMeans for FIC", "F", BRTransportModeList.GetTransportMeansForBRTransportMode(BRTransportModeList.Codes.FIC));
			});
		}

		public void TestMapCW1CodeToBRTransportMode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Mapping AIR to BR Transport Mode", BRTransportModeList.Codes.AIR, BRTransportModeList.MapCW1CodeToBRTransportMode(BRTransportModeList.Codes.AIR, ""));
				AssertEquals("Mapping FIX to BR Transport Mode", BRTransportModeList.Codes.FIX, BRTransportModeList.MapCW1CodeToBRTransportMode(TransportTypeGenericList.Codes.FixedTransportInstallations, ""));
				AssertEquals("Mapping OTH/O to BR Transport Mode", BRTransportModeList.Codes.OTH, BRTransportModeList.MapCW1CodeToBRTransportMode(TransportTypeGenericList.Codes.Other, "O"));
				AssertEquals("Mapping OTH/F to BR Transport Mode", BRTransportModeList.Codes.FIC, BRTransportModeList.MapCW1CodeToBRTransportMode(TransportTypeGenericList.Codes.Other, "F"));
			});
		}
	}
}

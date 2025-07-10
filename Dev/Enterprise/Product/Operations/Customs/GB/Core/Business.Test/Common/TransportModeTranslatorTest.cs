using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	class TransportModeTranslatorTest : TestCase
	{
		public void TestTranslateToWCOCode_Air()
		{
			AssertEquals(ModeOfTransportList.Codes._4_AirTransport, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Air));
		}

		public void TestTranslateToWCOCode_Fixed()
		{
			AssertEquals(ModeOfTransportList.Codes._7_FixedTransportInstallations, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.FixedTransportInstallations));
		}

		public void TestTranslateToWCOCode_InlandWaterways()
		{
			AssertEquals(ModeOfTransportList.Codes._8_InlandWaterwayTransport, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.InlandWaterwayTransport));
		}

		public void TestTranslateToWCOCode_Mail()
		{
			AssertEquals(ModeOfTransportList.Codes._5_PostalConsignment, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Mail));
		}

		public void TestTranslateToWCOCode_Other()
		{
			AssertEquals(string.Empty, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Other));
		}

		public void TestTranslateToWCOCode_Rail()
		{
			AssertEquals(ModeOfTransportList.Codes._2_RailTransport, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Rail));
		}

		public void TestTranslateToWCOCode_Road()
		{
			AssertEquals(ModeOfTransportList.Codes._3_RoadTransport, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Road));
		}

		public void TestTranslateToWCOCode_RollOnRollOff()
		{
			AssertEquals(GBModeOfTransportList.Codes._6_RoRoFreight, transportModeTranslator.TranslateToWCOCode(GBTransportTypeList.Codes.ROR));
		}

		public void TestTranslateToWCOCode_Sea()
		{
			AssertEquals(ModeOfTransportList.Codes._1_SeaTransport, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Sea));
		}

		public void TestTranslateToWCOCode_OwnPropulsion()
		{
			AssertEquals(ModeOfTransportList.Codes._9_OwnPropulsion, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.OwnPropulsion));
		}

		public void TestTranslateToWCOCode_Unknown()
		{
			AssertEquals(string.Empty, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Unknown));
		}

		public void TestTranslateToWCOCode_ReturnInput()
		{
			AssertEquals("42", transportModeTranslator.TranslateToWCOCode("42", true));
		}

		public void TestTranslateToCargoWiseCode_Air()
		{
			AssertEquals(Core.Constants.TransportModes.Air, transportModeTranslator.TranslateToCargoWiseCode(ModeOfTransportList.Codes._4_AirTransport));
		}

		public void TestTranslateToCargoWiseCode_Fixed()
		{
			AssertEquals(Core.Constants.TransportModes.FixedTransportInstallations, transportModeTranslator.TranslateToCargoWiseCode(ModeOfTransportList.Codes._7_FixedTransportInstallations));
		}

		public void TestTranslateToCargoWiseCode_InlandWaterway()
		{
			AssertEquals(Core.Constants.TransportModes.InlandWaterwayTransport, transportModeTranslator.TranslateToCargoWiseCode(ModeOfTransportList.Codes._8_InlandWaterwayTransport));
		}

		public void TestTranslateToCargoWiseCode_Mail()
		{
			AssertEquals(Core.Constants.TransportModes.Mail, transportModeTranslator.TranslateToCargoWiseCode(ModeOfTransportList.Codes._5_PostalConsignment));
		}

		public void TestTranslateToCargoWiseCode_Other()
		{
			AssertEquals(string.Empty, transportModeTranslator.TranslateToCargoWiseCode(TransportModeCodeList.Codes.Other));
		}

		public void TestTranslateToCargoWiseCode_Rail()
		{
			AssertEquals(Core.Constants.TransportModes.Rail, transportModeTranslator.TranslateToCargoWiseCode(ModeOfTransportList.Codes._2_RailTransport));
		}

		public void TestTranslateToCargoWiseCode_Road()
		{
			AssertEquals(Core.Constants.TransportModes.Road, transportModeTranslator.TranslateToCargoWiseCode(ModeOfTransportList.Codes._3_RoadTransport));
		}

		public void TestTranslateToCargoWiseCode_RollOnRollOff()
		{
			AssertEquals(GBTransportTypeList.Codes.ROR, transportModeTranslator.TranslateToCargoWiseCode(GBModeOfTransportList.Codes._6_RoRoFreight));
		}

		public void TestTranslateToCargoWiseCode_Sea()
		{
			AssertEquals(Core.Constants.TransportModes.Sea, transportModeTranslator.TranslateToCargoWiseCode(ModeOfTransportList.Codes._1_SeaTransport));
		}

		public void TestTranslateToCargoWiseCode_OwnPropulsion()
		{
			AssertEquals(Core.Constants.TransportModes.OwnPropulsion, transportModeTranslator.TranslateToCargoWiseCode(ModeOfTransportList.Codes._9_OwnPropulsion));
		}

		public void TestTranslateToCargoWiseCode_ReturnInput()
		{
			AssertEquals("XXX", transportModeTranslator.TranslateToCargoWiseCode("XXX", true));
		}

		public void TestCargoWiseToWCO_ExpectedList()
		{
			var expectedValues = new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.FixedTransportInstallations, Core.Constants.TransportModes.InlandWaterwayTransport,
				Core.Constants.TransportModes.Mail, Core.Constants.TransportModes.OwnPropulsion, Core.Constants.TransportModes.Rail, Core.Constants.TransportModes.Road, GBTransportTypeList.Codes.ROR,
				Core.Constants.TransportModes.Sea };
			AssertContainsExactElementsInAnyOrder(expectedValues, transportModeTranslator.CargoWiseToWCOCodesForTest);
		}

		public void TestWCOToCargoWise_ExpectedList()
		{
			var expectedValues = new[] { TransportModeCodeList.Codes.Air, TransportModeCodeList.Codes.FixedInstallations, TransportModeCodeList.Codes.InlandWater, TransportModeCodeList.Codes.Mail,
				ModeOfTransportList.Codes._9_OwnPropulsion, TransportModeCodeList.Codes.Rail, TransportModeCodeList.Codes.Road, GBModeOfTransportList.Codes._6_RoRoFreight,
				TransportModeCodeList.Codes.Sea };
			AssertContainsExactElementsInAnyOrder(expectedValues, transportModeTranslator.WCOToCargoWiseCodesForTest);
		}

		protected override void SetUp()
		{
			base.SetUp();
			transportModeTranslator = new TransportModeTranslatorForTest();
		}
		TransportModeTranslatorForTest transportModeTranslator;

		class TransportModeTranslatorForTest : TransportModeTranslator
		{
			public string[] CargoWiseToWCOCodesForTest => CargoWiseToWCO.Keys.ToArray();

			public string[] WCOToCargoWiseCodesForTest => WCOToCargoWise.Keys.ToArray();
		}
	}
}

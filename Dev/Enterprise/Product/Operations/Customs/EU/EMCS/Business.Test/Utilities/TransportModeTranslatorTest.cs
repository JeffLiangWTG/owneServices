using System.Linq;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class TransportModeTranslatorTest : TestCase
	{
		public void TestTranslateToWCOCode_Air()
		{
			AssertEquals(TransportModeCodeList.Codes.Air, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Air));
		}

		public void TestTranslateToWCOCode_Fixed()
		{
			AssertEquals(TransportModeCodeList.Codes.FixedInstallations, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.FixedTransportInstallations));
		}

		public void TestTranslateToWCOCode_InlandWaterways()
		{
			AssertEquals(TransportModeCodeList.Codes.InlandWater, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.InlandWaterwayTransport));
		}

		public void TestTranslateToWCOCode_Mail()
		{
			AssertEquals(TransportModeCodeList.Codes.Mail, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Mail));
		}

		public void TestTranslateToWCOCode_Other()
		{
			AssertEquals(TransportModeCodeList.Codes.Other, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Other));
		}

		public void TestTranslateToWCOCode_Rail()
		{
			AssertEquals(TransportModeCodeList.Codes.Rail, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Rail));
		}

		public void TestTranslateToWCOCode_Road()
		{
			AssertEquals(TransportModeCodeList.Codes.Road, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Road));
		}

		public void TestTranslateToWCOCode_Sea()
		{
			AssertEquals(TransportModeCodeList.Codes.Sea, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Sea));
		}

		public void TestTranslateToWCOCode_Unknown()
		{
			AssertEquals(string.Empty, transportModeTranslator.TranslateToWCOCode(Core.Constants.TransportModes.Unknown));
		}

		public void TestTranslateToWCOCode_ReturnInput()
		{
			AssertEquals("YYY", transportModeTranslator.TranslateToWCOCode("YYY", true));
		}

		public void TestTranslateToCargoWiseCode_Air()
		{
			AssertEquals(Core.Constants.TransportModes.Air, transportModeTranslator.TranslateToCargoWiseCode(TransportModeCodeList.Codes.Air));
		}

		public void TestTranslateToCargoWiseCode_Fixed()
		{
			AssertEquals(Core.Constants.TransportModes.FixedTransportInstallations, transportModeTranslator.TranslateToCargoWiseCode(TransportModeCodeList.Codes.FixedInstallations));
		}

		public void TestTranslateToCargoWiseCode_InlandWaterway()
		{
			AssertEquals(Core.Constants.TransportModes.InlandWaterwayTransport, transportModeTranslator.TranslateToCargoWiseCode(TransportModeCodeList.Codes.InlandWater));
		}

		public void TestTranslateToCargoWiseCode_Mail()
		{
			AssertEquals(Core.Constants.TransportModes.Mail, transportModeTranslator.TranslateToCargoWiseCode(TransportModeCodeList.Codes.Mail));
		}

		public void TestTranslateToCargoWiseCode_Other()
		{
			AssertEquals(Core.Constants.TransportModes.Other, transportModeTranslator.TranslateToCargoWiseCode(TransportModeCodeList.Codes.Other));
		}

		public void TestTranslateToCargoWiseCode_Rail()
		{
			AssertEquals(Core.Constants.TransportModes.Rail, transportModeTranslator.TranslateToCargoWiseCode(TransportModeCodeList.Codes.Rail));
		}

		public void TestTranslateToCargoWiseCode_Road()
		{
			AssertEquals(Core.Constants.TransportModes.Road, transportModeTranslator.TranslateToCargoWiseCode(TransportModeCodeList.Codes.Road));
		}

		public void TestTranslateToCargoWiseCode_Sea()
		{
			AssertEquals(Core.Constants.TransportModes.Sea, transportModeTranslator.TranslateToCargoWiseCode(TransportModeCodeList.Codes.Sea));
		}

		public void TestTranslateToCargoWiseCode_Unkown()
		{
			AssertEquals(string.Empty, transportModeTranslator.TranslateToCargoWiseCode(TransportModeCodeList.Codes.Unknown));
		}

		public void TestTranslateToCargoWiseCode_ReturnInput()
		{
			AssertEquals("XXX", transportModeTranslator.TranslateToCargoWiseCode("XXX", true));
		}

		public void TestCargoWiseToWCO_ExpectedList()
		{
			var expectedValues = new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.FixedTransportInstallations, Core.Constants.TransportModes.InlandWaterwayTransport,
				Core.Constants.TransportModes.Mail, Core.Constants.TransportModes.Other, Core.Constants.TransportModes.Rail, Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Sea };
			AssertContainsExactElementsInAnyOrder(expectedValues, transportModeTranslator.CargoWiseToWCOCodesForTest);
		}

		public void TestWCOToCargoWise_ExpectedList()
		{
			var expectedValues = new[] { TransportModeCodeList.Codes.Air, TransportModeCodeList.Codes.FixedInstallations, TransportModeCodeList.Codes.InlandWater,
				TransportModeCodeList.Codes.Mail, TransportModeCodeList.Codes.Other, TransportModeCodeList.Codes.Rail, TransportModeCodeList.Codes.Road, TransportModeCodeList.Codes.Sea };
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

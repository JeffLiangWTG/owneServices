using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Business.Testing
{
	public class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestXmlEdiMessage()
		{
			foreach (var countryCode in ZArchitecture.Environment.Country.LicenceKeyBuilderSupportedCountryCodes)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var type = Decider.GetTypeForApplicationCode(EDIMessage.ApplicationCodes.UniversalDataMessaging, ((IBusinessObjectInternals)Factory.New<EDIMessage>()).Row, Factory);
					AssertEquals("XmlEDIMessage", type.Name);

					type = Decider.GetTypeForApplicationCode(EDIMessage.ApplicationCodes.XMS, ((IBusinessObjectInternals)Factory.New<EDIMessage>()).Row, Factory);
					AssertEquals("XmlEDIMessage", type.Name);

					type = Decider.GetTypeForApplicationCode(EDIMessage.ApplicationCodes.NativeDataMessaging, ((IBusinessObjectInternals)Factory.New<EDIMessage>()).Row, Factory);
					AssertEquals("XmlEDIMessage", type.Name);
				}
			}
		}

		public void TestCNMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var message = EDIMessageTestFactory.New(Factory);
				message.EM_MessageType = "XDC";
				message.EM_MessageSubType = "XUE";

				var testResult = Decider.GetTypeForApplicationCode("UDM", ((IBusinessObjectInternals)message).Row, Factory);
				AssertEquals("Should return CN Universal Event type.", "Enterprise.Messaging.Business.XmlMessaging.XmlEDIMessage", testResult.FullName);

				message.EM_MessageSubType = "XUS";
				testResult = Decider.GetTypeForApplicationCode("UDM", ((IBusinessObjectInternals)message).Row, Factory);
				AssertEquals("XmlEDIMessage", testResult.Name);
			}
		}

		public void TestUSCustomsDIS()
		{
			var type = Decider.GetTypeForApplicationCode(EDIMessage.ApplicationCodes.USCustomsDIS, ((IBusinessObjectInternals)EDIMessageTestFactory.New(Factory)).Row, Factory);
			AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.US.DIS.IEDIMessage>(), type);
		}

		public void TestAU()
		{
			Check(EDIMessage.ApplicationCodes.COLS, "COLSMessage");
			Check(EDIMessage.ApplicationCodes.EXDOC, "RFPMessage");
			Check(EDIMessage.ApplicationCodes.NEXDOCS, "REXMessage");
		}

		public void TestTraxon()
		{
			Check(EDIMessage.ApplicationCodes.Traxon, "TraxonMessage");
		}

		public void TestUSCustomsImport()
		{
			Check(EDIMessage.ApplicationCodes.USCustomsImport, "MQEDIMessage");
		}

		public void TestUSeBond()
		{
			Check(EDIMessage.ApplicationCodes.USeBond, "EBondEDIMessage");
		}

		public void TestCACustoms()
		{
			CheckWithMessageType(EDIMessage.ApplicationCodes.CACustoms, "DLMMessage", "DLM");
		}

		public void TestCAACI()
		{
			CheckWithMessageType(EDIMessage.ApplicationCodes.CAACI, "ACIEDIMessage", "ACI");
		}

		public void TestCAEXP()
		{
			CheckWithMessageType(EDIMessage.ApplicationCodes.CAEXP, "EXPEDIMessage", "CAX");
		}

		public void TestCAIMP()
		{
			CheckWithMessageType(EDIMessage.ApplicationCodes.CAIMP, "EDIMessage", "CAI");
		}

		public void TestGB()
		{
			Check(EDIMessage.ApplicationCodes.GbCcsuk, "GbEDIMessage");
			Check(EDIMessage.ApplicationCodes.GbEdifactShared, "GbEDIMessage");
			Check(EDIMessage.ApplicationCodes.GbNesAllMessageTypes, "GbEDIMessage");
			CheckNameContains(EDIMessage.ApplicationCodes.GbMessageICSGreatBritain, "GreatBritain");
			CheckNameContains(EDIMessage.ApplicationCodes.GbMessageICSNorthernIreland, "NorthernIreland");
			Check(EDIMessage.ApplicationCodes.GbCustomsDeclarationServices, "CDSEDIMessage");
			Check(EDIMessage.ApplicationCodes.GbCDSDISQuery, "CDSDISQueryMessage");
			CheckWithDirection(EDIMessage.ApplicationCodes.GbCustomsEMCS, "EMCSOutboundEDIMessage", EDIMessage.Direction.Transmit);
			CheckWithDirection(EDIMessage.ApplicationCodes.GbCustomsEMCS, "EMCSInboundEDIMessage", EDIMessage.Direction.Receive);
			CheckWithDirection(EDIMessage.ApplicationCodes.GbCustomsNCTS, "NCTSOutboundEDIMessage", EDIMessage.Direction.Transmit);
		}

		public void TestSG()
		{
			Check(EDIMessage.ApplicationCodes.SingaporeTradenet4, "CUSDECEDIMessage");
			Check(EDIMessage.ApplicationCodes.SGCustomsTradenetXML, "SGXmlEDIMessage");
			Check(EDIMessage.ApplicationCodes.SingaporeNationalTradePlatform, "CUSDECEDIMessage");
		}

		public void TestUSAMA()
		{
			Check(EDIMessage.ApplicationCodes.USAMA, "AIMEDIMessage");
		}

		public void TestES()
		{
			Check(EDIMessage.ApplicationCodes.ESCustomsMessage, "ESEDIMessage");
		}

		public void TestCIM()
		{
			Check(EDIMessage.ApplicationCodes.CIM, "CIMEDIMessage");
		}

		public void TestDECustomsAtlasSystem()
		{
			CheckWithMessageType(EDIMessage.ApplicationCodes.DECustomsAtlasSystem, "AtlasEDIMessage", "TST");
		}

		public void TestDECustomsAesSystem()
		{
			CheckWithMessageType(EDIMessage.ApplicationCodes.DECustomsAesSystem, "AesEDIMessage", "AES");
		}

		public void TestDECustomsEmcsSystem()
		{
			CheckWithMessageType(EDIMessage.ApplicationCodes.DECustomsEmcsSystem, "EmcsEDIMessage", "EMC");
		}

		public void TestIE()
		{
			CheckWithDirection(EDIMessage.ApplicationCodes.IECustomsImport, "AISOutboundEDIMessage", "TRX");
			CheckWithDirection(EDIMessage.ApplicationCodes.IECustomsImport, "AISInboundEDIMessage", "RCV");
			CheckWithDirection(EDIMessage.ApplicationCodes.IECustomsUCC5Import, "AISUCC5OutboundEDIMessage", "TRX");
			CheckWithDirection(EDIMessage.ApplicationCodes.IECustomsUCC5Import, "AISUCC5InboundEDIMessage", "RCV");
			CheckWithDirection(EDIMessage.ApplicationCodes.IECustomsExport, "AESOutboundEDIMessage", "TRX");
			CheckWithDirection(EDIMessage.ApplicationCodes.IECustomsExport, "AESInboundEDIMessage", "RCV");
			CheckWithDirection(EDIMessage.ApplicationCodes.IECustomsEMCS, "EMCSOutboundEDIMessage", "TRX");
			CheckWithDirection(EDIMessage.ApplicationCodes.IECustomsEMCS, "EMCSInboundEDIMessage", "RCV");
			CheckWithDirection(EDIMessage.ApplicationCodes.IECustomsNCTS, "NCTSOutboundEDIMessage", "TRX");
			CheckWithDirection(EDIMessage.ApplicationCodes.IECustomsNCTS, "NCTSInboundEDIMessage", "RCV");
			CheckWithDirection(EDIMessage.ApplicationCodes.IECustomsAndExcise, "CustomsAndExciseReportOutboundMessage", "TRX");
			CheckWithDirection(EDIMessage.ApplicationCodes.IECustomsAndExcise, "CustomsAndExciseReportInboundMessage", "RCV");
		}

		public void TestEU()
		{
			CheckWithDirection(EDIMessage.ApplicationCodes.IC2, "ICS2OutboundEDIMessage", "TRX");
			CheckWithDirection(EDIMessage.ApplicationCodes.IC2, "ICS2InboundEDIMessage", "RCV");
		}

		public void TestTW()
		{
			Check(EDIMessage.ApplicationCodes.TaiwanCustoms, "TWMessage");
		}

		public void TestJP()
		{
			Check(EDIMessage.ApplicationCodes.JPCustoms, "EDIMessage");
		}

		public void TestKR()
		{
			Check(EDIMessage.ApplicationCodes.KRCustoms, "EDIMessage", "Enterprise.Customs.KR.Business.EDIMessage");
		}

		public void TestNL()
		{
			Check(EDIMessage.ApplicationCodes.NLCustoms, "NLEDIMessage");
		}

		public void TestPL()
		{
			CombineAssertions(() =>
			{
				Check(EDIMessage.ApplicationCodes.PLCustoms, "EDIMessage", "Enterprise.Customs.PL.Business.EDIMessage");
				Check(EDIMessage.ApplicationCodes.PLCustomsPUESCEmailSystem, "EDIMessage", "Enterprise.Customs.PL.Business.EDIMessage");
				Check(EDIMessage.ApplicationCodes.PLCustomsNCTS, "EDIMessage", "Enterprise.Customs.PL.NCTS.Business.EDIMessage");
				Check(EDIMessage.ApplicationCodes.PLCustomsExitControl, "EDIMessage", "Enterprise.Customs.PL.ExitControl.Business.EDIMessage");
			});
		}

		public void TestCN()
		{
			Check(EDIMessage.ApplicationCodes.CNCustomsSingleWindow, "CNEDIMessage", "Enterprise.Customs.CN.Business.CNEDIMessage");
		}

		public void TestCH()
		{
			Check(EDIMessage.ApplicationCodes.CHCustomsEdec, "CHEDIMessage", "Enterprise.Customs.CH.Business.CHEDIMessage");
			Check(EDIMessage.ApplicationCodes.CHCustomsPassar, "CHEDIMessage", "Enterprise.Customs.CH.Business.CHEDIMessage");
			Check(EDIMessage.ApplicationCodes.CHCustomsCharteraOutput, "CHEDIMessage", "Enterprise.Customs.CH.Business.CHEDIMessage");
		}

		public void TestBE()
		{
			Check(EDIMessage.ApplicationCodes.BECustoms, "BEMessage");
		}

		public void TestIN()
		{
			Check(ApplicationCodeList.Codes.INCustoms, "EDIMessage", "Enterprise.Customs.IN.Business.EDIMessage");
		}

		public void TestIT()
		{
			const string expectedFullName = "Enterprise.Customs.IT.Business.ITEDIMessage";

			CombineAssertions(() =>
			{
				Check(ApplicationCodeList.Codes.ITCustoms, "ITEDIMessage", expectedFullName);
				Check(ApplicationCodeList.Codes.ITCustomsXTrade, "ITEDIMessage", expectedFullName);
			});
		}

		public void TestIL()
		{
			Check(EDIMessage.ApplicationCodes.ILCustoms, "ILEDIMessage", "Enterprise.Customs.IL.Business.ILEDIMessage");
		}

		public void TestAE()
		{
			Check(ApplicationCodeList.Codes.UAECustoms, "AEEDIMessage", "Enterprise.Customs.AE.Business.AEEDIMessage");
		}

		#region Implementation

		Type Check(string appCode, string className)
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = appCode;

			var type = Decider.GetTypeForApplicationCode(appCode, ((IBusinessObjectInternals)message).Row, Factory);

			Assert(type.Assembly != GetType().Assembly);
			AssertEquals(className, type.Name);
			return type;
		}

		void Check(string appCode, string className, string fullName)
		{
			var type = Check(appCode, className);
			AssertEquals(fullName, type.FullName);
		}

		void CheckNameContains(string appCode, string expectedPartialName)
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ApplicationCode = appCode;
			var type = Decider.GetTypeForApplicationCode(appCode, ((IBusinessObjectInternals)message).Row, Factory);
			AssertContains(expectedPartialName, type.FullName);
		}

		void CheckWithMessageType(string appCode, string className, string messageType)
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_MessageType = messageType;
			message.EM_ApplicationCode = appCode;

			var type = Decider.GetTypeForApplicationCode(appCode, ((IBusinessObjectInternals)message).Row, Factory);
			Assert(type.Assembly != GetType().Assembly);
			AssertEquals(className, type.Name);
		}

		void CheckWithDirection(string appCode, string className, string direction)
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_ReceiveTransmit = direction;
			message.EM_ApplicationCode = appCode;

			var type = Decider.GetTypeForApplicationCode(appCode, ((IBusinessObjectInternals)message).Row, Factory);
			Assert(type.Assembly != GetType().Assembly);
			AssertEquals(className, type.Name);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Decider = new EDIMessageTypeDecider();
		}

		EDIMessageTypeDecider Decider;

		#endregion
	}
}

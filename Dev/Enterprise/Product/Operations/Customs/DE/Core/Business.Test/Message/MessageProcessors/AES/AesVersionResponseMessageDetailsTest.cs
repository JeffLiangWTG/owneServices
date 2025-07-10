using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Testing;
using MessageDefinitionsAESVersion3_0 = CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class AesVersionResponseMessageDetailsTest : TestCaseWithFactory
	{
		public void TestResponseMessages()
		{
			AssertEquals(13, AesResponseMessageDetails.Instance.ResponseMessages.Count);
		}

		public void TestDEXPRF()
		{
			TestHelper.AssertResponseDetails(AesResponseMessageDetails.Instance.ResponseMessages[nameof(MessageDefinitionsAESVersion3_0.DEXPRF)],
				"CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.Incoming.dexprf.xsd",
				typeof(MessageDefinitionsAESVersion3_0.DEXPRF),
				typeof(Messaging.AESVersion3_0.EXPRELProvider),
				typeof(AesInboundEDIMessage<IEXPREL>));
		}

		public void TestDEXPLD()
		{
			TestHelper.AssertResponseDetails(AesResponseMessageDetails.Instance.ResponseMessages[nameof(MessageDefinitionsAESVersion3_0.DEXPLD)],
				"CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.Incoming.dexpld.xsd",
				typeof(MessageDefinitionsAESVersion3_0.DEXPLD),
				typeof(Messaging.AESVersion3_0.EXPCTLProvider),
				typeof(AesInboundEDIMessage<IEXPCTL>));
		}

		public void TestDEXPNF()
		{
			TestHelper.AssertResponseDetails(AesResponseMessageDetails.Instance.ResponseMessages[nameof(MessageDefinitionsAESVersion3_0.DEXPNF)],
				"CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.Incoming.dexpnf.xsd",
				typeof(MessageDefinitionsAESVersion3_0.DEXPNF),
				typeof(Messaging.AESVersion3_0.EXPNOTProvider),
				typeof(AesInboundEDIMessage<IEXPNOT>));
		}

		public void TestDEXPUC()
		{
			TestHelper.AssertResponseDetails(AesResponseMessageDetails.Instance.ResponseMessages[nameof(MessageDefinitionsAESVersion3_0.DEXPUC)],
				"CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.Incoming.dexpuc.xsd",
				typeof(MessageDefinitionsAESVersion3_0.DEXPUC),
				typeof(Messaging.AESVersion3_0.EXPURGProvider),
				typeof(AesInboundEDIMessage<IEXPURG>));
		}

		public void TestDEXQSB()
		{
			TestHelper.AssertResponseDetails(AesResponseMessageDetails.Instance.ResponseMessages[nameof(MessageDefinitionsAESVersion3_0.DEXQSB)],
				"CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.Incoming.dexqsb.xsd",
				typeof(MessageDefinitionsAESVersion3_0.DEXQSB),
				typeof(Messaging.AESVersion3_0.EXQSTAProvider),
				typeof(AesInboundEDIMessage<IEXQSTA>));
		}

		public void TestDEERRG()
		{
			TestHelper.AssertResponseDetails(AesResponseMessageDetails.Instance.ResponseMessages[nameof(MessageDefinitionsAESVersion3_0.DEERRG)],
				"CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.Incoming.deerrg.xsd",
				typeof(MessageDefinitionsAESVersion3_0.DEERRG),
				typeof(Messaging.AESVersion3_0.ERRNCKProvider),
				typeof(AesInboundEDIMessage<IERRNCK>));
		}

		public void TestDEXPSE()
		{
			TestHelper.AssertResponseDetails(AesResponseMessageDetails.Instance.ResponseMessages[nameof(MessageDefinitionsAESVersion3_0.DEXPSE)],
				"CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.Incoming.dexpse.xsd",
				typeof(MessageDefinitionsAESVersion3_0.DEXPSE),
				typeof(Messaging.AESVersion3_0.EXPSTAProvider),
				typeof(AesInboundEDIMessage<IEXPSTA>));
		}

		public void TestDEXPJE()
		{
			TestHelper.AssertResponseDetails(AesResponseMessageDetails.Instance.ResponseMessages[nameof(MessageDefinitionsAESVersion3_0.DEXPJE)],
				"CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.Incoming.dexpje.xsd",
				typeof(MessageDefinitionsAESVersion3_0.DEXPJE),
				typeof(Messaging.AESVersion3_0.EXPREJProvider),
				typeof(AesInboundEDIMessage<IEXPREJ>));
		}

		public void TestDEXPFE()
		{
			TestHelper.AssertResponseDetails(AesResponseMessageDetails.Instance.ResponseMessages[nameof(MessageDefinitionsAESVersion3_0.DEXPFE)],
				"CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.Incoming.dexpfe.xsd",
				typeof(MessageDefinitionsAESVersion3_0.DEXPFE),
				typeof(Messaging.AESVersion3_0.EXPFUPProvider),
				typeof(AesInboundEDIMessage<IEXPFUP>));
		}

		public void TestDEXTLF()
		{
			TestHelper.AssertResponseDetails(AesResponseMessageDetails.Instance.ResponseMessages[nameof(MessageDefinitionsAESVersion3_0.DEXTLF)],
				"CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.Incoming.dextlf.xsd",
				typeof(MessageDefinitionsAESVersion3_0.DEXTLF),
				typeof(Messaging.AESVersion3_0.EXTCTLProvider),
				typeof(AesInboundEDIMessage<IEXTCTL>)
				);
		}

		public void TestDEXTJE()
		{
			TestHelper.AssertResponseDetails(AesResponseMessageDetails.Instance.ResponseMessages[nameof(MessageDefinitionsAESVersion3_0.DEXTJE)],
				"CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.Incoming.dextje.xsd",
				typeof(MessageDefinitionsAESVersion3_0.DEXTJE),
				typeof(CargoWise.Customs.DE.MessageContracts.AESVersion3_0.EXTREJProvider),
				typeof(AesInboundEDIMessage<IEXTREJ>));
		}

		public void TestDEXTSE()
		{
			TestHelper.AssertResponseDetails(AesResponseMessageDetails.Instance.ResponseMessages[nameof(MessageDefinitionsAESVersion3_0.DEXTSE)],
				"CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.Incoming.dextse.xsd",
				typeof(MessageDefinitionsAESVersion3_0.DEXTSE),
				typeof(Messaging.AESVersion3_0.EXTSTAProvider),
				typeof(AesInboundEDIMessage<IEXTSTA>)
			);
		}

		public void TestDEXTDE()
		{
			TestHelper.AssertResponseDetails(AesResponseMessageDetails.Instance.ResponseMessages[nameof(MessageDefinitionsAESVersion3_0.DEXTDE)],
				"CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.Incoming.dextde.xsd",
				typeof(MessageDefinitionsAESVersion3_0.DEXTDE),
				typeof(CargoWise.Customs.DE.MessageContracts.AESVersion3_0.EXTDATProvider),
				typeof(AesInboundEDIMessage<IEXTDAT>)
			);
		}
	}
}

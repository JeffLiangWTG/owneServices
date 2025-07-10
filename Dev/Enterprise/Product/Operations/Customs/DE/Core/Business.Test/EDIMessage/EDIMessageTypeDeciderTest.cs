using System.Data;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using ATLASVersion10_1 = CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Business.Testing
{
	class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForDefault()
		{
			message.EM_MessageType = "ZZZ";
			AssertEquals(typeof(EDIMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForBinding()
		{
			AssertNull(typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertNull(typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForTemporaryStorageMessage()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_MessageType = EDIMessageTypeList.Codes.TemporaryStorage;
			message.EM_ApplicationReference = nameof(ATLASVersion10_1.SCCANE);
			AssertEquals(typeof(AtlasInboundEDIMessage<ICUSCAN>), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForTemporaryStorageDefault()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_MessageType = EDIMessageTypeList.Codes.TemporaryStorage;
			message.EM_MessageSubType = "ZZZ";
			AssertEquals(typeof(AtlasEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForNctsMessage()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_MessageType = EDIMessageTypeList.Codes.NCTS;
			message.EM_ApplicationReference = nameof(ATLASVersion10_1.DETQSC);
			AssertEquals(typeof(AtlasInboundEDIMessage<ITRQSTA>), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForNctsDefault()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_MessageType = EDIMessageTypeList.Codes.NCTS;
			message.EM_MessageSubType = "ZZZ";
			AssertEquals(typeof(AtlasEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForImportMessage()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_MessageType = EDIMessageTypeList.Codes.Import;
			message.EM_ApplicationReference = nameof(ATLASVersion10_1.GNTAXK);
			AssertEquals(typeof(AtlasInboundEDIMessage<INFFTAX>), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForImportDefault()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_MessageType = EDIMessageTypeList.Codes.Import;
			message.EM_MessageSubType = "ZZZ";
			AssertEquals(typeof(AtlasEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetCollectiveTypeForTemporaryStorageCUSREC()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_MessageType = EDIMessageTypeList.Codes.TemporaryStorage;
			message.EM_ApplicationReference = nameof(ATLASVersion10_1.GCRECF);
			AssertEquals(typeof(AtlasInboundEDIMessage<ICUSREC>), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetCollectiveTypeForTemporaryStorageERRNCK()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_MessageType = EDIMessageTypeList.Codes.TemporaryStorage;
			message.EM_ApplicationReference = nameof(ATLASVersion10_1.DEERRF);
			AssertEquals(typeof(AtlasInboundEDIMessage<IERRNCK>), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForAesDefault()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAesSystem;
			message.EM_MessageType = EDIMessageTypeList.Codes.AES;
			message.EM_ApplicationReference = "ZZZZZZ";
			AssertEquals(typeof(AesEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestAESStatusRequest()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAesSystem;
			message.EM_MessageType = EDIMessageTypeList.Codes.AES;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageSubType = ExportMessageSubTypeList.Codes.EXQ;
			AssertEquals(typeof(StatusRequest), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestNctsStatusRequest()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_MessageType = EDIMessageTypeList.Codes.NCTS;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageSubType = NctsMessageSubTypeList.Codes.StatusRequestMessage;
			AssertEquals(typeof(StatusRequest), typeDecider.GetTypeForLoad(row, Factory));
		}

		protected override void SetUp()
		{
			base.SetUp();
			typeDecider = new EDIMessageTypeDecider();
			message = Factory.New<EDIMessage>();
			row = ((INeedRow)message).Row;
		}
		EDIMessageTypeDecider typeDecider;
		EDIMessage message;
		DataRow row;
	}
}

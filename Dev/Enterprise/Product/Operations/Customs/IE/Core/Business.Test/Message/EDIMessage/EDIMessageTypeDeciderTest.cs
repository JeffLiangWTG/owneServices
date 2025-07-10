using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class EDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad_Default()
		{
			message.EM_ApplicationCode = "ZZZ";
			AssertEquals("Should return EDIMessage for unrecognized MessageType.", typeof(EDIMessage), typeDecider.GetTypeForLoad(row, Factory));
		}

		public void TestGetTypeForLoad_IECustomsImport()
		{
			CombineAssertions(() =>
			{
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsImport;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				AssertEquals("Outbound", typeof(AISOutboundEDIMessage), typeDecider.GetTypeForLoad(row, Factory));

				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				AssertEquals("Inbound", typeof(AISInboundEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			});
		}

		public void TestGetTypeForLoad_IECustomsImportUCC5()
		{
			CombineAssertions(() =>
			{
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsUCC5Import;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				AssertEquals("Outbound", typeof(AISUCC5OutboundEDIMessage), typeDecider.GetTypeForLoad(row, Factory));

				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				AssertEquals("Inbound", typeof(AISUCC5InboundEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			});
		}

		public void TestGetTypeForLoad_IECustomsExport()
		{
			CombineAssertions(() =>
			{
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				AssertEquals("Outbound", typeof(AESOutboundEDIMessage), typeDecider.GetTypeForLoad(row, Factory));

				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				AssertEquals("Inbound", typeof(AESInboundEDIMessage), typeDecider.GetTypeForLoad(row, Factory));
			});
		}

		public void TestGetTypeForLoad_IECustomsPBN()
		{
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsPBN;
			AssertEquals("Enterprise.Customs.IE.PBN.Business.PBNOutboundEDIMessage", typeDecider.GetTypeForLoad(row, Factory).FullName);
		}

		public void TestGetTypeForLoad_IECustomsAndExcise()
		{
			CombineAssertions(() =>
			{
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsAndExcise;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				AssertEquals("Outbound", typeof(CustomsAndExciseReportOutboundMessage), typeDecider.GetTypeForLoad(row, Factory));

				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				AssertEquals("Inbound", typeof(CustomsAndExciseReportInboundMessage), typeDecider.GetTypeForLoad(row, Factory));
			});
		}

		public void TestGetTypeForBinding()
		{
			AssertNull("GetTypeForBinding should return NULL.", typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertNull("GetTypeForNew should return NULL.", typeDecider.GetTypeForNew());
		}

		protected override void SetUp()
		{
			base.SetUp();
			typeDecider = new EDIMessageTypeDecider();
			message = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			row = ((INeedRow)message).Row;
		}

		EDIMessageTypeDecider typeDecider;
		Enterprise.Messaging.Business.EDIMessage message;
		DataRow row;
	}
}

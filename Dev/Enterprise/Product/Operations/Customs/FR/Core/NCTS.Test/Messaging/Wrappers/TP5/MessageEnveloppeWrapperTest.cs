using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class MessageEnveloppeWrapperTest : Customs.Business.Testing.DataProviderTestCase<MessageEnveloppeWrapper>
	{
		public void TestSchemaVersion()
		{
			AssertEquals("SchemaVersion should equal 1.0.", "1.0", Provider.SchemaVersion);
		}

		public void TestPartyId()
		{
			AssertEquals("For Departure header, PartyId should equal CZ_RepresentativeID of DTA agreement of the principal.", "123456789001", Provider.PartyId);

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			var orgHeader = Factory.New<OrgHeader>();
			var agreement = orgHeader.DeltaAgreementNumberCollection.AddNew();
			agreement.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			agreement.CZ_Type = OrgCusAccountDeltaTTypeList.Codes.TR;
			agreement.CZ_RepresentativeID = "ARR-PRN";
			header.DestinationTrader.OrganisationPK = orgHeader.PK;
			var provider = MessageEnveloppeWrapper.New(header, "IE015");

			AssertEquals("For Arrival header, PartyId should equal CZ_RepresentativeID of DTA agreement of the Destination Trader.", "ARR-PRN", provider.PartyId);
		}

		public void TestNumseq_Departure()
		{
			AssertEquals("Numseq should be calculated from nctsHeader Messages which direction is TRX and status is sent or Acknowedge - seq is 0.", "0", Provider.Numseq);

			var message = nctsHeader.Messages.AddNew();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIMessage.Status.Failed;
			AssertEquals("Numseq should be calculated from nctsHeader Messages which direction is TRX and status is sent or Acknowedge - message is transmit but is not SNT or ACK  - seq is 0.", "0", Provider.Numseq);

			var wrapper = createMessageWrapper(ReceiveTransmitList.Codes.Transmit, EDIMessage.Status.Acknowledged);
			AssertEquals("Numseq should be calculated from nctsHeader Messages which direction is TRX and status is sent or Acknowedge - message is transmit and ACK  - seq is 1.", "1", wrapper.Numseq);

			wrapper = createMessageWrapper(ReceiveTransmitList.Codes.Transmit, EDIMessage.Status.Sent);
			AssertEquals("Numseq should be calculated from nctsHeader Messages which direction is TRX and status is sent or Acknowedge - message is transmit and SNT  - seq is 2.", "2", wrapper.Numseq);

			wrapper = createMessageWrapper(ReceiveTransmitList.Codes.Receive, EDIMessage.Status.Sent);
			AssertEquals("Numseq should be calculated from nctsHeader Messages which direction is TRX and status is sent or Acknowedge - message is sent but not transmit - seq is 2.", "2", wrapper.Numseq);

			wrapper = createMessageWrapper(ReceiveTransmitList.Codes.Transmit, EDIMessage.Status.Sent);
			AssertEquals("Numseq should be calculated from nctsHeader Messages which direction is TRX and status is sent or Acknowedge - message is trnasmit and SNT  - seq is 3.", "3", wrapper.Numseq);

			MessageEnveloppeWrapper createMessageWrapper(string receiveTransmit, string status)
			{
				nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
				var messageForDeparture = nctsHeader.MovementHeader.Messages.AddNew();
				messageForDeparture.EM_ReceiveTransmit = receiveTransmit;
				messageForDeparture.EM_Status = status;
				messageForDeparture.EM_MessageType = "TP5";
				messageForDeparture.EM_MessageSubType = "015";
				var wrapper = MessageEnveloppeWrapper.New(nctsHeader, "IE015");
				return wrapper;
			}
		}

		public void TestNumseq_Arrival()
		{
			AssertEquals("Numseq should be calculated from nctsHeader Messages which direction is TRX and status is sent or Acknowedge - seq is 0.", "0", Provider.Numseq);

			var message = nctsHeader.Messages.AddNew();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIMessage.Status.Failed;
			AssertEquals("Numseq should be calculated from nctsHeader Messages which direction is TRX and status is sent or Acknowedge - message is transmit but is not SNT or ACK  - seq is 0.", "0", Provider.Numseq);

			var wrapper = createMessageWrapper(ReceiveTransmitList.Codes.Transmit, EDIMessage.Status.Acknowledged);
			AssertEquals("Numseq should be calculated from nctsHeader Messages which direction is TRX and status is sent or Acknowedge - message is transmit and ACK  - seq is 1.", "1", wrapper.Numseq);

			wrapper = createMessageWrapper(ReceiveTransmitList.Codes.Transmit, EDIMessage.Status.Sent);
			AssertEquals("Numseq should be calculated from nctsHeader Messages which direction is TRX and status is sent or Acknowedge - message is transmit and SNT  - seq is 2.", "2", wrapper.Numseq);

			wrapper = createMessageWrapper(ReceiveTransmitList.Codes.Receive, EDIMessage.Status.Sent);
			AssertEquals("Numseq should be calculated from nctsHeader Messages which direction is TRX and status is sent or Acknowedge - message is sent but not transmit - seq is 2.", "2", wrapper.Numseq);

			wrapper = createMessageWrapper(ReceiveTransmitList.Codes.Transmit, EDIMessage.Status.Sent);
			AssertEquals("Numseq should be calculated from nctsHeader Messages which direction is TRX and status is sent or Acknowedge - message is trnasmit and SNT  - seq is 3.", "3", wrapper.Numseq);

			MessageEnveloppeWrapper createMessageWrapper(string receiveTransmit, string status)
			{
				nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
				var messageForArrival = nctsHeader.Messages.AddNew();
				messageForArrival.EM_ReceiveTransmit = receiveTransmit;
				messageForArrival.EM_Status = status;
				messageForArrival.EM_MessageType = "TP5";
				messageForArrival.EM_MessageSubType = "007";
				var wrapper = MessageEnveloppeWrapper.New(nctsHeader, "IE007");
				return wrapper;
			}
		}

		public void TestTransactionId()
		{
			AssertEquals("TransactionId should equal CorrelationId Place Holder.", "~CORRELATIONID~", Provider.TransactionId);
		}

		public void TestSchemaId()
		{
			AssertEquals("TransactionId should equal message subtype.", "IE015", Provider.SchemaId);
		}

		protected override MessageEnveloppeWrapper GetProvider()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.LocalReferenceNumber = "LRN001";
			nctsHeader.BH_JobReference = "MRN012115a12315";
			Factory.Save();
			var principal = Factory.New<OrgHeader>();
			var agreement = principal.DeltaAgreementNumberCollection.AddNew();
			agreement.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			agreement.CZ_Type = OrgCusAccountDeltaTTypeList.Codes.TR;
			agreement.CZ_RepresentativeID = "123456789001";

			nctsHeader.Principal.OrganisationPK = principal.PK;

			return MessageEnveloppeWrapper.New(nctsHeader, "IE015");
		}

		NctsHeader nctsHeader;
	}
}

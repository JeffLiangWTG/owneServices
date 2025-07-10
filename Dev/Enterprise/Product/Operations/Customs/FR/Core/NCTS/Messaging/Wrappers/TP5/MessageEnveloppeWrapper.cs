using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class MessageEnveloppeWrapper : IMessageEnveloppe
	{
		MessageEnveloppeWrapper(NctsHeader nctsHeader, string schemaID)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			this.schemaID = schemaID;
		}
		readonly NctsHeader nctsHeader;
		readonly string schemaID;

		public static MessageEnveloppeWrapper New(NctsHeader nctsHeader, string schemaID) => nctsHeader == null ? null : new MessageEnveloppeWrapper(nctsHeader, schemaID);

		public string SchemaId => schemaID;

		public string SchemaVersion => "1.0";

		public string PartyId => partyId ?? (partyId = GetPartyId(nctsHeader));
		string partyId;

		public string TransactionId => transactionId ??= FR.Messaging.MessageBuilders.MessageBuilderBase<object>.CorrelationidPlaceholder;
		string transactionId;

		public string Numseq => numseq ?? (numseq = GetSequenceNumber(nctsHeader, schemaID));
		string numseq;

		string GetPartyId(NctsHeader nctsHeader)
		{
			var result = "";
			var party = nctsHeader.IsDepartureMovement ? nctsHeader.Principal?.Organisation : nctsHeader.DestinationTrader?.Organisation;
			if (party != null)
			{
				var account = party.DeltaAgreementNumberCollection.Cast<OrgCusAccount>().FirstOrDefault(x => x.CZ_Code == OrgCusAccountCodeList.Codes.DTA && x.CZ_Type == OrgCusAccountDeltaTTypeList.Codes.TR);
				if (account != null)
				{
					result = account.CZ_RepresentativeID;
				}
			}
			return result;
		}

		string GetSequenceNumber(NctsHeader frNctsHeader, string schemaID)
		{
			var isArrival = schemaID == "IE007" || schemaID == "IE044";
			IFRMessagesOwner messageOwner = isArrival ? frNctsHeader : frNctsHeader.MovementHeader;
			return messageOwner.Messages.Cast<EDIMessage>().Count(m => m.IsTransmitMessage && (m.EM_Status == EDIMessage.Status.Acknowledged || m.EM_Status == EDIMessage.Status.Sent)).ToString();
		}
	}
}

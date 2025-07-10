using CargoWise.Customs.FR.MessageDefinitions.ECS.Send.IE507;
using Enterprise.Customs.FR.Messaging.Interfaces.ECS;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.ECS
{
	public class ECSSendIE507MessageBuilder : MessageBuilderBase<TMessage>
	{
		public ECSSendIE507MessageBuilder(IECSData eCSData

			, TransactionTypes transactionType
			)
		{
			itemECS = eCSData;
			messageTransactionType = transactionType;
		}
		protected override TMessage GenerateDeclarationMessage()
		{
			var message = new TMessage();

			if (messageTransactionType == TransactionTypes.Original)
			{
				var datasDec = new Tie507();
				message.EnveloppeMessage = PopulateEnvelopMessage();

				datasDec.Entete = PopulateHeader();
				datasDec.Gen = populateGen();
				message.Declaration = datasDec;
			}

			return message;
		}

		public TEnveloppeMessage PopulateEnvelopMessage()
		{
			var enveloppeMessage = new TEnveloppeMessage();
			enveloppeMessage.SchemaId = itemECS.SchemaID;
			enveloppeMessage.SchemaVersion = itemECS.SchemaVersion;
			enveloppeMessage.PartyId = itemECS.PartyId;
			enveloppeMessage.TransactionId = itemECS.TransactionId;
			enveloppeMessage.Numseq = itemECS.Numseq;
			return enveloppeMessage;
		}

		public virtual TEntete507 PopulateHeader()
		{
			var header = new TEntete507();
			header.Codact = TEntete507Codact.Item1;
			return header;
		}

		public virtual TGen populateGen()
		{
			if (itemECS == null)
			{
				return null;
			}

			var messageGen = new TGen();
			messageGen.Mrnecs = itemECS.MRN;
			messageGen.Bureausortie = itemECS.Office;
			messageGen.Opeben = itemECS.EORI;
			messageGen.NumagrEcs = itemECS.Agreement;
			messageGen.Locagr = itemECS.Location;
			return messageGen;
		}

		readonly TransactionTypes messageTransactionType;
		readonly IECSData itemECS;
	}
}

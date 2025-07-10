using CargoWise.Customs.FR.MessageDefinitions.ECS.Send.IE618;
using Enterprise.Customs.FR.Messaging.Interfaces.ECS;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.ECS
{
	public class ECSSendIE618MessageBuilder : MessageBuilderBase<TMessage>
	{
		public ECSSendIE618MessageBuilder(IECSData eCSData
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
				var datasDec = new Tie618();
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

		public virtual TEntete618 PopulateHeader()
		{
			var header = new TEntete618();
			header.Codact = TEntete618Codact.Item1;
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
			messageGen.Numagr = itemECS.Agreement;
			messageGen.Locagr = itemECS.Location;
			return messageGen;
		}

		readonly TransactionTypes messageTransactionType;
		readonly IECSData itemECS;
	}
}

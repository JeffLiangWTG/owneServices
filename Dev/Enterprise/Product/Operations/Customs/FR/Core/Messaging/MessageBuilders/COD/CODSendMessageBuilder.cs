using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.COD.Send;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.COD;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.COD
{
	public class CODSendMessageBuilder : MessageBuilderBase<TMessage>
	{
		public CODSendMessageBuilder(ICOD codMessage, EU.Business.ErrorCollector errorCollectorObject, TransactionTypes transactionType)
		{
			header = codMessage;
			messageTransactionType = transactionType;
		}
		readonly ICOD header;
		readonly TransactionTypes messageTransactionType;

		protected override TMessage GenerateDeclarationMessage()
		{
			var message = new TMessage();
			if (messageTransactionType == TransactionTypes.Original)
			{
				message.EnveloppeMessage = PopulateEnvelopMessage();
				message.Declaration = new Declaration
				{
					Entete = PopulateHeader()
				};

				if (message.Declaration.Entete.Codact == "1")
				{
					message.Declaration.Articles = PopulateArticles();
				}
				else if (message.Declaration.Entete.Codact == "3")
				{
					message.Declaration.Gens = PopulateGens();
				}
			}
			return message;
		}

		TEnveloppeMessage PopulateEnvelopMessage()
		{
			TEnveloppeMessage envelopeMessage = null;
			var messageEnvelope = header.MessageEnvelope;

			if (messageEnvelope != null)
			{
				envelopeMessage = new TEnveloppeMessage();
				envelopeMessage.SchemaId = messageEnvelope.SchemaID;
				envelopeMessage.SchemaVersion = messageEnvelope.SchemaVersion;
				if (!messageEnvelope.PartnerId.IsEmpty)
				{
					envelopeMessage.PartyId = messageEnvelope.PartnerId;
				}
				envelopeMessage.TransactionId = messageEnvelope.TransactionId;
				envelopeMessage.Numseq = messageEnvelope.NumSeq;
			}
			return envelopeMessage;
		}

		TEntete PopulateHeader()
		{
			return new TEntete
			{
				Codact = header.ActionCode,
				Refdos = header.FileReference
			};
		}

		Collection<TArticle> PopulateArticles()
		{
			var articles = new Collection<TArticle>();
			if (header.Items != null)
			{
				foreach (var article in header.Items)
				{
					var tArticle = new TArticle();
					tArticle.Refdec = article.EntryNumber;
					tArticle.Typflux = article.Direction;
					tArticle.Numart = ZShort.ParseSafe(article.ItemNumber, 0);
					tArticle.Documents = PopulateDocuments(article);
					tArticle.Apur = PopulateApur(article);
					articles.Add(tArticle);
				}
			}
			return articles.Any() ? articles : null;
		}

		Collection<TDocAapurer> PopulateDocuments(IArticle article)
		{
			var documents = new Collection<TDocAapurer>();
			if (article.Documents != null)
			{
				foreach (var document in article.Documents)
				{
					var tDocAapurer = new TDocAapurer();
					tDocAapurer.Doc = document.DocumentCode;
					tDocAapurer.Refdoc = document.DocumentReference;
					documents.Add(tDocAapurer);
				}
			}
			return documents.Any() ? documents : null;
		}

		TApur PopulateApur(IArticle article)
		{
			TApur apur = null;
			if (article.Apur != null)
			{
				apur = new TApur();
				apur.ApurementRec = new TApurementRec();
				apur.ApurementRec.IndicateurApurement = article.Apur.IndicateurApurement ? "1" : "0";
				apur.ApurementRec.Mnt = article.Apur.Mnt > 0 ? (decimal?)article.Apur.Mnt : null;
				apur.ApurementRec.Refdecapur = article.Apur.Refdecapur;
			}
			return apur;
		}

		Collection<TGen> PopulateGens()
		{
			var gens = new Collection<TGen>();
			if (header.Gens != null)
			{
				foreach (var gen in header.Gens)
				{
					var tGen = new TGen();
					tGen.Refdec = gen.EntryNumber;
					tGen.Typflux = gen.Direction;
					tGen.Numcod = gen.Numcod;
					tGen.Operateur = new TGenOperateur();
					tGen.Operateur.Opecod = gen.Opecod;
					gens.Add(tGen);
				}
			}
			return gens.Any() ? gens : null;
		}
	}
}

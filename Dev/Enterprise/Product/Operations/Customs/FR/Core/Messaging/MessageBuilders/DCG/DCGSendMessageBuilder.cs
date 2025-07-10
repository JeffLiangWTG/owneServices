using System;
using System.Globalization;
using CargoWise.Customs.FR.MessageDefinitions.DCG.Send;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.Customs.FR.Messaging.Interfaces.DCG;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.DCG
{
	public class DCGSendMessageBuilder : MessageBuilderBase<TMessage>
	{
		public DCGSendMessageBuilder(IDCG header, ErrorCollector errorCollector, TransactionTypes transactionTypes)
		{
			this.header = header;
			this.errorCollector = errorCollector;
			this.transactionTypes = transactionTypes;
		}

		protected override TMessage GenerateDeclarationMessage()
		{
			var message = new TMessage();
			if (transactionTypes == TransactionTypes.Original)
			{
				message.EnveloppeMessage = PopulateEnvelopMessage();
				message.Declaration = new TDeclaration
				{
					Entete = PopulateHeader(),
					Gen = PopulateGen()
				};
			}
			return message;
		}

		TEnveloppeMessage PopulateEnvelopMessage()
		{
			TEnveloppeMessage enveloppe = null;
			if (header.MessageEnvelope is IMessageEnvelope messageEnvelope)
			{
				enveloppe = new TEnveloppeMessage
				{
					SchemaId = messageEnvelope.SchemaID,
					SchemaVersion = messageEnvelope.SchemaVersion,
					TransactionId = messageEnvelope.TransactionId.IsEmpty ? CorrelationidPlaceholder : (string)messageEnvelope.TransactionId,
					Numseq = Convert.ToInt16(messageEnvelope.NumSeq)
				};
				if (messageEnvelope.PartnerId.IsEmpty)
				{
					var deltaMode = "G2";
					var agreementType = header.Direction == "IMP" ? "DGI" : "DGE";
					errorCollector.AddError(MessageBuilderHelper.RepresentativeIdNotConfigured(deltaMode, agreementType));
				}
				else
				{
					enveloppe.PartyId = messageEnvelope.PartnerId;
				}
			}
			return enveloppe;
		}

		TEntete PopulateHeader()
		{
			var transactionId = header.MessageEnvelope?.TransactionId ?? ZString.Empty;
			return new TEntete
			{
				Refdos = transactionId.IsEmpty ? CorrelationidPlaceholder : (string)transactionId
			};
		}

		TGen PopulateGen()
		{
			var gen = new TGen
			{
				Numcre = header.DefermentAccountNumber,
				Paiement = header.PaymentType,
				Debutregul = header.PeriodStartDate.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture)
			};

			if (!header.DeltaAgreementNumber.IsEmpty)
			{
				gen.Numagr = header.DeltaAgreementNumber;
			}

			if (header.OperationalRepresentative.IsEmpty)
			{
				errorCollector.AddError(MessageBuilderHelper.CBRNotConfigured);
			}
			else
			{
				gen.Operep = header.OperationalRepresentative;
			}

			if (GetRefPeriodeRegul() is RefPeriodeRegul perioderegul)
			{
				gen.Perioderegul = perioderegul;
			}

			if (GetRefTypflux() is RefTypflux typflux)
			{
				gen.Typflux = typflux;
			}

			return gen;
		}

		RefPeriodeRegul? GetRefPeriodeRegul()
		{
			switch (header.Frequency)
			{
				case "J":
					return RefPeriodeRegul.J;
				case "D":
					return RefPeriodeRegul.D;
				case "M":
					return RefPeriodeRegul.M;
				default:
					return null;
			}
		}

		RefTypflux? GetRefTypflux()
		{
			switch (header.Direction)
			{
				case "IMP":
					return RefTypflux.Imp;
				case "EXP":
					return RefTypflux.Exp;
				default:
					return null;
			}
		}

		readonly IDCG header;
		readonly ErrorCollector errorCollector;
		readonly TransactionTypes transactionTypes;
	}
}

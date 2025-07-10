using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.CIN.Send;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.CIN
{
	public class CIN745ExportMessageBuilder : MessageBuilderBase<TMessage>
	{
		public CIN745ExportMessageBuilder(ICIN745ExportMessage cinMessageExport, EU.Business.ErrorCollector errorCollectorObject, TransactionTypes transactionType)
		{
			itemCINMessageExport = cinMessageExport;
			errorCollector = errorCollectorObject;
			messageTransactionType = transactionType;
		}

		protected override TMessage GenerateDeclarationMessage()
		{
			var message = new TMessage745();
			if (messageTransactionType == TransactionTypes.Original)
			{
				message.EnveloppeMessage = PopulateMessageEnveloppe();
				message.EnveloppeCIN = PopulateMessageCIN();
			}
			return message;
		}

		TEnveloppeMessage PopulateMessageEnveloppe()
		{
			var messageEnveloppe = new TEnveloppeMessage();

			if (itemCINMessageExport?.MessageEnvelope == null)
			{
				errorCollector.AddError(Res.GetString("E164A8EA-18C3-49F3-B982-25A167B6A9AE", "Message envelope information is not available."));
				return null;
			}

			messageEnveloppe.schemaID = itemCINMessageExport.MessageEnvelope.SchemaID;
			messageEnveloppe.schemaVersion = itemCINMessageExport.MessageEnvelope.SchemaVersion;
			messageEnveloppe.transactionId = itemCINMessageExport.MessageEnvelope.TransactionId;

			return messageEnveloppe;
		}

		TEnveloppeCIN745 PopulateMessageCIN()
		{
			var messageCIN = new TEnveloppeCIN745();
			if (itemCINMessageExport?.MessageCIN745 == null)
			{
				errorCollector.AddError(Res.GetString("3ED34A83-8168-4D1C-930F-D43F5711D131", "Message CIN 745 information is not available."));
				return null;
			}
			messageCIN.BUR_DOUANE = itemCINMessageExport.MessageCIN745.BUR_DOUANE;
			messageCIN.DEST_OACI = itemCINMessageExport.MessageCIN745.DEST_OACI;
			messageCIN.MRN_ECS = itemCINMessageExport.MessageCIN745.MRN_ECS;
			messageCIN.OACI = itemCINMessageExport.MessageCIN745.OACI;
			messageCIN.REFERENCE = itemCINMessageExport.MessageCIN745.REFERENCE;
			messageCIN.MAGASIN = itemCINMessageExport.MessageCIN745.MAGASIN;
			messageCIN.EDIFACTCIN = PopulateEDIFACTCIN();
			return messageCIN;
		}
		XMLCIN PopulateEDIFACTCIN()
		{
			return new XMLCIN
			{
				Message745 = PopulateMessage745()
			};
		}
		CINMessage745 PopulateMessage745()
		{
			var cinMessage745 = new CINMessage745();
			cinMessage745.Messages = PopulateMessages();
			cinMessage745.EnvelopeConnexion = PopulateEnvelopeConnexion();
			return cinMessage745;
		}
		EnvelopeConnexion PopulateEnvelopeConnexion()
		{
			var envelopeConnexion = new EnvelopeConnexion
			{
				date = itemCINMessageExport.MessageCIN745.Date.ToString("yyyy-MM-dd"),
				sender = itemCINMessageExport.MessageCIN745.OACI,
				time = itemCINMessageExport.MessageCIN745.Time.ToString("hh:mm:ss")
			};
			return envelopeConnexion;
		}
		Messages PopulateMessages()
		{
			var messages = new Messages();
			messages.Message = PopulateMessage();
			return messages;
		}
		Message PopulateMessage()
		{
			var message = new Message();
			message.EnvelopeMessage = PopulateEnveloppeMessage();
			message.MessageBody = PopulateMessageBody();
			return message;
		}
		MRN[] PopulateCINMRNS()
		{
			var mrns = new List<MRN>();
			var mrn = new MRN
			{
				Level = itemCINMessageExport.MessageCIN745.Level,
				Name = itemCINMessageExport.MessageCIN745.Name,
				ExitOfOffice = itemCINMessageExport.MessageCIN745.ExitOfOffice,
				MrnNumber = itemCINMessageExport.MessageCIN745.MrnNumber,
				MrnQuantity = itemCINMessageExport.MessageCIN745.MrnQuantity,
				MrnQuantitySpecified = itemCINMessageExport.MessageCIN745.MrnQuantity != 0,
				MrnWeight = itemCINMessageExport.MessageCIN745.MrnWeight,
				MrnWeightSpecified = itemCINMessageExport.MessageCIN745.MrnWeight != 0
			};
			mrns.Add(mrn);
			return mrns.Any() ? mrns.ToArray() : null;
		}
		EnvelopeMessage PopulateEnveloppeMessage()
		{
			var envelopeMessage = new EnvelopeMessage
			{
				schemaId = itemCINMessageExport.MessageCIN745.SchemaID,
				transactionId = itemCINMessageExport.MessageCIN745.TransactionID,
				schemaVersion = itemCINMessageExport.MessageCIN745.SchemaVersion,
			};
			return envelopeMessage;
		}
		MessageBody PopulateMessageBody()
		{
			var messageBody = new MessageBody()
			{
				Header = PopulateCINHeader(),
				MRNS = PopulateCINMRNS()
			};
			return messageBody;
		}
		Header PopulateCINHeader()
		{
			var header = new Header();
			header.MessageTo = itemCINMessageExport.MessageCIN745.OACI_Shipper;
			header.MessageFrom = itemCINMessageExport.MessageCIN745.OACI_Carrier;
			return header;
		}
		readonly ICIN745ExportMessage itemCINMessageExport;
		readonly TransactionTypes messageTransactionType;
		readonly EU.Business.ErrorCollector errorCollector;
	}
}

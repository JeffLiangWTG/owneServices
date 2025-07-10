using CargoWise.Customs.FR.MessageDefinitions.CIN.Send;
using Enterprise.Customs.FR.Messaging.Interfaces.CIN;

namespace Enterprise.Customs.FR.Messaging.MessageBuilders.CIN
{
	public class CIN755ExportMessageBuilder : MessageBuilderBase<TMessage>
	{
		public CIN755ExportMessageBuilder(ICIN755ExportMessage cinMessageExport, EU.Business.ErrorCollector errorCollectorObject, TransactionTypes transactionType)
		{
			itemCINMessageExport = cinMessageExport;
			errorCollector = errorCollectorObject;
			messageTransactionType = transactionType;
		}
		protected override TMessage GenerateDeclarationMessage()
		{
			var message = new TMessage755();

			if (messageTransactionType == TransactionTypes.Original)
			{
				message.EnveloppeMessage = PopulateMessageEnvelope();
				message.EnveloppeCIN = PopulateCINMessageEnvelope();
			}

			return message;
		}

		#region Populate
		TEnveloppeMessage PopulateMessageEnvelope()
		{
			var messageEnvelop = new TEnveloppeMessage();

			if (itemCINMessageExport?.MessageEnvelope == null)
			{
				errorCollector.AddError(Res.GetString("B802A8F2-298A-4945-AE3D-84BBEFEC4989", "Message envelope information is not available."));
				return null;
			}

			messageEnvelop.schemaID = itemCINMessageExport.MessageEnvelope.SchemaID;
			messageEnvelop.schemaVersion = itemCINMessageExport.MessageEnvelope.SchemaVersion;
			messageEnvelop.transactionId = itemCINMessageExport.MessageEnvelope.TransactionId;

			return messageEnvelop;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		TEnveloppeCIN755 PopulateCINMessageEnvelope()
		{
			var cinMessageEnvelop = new TEnveloppeCIN755();

			if (itemCINMessageExport?.NestedCINMessageEnvelope == null)
			{
				errorCollector.AddError(Res.GetString("848CB832-7D40-4906-87A5-090E5A81E06E", "Nested CIN message envelope information is not available."));
				return null;
			}

			cinMessageEnvelop.OACI = itemCINMessageExport.NestedCINMessageEnvelope.OACI;
			cinMessageEnvelop.REFERENCE = itemCINMessageExport.NestedCINMessageEnvelope.REFERENCE;
			cinMessageEnvelop.MRN_ECS = itemCINMessageExport.NestedCINMessageEnvelope.MRN_ECS;
			cinMessageEnvelop.MAGASIN = itemCINMessageExport.NestedCINMessageEnvelope.MAGASIN;
			cinMessageEnvelop.BUR_DOUANE = itemCINMessageExport.NestedCINMessageEnvelope.BUR_DOUANE;
			cinMessageEnvelop.DEST_OACI = itemCINMessageExport.NestedCINMessageEnvelope.DEST_OACI;
			cinMessageEnvelop.NUM_LTA = itemCINMessageExport.NestedCINMessageEnvelope.NUM_LTA;
			cinMessageEnvelop.EDIFACTCIN = itemCINMessageExport.NestedCINMessageEnvelope.CIN;

			var shouldReturnNull = false;
			if (string.IsNullOrEmpty(cinMessageEnvelop.NUM_LTA))
			{
				errorCollector.AddError(Res.GetString("89A107B5-8E27-4E3C-9532-A6EA58619812", "MAWB is empty."));
				shouldReturnNull = true;
			}
			if (string.IsNullOrEmpty(cinMessageEnvelop.MRN_ECS))
			{
				errorCollector.AddError(Res.GetString("0AC0CF5F-B8B5-49DB-80DF-3971321BAACA", "MRN is empty."));
				shouldReturnNull = true;
			}
			if (string.IsNullOrEmpty(cinMessageEnvelop.MAGASIN))
			{
				errorCollector.AddError(Res.GetString("F8EE3D5B-FC55-4C3D-AD88-E78756BCFB7F", "MAGASIN is empty."));
				shouldReturnNull = true;
			}

			if (shouldReturnNull)
			{
				return null;
			}

			return cinMessageEnvelop;
		}
		#endregion
		readonly ICIN755ExportMessage itemCINMessageExport;
		readonly TransactionTypes messageTransactionType;
		readonly EU.Business.ErrorCollector errorCollector;
	}
}

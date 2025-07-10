using System;
using System.Globalization;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public static class MessageProcessorHelper
	{
		public static CusEntryHeader GetRelevantBusinessObjectFromMRNCode<TResponse>(EDIMessage message, ZString xsdSchemaEmbeddedResourceName, BusinessObject[] sentBusinessObjects, bool serializeWithValidation = true, bool isAES = false, bool isNCTS = false)
			where TResponse : class, IMRNField
		{
			try
			{
				if (message.EM_MessageText.IsEmpty)
				{
					throw new InvalidOperationException(Res.GetString("4190074D-318A-4331-B387-6C869AAC88BE", "Message Text is empty so can't continue with processing"));
				}
				ZString mrnCode;
				using (var textReader = message.GetEM_MessageTextReader())
				{
					using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
					{
						if (serializeWithValidation)
						{
							var response = ESXmlObjectSerializer.DeserializeWithValidation<TResponse>(xsdSchemaEmbeddedResourceName, bodyTextReader);
							mrnCode = response.MRN;
						}
						else
						{
							var response = ESXmlObjectSerializer.DeserializeWithoutValidation<TResponse>(xsdSchemaEmbeddedResourceName, bodyTextReader, isAES, isNCTS);
							mrnCode = response.MRN;
						}
					}
				}

				return !mrnCode.IsEmpty ? sentBusinessObjects.Cast<CusEntryHeader>().FirstOrDefault(x => x.MovementReferenceNumber == mrnCode) : null;
			}
			catch (Exception ex) when (ex is XmlSchemaValidationException || ex is InvalidOperationException)
			{
				throw;
			}
		}

		public static T GetRelevantBusinessObjectForEmailResponse<T>(EDIMessage message, ZString entryTypeToSearch, ZDBOnlyQuery extraQuery = null)
			where T : BusinessObject
		{
			var entryNumberFilter = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryNum, message.EM_ApplicationReference);
			entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, entryTypeToSearch);
			entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Spain);

			var query = new ZDBOnlyQuery(typeof(T));
			query.AddSubQuery(entryNumberFilter, JoinCondition.And);
			if (extraQuery != null)
			{
				query.AddToFilter(extraQuery, JoinCondition.And);
			}
			var entryHeaderReturned = message.Factory.LoadTop1<T>(query);
			return entryHeaderReturned;
		}

		public static EDIInterchange GetRelatedSentInterchange(EDIMessage message)
		{
			EDIInterchange sentInterchange = null;
			var trackingID = message.Interchange?.EI_SessionGUID ?? ZGuid.Empty;
			if (!trackingID.IsEmpty)
			{
				var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, trackingID);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				sentInterchange = message.Factory.Load<EDIInterchange>(query).FirstOrDefault();
			}
			if (sentInterchange == null)
			{
				throw new InvalidOperationException(Res.GetString("E2C2A71E-1ABE-421C-87FB-AD9D1584976D", "Unable to locate the related sent interchange"));
			}
			return sentInterchange;
		}

		public static EDIMessage GetOutgoingMessage(EDIMessage message)
		{
			var sentMessages = MessageProcessorHelper.GetRelatedSentInterchange(message).ContainedMessages;
			if (!sentMessages.Any() || sentMessages.Skip(1).Any())
			{
				throw new InvalidOperationException(Res.GetString("218ECE99-3771-4A77-B254-DE7FE5C8A111", "Sent interchange must have one and only one message associated"));
			}
			return sentMessages[0];
		}

		public static ZString GetInterchangeCertificateName(EDIMessage receivedMessage, LoggingInformation logger)
		{
			var certificateName = ZString.Empty;
			var receivedInterchange = receivedMessage.Interchange;
			if ((receivedInterchange?.EI_TransportType ?? ZString.Empty) == EDIInterchange.TransportType.xT)
			{
				certificateName = GetOutgoingMessage(receivedMessage).EM_ApplicationReference;
			}
			else
			{
				try
				{
					using (var textReaderInterchange = receivedInterchange.GetEI_HeaderTextReader())
					{
						var headers = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.DeserializeWithXSDValidation<Headers>(HeadersXsdSchemaEmbeddedResourceName, textReaderInterchange);
						certificateName = headers.CertificateName;
					}
				}
				catch (Exception ex) when (ex is XmlSchemaValidationException || ex is InvalidOperationException || ex is NullReferenceException)
				{
					logger.Log(string.Format(CultureInfo.CurrentCulture, "Error reading xml {0}", ex.ToString()), Integration.LogType.Error);
				}
			}
			return certificateName;
		}

		const string HeadersXsdSchemaEmbeddedResourceName = "CargoWise.Customs.ES.MessageDefinitions.Interchange.Headers.xsd";
	}
}

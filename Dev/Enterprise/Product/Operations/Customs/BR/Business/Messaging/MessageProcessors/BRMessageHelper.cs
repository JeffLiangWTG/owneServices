using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public static class BRMessageHelper
	{
		public static XDocument TryParseXML(ZString content, bool throwExceptionIfOccurs = false)
		{
			try
			{
				return XDocument.Parse(content);
			}
			catch (XmlException)
			{
				if (throwExceptionIfOccurs)
				{
					throw;
				}
				return null;
			}
		}

		static JsonSerializerOptions serializerOptions { get; set; }

		public static T DeserializeObject<T>(ZString content, bool throwExceptionIfOccurs = true)
		{
			try
			{
				if (serializerOptions == null)
				{
					serializerOptions = new JsonSerializerOptions()
					{
						DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
						AllowTrailingCommas = true,
						PropertyNameCaseInsensitive = true
					};
					serializerOptions.Converters.Add(new NumberToStringConverter());
					serializerOptions.Converters.Add(new LenientDateTimeConverter());
					serializerOptions.Converters.Add(new StringToNumberConverter(true));
				}

				return JsonSerializer.Deserialize<T>(content, serializerOptions);
			}
			catch (Exception)
			{
				if (throwExceptionIfOccurs && content.Length > 0)
				{
					throw;
				}
				return default(T);
			}
		}

		public static string ToJson(this object value)
		{
			return JsonSerializer.Serialize(value, new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				WriteIndented = true,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			});
		}

		public static ZString FormatIdLote(string idLote) => new ZString(idLote).Right(6);

		public static string GetFormattedDate(string value)
		{
			return ZDateTime.TryParseExact(value, out var result, Constants.DataFormat) ? result.ToISO8601ShortDateString() : string.Empty;
		}

		public static ZString[] DeserializeJsonObjectArray(ZString responseMessage, bool needsUnzip)
		{
			var messages = needsUnzip && !responseMessage.ContainsAnyChar("[]") ? UnzipAndDecodeMessages(responseMessage).ToArray() : [responseMessage];
			return messages.SelectMany(message => DeserializeObject<object[]>(message)).Select(jsonObject => new ZString(jsonObject.ToJson())).ToArray();
		}

		static IEnumerable<ZString> UnzipAndDecodeMessages(ZString encodedMessage)
		{
			var decodedMessage = Convert.FromBase64String(encodedMessage);
			using var memoryStream = new MemoryStream(decodedMessage);
			using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
			foreach (var entry in archive.Entries.Cast<ZipArchiveEntry>().Where(w => w.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
			{
				using var reader = new StreamReader(entry.Open());
				yield return reader.ReadToEnd();
			}
		}

		static ZQuery GetOutgoingInterchangeQuery(ZString applicationCode, ZGuid sessionGUID)
		{
			var query = new ZQuery();
			query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, applicationCode);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			query.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, sessionGUID);
			query.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			query.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + @" DESC";
			return query;
		}

		static ZQuery GetOutgoingMessageQuery(ZGuid interchangePK, int? messageNum = null)
		{
			var incomingInterchangeQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.EI_SessionGUID);
			incomingInterchangeQuery.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			incomingInterchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
			incomingInterchangeQuery.AddToFilter(EDIInterchangeSchema.PK, interchangePK);

			var outgoingInterchangeQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
			outgoingInterchangeQuery.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			outgoingInterchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIMessage.ApplicationCodes.BRCustoms);
			outgoingInterchangeQuery.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			outgoingInterchangeQuery.AddSubQuery(EDIInterchangeSchema.EI_SessionGUID, incomingInterchangeQuery, JoinCondition.And);

			var outgoingMessagesQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			outgoingMessagesQuery.AddSubQuery(EDIMessageSchema.EM_EI, outgoingInterchangeQuery, JoinCondition.And);
			outgoingMessagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			if (messageNum != null)
			{
				outgoingMessagesQuery.AddToFilter(EDIMessageSchema.EM_MessageNum, messageNum.ToString());
			}
			outgoingMessagesQuery.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + @" DESC";

			return outgoingMessagesQuery;
		}

		public static EDIInterchange GetOutgoingInterchange(EDIInterchange incomingInterchange)
		{
			var query = GetOutgoingInterchangeQuery(incomingInterchange.EI_ApplicationCode, incomingInterchange.EI_SessionGUID);
			return incomingInterchange.Factory.LoadTop1<EDIInterchange>(query);
		}

		public static IEnumerable<EDIMessage> GetOutgoingMessages(EDIMessage incomingMessage)
		{
			return incomingMessage.Factory.Load<EDIMessage>(GetOutgoingMessageQuery(incomingMessage.EM_EI));
		}

		public static EDIMessage GetOutgoingMessage(EDIMessage incomingMessage, int? messageNum = null)
		{
			return incomingMessage.Factory.LoadTop1<EDIMessage>(GetOutgoingMessageQuery(incomingMessage.EM_EI, messageNum));
		}

		public static EDIMessage GetOutgoingMessage(EDIInterchange incomingInterchange, int? messageNum = null)
		{
			return incomingInterchange.Factory.LoadTop1<EDIMessage>(GetOutgoingMessageQuery(incomingInterchange.PK, messageNum));
		}

		public static EDIMessage[] GetOutgoingMessagesByApplicationReference(BusinessObjectFactory factory, ZString applicationReference, ZString messageType, string[] messageSubTypes = null)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.BRCustoms);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, messageType);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, applicationReference);
			query.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessageStatusList.Codes.Discarded);

			if (messageSubTypes != null && messageSubTypes.Length > 0)
			{
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, messageSubTypes);
			}

			return factory.Load<EDIMessage>(query);
		}

		public static EDIMessage[] GetOutgoingMessagesSentAtTheSameTime(EDIMessage incomingMessage, ZString messageType, string[] messageSubTypes = null)
		{
			var outgoingMessage = GetOutgoingMessage(incomingMessage);

			var outgoingMessagesQuery = ZQuery.NoResultQuery;
			if (outgoingMessage != null)
			{
				outgoingMessagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.BRCustoms);
				outgoingMessagesQuery.AddToFilter(EDIMessageSchema.EM_MessageType, messageType);
				outgoingMessagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				outgoingMessagesQuery.AddToFilter(EDIMessageSchema.EM_SystemCreateUser, outgoingMessage.EM_SystemCreateUser);
				outgoingMessagesQuery.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, outgoingMessage.EM_SystemCreateTimeUtc);
				outgoingMessagesQuery.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessageStatusList.Codes.Discarded);

				if (messageSubTypes != null && messageSubTypes.Length > 0)
				{
					outgoingMessagesQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, messageSubTypes);
				}
			}

			return incomingMessage.Factory.Load<EDIMessage>(outgoingMessagesQuery);
		}

		public static bool AllMessagesHaveResponseAndBeenProcessed(BusinessObjectFactory factory, EDIMessage[] outgoingMessages)
		{
			if (outgoingMessages.Length == 0)
			{
				return true;
			}

			if (outgoingMessages.Any(x => x.EM_Status == EDIInterchangeStatusList.Codes.Queued))
			{
				return false;
			}

			var sentOutgoingMessages = outgoingMessages.Where(x => x.EM_Status == EDIInterchangeStatusList.Codes.Sent && x.EM_EI.IsValid).ToArray();
			if (sentOutgoingMessages.Length == 0)
			{
				return true;
			}

			var outgoingInterchangesQuery = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIMessage.ApplicationCodes.BRCustoms)
																		.AddToFilter(EDIInterchangeSchema.EI_IsActive, true)
																		.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
																		.AddToFilter(EDIInterchangeSchema.PK, sentOutgoingMessages.Select(x => x.EM_EI).Distinct());

			var outgoingInterchanges = factory.Load<EDIInterchange>(outgoingInterchangesQuery);
			if (outgoingInterchanges.Any(x => x.EI_Status == EDIInterchangeStatusList.Codes.Queued))
			{
				return false;
			}

			var sentOutgoingInterchanges = outgoingInterchanges.Where(x => x.EI_Status == EDIInterchangeStatusList.Codes.Sent).ToArray();
			if (sentOutgoingInterchanges.Length == 0)
			{
				return true;
			}

			var incomingInterchangesQuery = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, EDIMessage.ApplicationCodes.BRCustoms)
																		.AddToFilter(EDIInterchangeSchema.EI_IsActive, true)
																		.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive)
																		.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, sentOutgoingInterchanges.Select(x => x.EI_SessionGUID));

			var incomingInterchanges = factory.Load<EDIInterchange>(incomingInterchangesQuery);
			if (incomingInterchanges.Length != sentOutgoingInterchanges.Length || incomingInterchanges.Any(x => x.EI_Status == EDIInterchangeStatusList.Codes.Queued))
			{
				return false;
			}

			var receivedIncomingInterchanges = incomingInterchanges.Where(x => x.EI_Status == EDIInterchangeStatusList.Codes.Received).ToArray();
			if (receivedIncomingInterchanges.Length == 0)
			{
				return true;
			}

			var incomingMessagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.BRCustoms)
																.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive)
																.AddToFilter(EDIMessageSchema.EM_EI, receivedIncomingInterchanges.Select(x => x.PK));

			var incomingMessages = factory.Load<EDIMessage>(incomingMessagesQuery);
			return incomingMessages.All(x => !NotProcessedMessageStatuses.Contains(x.EM_Status));
		}

		public static readonly ImmutableArray<ZString> NotProcessedMessageStatuses = new ZString[]
		{
			EDIMessageStatusList.Codes.Queued,
			EDIMessageStatusList.Codes.PreProcessedOK,
		}.ToImmutableArray();
	}
}

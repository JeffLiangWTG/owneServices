using System.Data;
using System.Xml.XPath;

namespace eServices.ApplicationEvent.EnrichmentService.Enrichers;

public class OceanMessaging(eHubTransactionsContext eHubTransactionsContext, CacheService cache) : IEnricherBatched, IDisposable
{
	public async Task<IEnumerable<JsonObject>> EnrichMessageBatchAsync(JsonObject messageEvent, string criteria, CancellationToken cancelToken = default)
	{
		var enrichedData = new JsonObject();
		var inboxPk = messageEvent["MessageEvent"]?["eHub"]?["Outbox"]?["InboxPK"]?.ToString();
		var outboxPk = messageEvent["MessageEvent"]?["eHub"]?["Outbox"]?["PK"]?.ToString();
		if (inboxPk is null || outboxPk is null)
			return [enrichedData];

		return criteria switch
		{
			"Inbound" => await EnrichInbound(messageEvent, enrichedData, inboxPk, outboxPk, cancelToken: cancelToken),
			"Outbound" => await EnrichOutbound(messageEvent, enrichedData, inboxPk, outboxPk, cancelToken: cancelToken),
			"OutboundError" => await EnrichOutboundError(messageEvent, enrichedData, inboxPk, outboxPk, cancelToken: cancelToken),
			"DeliveryNotification" => await EnrichDeliveryNotification(messageEvent, enrichedData, inboxPk, outboxPk, cancelToken: cancelToken),
			_ => [enrichedData]
		};
	}

	private async Task<List<JsonObject>> EnrichInbound(JsonObject messageEvent, JsonObject enrichedData, string inboxPk, string outboxPk, CancellationToken cancelToken)
	{
		if (!int.TryParse(await GetFromXml("Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_OutboxBatchCount), cancelToken: cancelToken), out var outboxBatchCount)
			|| outboxBatchCount is 0)
			return [enrichedData];

		var encrichedDataBatch = new List<JsonObject>();

		var inboxMsgType = await EnrichFromXml(enrichedData, "MessageType", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Inbound_InboxEdifactMsgType), cancelToken: cancelToken);
		if (inboxMsgType is "CONTRL")
		{
			await EnrichFromXml(enrichedData, "InterchangeNum", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Inbound_InterchangeNum), cancelToken: cancelToken);
		}

		for (int i = 1; i <= outboxBatchCount; i++)
		{
			var enrichedItem = enrichedData.DeepClone().AsObject();
			encrichedDataBatch.Add(enrichedItem);

			var outboxMsgType = await GetFromXml("Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_OutboxMsgType), i, cancelToken: cancelToken);

			if (outboxMsgType is "UniversalEvent")
			{
				await EnrichFromXml(enrichedItem, "Carrier", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUE_Carrier), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "CarrierBookingRef", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUE_CarrierBookingRef), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "MasterBillNumber", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUE_MasterBillNumber), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "CoLoadCarrierBookingRef", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadCarrierBookingRef), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "CoLoadMasterBillNumber", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUE_CoLoadMasterBillNumber), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "MessageRef", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUE_MessageRef), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "DocumentName", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUE_DocumentName), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "DataType", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUE_DataType), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "DataKey", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUE_DataKey), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "EventType", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUE_EventType), i, cancelToken: cancelToken);
			}
			else if (outboxMsgType is "UniversalShipment")
			{
				await EnrichFromXml(enrichedItem, "Carrier", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUS_Carrier), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "CarrierBookingRef", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUS_CarrierBookingRef), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "MasterBillNumber", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUS_MasterBillNumber), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "CoLoadCarrierBookingRef", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUS_CoLoadCarrierBookingRef), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "CoLoadMasterBillNumber", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUS_CoLoadMasterBillNumber), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "MessageRef", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUS_MessageRef), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "DocumentName", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUS_DocumentName), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "DataType", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUS_DataType), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "DataKey", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUS_DataKey), i, cancelToken: cancelToken);
				await EnrichFromXml(enrichedItem, "EventType", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Inbound_XUS_EventType), i, cancelToken: cancelToken);
			}

			enrichedItem["TrackingID"] = messageEvent["MessageEvent"]!["Message"]!["TrackingID"]!.ToString().ToUpperInvariant();
			enrichedItem["Direction"] = "Response";

			if (messageEvent["MessageEvent"]?["eHub"]?["Outbox"]?["FileName"]?.ToString() is string fileName and { Length: > 0 })
				enrichedItem["FileName"] = fileName;
		}

		return encrichedDataBatch;
	}

	private async Task<List<JsonObject>> EnrichOutbound(JsonObject messageEvent, JsonObject enrichedData, string inboxPk, string outboxPk, CancellationToken cancelToken)
	{
		await EnrichFromXml(enrichedData, "MessageType", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Outbound_MessageType), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "InterchangeNum", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Outbound_InterchangeNum), cancelToken: cancelToken);
		var isCoLoad = bool.TryParse(await GetFromXml("Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_IsCoLoad), cancelToken: cancelToken), out bool isCoLoadValue) ? isCoLoadValue : false;
		var carrierQuery = isCoLoad ? nameof(OceanMessagingResources.XPath_Outbound_Carrier_CoLoad) : nameof(OceanMessagingResources.XPath_Outbound_Carrier);
		await EnrichFromXml(enrichedData, "Carrier", "Inbox", inboxPk, carrierQuery, cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "CarrierBookingRef", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_CarrierBookingRef), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "MasterBillNumber", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_MasterBillNumber), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "CoLoadCarrierBookingRef", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_CoLoadCarrierBookingRef), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "CoLoadMasterBillNumber", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_CoLoadMasterBillNumber), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "MessageRef", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_Outbound_MessageRef), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "DocumentName", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_DocumentName), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "Purpose", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_Purpose), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "SubmissionVersion", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_SubmissionVersion), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "DataType", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_DataType), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "DataKey", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_DataKey), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "OperationalPort", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_OperationalPort), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "TrackingID", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_TrackingID), transform: s => s.ToUpperInvariant(), cancelToken: cancelToken);
		enrichedData["Direction"] = "Request";

		if (messageEvent["MessageEvent"]?["eHub"]?["Outbox"]?["FileName"]?.ToString() is string fileName and { Length: > 0 })
			enrichedData["FileName"] = fileName;
		return [enrichedData];
	}

	private async Task<List<JsonObject>> EnrichOutboundError(JsonObject messageEvent, JsonObject enrichedData, string inboxPk, string outboxPk, CancellationToken cancelToken)
	{
		var isCoLoad = bool.TryParse(await GetFromXml("Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_IsCoLoad), cancelToken: cancelToken), out bool isCoLoadValue) ? isCoLoadValue : false;
		var carrierQuery = isCoLoad ? nameof(OceanMessagingResources.XPath_Outbound_Carrier_CoLoad) : nameof(OceanMessagingResources.XPath_Outbound_Carrier);
		await EnrichFromXml(enrichedData, "Carrier", "Inbox", inboxPk, carrierQuery, cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "CarrierBookingRef", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_CarrierBookingRef), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "MasterBillNumber", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_MasterBillNumber), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "CoLoadCarrierBookingRef", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_CoLoadCarrierBookingRef), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "CoLoadMasterBillNumber", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_CoLoadMasterBillNumber), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "DocumentName", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_DocumentName), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "Purpose", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_Purpose), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "SubmissionVersion", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_SubmissionVersion), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "DataType", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_DataType), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "DataKey", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_DataKey), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "OperationalPort", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_OperationalPort), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "TrackingID", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_TrackingID), transform: s => s.ToUpperInvariant(), cancelToken: cancelToken);
		enrichedData["Direction"] = "Request";
		return [enrichedData];
	}

	private async Task<List<JsonObject>> EnrichDeliveryNotification(JsonObject messageEvent, JsonObject enrichedData, string inboxPk, string outboxPk, CancellationToken cancelToken)
	{
		await EnrichFromXml(enrichedData, "EventType", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_EventType), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "InterchangeNum", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_InterchangeNum), cancelToken: cancelToken);
		await EnrichFromXml(enrichedData, "MessageRef", "Outbox", outboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_MessageRef), cancelToken: cancelToken);

		var inboxMsgType = await GetFromXml("Inbox", inboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_InboxMsgType), cancelToken: cancelToken);
		if (inboxMsgType is "DeliveryNotificationMessage")
		{
			var isCoLoad = bool.TryParse(await GetFromXml("Inbox", inboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_IsCoLoad), cancelToken: cancelToken), out bool isCoLoadValue) ? isCoLoadValue : false;
			var carrierQuery = isCoLoad ? nameof(OceanMessagingResources.XPath_Outbound_Carrier_CoLoad) : nameof(OceanMessagingResources.XPath_DeliveryNotification_Carrier);
			await EnrichFromXml(enrichedData, "Carrier", "Inbox", inboxPk, carrierQuery, cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "CarrierBookingRef", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_CarrierBookingRef), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "MasterBillNumber", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_MasterBillNumber), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "CoLoadCarrierBookingRef", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_CoLoadCarrierBookingRef), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "CoLoadMasterBillNumber", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_CoLoadMasterBillNumber), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "DocumentName", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_DocumentName), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "Purpose", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_Purpose), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "SubmissionVersion", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_SubmissionVersion), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "DataType", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_DataType), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "DataKey", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_DataKey), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "OperationalPort", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_OperationalPort), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "TrackingID", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_DeliveryNotification_TrackingID), transform: s => s.ToUpperInvariant(), cancelToken: cancelToken);
		}
		else
		{
			var isCoLoad = bool.TryParse(await GetFromXml("Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_IsCoLoad), cancelToken: cancelToken), out bool isCoLoadValue) ? isCoLoadValue : false;
			var carrierQuery = isCoLoad ? nameof(OceanMessagingResources.XPath_Outbound_Carrier_CoLoad) : nameof(OceanMessagingResources.XPath_Outbound_Carrier);
			await EnrichFromXml(enrichedData, "Carrier", "Inbox", inboxPk, carrierQuery, cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "CarrierBookingRef", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_CarrierBookingRef), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "MasterBillNumber", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_MasterBillNumber), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "CoLoadCarrierBookingRef", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_CoLoadCarrierBookingRef), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "CoLoadMasterBillNumber", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_CoLoadMasterBillNumber), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "DocumentName", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_DocumentName), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "Purpose", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_Purpose), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "SubmissionVersion", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_SubmissionVersion), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "DataType", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_DataType), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "DataKey", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_DataKey), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "OperationalPort", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_OperationalPort), cancelToken: cancelToken);
			await EnrichFromXml(enrichedData, "TrackingID", "Inbox", inboxPk, nameof(OceanMessagingResources.XPath_Outbound_TrackingID), transform: s => s.ToUpperInvariant(), cancelToken: cancelToken);
		}
		enrichedData["Direction"] = "Request";
		return [enrichedData];
	}

	[SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.", Justification = "No user input")]
	private async Task<string> GetFromDbXml(
		string type,
		string pk,
		string queryName,
		CancellationToken cancelToken)
	{
		var query = OceanMessagingResources.ResourceManager.GetString(queryName)!;
		var key = $"OceanMessaging:{type}:{pk}:{queryName}";
		return await cache.GetOrAddCacheItem(key, async () =>
		{
			return await Task.Run(() => eHubTransactionsContext.Database
				.SqlQueryRaw<string>($"""
					DECLARE @Result TABLE (Content nvarchar(max))
					INSERT INTO @Result
						EXEC [dbo].[SelectMessageDetails] @pk='{pk}', @type='{type}', @format='XML'
					DECLARE @MsgXml xml = (SELECT CONVERT(xml,Content) FROM @Result)
					SELECT @MsgXml.value('{query.Replace("'", "''")}','nvarchar(max)')
					""")
				.AsEnumerable()
				.FirstOrDefault() ?? "", cancelToken);
		}, cancellationToken: cancelToken);
	}

	private async Task<string> EnrichFromXml(
		JsonObject enrichedData,
		string name,
		string type,
		string pk,
		string queryName,
		int? idx = null,
		Func<string, string>? transform = null,
		CancellationToken cancelToken = default)
	{
		var value = await GetFromXml(type, pk, queryName, idx, cancelToken: cancelToken);
		if (value.Length > 0)
		{
			if (transform is not null)
				value = transform(value);
			enrichedData[name] = value;
		}
		return value;
	}

	private async Task<string> GetFromXml(
		string type,
		string pk,
		string queryName,
		int? idx = null,
		CancellationToken cancelToken = default)
	{
		var query = OceanMessagingResources.ResourceManager.GetString(queryName)!;
		var key = $"OceanMessaging:{type}:{pk}:{queryName}";
		if (idx.HasValue)
		{
			query = query.Replace("{idx}", idx.ToString());
			key += $":{idx}";
		}
		return await cache.GetOrAddCacheItem(key, async () =>
		{
			var xml = await GetMessageXml(type, pk, cancelToken);

			return xml.XPNav?.Evaluate(query).ToString() ?? "";
		}, cancellationToken: cancelToken);
	}

	private async Task<MessageXml> GetMessageXml(
		string type,
		string pk,
		CancellationToken cancelToken)
	{
		if (XmlCache.TryGetValue((type, pk), out var cachedXml))
			return cachedXml;

		await XmlCacheSemaphore.WaitAsync(cancelToken);
		try
		{
			if (XmlCache.TryGetValue((type, pk), out cachedXml))
				return cachedXml;

			if (await GetMessage(type, pk, cancelToken: cancelToken) is string content and { Length: > 0 })
			{
				var reader = new StringReader(content);
				var xpDoc = new XPathDocument(reader);
				var xpNav = xpDoc.CreateNavigator();
				return XmlCache[(type, pk)] = new(reader, xpDoc, xpNav);
			}
			else
			{
				return XmlCache[(type, pk)] = new(null, null, null);
			}
		}
		finally
		{
			XmlCacheSemaphore.Release();
		}
	}

	private async Task<string> GetMessage(string type, string pk, CancellationToken cancelToken = default)
	{
		var key = $"OceanMessaging:{type}:{pk}:XML";
		return await cache.GetOrAddCacheItem(key, async () =>
		{
			return await Task.Run(() => eHubTransactionsContext.Database
				.SqlQuery<string>($"EXEC [dbo].[SelectMessageDetails] @pk={pk}, @type={type}, @format='XML'")
				.AsEnumerable()
				.FirstOrDefault() ?? "", cancelToken);
		}, 5, cancelToken);
	}

	public void Dispose()
	{
		foreach (var xml in XmlCache.Values)
		{
			xml.Reader?.Dispose();
		}
		XmlCache.Clear();
	}

	private record class MessageXml(TextReader? Reader, XPathDocument? XPDoc, XPathNavigator? XPNav);
	private readonly SemaphoreSlim XmlCacheSemaphore = new(1);
	private readonly ConcurrentDictionary<(string, string), MessageXml> XmlCache = new();
}

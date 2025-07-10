using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common.Testing;
using Newtonsoft.Json;

namespace WTG.Serialization.DataScience.Audit.ObjectModel;

sealed class PartialAuditMessage
{
	[JsonConstructor]
	public PartialAuditMessage(PartialAuditMessageTrackingInfo multipartTrackingInfo, string payloadString)
	{
		MultipartTrackingInfo = multipartTrackingInfo ?? throw new ArgumentNullException(nameof(multipartTrackingInfo));
		PayloadString = payloadString ?? throw new ArgumentNullException(nameof(payloadString));
	}

	public PartialAuditMessage(Guid messageId, int partIndex, int partsCount, string payloadString)
		: this(new PartialAuditMessageTrackingInfo(messageId, partIndex, partsCount), payloadString)
	{ }

	public PartialAuditMessageTrackingInfo MultipartTrackingInfo { get; private set; }
	public string PayloadString { get; private set; }

	public static PartialAuditMessage[] SplitFrom(AuditMessage auditMessage, int maxMessageSize, int kafkaMessageOverhead, JsonSerializer messageSerialiser, Guid messageId)
	{
		if (auditMessage == null)
		{
			throw new ArgumentNullException(nameof(auditMessage));
		}

		var messageEncoding = Encoding.UTF8;
		var templateMessage = new PartialAuditMessage(messageId, 1_000, 1_000, string.Empty);
		var messageOverhead = messageEncoding.GetByteCount(SerialisePartialAuditMessage(templateMessage, messageSerialiser)) + kafkaMessageOverhead;

		var auditMessageJsonString = SerialiseAuditMessage(auditMessage, messageSerialiser);
		var payloadParts = SplitMultipartMessagePayloadString(auditMessageJsonString, messageEncoding, messageOverhead, maxMessageSize).ToList();

		var partsCount = payloadParts.Count;
		var partialMessages = new PartialAuditMessage[partsCount];
		for (var partIndex = 0; partIndex < partsCount; ++partIndex)
		{
			partialMessages[partIndex] = new (messageId, partIndex, partsCount, payloadParts[partIndex]);
		}

		return partialMessages;
	}

	[SuppressThreadStaticFieldMessage] // Oh God this is some next level stupid
	static readonly char[] escapableChars = { '\"', '\'', '\\', '\r', '\n', '\f', '\b', '\t' };

	static IEnumerable<string> SplitMultipartMessagePayloadString(string payload, Encoding messageEncoding, int messageOverhead, int maxMessageSize)
	{
		if (messageOverhead <= 0)
		{
			throw new ArgumentException($"Invalid {nameof(messageOverhead)}: {messageOverhead}");
		}

		if (maxMessageSize <= 6 + messageOverhead)
		{
			throw new ArgumentException($"Invalid {nameof(maxMessageSize)}: {maxMessageSize}");
		}

		var n = payload.Length;
		var charBuffer = new char[1]; // Encoding.GetByteCount() takes a char array
		var partPayloadIndexFrom = 0;
		while (partPayloadIndexFrom < n)
		{
			var messageSizeEstimate = messageOverhead;
			var partPayloadIndexTo = partPayloadIndexFrom;
			for (; partPayloadIndexTo < n; ++partPayloadIndexTo)
			{
				var c = payload[partPayloadIndexTo];
				charBuffer[0] = c;
				var estimatedCharSize = c switch
				{
					var singleEscape when escapableChars.Contains(singleEscape) => 2,
					var hexCodePoint when hexCodePoint >= 128 => 6, // e.g. \u1234
					_ => messageEncoding.GetByteCount(charBuffer)
				};

				var nextMessageSizeEstimate = messageSizeEstimate + estimatedCharSize;
				if (nextMessageSizeEstimate > maxMessageSize)
				{
					break;
				}

				messageSizeEstimate = nextMessageSizeEstimate;
			}

			Debug.Assert(partPayloadIndexTo > partPayloadIndexFrom);
			yield return payload.Substring(partPayloadIndexFrom, partPayloadIndexTo - partPayloadIndexFrom);
			partPayloadIndexFrom = partPayloadIndexTo;
		}
	}

	static string SerialiseAuditMessage(AuditMessage auditMessage, JsonSerializer messageSerialiser)
	{
		var buffer = new StringBuilder(1_000_000);
		using var textWriter = new StringWriter(buffer);
		using var jsonWriter = new JsonTextWriter(textWriter);
		messageSerialiser.Serialize(jsonWriter, auditMessage);
		return buffer.ToString();
	}

	static string SerialisePartialAuditMessage(PartialAuditMessage partialAuditMessage, JsonSerializer messageSerialiser)
	{
		var buffer = new StringBuilder(1_000_000);
		using var textWriter = new StringWriter(buffer);
		using var jsonWriter = new JsonTextWriter(textWriter);
		messageSerialiser.Serialize(jsonWriter, partialAuditMessage);
		return buffer.ToString();
	}
}

sealed class PartialAuditMessageTrackingInfo
{
	[JsonConstructor]
	public PartialAuditMessageTrackingInfo(Guid messageId, int partIndex, int partsCount)
	{
		MessageId = messageId;
		PartIndex = partIndex;
		PartsCount = partsCount;
	}

	public Guid MessageId { get; private set; }
	public int PartIndex { get; private set; }
	public int PartsCount { get; private set; }
}
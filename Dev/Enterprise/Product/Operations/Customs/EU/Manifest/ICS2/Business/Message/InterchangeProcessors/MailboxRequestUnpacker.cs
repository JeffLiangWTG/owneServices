using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.SoapEnvelope.S12;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using HttpMultipartParser;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	sealed class MailboxRequestUnpacker
	{
		public MailboxRequestUnpacker(LoggingInformation logger)
		{
			this.logger = logger;
		}

		readonly LoggingInformation logger;

		const string AttachmentContentType = @"application/gzip";
		readonly static Regex BoundaryRegex = new Regex(@"(?<=boundary="")([^""]*)(?="")", RegexOptions.Multiline);

		public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange)
		{
			var boundary = GetBoundary(interchange);
			if (string.IsNullOrWhiteSpace(boundary))
			{
				return new EDIInterchangeUnpackerResult((NoResString)"No valid boundary found.");
			}
			return ExtractMessagesFromAttachements(interchange, boundary);
		}

		IUniversalCustomsInterchangeUnpackerResult ExtractMessagesFromAttachements(EDIInterchange interchange, string boundary)
		{
			var messages = new List<EDIMessage>();
			using (var stream = interchange.GetEI_BodyDataReader())
			{
				var parser = MultipartFormDataParser.Parse(stream, boundary);

				var fileParts = parser.Files;
				var linkedObject = TryGetLinkedObject(interchange, fileParts);

				var attachments = fileParts.Where(c => c.ContentType.Equals(AttachmentContentType, StringComparison.InvariantCultureIgnoreCase));
				if (attachments.Any())
				{
					foreach (var attachment in attachments)
					{
						using (var attachmentStream = attachment.Data)
						{
							if (attachmentStream.CanSeek)
							{
								attachmentStream.Seek(0, SeekOrigin.Begin);
							}

							var msgTypeAndDocument = GetMessageTypeAndDocumentCore(attachmentStream);
							var messageType = msgTypeAndDocument.MessageType;

							if (!messageType.IsEmpty)
							{
								var bodyStream = new MemoryStream();
								msgTypeAndDocument.Document.Save(bodyStream);

								var message = CreateMessage(interchange, messageType, bodyStream);

								if (linkedObject != null)
								{
									message.EM_LinkedObject = linkedObject;
									message.EM_GB = linkedObject.AMA_GB;
								}

								messages.Add(message);
							}
						}
					}
				}
				else
				{
					return new EDIInterchangeUnpackerResult((NoResString)"No valid zip attachment found.");
				}
			}

			if (messages.Count > 0)
			{
				interchange.ContainedMessages.AddRange(messages);
				logger.Log(Res.GetString("1DB21156-5E75-4F97-B515-FA93C04FC8BB", "Create {0} message(s) from {1}.", messages.Count, interchange.EI_InterchangeNum));
			}

			return new EDIInterchangeUnpackerResult(messages);
		}

		string GetBoundary(EDIInterchange interchange)
		{
			using (var stream = interchange.GetEI_BodyDataReader())
			using (var reader = new StreamReader(stream))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();

					var boundaryMatch = BoundaryRegex.Match(line);
					if (boundaryMatch.Success)
					{
						return boundaryMatch.Value;
					}
				}
			}

			return string.Empty;
		}

		AsycudaManifestHeader TryGetLinkedObject(EDIInterchange interchange, IEnumerable<FilePart> fileParts)
		{
			var factory = interchange.Factory;

			foreach (var filePart in fileParts)
			{
				if (!filePart.ContentType.Equals(AttachmentContentType, StringComparison.InvariantCultureIgnoreCase) && TryGetOutgoingInterchangePk(filePart, out var outgoingInterchangePk))
				{
					var query = new ZQuery();
					query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
					query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent);
					query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.IC2);
					query.AddToFilter(EDIMessageSchema.EM_EI, outgoingInterchangePk);
					query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + " desc";

					var linkedObject = interchange.Factory.Load<EDIMessage>(query)
						.FirstOrDefault(c => c.EM_LinkedObject is AsycudaManifestHeader)
						?.EM_LinkedObject as AsycudaManifestHeader;

					if (linkedObject != null)
					{
						return linkedObject;
					}
				}
			}

			return null;
		}

		bool TryGetOutgoingInterchangePk(FilePart filePart, out ZGuid outgoingInterchangePk)
		{
			const char Separator = '@';

			outgoingInterchangePk = ZGuid.Invalid;

			try
			{
				using (var reader = XmlReader.Create(filePart.Data))
				{
					var envelope = XmlObjectSerializer.Deserialize<Envelope>(reader);
					var element = envelope?.Header?.Any?.FirstOrDefault();
					var xml = element?.OuterXml;

					var msgObject = string.IsNullOrWhiteSpace(xml)
						? null
						: XmlObjectSerializer.Deserialize<CargoWise.Customs.EU.MessageDefinitions.ICS2.SoapEnvelope.msg_ebms_3.Messaging>(xml);

					if (msgObject != null)
					{
						var message = msgObject.UserMessage?.FirstOrDefault();
						var refToMessageId = message?.MessageInfo?.RefToMessageId ?? string.Empty;
						var key = refToMessageId.Split(Separator).FirstOrDefault();

						if (ZGuid.TryParse(key, out outgoingInterchangePk) && outgoingInterchangePk.IsValid)
						{
							return true;
						}
					}
				}

				return false;
			}
			catch
			{
				return false;
			}
		}

		EDIMessage CreateMessage(EDIInterchange interchange, string messageType, Stream body)
		{
			var message = interchange.Factory.New<EDIMessage>();
			message.EM_ApplicationCode = interchange.EI_ApplicationCode;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = EUICS2InterchangeTypeList.Codes.MailboxRequest;
			message.EM_GB = interchange.EI_GB;
			message.EM_MessageNum = interchange.EI_InterchangeNum.SubstringSafe(0, EDIMessageSchema.EM_MessageNum.MaxLength);

			message.SetEM_MessageTextOrDataSource(body);

			return message;
		}

		(ZString MessageType, XDocument Document) GetMessageTypeAndDocumentCore(Stream stream)
		{
			using (var xmlStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
			{
				var document = XDocument.Load(xmlStream, LoadOptions.None);
				var node = document.Root;

				var messageType = ((ZString)node.Name.LocalName).SubstringSafe(3, 3);

				if (!messageType.IsEmpty)
				{
					return (messageType, document);
				}
			}

			return (string.Empty, null);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.ICS
{
	public abstract class IcsSsGreatBritainResponseMessageProcessorBase<T> : ApplicationTypeMessageProcessor
	{
		public IcsSsGreatBritainResponseMessageProcessorBase(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => "ICS (S&S) GB Response Message Processor";

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.GbMessageICSGreatBritain;

		protected abstract ZString MessageType { get; }

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new[] { MessageType };

		protected abstract ZString MessageInterpretation(T messageObject);

		protected virtual void UpdateManifestHeader(AsycudaManifestHeaderSS manifestHeader, T messageObject)
		{
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message is IcsSsGreatBritainEDIMessage icsMessage)
			{
				var messageXml = icsMessage.EM_MessageText;

				if (!messageXml.IsEmpty)
				{
					var (manifestHeader, transmittedMessage) = FindManifestAndTransmittedMessage(icsMessage, messageXml);

					if (manifestHeader != null)
					{
						try
						{
							var messageObject = ResponseMessageProcessorHelper.GetMessageObject<T>(messageXml);
							icsMessage.EM_LinkedObject = manifestHeader;
							icsMessage.EM_GB = manifestHeader.Branch.PK;
							icsMessage.EM_MessageInterpretation = MessageInterpretation(messageObject);

							UpdateTransmittedMessageStatus(icsMessage, transmittedMessage);
							UpdateManifestHeader(manifestHeader, messageObject);

							icsMessage.EM_Status = EDIMessageStatusList.Codes.Received;
						}
						catch (Exception ex)
						{
							Logger.LogWarning($"Message {message.EM_MessageNum}. Failed to deserialize the message as {message.EM_MessageType} message type. Inner exception: {ex.InnerException}");
							icsMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
						}
					}
					else
					{
						HoldAndRetry(icsMessage);
					}
				}
				else
				{
					Logger.LogWarning($"Message {message.EM_MessageNum}. No Message Text data to process.");
					icsMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
				}
			}
		}

		void HoldAndRetry(IcsSsGreatBritainEDIMessage incomingMessage)
		{
			var interchange = incomingMessage.Interchange;
			if (interchange != null)
			{
				interchange.EI_RetryCount += 1;
			}
			if (interchange?.EI_RetryCount < 5)
			{
				incomingMessage.EM_HeldUntilDate = ZDateTime.Now.AddMinutes(1);
				Logger.LogWarning($"Attempt {interchange.EI_RetryCount} failed, HeldUntilDate set to {incomingMessage.EM_HeldUntilDate} for retry");
			}
			else
			{
				incomingMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
				if (interchange != null)
				{
					Logger.LogWarning("Attempt 5 failed, setting message status to failed");
				}
				else
				{
					Logger.LogWarning("Retry not attempted as Interchange is null. Message status set to failed");
				}
			}
		}

		protected virtual void UpdateTransmittedMessageStatus(IcsSsGreatBritainEDIMessage incomingMessage, IcsSsGreatBritainEDIMessage transmittedMessage)
		{
		}

		(AsycudaManifestHeaderSS, IcsSsGreatBritainEDIMessage) FindManifestAndTransmittedMessage(IcsSsGreatBritainEDIMessage incomingMessage, ZString messageText)
		{
			XmlDocument xmlDocument = null;
			XmlDocument LoadXmlDocument()
			{
				xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(messageText);
				return xmlDocument;
			}

			var transmittedMessage = FindOutboundMessageByCorrelationId(incomingMessage);
			if (transmittedMessage == null)
			{
				Logger.LogWarning($"Message {incomingMessage.EM_MessageNum}. Failed to find a transmitted message with an Application Reference matching the Correlation Id.");

				transmittedMessage = FindOutboundMessageByMessageNum(incomingMessage, xmlDocument ??= LoadXmlDocument());
			}

			AsycudaManifestHeaderSS manifest = null;

			if (transmittedMessage != null)
			{
				manifest = transmittedMessage.EM_LinkedObject as AsycudaManifestHeaderSS;
				if (manifest == null)
				{
					Logger.LogWarning($"Matched transmitted message {transmittedMessage.EM_MessageNum} does not have expected LinkedObject");
				}
				else
				{
					Logger.Log($"Matched outgoing message {transmittedMessage.EM_MessageNum} on manifest {manifest.AMA_JobReference}");
				}
			}

			if (manifest == null)
			{
				manifest = FindManifestByJobReference(incomingMessage, xmlDocument ??= LoadXmlDocument());
				if (manifest != null)
				{
					transmittedMessage = manifest.Messages.OfType<IcsSsGreatBritainEDIMessage>().Where(x => x.EM_ReceiveTransmit == EDIInterchange.Direction.Transmit).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
					if (transmittedMessage == null)
					{
						Logger.LogWarning($"Manifest {manifest.AMA_JobReference} does not have a suitable transmitted message.");
						manifest = null;
					}
					else
					{
						Logger.Log($"Matched by manifest reference {manifest.AMA_JobReference}. Last sent message {transmittedMessage.EM_MessageNum}");
					}
				}
			}

			return (manifest, transmittedMessage);
		}

		IcsSsGreatBritainEDIMessage FindOutboundMessageByCorrelationId(IcsSsGreatBritainEDIMessage message)
		{
			IcsSsGreatBritainEDIMessage transmittedMesasge = null;

			if (message?.EM_ApplicationReference.IsEmpty != true)
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationReference, message.EM_ApplicationReference);
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbMessageICSGreatBritain);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				transmittedMesasge = message.Factory.LoadTop1<IcsSsGreatBritainEDIMessage>(query);
			}
			return transmittedMesasge;
		}

		IcsSsGreatBritainEDIMessage FindOutboundMessageByMessageNum(IcsSsGreatBritainEDIMessage message, XmlDocument messageXml)
		{
			IcsSsGreatBritainEDIMessage outboundMessage = null;
			var messageNum = messageXml.SelectSingleNode("//MesIdeMES19")?.InnerText;
			if (string.IsNullOrEmpty(messageNum))
			{
				Logger.LogWarning("Cannot match by message number as MesIdeMES19 is not set in the message");
			}
			else
			{
				var query = new ZQuery(EDIMessageSchema.EM_MessageNum, messageNum) { MaximumRows = 2 };
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbMessageICSGreatBritain);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				var matches = message.Factory.Load<IcsSsGreatBritainEDIMessage>(query);
				if (matches.Length == 1)
				{
					outboundMessage = matches[0];
				}
				else if (matches.Length == 0)
				{
					Logger.LogWarning($"Tried to match outgoing message {messageNum} but it was not found");
				}
				else
				{
					Logger.LogWarning($"Tried to match outgoing message {messageNum} but multiple matches were found");
				}
			}
			return outboundMessage;
		}

		AsycudaManifestHeaderSS FindManifestByJobReference(IcsSsGreatBritainEDIMessage message, XmlDocument messageXml)
		{
			AsycudaManifestHeaderSS manifest = null;
			var jobReference = messageXml.SelectSingleNode("//HEAHEA/RefNumHEA4")?.InnerText;

			if (string.IsNullOrEmpty(jobReference))
			{
				Logger.LogWarning("Cannot match by job reference as RefNumHEA4 is not set in the message");
			}
			else
			{
				var query = new ZQuery(AsycudaManifestHeaderSchema.AMA_JobReference, jobReference) { MaximumRows = 2 };
				var matches = message.Factory.Load<AsycudaManifestHeaderSS>(query);
				if (matches.Length == 1)
				{
					manifest = matches[0];
				}
				else if (matches.Length == 0)
				{
					Logger.LogWarning($"Tried to match manifest reference {jobReference} but it was not found");
				}
				else
				{
					Logger.LogWarning($"Tried to match manifest reference {jobReference} but multiple matches were found");
				}
			}

			return manifest;
		}

		protected string GetMessageTypeInterpretation(string messageTypeDescription) => MessagePrettierCss.CSS + $"<h3>{Argument.NotNull(messageTypeDescription, nameof(messageTypeDescription))}</h3>";

		protected string GenerateFunctionalErrorsInterpretation(List<(CargoWise.Customs.GB.MessageDefinitions.ICS.TCL.FunctionalErrorCodes errTypEr11, string errPoiEr12, string errReaEr13, string oriAttValEr14)> funerrer1)
		{
			var result = string.Empty;
			if (funerrer1?.Count > 0)
			{
				result = $"Functional Error{(funerrer1.Count > 1 ? "s" : "")}<br>";
				var tableCreator = new HtmlTableCreator(columnTitles: ["Error Type", "Error Pointer", "Error Reason", "Original Value"]);
				foreach (var (errTypEr11, errPoiEr12, errReaEr13, oriAttValEr14) in funerrer1)
				{
					var typeString = errTypEr11.ToString();
					if (typeString.StartsWith("Item"))
					{
						typeString = typeString.Remove(0, 4);
					}
					tableCreator.WriteRow(typeString, errPoiEr12, errReaEr13, oriAttValEr14);
				}
				result += tableCreator.ToHtml();
			}
			return result;
		}

		public new LoggingInformation Logger => base.Logger;
	}
}

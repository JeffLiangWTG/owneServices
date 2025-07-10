using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.SoapEnvelope.msg_ebms_3;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.SoapEnvelope.S12;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class SoapEnvelopeMessageProcessor : MessageProcessorWithEmailNotification<Envelope>
	{
		enum SeverityType
		{
			Unknown,
			Receipt,
			Failure
		}

		public SoapEnvelopeMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		SeverityType CurrentSeverityType { get; set; }

		string EmailBodyWithErrors { get; set; }

		protected override string MessageFriendlyNameCore => Res.GetString("51a3d2f4-65ed-447c-8f26-63e21c57e6cf", "Confirmation Response");

		protected override void ProcessMessageSendEmailNotification(BusinessObjectFactory factory, AsycudaManifestHeader manifestHeader, Envelope messageObject)
		{
			UpdateManifestHeader(manifestHeader, messageObject);

			if (CurrentSeverityType == SeverityType.Failure)
			{
				SendEmailNotification(factory, manifestHeader, messageObject);
			}
		}

		protected override void SetMessageInterpretation(EDIMessage message, AsycudaManifestHeader manifestHeader, Envelope messageObject)
		{
			if (CurrentSeverityType == SeverityType.Failure)
			{
				message.EM_MessageInterpretation = GenerateEmailBodyText(manifestHeader, messageObject);
			}
		}

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Envelope messageObject)
		{
			return EmailBodyWithErrors ?? GenerateEmailSubjectText(manifestHeader, messageObject);
		}

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Envelope messageObject)
		{
			return Res.GetString("a78f12f2-9bf0-421a-aa5e-7d0d1f20b051", "ICS2 - Confirmation Response with {0}(s) for {1}", CurrentSeverityType, manifestHeader.AMA_JobReference);
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2ErrorsTo;

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Envelope messageObject)
		{
			CurrentSeverityType = SeverityType.Unknown;
			EmailBodyWithErrors = null;

			var signalMessage = BuildSignalMessage(messageObject);

			if (signalMessage != null)
			{
				var receipt = signalMessage.Receipt;

				if (receipt != null && receipt.Any.Count > 0)
				{
					CurrentSeverityType = SeverityType.Receipt;
					if (manifestHeader.AMA_MessageStatus == MessageStatusCodeList.Codes.Awaiting)
					{
						manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
					}
				}
				else
				{
					var errors = signalMessage.Error;

					if (errors != null)
					{
						var htmlBuilder = new ZStringBuilder();
						htmlBuilder.AppendLine();

						var errorInfoTableBuilder = new HtmlTableCreator(
						new[]
						{
							Res.GetString("3a486ec7-d8d7-437b-a291-deee33cfef7f", "Category"),
							Res.GetString("ca1093bb-d469-48e8-986b-483b408d3a3e", "Code"),
							Res.GetString("76e46b0e-7334-434c-bc2f-0902b510eaf8", "Severity"),
							Res.GetString("4104d683-6791-4e6a-8256-e16fac27ca1d", "Short Description"),
							Res.GetString("d4fb0309-01c7-43fa-b084-2d4823f6c704", "Long Description"),
							Res.GetString("407cf375-0b08-416c-9f47-9c5ccb7e41b8", "Details"),
						});

						var hasFailure = false;

						foreach (var error in errors)
						{
							var severity = error.Severity ?? string.Empty;
							hasFailure = hasFailure || severity.Equals(nameof(SeverityType.Failure), StringComparison.InvariantCultureIgnoreCase);

							errorInfoTableBuilder.WriteRow(error.Category, error.ErrorCode, severity, error.ShortDescription, error.Description?.Value, error.ErrorDetail);
						}

						if (hasFailure)
						{
							CurrentSeverityType = SeverityType.Failure;
							if (manifestHeader.AMA_MessageStatus == MessageStatusCodeList.Codes.Awaiting)
							{
								manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;
							}
						}

						htmlBuilder.Append(errorInfoTableBuilder.ToHtml());
						EmailBodyWithErrors = htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
					}
				}

				UpdateRequestHeadersIfNeeded(manifestHeader, messageObject);
			}
		}

		void UpdateRequestHeadersIfNeeded(AsycudaManifestHeader manifestHeader, Envelope envelope)
		{
			var transmitMessage = TrGetTransmitMessage(envelope, manifestHeader.Factory);

			var messageType = transmitMessage?.EM_MessageType ?? ZString.Empty;
			if (messageType == MessageTypes.Codes.R02 || messageType == MessageTypes.Codes.R03)
			{
				manifestHeader.RequestHeaders
					.Cast<RequestHeader>()
					.Where(x => x.IsAwaiting)
					.ForEach(x => x.EUS_Status = MessageStatusCodeList.Codes.Sent);
			}
		}

		protected override Func<Envelope, EDIMessage, AsycudaManifestHeader> FindManifestHeaderFromSpecificContext => (envelope, message) =>
		{
			var transmitMessage = TrGetTransmitMessage(envelope, message.Factory);
			return transmitMessage?.EM_LinkedObject as AsycudaManifestHeader;
		};

		EDIMessage TrGetTransmitMessage(Envelope envelope, BusinessObjectFactory factory)
		{
			EDIMessage result = null;

			var signalMessage = BuildSignalMessage(envelope);
			if (signalMessage != null)
			{
				var refToMessageId = signalMessage.MessageInfo?.RefToMessageId;
				var interchangePK = refToMessageId?.Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

				if (ZGuid.TryParse(interchangePK, out var pk))
				{
					var query = new ZQuery(EDIMessageSchema.EM_EI, pk);
					query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
					query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.IC2);
					query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.ProcessedOK);
					query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + " desc";

					result = factory.LoadTop1<EDIMessage>(query);
				}
			}

			return result;
		}

		SignalMessage BuildSignalMessage(Envelope envelope)
		{
			var element = envelope?.Header?.Any?.FirstOrDefault();
			var xml = element?.OuterXml;

			var msgObject = string.IsNullOrWhiteSpace(xml)
				? null
				: XmlObjectSerializer.Deserialize<CargoWise.Customs.EU.MessageDefinitions.ICS2.SoapEnvelope.msg_ebms_3.Messaging>(xml);

			return msgObject?.SignalMessage?.FirstOrDefault();
		}
	}
}

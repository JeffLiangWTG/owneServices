using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Context = Enterprise.UniversalDataBuss.DataObjects.Universal.Context;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCEventDecoder
	{
		public NEXDOCEventDecoder(IXmlEventValueObject eventData, IXmlImportLogger logger)
		{
			EventData = (Event)eventData;
			EventValues = eventData;
			Logger = logger;
			Notices = Array.Empty<EXDOCMessageDecoderNotice>();
			Lines = Array.Empty<NEXDOCEventLineDecoder>();
		}

		public IXmlImportLogger Logger { get; }
		public IXmlEventValueObject EventValues { get; }
		Event EventData { get; }

		#region properties

		public string MessageType
		{
			get
			{
				return messageType?.ToString() ?? GetResponseType(EventData);
			}
		}

		ZString? messageType;

		public ZBool EmailNotificationRequired { get; protected set; } = true;

		public EXDOCMessageDecoderNotice[] Notices { get; protected set; }
		public NEXDOCEventLineDecoder[] Lines { get; protected set; }

		public ZString ComplianceStatus => (complianceStatus ?? (complianceStatus = EventValues.Context.ComplianceStatus.GetValueOrDefault())).Value;
		ZString? complianceStatus;

		public ZString ConsigneeDetails
		{
			get
			{
				if (!consigneeDetails.HasValue)
				{
					var consigneeDetailBuilder = new ZStringBuilder();
					consigneeDetailBuilder.AppendLine("Name: " + GetContextValue(Constants.EventContextTypes.ConsigneeName));
					consigneeDetailBuilder.AppendLine("Street Address 1: " + GetContextValue(Constants.EventContextTypes.ConsigneeStreetAddress1));
					consigneeDetailBuilder.AppendLine("Street Address 2: " + GetContextValue(Constants.EventContextTypes.ConsigneeStreetAddress2));
					consigneeDetailBuilder.AppendLine("City: " + GetContextValue(Constants.EventContextTypes.ConsigneeCity));
					consigneeDetailBuilder.AppendLine("State: " + GetContextValue(Constants.EventContextTypes.ConsigneeState));
					consigneeDetailBuilder.AppendLine("Country/Region: " + GetContextValue(Constants.EventContextTypes.ConsigneeCountry));
					consigneeDetailBuilder.AppendLine("Postal Code: " + GetContextValue(Constants.EventContextTypes.ConsigneePostalCode));
					consigneeDetailBuilder.AppendLine("Phone Number: " + GetContextValue(Constants.EventContextTypes.ConsigneePhoneNumber));
					consigneeDetailBuilder.AppendLine("Phone Number Type: " + GetContextValue(Constants.EventContextTypes.ConsigneePhoneNumberType));
					consigneeDetailBuilder.AppendLine("Representative: " + GetContextValue(Constants.EventContextTypes.ConsigneeRepresentative));
					consigneeDetails = consigneeDetailBuilder.ToString();
				}
				return consigneeDetails.Value;
			}
		}
		ZString? consigneeDetails;

		public ZString CustomsAuthorityNumber => (customsAuthorityNumber ?? (customsAuthorityNumber = EventValues.Context.ECNNumber.GetValueOrDefault())).Value;
		ZString? customsAuthorityNumber;

		public ZString DepartureDate => (departureDate ?? (departureDate = GetContextValue(Constants.EventContextTypes.DepartureDate))).Value;
		ZString? departureDate;

		public ZString ExporterReference => (exporterReference ?? (exporterReference = GetContextValue(Constants.EventContextTypes.ExporterReference))).Value;
		ZString? exporterReference;

		public ZString NotificationText => (notificationText ?? (notificationText = EventValues.Context.NotificationText.GetValueOrDefault())).Value;
		ZString? notificationText;

		public ZString NotificationTitle => (notificationTitle ?? (notificationTitle = EventValues.Context.NotificationTitle.GetValueOrDefault())).Value;
		ZString? notificationTitle;

		public ZString NotificationType => (notificationType ?? (notificationType = EventValues.Context.NotificationType.GetValueOrDefault())).Value;
		ZString? notificationType;

		public ZString PermitNumber => (permitNumber ?? (permitNumber = GetContextValue(Constants.EventContextTypes.PermitNumber))).Value;
		ZString? permitNumber;

		public ZString RexNumber => (rexNumber ?? (rexNumber = EventValues.Context.RexNumber.GetValueOrDefault())).Value;
		ZString? rexNumber;

		public ZString RexResponseType => (rexResponseType ?? (rexResponseType = EventValues.Context.RexResponseType.GetValueOrDefault())).Value;
		ZString? rexResponseType;

		public ZDateTimeOffset LastAmendDateTime => (lastAmendDateTime ?? (lastAmendDateTime = GetContextValueASZDateTimeOffset(Constants.EventContextTypes.LastAmendDateTime))).Value;
		ZDateTimeOffset? lastAmendDateTime;

		public AttachmentDef[] Attachments => attachments ?? (attachments = EventData.AttachedDocumentCollection.Where(x => x.ImageData != null).Select(x => new AttachmentDef(x.FileName, x.ImageData.ToByteArray())).ToArray());
		AttachmentDef[] attachments;

		public EXDOCMessageDecoderNotice[] ValidationNotices
		{
			get
			{
				if (validationNotices == null)
				{
					var notices = EventData.ContextCollection.Where(x => IsContextType(x, Constants.EventContextTypes.ValidationNotice) && x.Value.GetValueOrDefault().EqualsIgnoringCase(Constants.EventContextTypes.Notice));
					validationNotices = notices.Select(x => new EXDOCMessageDecoderNotice()
					{
						lineNumber = GetContextValue(x.SubContextCollection, Constants.EventContextTypes.NoticeID),
						errorMessageIdentifier = GetContextValue(x.SubContextCollection, Constants.EventContextTypes.NoticeType),
						narrativeMessage = GetContextValue(x.SubContextCollection, Constants.EventContextTypes.NoticeMessage)
					}).ToArray();
				}
				return validationNotices;
			}
		}
		EXDOCMessageDecoderNotice[] validationNotices;

		#endregion

		public string GetResponseType(Event universalEvent)
		{
			var result = RFPMessageInterpretationGenerator.Constants.ReponseType.Success;
			var contexts = universalEvent.ContextCollection;

			var messageStatus = GetContextValue(contexts, RFPMessageInterpretationGenerator.Constants.ContextType.MessageStatus);
			if (messageStatus.EqualsIgnoringCase(RFPMessageInterpretationGenerator.Constants.ContextValue.MessageStatusError))
			{
				result = RFPMessageInterpretationGenerator.Constants.ReponseType.Error;
			}
			else if (contexts.Any(x => IsContextType(x, RFPMessageInterpretationGenerator.Constants.ContextType.NotificationTitle) || IsContextType(x, RFPMessageInterpretationGenerator.Constants.ContextType.NotificationText)))
			{
				result = RFPMessageInterpretationGenerator.Constants.ReponseType.Notify;
			}
			else if (contexts.Count == 1 && IsContextType(contexts.FirstOrDefault(), RFPMessageInterpretationGenerator.Constants.ContextType.Message))
			{
				var eventReference = universalEvent.EventReference.GetValueOrDefault();

				result = eventReference.EqualsIgnoringCase(Constants.EventReference.Reissue)
					? RFPMessageInterpretationGenerator.Constants.ReponseType.Receipt
					: eventReference.EqualsIgnoringCase(Constants.EventReference.Replace)
						? RFPMessageInterpretationGenerator.Constants.ReponseType.Replace
						: result;
			}

			return result;
		}

		public bool Process()
		{
			FormattableString message = null;
			var eventReference = EventValues.EventReference;
			var messageStatus = EventValues.Context.MessageStatus.GetValueOrDefault();

			if (eventReference == Constants.EventReference.Notify)
			{
				ProcessNotification();
			}
			else if (eventReference == Constants.EventReference.Reissue)
			{
				ProcessReIssueReceipt();
			}
			else if (eventReference == Constants.EventReference.Replace)
			{
				ProcessReplaceReceipt();
			}
			else if (messageStatus == Constants.MessageStatus.Error)
			{
				ProcessError();
			}
			else if (!RexNumber.IsEmpty)
			{
				ProcessSuccess();
			}
			else if (ValidationNotices.Any())
			{
				ProcessValidationError();
			}
			else
			{
				message = $"Unrecognised NEXDOC Message. EventReference = {eventReference}, REXNumber = {RexNumber}, MessageStatus = {messageStatus}";
			}

			if (message != null)
			{
				Logger?.LogBoth(LogType.Error, message.ToString(CultureInfo.InvariantCulture));
				return false;
			}

			return true;
		}

		void ProcessNotification()
		{
			Notices = new EXDOCMessageDecoderNotice[]
			{
				new EXDOCMessageDecoderNotice() { narrativeMessage = NotificationTitle },
				new EXDOCMessageDecoderNotice() { narrativeMessage = NotificationText }
			};
		}

		void ProcessReIssueReceipt()
		{
			EmailNotificationRequired = false;
		}

		void ProcessReplaceReceipt()
		{
			EmailNotificationRequired = false;
		}

		void ProcessSuccess()
		{
			var allLines = EventData.ContextCollection.Where(x => IsContextType(x, Constants.EventContextTypes.LineNumber));
			Lines = allLines.Select(x => new NEXDOCEventLineDecoder(x)).Where(d => d.IsValid).ToArray();
		}

		void ProcessError()
		{
			var messages = EventData.ContextCollection.Where(x => IsContextType(x, Constants.EventContextTypes.Message)).Select(x => x.Value.GetValueOrDefault());
			Notices = messages.Where(m => !m.IsEmpty).Select(m => new EXDOCMessageDecoderNotice() { narrativeMessage = m }).ToArray();
		}

		void ProcessValidationError()
		{
			messageType = RFPMessageInterpretationGenerator.Constants.ReponseType.Error;
		}

		#region ContextValue

		ZDateTimeOffset GetContextValueASZDateTimeOffset(ZString type)
		{
			var result = ZDateTimeOffset.Empty;
			var dateString = GetContextValue(type);
			if (!dateString.IsEmpty)
			{
				if (DateTime.TryParse(dateString, out var dateValue))
				{
					result = new ZDateTimeOffset(dateValue.ToUniversalTime());
				}
				else
				{
					Logger?.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Context Collection Key ({0}) has an invalid datetime value '{1}'", type, dateString));
				}
			}

			return result;
		}

		ZString GetContextValue(ZString type)
		{
			return GetContextValue(EventData.ContextCollection, type);
		}

		ZString GetContextValue(IEnumerable<Context> contexts, ZString type)
		{
			var context = contexts?.FirstOrDefault(x => IsContextType(x, type));
			return context?.Value.GetValueOrDefault() ?? ZString.Empty;
		}

		bool IsContextType(Context context, ZString type)
		{
			return context.Type.Type.GetValueOrDefault().EqualsIgnoringCase(type);
		}

		#endregion
	}
}

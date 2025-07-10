using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRPortsResponseMessageProcessor : ApplicationTypeMessageProcessor
	{
		public FRPortsResponseMessageProcessor(LoggingInformation loggingInformation) : base(loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"France Ports Response";
		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.FRPortMessage;
		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => Array.Empty<ZString>();

		protected override void ProcessMessageCore(EDIMessage message)
		{
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(message.EM_MessageText);

			UpdateMessage(message, xml);
			UpdateLinkedObject(message, xml);
		}

		void UpdateMessage(EDIMessage message, XmlDocument xml)
		{
			message.EM_MessageInterpretation = GetMessageInterpretation(xml);
			message.EM_Status = MessageStatusCodeList.Codes.OK;

			var messageType = GetMessageType(xml);
			message.EM_MessageType = MessageTypeList.Codes.POR;
			message.EM_MessageSubType = messageType;
		}

		static ZString GetReadableDateAndTime(XmlDocument xml)
		{
			var dateTime = GetResponseDateTime(xml);
			var result = ZString.Empty;
			if (dateTime.IsEmpty)
			{
				result = GetResponseDate(xml) + GetResponseTime(xml);
			}
			else
			{
				result = dateTime.ToString("dd/MM/yyyy hh:mm");
			}
			return result;
		}

		static ZString GetParagraphInterpretation(ZString text)
		{
			return text.IsEmpty ? ZString.Empty : new ZString(FormattableString.Invariant($"<p>{text}</p>"));
		}

		static ZString GetMessageInterpretation(XmlDocument xml)
		{
			var sb = new ZStringBuilder();

			var responseDateTime = GetReadableDateAndTime(xml);
			var responseSatus = IsValidStatus(xml) ? ValidReadableStatus : RefusedReadableStatus;
			var trackingReference = ProcessorHelper.GetElementTextByTagNameWithDefaultValue(xml, "REF_TRC");

			if (GetMessageType(xml) != MessageSubTypeList.Codes.CAED)
			{
				sb.Append(GetParagraphInterpretation(FormattableString.Invariant($"Tracking reference : {trackingReference}")));
			}
			sb.Append(GetParagraphInterpretation(FormattableString.Invariant($"Status : {responseSatus}")));
			sb.Append(GetParagraphInterpretation(FormattableString.Invariant($"Status granted on: {responseDateTime}")));
			GetReadableErrors(xml, ref sb);
			return sb.ToString();
		}

		static void GetReadableErrors(XmlDocument xml, ref ZStringBuilder sb)
		{
			var errors = xml.GetElementsByTagName((NoResString)"erreur");
			foreach (XmlNode errorNode in errors)
			{
				var errorCode = errorNode.Attributes?[(NoResString)"code"].Value ?? ZString.Empty;
				var errorDescription = errorNode.SelectSingleNode((NoResString)"libelle")?.InnerText ?? ZString.Empty;
				sb.Append(GetParagraphInterpretation(FormattableString.Invariant($"Error {errorCode} : {errorDescription}")));
			}
		}

		static ZString GetMessageType(XmlDocument xml) => ProcessorHelper.GetElementTextByTagNameWithDefaultValue(xml, "TYPE").Substring(0, 3);

		void UpdateLinkedObject(EDIMessage message, XmlDocument xml)
		{
			var interchangeNumber = ProcessorHelper.GetElementTextByTagNameWithDefaultValue(xml, "InterchangeNumber");
			var outgoingMessage = GetOutgoingMessage(message.Factory, interchangeNumber);

			if (outgoingMessage != null)
			{
				outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;

				if (outgoingMessage.EM_LinkedObject is IFRMessagesOwner messagesOwner)
				{
					messagesOwner.Messages.Add(message);
				}

				if (outgoingMessage.EM_LinkedObject is IStmALogParent stmAlogParent)
				{
					var messageType = GetMessageType(xml);
					var zDateTimeOffset = GetResponseDateTime(xml);
					if (IsValidStatus(xml))
					{
						stmAlogParent.Logs.AddNew(AutoEvents.MessageAccepted, messageType, new ZDateTimeOffset(zDateTimeOffset));
					}
					else
					{
						stmAlogParent.Logs.AddNew(AutoEvents.MessageRejected, messageType, new ZDateTimeOffset(zDateTimeOffset));
					}
				}
			}
		}

		static EDIMessage GetOutgoingMessage(BusinessObjectFactory factory, ZString interchangeNumber)
		{
			EDIMessage result = null;

			var query = new ZQuery(EDIInterchangeSchema.EI_InterchangeNum, interchangeNumber);
			query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, ApplicationCodeList.Codes.FRPortMessage);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
			var outgoingInterchange = factory.LoadTop1<EDIInterchange>(query);

			if (outgoingInterchange != null)
			{
				result = outgoingInterchange.ContainedMessages.Cast<EDIMessage>().FirstOrDefault();
			}

			return result;
		}

		static ZString ValidReadableStatus => Res.GetString("0203FE9F-82A0-4A66-ACD4-440BBE15D01F", "Valid");
		static ZString RefusedReadableStatus => Res.GetString("86808DF6-00CA-4DED-A1E2-57C2DD75B831", "Refused");
		static ZBool IsValidStatus(XmlDocument xml) => ProcessorHelper.GetElementTextByTagNameWithDefaultValue(xml, "STATUT") == ValidStatus;
		static ZString GetResponseDate(XmlDocument xml) => ProcessorHelper.GetElementTextByTagNameWithDefaultValue(xml, "AP_DATE");
		static ZString GetResponseTime(XmlDocument xml) => ProcessorHelper.GetElementTextByTagNameWithDefaultValue(xml, "AP_HEURE");
		static ZDateTime GetResponseDateTime(XmlDocument xml)
		{
			if (ZDateTime.TryParseExact(GetResponseDate(xml) + GetResponseTime(xml), out var result, "yyyyMMddhhmm"))
			{
				return result;
			}
			return ZDateTime.Empty;
		}

		const string ValidStatus = "V";
	}
}


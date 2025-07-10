using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Business;

public sealed class FileProcessingErrorMessageProcessor : IEmailMessageProcessor
{
	public FileProcessingErrorMessageProcessor()
	{
	}

	public bool CanProcess(EmailInfo emailInfo) => !emailInfo.HasAttachments && !string.IsNullOrEmpty(emailInfo.Subject) && MessageIDRegex.IsMatch(emailInfo.Subject);

	public BusinessObject GetLinkedObject(EDIMessage message, EmailInfo emailInfo, LoggingInformation logger)
	{
		return FindMatchingMessage(message, emailInfo, logger)?.EM_LinkedObject;
	}

	public bool Process(EDIMessage message, EmailInfo emailInfo, LoggingInformation logger)
	{
		if (FindMatchingMessage(message, emailInfo, logger) is EDIMessage outgoingMessage)
		{
			message.EM_MessageType = outgoingMessage.EM_MessageType;
			message.EM_MessageSubType = EDIMessageSubTypeList.GetNegativeSubType(outgoingMessage.EM_MessageSubType);
			SetMessageStatus(outgoingMessage.EM_LinkedObject);
			return true;
		}
		return false;
	}

	EDIMessage FindMatchingMessage(EDIMessage message, EmailInfo emailInfo, LoggingInformation logger)
	{
		var subjectInfo = ParseEmailSubjectInfo(emailInfo.Subject);
		if (subjectInfo.IsEmpty)
		{
			logger.Log($"Cannot extract Message Number, Message Sub Type, Filling Date & Receiver ID from email subject: '{emailInfo.Subject}'.", LogType.Error);
			return null;
		}
		var outgoingMessage = GetOutgoingMessageByEmail(message.Factory, subjectInfo);
		if (outgoingMessage == null)
		{
			logger.Log($"Failed to load the outgoing message by email subject info: {subjectInfo}.", LogType.Warning);
		}
		return outgoingMessage;
	}

	internal static EDIMessage GetOutgoingMessageByEmail(BusinessObjectFactory factory, EmailSubjectInfo emailSubject)
	{
		var query = new ZQuery(EDIMessageSchema.EM_MessageSubType, emailSubject.MessageSubType);
		query.AddToFilter(EDIMessageSchema.EM_MessageNum, emailSubject.MessageNumber);
		query.AddToFilter(EDIMessageSchema.EM_MessageOwner, emailSubject.ReceiverID);
		query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.INCustoms);
		query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
		query.OrderBy = EDIMessage.Schema.EM_SystemCreateTimeUtc + OrderByClause.Descending;
		var ediMessages = factory.Load<EDIMessage>(query);
		return ediMessages.FirstOrDefault(x => x.EM_MessageDateTime.Date.ToDateTime() == emailSubject.FilingDate);
	}

	void SetMessageStatus(BusinessObject linkObject)
	{
		if (linkObject is IMessageAttachee messageAttachee)
		{
			messageAttachee.MessageStatus = MessageStatusList.Codes.ErrorResponseReceived;
		}
		else
		{
			ErrorReporter.ReportOnce(message: $"LinkObject is not IMessageAttachee, type is {linkObject.GetType().FullName}");
		}
	}

	#region Email Subject Parser

	internal static EmailSubjectInfo ParseEmailSubjectInfo(string emailSubject)
	{
		var result = new EmailSubjectInfo();

		if (!string.IsNullOrEmpty(emailSubject))
		{
			var messageSubType = string.Empty;
			var messageID = (ZString)MessageIDRegex.Match(emailSubject).Groups["MessageID"]?.Value;
			if (!messageID.IsEmpty && !MessageSubTypeDictionary.TryGetValue(messageID, out messageSubType))
			{
				ErrorReporter.ReportOnce("INCMessageProcessorCannotGetMessageSubTypeByMessageID", $"Cannot get Message Sub Type by Message ID : {messageID}.");
			}
			result.MessageSubType = messageSubType;

			var fillingDate = FilingDateRegex.Match(emailSubject).Groups["FilingDate"];
			if (fillingDate != null && DateTime.TryParseExact(fillingDate.Value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
			{
				result.FilingDate = date;
			}

			result.MessageNumber = MessageNumRegex.Match(emailSubject).Groups["ControlNo"]?.Value;
			result.ReceiverID = ReceiverIDRegex.Match(emailSubject).Groups["ReceiverID"]?.Value;
		}
		return result;
	}

	static readonly Regex MessageIDRegex = new(@"Message\s*ID\s*(?<MessageID>[A-Z0-9_]+)", RegexOptions.IgnoreCase);
	static readonly Regex MessageNumRegex = new(@"control\s*no.\s*(?<ControlNo>[0-9]+)", RegexOptions.IgnoreCase);
	static readonly Regex FilingDateRegex = new(@"filing\s*date\s*(?<FilingDate>[0-9]+)", RegexOptions.IgnoreCase);
	static readonly Regex ReceiverIDRegex = new(@"Receiver\s*ID\s*(?<ReceiverID>[A-Z0-9_]+)", RegexOptions.IgnoreCase);

	internal static Dictionary<string, string> MessageSubTypeDictionary => new()
	{
		{ Constants.MessageID.AirCgm, EDIMessageSubTypeList.Codes.AirCgm },
		{ Constants.MessageID.SeaCgm, EDIMessageSubTypeList.Codes.SeaCgm },
		{ Constants.MessageID.ShippingBill, EDIMessageSubTypeList.Codes.ShippingBillFresh },
		{ Constants.MessageID.ReplyToShippingBill, EDIMessageSubTypeList.Codes.ReplyToShippingBill },
		{ Constants.MessageID.GoodsRegistration, EDIMessageSubTypeList.Codes.GoodsRegistration },
		{ Constants.MessageID.BillOfEntry, EDIMessageSubTypeList.Codes.BillOfEntry },
		{ Constants.MessageID.BillOfEntryAmendment, EDIMessageSubTypeList.Codes.BillOfEntryAmendment },
		{ Constants.MessageID.ReplyToBillOfEntry, EDIMessageSubTypeList.Codes.ReplyToBillOfEntry },
		{ Constants.MessageID.AirIgm, EDIMessageSubTypeList.Codes.AirIgm },
		{ Constants.MessageID.SeaIgm, EDIMessageSubTypeList.Codes.SeaIgm },
		{ Constants.MessageID.AirEgm, EDIMessageSubTypeList.Codes.AirEgm },
		{ Constants.MessageID.SeaEgm, EDIMessageSubTypeList.Codes.SeaEgm },
	};

	internal class EmailSubjectInfo
	{
		public ZString MessageNumber { get; set; }
		public ZString MessageSubType { get; set; }
		public DateTime? FilingDate { get; set; }
		public ZString ReceiverID { get; set; }

		public bool IsEmpty => MessageNumber.IsEmpty
			|| MessageSubType.IsEmpty
			|| ReceiverID.IsEmpty
			|| FilingDate == null;

		public override string ToString()
		{
			return $"Message Number: {MessageNumber}, Message Sub Type: {MessageSubType}, Filing Date: {FilingDate?.ToString("yyyyMMdd")}, Receiver ID: {ReceiverID}";
		}
	}

	#endregion
}

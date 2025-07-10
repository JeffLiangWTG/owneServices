using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Integration;

namespace Enterprise.Customs.IN.Business;

public sealed class ShippingBillQueryPendingMessageProcessor : IEmailMessageProcessor
{
	public ShippingBillQueryPendingMessageProcessor()
	{
	}

	public bool CanProcess(EmailInfo emailInfo) => !emailInfo.HasAttachments && EmailSubjectRegex.IsMatch(emailInfo.Subject);

	public BusinessObject GetLinkedObject(EDIMessage message, EmailInfo emailInfo, LoggingInformation logger)
	{
		if (EmailBodyRegex.Match(emailInfo.TextBody) is { } match && match.Success)
		{
			var sbNumber = match.Groups["SBNumber"].Value;
			var sbDate = match.Groups["SBDate"].Value;
			if (!sbNumber.IsNullOrEmpty() && DateTime.TryParseExact(sbDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
			{
				var entryHeader = new CusEntryHeader.Loader(message.Factory).FindByEntryNumber(CusEntryNumberTypes.Indian.ShippingBill, sbNumber, date);
				if (entryHeader == null)
				{
					logger.Log($"Entry Header with Shipping Bill number {sbNumber} and date {date:yyyy-MM-dd} not found.", LogType.Warning);
				}
				return entryHeader;
			}
			else
			{
				logger.Log("Cannot extract SB Number and SB Date from email body.", LogType.Error);
			}
		}
		else
		{
			logger.Log("Email body does not match expected format for Shipping Bill query.", LogType.Error);
		}
		return null;
	}

	public bool Process(EDIMessage message, EmailInfo emailInfo, LoggingInformation logger)
	{
		var result = false;
		if (message.EM_LinkedObject is CusEntryHeader)
		{
			message.EM_MessageType = EDIMessageTypeList.Codes.ShippingBill;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.ShippingBillQuery;
			result = true;
		}

		return result;
	}

	static readonly Regex EmailBodyRegex = new Regex(@"SB\s*number\s*(?<SBNumber>\d{7})\s*dated\s*(?<SBDate>\d{4}-\d{2}-\d{2})", RegexOptions.IgnoreCase);
	static readonly Regex EmailSubjectRegex = new Regex(@"shipping\s*bill\s*pending\s*for\s*query\s*reply\s*", RegexOptions.IgnoreCase);
}

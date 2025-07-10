using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Events = Enterprise.ZArchitecture.Business.AutoEvents;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public static class UniversalEventHelper
	{
		#region For Context Value

		public static ZString GetContextValueByType(this UniversalEvent eventDataObject, ZString type)
		{
			var result = ZString.Empty;
			if (eventDataObject?.ContextCollection != null)
			{
				var context = eventDataObject.ContextCollection.FirstOrDefault(o => o.Type != null && o.Type.Type.GetValueOrDefault() == type);
				if (context != null)
				{
					result = context.Value.GetValueOrDefault();
				}
			}
			return result;
		}

		public static ZDateTime GetContextValueByTypeAsDateTime(this UniversalEvent eventDataObject, ZString type, ZDateTime defaultValue)
		{
			var value = GetContextValueByType(eventDataObject, type);
			return !value.IsEmpty && (DateTime.TryParse(value, out var result)
				|| DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result)) ? result : defaultValue;
		}

		#endregion

		#region For Entry Header

		public static CusEntryHeader GetEntryHeaderFromUNINumber(ZString entryNumber, BusinessObjectFactory factory)
		{
			var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, SQLComparisonOperator.Equal, CusEntryHeaderSchema.Constants.TableName);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.China.DeclarationUnifiedNumber);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.Equal, entryNumber);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.China);

			entryHeaderQuery.AddSubQuery(CusEntryHeaderSchema.PK, entryNumQuery, JoinCondition.And);

			return factory.LoadTop1<CusEntryHeader>(entryHeaderQuery);
		}

		#endregion

		#region For Message

		public static EDIMessage GetLastOutgoingMessage(this CusEntryHeader entryHeader)
		{
			return entryHeader?.Messages.GetLastMessage(XmlEDIMessage.ApplicationCodes.UniversalDataMessaging, EDIMessageSubTypeList.Codes.XmlUniversalShipment, EDIMessage.Direction.Transmit);
		}

		public static EDIMessage GetLastIncomingdMessageUpdatedCustomsStatus(this CusEntryHeader entryHeader)
		{
			return entryHeader?.Messages.GetMatchingMessages(XmlEDIMessage.ApplicationCodes.UniversalDataMessaging, new ZString[] { Messaging.Integration.EDIMessageTypeList.Codes.XDC }, EDIMessage.Direction.Receive)
				.OfType<EDIMessage>().Where(m => m.ShouldUpdateCustomsStatus() && m.GetUniversalEvent().EventTime.HasValue)
				.OrderBy(m => m.GetUniversalEvent().EventTime).LastOrDefault();
		}

		public static bool ShouldUpdateCustomsStatus(this EDIMessage message)
		{
			var entryStatus = message?.GetUniversalEvent()?.GetContextValueByType(Constants.Universal.ContextType.EntryStatus) ?? ZString.Empty;
			return !entryStatus.IsEmpty && CustomsStatusAttributeHelper.ShouldUpdateCustomsStatus(message.Factory, entryStatus, Core.Constants.CountryCodes.China, ZDateTime.Today);
		}

		public static EDIMessage GetLastIncomingMessageUpdatedCIQStatus(this CusEntryHeader entryHeader)
		{
			return entryHeader?.Messages.GetMatchingMessages(XmlEDIMessage.ApplicationCodes.UniversalDataMessaging, new ZString[] { Messaging.Integration.EDIMessageTypeList.Codes.XDC }, EDIMessage.Direction.Receive)
				.OfType<EDIMessage>().Where(m => m.ShouldUpdateCIQStatus() && m.GetUniversalEvent().EventTime.HasValue)
				.OrderBy(x => x.GetUniversalEvent().EventTime).LastOrDefault();
		}

		public static bool ShouldUpdateCIQStatus(this EDIMessage message)
		{
			var entryStatus = message?.GetUniversalEvent()?.GetContextValueByType(Constants.Universal.ContextType.EntryStatus) ?? ZString.Empty;
			return !entryStatus.IsEmpty && CustomsStatusAttributeHelper.ShouldUpdateCIQStatus(message.Factory, entryStatus, Core.Constants.CountryCodes.China, ZDateTime.Today);
		}

		public static EDIMessage GetLastDeliverResponseMessage(this CusEntryHeader entryHeader)
		{
			return entryHeader?.Messages.GetMatchingMessages(XmlEDIMessage.ApplicationCodes.UniversalDataMessaging, new ZString[] { Messaging.Integration.EDIMessageTypeList.Codes.XDC }, EDIMessage.Direction.Receive)
				.OfType<EDIMessage>().Where(m => m.IsDeliverResponseMessage() && m.GetUniversalEvent().EventTime.HasValue)
				.OrderBy(x => x.GetUniversalEvent().EventTime).LastOrDefault();
		}

		public static bool IsDeliverResponseMessage(this EDIMessage message)
		{
			var eventType = message.GetUniversalEvent()?.EventType.GetValueOrDefault() ?? ZString.Empty;
			return eventType == Events.MessageDeliveredCode || eventType == Events.MessageRejectedCode;
		}

		internal static UniversalEvent GetUniversalEvent(this EDIMessage message)
		{
			return message?.Factory.GetCachedValue(message.PK.ToStringKey() + "_UE", () => message.GetEM_MessageTextReader().Parse<UniversalEvent>());
		}

		#endregion

		#region For Html Interpretation

		public static string GetHtmlInterpretation(this UniversalEvent eventData)
		{
			var items = new Dictionary<string, string>();
			if (eventData != null)
			{
				foreach (var item in InterpretationList)
				{
					var value = eventData.FormatContextValueByType(item.key);
					if (!value.IsEmpty)
					{
						items.Add(item.value, value);
					}
				}
			}

			return ResponseMessageProcessHelper.GetHtmlInterpretation(items);
		}

		#region SuppressResourceStringsCheckRegion

		static readonly ImmutableArray<(string key, string value)> InterpretationList = ImmutableArray.CreateRange(new List<(string key, string value)>
		{
			(Constants.Universal.EventTime,                            "通知时间"),
			(Constants.Universal.ContextType.LocalReferenceNumber,     "客户端编号"),
			(Constants.Universal.ContextType.ResponseCode,             "响应代码"),
			(Constants.Universal.ContextType.ResponseDetail,           "响应信息"),
			(Constants.Universal.ContextType.DeclarationUnifiedNumber, "统一编号"),
			(Constants.Universal.ContextType.CustomsDeclarationNumber, "报关单编号"),
			(Constants.Universal.ContextType.CustomsOffice,            "申报地海关"),
			(Constants.Universal.ContextType.CustomsDeclarationDate,   "申报日期"),
			(Constants.Universal.ContextType.ImportExportDate,         "进出口日期"),
			(Constants.Universal.ContextType.EntryStatus,              "回执代码"),
			(Constants.Universal.ContextType.Note,                     "回执说明"),
		});

		#endregion

		static ZString GetFormatByType(ZString type)
		{
			switch (type)
			{
				case Constants.Universal.EventTime:
				case Constants.Universal.ContextType.CustomsDeclarationDate:
					return (NoResString)"yyyy-MM-dd HH:mm:ss";
				case Constants.Universal.ContextType.ImportExportDate:
					return "yyyy-MM-dd";
				default:
					return ZString.Empty;
			}
		}

		public static ZString FormatContextValueByType(this UniversalEvent eventDataObject, ZString type)
		{
			var formatedValue = ZString.Empty;

			if (eventDataObject != null)
			{
				var format = GetFormatByType(type);
				if (format.IsEmpty)
				{
					formatedValue = eventDataObject.GetContextValueByType(type);
				}
				else
				{
					var dateTime = type == Constants.Universal.EventTime ? eventDataObject.EventTime.GetValueOrDefault().ToZDateTime() : eventDataObject.GetContextValueByTypeAsDateTime(type, ZDateTime.Empty);
					if (!dateTime.IsEmpty)
					{
						formatedValue = dateTime.ToString(format, CultureInfo.InvariantCulture);
					}
				}

				if (type == Constants.Universal.ContextType.EntryStatus)
				{
					formatedValue = formatedValue.RemoveSafe(0, 1);
				}
			}

			return formatedValue;
		}

		#endregion

		#region For Parsing

		public static ZString ParseCIQNumberFromNoteText(this UniversalEvent eventData)
		{
			var ciqNumber = ZString.Empty;
			if (eventData != null)
			{
				var note = eventData.GetContextValueByType(Constants.Universal.ContextType.Note);
				var numberReg = new Regex(@"(\[)(\d{15}|\d{18})(\])");
				var match = numberReg.Match(note);
				if (match.Success)
				{
					ciqNumber = match.Groups[2].Value;
				}
			}
			return ciqNumber;
		}

		public static ZDateTime ParseCIQIssueDateFromNoteText(this UniversalEvent eventData)
		{
			var issueDateString = ZString.Empty;
			if (eventData != null)
			{
				var note = eventData.GetContextValueByType(Constants.Universal.ContextType.Note);
				if (note.Contains((NoResString)"报检日期", StringComparison.Ordinal))
				{
					var dateReg = new Regex(@"(报检日期:)([0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2})");
					var match = dateReg.Match(note);
					if (match.Success)
					{
						issueDateString = match.Groups[2].Value;
					}
				}
			}
			return !issueDateString.IsEmpty && ZDateTime.TryParseExact(issueDateString, out var issueDate, (NoResString)"yyyy-MM-dd HH:mm:ss") ? issueDate : ZDateTime.Empty;
		}

		#endregion
	}
}

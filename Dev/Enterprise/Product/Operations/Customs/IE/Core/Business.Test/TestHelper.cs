using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.TransactionIDRequest;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	public static class TestHelper
	{
		public static void SetupTransactionIDNoOfDays(BusinessObjectFactory factory, int noOfDays)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetExistingRefSysConfigType("IETIDDAY", "No of days", "No of days");
			helper.CreateRefSysConfig("IETIDDAY", noOfDays, ZDateTime.BrettsBirthday, ZDateTime.Empty);
		}

		public static void DeleteTransactionIDNoOfDaysSetting(BusinessObjectFactory factory)
		{
			factory.Load<RefSysConfig>(new ZQuery(RefSysConfigSchema.ZRC_ZRT_NKConfigCode, "IETIDDAY")).DeleteAll();
		}

		public static EDIInterchange CreateTransactionIDRequest(BusinessObjectFactory factory, ZGuid branchPK)
		{
			var transactionIDRequest = new TransactionIdRequest()
			{
				Transactions = new Transactions()
				{
					NumberOfTxIds = 100
				}
			};
			return InterchangeCreator.CreateOutgoingInterchange(factory, EDIInterchange.ApplicationCodes.IECustomsCommon, CommonInterchangeTypeList.Codes.TransactionID, branchPK, WebServiceEndPointProvider.GetTransactionIDURL(factory, EDIInterchange.ApplicationCodes.IECustomsCommon), IEXmlObjectSerializer.Serialize(transactionIDRequest));
		}

		public static AISInboundEDIMessage GetIM415VMessage(BusinessObjectFactory factory)
		{
			var messageText = AISInterchangeProcessorTestHelper.GetStandardIE415VInterchangeText("6debb28a-c9b0-44fb-9e85-0737b42aef48", "ACPTESTIM0990446123456", "21IEDUB11A782454R2", includeResponseWrap: false);
			var message = factory.New<AISInboundEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsImport;
			message.EM_ApplicationReference = "6debb28a-c9b0-44fb-9e85-0737b42aef48";
			message.EM_MessageType = AISInterchangeTypeList.Codes.IM415V;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageText;
			return message;
		}

		public static AISInboundEDIMessage GetIM917Message(BusinessObjectFactory factory)
		{
			return GetIM917Message(factory, "020ddfa8-b792-452a-bb65-51b36a83937c");
		}

		public static AISInboundEDIMessage GetIM917Message(BusinessObjectFactory factory, string transactionId)
		{
			var messageText = AISInterchangeProcessorTestHelper.GetStandardIM917InterchangeText(transactionId, includeResponseWrap: false);
			var message = factory.New<AISInboundEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsImport;
			message.EM_ApplicationReference = transactionId;
			message.EM_MessageType = AISInterchangeTypeList.Codes.IM917;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageText;
			return message;
		}

		public static AESInboundEDIMessage GetCC521CMessage(BusinessObjectFactory factory)
		{
			return GetCC521CMessage(factory, "B29EE70C-B089-4C10-BAF8-4C98249E7DBB");
		}

		public static AESInboundEDIMessage GetCC521CMessage(BusinessObjectFactory factory, string transactionId)
		{
			var messageText = AESInterchangeProcessorTestHelper.GetStandardCC521CMailboxItemText(transactionId, includeResponseWrap: false);
			var message = factory.New<AESInboundEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			message.EM_ApplicationReference = transactionId;
			message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE521;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageText;
			return message;
		}

		public static AESInboundEDIMessage GetCC528CMessage(BusinessObjectFactory factory)
		{
			return GetCC528CMessage(factory, "71854bf4-bb4e-44d3-a3a7-2f81ac4b3db9");
		}

		public static AESInboundEDIMessage GetCC528CMessage(BusinessObjectFactory factory, string transactionId)
		{
			var messageText = AESInterchangeProcessorTestHelper.GetStandardCC528CMailboxItemText(transactionId, includeResponseWrap: false);
			var message = factory.New<AESInboundEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			message.EM_ApplicationReference = transactionId;
			message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE528;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageText;
			return message;
		}

		public static AESInboundEDIMessage GetCC551CMessage(BusinessObjectFactory factory)
		{
			return GetCC551CMessage(factory, "D25FFC7F-3BE3-46AC-AB5C-BAAA58ADBA58");
		}

		public static AESInboundEDIMessage GetCC551CMessage(BusinessObjectFactory factory, string transactionId)
		{
			var messageText = AESInterchangeProcessorTestHelper.GetStandardCC551CMailboxItemText(transactionId, includeResponseWrap: false);
			var message = factory.New<AESInboundEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			message.EM_ApplicationReference = transactionId;
			message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE551;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageText;
			return message;
		}

		public static AESInboundEDIMessage GetCC556CMessage(BusinessObjectFactory factory)
		{
			return GetCC556CMessage(factory, "0840785F-2D91-493F-A1CC-F0D6085D8D57");
		}

		public static AESInboundEDIMessage GetCC556CMessage(BusinessObjectFactory factory, string transactionId)
		{
			var messageText = AESInterchangeProcessorTestHelper.GetStandardCC556CMailboxItemText(transactionId, includeResponseWrap: false);
			var message = factory.New<AESInboundEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			message.EM_ApplicationReference = transactionId;
			message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE556;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageText;
			return message;
		}

		public static AESInboundEDIMessage GetCC560CMessage(BusinessObjectFactory factory)
		{
			return GetCC560CMessage(factory, "89E11965-BD4B-4A03-9186-1826BA45F9EC");
		}

		public static AESInboundEDIMessage GetCC560CMessage(BusinessObjectFactory factory, string transactionId)
		{
			var messageText = AESInterchangeProcessorTestHelper.GetStandardCC560CMailboxItemText(transactionId, includeResponseWrap: false);
			var message = factory.New<AESInboundEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			message.EM_ApplicationReference = transactionId;
			message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE560;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = messageText;
			return message;
		}

		public static Enterprise.Messaging.Business.EDIMessage GetIE801Message(BusinessObjectFactory factory)
		{
			return GetIE801Message(factory, "6CF98DF8-3CC8-4AED-A039-A9B552E4D2FE");
		}

		public static Enterprise.Messaging.Business.EDIMessage GetIE801Message(BusinessObjectFactory factory, string transactionId)
		{
			var message = factory.New<Enterprise.Messaging.Business.EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsEMCS;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationReference = transactionId;
			message.EM_MessageType = "801";
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}

		public static CustomsAndExciseReportInboundMessage GetPSRMessage(BusinessObjectFactory factory)
		{
			var message = factory.New<CustomsAndExciseReportInboundMessage>();
			message.EM_MessageType = CustomsAndExciseReportTypeList.Codes.PSR;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = CustomsAndExciseReportInterchangeProcessorTestHelper.CreatePSRText();
			return message;
		}

		public static AESInboundEDIMessage GetAcknowledgementMessage(BusinessObjectFactory factory)
		{
			var message = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(factory, EDIMessage.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.ExitNotification, "6884631A-2ECE-46BF-B343-22FA752A7DC2");
			message.EM_MessageSubType = CommonInterchangeTypeList.Codes.MessageAcknowledge;
			return message;
		}

		public static void AssertResponseDetail(ResponseDetail detail, Type xmlObjectType, Type processorType)
		{
			AssertionWithHtml.CombineAssertions(() =>
			{
				Assertion.AssertEquals("XmlObjectType", xmlObjectType, detail.XmlObjectType);
				Assertion.AssertEquals("ProcessorType", processorType, detail.ProcessorType);
			});
		}

		public static RefCusCodeList CreateNewOrGetExistingCusCodeList(BusinessObjectFactory factory, ZString codeType, ZString code, ZString description, string dataGrouping = Core.Constants.CountryCodes.Ireland)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping, parent: dataGrouping == Core.Constants.CountryCodes.Ireland ? helper.CreateNewOrGetExistingDataGrouping(Customs.Universal.Constants.DataGrouping.EuropeanUnion) : null);
			helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
			return helper.CreateNewOrGetExistingCusCodeList(dataGrouping, codeType, code, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		public static string RemoveLineBreakingsAndIndents(this string input) => Regex.Replace(input, @"(\r|\n|\r\n)\s*", string.Empty);

		public static string RemoveLineBreakingsAndIndents(this ZString input) => input.ToString().RemoveLineBreakingsAndIndents();

		public static string AdjustEmptyXmlTags(this string input) => Regex.Replace(input, @"\<(\w+)\s+\/\>", @"<$1/>");

		public static string AdjustEmptyXmlTags(this ZString input) => input.ToString().AdjustEmptyXmlTags();

		public static TObject FirstOrAddNew<TObject>(this IBusinessObjectCollection collection) where TObject : BusinessObject
			=> collection.Cast<TObject>().OrderBy(item => item.InstantiationTime).FirstOrDefault() ?? (TObject)collection.AddNew();

		public static TObject FirstOrAddNew<TObject>(this IBusinessObjectCollection<TObject> collection) where TObject : BusinessObject
			=> collection.OrderBy(item => item.InstantiationTime).FirstOrDefault() ?? collection.AddNew();
	}
}

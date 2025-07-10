using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	class UniversalEventHelperTest : TestCaseWithFactory
	{
		public void TestGetContextValueByType()
		{
			var eventData = new Event
			{
				EventTime = new ZDateTimeOffset(2019, 05, 27, 15, 7, 12),
				ContextCollection = new List<Context>
				{
					new Context { Type = Constants.Universal.ContextType.Note, Value = "Note Text" },
					new Context { Type = Constants.Universal.ContextType.ImportExportDate, Value = "20190527" },
					new Context { Type = Constants.Universal.ContextType.CustomsDeclarationDate, Value = "2019-05-27T14:47:11" }
				}
			};

			AssertEquals("Note Text", eventData.GetContextValueByType(Constants.Universal.ContextType.Note));
			AssertEquals("20190527", eventData.GetContextValueByType(Constants.Universal.ContextType.ImportExportDate));
			AssertEquals("2019-05-27T14:47:11", eventData.GetContextValueByType(Constants.Universal.ContextType.CustomsDeclarationDate));

			AssertEquals(ZDateTime.Empty, eventData.GetContextValueByTypeAsDateTime(Constants.Universal.ContextType.Note, ZDateTime.Empty));
			AssertEquals(new ZDateTime(2019, 5, 27), eventData.GetContextValueByTypeAsDateTime(Constants.Universal.ContextType.ImportExportDate, ZDateTime.Empty));
			AssertEquals(new ZDateTime(2019, 5, 27, 14, 47, 11), eventData.GetContextValueByTypeAsDateTime(Constants.Universal.ContextType.CustomsDeclarationDate, ZDateTime.Empty));

			AssertEquals("2019-05-27 15:07:12", eventData.FormatContextValueByType(Constants.Universal.EventTime));
			AssertEquals("Note Text", eventData.FormatContextValueByType(Constants.Universal.ContextType.Note));
			AssertEquals("2019-05-27", eventData.FormatContextValueByType(Constants.Universal.ContextType.ImportExportDate));
			AssertEquals("2019-05-27 14:47:11", eventData.FormatContextValueByType(Constants.Universal.ContextType.CustomsDeclarationDate));
		}

		public void TestGetLastOutgoingMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var msg1 = Factory.NewWithValidTestData<XmlEDIMessage>();
			msg1.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			msg1.EM_MessageType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			msg1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			msg1.EM_ReceiveTransmit = "TRX";
			entryHeader.Messages.Add(msg1);

			var msg2 = Factory.NewWithValidTestData<XmlEDIMessage>();
			msg2.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			msg2.EM_MessageType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			msg2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			msg2.EM_ReceiveTransmit = "RCV";
			entryHeader.Messages.Add(msg2);

			var msg3 = Factory.NewWithValidTestData<XmlEDIMessage>();
			msg3.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			msg3.EM_MessageType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			msg3.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			msg3.EM_ReceiveTransmit = "TRX";
			entryHeader.Messages.Add(msg3);

			Factory.Save();

			AssertSame(msg1, entryHeader.GetLastOutgoingMessage());
		}

		public void TestParseCIQNumberFromNoteText()
		{
			var eventData = new Event
			{
				ContextCollection = new List<Context>
				{
					new Context { Type = Constants.Universal.ContextType.Note, Value = "" },
				}
			};
			AssertEquals(ZString.Empty, eventData.ParseCIQNumberFromNoteText());

			eventData.ContextCollection[0].Value = ",,,:[118000009179148]已检验检疫合格";
			AssertEquals("118000009179148", eventData.ParseCIQNumberFromNoteText());

			eventData.ContextCollection[0].Value = ",,,:[118000009179148123]已检验检疫合格";
			AssertEquals("118000009179148123", eventData.ParseCIQNumberFromNoteText());

			eventData.ContextCollection[0].Value = ",,,:[11800000917914812]已检验检疫合格";
			AssertEquals(ZString.Empty, eventData.ParseCIQNumberFromNoteText());
		}

		public void TestParseCIQIssueDateFromNoteText()
		{
			var eventData = new Event
			{
				ContextCollection = new List<Context>
				{
					new Context { Type = Constants.Universal.ContextType.Note, Value = "" },
				}
			};
			AssertEquals(ZDateTime.Empty, eventData.ParseCIQIssueDateFromNoteText());

			eventData.ContextCollection[0].Value = "报检日期:2018-12-24 15:41:10";
			AssertEquals(new ZDateTime(2018, 12, 24, 15, 41, 10), eventData.ParseCIQIssueDateFromNoteText());
		}
	}
}

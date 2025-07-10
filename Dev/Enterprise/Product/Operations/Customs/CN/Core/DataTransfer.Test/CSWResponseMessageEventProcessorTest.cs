using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	sealed class CSWResponseMessageEventProcessorTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public const string incomingMDLEvent = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataProvider>CSW</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key></Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2019-4-16T20:17:41</EventTime>
		<EventType>MDL</EventType>
		<EventReference>MST=SW</EventReference>
		<ContextCollection>
			<Context>
				<Type>LocalReferenceNumber</Type>
				<Value>000000000000194233</Value>
			</Context>
			<Context>
				<Type>ResponseCode</Type>
				<Value>0</Value>
			</Context>
			<Context>
				<Type>ResponseDetail</Type>
				<Value>暂存成功</Value>
			</Context>
			<Context>
				<Type>DeclarationUnifiedNumber</Type>
				<Value>I20180000144486227</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessage_Accepted()
		{
			var message = GetQueuedUniversalEventMessage(Factory, incomingMDLEvent);
			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();
			var reloadedMessage = newFactory.Load<EDIMessage>(message.PK);

			var manager = new UniversalMessageProcessingManager(logger);
			manager.Process(reloadedMessage);

			var reloadEntryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.CN.DataTransfer.Testing.TestFiles.MessageInterpretation.html"))
			{
				var messageInterpretationText = stream.ConvertToUTF8StringAndCloseStream();
				AssertEquals("ACO", reloadEntryHeader.CH_Status);
				AssertEquals("I20180000144486227", reloadEntryHeader.DeclarationUnifiedNumber);
				AssertMultilineASCIIEquals(messageInterpretationText, reloadedMessage.EM_MessageInterpretation);
			}

			var testItem = new EntryNumbersBO(reloadEntryHeader);
			Assert("DeclarationUnifiedNumber should be readonly as CE_EntryIsSystemGenerated=true", testItem.DeclarationUnifiedNumber_ReadOnly);
			Assert("DeclarationUnifiedNumberInfo should be readonly as CE_EntryIsSystemGenerated=true", testItem.DeclarationUnifiedNumberInfo.ReadOnly);

			var savedEmails = Env.OutgoingCustomsMailManager.EmailsCreated.FindAll(email => email.Subject.StartsWith("报关单导入单一窗口成功"));
			CombineAssertions("Send eMail When Receiving Deliver Response: MDL", () =>
			{
				AssertEquals("should have sent an email", savedEmails.Count, 1);
				var savedEmail = savedEmails.FirstOrDefault();
				AssertEquals("Email should have the correct subject", "报关单导入单一窗口成功: 000000000000194233, B00001139", savedEmail.Subject);
				var emailText = Regex.Replace(savedEmail.Body, @"<td width=""[^""]+"">", "<td>");
				AssertContains("Should have a Hyperlink to the Job", "<a href=\"edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK, emailText);
				AssertContains("Should have a row for DeclarationUnifiedNumber", "<tr><td>统一编号</td><td>I20180000144486227</td></tr>", emailText);
				AssertContains("Should have a row for ResponseCode", "<tr><td>响应代码</td><td>0</td></tr>", emailText);
			});

			entryHeader.CH_Status = "AWP";
			entryHeader.Factory.Save();
			var message2 = GetQueuedUniversalEventMessage(Factory, incomingMDLEvent.Replace("2019-4-16T20:17:41", "2019-4-16T21:17:41"));
			Factory.SaveForTesting();
			var reloadedMessage2 = newFactory.Load<EDIMessage>(message2.PK);
			manager.Process(reloadedMessage2);
			var reloadEntryHeader2 = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("ACP", reloadEntryHeader2.CH_Status);

			entryHeader.CH_Status = "AWM";
			entryHeader.Factory.Save();
			var message3 = GetQueuedUniversalEventMessage(Factory, incomingMDLEvent.Replace("2019-4-16T20:17:41", "2019-4-16T22:17:41"));
			Factory.SaveForTesting();
			var reloadedMessage3 = newFactory.Load<EDIMessage>(message3.PK);
			manager.Process(reloadedMessage3);
			var reloadEntryHeader3 = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("ACM", reloadEntryHeader3.CH_Status);
		}

		public void TestProcessMessage_Rejected()
		{
			var uin = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.China.DeclarationUnifiedNumber, Core.Constants.CountryCodes.China);
			uin.CE_EntryNum = "123456";
			factory.Save();

			const string incomingEvent_Rej = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataProvider>CSW</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key></Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2019-4-16T20:17:41</EventTime>
		<EventType>MRJ</EventType>
		<EventReference>MST=SW</EventReference>
		<ContextCollection>
			<Context>
				<Type>LocalReferenceNumber</Type>
				<Value>000000000000194233</Value>
			</Context>
			<Context>
				<Type>ResponseCode</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>ResponseDetail</Type>
				<Value>暂存失败：商品序号7：,规格型号输入超长</Value>
			</Context>
			<Context>
				<Type>DeclarationUnifiedNumber</Type>
				<Value>I20180000144486227</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(Factory, incomingEvent_Rej);
			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();
			var reloadedMessage = newFactory.Load<EDIMessage>(message.PK);

			var manager = new UniversalMessageProcessingManager(logger);
			manager.Process(reloadedMessage);

			var reloadEntryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("ERO", reloadEntryHeader.CH_Status);
			AssertEquals("It's not updated if it exists", "123456", reloadEntryHeader.DeclarationUnifiedNumber);

			var savedEmails = Env.OutgoingCustomsMailManager.EmailsCreated.FindAll(email => email.Subject.StartsWith("报关单导入单一窗口失败"));
			CombineAssertions("Send eMail When Receiving Deliver Response: MRJ", () =>
			{
				AssertEquals("should have sent an email", savedEmails.Count, 1);
				var savedEmail = savedEmails.FirstOrDefault();
				AssertEquals("Email should have the correct subject", "报关单导入单一窗口失败: 000000000000194233, B00001139", savedEmail.Subject);
				var emailText = Regex.Replace(savedEmail.Body, @"<td width=""[^""]+"">", "<td>");
				AssertContains("Should have a Hyperlink to the Job", "<a href=\"edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK, emailText);
				AssertContains("Should have a row for DeclarationUnifiedNumber", "<tr><td>统一编号</td><td>I20180000144486227</td></tr>", emailText);
				AssertContains("Should have a row for ResponseCode", "<tr><td>响应代码</td><td>1</td></tr>", emailText);
			});

			entryHeader.CH_Status = "ACP";
			entryHeader.Factory.Save();
			var message2 = GetQueuedUniversalEventMessage(Factory, incomingEvent_Rej.Replace("2019-4-16T20:17:41", "2019-4-16T21:17:41"));
			Factory.SaveForTesting();
			var reloadedMessage2 = newFactory.Load<EDIMessage>(message2.PK);
			new UniversalMessageProcessingManager(logger).Process(reloadedMessage2);
			var reloadEntryHeader2 = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("ERP", reloadEntryHeader2.CH_Status);

			entryHeader.CH_Status = "ACM";
			entryHeader.Factory.Save();
			var message3 = GetQueuedUniversalEventMessage(Factory, incomingEvent_Rej.Replace("2019-4-16T20:17:41", "2019-4-16T22:17:41"));
			Factory.SaveForTesting();
			var reloadedMessage3 = newFactory.Load<EDIMessage>(message3.PK);
			new UniversalMessageProcessingManager(logger).Process(reloadedMessage3);
			var reloadEntryHeader3 = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("ERM", reloadEntryHeader3.CH_Status);
		}

		public void TestGetLogParentsForEventUsingContext()
		{
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingMDLEvent);
			var eventDataObject = xmlEvent as UniversalEvent;

			var parents = CSWResponseMessageEventProcessor.GetLogParentsForEventUsingContext(eventDataObject, factory);

			AssertEquals(1, parents.Length);
			AssertEquals(entryHeader.PK, parents[0].PK);
		}

		public void TestIsCSWResponseMessage()
		{
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingMDLEvent);
			var eventDataObject = xmlEvent as UniversalEvent;
			AssertEquals(true, CSWResponseMessageEventProcessor.IsCSWResponseMessage(eventDataObject));

			const string event3 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataProvider>CSW</DataProvider>
		</DataContext>
		<EventReference>MST=ST</EventReference>
	</Event>
</UniversalEvent>";
			xmlEvent = eventDeserializer.Parse(event3);
			eventDataObject = xmlEvent as UniversalEvent;
			AssertEquals(false, CSWResponseMessageEventProcessor.IsCSWResponseMessage(eventDataObject));

			const string event4 = @"
<UniversalEvent>
	<Event>
		<EventReference>MST=SW</EventReference>
	</Event>
</UniversalEvent>";
			xmlEvent = eventDeserializer.Parse(event4);
			eventDataObject = xmlEvent as UniversalEvent;
			AssertEquals(true, CSWResponseMessageEventProcessor.IsCSWResponseMessage(eventDataObject));
		}

		public void TestEmptyDataProvider()
		{
			const string emptyDataProviderMsg = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key></Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2019-4-16T20:17:41</EventTime>
		<EventType>MDL</EventType>
		<EventReference>MST=SW</EventReference>
		<ContextCollection>
			<Context>
				<Type>LocalReferenceNumber</Type>
				<Value>000000000000194233</Value>
			</Context>
			<Context>
				<Type>ResponseCode</Type>
				<Value>0</Value>
			</Context>
			<Context>
				<Type>ResponseDetail</Type>
				<Value>暂存成功</Value>
			</Context>
			<Context>
				<Type>DeclarationUnifiedNumber</Type>
				<Value>I20180000144486227</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

			var message = GetQueuedUniversalEventMessage(Factory, emptyDataProviderMsg);
			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();
			var reloadedMessage = newFactory.Load<EDIMessage>(message.PK);

			var manager = new UniversalMessageProcessingManager(logger);
			manager.Process(reloadedMessage);

			var reloadEntryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("Processor should have given effect even with empty DataProvider", "ACO", reloadEntryHeader.CH_Status);
		}

		const string MSCEventMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataProvider>CSW</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key></Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>

		<EventTime>2019-04-20T19:18:17</EventTime>
		<EventType>MSC</EventType>
		<EventReference>MST=SW</EventReference>
		<ContextCollection>
			<Context>
				<Type>CustomsDeclarationNumber</Type>
				<Value>HXUTEST20190428001</Value>
			</Context>
			<Context>
				<Type>CustomsDeclarationDate</Type>
				<Value>2019-04-20</Value>
			</Context>
			<Context>
				<Type>Note</Type>
				<Value>:[118000009128617]已正式受理通过，请与【 上海浦东局本部检务科】 联系办理检验检疫事宜。联系电话:021-50361015。报检日期:2018-12-24 15:41:10。审单结论：实施审单放行。</Value>
			</Context>
			<Context>
				<Type>CustomsOffice</Type>
				<Value>TestHXU</Value>
			</Context>
			<Context>
				<Type>DeclarationUnifiedNumber</Type>
				<Value>TESTHXU20190428001</Value>
			</Context>
			<Context>
				<Type>ImportExportDate</Type>
				<Value>20190410</Value>
			</Context>
			<Context>
				<Type>EntryStatus</Type>
				<Value>0a</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

		const string MSCEventMessage1 = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataProvider>CSW</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key></Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>

		<EventTime>2019-05-20T19:18:17</EventTime>
		<EventType>MSC</EventType>
		<EventReference>MST=SW</EventReference>
		<ContextCollection>
			<Context>
				<Type>CustomsDeclarationNumber</Type>
				<Value>HXUTEST20190428001</Value>
			</Context>
			<Context>
				<Type>CustomsDeclarationDate</Type>
				<Value>2019-04-20</Value>
			</Context>
			<Context>
				<Type>Note</Type>
				<Value>:[118000009128617]已正式受理通过，请与【 上海浦东局本部检务科】 联系办理检验检疫事宜。联系电话:021-50361015。报检日期:2019-05-22 15:41:10。审单结论：实施审单放行。</Value>
			</Context>
			<Context>
				<Type>CustomsOffice</Type>
				<Value>TestHXU</Value>
			</Context>
			<Context>
				<Type>DeclarationUnifiedNumber</Type>
				<Value>TESTHXU20190428001</Value>
			</Context>
			<Context>
				<Type>ImportExportDate</Type>
				<Value>20190410</Value>
			</Context>
			<Context>
				<Type>EntryStatus</Type>
				<Value>0d</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";

		public void TestUpdateCIQEntryNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping("CN", "China");
			helper.CreateNewOrGetExistingCusCodeType("CSTA", "desc");
			var codeList = helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "0a", "签证", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList.PK, "IUpdateCIQStatus", "");
			factory.Save();

			entryHeader.CH_EntryStatus = "0a";
			entryHeader.DeclarationUnifiedNumber = "TESTHXU20190428001";
			factory.Save();

			var message = GetQueuedUniversalEventMessage(Factory, MSCEventMessage);
			Factory.SaveForTesting();

			var reloadedMessage = factory.Load<EDIMessage>(message.PK);

			var manager = new UniversalMessageProcessingManager(logger);
			manager.Process(reloadedMessage);

			entryHeader.Reload();

			var query = new ZQuery(StmALogSchema.SL_Parent, entryHeader.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "STC");
			var log = entryHeader.Logs.MostRecentLogByEventTime(Events.StatusChange);
			AssertNotNull(log);
			AssertEquals("CIQ Status Changed|NEW=0a|OLD=", log.SL_Reference);

			var ciqNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.China.CIQNumber, "CN");
			AssertEquals("CIQNumber", "118000009128617", entryHeader.CIQNumber);
			AssertEquals("CE_EntryStatus", "0a", ciqNumber.CE_EntryStatus);
			AssertEquals("CE_IssueDate", new ZDateTime(2018, 12, 24, 15, 41, 10), ciqNumber.CE_IssueDate);
		}

		public void TestNotUpdateCIQEntryNumber_DifferentCIQNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping("CN", "China");
			helper.CreateNewOrGetExistingCusCodeType("CSTA", "desc");
			var codeList = helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "0a", "签证", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList.PK, "IUpdateCIQStatus", "");
			factory.Save();

			entryHeader.DeclarationUnifiedNumber = "TESTHXU20190428001";
			entryHeader.CIQNumber = "118000009128618";
			factory.Save();

			var message = GetQueuedUniversalEventMessage(Factory, MSCEventMessage);
			Factory.SaveForTesting();

			var reloadedMessage = factory.Load<EDIMessage>(message.PK);

			var manager = new UniversalMessageProcessingManager(logger);
			manager.Process(reloadedMessage);

			entryHeader.Reload();

			var ciqNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.China.CIQNumber, "CN");
			AssertEquals("CIQNumber is not updated", "118000009128618", entryHeader.CIQNumber);
			AssertEquals("CE_EntryStatus is not updated", "", ciqNumber.CE_EntryStatus);
			AssertNotEquals("CE_IssueDate is not updated", new ZDateTime(2018, 12, 24, 15, 41, 10), ciqNumber.CE_IssueDate);

			var query = new ZQuery(StmALogSchema.SL_Parent, entryHeader.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "STC");
			var log = factory.LoadTop1<StmALog>(query);
			AssertNull("No log is created.", log);
		}

		public void TestNotUpdateCIQEntryStatus_EventTimeIsEarlier()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping("CN", "China");
			helper.CreateNewOrGetExistingCusCodeType("CSTA", "desc");
			var codeList1 = helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "0a", "签证", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList1.PK, "IUpdateCIQStatus", "");

			var codeList2 = helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "0d", "检验检疫受理通过", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList2.PK, "IUpdateCIQStatus", "");
			factory.Save();

			entryHeader.CH_EntryStatus = "0d";
			entryHeader.DeclarationUnifiedNumber = "TESTHXU20190428001";

			var msg = Factory.New<XmlEDIMessage>();
			msg.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			msg.EM_MessageType = Messaging.Integration.EDIMessageTypeList.Codes.XDC;
			msg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			msg.EM_MessageText = MSCEventMessage1;
			entryHeader.Messages.Add(msg);
			factory.Save();

			var message = GetQueuedUniversalEventMessage(Factory, MSCEventMessage);
			Factory.SaveForTesting();

			var reloadedMessage = factory.Load<EDIMessage>(message.PK);

			var manager = new UniversalMessageProcessingManager(logger);
			manager.Process(reloadedMessage);

			entryHeader.Reload();

			var ciqNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.China.CIQNumber, "CN");
			AssertEquals("CIQNumber is updated", "118000009128617", entryHeader.CIQNumber);
			AssertEquals("CE_EntryStatus is not updated", "", ciqNumber.CE_EntryStatus);
			AssertEquals("CE_IssueDate is updated", new ZDateTime(2018, 12, 24, 15, 41, 10), ciqNumber.CE_IssueDate);

			var query = new ZQuery(StmALogSchema.SL_Parent, entryHeader.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, "STC");
			var log = factory.LoadTop1<StmALog>(query);
			AssertNull("No log is created.", log);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessage_MSCEventTypeAndD_DateGreaterThanMRNDate()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping("CN", "China");
			helper.CreateNewOrGetExistingCusCodeType("CSTA", "desc");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "0a", "签证", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			factory.Save();

			entryHeader.DeclarationUnifiedNumber = "TESTHXU20190428001";
			factory.Save();

			var message = GetQueuedUniversalEventMessage(Factory, MSCEventMessage);
			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();
			var reloadedMessage = newFactory.Load<EDIMessage>(message.PK);

			var manager = new UniversalMessageProcessingManager(logger);
			manager.Process(reloadedMessage);

			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.CN.DataTransfer.Testing.TestFiles.MSCEventMessageInterpretation.html"))
			{
				var reloadEntryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
				var messageInterpretationText = stream.ConvertToUTF8StringAndCloseStream();
				AssertEquals("HXUTEST20190428001", reloadEntryHeader.MovementReferenceNumber);
				AssertEquals("Pre Entry Number should have been set.", "HXUTEST20190428001", reloadEntryHeader.PreEntryNumber);
				AssertEquals(new ZDateTime("2019-04-20"), reloadEntryHeader.MovementReferenceNumberIssueDate);
				AssertEquals("Pre Entry Number IssueDate should have been set.", new ZDateTime("2019-04-20"), reloadEntryHeader.PreEntryNumberIssueDate);
				AssertMultilineASCIIEquals(messageInterpretationText, reloadedMessage.EM_MessageInterpretation);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProcessMessage_MSCEventTypeAndD_DateLessThanMRNDate()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping("CN", "China");
			helper.CreateNewOrGetExistingCusCodeType("CSTA", "desc");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "0a", "签证", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			factory.Save();

			entryHeader.DeclarationUnifiedNumber = "TESTHXU20190428001";
			entryHeader.SetMovementReferenceNumber("123456789012345678", new ZDateTime("2019-04-21"));
			entryHeader.SetPreEntryNumber("123456789012345678", new ZDateTime("2019-04-21"));
			factory.Save();

			var message = GetQueuedUniversalEventMessage(Factory, MSCEventMessage);
			Factory.SaveForTesting();

			var newFactory = new BusinessObjectFactory();
			var reloadedMessage = newFactory.Load<EDIMessage>(message.PK);

			var manager = new UniversalMessageProcessingManager(logger);
			manager.Process(reloadedMessage);

			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.CN.DataTransfer.Testing.TestFiles.MSCEventMessageInterpretation.html"))
			{
				var reloadEntryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
				var messageInterpretationText = stream.ConvertToUTF8StringAndCloseStream();
				AssertEquals("123456789012345678", reloadEntryHeader.MovementReferenceNumber);
				AssertEquals("No Update Time.", new ZDateTime("2019-04-21"), reloadEntryHeader.MovementReferenceNumberIssueDate);
				AssertEquals("No Update Time.", new ZDateTime("2019-04-21"), reloadEntryHeader.MovementReferenceNumberIssueDate);
				AssertEquals("Pre Entry Number should NOT have been changed.", "123456789012345678", reloadEntryHeader.PreEntryNumber);
				AssertEquals("Pre Entry Number IssueDate should NOT have been changed.", new ZDateTime("2019-04-21"), reloadEntryHeader.PreEntryNumberIssueDate);
				AssertMultilineASCIIEquals(messageInterpretationText, reloadedMessage.EM_MessageInterpretation);
				AssertEquals(EDIMessage.Status.Warning, reloadedMessage.EM_Status);
				AssertEquals("123456789012345678", reloadEntryHeader.MovementReferenceNumber);
				AssertEquals("No Update Time.", new ZDateTime("2019-04-21"), reloadEntryHeader.MovementReferenceNumberIssueDate);
				AssertEquals("Pre Entry Number should have been changed.", "123456789012345678", reloadEntryHeader.PreEntryNumber);
				AssertEquals("Pre Entry Number IssueDate should have been changed.", new ZDateTime("2019-04-21"), reloadEntryHeader.PreEntryNumberIssueDate);
				AssertMultilineASCIIEquals(messageInterpretationText, reloadedMessage.EM_MessageInterpretation);
				AssertEquals(EDIMessage.Status.Warning, reloadedMessage.EM_Status);
			}
		}

		public void TestProcessMessage_ChangingStatusByEventType()
		{
			const string mscEventStatusMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataProvider>CSW</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key></Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>

		<EventTime>2019-04-20T19:18:17</EventTime>
		<EventType>MSC</EventType>
		<EventReference>MST=SW</EventReference>
		<ContextCollection>
			<Context>
				<Type>CustomsDeclarationNumber</Type>
				<Value>HXUTEST20190428001</Value>
			</Context>
			<Context>
				<Type>CustomsDeclarationDate</Type>
				<Value>2019-04-20</Value>
			</Context>
			<Context>
				<Type>Note</Type>
				<Value>Note</Value>
			</Context>
			<Context>
				<Type>CustomsOffice</Type>
				<Value>TestHXU</Value>
			</Context>
			<Context>
				<Type>DeclarationUnifiedNumber</Type>
				<Value>TESTHXU20190428001</Value>
			</Context>
			<Context>
				<Type>ImportExportDate</Type>
				<Value>20190410</Value>
			</Context>
			<Context>
				<Type>EntryStatus</Type>
				<Value>{EntryStatus}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var entryStatusMarking = "{EntryStatus}";
			var eventTimeTextToReplace = "<EventTime>2019-04-20T19:18:17</EventTime>";

			entryHeader.DeclarationUnifiedNumber = "TESTHXU20190428001";
			factory.Save();

			SetupRefCusCodeLists();
			var testStarted = ZDateTime.Now;

			var message = GetQueuedUniversalEventMessage(Factory, mscEventStatusMessage.Replace(entryStatusMarking, "ZZ"), true);
			var reloadedMessage = factory.Load<EDIMessage>(message.PK);
			var manager = new UniversalMessageProcessingManager(logger);
			manager.Process(reloadedMessage);
			factory.Save();
			AssertEquals("Unkonw entry status, should not change CH_EntryStatus", "", entryHeader.CH_EntryStatus);
			AssertEquals("Unkonw entry status, ErrorReporter", "The Entry Status 'ZZ' from the response message does not defined in RefCusCodeList.\r\nDeclaration Unified Number = TESTHXU20190428001\r\nEntry Number = HXUTEST20190428001", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			message = GetQueuedUniversalEventMessage(Factory, mscEventStatusMessage.Replace(entryStatusMarking, "07"), true);
			reloadedMessage = factory.Load<EDIMessage>(message.PK);
			manager = new UniversalMessageProcessingManager(logger);
			manager.Process(reloadedMessage);
			factory.Save();
			AssertEquals("IUpdateCustomsStatus, should have changed CH_EntryStatus", "07", entryHeader.CH_EntryStatus);
			Assert("IUpdateCustomsStatus, should have logged a CES event", entryHeader.Logs.Find(log => log.SL_SE_NKEvent == "CES" && log.SL_Reference == "07" && log.SL_EventTime >= testStarted).Any());
			AssertNull("Unkonw entry status, No ErrorReporter", ErrorReporter.LastExceptionReported);

			entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
			var customsCommencedMsg = mscEventStatusMessage.Replace(entryStatusMarking, "08").Replace(eventTimeTextToReplace, "<EventTime>2019-04-20T20:18:17</EventTime>");
			message = GetQueuedUniversalEventMessage(Factory, customsCommencedMsg);
			reloadedMessage = factory.Load<EDIMessage>(message.PK);
			manager.Process(reloadedMessage);
			factory.Save();
			Assert("CustomsCommenced, should have logged a CCC event", declaration.Logs.Find(log => log.SL_SE_NKEvent == "CCC" && log.SL_EventTime >= testStarted).Any());
			AssertEquals("CustomsCommenced, should have set JE_EntrySubmittedDate", new ZDateTime(2019, 4, 20, 20, 18, 17), declaration.JE_EntrySubmittedDate);

			var customsClearedMsg = mscEventStatusMessage.Replace(entryStatusMarking, "09").Replace(eventTimeTextToReplace, "<EventTime>2019-04-20T21:18:17</EventTime>");
			message = GetQueuedUniversalEventMessage(Factory, customsClearedMsg);
			reloadedMessage = factory.Load<EDIMessage>(message.PK);
			manager.Process(reloadedMessage);
			factory.Save();
			Assert("CustomsCleared, should have logged a CLS event", entryHeader.Logs.Find(log => log.SL_SE_NKEvent == "CLR" && log.SL_EventTime >= testStarted).Any());
			AssertEquals("CustomsCleared, CH_Status should be CLO", "CLO", entryHeader.CH_Status);

			var iUpdateReleaseDateMsg = mscEventStatusMessage.Replace(entryStatusMarking, "1A").Replace(eventTimeTextToReplace, "<EventTime>2019-04-20T22:18:17</EventTime>");
			message = GetQueuedUniversalEventMessage(Factory, iUpdateReleaseDateMsg);
			reloadedMessage = factory.Load<EDIMessage>(message.PK);
			manager.Process(reloadedMessage);
			factory.Save();
			AssertEquals("IUpdateReleaseDate, should have set CH_EntryReleaseDate", new ZDateTime(2019, 4, 20, 22, 18, 17), entryHeader.CH_EntryReleaseDate);

			var customsRejectedMsg = mscEventStatusMessage.Replace(entryStatusMarking, "1B").Replace(eventTimeTextToReplace, "<EventTime>2019-04-20T23:18:17</EventTime>");
			message = GetQueuedUniversalEventMessage(Factory, customsRejectedMsg);
			reloadedMessage = factory.Load<EDIMessage>(message.PK);
			manager.Process(reloadedMessage);
			factory.Save();
			AssertEquals("CustomsRejected, should have set CH_Status to ERO", "ERO", entryHeader.CH_Status);

			var cancelledMsg = mscEventStatusMessage.Replace(entryStatusMarking, "1C").Replace(eventTimeTextToReplace, "<EventTime>2019-04-21T00:18:17</EventTime>");
			message = GetQueuedUniversalEventMessage(Factory, cancelledMsg);
			reloadedMessage = factory.Load<EDIMessage>(message.PK);
			manager.Process(reloadedMessage);
			factory.Save();
			AssertEquals("CustomsCancelled, should have set CH_Status to CLD", "CLD", entryHeader.CH_Status);

			var shouldUpdateCustomsStatusMsg = mscEventStatusMessage.Replace(entryStatusMarking, "07");
			message = GetQueuedUniversalEventMessage(Factory, mscEventStatusMessage.Replace(entryStatusMarking, "07"));
			var newFactory = new BusinessObjectFactory();
			var reloadedEntryHeader = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			reloadedMessage = newFactory.Load<EDIMessage>(message.PK);
			manager.Process(reloadedMessage);
			newFactory.Save();
			AssertEquals("Should not change anything if the current message is older than the last ShouldUpdateCustomsStatus incoming one.", "1C", reloadedEntryHeader.CH_EntryStatus);

			shouldUpdateCustomsStatusMsg = shouldUpdateCustomsStatusMsg.Replace(eventTimeTextToReplace, "<EventTime>2019-04-21T01:18:17</EventTime>");
			message = GetQueuedUniversalEventMessage(Factory, shouldUpdateCustomsStatusMsg);
			reloadedMessage = newFactory.Load<EDIMessage>(message.PK);
			manager.Process(reloadedMessage);
			newFactory.Save();
			AssertEquals("Should have changed CH_EntryStatus if the current message is newer than the last ShouldUpdateCustomsStatus incoming one.", "07", reloadedEntryHeader.CH_EntryStatus);

			entryHeader.CH_Status = "ACP";
			entryHeader.Factory.Save();
			var messageTextAWP = mscEventStatusMessage
				.Replace(eventTimeTextToReplace, "<EventTime>2019-04-21T02:18:17</EventTime>")
				.Replace(entryStatusMarking, "09");
			var messageAWP = GetQueuedUniversalEventMessage(Factory, messageTextAWP);
			var reloadedMessageAWP = newFactory.Load<EDIMessage>(messageAWP.PK);
			manager.Process(reloadedMessageAWP);
			var reloadedEntryHeaderAWP = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("CLP", reloadedEntryHeaderAWP.CH_Status);

			entryHeader.CH_Status = "ACM";
			entryHeader.Factory.Save();
			var messageTextAWM = mscEventStatusMessage
				.Replace(eventTimeTextToReplace, "<EventTime>2019-04-21T03:18:17</EventTime>")
				.Replace(entryStatusMarking, "09");
			var messageAWM = GetQueuedUniversalEventMessage(Factory, messageTextAWM);
			var reloadedMessageAWM = newFactory.Load<EDIMessage>(messageAWM.PK);
			manager.Process(reloadedMessageAWM);
			var reloadedEntryHeaderAWM = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("CLM", reloadedEntryHeaderAWM.CH_Status);

			entryHeader.CH_Status = "ACP";
			entryHeader.Factory.Save();
			var messageTextACP = mscEventStatusMessage
				.Replace(eventTimeTextToReplace, "<EventTime>2019-04-21T04:18:17</EventTime>")
				.Replace(entryStatusMarking, "1B");
			var messageACP = GetQueuedUniversalEventMessage(Factory, messageTextACP);
			var reloadedMessageACP = newFactory.Load<EDIMessage>(messageACP.PK);
			manager.Process(reloadedMessageACP);
			var reloadedEntryHeaderACP = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("ERP", reloadedEntryHeaderACP.CH_Status);

			entryHeader.CH_Status = "ACM";
			entryHeader.Factory.Save();
			var messageTextACM = mscEventStatusMessage
				.Replace(eventTimeTextToReplace, "<EventTime>2019-04-21T05:18:17</EventTime>")
				.Replace(entryStatusMarking, "1B");
			var messageACM = GetQueuedUniversalEventMessage(Factory, messageTextACM);
			var reloadedMessageACM = newFactory.Load<EDIMessage>(messageACM.PK);
			manager.Process(reloadedMessageACM);
			var reloadedEntryHeaderACM = newFactory.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("ERM", reloadedEntryHeaderACM.CH_Status);

			entryHeader.CH_Status = "CLP";
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStepManual;
			customsClearedMsg = mscEventStatusMessage.Replace(entryStatusMarking, "09").Replace(eventTimeTextToReplace, "<EventTime>2019-04-20T21:18:17</EventTime>");
			message = GetQueuedUniversalEventMessage(Factory, customsClearedMsg);
			reloadedMessage = factory.Load<EDIMessage>(message.PK);
			manager.Process(reloadedMessage);
			factory.Save();
			AssertEquals("CustomsCleared, CH_Status should be AWM", "AWM", entryHeader.CH_Status);

			entryHeader.CH_Status = "CLP";
			declaration.JE_ClearanceMode = ClearanceModeList.Codes.TwoStepAuto;
			customsClearedMsg = mscEventStatusMessage.Replace(entryStatusMarking, "09").Replace(eventTimeTextToReplace, "<EventTime>2019-04-20T21:18:17</EventTime>");
			message = GetQueuedUniversalEventMessage(Factory, customsClearedMsg);
			reloadedMessage = factory.Load<EDIMessage>(message.PK);
			manager.Process(reloadedMessage);
			factory.Save();
			AssertEquals("CustomsCleared, CH_Status should be AWM", "AWM", entryHeader.CH_Status);
		}

		public void TestSendEmail()
		{
			const string mscEventStatusMessage = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataProvider>CSW</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key></Key>
					<Type>CustomsDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>

		<EventTime>2019-04-20T19:18:17</EventTime>
		<EventType>MSC</EventType>
		<EventReference>MST=SW</EventReference>
		<ContextCollection>
			<Context>
				<Type>CustomsDeclarationNumber</Type>
				<Value>HXUTEST20190428001</Value>
			</Context>
			<Context>
				<Type>CustomsDeclarationDate</Type>
				<Value>2019-04-20</Value>
			</Context>
			<Context>
				<Type>Note</Type>
				<Value>TESTHXU20190428001,TESTHXU20190428001直接申报成功</Value>
			</Context>
			<Context>
				<Type>CustomsOffice</Type>
				<Value>TestHXU</Value>
			</Context>
			<Context>
				<Type>DeclarationUnifiedNumber</Type>
				<Value>TESTHXU20190428001</Value>
			</Context>
			<Context>
				<Type>ImportExportDate</Type>
				<Value>20190410</Value>
			</Context>
			<Context>
				<Type>EntryStatus</Type>
				<Value>{EntryStatus}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			var entryStatusMarking = "{EntryStatus}";

			entryHeader.DeclarationUnifiedNumber = "TESTHXU20190428001";
			factory.Save();

			SetupRefCusCodeLists();

			var message = GetQueuedUniversalEventMessage(Factory, mscEventStatusMessage.Replace(entryStatusMarking, "07"), true);
			var reloadedMessage = factory.Load<EDIMessage>(message.PK);
			var manager = new UniversalMessageProcessingManager(logger);
			manager.Process(reloadedMessage);
			factory.Save();
			AssertNull("07, should have NOT sent an email", Env.OutgoingCustomsMailManager.EmailsCreated.Find(email => email.Subject.StartsWith("Entry Notification:")));

			message = GetQueuedUniversalEventMessage(Factory, mscEventStatusMessage.Replace(entryStatusMarking, "09"), true);
			reloadedMessage = factory.Load<EDIMessage>(message.PK);
			manager.Process(reloadedMessage);
			factory.Save();
			var savedEmails = Env.OutgoingCustomsMailManager.EmailsCreated.FindAll(email => email.Subject.StartsWith("单一窗口报关状态回执:"));
			CombineAssertions(() =>
			{
				AssertEquals("09, should have sent an email", savedEmails.Count, 1);
				var savedEmail = savedEmails.FirstOrDefault();
				AssertEquals("Email should have the correct subject", "单一窗口报关状态回执: HXUTEST20190428001 已放行, B00001139", savedEmail.Subject);
				var emailText = Regex.Replace(savedEmail.Body, @"<td width=""[^""]+"">", "<td>");
				AssertContains("Should have a Hyperlink to the Job", "<a href=\"edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK, emailText);
				AssertContains("Should have a row for DeclarationUnifiedNumber", "<tr><td>统一编号</td><td>TESTHXU20190428001</td></tr>", emailText);
				AssertContains("Should have a row for EntryStatus", "<tr><td>回执代码</td><td>9</td></tr>", emailText);
			});
		}

		void SetupRefCusCodeLists()
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType("CSTA", "Customs Status");
			factory.Save();

			var code07 = helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "07", "07", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var code08 = helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "08", "08", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var code09 = helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "09", "09", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			code09.ZZD_Description = "已放行";
			var code1A = helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "1A", "1A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var code1B = helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "1B", "1B", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var code1C = helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "1C", "1C", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			factory.Save();

			helper.CreateCusCodeListAttribute(code07.PK, "IUpdateCustomsStatus", "");
			helper.CreateCusCodeListAttribute(code08.PK, "CustomsCommenced", "");
			helper.CreateCusCodeListAttribute(code08.PK, "IUpdateCustomsStatus", "");
			helper.CreateCusCodeListAttribute(code09.PK, "CustomsCleared", "true");
			helper.CreateCusCodeListAttribute(code09.PK, "INotify", "");
			helper.CreateCusCodeListAttribute(code09.PK, "IUpdateCustomsStatus", "");
			helper.CreateCusCodeListAttribute(code1A.PK, "IUpdateReleaseDate", "");
			helper.CreateCusCodeListAttribute(code1A.PK, "IUpdateCustomsStatus", "");
			helper.CreateCusCodeListAttribute(code1B.PK, "CustomsRejected", "true");
			helper.CreateCusCodeListAttribute(code1B.PK, "IUpdateCustomsStatus", "");
			helper.CreateCusCodeListAttribute(code1C.PK, "ICustomsCancelled", "");
			helper.CreateCusCodeListAttribute(code1C.PK, "IUpdateCustomsStatus", "");
			factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();

			logger = new ServiceTaskLogForTesting();

			factory = new BusinessObjectFactory();
			declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = "B00001139";
			declaration.JE_HouseBill = "20247654321";
			declaration.JE_MasterBill = "081-203212";
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = "AWO";
			entryHeader.CH_BGMReference = "000000000000194233";
			entryHeader.CH_MessageType = "CUS";

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "~~";
			broker.GS_FullName = "BROKER NAME";
			broker.GS_EmailAddress = "email1@wisetechgloabal.com";

			var outgoingMessage = Factory.NewWithValidTestData<XmlEDIMessage>();
			outgoingMessage.EM_ApplicationCode = "UDM";
			outgoingMessage.EM_MessageType = "XUS";
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_SystemCreateUser = broker.GS_Code;
			entryHeader.Messages.Add(outgoingMessage);

			factory.Save();
		}
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		ServiceTaskLogForTesting logger;
		BusinessObjectFactory factory;
	}
}

using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5UTMessageProcessorTest : XMLMessageTestHelper<GOVCBR5UTMessageProcessorTest>
	{
		public void Test5UT()
		{
			CreateEntryWithOutgoingMessageForExport();
			exportEntry.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			exportEntry.Declaration.JE_VoyageFlightNo = "1111";
			exportEntry.Declaration.JE_ExportDate = new ZDateTime(2020, 11, 23);
			exportEntry.Declaration.JE_RL_NKPortOfLoading = "KRPUS";
			exportEntry.Declaration.JE_RL_NKPortOfArrival = "AUSYD";
			Factory.Save();
			var incomingMessage = CreateMessageForTest("GOVCBR5UT_0.xml");

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			var log = exportEntry.Logs.MostRecentLog;
			AssertNull(log);
			Factory.Save();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			log = exportEntry.Logs.MostRecentLog;
			AssertEquals("stmALog EventDate is updated correctly to 2020-11-24", "2020-11-24",
				log.SL_EventTime.ToString(DateFormatType.DateKorean));

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수출신고서", email.Body);
			AssertContains("6N00220000052X", email.Body);
			AssertContains("Y", email.Body);
			AssertContains("2020-11-24", email.Body);

			AssertEquals(incomingMessage.EM_MessageInterpretation,
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
				"<th>항 목</th><th>내 용</th></tr></thead>" +
				"<tr><td>제출문서</td><td>수출신고서</td></tr>" +
				"<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
				"<tr><td>선적여부</td><td>Y</td></tr>" +
				"<tr><td>선적일자</td><td>2020-11-24</td></tr></table>");
		}

		public void TestEmptyStmALogDate()
		{
			CreateEntryWithOutgoingMessageForExport();
			var incomingMessage = CreateMessageForTest("GOVCBR5UT_EmptyDate.xml");
			Factory.Save();
			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			AssertNoExceptionThrown(() => new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));

			var log = exportEntry.Logs.MostRecentLog;
			AssertNull(log);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("N", email.Body);

			AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																   "<th>항 목</th><th>내 용</th></tr></thead>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>선적여부</td><td>N</td></tr>" +
																   "<tr><td>선적일자</td><td>&nbsp;</td></tr></table>");
		}

		public void TestExport5UT_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				var incomingMessage = CreateMessageForTest("GOVCBR5UT_0.xml");
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수출 선적정보통보]6N00220000052X 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																	   "<th>항 목</th><th>내 용</th></tr></thead>" +
																	   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																	   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																	   "<tr><td>선적여부</td><td>Y</td></tr>" +
																	   "<tr><td>선적일자</td><td>2020-11-24</td></tr></table>");
			}
		}

		public void TestExport5UT_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5UT_0.xml");
				CreateEntryForExport(false);
				exportEntry.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				exportEntry.Declaration.JE_VoyageFlightNo = "1111";
				exportEntry.Declaration.JE_ExportDate = new ZDateTime(2020, 11, 23);
				exportEntry.Declaration.JE_RL_NKPortOfLoading = "KRPUS";
				exportEntry.Declaration.JE_RL_NKPortOfArrival = "AUSYD";
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 선적정보통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수출신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																	   "<th>항 목</th><th>내 용</th></tr></thead>" +
																	   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																	   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																	   "<tr><td>선적여부</td><td>Y</td></tr>" +
																	   "<tr><td>선적일자</td><td>2020-11-24</td></tr></table>");
			}
		}

		public void TestExport5UT_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5UT_0.xml");
				CreateEntryForExport(true);
				exportEntry.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				exportEntry.Declaration.JE_VoyageFlightNo = "1111";
				exportEntry.Declaration.JE_ExportDate = new ZDateTime(2020, 11, 23);
				exportEntry.Declaration.JE_RL_NKPortOfLoading = "KRPUS";
				exportEntry.Declaration.JE_RL_NKPortOfArrival = "AUSYD";
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 선적정보통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																	   "<th>항 목</th><th>내 용</th></tr></thead>" +
																	   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																	   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																	   "<tr><td>선적여부</td><td>Y</td></tr>" +
																	   "<tr><td>선적일자</td><td>2020-11-24</td></tr></table>");
			}
		}

		public void TestExport5UT_ATDWhenDoesntMatchTransportRows()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5UT_0.xml");

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_VoyageFlightNo = "1111";
				declaration.JE_ExportDate = new ZDateTime(2020, 11, 23);
				declaration.JE_RL_NKPortOfLoading = "KRPUS";
				declaration.JE_RL_NKPortOfArrival = "AUSYD";

				exportEntry = declaration.CustomsEntryHeaders.AddNew();
				var entryNumber = exportEntry.EntryNumbers.AddNew();
				entryNumber.CE_EntryNum = "6N00220000052X";
				entryNumber.CE_EntryType = "EXP";
				entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
				entryNumber.CE_ParentID = exportEntry.PK;
				entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;

				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];

				var stmAlogFilter = new ZQuery(StmALogSchema.SL_Parent, exportEntry.PK);
				stmAlogFilter.AddToFilter(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
				var log = Factory.LoadTop1<StmALog>(stmAlogFilter);
				AssertEquals("|FAC=CTO|LOC=KRPUS", log.SL_Reference);
				AssertEquals("2020-11-24", log.SL_EventTime.ToString("yyyy-MM-dd"));

				AssertContains("2020-11-24", email.Body);
				AssertContains("2020-11-24", incomingMessage.EM_MessageInterpretation);

				var declarationLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				AssertEquals(new ZDateTime(2020, 11, 24), declarationLoaded.JE_EntryDate);
			}
		}
		public void TestExport5UT_ATDWhenMatchTransportRows()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5UT_0.xml");

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_RL_NKPortOfLoading = "KRPUS";
				var transpot = declaration.Transports.AddNew();
				transpot.JW_TransportMode = declaration.JE_TransportMode;
				transpot.JW_RL_NKLoadPort = declaration.JE_RL_NKPortOfLoading;

				exportEntry = declaration.CustomsEntryHeaders.AddNew();
				var entryNumber = exportEntry.EntryNumbers.AddNew();
				entryNumber.CE_EntryNum = "6N00220000052X";
				entryNumber.CE_EntryType = "EXP";
				entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
				entryNumber.CE_ParentID = exportEntry.PK;
				entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;

				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];

				var stmAlogFilter = new ZQuery(StmALogSchema.SL_Parent, exportEntry.PK);
				stmAlogFilter.AddToFilter(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
				var log = Factory.LoadTop1<StmALog>(stmAlogFilter);
				AssertEquals("|FAC=CTO|LOC=KRPUS", log.SL_Reference);
				AssertEquals("2020-11-24", log.SL_EventTime.ToString("yyyy-MM-dd"));

				AssertContains("2020-11-24", email.Body);
				AssertContains("2020-11-24", incomingMessage.EM_MessageInterpretation);

				var declarationLoaded = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				AssertEquals(new ZDateTime(2020, 11, 24), declarationLoaded.JE_EntryDate);
			}
		}

		public void TestExport5UT_ATDWhenMatchTransportRowsButHasDifferentDate()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5UT_0.xml");

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_RL_NKPortOfLoading = "KRPUS";
				declaration.JE_EntryDate = new ZDate(2020, 11, 23);
				var transpot = declaration.Transports.AddNew();
				transpot.JW_TransportMode = declaration.JE_TransportMode;
				transpot.JW_RL_NKLoadPort = declaration.JE_RL_NKPortOfLoading;

				exportEntry = declaration.CustomsEntryHeaders.AddNew();
				var entryNumber = exportEntry.EntryNumbers.AddNew();
				entryNumber.CE_EntryNum = "6N00220000052X";
				entryNumber.CE_EntryType = "EXP";
				entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
				entryNumber.CE_ParentID = exportEntry.PK;
				entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;

				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];

				var stmAlogFilter = new ZQuery(StmALogSchema.SL_Parent, exportEntry.PK);
				stmAlogFilter.AddToFilter(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
				var log = Factory.LoadTop1<StmALog>(stmAlogFilter);
				AssertEquals("|FAC=CTO|LOC=KRPUS", log.SL_Reference);
				AssertEquals("2020-11-24", log.SL_EventTime.ToString("yyyy-MM-dd"));

				var warningMsg = "System was going to set the actual date of loading to Declaration, but it could not because it already has a different date. Please check";
				AssertContains(warningMsg, email.Body);
				AssertContains(warningMsg, incomingMessage.EM_MessageInterpretation);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ORG";
			staff1.GS_LoginName = "Origin";
			staff1.GS_EmailAddress = "OriginalSender@wisetechglobal.com";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "T1";
			staff1.GS_LoginName = "Test1";
			staff2.GS_EmailAddress = "ExportGroupTest@wisetechglobal.com";
			exportGroup = Factory.New<GlbGroup>();
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = exportGroup.PK;
			link1.GK_GS = staff2.PK;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "AG";
			staff3.GS_LoginName = "Agent";
			staff3.GS_EmailAddress = "CusAgent@wisetechglobal.com";
			Factory.Save();
		}
		GlbGroup exportGroup;

		void CreateEntryForExport(bool setCusAgent)
		{
			var declaration = Factory.New<JobDeclaration>();
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			exportEntry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = exportEntry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "6N00220000052X";
			entryNumber.CE_EntryType = "EXP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = exportEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			Factory.Save();
		}
		CusEntryHeader exportEntry;

		void CreateEntryWithOutgoingMessageForExport()
		{
			if (exportEntry == null)
			{
				CreateEntryForExport(true);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._830;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = exportEntry.PK;
			outgoingMessage.EM_LinkedObject = exportEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5UTMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UT;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Incoming";
	}
}

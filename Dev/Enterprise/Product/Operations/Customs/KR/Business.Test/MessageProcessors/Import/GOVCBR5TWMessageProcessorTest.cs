using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5TWMessageProcessorTest : XMLMessageTestHelper<GOVCBR5TWMessageProcessorTest>
	{
		public void TestEM_MessageSubTypeIsOne()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			SampleCodeType();
			var entry = CreateEntryWithOutgoingMessageForImport("2292611001080U");
			var incomingMessage = CreateMessageForTest("GOVCBR5TW_ONE.xml");

			var orgHeader = entry.Declaration.DutyPayer;
			orgHeader.OH_Category = OrgConstants.Category.Business;
			orgHeader.CustomsCodes.AddNew(Constants.IdentificationType.BusinessRegNo, "Business", Core.Constants.CountryCodes.KoreaSouth);
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "TEST";

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EDI_Message.EM_MessageSubType is Empty", ZString.Empty, incomingMessage.EM_MessageSubType);
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();

			AssertEquals("EDI_Message.EM_MessageSubType is updated to ONE", "ONE", incomingMessage.EM_MessageSubType);
			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals(incomingMessage.EM_MessageOwner, ZString.Empty);
			AssertContains("수입(납세)신고 정정 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("021-11-000080", incomingMessage.EM_MessageInterpretation);
			AssertContains("021-11-00-008000", incomingMessage.EM_MessageInterpretation);
			AssertContains("(주)커피빈코리아", incomingMessage.EM_MessageInterpretation);
			AssertContains("박상배", incomingMessage.EM_MessageInterpretation);
			AssertContains("22926-11-001080U", incomingMessage.EM_MessageInterpretation);
			AssertContains("1", incomingMessage.EM_MessageInterpretation);
			AssertContains("2011-04-23", incomingMessage.EM_MessageInterpretation);
			AssertContains("2011-10-25~2011-11-17", incomingMessage.EM_MessageInterpretation);
			AssertContains("분석결과에 따른 감액보정 내용(C-11-04580)", incomingMessage.EM_MessageInterpretation);
			AssertContains("이상없음", incomingMessage.EM_MessageInterpretation);
			AssertContains("2011-09-28", incomingMessage.EM_MessageInterpretation);
			AssertContains("[010] 서울세관", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			AssertContains("22926", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입(납세)신고 정정 신청서", email.Body);
			AssertContains("021-11-000080", email.Body);
			AssertContains("021-11-00-008000", email.Body);
			AssertContains("(주)커피빈코리아", email.Body);
			AssertContains("박상배", email.Body);
			AssertContains("22926-11-001080U", email.Body);
			AssertContains("1", email.Body);
			AssertContains("2011-04-23", email.Body);
			AssertContains("2011-10-25~2011-11-17", email.Body);
			AssertContains("분석결과에 따른 감액보정 내용(C-11-04580)", email.Body);
			AssertContains("이상없음", email.Body);
			AssertContains("2011-09-28", email.Body);
			AssertContains("[010] 서울세관", email.Body);
			AssertContains("0", email.Body);
			AssertContains("22926", email.Body);
		}

		public void TestEM_MessageSubTypeisMUL()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			SampleCodeType();
			var entry1 = CreateEntryWithOutgoingMessageForImport("2292611001049U");
			var entry2 = CreateEntryWithOutgoingMessageForImport("2292611001270U");
			var entry3 = CreateEntryWithOutgoingMessageForImport("2292611001533U");
			var incomingMessage = CreateMessageForTest("GOVCBR5TW_MUL.xml");

			var orgHeader = entry1.Declaration.DutyPayer;
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			orgHeader.CustomsCodes.AddNew(Constants.IdentificationType.UnipassIDForIndividual, "Individual", Core.Constants.CountryCodes.KoreaSouth);
			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "TEST";
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EDI_Message.EM_MessageSubType is Empty", ZString.Empty, incomingMessage.EM_MessageSubType);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();

			AssertEquals("EDI_Message.EM_MessageSubType is updated to MUL", "MUL", incomingMessage.EM_MessageSubType);

			entry1.Reload();
			entry2.Reload();
			entry3.Reload();

			var messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
			messageFilter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			messageFilter.AddToFilter(EDIMessageSchema.EM_MessageType, ElectronicDocumentTypeList.Codes._5TW);
			messageFilter.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, new ZGuid[] { entry1.PK, entry2.PK, entry3.PK });
			var messages = new BusinessObjectFactory().Load<EDIMessage>(messageFilter).Where(x => x.PK != incomingMessage.PK);
			AssertEquals("When EDI_Message.EM_MessageSubType is MUL, Create EDIM", 3, messages.Count());

			var messagesList = messages.ToList();
			AssertEquals("Created Cloned EDIMessage.EM_MessageSubType is 'OST'", "OST", messagesList[0].EM_MessageSubType);
			AssertEquals("Created Cloned EDIMessage.EM_MessageSubType is 'OST'", "OST", messagesList[1].EM_MessageSubType);
			AssertEquals("Created Cloned EDIMessage.EM_MessageSubType is 'OST'", "OST", messagesList[2].EM_MessageSubType);

			AssertNotEquals("EM_MessageNum for 'OST'", incomingMessage.EM_MessageNum, messagesList[0].EM_MessageNum);
			AssertNotEquals("EM_MessageNum for 'OST'", incomingMessage.EM_MessageNum, messagesList[1].EM_MessageNum);
			AssertNotEquals("EM_MessageNum for 'OST'", incomingMessage.EM_MessageNum, messagesList[2].EM_MessageNum);

			Assert("Cloned EDIMessage should connect to object.", messages.Any(x => x.EM_LinkUniqueID == entry1.PK));
			Assert("Cloned EDIMessage should connect to object.", messages.Any(x => x.EM_LinkUniqueID == entry2.PK));
			Assert("Cloned EDIMessage should connect to object.", messages.Any(x => x.EM_LinkUniqueID == entry3.PK));

			AssertEquals("Created Cloned EDIMessage.EM_ApplicationReference is updated to Original EDIMessage.EM_MessageNum", incomingMessage.EM_MessageNum, messagesList[0].EM_ApplicationReference);
			AssertEquals("Created Cloned EDIMessage.EM_ApplicationReference is updated to Original EDIMessage.EM_MessageNum", incomingMessage.EM_MessageNum, messagesList[1].EM_ApplicationReference);
			AssertEquals("Created Cloned EDIMessage.EM_ApplicationReference is updated to Original EDIMessage.EM_MessageNum", incomingMessage.EM_MessageNum, messagesList[2].EM_ApplicationReference);

			AssertEquals(messagesList[0].EM_MessageOwner, ZString.Empty);
			AssertEquals(messagesList[1].EM_MessageOwner, ZString.Empty);
			AssertEquals(messagesList[2].EM_MessageOwner, ZString.Empty);

			var message1 = messages.SingleOrDefault(x => x.EM_MessageText.Contains("2292611001049U") && !x.EM_MessageText.Contains("2292611001270U") && !x.EM_MessageText.Contains("2292611001533U"));
			var message2 = messages.SingleOrDefault(x => x.EM_MessageText.Contains("2292611001270U") && !x.EM_MessageText.Contains("2292611001049U") && !x.EM_MessageText.Contains("2292611001533U"));
			var message3 = messages.SingleOrDefault(x => x.EM_MessageText.Contains("2292611001533U") && !x.EM_MessageText.Contains("2292611001049U") && !x.EM_MessageText.Contains("2292611001270U"));
			AssertNotNull("2292611001049U", message1);
			AssertNotNull("2292611001270U", message2);
			AssertNotNull("2292611001533U", message3);

			var fileReader = new TestFileReader(typeof(GOVCBR5TWMessageProcessorTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5TW_Result1.xml");
			AssertXMLEquals(testFile, message1.EM_MessageText.SubstringSafe(1));

			testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5TW_Result2.xml");
			AssertXMLEquals(testFile, message2.EM_MessageText.SubstringSafe(1));

			testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5TW_Result3.xml");
			AssertXMLEquals(testFile, message3.EM_MessageText.SubstringSafe(1));

			AssertEquals("entry is located", entry1.PK, message1.EM_LinkUniqueID);

			AssertContains("수입(납세)신고 정정 신청서", message1.EM_MessageInterpretation);
			AssertContains("010-11-000096", message1.EM_MessageInterpretation);
			AssertContains("010-64-11-000096", message1.EM_MessageInterpretation);
			AssertContains("(주)커피빈코리아", message1.EM_MessageInterpretation);
			AssertContains("박상배", message1.EM_MessageInterpretation);
			AssertContains("22926-11-001049U", message1.EM_MessageInterpretation);
			AssertContains("1", message1.EM_MessageInterpretation);
			AssertContains("2011-04-23", message1.EM_MessageInterpretation);
			AssertContains("2011-10-25~2011-11-17", message1.EM_MessageInterpretation);
			AssertContains("분석결과에 따른 감액보정 내용(C-11-04595)", message1.EM_MessageInterpretation);
			AssertContains("이상없음", message1.EM_MessageInterpretation);
			AssertContains("2011-09-28", message1.EM_MessageInterpretation);
			AssertContains("[010] 서울세관", message1.EM_MessageInterpretation);
			AssertContains("1", message1.EM_MessageInterpretation);
			AssertContains("22926", message1.EM_MessageInterpretation);
			AssertContains("보정심사결과 첨부내역", message1.EM_MessageInterpretation);
			AssertContains("분석결과에 따른 감액보정 내용(C-22-04595)", message1.EM_MessageInterpretation);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", message1.EM_MessageInterpretation);

			AssertEquals("entry is located", entry2.PK, message2.EM_LinkUniqueID);

			AssertContains("수입(납세)신고 정정 신청서", message2.EM_MessageInterpretation);
			AssertContains("010-11-000096", message2.EM_MessageInterpretation);
			AssertContains("010-64-11-000097", message2.EM_MessageInterpretation);
			AssertContains("(주)커피빈코리아", message2.EM_MessageInterpretation);
			AssertContains("박상배", message2.EM_MessageInterpretation);
			AssertContains("22926-11-001270U", message2.EM_MessageInterpretation);
			AssertContains("1", message2.EM_MessageInterpretation);
			AssertContains("2011-05-19", message2.EM_MessageInterpretation);
			AssertContains("2011-10-25~2011-11-17", message2.EM_MessageInterpretation);
			AssertContains("분석결과에 따른 감액보정 내용(C-11-04595)", message2.EM_MessageInterpretation);
			AssertContains("이상없음", message2.EM_MessageInterpretation);
			AssertContains("2011-09-28", message2.EM_MessageInterpretation);
			AssertContains("[010] 서울세관", message2.EM_MessageInterpretation);
			AssertContains("0", message2.EM_MessageInterpretation);
			AssertContains("22926", message2.EM_MessageInterpretation);

			AssertEquals("entry is located", entry3.PK, message3.EM_LinkUniqueID);

			AssertContains("수입(납세)신고 정정 신청서", message3.EM_MessageInterpretation);
			AssertContains("010-11-000096", message3.EM_MessageInterpretation);
			AssertContains("010-64-11-000098", message3.EM_MessageInterpretation);
			AssertContains("(주)커피빈코리아", message3.EM_MessageInterpretation);
			AssertContains("박상배", message3.EM_MessageInterpretation);
			AssertContains("22926-11-001533U", message3.EM_MessageInterpretation);
			AssertContains("1", message3.EM_MessageInterpretation);
			AssertContains("2011-06-16", message3.EM_MessageInterpretation);
			AssertContains("2011-10-25~2011-11-17", message3.EM_MessageInterpretation);
			AssertContains("분석결과에 따른 감액보정 내용(C-11-04595)", message3.EM_MessageInterpretation);
			AssertContains("이상없음", message3.EM_MessageInterpretation);
			AssertContains("2011-09-28", message3.EM_MessageInterpretation);
			AssertContains("[010] 서울세관", message3.EM_MessageInterpretation);
			AssertContains("1", message3.EM_MessageInterpretation);
			AssertContains("22926", message3.EM_MessageInterpretation);
			AssertContains("보정심사결과 첨부내역", message3.EM_MessageInterpretation);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", message3.EM_MessageInterpretation);

			AssertEquals(3, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Body.Contains("22926-11-001049U"));
			var recipient = email1.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입(납세)신고 정정 신청서", email1.Body);
			AssertContains("010-11-000096", email1.Body);
			AssertContains("010-64-11-000096", email1.Body);
			AssertContains("(주)커피빈코리아", email1.Body);
			AssertContains("박상배", email1.Body);
			AssertContains("22926-11-001049U", email1.Body);
			AssertContains("1", email1.Body);
			AssertContains("2011-04-23", email1.Body);
			AssertContains("2011-10-25~2011-11-17", email1.Body);
			AssertContains("분석결과에 따른 감액보정 내용(C-11-04595)", email1.Body);
			AssertContains("이상없음", email1.Body);
			AssertContains("2011-09-28", email1.Body);
			AssertContains("[010] 서울세관", email1.Body);
			AssertContains("1", email1.Body);
			AssertContains("22926", email1.Body);
			AssertContains("보정심사결과 첨부내역", email1.Body);
			AssertContains("분석결과에 따른 감액보정 내용(C-22-04595)", email1.Body);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", email1.Body);

			var email2 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Body.Contains("22926-11-001270U"));
			AssertContains("수입(납세)신고 정정 신청서", email2.Body);
			AssertContains("010-11-000096", email2.Body);
			AssertContains("010-64-11-000097", email2.Body);
			AssertContains("(주)커피빈코리아", email2.Body);
			AssertContains("박상배", email2.Body);
			AssertContains("22926-11-001270U", email2.Body);
			AssertContains("1", email2.Body);
			AssertContains("2011-05-19", email2.Body);
			AssertContains("2011-10-25~2011-11-17", email2.Body);
			AssertContains("분석결과에 따른 감액보정 내용(C-11-04595)", email2.Body);
			AssertContains("이상없음", email2.Body);
			AssertContains("2011-09-28", email2.Body);
			AssertContains("[010] 서울세관", email2.Body);
			AssertContains("0", email2.Body);
			AssertContains("22926", email2.Body);

			var email3 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Body.Contains("22926-11-001533U"));
			AssertContains("수입(납세)신고 정정 신청서", email3.Body);
			AssertContains("010-11-000096", email3.Body);
			AssertContains("010-64-11-000098", email3.Body);
			AssertContains("(주)커피빈코리아", email3.Body);
			AssertContains("박상배", email3.Body);
			AssertContains("22926-11-001533U", email3.Body);
			AssertContains("1", email3.Body);
			AssertContains("2011-06-16", email3.Body);
			AssertContains("2011-10-25~2011-11-17", email3.Body);
			AssertContains("분석결과에 따른 감액보정 내용(C-11-04595)", email3.Body);
			AssertContains("이상없음", email3.Body);
			AssertContains("2011-09-28", email3.Body);
			AssertContains("[010] 서울세관", email3.Body);
			AssertContains("1", email3.Body);
			AssertContains("22926", email3.Body);
			AssertContains("보정심사결과 첨부내역", email3.Body);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", email3.Body);
		}

		public void TestImport5TW_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR5TW_ONE.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [보정심사결과통지서]2292611001080U 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport5TW_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5TW_ONE.xml");
				CreateEntryForImport(false, "2292611001080U");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[보정심사결과통지서] Response for Declaration Number: B00001000 / 제출번호: 2292611001080U", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입정정신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport5TW_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5TW_ONE.xml");
				CreateEntryForImport(true, "2292611001080U");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[보정심사결과통지서] Response for Declaration Number: B00001000 / 제출번호: 2292611001080U", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
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
			staff2.GS_Code = "T2";
			staff2.GS_LoginName = "Test2";
			staff2.GS_EmailAddress = "ImportGroupTest@wisetechglobal.com";
			importGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = importGroup.PK;
			link2.GK_GS = staff2.PK;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "AG";
			staff3.GS_LoginName = "Agent";
			staff3.GS_EmailAddress = "CusAgent@wisetechglobal.com";
			Factory.Save();
		}
		GlbGroup importGroup;

		void CreateEntryForImport(bool setCusAgent, string importEntryNumber)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.JE_OH_DutyPayer = declaration.BrokerAddress?.Header?.PK ?? ZGuid.Empty;
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			importEntry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = importEntry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = importEntryNumber;
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = importEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
		}
		CusEntryHeader importEntry;

		CusEntryHeader CreateEntryWithOutgoingMessageForImport(string importEntryNumber)
		{
			CreateEntryForImport(true, importEntryNumber);

			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = importEntry;

			return importEntry;
		}

		void SampleCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5TVMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5TW;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}

using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ESRMessageProcessorTest : TestCaseWithFactory
	{
		public void TestESRMessageProcessorWhenMessageHasNoError()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				var glbCompanyCredential = Factory.NewWithValidTestData<GlbCompanyCredential>();
				glbCompanyCredential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
				glbCompanyCredential.GP_GC = GlbCompany.CurrentCompany.PK;
				var interchange = helper.CreateIncomingEDIInterchangeForTest("ESR_Interchange_C500.txt", EDIInterchangeType.ESR);
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
				interchange.Reload();

				AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch());
				var glbExternalPassword = GetGlbExternalPassword();

				AssertEquals(PasswordStatusList.Codes.Valid, glbExternalPassword.GP_PasswordStatus);
				AssertEquals("", glbExternalPassword.GP_StatusReason);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertContains(NotificationSender.ESRNotificationEmailBody, email.Body);
				AssertContains("에러 수신", email.Subject);
			}
		}

		public void TestESRMessageProcessorWhenMessageHasError()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				var glbCompanyCredential = Factory.NewWithValidTestData<GlbCompanyCredential>();
				glbCompanyCredential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
				glbCompanyCredential.GP_GC = GlbCompany.CurrentCompany.PK;
				var outInterchange = helper.CreateOutgoingEDIInterchangeForTest(ElectronicDocumentTypeList.Codes._929);
				var interchange = helper.CreateIncomingEDIInterchangeForTest("ESR_Interchange_C401.txt", EDIInterchangeType.ESR);
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
				interchange.Reload();
				var query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
				var incomingMessage = Factory.LoadTop1<EDIMessage>(query);
				AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch());
				incomingMessage.Reload();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertContains(NotificationSender.ESRNotificationEmailBody, email.Body);
				AssertContains("Identification Error(신원확인오류)", email.Body);
				AssertContains("에러 수신", email.Subject);

				var glbExternalPassword = GetGlbExternalPassword();
				AssertEquals(PasswordStatusList.Codes.Invalid, glbExternalPassword.GP_PasswordStatus);
				AssertEquals("Identification Error(신원확인오류)", glbExternalPassword.GP_StatusReason);
				AssertEquals("{\"custom.CustomsErrCode\":\"C401\",\"custom.CustomsErrDesc\":\"Identification Error(신원확인오류)\"}\r\n", incomingMessage.EM_MessageText);
				glbExternalPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

				interchange = helper.CreateIncomingEDIInterchangeForTest("ESR_Interchange_C402.txt", EDIInterchangeType.ESR);
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
				interchange.Reload();
				query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
				incomingMessage = Factory.LoadTop1<EDIMessage>(query);
				AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch());
				incomingMessage.Reload();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertContains(NotificationSender.ESRNotificationEmailBody, email.Body);
				AssertContains("Unregistered Document Box(등록되지 않은 사서함)", email.Body);
				AssertContains("에러 수신", email.Subject);

				glbExternalPassword = GetGlbExternalPassword();
				AssertEquals(PasswordStatusList.Codes.Invalid, glbExternalPassword.GP_PasswordStatus);
				AssertEquals("Unregistered Document Box(등록되지 않은 사서함)", glbExternalPassword.GP_StatusReason);
				AssertEquals("{\"custom.CustomsErrCode\":\"C402\",\"custom.CustomsErrDesc\":\"Unregistered Document Box(등록되지 않은 사서함)\"}\r\n", incomingMessage.EM_MessageText);
				glbExternalPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

				interchange = helper.CreateIncomingEDIInterchangeForTest("ESR_Interchange_C450.txt", EDIInterchangeType.ESR);
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
				interchange.Reload();
				query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
				incomingMessage = Factory.LoadTop1<EDIMessage>(query);
				AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch());
				incomingMessage.Reload();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertContains(NotificationSender.ESRNotificationEmailBody, email.Body);
				AssertContains("수신정지대상 문서함", email.Body);
				AssertContains("에러 수신", email.Subject);

				glbExternalPassword = GetGlbExternalPassword();
				AssertEquals(PasswordStatusList.Codes.Invalid, glbExternalPassword.GP_PasswordStatus);
				AssertEquals("수신정지대상 문서함", glbExternalPassword.GP_StatusReason);
				AssertEquals("{\"custom.CustomsErrCode\":\"C450\",\"custom.CustomsErrDesc\":\"수신정지대상 문서함\"}\r\n", incomingMessage.EM_MessageText);

				interchange = helper.CreateIncomingEDIInterchangeForTest("ESR_Interchange_C501.txt", EDIInterchangeType.ESR);
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
				interchange.Reload();
				query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
				incomingMessage = Factory.LoadTop1<EDIMessage>(query);
				AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch());
				incomingMessage.Reload();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertContains(NotificationSender.ESRNotificationEmailBody, email.Body);
				AssertContains("'BorderTransportMeans' 요소로 시작하는 올바르지 않은 컨텐츠가 있습니다.", email.Body);
				AssertContains("에러 수신", email.Subject);

				glbExternalPassword = GetGlbExternalPassword();
				AssertEquals(PasswordStatusList.Codes.Invalid, glbExternalPassword.GP_PasswordStatus);
				AssertEquals("When error code is 501, not updated.", "수신정지대상 문서함", glbExternalPassword.GP_StatusReason);
				AssertEquals("{\"custom.CustomsErrCode\":\"C501\",\"custom.CustomsErrDesc\":\"com.mobicware.process.runtime.ProcessRuntimeException: Validate failure:cvc-complex-type.2.4.a: 'BorderTransportMeans' 요소로 시작하는 올바르지 않은 컨텐츠가 있습니다. '{\"urn:kr:gov:kcs:data:standard:KCS_DeclarationOfEXP_830SchemaModule:1:0\":Agent}' 중 하나가 예상됩니다.\"}\r\n", incomingMessage.EM_MessageText);
			}
		}

		public void TestStatusUpdateWithEntry()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				SetupCompanyCredential();
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "ORG";
				staff.GS_LoginName = "Orgin";
				staff.GS_EmailAddress = "OriginalSender@wisetechglobal.com";

				var inInterchange = helper.CreateIncomingEDIInterchangeForTest("ESR_Interchange_C401.txt", EDIInterchangeType.ESR);
				var incomingMessage = helper.CreateIncomingMessageForTest("ESR_Interchange_C401.txt", EDIInterchangeType.ESR);
				inInterchange.ContainedMessages.Add(incomingMessage);
				var outInterchange = helper.CreateOutgoingEDIInterchangeForTest(ElectronicDocumentTypeList.Codes._830);

				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				outInterchange.ContainedMessages[0].EM_LinkedObject = entryHeader;
				outInterchange.ContainedMessages[0].EM_SystemCreateUser = "ORG";
				entryHeader.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch());
				entryHeader.Reload();
				incomingMessage.Reload();
				AssertEquals(CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal, entryHeader.CH_Status);
				AssertEquals(incomingMessage.EM_LinkedObject, entryHeader);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertContains(NotificationSender.ESRNotificationEmailBody, email.Body);
				AssertContains("ESR Response", email.Subject);

				incomingMessage = helper.CreateIncomingMessageForTest("ESR_Interchange_C401.txt", EDIInterchangeType.ESR);
				inInterchange.ContainedMessages.Add(incomingMessage);
				entryHeader.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch());
				entryHeader.Reload();
				incomingMessage.Reload();
				AssertEquals(CustomsMessageStatusTypeList.Codes.ErrorSendingAmendment, entryHeader.CH_Status);
				AssertEquals(incomingMessage.EM_LinkedObject, entryHeader);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("OriginalSender@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains(NotificationSender.ESRNotificationEmailBody, email.Body);
				AssertContains("ESR Response", email.Subject);

				incomingMessage = helper.CreateIncomingMessageForTest("ESR_Interchange_C401.txt", EDIInterchangeType.ESR);
				inInterchange.ContainedMessages.Add(incomingMessage);
				entryHeader.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationSent;
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch());
				entryHeader.Reload();
				incomingMessage.Reload();
				AssertEquals(CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation, entryHeader.CH_Status);
				AssertEquals(incomingMessage.EM_LinkedObject, entryHeader);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertContains(NotificationSender.ESRNotificationEmailBody, email.Body);
				AssertContains("ESR Response", email.Subject);
			}
		}

		public void TestStatusUpdateWithMiscRequest()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				SetupCompanyCredential();
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "ORG";
				staff.GS_LoginName = "Orgin";
				staff.GS_EmailAddress = "OriginalSender@wisetechglobal.com";

				var outInterchange = helper.CreateOutgoingEDIInterchangeForTest(ElectronicDocumentTypeList.Codes._5AC);
				var miscRequestHeader = Factory.New<CusMiscRequestHeader>();
				miscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
				miscRequestHeader.CMR_GB = GlbBranch.CurrentBranch.PK;
				miscRequestHeader.CMR_RequestDate = ZDateTime.Today;
				miscRequestHeader.CMR_CustomsOffice = "01020";
				outInterchange.ContainedMessages[0].EM_LinkedObject = miscRequestHeader;
				outInterchange.ContainedMessages[0].EM_SystemCreateUser = "ORG";

				var inInterchange = helper.CreateIncomingEDIInterchangeForTest("ESR_Interchange_C401.txt", EDIInterchangeType.ESR);
				var incomingMessage = helper.CreateIncomingMessageForTest("ESR_Interchange_C401.txt", EDIInterchangeType.ESR);
				inInterchange.ContainedMessages.Add(incomingMessage);
				miscRequestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch());
				miscRequestHeader.Reload();
				incomingMessage.Reload();
				AssertEquals(CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal, miscRequestHeader.CMR_Status);
				AssertEquals(incomingMessage.EM_LinkedObject, miscRequestHeader);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("OriginalSender@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains(NotificationSender.ESRNotificationEmailBody, email.Body);
				AssertContains("ESR Response", email.Subject);
			}
		}

		public void TestStatusUpdateWithoutOriginalSender()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				SetupCompanyCredential();
				var outInterchange = helper.CreateOutgoingEDIInterchangeForTest(ElectronicDocumentTypeList.Codes._5SG);
				var miscRequestHeader = Factory.New<CusMiscRequestHeader>();
				var entryNum1 = Factory.New<CusEntryNumber>();
				entryNum1.CE_EntryIsSystemGenerated = true;
				entryNum1.CE_ParentID = miscRequestHeader.PK;
				entryNum1.CE_ParentTable = miscRequestHeader.TableName;
				entryNum1.CE_RN_NKCountryCode = GlbBranch.CurrentBranch.Country.RN_Code;
				entryNum1.CE_EntryType = ElectronicDocumentTypeList.Codes._5SG;
				entryNum1.CE_EntryNum = "6N00221000010M";
				entryNum1.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);

				miscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;
				miscRequestHeader.CMR_GB = GlbBranch.CurrentBranch.PK;
				miscRequestHeader.CMR_RequestDate = ZDateTime.Today;
				miscRequestHeader.CMR_CustomsOffice = "01020";
				outInterchange.ContainedMessages[0].EM_LinkedObject = miscRequestHeader;

				var inInterchange = helper.CreateIncomingEDIInterchangeForTest("ESR_Interchange_C401.txt", EDIInterchangeType.ESR);
				var incomingMessage = helper.CreateIncomingMessageForTest("ESR_Interchange_C401.txt", EDIInterchangeType.ESR);
				inInterchange.ContainedMessages.Add(incomingMessage);
				miscRequestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch());
				miscRequestHeader.Reload();
				incomingMessage.Reload();
				AssertEquals(CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal, miscRequestHeader.CMR_Status);
				AssertEquals(incomingMessage.EM_LinkedObject, miscRequestHeader);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains(NotificationSender.ESRNotificationEmailBody, email.Body);
				AssertContains("ESR Response for Request Number: MSC00000001 / 제출번호: 6N00221000010M", email.Subject);
			}
		}

		public void TestStatusUpdateWithCusPollingTransaction()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				SetupCompanyCredential();
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "ORG";
				staff.GS_LoginName = "Orgin";
				staff.GS_EmailAddress = "OriginalSender@wisetechglobal.com";

				var outInterchange = helper.CreateOutgoingEDIInterchangeForTest(Constants.EDIInterchangeType.DOC);
				var cusPollingTransaction = Factory.New<CusPollingTransaction>();
				cusPollingTransaction.CPT_TransactionID = "TEST";
				cusPollingTransaction.CPT_Reference = ElectronicDocumentTypeList.Codes._830;
				cusPollingTransaction.CPT_NumberOfAttempts = 1;
				outInterchange.ContainedMessages[0].EM_LinkedObject = cusPollingTransaction;
				outInterchange.ContainedMessages[0].EM_SystemCreateUser = "ORG";

				var inInterchange = helper.CreateIncomingEDIInterchangeForTest("ESR_Interchange_C401.txt", EDIInterchangeType.ESR);
				var incomingMessage = helper.CreateIncomingMessageForTest("ESR_Interchange_C401.txt", EDIInterchangeType.ESR);
				inInterchange.ContainedMessages.Add(incomingMessage);
				cusPollingTransaction.CPT_Status = CusPollingTransactionStatusList.Codes.Opened;
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch());
				cusPollingTransaction.Reload();
				incomingMessage.Reload();
				AssertEquals(CusPollingTransactionStatusList.Codes.Error, cusPollingTransaction.CPT_Status);
				AssertEquals(incomingMessage.EM_LinkedObject, cusPollingTransaction);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}
		}

		public void TestStatusNotUpdateWhenC901()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				SetupCompanyCredential();

				var inInterchange = helper.CreateIncomingEDIInterchangeForTest("ESR_Interchange_C901.txt", EDIInterchangeType.ESR);
				var incomingMessage = helper.CreateIncomingMessageForTest("ESR_Interchange_C901.txt", EDIInterchangeType.ESR);
				inInterchange.ContainedMessages.Add(incomingMessage);
				var outInterchange = helper.CreateOutgoingEDIInterchangeForTest(ElectronicDocumentTypeList.Codes._830);

				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				outInterchange.ContainedMessages[0].EM_LinkedObject = entryHeader;
				outInterchange.ContainedMessages[0].EM_SystemCreateUser = "ORG";
				entryHeader.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch());
				entryHeader.Reload();
				AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, entryHeader.CH_Status);

				AssertEquals("No email should have been sent", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}
		}

		void SetupCompanyCredential()
		{
			var glbCompanyCredential = Factory.NewWithValidTestData<GlbCompanyCredential>();
			glbCompanyCredential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			glbCompanyCredential.GP_GC = GlbCompany.CurrentCompany.PK;
		}

		GlbExternalPassword GetGlbExternalPassword()
		{
			ZQuery zQuery = new ZQuery(GlbExternalPasswordSchema.GP_GC, GlbCompany.CurrentCompany.PK);
			zQuery.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.KRB);
			zQuery.AddToFilter(GlbExternalPasswordSchema.GP_GS, SQLComparisonOperator.Equal, null);
			zQuery.OrderBy = "GP_SystemCreateTimeUtc";
			return new BusinessObjectFactory().LoadTop1<GlbExternalPassword>(zQuery);
		}

		protected override void SetUp()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "T1";
			staff.GS_LoginName = "Test1";
			staff.GS_EmailAddress = "ImportGroupTest@wisetechglobal.com";
			importGroup = Factory.New<GlbGroup>();
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = importGroup.PK;
			link1.GK_GS = staff.PK;

			helper = new TestDataSetUpHelper(Factory, typeof(ESRMessageProcessorTest));
		}
		TestDataSetUpHelper helper;
		GlbGroup importGroup;
	}
}

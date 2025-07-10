using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2L.Incoming;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.ESConstants;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class T2LCommonResponseMessageProcessorTest<TResponse, TResponseProcessor> : XMLResponseMessageProcessorTest<TResponse, IMessagePrettyFormatter, TResponseProcessor>
		where TResponse : T2LCommonResponseMessageProcessor<TResponseProcessor>
		where TResponseProcessor : class, ICommonServiceSegment, IT2LCommon, IResponseCode
	{
		public void TestProcessRejectedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: RejectedMessageInterpretation, emStatus: RejectedMessageStatus, chStatus: RejectedMessageStatus, messageNum: MessageNum, messageSubType: "REJ", entryStatusCode: RejectedEntryStatus, movementReferenceNumber: RejectedMRN);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			staff = staffWithCertificateHelperTest.Staff;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.T2L;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.CH_EntryStatus = OriginalEntryStatus;

			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			var sentInterchange = SetSentInterchange(entryHeader, InterchangeID);
			sentInterchange.EI_To = SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub;
			sentMessage = (TestEdiMessage)sentInterchange.ContainedMessages[0];

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "INI", "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLP", "T2L Declaration Accepted", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLR", "T2L Declaration Accepted And Cleared", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CDA", "T2L Declaration Needs Inspection", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
			entryStatusList = RefCusCodeListTypes.GetCachedList(Factory, EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today);
		}
		protected CodeDescriptionPairList entryStatusList;
		protected TestEdiMessage sentMessage;

		protected abstract string GetRejectedTestFile();
		protected override sealed string GetAcceptanceTestFileWithLongSegmentId() => string.Empty;

		protected abstract ZString RejectedMRN { get; }
		protected virtual ZString RejectedMessageStatus => EDIMessage.Status.Received;
		ZString RejectedEntryStatus => OriginalEntryStatus;
		protected abstract ZString RejectedMessageInterpretation { get; }

		protected const string MRNCode = "20ES00999930006184";
		protected const string MessageNum = "20200507145032108085";
		protected readonly ZDateTime AcceptanceDate = new ZDateTime(2020, 05, 07, 14, 50, 32);
		protected const string CsvClearance = "TEST444444444444";
		protected const string CsvElectronicDeclaration = "HHAXSXPQA6NT963Y";
		protected const string MRNCodeExisting = "21ES009999L0000001";
		protected readonly ZDateTime AcceptanceDateExisting = new ZDateTime(2021, 01, 15, 05, 40, 55);

		protected CusEntryHeader CreateT2LAnnexDeclaration(ZString applicationReference, ZGuid interchangeId, ZBool addSecondAnnexDoc, bool addThirdAnnexDoc = false)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Enterprise.Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Enterprise.Customs.EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.DontReAssignReferenceNoForUnitTest = true;

			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);
			Factory.Save();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = OriginalEntryStatus;
			entryHeader.ZG_POUSVersion = POUSVersionCodes.NoPOUS;

			Factory.Save();

			entryHeader.CH_BGMReference = applicationReference;
			Factory.Save();

			var sentInterchange = Factory.New<EDIInterchange>();
			sentInterchange.EI_SessionGUID = interchangeId;
			sentInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			sentInterchange.EI_From = "CW1";
			sentInterchange.EI_To = SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub;

			sentMessage = Factory.New<TestEdiMessage>();
			sentMessage.EM_MessageNum = SentMessageNumber;
			sentMessage.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			sentMessage.EM_MessageType = DeclarationMessageTypeList.Codes.T2lAnnex;
			sentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sentMessage.EM_Status = EDIMessage.Status.Sent;
			entryHeader.Messages.Add(sentMessage);
			sentInterchange.ContainedMessages.Add(sentMessage);

			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var pivot = entryHeader.EDocPivotCollection.AddNew();
			pivot.CSD_StorageDocReference = eDoc.UniqueKey;

			if (addSecondAnnexDoc)
			{
				var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice2.txt", "MSC");
				var pivot2 = entryHeader.EDocPivotCollection.AddNew();
				pivot2.CSD_StorageDocReference = eDoc2.UniqueKey;
			}

			if (addThirdAnnexDoc)
			{
				var eDoc3 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice3.txt", "FUB");
				var pivot3 = entryHeader.EDocPivotCollection.AddNew();
				pivot3.CSD_StorageDocReference = eDoc3.UniqueKey;
			}

			Factory.Save();
			declaration.DocManagerInfo.Save();

			return entryHeader;
		}

		protected ZString ApplicationReference2 => "ES000002";

		protected ZGuid InterchangeID2 => new ZGuid("242A4F1F-A4FC-4D1E-A682-37D5DF87BA4F");

		protected override EDIInterchange CreateTestResponseInterchange(ZGuid interchangeID, ZString interchangeTransportType)
		{
			var interchange = base.CreateTestResponseInterchange(interchangeID, interchangeTransportType);
			if (interchangeTransportType != EDIInterchange.TransportType.xT)
			{
				interchange.EI_HeaderText = ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AZ</BrokerCode>
  <CertificateName>CertName</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
  <TestMessage>Y</TestMessage>
  <Service>Service</Service>
  <Operation>Operation</Operation>
  <SentEDIMessageNumber>1</SentEDIMessageNumber>
</Headers>");
			}
			return interchange;
		}

		GlbStaff staff;
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(RefundRequest))]
	sealed class RefundRequestTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new RefundRequest(Create5ULMessage());

		public void TestGetOtherData()
		{
			var outgoingMessage = Create5ULMessage();

			var otherInstruction = declaration.CustomsEntryInstructions.AddNew();
			otherInstruction.CEI_DataModel = Core.Constants.CountryCodes.KoreaSouth;

			supporting.CSI_ParentID = otherInstruction.PK;
			supporting.CSI_ParentTableCode = otherInstruction.TablePrefix;
			supporting.CSI_Type = ElectronicDocumentTypeList.Codes._5UL;
			supporting.CSI_ReferenceNumber = "416341950319U";
			supporting.CSI_Status = CustomsEntryStatusTypeList.Codes.ANT;
			supporting.CSI_ReferenceNumber2 = "1234567890123456789";
			supporting.CSI_DateOfExpiry = new ZDateTime(2025, 02, 26);
			supporting.CSI_Tariff = "030752019608";
			supporting.CSI_SystemCreateTimeUtc = new ZDateTime(2025, 02, 24);

			var entryNum5UL = entry.EntryNumbers.AddNew();
			entryNum5UL.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNum5UL.CE_EntryNum = "416341950319U";
			entryNum5UL.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			entryNum5UL.CE_IssueDate = new ZDateTime(2025, 02, 25);
			entryNum5UL.CE_EntryLineReference = "1";

			var fileReader = new TestFileReader(typeof(RefundRequestTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5UN_CUS.xml");
			var incomingMessage5UN = CreateMessageForTest(ElectronicDocumentTypeList.Codes._5UN);
			incomingMessage5UN.EM_MessageText = messageText;
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage5UN.Reload();

			AssertEquals(incomingMessage5UN.EM_ApplicationReference, outgoingMessage.EM_MessageNum);
			Factory.Save();

			var wrapper = new RefundRequest(outgoingMessage);

			AssertEquals(ZString.Empty, wrapper.RefundDeclarationNumber);

			supporting.CSI_ParentID = instruction.PK;
			supporting.CSI_ParentTableCode = instruction.TablePrefix;
			Factory.Save();

			wrapper = new RefundRequest(outgoingMessage);

			AssertEquals("41634-19-50319U", wrapper.RefundDeclarationNumber);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, wrapper.MessageStatus5UL);
			AssertEquals(CustomsMessageStatusTypeList.Descriptions.OriginalAccepted, wrapper.MessageStatusDesc5UL);
			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, wrapper.ReviewResult);
			AssertEquals(CustomsEntryStatusTypeList.Descriptions.ANT, wrapper.ReviewResultDesc);
			AssertEquals(new ZDateTime(2025, 02, 25), wrapper.AcceptedDate5UL);
			AssertEquals("1234-567-89-01-2-345678-9", wrapper.CustomsDisbursementBillNumber);
			AssertEquals(new ZDateTime(2025, 02, 26), wrapper.RefundApprovalDate);
			AssertEquals("030752019608", wrapper.RefundApprovalNumber);

			AssertEquals(new ZDateTime(2019, 12, 19), wrapper.ProvisionDate);
			AssertEquals("030211900030036", wrapper.ProvisionNo);

			var supporting2 = Factory.New<CusSupportingInfo>();
			supporting2.CSI_Type = ElectronicDocumentTypeList.Codes._5UL;
			supporting2.CSI_ParentID = instruction.PK;
			supporting2.CSI_ParentTableCode = instruction.TablePrefix;
			supporting2.CSI_ReferenceNumber = "416341950319U";
			supporting2.CSI_Status = CustomsEntryStatusTypeList.Codes.DMS;
			supporting2.CSI_ReferenceNumber2 = "1234567890123456789";
			supporting2.CSI_DateOfExpiry = new ZDateTime(2025, 01, 16);
			supporting2.CSI_Tariff = "123452019608";
			supporting2.CSI_SystemCreateTimeUtc = new ZDateTime(2025, 01, 15);
			supporting2.CSI_SystemCreateTimeUtc = new ZDateTime(2025, 03, 15);
			Factory.Save();

			wrapper = new RefundRequest(outgoingMessage);

			AssertEquals(CustomsEntryStatusTypeList.Codes.DMS, wrapper.ReviewResult);
			AssertEquals(CustomsEntryStatusTypeList.Descriptions.DMS, wrapper.ReviewResultDesc);
			AssertEquals(new ZDateTime(2025, 01, 16), wrapper.RefundApprovalDate);
			AssertEquals("123452019608", wrapper.RefundApprovalNumber);
		}

		EDIMessage Create5ULMessage()
		{
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageOwner = "416341950319U";
			outgoingMessage.EM_MessageNum = "1";
			entry.Messages.Add(outgoingMessage);

			supporting = Factory.New<CusSupportingInfo>();

			return outgoingMessage;
		}
		CusSupportingInfo supporting;

		EDIMessage CreateMessageForTest(string messageType)
		{
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = messageType;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			return incomingMessage;
		}

		public string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";

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
			var importGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = importGroup.PK;
			link2.GK_GS = staff2.PK;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "AG";
			staff3.GS_LoginName = "Agent";
			staff3.GS_EmailAddress = "CusAgent@wisetechglobal.com";
			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();

			instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_DataModel = Core.Constants.CountryCodes.KoreaSouth;
			entry.CH_CEI_Instruction = instruction.PK;
		}
		JobDeclaration declaration;
		CusEntryHeader entry;
		CusEntryInstruction instruction;
	}
}

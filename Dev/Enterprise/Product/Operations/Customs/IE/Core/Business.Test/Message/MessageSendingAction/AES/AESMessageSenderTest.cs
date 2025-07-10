using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC511C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC513C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC514C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC515C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC570C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC573C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC613C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC614C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC615C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX513;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	class AESMessageSenderTest : TestCaseWithFactory
	{
		public void TestSend_ExportPresentation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var credential = InterchangeProcessorTestHelper.CreateValidCredential(declaration.Company);
			credential.GP_MailBoxID = "MS12345";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			CombineAssertions(() =>
			{
				(var xmlObject, var message) = AssertSend<Cc511C>(AESOutgoingMessageTypeList.Codes.ExportPresentation, entryHeader, true);
				AssertEquals("MessageSender", "MS12345", xmlObject.MessageSender);
				AssertEquals("MessageIdentification", message.EM_MessageNum, xmlObject.MessageIdentification);
			});
		}

		public void TestSend_Original()
		{
			//TODO: should replace this with fully populated data to test end to end
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var credential = InterchangeProcessorTestHelper.CreateValidCredential(declaration.Company);
			credential.GP_MailBoxID = "MS12345";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			CombineAssertions(() =>
			{
				(var xmlObject, var message) = AssertSend<Cc515C>(AESOutgoingMessageTypeList.Codes.ExportOriginal, entryHeader, false);
				AssertEquals("MessageSender", "MS12345", xmlObject.MessageSender);
				AssertEquals("MessageIdentification", message.EM_MessageNum, xmlObject.MessageIdentification);
			});
		}

		[TestDate(2021, 01, 01)]
		public void TestSend_Original_HasANewLRN()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var cei1 = declaration1.CustomsEntryInstructions.AddNew();
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = cei1.PK;

			(var xmlObject1, var message1) = AssertSend<Cc515C>(AESOutgoingMessageTypeList.Codes.ExportOriginal, entryHeader1, false);

			var declaration2 = Factory.New<JobDeclaration>();
			var cei2 = declaration2.CustomsEntryInstructions.AddNew();
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = cei2.PK;

			(var xmlObject2, var message2) = AssertSend<Cc515C>(AESOutgoingMessageTypeList.Codes.ExportOriginal, entryHeader2, false);

			var prefix = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID + declaration1.Branch.GB_Code;
			var expectedLRN1 = prefix + "2100000001V01";
			var expectedLRN2 = prefix + "2100000002V01";

			CombineAssertions(() =>
			{
				AssertEquals("LRN declaration 1", expectedLRN1, entryHeader1.CH_BGMReference);
				AssertContains("Message created 1 has correct LRN version", expectedLRN1, message1.EM_MessageText);
				AssertEquals("LRN declaration 2", expectedLRN2, entryHeader2.CH_BGMReference);
				AssertContains("Message created 2 has correct LRN version", expectedLRN2, message2.EM_MessageText);
			});
		}

		[TestDate(2021, 01, 01)]
		public void TestSend_OriginalWithLRNHasANewLRNVersion()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cei.PK;

			AssertSend<Cc515C>(AESOutgoingMessageTypeList.Codes.ExportOriginal, entryHeader, false);

			entryHeader.Messages.RemoveAll();

			(var xmlObject, var message) = AssertSend<Cc515C>(AESOutgoingMessageTypeList.Codes.ExportOriginal, entryHeader, false);

			var prefix = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID + declaration.Branch.GB_Code;
			var expectedLRN = prefix + "2100000001V02";

			CombineAssertions(() =>
			{
				AssertEquals("Entry Header has correct LRN version", expectedLRN, entryHeader.CH_BGMReference);
				AssertContains("Message created has correct LRN version", expectedLRN, message.EM_MessageText);
			});
		}

		[TestDate(2021, 01, 01)]
		public void TestSend_OriginalAfterIE917ReceivedHasExistingLRNVersion()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cei.PK;

			AssertSend<Cc515C>(AESOutgoingMessageTypeList.Codes.ExportOriginal, entryHeader, false);

			entryHeader.Messages.RemoveAll();

			entryHeader.CH_CEI_Instruction = cei.PK;
			entryHeader.CH_Status = LogicalStatusList.Codes.Accepted;

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE917;
			message.EM_LinkedObject = entryHeader;

			AssertSend<Cc515C>(AESOutgoingMessageTypeList.Codes.ExportOriginal, entryHeader, false);

			var prefix = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID + declaration.Branch.GB_Code;
			var expectedLRN = prefix + "2100000001V01";

			CombineAssertions(() =>
			{
				AssertEquals("LRN declaration", expectedLRN, entryHeader.CH_BGMReference);
			});
		}

		[TestDate(2021, 01, 01)]
		public void TestSend_OriginalAfterIE556ReceivedHasANewLRNVersion()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cei.PK;

			AssertSend<Cc515C>(AESOutgoingMessageTypeList.Codes.ExportOriginal, entryHeader, false);

			entryHeader.Messages.RemoveAll();

			entryHeader.CH_CEI_Instruction = cei.PK;
			entryHeader.CH_Status = LogicalStatusList.Codes.Accepted;

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsExport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE556;
			message.EM_LinkedObject = entryHeader;

			AssertSend<Cc515C>(AESOutgoingMessageTypeList.Codes.ExportOriginal, entryHeader, false);

			var prefix = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID + declaration.Branch.GB_Code;
			var expectedLRN = prefix + "2100000001V02";

			CombineAssertions(() =>
			{
				AssertEquals("LRN declaration", expectedLRN, entryHeader.CH_BGMReference);
			});
		}

		[TestDate(2021, 01, 01)]
		public void TestSend_AmendmentWithLRNHasExistingLRNVersion()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cei.PK;

			AssertSend<Cc515C>(AESOutgoingMessageTypeList.Codes.ExportOriginal, entryHeader, false);

			entryHeader.Messages.RemoveAll();

			entryHeader.CH_CEI_Instruction = cei.PK;
			entryHeader.CH_Status = LogicalStatusList.Codes.Accepted;

			(var xmlObject, var messageAmendment) = AssertSend<Cc513C>(AESOutgoingMessageTypeList.Codes.ExportAmendment, entryHeader, false);

			var prefix = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID + declaration.Branch.GB_Code;
			var expectedLRN = prefix + "2100000001V01";

			CombineAssertions(() =>
			{
				AssertEquals("LRN declaration", expectedLRN, entryHeader.CH_BGMReference);
				AssertContains("Amendment Message created has original LRN version", expectedLRN, messageAmendment.EM_MessageText);
			});
		}

		public void TestSend_Amendment()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cei.PK;
			entryHeader.CH_Status = LogicalStatusList.Codes.Accepted;
			AssertSend<Cc513C>(AESOutgoingMessageTypeList.Codes.ExportAmendment, entryHeader, false);
		}

		public void TestSend_ExportInvalidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "IEORK400";

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IE08970987987");
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeclarantType = "1";

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			exporter.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EXP6123");
			declaration.ExporterDocAddress.E2_OA_Address = exporter.MainAddress.PK;

			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExport, "AZ098765");

			var cei = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("22IEDU4EX151267688");
			entryHeader.CH_BGMReference = "LRN2343234242";

			entryHeader.CH_CEI_Instruction = cei.PK;
			entryHeader.CH_Status = LogicalStatusList.Codes.Accepted;

			AssertSend<Cc514C>(AESOutgoingMessageTypeList.Codes.ExportCancellation, entryHeader, true, extraSetup: action => action.Annotation = "INV");
		}

		[TestDate(2021, 01, 01)]
		public void TestSend_ReExport()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var instruction = declaration1.CustomsEntryInstructions.AddNew();
			var invoice = declaration1.Invoices.AddNew();
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader1.MergedLines.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			(var xmlObject1, var message1) = AssertSend<Cc570C>(AESOutgoingMessageTypeList.Codes.ReExport, entryHeader1, false);

			var declaration2 = Factory.New<JobDeclaration>();
			var cei2 = declaration2.CustomsEntryInstructions.AddNew();
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = cei2.PK;

			(var xmlObject2, var message2) = AssertSend<Cc570C>(AESOutgoingMessageTypeList.Codes.ReExport, entryHeader2, false);

			var prefix = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID + declaration1.Branch.GB_Code;
			var expectedLRN1 = prefix + "2100000001V01";
			var expectedLRN2 = prefix + "2100000002V01";

			CombineAssertions(() =>
			{
				AssertEquals("LRN declaration 1", expectedLRN1, entryHeader1.CH_BGMReference);
				AssertContains("Message created 1 has correct LRN version", expectedLRN1, message1.EM_MessageText);
				AssertEquals("LRN declaration 2", expectedLRN2, entryHeader2.CH_BGMReference);
				AssertContains("Message created 2 has correct LRN version", expectedLRN2, message2.EM_MessageText);
			});
		}

		public void TestSend_ReExportStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			(var _, var msg) = AssertSend<Cc570C>(AESOutgoingMessageTypeList.Codes.ReExport, entryHeader, false);
			AssertNotEquals($"Message status should not be {LogicalStatusList.Codes.Error}", LogicalStatusList.Codes.Error, msg.EM_Status);
		}

		public void TestSend_ReExportAmendment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			AssertSend<Cc573C>(AESOutgoingMessageTypeList.Codes.ReExportAmendment, entryHeader, false);
		}

		public void TestSend_ExitOriginal()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			AssertSend<Cc615C>(AESOutgoingMessageTypeList.Codes.ExitOriginal, entryHeader, true);
		}

		public void TestSend_ExitAmendment()
		{
			var declaration = Factory.New<JobDeclaration>();

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IE08970987987");
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");
			declaration.JE_OA_Representative = declarant.MainAddress.PK;

			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "AZ098765");
			declaration.JE_DeclarantType = "1";
			var cei = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("22IEDU4EX151267639");
			entryHeader.CH_CEI_Instruction = cei.PK;
			entryHeader.CH_Status = LogicalStatusList.Codes.Accepted;

			AssertSend<Cc613C>(AESOutgoingMessageTypeList.Codes.ExitAmendment, entryHeader, true);
		}

		public void TestSend_ExitCancellation()
		{
			var declaration = Factory.New<JobDeclaration>();

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IE08970987987");
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");
			declaration.JE_OA_Representative = declarant.MainAddress.PK;

			declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfExit, "AZ098765");
			declaration.JE_DeclarantType = "1";
			var cei = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("22IEDU4EX151267639");
			entryHeader.CH_CEI_Instruction = cei.PK;
			entryHeader.CH_Status = LogicalStatusList.Codes.Accepted;

			AssertSend<Cc614C>(AESOutgoingMessageTypeList.Codes.ExitCancellation, entryHeader, true, extraSetup: action => action.Annotation = "CAN");
		}

		public void TestSend_ReleaseAmendment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var credential = InterchangeProcessorTestHelper.CreateValidCredential(declaration.Company);
			credential.GP_MailBoxID = "MS12345";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			CombineAssertions(() =>
			{
				(var xmlObject, var message) = AssertSend<Ex513>(AESOutgoingMessageTypeList.Codes.ReleaseAmendment, entryHeader, true);
				AssertEquals("MessageSender", "MS12345", xmlObject.MessageSender);
				AssertEquals("MessageIdentification", message.EM_MessageNum, xmlObject.MessageIdentification);
			});
		}

		(T xmlObj, AESOutboundEDIMessage message) AssertSend<T>(string messageType, CusEntryHeader entryHeader, bool isEndToEndTest, Action<AESMessageSendingAction> extraSetup = null)
		where T : IAESMessageXmlObject
		{
			var result = default(T);
			var sendingAction = new AESMessageSendingAction(entryHeader);
			sendingAction.MessageType = messageType;
			extraSetup?.Invoke(sendingAction);

			var sender = new AESMessageSender(sendingAction);
			sender.Send();
			Factory.Save();
			var createdMessage = (AESOutboundEDIMessage)entryHeader.Messages.Single();
			if (isEndToEndTest)
			{
				using (var reader = createdMessage.GetEM_MessageTextReader())
				{
					result = IEXmlObjectSerializer.Deserialize<T>(reader);
					AssertNotNull(result);
				}
			}
			else
			{
				result = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.Deserialize<T>(createdMessage.EM_MessageText);
				AssertNotNull(result);
			}
			return (result, createdMessage);
		}
	}
}

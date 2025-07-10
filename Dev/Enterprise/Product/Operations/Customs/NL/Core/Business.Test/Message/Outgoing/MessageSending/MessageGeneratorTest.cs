using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class MessageGeneratorTest : TestCaseWithFactory
{
	[TestDate(2022, 02, 24, 15, 48, 23)]
	public void TestGenerate()
	{
		var currentCompanyOrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		var senderIDCollection = new SenderInfoCollection();
		var senderID = senderIDCollection.AddNew();
		senderID.OrganizationPK = currentCompanyOrgHeader;
		senderID.SenderID = "NL56785678";
		senderID.DefaultSenderID = true;

		using (NLCustomsRegistry.Instance.SenderIDs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, senderIDCollection))
		{
			var expectedMessage = MessageGeneratorTestHelper.GetExpectedMessageXML("Enterprise.Customs.NL.Business.Testing.Message.Outgoing.MessageSending.TestFiles.H1Message.xml");

			var entry = WrapperTestHelper.GetEntryHeaderForTest(Factory);
			MessageGeneratorTestHelper.CreateSupportingDocument(entry.EntryInstruction.SupportingDocuments, 1, "REF-123", "REF2-123", new ZDateTime(2021, 12, 31, 23, 59, 59), "REF");
			MessageGeneratorTestHelper.CreateSupportingDocument(entry.EntryInstruction.SupportingDocuments, 1, "REF-789", "REF2-456", new ZDateTime(2021, 12, 25, 21, 23, 49), "RF2");

			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.MessageType = ImportSendMessageTypes.Codes.DEC;
			var messageSendingObjects = new[] { messageSendingObject };
			var generator = GetGenerator(messageSendingObjects);

			var result = generator.Generate(entry);

			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.Queued, result.Message.EM_Status);
				AssertEquals("Message.EM_MessageType", NLEDIMessageTypes.Codes.DMS, result.Message.EM_MessageType);
				AssertEquals("Message.EM_MessageSubType", ImportSendMessageTypes.Codes.DEC, result.Message.EM_MessageSubType);
				AssertEquals("Message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, result.Message.EM_ReceiveTransmit);
				AssertEquals("Message.EM_ApplicationReference", "MRN123", result.Message.EM_ApplicationReference);
				AssertSame("Linked Object", entry, result.Message.EM_LinkedObject);

				AssertNotNullOrEmpty(result.Message.EM_MessageText);
				AssertXMLEquals("Message content", expectedMessage, result.Message.EM_MessageText);
			});
		}
	}

	[TestDate(2022, 02, 24, 15, 48, 23)]
	public void TestCAN_CustomsMessageRemarks()
	{
		var currentCompanyOrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		var senderIDCollection = new SenderInfoCollection();
		var senderID = senderIDCollection.AddNew();
		senderID.OrganizationPK = currentCompanyOrgHeader;
		senderID.SenderID = "NL56785678";
		senderID.DefaultSenderID = true;

		using (NLCustomsRegistry.Instance.SenderIDs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, senderIDCollection))
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			var entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.ReasonForInvalidation = "invalidation reason";
			messageSendingObject.MessageType = ImportSendMessageTypes.Codes.CAN;
			var messageSendingObjects = new[] { messageSendingObject };
			var generator = GetGenerator(messageSendingObjects);

			var result = generator.Generate(entry);
			var message = (NLEDIMessage)result.Message;
			AssertEquals("Message.CustomsMessageRemarks", "CUS|invalidation reason", message.CustomsMessageRemarks);
		}
	}

	public void TestGenerate_ImportAmendmentMessage()
	{
		var entry = WrapperTestHelper.GetEntryHeaderForTest(Factory);
		var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
		var messageSendingObjects = new[] { messageSendingObject };
		messageSendingObject.MessageType = ImportSendMessageTypes.Codes.AMD;
		var generator = GetGenerator(messageSendingObjects);

		AssertExceptionThrown<TargetInvocationException>("No ComparedMetaData", () => generator.Generate(entry));
	}

	public void TestGenerate_ExportAmendmentMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.SupplierDocumentaryAddress.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = DeclarationTypeList.Codes.B1;
		var iSendObject = new Customs.Business.SendsMessagesToCustomsShutterUpperer(true);
		declaration.MessageInitiator = iSendObject;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		declaration.DoMerge();

		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders.Cast<Declaration.CusEntryHeader>().Single();
		entryHeader.CH_EntryStatus = EntryStatus.AdvanceDeclarationReceived;
		WrapperTestHelper.CreateSentMessage(entryHeader, ExportSendMessageTypes.Codes.DEC, "EXA");

		var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
		var messageSendingObject = decWrapper.SendingObjectsCollection[0];
		messageSendingObject.MessageType = ExportSendMessageTypes.Codes.AMD;
		messageSendingObject.ShouldSend = true;
		messageSendingObject.Update = true;

		var sender = new JobDeclarationMessageSender(decWrapper);
		sender.Send(iSendObject);

		CombineAssertions(() =>
		{
			AssertEquals("CH_PhaseStatus", CustomsEntryPhaseStatusList.Codes._513, entryHeader.CH_PhaseStatus);
			AssertEquals("CH_Status", NLConstants.StatusNew.SentToCustoms, entryHeader.CH_Status);
		});
	}

	[TestDate(2022, 02, 24, 15, 48, 23)]
	public void TestGenerate_FallbackMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.SupplierDocumentaryAddress.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;
		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = DeclarationTypeList.Codes.B1;
		var iSendObject = new Customs.Business.SendsMessagesToCustomsShutterUpperer(true);
		declaration.MessageInitiator = iSendObject;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		declaration.DoMerge();

		Factory.Save();

		var entryHeader = declaration.CustomsEntryHeaders.Cast<Declaration.CusEntryHeader>().Single();
		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.FBK;

		var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
		var messageSendingObject = decWrapper.SendingObjectsCollection[0];
		messageSendingObject.MessageType = ExportSendMessageTypes.Codes.FBK;
		messageSendingObject.ShouldSend = true;

		var sender = new JobDeclarationMessageSender(decWrapper);
		sender.Send(iSendObject);

		CombineAssertions(() =>
		{
			AssertEquals("FallbackEntryNumberIssueDate", new ZDateTime(2022, 02, 24, 15, 48, 23), entryHeader.FallbackEntryNumberIssueDate);
			AssertEquals("EM_HeldUntilDate", ZDateTime.MaxSmallDateTime, entryHeader.Messages[0].EM_HeldUntilDate);
		});
	}

	public void TestMakePrettyForInterpretation()
	{
		var entry = WrapperTestHelper.GetEntryHeaderForTest(Factory);
		var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
		var messageSendingObjects = new[] { messageSendingObject };
		var generator = GetGenerator(messageSendingObjects);
		var generatorResult = generator.Generate(entry);
		var result = generator.MakePrettyForInterpretation(generatorResult.Message);

		var makePrettyExpected = "<font size='2' face='Courier New'>" + generatorResult.Message.EM_MessageText + "</font>";

		AssertNotNullOrEmpty(result);
		AssertEquals("Message Interpretation", makePrettyExpected, result);
	}

	public void TestMappingWcoTypeCode_Import()
	{
		var entry = WrapperTestHelper.GetEntryHeaderForTest(Factory);
		var sendingObject = new JobDeclarationMessageSendingObject(entry);
		var generator = new MessageGeneratorForTest(new[] { sendingObject });

		CombineAssertions(() =>
		{
			sendingObject.MessageType = ImportSendMessageTypes.Codes.CAN;
			AssertEquals("INV > CC414A", "CC414A", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.MessageType =	ImportSendMessageTypes.Codes.PRE;
			AssertEquals("GPR > CC432A", "CC432A", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.MessageType = ImportSendMessageTypes.Codes.DEC;
			sendingObject.Declaration.CustomsEntryInstructions[0].CEI_Style = "I1";
			AssertEquals("DEC (I1) > CC415A", "CC415A", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.Declaration.CustomsEntryInstructions[0].CEI_Style = "H1";
			AssertEquals("DEC (H1) > CC415A", "CC415A", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.Declaration.CustomsEntryInstructions[0].CEI_Style = "H2";
			AssertEquals("DEC (H2) > CC415A", "CC415A", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.Declaration.CustomsEntryInstructions[0].CEI_Style = "H3";
			AssertEquals("DEC (H3) > CC415A", "CC415A", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.Declaration.CustomsEntryInstructions[0].CEI_Style = "H4";
			AssertEquals("DEC (H4) > CC415A", "CC415A", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.Declaration.CustomsEntryInstructions[0].CEI_Style = "H5";
			AssertEquals("DEC (H5) > CC415A", "CC415A", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.Declaration.CustomsEntryInstructions[0].CEI_Style = "H6";
			AssertEquals("DEC (H6) > CC415A", "CC415A", generator.GetWcoTypeCodeExposed(sendingObject));
		});
	}

	public void TestMappingWcoTypeCode_Export()
	{
		var entry = WrapperTestHelper.GetEntryHeaderForTest(Factory);
		entry.Declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var sendingObject = new JobDeclarationMessageSendingObject(entry);
		var generator = new MessageGeneratorForTest(new[] { sendingObject });

		CombineAssertions(() =>
		{
			sendingObject.MessageType = ExportSendMessageTypes.Codes.AMD;
			AssertEquals("AMD > CC513C", "CC513C", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.MessageType = ExportSendMessageTypes.Codes.CRE;
			AssertEquals("CRE > CCCREA", "CCCREA", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.MessageType = ExportSendMessageTypes.Codes.DEC;
			sendingObject.Declaration.CustomsEntryInstructions[0].CEI_Style = "B2";
			AssertEquals("DEC (B2) > CC515C", "CC515C", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.Declaration.CustomsEntryInstructions[0].CEI_Style = "B3";
			AssertEquals("DEC (B3) > CC515C", "CC515C", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.Declaration.CustomsEntryInstructions[0].CEI_Style = "C2";
			AssertEquals("DEC (C2) > CC515C", "CC515C", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.MessageType = ExportSendMessageTypes.Codes.CAN;
			AssertEquals("INV > CC514C", "CC514C", generator.GetWcoTypeCodeExposed(sendingObject));
			sendingObject.MessageType = ExportSendMessageTypes.Codes.PRE;
			AssertEquals("PRN > CC511C", "CC511C", generator.GetWcoTypeCodeExposed(sendingObject));
		});
	}

	public void TestWithEmptyWrappers()
	{
		var entry = WrapperTestHelper.GetEmptyEntryDataForTest(Factory);
		var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
		var messageSendingObjects = new[] { messageSendingObject };
		var generator = GetGenerator(messageSendingObjects);

		AssertNoExceptionThrown(() => generator.Generate(entry));
	}

	public void TestMessageTextCRE()
	{
		var entry = WrapperTestHelper.GetEntryHeaderForTest(Factory);
		entry.Declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
		messageSendingObject.MessageType = ExportSendMessageTypes.Codes.CRE;
		var messageSendingObjects = new[] { messageSendingObject };
		var generator = GetGenerator(messageSendingObjects);
		var message = generator.Generate(entry).Message;
		AssertNotNullOrEmpty(message.EM_MessageText);
	}

	static IMessageGenerator<CusEntryHeader> GetGenerator(IEnumerable<JobDeclarationMessageSendingObject> objectsToSend)
	{
		return new MessageGenerator(objectsToSend);
	}

	sealed class MessageGeneratorForTest : MessageGenerator
	{
		public MessageGeneratorForTest(IEnumerable<JobDeclarationMessageSendingObject> objectsToSend) : base(objectsToSend)
		{
		}

		public ZString GetWcoTypeCodeExposed(JobDeclarationMessageSendingObject sendingObject) => GetWcoType(sendingObject);
	}
}

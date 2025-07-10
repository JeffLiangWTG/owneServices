using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

abstract class MessageGenerationAbstractTest : TestCaseWithFactory
{
	[TestDate(2022, 02, 24, 15, 48, 23)]
	public void TestGenerationMessage()
	{
		var currentCompanyOrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
		var senderIDCollection = new SenderInfoCollection();
		var senderID = senderIDCollection.AddNew();
		senderID.OrganizationPK = currentCompanyOrgHeader;
		senderID.SenderID = "NL56785678";
		senderID.DefaultSenderID = true;

		using (NLCustomsRegistry.Instance.SenderIDs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, senderIDCollection))
		{
			var entry = WrapperTestHelper.GetEntryHeaderForTest(Factory);
			entry.Declaration.JE_MessageType = DeclarationMessageType;
			MessageGeneratorTestHelper.CreateSupportingDocument(entry.EntryInstruction.SupportingDocuments, 1, "REF-123", "REF2-123", new ZDateTime(2021, 12, 31, 23, 59, 59), "REF");
			MessageGeneratorTestHelper.CreateSupportingDocument(entry.EntryInstruction.SupportingDocuments, 1, "REF-789", "REF2-456", new ZDateTime(2021, 12, 25, 21, 23, 49), "RF2");

			var messageSendingObject = new JobDeclarationMessageSendingObject(entry);
			messageSendingObject.MessageType = MessageType;
			messageSendingObject.Declaration.CustomsEntryInstructions[0].CEI_Style = CustomsEntryInstructionStyle;
			var messageSendingObjects = new[] { messageSendingObject };
			messageSendingObject.ReasonForInvalidation = ReasonOfInvalidation;

			var generator = new MessageGenerator(messageSendingObjects) as IMessageGenerator<CusEntryHeader>;
			var result = generator.Generate(entry);

			CombineAssertions($"Generation of message type {MessageType}", () =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.Queued, result.Message.EM_Status);
				AssertEquals("Message.EM_MessageSubType", ExpectedMessageSubType, result.Message.EM_MessageSubType);
				AssertEquals("Message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, result.Message.EM_ReceiveTransmit);
				AssertEquals("Message.EM_ApplicationReference", "MRN123", result.Message.EM_ApplicationReference);
				AssertSame("Linked Object", entry, result.Message.EM_LinkedObject);

				AssertXMLEquals("Message Content", ExpectedMessage, result.Message.EM_MessageText);
				AssertXMLEquals("Message Interpretation", ExpectedPrettyMessage, generator.MakePrettyForInterpretation(result.Message));
			});
		}
	}

	public void TestGenerate_AfterFullSuccess()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.SupplierDocumentaryAddress.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;
		declaration.JE_MessageType = DeclarationMessageType;

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = CustomsEntryInstructionStyle;
		entryInstruction.CEI_SubStyle = "A";
		var iSendObject = new Customs.Business.SendsMessagesToCustomsShutterUpperer(true);
		declaration.MessageInitiator = iSendObject;

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		declaration.DoMerge();

		Factory.Save();

		var entry = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().Single();
		entry.CH_PhaseStatus = InitialPhaseStatus;
		var decWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
		var messageSendingObject = decWrapper.SendingObjectsCollection[0];
		messageSendingObject.MessageType = MessageType;
		messageSendingObject.ShouldSend = true;

		var sender = new JobDeclarationMessageSender(decWrapper);
		sender.Send(iSendObject);

		AssertEquals("CH_PhaseStatus", ExpectedEntryHeaderPhaseStatus, entry.CH_PhaseStatus);
		AssertEquals("CH_Status", ExpectedEntryHeaderStatus, entry.CH_Status);
		AssertEquals("CH_EntryStatus", ExpectedEntryHeaderEntryStatus, entry.CH_EntryStatus);
	}

	protected abstract ZString MessageType { get; }

	protected virtual ZString ReasonOfInvalidation => ZString.Empty;

	protected virtual ZString CustomsEntryInstructionStyle => "H1";

	protected virtual ZString DeclarationMessageType => MessageTypeList.Codes.Import;

	protected virtual ZString InitialPhaseStatus => ZString.Empty;

	protected abstract ZString ExpectedMessage { get; }

	protected abstract ZString ExpectedPrettyMessage { get; }

	protected virtual ZString ExpectedMessageSubType => "DEC";

	protected virtual ZString ExpectedEntryHeaderPhaseStatus => ZString.Empty;

	protected virtual ZString ExpectedEntryHeaderStatus => ZString.Empty;

	protected virtual ZString ExpectedEntryHeaderEntryStatus => ZString.Empty;
}

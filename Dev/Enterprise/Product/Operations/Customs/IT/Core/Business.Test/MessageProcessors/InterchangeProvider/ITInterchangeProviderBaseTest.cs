using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class ITInterchangeProviderBaseTest : InterchangeProviderTestCase
{
	public override void TestMessagesPopulateNewInterchange()
	{
		var registryAccount = SetUpDeclarantAndRegistryAccount();

		var applicationReferenceBizObj = Factory.New<ApplicationReferenceDummyBusinessObject>();
		applicationReferenceBizObj.Subscriber = "~U1";
		applicationReferenceBizObj.Node = registryAccount.AccountNode;
		applicationReferenceBizObj.CustomsProfile = "INTCODE";

		var message = Factory.New<ITEDIMessage>();
		message.EM_LinkedObject = applicationReferenceBizObj;
		message.EM_MessageType = MessageTypeToTest;
		message.MessageNumberStrategy = new FixedMessageNumberStrategy(ZString.Empty);

		var interchanges = PackCollatedMessagesIntoInterchanges(message);
		Factory.Save();
		message.Reload();

		AssertEquals("One interchange created", 1, interchanges.Length);
		AssertNotNull("Message is linked to the interchange", message.Interchange);
		AssertEquals("Message status", ExpectedProcessedMessageStatusCode, message.EM_Status);

		CombineAssertions("Created Interchange", () =>
		{
			ITInterchangeProviderTestHelper.AssertCreatedInterchange(message, ExpectedQueuedInterchangeStatusCode);
			AssertXMLEquals("Interchange header text", ExpectedInterchangeHeaderText, message.Interchange.EI_HeaderText);
		});
	}

	public void TestCreateOneInterchangeEachMessage()
	{
		var registryAccount = SetUpDeclarantAndRegistryAccount();

		var applicationReferenceBizObj1 = Factory.New<ApplicationReferenceDummyBusinessObject>();
		applicationReferenceBizObj1.Node = registryAccount.AccountNode;
		applicationReferenceBizObj1.CustomsProfile = "INTCODE";

		var message1 = Factory.New<ITEDIMessage>();
		message1.EM_LinkedObject = applicationReferenceBizObj1;

		var applicationReferenceBizObj2 = Factory.New<ApplicationReferenceDummyBusinessObject>();
		applicationReferenceBizObj2.Node = registryAccount.AccountNode;
		var message2 = Factory.New<ITEDIMessage>();
		message2.EM_LinkedObject = applicationReferenceBizObj2;

		var interchanges = PackCollatedMessagesIntoInterchanges(message1, message2);
		CombineAssertions(() =>
		{
			AssertEquals("Number Of interchanges", 2, interchanges.Length);
			AssertNotEquals("message1 has an interchange", ZGuid.Empty, message1.EM_EI);
			AssertNotEquals("message2 has an interchange", ZGuid.Empty, message2.EM_EI);
			AssertNotEquals("message1 and message2 are linked to different interchanges (no collation)", message1.EM_EI, message2.EM_EI);
		});
	}

	public void TestMessagesPopulateNewInterchangeNoLinkedDeclaration()
	{
		var ediMessageWithoutDeclaration = Factory.New<ITEDIMessage>();
		ediMessageWithoutDeclaration.EM_MessageType = MessageTypeToTest;
		ediMessageWithoutDeclaration.MessageNumberStrategy = new FixedMessageNumberStrategy(ZString.Empty);
		AssertFailedInterchangeAndMessage(ediMessageWithoutDeclaration, "Message has no linked object.");
	}

	public void TestMessagesPopulateNewInterchangeLinkedObjectDoesNotImplementICustomsEntryApplicationReference()
	{
		var ediMessageWithoutDeclaration = Factory.New<ITEDIMessage>();
		ediMessageWithoutDeclaration.EM_LinkedObject = Factory.New<DummyBusinessObject>();
		ediMessageWithoutDeclaration.EM_MessageType = MessageTypeToTest;
		ediMessageWithoutDeclaration.MessageNumberStrategy = new FixedMessageNumberStrategy(ZString.Empty);
		AssertFailedInterchangeAndMessage(ediMessageWithoutDeclaration, "Unexpected business object linked to the message: 'CargoWise.EntityFramework.Testing.DummyBusinessObject' does not implement 'ICustomsEntryApplicationReference'.");
		AssertEquals("ErrorReporter.LastMessageReported", "Unexpected business object linked to the message: 'CargoWise.EntityFramework.Testing.DummyBusinessObject' does not implement 'ICustomsEntryApplicationReference'.", ErrorReporter.LastMessageReported);
		ErrorReporter.Clear();
	}

	public void TestMessagesPopulateNewInterchangeCannotFindRegistryAccount()
	{
		var applicationReferenceBizObj = Factory.New<ApplicationReferenceDummyBusinessObject>();
		var ediMessageWithDeclaration = Factory.New<ITEDIMessage>();
		ediMessageWithDeclaration.EM_LinkedObject = applicationReferenceBizObj;
		ediMessageWithDeclaration.EM_MessageType = MessageTypeToTest;
		ediMessageWithDeclaration.MessageNumberStrategy = new FixedMessageNumberStrategy(ZString.Empty);
		AssertFailedInterchangeAndMessage(ediMessageWithDeclaration, "Failed to find the registry account.");
	}

	protected abstract string MessageTypeToTest { get; }
	protected abstract string ExpectedInterchangeHeaderText { get; }

	protected virtual ZString ExpectedProcessedMessageStatusCode => EDIMessage.Status.Sent;

	protected virtual ZString ExpectedQueuedInterchangeStatusCode => EDIInterchange.Status.eHubQueued;

	#region Implementation

	void AssertFailedInterchangeAndMessage(ITEDIMessage ediMessage, ZString expectedNoteDataAsText)
	{
		using (ITCustomsDataRegistry.Instance.ITRecipientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ITCustoms"))
		{
			var interchanges = PackCollatedMessagesIntoInterchanges(ediMessage);
			Factory.Save();

			AssertEquals("PRE-CONDITION: number of interchanges", 1, interchanges.Length);
			var ediInterchange = (ITEDIInterchange)interchanges.Single();
			ediMessage.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Failed, ediMessage.EM_Status);
				AssertEquals("EM_EI", ediInterchange.PK, ediMessage.EM_EI);

				AssertEquals("EI_ApplicationCode", EDIInterchange.ApplicationCodes.ITCustoms, ediInterchange.EI_ApplicationCode);
				AssertNotEquals("EI_InterchangeNum", ZString.Empty, ediInterchange.EI_InterchangeNum);
				Assert("IsTransmitInterchange", ediInterchange.IsTransmitInterchange);
				AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.Failed, ediInterchange.EI_Status);
				AssertEquals("EI_InterchangeType", ediMessage.EM_MessageType, ediInterchange.EI_InterchangeType);
				AssertEquals("EI_From", GlbCompany.CurrentCompany.GC_Code, ediInterchange.EI_From);
				AssertEquals("EI_To", ITCustomsDataRegistry.Instance.ITRecipientID.Value, ediInterchange.EI_To);
				AssertEquals("EI_BodyText", ZString.Empty, ediInterchange.EI_BodyText);
				AssertEquals("EI_HeaderText", ZString.Empty, ediInterchange.EI_HeaderText);
				AssertNull("FileNameStrategy", ediInterchange.FileNameStrategy);

				var interchangeNotes = ediInterchange.Notes.GetAllNotes().Cast<StmNote>();
				AssertNotNull("Interchange notes", interchangeNotes);
				AssertEquals("Expected single note", 1, interchangeNotes.Count());
				var note = interchangeNotes.ElementAt(0);
				AssertEquals("Note ST_Description", "CargoWiseOne error", note.ST_Description);
				AssertEquals("Note ST_NoteDataAsText", expectedNoteDataAsText, note.ST_NoteDataAsText);
			});
		}
	}

	EDIInterchange[] PackCollatedMessagesIntoInterchanges(params ITEDIMessage[] messages)
	{
		var ediMessageCollection = new NonDependentEDIMessageCollection(Factory);
		ediMessageCollection.AddRange(messages);

		var interchangeProvider = GetInterchangeProvider(ediMessageCollection);
		interchangeProvider.PackCollatedMessagesIntoInterchanges();
		return interchangeProvider.Interchanges;
	}

	Account SetUpDeclarantAndRegistryAccount()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		var registryAccount = new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("INTCODE", "DEC1", "LECFER123-001")
			.Build()[0];

		return registryAccount;
	}

	#endregion
}

#region ApplicationReferenceDummyBusinessObject

class ApplicationReferenceDummyBusinessObject : DummyBusinessObject, ICustomsEntryApplicationReference
{
	public ApplicationReferenceDummyBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public ZString Node { get; set; }

	public ZString Subscriber { get; set; }

	public ZString CustomsOffice { get; set; }

	public ZString CustomsProfile { get; set; }
}

#endregion

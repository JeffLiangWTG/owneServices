using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Testing;

[TestedType(typeof(Ucc6JobDeclarationMessageSendingObjectParent))]
sealed class Ucc6JobDeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestGetSendingObjectsCollectionCore()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();

		declaration.JE_MessageType = "IMP";
		var sendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declaration);
		AssertEquals("SendingObjectsCollection Count", 1, sendingObjectParent.SendingObjectsCollection.Count);
		AssertType<ImportMessageSendingObject>("SendingObjectsCollection Single Element", sendingObjectParent.SendingObjectsCollection[0]);

		declaration.JE_MessageType = "EXP";
		sendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declaration);
		AssertEquals("SendingObjectsCollection Count", 1, sendingObjectParent.SendingObjectsCollection.Count);
		AssertType<ExportMessageSendingObject>("SendingObjectsCollection Single Element", sendingObjectParent.SendingObjectsCollection[0]);
	}

	public void TestMessageSendingObjectProperties()
	{
		var declaration = Factory.New<JobDeclaration>();
		var actualPropertyNames = new Ucc6JobDeclarationMessageSendingObjectParent(declaration).MessageSendingObjectProperties.Select(x => x.PropertyName).ToArray();

		var expectedPropertyNamesInOrder = new ZString[] { "MessageType", "VOCReason", "CancellationAndAmendmentLegislativeReference", "CombinedCustomsMessageSubType", "DeclarationType", "DeclarationDescription", "EntryStatus", "MessageStatus", "CH_BGMReference" };
		AssertArrayEqualsByElements(expectedPropertyNamesInOrder, actualPropertyNames);
	}

	public void TestBizObjValidationMessageErrors()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		var sendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declaration);
		var sendingObject = sendingObjectParent.SendingObjectsCollection[0];

		sendingObject.ShouldSend = true;
		sendingObject.MessageType = "NEW";
		AssertNotEquals("When MessageType is not CAN Message Errors must be populated", "", sendingObjectParent.BizObjValidationMessageErrors);

		sendingObject.MessageType = "CAN";
		AssertEquals("When MessageType is CAN Message Errors must NOT be populated", "", sendingObjectParent.BizObjValidationMessageErrors);
	}

	public void TestGetNewSendingObjectFactory()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var sendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParentForTest(declaration);
		AssertType<Ucc6XmlJobDeclarationMessageSendingObjectFactory>("Sending Object Factory Type", sendingObjectParent.GetNewSendingObjectFactoryExposed(entryHeader, sendingObjectParent));
	}

	public void TestHasAnySelectedCancelMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		declaration.CustomsEntryHeaders.AddNew();

		var sendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declaration);
		var sendingObject1 = sendingObjectParent.SendingObjectsCollection[0];

		sendingObject1.ShouldSend = false;
		sendingObject1.MessageType = "NEW";
		AssertEquals("When ShouldSend = 'false', MessageType = 'NEW'", false, sendingObjectParent.HasAnySelectedCancelMessage);

		sendingObject1.ShouldSend = false;
		sendingObject1.MessageType = "CAN";
		AssertEquals("When ShouldSend = 'false', MessageType = 'CAN'", false, sendingObjectParent.HasAnySelectedCancelMessage);

		sendingObject1.ShouldSend = true;
		sendingObject1.MessageType = "CAN";
		AssertEquals("When ShouldSend = 'true', MessageType = 'CAN'", true, sendingObjectParent.HasAnySelectedCancelMessage);

		sendingObject1.ShouldSend = true;
		sendingObject1.MessageType = "NEW";
		AssertEquals("When ShouldSend = 'true', MessageType = 'NEW'", false, sendingObjectParent.HasAnySelectedCancelMessage);
	}

	public void TestAdditionalWarnings_WhenNoSendingObjectAreSetAsSendable()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "H1";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var sendingObjectParent = new Ucc6JobDeclarationMessageSendingObjectParent(declaration);
		AssertEquals(nameof(sendingObjectParent.AdditionalWarnings), "", sendingObjectParent.AdditionalWarnings);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		return new Ucc6JobDeclarationMessageSendingObjectParent(declaration);
	}

	sealed class Ucc6JobDeclarationMessageSendingObjectParentForTest : Ucc6JobDeclarationMessageSendingObjectParent
	{
		public Ucc6JobDeclarationMessageSendingObjectParentForTest(JobDeclaration declaration) : base(declaration)
		{
		}

		internal JobDeclarationMessageSendingObjectAbstractFactory GetNewSendingObjectFactoryExposed(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent sendingObjectParent) => GetNewSendingObjectFactory(entryHeader, sendingObjectParent);
	}
}

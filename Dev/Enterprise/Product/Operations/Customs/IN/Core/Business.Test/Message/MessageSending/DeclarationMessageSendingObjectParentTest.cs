using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DeclarationMessageSendingObjectParent))]
sealed class DeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestMessageSendingObjectProperties()
	{
		var messageSendingObjectParent = CreateSendingObjectParentForTest();
		var messageSendingObjectProperties = messageSendingObjectParent.MessageSendingObjectProperties;

		CombineAssertions(() =>
		{
			AssertEquals("Columns Count", 8, messageSendingObjectProperties.Count());

			AssertMessageSendingObjectProperty(messageSendingObjectProperties, DeclarationMessageSendingObject.Schema.MessageType, 100, true, true);
			AssertMessageSendingObjectProperty(messageSendingObjectProperties, DeclarationMessageSendingObject.Schema.LocalReferenceNumber, 150, true, true);
			AssertMessageSendingObjectProperty(messageSendingObjectProperties, DeclarationMessageSendingObject.Schema.LocalReferenceNumberDate, 150, false, true);
			AssertMessageSendingObjectProperty(messageSendingObjectProperties, DeclarationMessageSendingObject.Schema.ShippingBillNumber, 150, false, true);
			AssertMessageSendingObjectProperty(messageSendingObjectProperties, DeclarationMessageSendingObject.Schema.ShippingBillDate, 150, false, true);
			AssertMessageSendingObjectProperty(messageSendingObjectProperties, DeclarationMessageSendingObject.Schema.Description, 150, false, false);
			AssertMessageSendingObjectProperty(messageSendingObjectProperties, DeclarationMessageSendingObject.Schema.DeclarationType, 150, false, false);
			AssertMessageSendingObjectProperty(messageSendingObjectProperties, DeclarationMessageSendingObject.Schema.CustomsHouse, 150, false, true);
		});
	}

	public void TestSendAndSaveMessages()
	{
		CombineAssertions(() =>
		{
			var messageSendingObjectParent = CreateSendingObjectParentForTest();
			messageSendingObjectParent.ParentDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			AssertEquals("should not send for Import", 0, messageSendingObjectParent.SendAndSaveMessages(MessageSendingContext.EMAIL).Length);

			messageSendingObjectParent.ParentDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			messageSendingObjectParent = CreateSendingObjectParentForTest();
			AssertEquals("should send for Export", 0, messageSendingObjectParent.SendAndSaveMessages(MessageSendingContext.EMAIL).Length);

			messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals("should send for Export", 1, messageSendingObjectParent.SendAndSaveMessages(MessageSendingContext.EMAIL).Length);

			messageSendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;
			AssertEquals("should send for Export", 2, messageSendingObjectParent.SendAndSaveMessages(MessageSendingContext.EMAIL).Length);
		});
	}

	public void TestSendAndSaveMessages_RollbackOnSaveFailed()
	{
		CombineAssertions(() =>
		{
			var messageSendingObjectParent = CreateSendingObjectParentForTest();
			messageSendingObjectParent.ParentDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			messageSendingObjectParent.ParentDeclaration.JE_OH_Importer = ZGuid.NewZGuid();

			messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			messageSendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;

			AssertEquals("Message deleted when saving failed", 0, messageSendingObjectParent.SendAndSaveMessages(MessageSendingContext.EMAIL).Length);
			AssertEquals("Message deleted when saving failed", 0, messageSendingObjectParent.ParentDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Sum(x => x.Messages.Count));
		});
	}

	public void TestSendAndSaveMessages_GoodsRegistration()
	{
		CombineAssertions(() =>
		{
			var messageSendingObjectParent = CreateSendingObjectParentForTest();
			messageSendingObjectParent.ParentDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			messageSendingObjectParent = CreateSendingObjectParentForTest();
			AssertEquals("should send for Export", 0, messageSendingObjectParent.SendAndSaveMessages(MessageSendingContext.EMAIL).Length);

			messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
			messageSendingObjectParent.SendingObjectsCollection[0].MessageType = DeclarationMessageTypeList.Codes.GoodsRegistration;
			var messages = messageSendingObjectParent.SendAndSaveMessages(MessageSendingContext.EMAIL);
			AssertEquals("should send for Export, one SGR", 1, messages.Length);
			AssertEquals("message sub type should be SGR", EDIMessageSubTypeList.Codes.GoodsRegistration, messages[0].EM_MessageSubType);

			messageSendingObjectParent.SendingObjectsCollection[1].ShouldSend = true;
			messageSendingObjectParent.SendingObjectsCollection[1].MessageType = DeclarationMessageTypeList.Codes.Fresh;
			messages = messageSendingObjectParent.SendAndSaveMessages(MessageSendingContext.EMAIL);
			AssertEquals("should send for Export, one SGR, one SBF", 2, messages.Length);
			AssertNotNull("GoodsRegistration", messages.First(x => x.EM_MessageSubType == EDIMessageSubTypeList.Codes.GoodsRegistration));
			AssertNotNull("ShippingBillFresh", messages.First(x => x.EM_MessageSubType == EDIMessageSubTypeList.Codes.ShippingBillFresh));
		});
	}

	void AssertMessageSendingObjectProperty(IEnumerable<MessageSendingObjectProperty> messageSendingObjectProperties, string propertyName, int expectedColumnWidth, bool expectedMandatory, bool expectedVisible)
	{
		var property = messageSendingObjectProperties.Single(i => i.PropertyName == propertyName);
		AssertNotNull($"Should contain {propertyName}", property);
		AssertEquals($"{propertyName} ColumnWidth", expectedColumnWidth, property.ColumnWidth);
		AssertEquals($"{propertyName} IsMandatory", expectedMandatory, property.IsMandatory);
		AssertEquals($"{propertyName} IsVisible", expectedVisible, property.IsVisible);
	}

	public void TestValidateBeforeSend()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.ActiveEntryHeaders.AddNew();
		IMessageSendingObjectParent sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
		AssertEquals("Please select Customs House to send the message electronically to ICEGate.", sendingObjectParent.ValidateBeforeSend());
	}

	public void TestAdditionalWarnings()
	{
		var warning = "The Net Weight should not be greater than the Gross Weight.";

		var declaration = Factory.New<JobDeclaration>();
		AddEntryWithWarning(declaration);
		AddEntryWithWarning(declaration);

		var sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
		AssertEquals(ZString.Empty, sendingObjectParent.AdditionalWarnings.ToString());

		var sendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<DeclarationMessageSendingObject>();
		sendingObjects.First().ShouldSend = true;
		AssertContains(warning, sendingObjectParent.AdditionalWarnings);

		void AddEntryWithWarning(JobDeclaration declaration)
		{
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.InvoiceLines.Add(invoiceLine);

			invoice.JZ_Weight = 10m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			invoice.JZ_NetWeight = 11m;
			invoice.JZ_NetWeightUQ = Core.Constants.Weight.Kilograms;
		}
	}

	protected override BusinessObject GetNewBusinessObject() => CreateSendingObjectParentForTest();

	DeclarationMessageSendingObjectParent CreateSendingObjectParentForTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.ActiveEntryHeaders.AddNew();
		declaration.ActiveEntryHeaders.AddNew();
		declaration.JE_CustomsOffice = "INNSA1";
		var sendingObjectParent = new DeclarationMessageSendingObjectParent(declaration);
		return sendingObjectParent;
	}
}

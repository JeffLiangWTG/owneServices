using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(ManifestMessageSendingObject))]
sealed class ManifestMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When header is null", () => new ManifestMessageSendingObject(null));
		AssertNoExceptionThrown("When header is not null", () => new ManifestMessageSendingObject(ManifestHeader));
	}

	public void TestDefaultValues()
	{
		AssertEquals("ShouldSend should be true", expected: ZBool.True, MessageSendingObject.ShouldSend);
	}

	public void TestMessageTypeReadOnly()
	{
		ManifestHeader.Action = ManifestMessageTypeList.Codes.Delete;
		var sendingObject = new ManifestMessageSendingObject(ManifestHeader);
		CombineAssertions(() =>
		{
			AssertEquals("Should be read-only for D", true, sendingObject.MessageTypeInfo.ReadOnly);

			ManifestHeader.Action = ManifestMessageTypeList.Codes.Fresh;
			AssertEquals("Should not be read-only for F", false, sendingObject.MessageTypeInfo.ReadOnly);
		});
	}

	public void TestMessageTypeCaption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(MessageSendingObject.MessageTypeInfo);
		AssertEquals("MessageType Caption", "Message Type", resourceStringData.Caption);
	}

	public void TestMessageTypeDescriptionCaption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(MessageSendingObject.MessageTypeDescriptionInfo);
		AssertEquals("MessageTypeDescription Caption", "Description", resourceStringData.Caption);
	}

	public void TestMessageTypeDescription()
	{
		MessageSendingObject.Parent.Messages.AddNew();
		CombineAssertions(() =>
		{
			MessageSendingObject.MessageType = ZString.Empty;
			AssertEquals("Empty MessageType", ZString.Empty, MessageSendingObject.MessageTypeDescription);

			MessageSendingObject.MessageType = "F";
			AssertEquals("MessageType F", "Fresh", MessageSendingObject.MessageTypeDescription);

			MessageSendingObject.MessageType = "D";
			AssertEquals("MessageType D", "Delete", MessageSendingObject.MessageTypeDescription);

			MessageSendingObject.MessageType = "A";
			AssertEquals("MessageType A", "Amendment", MessageSendingObject.MessageTypeDescription);

			MessageSendingObject.MessageType = "XYZ";
			AssertEquals("MessageType invalid", ZString.Empty, MessageSendingObject.MessageTypeDescription);
		});
	}

	public void TestMessageTypeDescriptionReadonly()
	{
		AssertEquals("MessageTypeDescription is readonly", expected: true, MessageSendingObject.MessageTypeDescriptionInfo.ReadOnly);
	}

	public void TestMessageTypeText()
	{
		MessageSendingObject.Parent.Messages.AddNew();
		CombineAssertions(() =>
		{
			MessageSendingObject.MessageType = ZString.Empty;
			AssertEquals("MessageType Empty", ZString.Empty, MessageSendingObject.MessageTypeText);

			MessageSendingObject.MessageType = "F";
			AssertEquals("MessageType F", "Submission of the Fresh CGM Manifest Message to ICEGate.", MessageSendingObject.MessageTypeText);

			MessageSendingObject.MessageType = "D";
			AssertEquals("MessageType D", "Delete requisition for registered CGM Manifest Message to ICEGate.", MessageSendingObject.MessageTypeText);

			MessageSendingObject.MessageType = "A";
			AssertEquals("MessageType A", "Amendment requisition for registered CGM Manifest Message to ICEGate.", MessageSendingObject.MessageTypeText);

			MessageSendingObject.MessageType = "XYZ";
			AssertEquals("MessageType Invalid", ZString.Empty, MessageSendingObject.MessageTypeText);
		});
	}

	public void TestBillNumber()
	{
		var masterBill = ManifestHeader.MasterBill;
		masterBill.ABL_BillNumber = "XYZ456";

		AssertEquals("BillNumber is populated from MasterBill", "XYZ456", MessageSendingObject.BillNumber);
	}

	public void TestBillNumberReadonly()
	{
		AssertEquals("BillNumber is readonly", expected: true, MessageSendingObject.BillNumberInfo.ReadOnly);
	}

	public void TestLookups()
	{
		AssertType<ManifestMessageSendingObjectLookups>(MessageSendingObject.Lookups);
	}

	public void TestMessageAttachee()
	{
		AssertEquals(ManifestHeader, ((IMessageSendingObject)MessageSendingObject).MessageAttachee);
	}

	public void TestMessageTypeFreshOnFirstMessage()
	{
		AssertEquals("MessageType Fresh On First Message", expected: ManifestMessageTypeList.Codes.Fresh, MessageSendingObject.MessageType);
	}

	public void TestMessageTypeMultipleMessages()
	{
		ManifestHeader.Messages.AddNew();
		AssertEquals("MessageType Empty value", expected: ZString.Empty, MessageSendingObject.MessageType);
	}

	public void TestDefaultMessageType()
	{
		AssertEquals("MessageType", ManifestMessageTypeList.Codes.Fresh, new ManifestMessageSendingObject(ManifestHeader).MessageType);

		ManifestHeader.Action = ManifestMessageTypeList.Codes.Delete;
		AssertEquals("MessageType for Delete", ManifestMessageTypeList.Codes.Delete, new ManifestMessageSendingObject(ManifestHeader).MessageType);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return new ManifestMessageSendingObject(Factory.New<CGMAsycudaManifestHeader>());
	}

	CGMAsycudaManifestHeader ManifestHeader => manifestHeader ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader manifestHeader;

	ManifestMessageSendingObject MessageSendingObject => messageSendingObject ??= new ManifestMessageSendingObject(ManifestHeader);
	ManifestMessageSendingObject messageSendingObject;
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageMessageSendingObject))]
sealed class TemporaryStorageMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestMessageSendingDefaultValues()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Header AMA_MessageType is empty: default DeclarationType", string.Empty, sendingObject.DeclarationType);
			AssertEquals("Header AMA_MessageType is empty: default MessageType", string.Empty, sendingObject.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Header AMA_MessageType is 'TF': default DeclarationType", PNTSMessageTypeList.Codes.Transfer, sendingObject.DeclarationType);
			AssertEquals("Header AMA_MessageType is 'TF': default MessageType", PNTSEntryTypeList.Codes.TransferNotification, sendingObject.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Header AMA_MessageType is 'DC': default DeclarationType", PNTSMessageTypeList.Codes.Deconsolidation, sendingObject.DeclarationType);
			AssertEquals("Header AMA_MessageType is 'DC': default MessageType", PNTSEntryTypeList.Codes.DeconsolidationNotification, sendingObject.MessageType);

			header.AMA_MessageType = "XX";
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Header AMA_MessageType is unknown/invalid (XX): default DeclarationType", "XX", sendingObject.DeclarationType);
			AssertEquals("Header AMA_MessageType is unknown/invalid (XX): default MessageType", string.Empty, sendingObject.MessageType);
		});
	}

	public void TestMessagDeclarationType()
	{
		CombineAssertions(() =>
		{
			AssertEquals("DeclarationType readonly", true, sendingObject.DeclarationTypeInfo.ReadOnly);
			AssertEquals("property DeclarationType_ReadOnly", true, sendingObject.GetType().GetProperty("DeclarationType_ReadOnly", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(sendingObject));
			header.AMA_MessageType = "XX";
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("DeclarationType links to Message Mode on Header", "XX", sendingObject.DeclarationType);
		});
	}

	public void TestIsTestDeclaration()
	{
		CombineAssertions(() =>
		{
			AssertEquals("IsTestDeclaration readonly", false, sendingObject.IsTestDeclarationInfo.ReadOnly);
			AssertEquals("IsTestDeclaration Caption", "Test?", DataBoundResourceStrings.GetDataForProperty(sendingObject.IsTestDeclarationInfo).Caption);
		});
	}

	public void TestEntryStatus()
	{
		CombineAssertions(() =>
		{
			AssertEquals("EntryStatus readonly", true, sendingObject.EntryStatusInfo.ReadOnly);
			header.CustomsStatus = "XX";
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("EntryStatus links to Customs Status on Header", "XX", sendingObject.EntryStatus);
		});
	}

	public void TestReferenceNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("ReferenceNumber readonly", true, sendingObject.ReferenceNumberInfo.ReadOnly);
			AssertEquals("ReferenceNumber Caption", "Reference Number", DataBoundResourceStrings.GetDataForProperty(sendingObject.ReferenceNumberInfo).Caption);
			header.LRN = "LRN123";
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Reference Number links to LRN on Header", "LRN123", sendingObject.ReferenceNumber);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => sendingObject;

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<TemporaryStorageHeader>();
		sendingObject = new TemporaryStorageMessageSendingObject(header);
	}

	TemporaryStorageHeader header;
	TemporaryStorageMessageSendingObject sendingObject;
}

using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(ManifestMessageSendingObjectParent))]
sealed class ManifestMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new ManifestMessageSendingObjectParent(null));
	}

	public void TestTopLevelBusinessObject()
	{
		AssertType<CGMAsycudaManifestHeader>(MessageSendingObjectParent.TopLevelBusinessObject);
	}

	public void TestSecurityCheckpointToSendWithMessageError()
	{
		AssertEquals(Env.Security.GlobalManifestSendWithMessageErrors, MessageSendingObjectParent.SecurityCheckpointToSendWithMessageError);
	}

	public void TestSendingObjectsCollection()
	{
		AssertType<ManifestMessageSendingObjectCollection>(MessageSendingObjectParent.SendingObjectsCollection);
	}

	public void TestSendAndSaveMessages_CGM_Road()
	{
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("No message sender found for {CGM, Road}", expected: 0, MessageSendingObjectParent.SendAndSaveMessages(MessageSendingContext.EMAIL).Length);
			AssertEquals("No message created and saved", 0, Header.Messages.Count);
		});
	}

	public void TestSendAndSaveMessages_CGM_Sea()
	{
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("Message sender found for {CGM, Sea}", expected: 1, MessageSendingObjectParent.SendAndSaveMessages(MessageSendingContext.EMAIL).Length);

			var message = Header.Messages.Single() as EDIMessage;
			AssertEquals("Message created and saved", expected: true, message.IsInDatabase);
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.ConsolGeneralManifest, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.SeaCgm, message.EM_MessageSubType);
		});
	}

	public void TestSendAndSaveMessages_CGM_Air()
	{
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("Message sender found for {CGM, Air}", expected: 1, MessageSendingObjectParent.SendAndSaveMessages(MessageSendingContext.EMAIL).Length);

			var message = Header.Messages.Single() as EDIMessage;
			AssertEquals("Message created and saved", expected: true, message.IsInDatabase);
			AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.ConsolGeneralManifest, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.AirCgm, message.EM_MessageSubType);
		});
	}

	public void TestSendAndSaveMessages_ExceptionWhileSaving()
	{
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Header.AMA_OA_Carrier = ZGuid.Invalid;
			AssertEquals("Message sender found for {CGM, Sea}", expected: 0, MessageSendingObjectParent.SendAndSaveMessages(MessageSendingContext.EMAIL).Length);

			AssertEquals("Error while saving", "Cannot Save...", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("No message for Manifest", 0, Header.Messages.Count);
		});
	}

	public void TestValidateBeforeSend()
	{
		CombineAssertions(() =>
		{
			AssertNullOrEmpty(MessageSendingObjectParent.ValidateBeforeSend());
			Header.AMA_CustomsOffice = ZString.Empty;
			AssertEquals("Please enter a Customs Office to proceed with CGM message sending.", MessageSendingObjectParent.ValidateBeforeSend());
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var header = Factory.NewWithValidTestData<CGMAsycudaManifestHeader>();
		header.AMA_CustomsOffice = "TestOffice";
		return new ManifestMessageSendingObjectParent(header);
	}

	CGMAsycudaManifestHeader Header => MessageSendingObjectParent.ManifestHeader as CGMAsycudaManifestHeader;

	ManifestMessageSendingObjectParent MessageSendingObjectParent => messageSendingObjectParent ??= GetNewBusinessObject() as ManifestMessageSendingObjectParent;
	ManifestMessageSendingObjectParent messageSendingObjectParent;
}

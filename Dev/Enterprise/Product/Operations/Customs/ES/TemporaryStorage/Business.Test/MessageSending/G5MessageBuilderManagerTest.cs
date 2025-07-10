using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;

public class G5MessageBuilderManagerTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown("Constructor Throws Exception if objectToSend is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "objectToSend"), () => new G5MessageBuilderManager(null, certificate));

			AssertExceptionThrown("Constructor Throws Exception if Header is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "header"), () => new G5MessageBuilderManager(new G5TemporaryStorageMessageSendingObject(null), certificate));

			var sendingObject = new G5TemporaryStorageMessageSendingObject(header);
			AssertExceptionThrown("Constructor Throws Exception if certificate is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "certificate"), () => new G5MessageBuilderManager(sendingObject, null));
		});
	}

	[TestDate(2020, 1, 9, 15, 13, 23, 456)]
	public void TestGetCompleteMessageG5V1Cancellation_WithSendingObject()
	{
		using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			header.AMA_JobReference = "TSD00001";
			header.TrainingEntry = true;

			var sendingObject = new G5TemporaryStorageMessageSendingObject(header);
			sendingObject.MessageType = DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation;

			var messageBuilderManager = new G5MessageBuilderManager(sendingObject, certificate);
			var messageBuilder = messageBuilderManager.NewMessageBuilder();

			CombineAssertions(() =>
			{
				AssertEquals("messageBuilder.MessageType", DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation, messageBuilder.MessageType);
				AssertEquals("messageBuilder.MessageSubType", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, messageBuilder.MessageSubType);
				AssertEquals("messageBuilder.Provider.IsTest", true, messageBuilder.Provider.IsTest);
				AssertEquals("messageBuilder.Provider.CertificateName", certificate.CertificateName, messageBuilder.Provider.CertificateName);
				AssertEquals("messageBuilder.Provider.CertificatePK", certificate.CertificatePK, messageBuilder.Provider.CertificatePK);
				AssertEquals("messageBuilder.Provider.BusinessObjectReference", header.AMA_JobReference, messageBuilder.Provider.BusinessObjectReference);
			});
		}
	}

	public void TestGetCompleteMessageG5V1ExpAmendment_WithSendingObject()
	{
		using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			header.AMA_JobReference = "TSD00001";
			header.TrainingEntry = true;

			var sendingObject = new G5TemporaryStorageMessageSendingObject(header);
			sendingObject.MessageType = DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment;

			var messageBuilderManager = new G5MessageBuilderManager(sendingObject, certificate);
			var messageBuilder = messageBuilderManager.NewMessageBuilder();

			CombineAssertions(() =>
			{
				AssertEquals("messageBuilder.MessageType", DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment, messageBuilder.MessageType);
				AssertEquals("messageBuilder.MessageSubType", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, messageBuilder.MessageSubType);
				AssertEquals("messageBuilder.Provider.IsTest", true, messageBuilder.Provider.IsTest);
				AssertEquals("messageBuilder.Provider.CertificateName", certificate.CertificateName, messageBuilder.Provider.CertificateName);
				AssertEquals("messageBuilder.Provider.CertificatePK", certificate.CertificatePK, messageBuilder.Provider.CertificatePK);
				AssertEquals("messageBuilder.Provider.BusinessObjectReference", header.AMA_JobReference, messageBuilder.Provider.BusinessObjectReference);
			});
		}
	}

	public void TestGetCompleteMessageG5V1Reception_WithSendingObject()
	{
		using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			header.AMA_JobReference = "TSD00001";
			header.TrainingEntry = true;

			var sendingObject = new G5TemporaryStorageMessageSendingObject(header);
			sendingObject.MessageType = DeclarationMessageTypeList.Codes.G5v1Reception;

			var messageBuilderManager = new G5MessageBuilderManager(sendingObject, certificate);
			var messageBuilder = messageBuilderManager.NewMessageBuilder();

			CombineAssertions(() =>
			{
				AssertEquals("messageBuilder.MessageType", DeclarationMessageTypeList.Codes.G5v1Reception, messageBuilder.MessageType);
				AssertEquals("messageBuilder.MessageSubType", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, messageBuilder.MessageSubType);
				AssertEquals("messageBuilder.Provider.IsTest", true, messageBuilder.Provider.IsTest);
				AssertEquals("messageBuilder.Provider.CertificateName", certificate.CertificateName, messageBuilder.Provider.CertificateName);
				AssertEquals("messageBuilder.Provider.CertificatePK", certificate.CertificatePK, messageBuilder.Provider.CertificatePK);
				AssertEquals("messageBuilder.Provider.BusinessObjectReference", header.AMA_JobReference, messageBuilder.Provider.BusinessObjectReference);
			});
		}
	}

	public void TestNewMessageBuilderG5V1Cancellation()
	{
		AssertMessageBuilderTypeWithSendingObject<ExpCancelG5MessageBuilder>(G5MessageTypeCodeList.Codes.G5v1Expedition, DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation);
	}

	public void TestNewMessageBuilderG5V1Expedition()
	{
		AssertMessageBuilderTypeWithSendingObject<ExpeditionG5MessageBuilder>(G5MessageTypeCodeList.Codes.G5v1Expedition, DeclarationMessageTypeList.Codes.G5v1Expedition);
	}

	public void TestNewMessageBuilderG5V1ExpAmendment()
	{
		AssertMessageBuilderTypeWithSendingObject<ExpAmendmentG5MessageBuilder>(G5MessageTypeCodeList.Codes.G5v1Expedition, DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment);
	}

	public void TestNewMessageBuilderG5V1Reception()
	{
		AssertMessageBuilderTypeWithSendingObject<ReceptionG5MessageBuilder>(G5MessageTypeCodeList.Codes.G5v1Expedition, DeclarationMessageTypeList.Codes.G5v1Reception);
	}

	public void TestNewMessageBuilderNotImplementedException_WithSendingObject()
	{
		var sendingObject = new G5TemporaryStorageMessageSendingObject(header);
		sendingObject.MessageType = "AAA";
		var messageBuilderManager = new G5MessageBuilderManager(sendingObject, certificate);
		AssertExceptionThrown<NotImplementedException>("NewMessageBuilder throws an exception for any other DeclarationMessageType not yet supported", () => messageBuilderManager.NewMessageBuilder());
	}

	[TestDate(2020, 1, 9, 15, 13, 23, 456)]
	public void TestGetCompleteMessageG5V1Expedition_WithSendingObject()
	{
		using (RegistryTemporarySetterHelper.SetMailboxEmailAddress(string.Empty))
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			header.AMA_JobReference = "TSD00001";
			header.TrainingEntry = true;

			var sendingObject = new G5TemporaryStorageMessageSendingObject(header);
			sendingObject.MessageType = DeclarationMessageTypeList.Codes.G5v1Expedition;

			var messageBuilderManager = new G5MessageBuilderManager(sendingObject, certificate);
			var messageBuilder = messageBuilderManager.NewMessageBuilder();

			CombineAssertions(() =>
			{
				AssertEquals("messageBuilder.MessageType", DeclarationMessageTypeList.Codes.G5v1Expedition, messageBuilder.MessageType);
				AssertEquals("messageBuilder.MessageSubType", DeclarationMessageSubTypeList.Codes.OriginalDeclaration, messageBuilder.MessageSubType);
				AssertEquals("messageBuilder.Provider.IsTest", true, messageBuilder.Provider.IsTest);
				AssertEquals("messageBuilder.Provider.CertificateName", certificate.CertificateName, messageBuilder.Provider.CertificateName);
				AssertEquals("messageBuilder.Provider.CertificatePK", certificate.CertificatePK, messageBuilder.Provider.CertificatePK);
				AssertEquals("messageBuilder.Provider.BusinessObjectReference", header.AMA_JobReference, messageBuilder.Provider.BusinessObjectReference);
			});
		}
	}

	T AssertMessageBuilderTypeWithSendingObject<T>(ZString messageMode, ZString messageType)
		where T : IMessageBuilderBase
	{
		header.AMA_MessageType = messageMode;

		var sendingObject = new G5TemporaryStorageMessageSendingObject(header);
		sendingObject.MessageType = messageType;

		var messageBuilderManager = new G5MessageBuilderManager(sendingObject, certificate);
		var messageBuilder = messageBuilderManager.NewMessageBuilder();
		AssertType<T>("NewMessageBuilder is " + messageType + " type", messageBuilder);
		return (T)messageBuilder;
	}

	protected override void SetUp()
	{
		base.SetUp();

		var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
		staff = staffWithCertificateHelperTest.Staff;
		certificate = staffWithCertificateHelperTest.Certificate;

		header = Factory.New<TemporaryStorageHeader>();
		header.AMA_GS_NKCustomsAgent = staff.GS_Code;
	}
	TemporaryStorageHeader header;
	CertificateProviderTestClass certificate;
	GlbStaff staff;
}

using CargoWise.Customs.IT.MessageContracts.TemporaryStorage;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageMessageSendingObject))]
sealed class TemporaryStorageMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestJobReferenceNumberCaptions()
	{
		AssertEntity<TemporaryStorageMessageSendingObject>()
			.HasProperty(x => x.JobReferenceNumber)
			.WithCaption("Job Reference Number")
			.WithShortCaption("Ref. No.")
			.WithMediumCaption("Job Ref. No.");
	}

	public void TestShouldSendReadOnly()
	{
		var sendingObject = (TemporaryStorageMessageSendingObject)GetNewBusinessObject();
		AssertEquals("ShouldSend ReadOnly", true, sendingObject.ShouldSendInfo.ReadOnly);
	}

	public void TestDeclarationTypeReadOnly()
	{
		var sendingObject = (TemporaryStorageMessageSendingObject)GetNewBusinessObject();
		AssertEquals("DeclarationType ReadOnly", true, sendingObject.DeclarationTypeInfo.ReadOnly);
	}

	public void TestDefaultDeclarationType()
	{
		var sendingObject = (TemporaryStorageMessageSendingObject)GetNewBusinessObject();
		AssertEquals("Default DeclarationType", "G4", sendingObject.DeclarationType);
	}

	public void TestGetServiceIdForNewDeclarationMessages()
	{
		var sendingObject = (TemporaryStorageMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "NEW";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetServiceId", "submission", valuesProvider.GetServiceId());
	}

	public void TestGetServiceIdForAmendmentMessages()
	{
		var sendingObject = (TemporaryStorageMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "AMD";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetServiceId", "submission", valuesProvider.GetServiceId());
	}

	public void TestCryptokiCertificate()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var currentStaff = GlbStaff.CurrentUser;
			var staffWrapper = IT.Business.GlbStaffWrapper.Get(currentStaff);
			var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
			cryptokiCertificate.GP_Name = ChipsetList.Codes.Bit4id;
			Factory.Save();

			var sendingObject = (TemporaryStorageMessageSendingObject)GetNewBusinessObject();
			var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
			AssertNotNull("Cryptoki Certificate", valuesProvider.CryptokiCertificate);
			AssertEquals("GP_Name", "BIT4ID", valuesProvider.CryptokiCertificate.GP_Name);
		}
	}

	public void TestMauCertificate()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var companyWrapper = IT.Business.GlbCompanyWrapper.Get(currentCompany);
			var mauPassword = companyWrapper.PasswordCollection.AddNew();
			mauPassword.GP_UserID = "1234";
			currentCompany.Factory.Save();

			header.AMA_CustomsProfile = "1234";

			var sendingObject = (TemporaryStorageMessageSendingObject)GetNewBusinessObject();
			var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
			AssertNotNull("MAU Certificate", valuesProvider.MauCertificate);
			AssertEquals("GP_UserID", "1234", valuesProvider.MauCertificate.GP_UserID);
		}
	}

	public void TestServiceTypeNamespace()
	{
		var sendingObject = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		AssertEquals("ServiceTypeNamespace", "http://temporarystorageservice.domest.sogei.it", sendingObject.ServiceTypeNamespace);
	}

	public void TestServiceTypePrefix()
	{
		var sendingObject = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		AssertEquals("ServiceTypePrefix", "tns", sendingObject.ServiceTypePrefix);
	}

	public void TestXmlSigner()
	{
		var sendingObject = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		AssertType<AidaXmlSigner>("XmlSigner Type", sendingObject.XmlSigner);
	}

	public void TestGetMessageType()
	{
		var sendingObject = (TemporaryStorageMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "NEW";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetMessageType", "NEW", valuesProvider.GetMessageType());

		sendingObject.MessageType = "AMD";
		AssertEquals("GetMessageType for AMD", "AMD", valuesProvider.GetMessageType());
	}

	public void TestValidation()
	{
		var sendingObject = (TemporaryStorageMessageSendingObject)GetNewBusinessObject();
		AssertType<TemporaryStorageMessageSendingObjectValidation>("Validation Type", sendingObject.Validation);
	}

	public void TestIEntryMessageSendingObjectInfo()
	{
		header.AMA_JobReference = "TS001";
		header.AMA_MessageStatus = "SNT";
		header.CustomsStatus = "TPA";
		CombineAssertions(() =>
		{
			var entryMessageSendingObjectInfo = (IEntryMessageSendingObjectInfo)GetNewBusinessObject();
			AssertEquals("EntryStatusAllowsSending", false, entryMessageSendingObjectInfo.EntryStatusAllowsSending);
			AssertEquals("EntryReference", "TS001", entryMessageSendingObjectInfo.EntryReference);
			AssertEquals("EntryMessageStatus", "SNT", entryMessageSendingObjectInfo.EntryMessageStatus);
			AssertEquals("EntryCustomsStatus", "TPA", entryMessageSendingObjectInfo.EntryCustomsStatus);
		});
	}

	public void TestGetApplicationReference()
	{
		var sendingObject = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		AssertEquals("GetApplicationReference", "TST", sendingObject.GetApplicationReference());
	}

	public void TestGetMessageBuilder()
	{
		var sendingObject = new TemporaryStorageMessageSendingObjectForTest(header);
		sendingObject.DeclarationType = "G4";
		sendingObject.MessageType = EDIMessageTypeList.Codes.NewDeclaration;
		AssertType<TemporaneaCustodiaG4MessageBuilder>("DeclarationType = G4, MessageType = NEW", sendingObject.GetMessageBuilder_Exposed());

		sendingObject.MessageType = EDIMessageTypeList.Codes.Amendment;
		AssertType<TemporaneaCustodiaG4AmendmentMessageBuilder>("DeclarationType = G4, MessageType = AMD", sendingObject.GetMessageBuilder_Exposed());

		sendingObject.DeclarationType = "XX";
		AssertNull("DeclarationType = XX", sendingObject.GetMessageBuilder_Exposed());
	}

	public void TestHasValidAutomaticSignature()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var sendingObject = new TemporaryStorageMessageSendingObjectForTest(header);
			var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
			AssertEquals("When user does not have any Automatic Signature Password, HasValidAutomaticSignature",
				false,
				valuesProvider.HasValidAutomaticSignature);

			var currentStaff = GlbStaff.CurrentUser;
			var staffWrapper = IT.Business.GlbStaffWrapper.Get(currentStaff);
			var automaticSignature = staffWrapper.AutomaticSignaturePasswordCollection.AddNew();

			automaticSignature.IsConfigurationActive = true;
			AssertEquals("When user has an active Automatic Signature Password, HasValidAutomaticSignature",
				true,
				valuesProvider.HasValidAutomaticSignature);

			automaticSignature.IsConfigurationActive = false;
			AssertEquals("When user has an Automatic Signature Password but it is not Active, HasValidAutomaticSignature",
				false,
				valuesProvider.HasValidAutomaticSignature);
		}
	}

	public void TestLocalReferenceNumberGenerator()
	{
		var sendingObject = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		AssertType<TemporaryStorageLocalReferenceNumberGenerator>("LocalReferenceNumberGenerator Type", sendingObject.LocalReferenceNumberGenerator);
	}

	protected override BusinessObject GetNewBusinessObject() => new TemporaryStorageMessageSendingObject(header);

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<TemporaryStorageHeader>();
	}

	TemporaryStorageHeader header;

	sealed class TemporaryStorageMessageSendingObjectForTest : TemporaryStorageMessageSendingObject
	{
		public TemporaryStorageMessageSendingObjectForTest(TemporaryStorageHeader header) : base(header)
		{
		}

		internal IXmlMessageBuilder GetMessageBuilder_Exposed() => GetMessageBuilder();
	}
}

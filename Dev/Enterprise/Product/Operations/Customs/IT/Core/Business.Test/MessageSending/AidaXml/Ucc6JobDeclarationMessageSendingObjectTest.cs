using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Testing;

abstract class Ucc6JobDeclarationMessageSendingObjectTest : JobDeclarationMessageSendingObjectTest
{
	protected override Type ExpectedValidationType => typeof(Ucc6JobDeclarationMessageSendingObjectValidation);

	public void TestDefaultMessageType()
	{
		var sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();
		AssertEquals("By default, Message Type", "NEW", sendingObject.MessageType);

		EntryHeader.CH_EntryStatus = "AMG";
		sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();
		AssertEquals("When EntryHeader is in Amending Status, Message Type", "AMD", sendingObject.MessageType);
	}

	public override void TestCombinedCustomsMessageSubType()
	{
		var sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();

		EntryInstruction.CEI_Style = "AA";
		AssertEquals("CombinedCustomsMessageSubType", "AA", sendingObject.CombinedCustomsMessageSubType);
	}

	public abstract void TestCustomsMessageText();

	public abstract void TestCustomsMessageTextForCancellationMessages();

	public abstract void TestLookups();

	public void TestGetMessageSubTypeForCancellationMessages()
	{
		var sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "CAN";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetSubType", "CAN", valuesProvider.GetSubType());
	}

	public void TestGetServiceIdForCancellationMessages()
	{
		var sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "CAN";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetServiceId", "annullaDichiarazione", valuesProvider.GetServiceId());
	}

	public void TestGetServiceIdForNewDeclarationMessages()
	{
		var sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "NEW";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetServiceId", "invioDichiarazione", valuesProvider.GetServiceId());
	}

	public void TestGetServiceIdForAmendmentDeclarationMessages()
	{
		var sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "AMD";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetServiceId", "rettificaDichiarazione", valuesProvider.GetServiceId());
	}

	public void TestCryptokiCertificate()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var currentStaff = GlbStaff.CurrentUser;
			var staffWrapper = GlbStaffWrapper.Get(currentStaff);
			var cryptokiCertificate = staffWrapper.CryptokiCertificateCollection.AddNew();
			cryptokiCertificate.GP_Name = ChipsetList.Codes.Bit4id;
			Factory.Save();

			var sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();
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
			var companyWrapper = GlbCompanyWrapper.Get(currentCompany);
			var mauPassword = companyWrapper.PasswordCollection.AddNew();
			mauPassword.GP_UserID = "1234";
			currentCompany.Factory.Save();

			Declaration.JE_CustomsProfile = "1234";

			var sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();
			var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
			AssertNotNull("MAU Certificate", valuesProvider.MauCertificate);
			AssertEquals("GP_UserID", "1234", valuesProvider.MauCertificate.GP_UserID);
		}
	}

	public void TestServiceTypeNamespace()
	{
		var sendingObject = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		AssertEquals("ServiceTypeNamespace", ExpectedServiceTypeNamespace, sendingObject.ServiceTypeNamespace);
	}

	protected abstract string ExpectedServiceTypeNamespace { get; }

	public void TestServiceTypePrefix()
	{
		var sendingObject = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		AssertEquals("ServiceTypePrefix", ExpectedServiceTypePrefix, sendingObject.ServiceTypePrefix);
	}

	public void TestHasValidAutomaticSignature()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			var sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();
			var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
			AssertEquals("HasValidAutomaticSignature", false, valuesProvider.HasValidAutomaticSignature);

			var currentStaff = GlbStaff.CurrentUser;
			var staffWrapper = GlbStaffWrapper.Get(currentStaff);
			var automaticSignature = staffWrapper.AutomaticSignaturePasswordCollection.AddNew();

			automaticSignature.IsConfigurationActive = true;
			AssertEquals("HasValidAutomaticSignature", true, valuesProvider.HasValidAutomaticSignature);

			automaticSignature.IsConfigurationActive = false;
			AssertEquals("HasValidAutomaticSignature", false, valuesProvider.HasValidAutomaticSignature);
		}
	}

	protected abstract string ExpectedServiceTypePrefix { get; }

	protected override BusinessObject GetNewBusinessObject()
	{
		return (Ucc6JobDeclarationMessageSendingObject)Activator.CreateInstance(GetExpectedBusinessObjectType(), EntryHeader, JobDeclarationMessageSendingObjectParent);
	}

	public void TestXmlSigner()
	{
		var sendingObject = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		AssertType<AidaXmlSigner>("XmlSigner Type", sendingObject.XmlSigner);
	}

	public void TestGetMessageType()
	{
		var sendingObject = (Ucc6JobDeclarationMessageSendingObject)GetNewBusinessObject();
		sendingObject.MessageType = "ABC";
		var valuesProvider = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)sendingObject;
		AssertEquals("GetMessageType", "ABC", valuesProvider.GetMessageType());
	}

	public void TestLocalReferenceNumberGenerator()
	{
		var sendingObject = (IAidaXmlOutgoingCustomsMessageGeneratorValuesProvider)GetNewBusinessObject();
		AssertType<LocalReferenceNumberGenerator>("LocalReferenceNumberGenerator Type", sendingObject.LocalReferenceNumberGenerator);
	}
}

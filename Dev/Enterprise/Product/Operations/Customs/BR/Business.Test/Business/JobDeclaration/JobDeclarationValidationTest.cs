using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class JobDeclarationValidationTest : BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestDeclaration()
		{
			AssertEquals(declaration.Validation.Declaration, declaration);
		}

		public void TestCheckJE_MessageType_MessageErrorIfChangedAfterSendingMessage()
		{
			JobDeclarationValidationBaseOnlyTest.AssertErrorsAfterChangingJE_MessageType<JobDeclaration>(Factory, false, true, false);
		}

		public virtual void TestCheckJE_CustomsOffice()
		{
			declaration.JE_CustomsOffice = "3432431";
			AssertHasMessageErrorContaining(declaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
		}

		public virtual void TestCheckJE_LocationOfGoods()
		{
			ReferenceTestDataHelper.CreateCustomsEnclosureCodes(declaration.Factory);
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_LocationOfGoodsInfo, "XX99999", "CE00001");
		}

		public virtual void TestCheckJE_MessageTypeIsEnteredOrValid()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(declaration.JE_MessageTypeInfo);
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.JE_MessageTypeInfo, "XXX", BRJobMessageTypeList.Codes.Import);
		}

		public virtual void TestCheckJE_ContainerMode_Mandatory()
		{
			const string containerModeMandatory = "Container type is required for sea shipment.";

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.Validation.ValidateJE_ContainerMode();
			AssertHasWarning($"Message Type: {JobMessageType}", declaration.JE_ContainerModeInfo, containerModeMandatory);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.Validation.ValidateJE_ContainerMode();
			AssertNoNotifications("JE_ContainerMode must NOT have notification", declaration.JE_ContainerModeInfo);
		}

		public void TestCheckJE_MessageType()
		{
			const string licensesAttachedErrorsMessage = "Licenses should not be attached to any jobs but ISW. Please either detach the Licenses(s) or keep the Shipment Type as ISW.";

			declaration.AttachedImportLicenseEntries.AddNew();
			declaration.Validation.ValidateJE_MessageType();

			if (JobMessageType == BRJobMessageTypeList.Codes.ImportSiscomex)
			{
				AssertNoNotifications(declaration.JE_MessageTypeInfo);
			}
			else
			{
				AssertHasError("Has attached licenses", declaration.JE_MessageTypeInfo, licensesAttachedErrorsMessage);

				declaration.AttachedImportLicenseEntries.RemoveAndDeleteAll();
				declaration.Validation.ValidateJE_MessageType();
				AssertNoError("No attached licenses", declaration.JE_MessageTypeInfo, licensesAttachedErrorsMessage);
			}
		}

		public void TestCheckJE_LocationQualifier()
		{
			var clearanceLocal = declaration.ClearanceLocalInvolvedParty;

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.ClearanceOfficeIsCustomsEnclosure = false;
			AssertNoMessageErrorContaining("No Message Error when MessageType is Import", clearanceLocal.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.ClearanceOfficeIsCustomsEnclosure = false;
			AssertHasMessageErrorContaining(clearanceLocal.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_ApplicationCode()
		{
			foreach (var messageType in new BRJobMessageTypeList().GetAllCodes())
			{
				declaration.JE_MessageType = messageType;
				CombineAssertions($"JE_MessageType = {messageType}", () =>
				{
					ValidationTestHelper.AssertErrorIfNotEntered(declaration.JE_ApplicationCodeInfo);
					ValidationTestHelper.AssertErrorIfInvalidCode(declaration.JE_ApplicationCodeInfo, "XXX", DeclarationApplicationCodeList.Codes.Builtin);
				});
			}
		}

		public virtual void TestCheckBRTransportMode()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.BRTransportModeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(declaration.BRTransportModeInfo, "XXX", BRTransportModeList.Codes.SEA);
		}

		public void TestCheckJE_TransportMode()
		{
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.JE_TransportModeInfo);
		}

		public void TestCheckJE_GS_NKCusAgent()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BR1";

			declaration.Validation.ValidateJE_GS_NKCusAgent();

			if (JobMessageType == BRJobMessageTypeList.Codes.Export || JobMessageType == BRJobMessageTypeList.Codes.Import)
			{
				AssertHasMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_GS_NKCusAgent = broker.GS_Code;
				AssertNoMessageErrorContaining(declaration.JE_GS_NKCusAgentInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageError(declaration.JE_GS_NKCusAgentInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

				var wrapper = BRGlbStaffWrapper.Get(broker);
				var password = wrapper.CCTPassword;
				password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
				password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				password.GP_ExpiryDate = ZDateTime.Today.AddDays(-10);
				password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;
				declaration.Validation.ValidateJE_GS_NKCusAgent();
				AssertHasMessageError(declaration.JE_GS_NKCusAgentInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

				password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
				password.GP_PasswordStatus = BRPasswordStatusList.Codes.Invalid;
				declaration.Validation.ValidateJE_GS_NKCusAgent();
				AssertHasMessageError(declaration.JE_GS_NKCusAgentInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");

				password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;
				declaration.Validation.ValidateJE_GS_NKCusAgent();
				AssertNoMessageError(declaration.JE_GS_NKCusAgentInfo, "Digital Certificate not informed or expired or invalid. Please press F3 in the field and enter a valid digital certificate in Credentials tab.");
			}
			else
			{
				AssertNoNotifications(declaration.JE_GS_NKCusAgentInfo);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			BRCustomsDataRegistry.Instance.EnableLPCO.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BRCustomsDataRegistry.Instance.EnableImportLicense.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BRCustomsDataRegistry.Instance.EnableImportSiscomex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			declaration.JE_MessageType = JobMessageType;
		}

		protected new JobDeclaration declaration => base.declaration as JobDeclaration;

		protected virtual string JobMessageType => ZString.Empty;
	}
}

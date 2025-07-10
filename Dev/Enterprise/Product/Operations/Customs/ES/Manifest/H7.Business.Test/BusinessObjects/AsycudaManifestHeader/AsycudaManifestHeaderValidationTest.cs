using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using OrgCusCode = Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_CustomsProfile_Mandatory()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";

			var wrapper = ES.Business.GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TESTCERT1";
			cert.GP_MailBoxID = "Test1";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			var auth = cert.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff.PK;

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_GS_NKCustomsAgent = "AH";
			manifestHeader.AMA_CustomsProfile = "TESTCERT1";

			CombineAssertions(() =>
			{
				manifestHeader.AMA_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining("Error is not shown cause Certificate is not empty", manifestHeader.AMA_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

				manifestHeader.AMA_CustomsProfile = ZString.Empty;
				AssertHasMessageErrorContaining("Error is shown cause Certificate is empty", manifestHeader.AMA_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestGoodsLocationDescription()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			cusGoodsLocation.CGL_ParentID = manifest.PK;
			cusGoodsLocation.CGL_LocationUse = CusGoodsLocationUseList.Codes.Departure;
			cusGoodsLocation.CGL_ParentTableCode = "AMA";
			bill.ABL_ShipmentType = EU.Business.EntrySubStyleList.Codes.NormalDeclaration;

			manifest.ValidateGoodsLocationDescription();
			AssertNoMessageErrors("No errors during validation of manifest header's GoodsLocationDescription", manifest.GoodsLocationDescriptionInfo);
		}

		public void TestValidateRepresentative()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				AssertNoMessageErrors("Representative is optional", header.AMA_OA_RepresentativeInfo);

				header.AMA_OA_Representative = orgAddress.PK;
				header.Validation.ValidateAMA_OA_Representative();
				AssertHasMessageError("Contact info is required", header.AMA_OA_RepresentativeInfo, requiredCusTypeContactMessage);

				var contact = orgHeader.Contacts.AddNew();
				var personalAlloc = contact.Allocations.AddNew();
				personalAlloc.PC_Type = OrgConstants.ContactAllocationType.CUS;

				contact.OC_IsActive = false;
				header.Validation.ValidateAMA_OA_Representative();
				AssertHasMessageError("Contact info is still required if the only qualified contact is not active", header.AMA_OA_RepresentativeInfo, requiredCusTypeContactMessage);

				contact.OC_IsActive = true;
				header.Validation.ValidateAMA_OA_Representative();
				AssertEquals(1, header.AMA_OA_RepresentativeInfo.GetMessageErrors().Count());
				AssertHasMessageError("EORI or NIF number is required", header.AMA_OA_RepresentativeInfo, noEoriOrNifForRepresentativeMessage);

				header.AMA_AgentType = EUH7AgentTypes.Codes.IND;
				header.Validation.ValidateAMA_OA_Representative();
				AssertNoMessageErrorContaining("When AMA_AgentType is equal to IND, the email verification has not been triggered.", header.AMA_OA_RepresentativeInfo, noEmailForCusContactMessage);

				header.AMA_AgentType = EUH7AgentTypes.Codes.DIR;
				header.Validation.ValidateAMA_OA_Representative();
				AssertHasMessageError("When AMA_AgentType is equal to DIR, Contact email is required", header.AMA_OA_RepresentativeInfo, noEmailForCusContactMessage);

				header.AMA_AgentType = ESH7AgentTypes.Codes.ICA;
				header.Validation.ValidateAMA_OA_Representative();
				AssertHasMessageError("When AMA_AgentType is equal to ICA, Contact email is required", header.AMA_OA_RepresentativeInfo, noEmailForCusContactMessage);

				contact.OC_Email = "test@123.com";
				header.Validation.ValidateAMA_OA_Representative();
				AssertNoMessageError("Contact email is satisfied", header.AMA_OA_RepresentativeInfo, noEmailForCusContactMessage);

				AssertOrgAddressEORIOrNIFNumber(header.AMA_OA_RepresentativeInfo, noEoriOrNifForRepresentativeMessage, header.Validation.ValidateAMA_OA_Representative);

				AssertNoMessageErrors(header.AMA_OA_RepresentativeInfo);
			});
		}

		public void TestValidateDeclarant_IdentificationNumberCheck()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_OA_Declarant = orgAddress.PK;

			AssertOrgAddressEORIOrNIFNumber(header.AMA_OA_DeclarantInfo, noEoriOrNifForDeclarantMessage, header.Validation.ValidateAMA_OA_Declarant);
		}

		public void TestCheckAMA_CustomsOffice_Mandatory()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Validation.ValidateAMA_CustomsOffice();
			AssertHasMessageError(header.AMA_CustomsOfficeInfo, noCustomsOfficeMessage);
		}

		public void TestCheckAMA_OA_Presenter_MandatoryCheck()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_OH = orgHeader.PK;
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_OA_Presenter = ZGuid.Empty;
			AssertHasMessageError(header.AMA_OA_PresenterInfo, noPresenterMessage);

			header.AMA_OA_Presenter = orgAddress.PK;
			AssertNoMessageError(header.AMA_OA_PresenterInfo, noPresenterMessage);
		}

		public void TestCheckAMA_OA_Presenter_IdentificationNumberCheck()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_OH = orgHeader.PK;
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_OA_Presenter = orgAddress.PK;
			AssertOrgAddressEORIOrNIFNumber(header.AMA_OA_PresenterInfo, noEoriOrNifForPresenterMessage, header.Validation.ValidateAMA_OA_Presenter);
		}

		void AssertOrgAddressEORIOrNIFNumber(ZPropertyInfo orgAddressFKPropertyInfo, string expectedMessageErrorText, Action validationAction)
		{
			var orgAddress = Factory.Load<OrgAddress>((ZGuid)orgAddressFKPropertyInfo.Value);
			orgAddress.CustomsCodes.DeleteAll();
			validationAction();
			AssertHasMessageError(orgAddressFKPropertyInfo, expectedMessageErrorText);

			var cusCode = orgAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Spain);
			validationAction();
			AssertNoMessageError("EORI number is satisfied", orgAddressFKPropertyInfo, expectedMessageErrorText);

			orgAddress.CustomsCodes.DeleteAll();
			orgAddress.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "12345", Core.Constants.CountryCodes.Spain);
			validationAction();
			AssertNoMessageError("NIF number is satisfied", orgAddressFKPropertyInfo, expectedMessageErrorText);
		}

		public void TestCheckAMA_MasterInformation_MandatoryCheck()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_MasterInformation = ZString.Empty;
			AssertHasMessageError(header.AMA_MasterInformationInfo, noMasterInformationMessage);

			header.AMA_MasterInformation = "123456";
			AssertNoMessageError(header.AMA_MasterInformationInfo, noMasterInformationMessage);
		}

		public void TestCheckAMA_MasterInformation_FormatCheck()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_MasterInformation = "123+abc";
			AssertHasMessageError(header.AMA_MasterInformationInfo, invalidMasterInformationMessage);

			header.AMA_MasterInformation = "123abc";
			AssertNoMessageError(header.AMA_MasterInformationInfo, invalidMasterInformationMessage);
		}

		public void TestCheckAMA_MasterInformation_LengthCheck()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("Max length of AMA_MasterInformationInfo is 70", header.AMA_MasterInformationInfo.MaxLength, 70);
		}

		public void TestEntryLineNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using (header.SuspendValidationTesting())
			{
				header.Validation.ValidateEntryLineNumber();

				AssertHasMessageError(header.EntryLineNumberInfo, "You have not entered a Bill Number or a Entry Line Number.");

				header.MasterBill.ABL_BillNumber = "12345";
				header.EntryLineNumber = "12345";
				AssertHasMessageError(header.EntryLineNumberInfo, "You have entered both a Master Bill Number and a Entry Line Number. Only one must be entered.");

				header.EntryLineNumberInfo.ClearAllNotifications();
				header.Validation.ValidateAll();
				AssertHasMessageError("ValidateEntryLineNumbr() is included in ValidateAll()", header.EntryLineNumberInfo, "You have entered both a Master Bill Number and a Entry Line Number. Only one must be entered.");

				header.MasterBill.ABL_BillNumber = string.Empty;
				header.Validation.ValidateEntryLineNumber();
				AssertNoMessageErrors(header.EntryLineNumberInfo);
			}
		}

		readonly string requiredCusTypeContactMessage = "'CUS - Customs' type contact is required when Representative is provided. It can be configured within the Organization > Contact > Allocated Contact.";
		readonly string noEmailForCusContactMessage = "You have not entered a Contact Email for the ‘CUS - Customs’ type contact. It can be configured within the Organization > Contact.";
		readonly string noCustomsOfficeMessage = "You have not entered an office of type Office of Lodgement.";
		readonly string noPresenterMessage = "You have not entered a Presenter.";
		readonly string noEoriOrNifForPresenterMessage = "You have not entered an EORI or NIF number for the Presenter.";
		readonly string noEoriOrNifForRepresentativeMessage = "You have not entered an EORI or NIF number for the Representative.";
		readonly string noEoriOrNifForDeclarantMessage = "You have not entered an EORI or NIF number for the Declarant.";
		readonly string noMasterInformationMessage = "You have not entered a DSDT MRN/Flight No..";
		readonly string invalidMasterInformationMessage = "DSDT MRN/Flight No. must be alphanumeric.";
	}
}

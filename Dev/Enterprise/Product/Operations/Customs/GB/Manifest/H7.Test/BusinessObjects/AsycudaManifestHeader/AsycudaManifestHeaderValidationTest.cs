using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	public class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAMA_CustomsProfile_WhenCurrentBranchHasNoBadgeCodeForPortOfFirstArrival_ShouldShowError()
		{
			ClearRegistryBadgeCodes();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfFirstArrival = "GBABD";
			header.AMA_RL_NKPortOfDischarge = "GBLIV";
			header.Validation.ValidateAll();

			AssertHasMessageError(header.AMA_CustomsProfileInfo, EmptyProfileListErrorForPortOfFirstArrival);
		}

		public void TestValidateAMA_CustomsProfile_WhenCurrentBranchHasNoBadgeCodeForPortOfDischarge_ShouldShowError()
		{
			ClearRegistryBadgeCodes();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfFirstArrival = string.Empty;
			header.AMA_RL_NKPortOfDischarge = "GBLIV";
			header.Validation.ValidateAll();

			AssertHasMessageError(header.AMA_CustomsProfileInfo, EmptyProfileListErrorForPortOfDischarge);
		}

		public void TestValidateAMA_CustomsProfile_WhenBadgeCodeIsInvalid_ShouldShowError()
		{
			CreateBadgeCode("PR1", "CDS");

			var header = CreateManifestHeader();
			header.AMA_CustomsProfile = "ERR";
			header.Validation.ValidateAll();

			AssertHasMessageError(header.AMA_CustomsProfileInfo, InvalidBadgeCodeError);
		}

		public void TestValidateAMA_CustomsProfile_WhenBadgeCodeIsEmpty_ShouldShowError()
		{
			CreateBadgeCode("PR1", "CDS");

			var header = CreateManifestHeader();
			header.AMA_CustomsProfile = string.Empty;
			header.Validation.ValidateAll();

			AssertHasMessageError(header.AMA_CustomsProfileInfo, InvalidBadgeCodeError);
		}

		public void TestValidateAMA_CustomsProfile_WhenNoCDSCredentialsExists_ShouldShowError()
		{
			CreateBadgeCode("PR1", "CDS");

			var header = CreateManifestHeader();
			header.AMA_CustomsProfile = "PR1";
			header.Validation.ValidateAll();

			AssertHasMessageErrorContaining(header.AMA_CustomsProfileInfo, NoCompanyLevelCDSCredentialsError);
		}

		public void TestValidateAMA_CustomsProfile_WhenCDSCredentialStatusIsValidButAboutToExpire_ShouldShowWarning()
		{
			CreateBadgeCode("PR1", "CDS");
			CreatePassword("PR1", PasswordStatusList.Codes.PasswordOK, DateTime.Today.AddMonths(-18).AddDays(5), DateTime.Today.AddDays(5));

			var header = CreateManifestHeader();
			header.AMA_CustomsProfile = "PR1";
			header.Validation.ValidateAll();

			AssertHasWarningContaining(header.AMA_CustomsProfileInfo, AboutToExpireCDSCredentialsWarning);
		}

		public void TestValidateAMA_CustomsProfile_WhenCDSCredentialStatusIsOkButAboutToExpire_ShouldShowWarning()
		{
			CreateBadgeCode("PR1", "CDS");
			CreatePassword("PR1", PasswordStatusList.Codes.PasswordOK, DateTime.Today.AddMonths(-18).AddDays(5), DateTime.Today.AddDays(5));

			var header = CreateManifestHeader();
			header.AMA_CustomsProfile = "PR1";
			header.Validation.ValidateAll();

			AssertHasWarningContaining(header.AMA_CustomsProfileInfo, AboutToExpireCDSCredentialsWarning);
		}

		public void TestValidateAMA_CustomsProfile_WhenCDSCredentialStatusIsValidButHasExpired_ShouldShowError()
		{
			CreateBadgeCode("PR1", "CDS");
			CreatePassword("PR1", PasswordStatusList.Codes.Valid, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(-10));

			var header = CreateManifestHeader();
			header.AMA_CustomsProfile = "PR1";
			header.Validation.ValidateAll();

			AssertHasMessageErrorContaining(header.AMA_CustomsProfileInfo, InvalidCDSCredentialsError);
		}

		public void TestValidateAMA_CustomsProfile_WhenCDSCredentialStatusIsOkButHasExpired_ShouldShowError()
		{
			CreateBadgeCode("PR1", "CDS");
			CreatePassword("PR1", PasswordStatusList.Codes.PasswordOK, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(-10));

			var header = CreateManifestHeader();
			header.AMA_CustomsProfile = "PR1";
			header.Validation.ValidateAll();

			AssertHasMessageErrorContaining(header.AMA_CustomsProfileInfo, ExpiredCDSCredentialsError);
		}

		public void TestValidateAMA_CustomsProfile_WhenCDSCredentialStatusIsInvalid_ShouldShowError()
		{
			CreateBadgeCode("PR1", "CDS");
			CreatePassword("PR1", PasswordStatusList.Codes.Invalid, DateTime.Today.AddDays(-5));

			var header = CreateManifestHeader();
			header.AMA_CustomsProfile = "PR1";
			header.Validation.ValidateAll();

			AssertHasMessageErrorContaining(header.AMA_CustomsProfileInfo, InvalidCDSCredentialsError);
		}

		public void TestValidateAMA_CustomsProfile_WhenCDSCredentialStatusIsDeactivated_ShouldShowError()
		{
			CreateBadgeCode("PR1", "CDS");
			CreatePassword("PR1", PasswordStatusList.Codes.Deactivated, DateTime.Today.AddDays(-5));

			var header = CreateManifestHeader();
			header.AMA_CustomsProfile = "PR1";
			header.Validation.ValidateAll();

			AssertHasMessageErrorContaining(header.AMA_CustomsProfileInfo, DeactivatedCDSCredentialsError);
		}

		public void TestValidateAMA_CustomsProfile_WhenCDSCredentialStatusIsError_ShouldShowError()
		{
			CreateBadgeCode("PR1", "CDS");
			CreatePassword("PR1", PasswordStatusList.Codes.Error, DateTime.Today.AddDays(-5));

			var header = CreateManifestHeader();
			header.AMA_CustomsProfile = "PR1";
			header.Validation.ValidateAll();

			AssertHasMessageErrorContaining(header.AMA_CustomsProfileInfo, UnexpectedCDSCredentialsError);
		}

		public void TestValidateAMA_CustomsProfile_WhenCDSCredentialStatusIsValidAndNotAboutToExpire_ShouldShowError()
		{
			CreateBadgeCode("PR1", "CDS");
			CreatePassword("PR1", PasswordStatusList.Codes.Valid, DateTime.Today.AddDays(-5));

			var header = CreateManifestHeader();
			header.AMA_CustomsProfile = "PR1";
			header.Validation.ValidateAll();

			AssertNoMessageErrors(header.AMA_CustomsProfileInfo);
		}

		public void TestValidateAMA_CustomsProfile_WhenCDSCredentialStatusIsOkAndNotAboutToExpire_ShouldNotShowError()
		{
			CreateBadgeCode("PR1", "CDS");
			CreatePassword("PR1", PasswordStatusList.Codes.PasswordOK, DateTime.Today.AddDays(-5));

			var header = CreateManifestHeader();
			header.AMA_CustomsProfile = "PR1";
			header.Validation.ValidateAll();

			AssertNoMessageErrors(header.AMA_CustomsProfileInfo);
		}

		public void TestValidateAMA_CustomsProfile_WhenCSPIsNotCDS_ShouldNotShowError()
		{
			CreateBadgeCode("PR1", "PNT");

			var header = CreateManifestHeader();
			header.AMA_CustomsProfile = "PR1";
			header.Validation.ValidateAll();

			AssertNoMessageErrors(header.AMA_CustomsProfileInfo);
		}

		public void TestValidateCSP_WhenCSPIsEmpty_ShouldShowError()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR1";

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_CustomsProfile = "ERR";
			header.Validation.ValidateAll();

			AssertHasMessageError(header.CSPInfo, InvalidCSPError);
		}

		public void TestValidateCSP_WhenCSPIsNotEmpty_ShouldNotShowError()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR1";
			badgeCodeSetting.CSPCode = "CDS";

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_CustomsProfile = "PR1";
			header.Validation.ValidateAll();

			AssertNoMessageErrors(header.CSPInfo);
		}

		public void TestValidateRepresentativeEmptyWhenDeclarantIsImporter()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var commonOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var representative = Factory.NewWithValidTestData<OrgAddress>();
			var declarant = commonOrgHeader.Addresses.AddNew();
			var consignee = commonOrgHeader.Addresses.AddNew();

			var bill = header.Bills.AddNew();
			bill.ABL_OA_Consignee = consignee.PK;
			header.AMA_OA_Declarant = declarant.PK;
			header.AMA_OA_Representative = representative.PK;

			header.Validation.ValidateAMA_OA_Representative();
			AssertHasMessageError(header.AMA_OA_RepresentativeInfo, RepresentativeShouldBeEmptyError);
		}

		public void TestValidateRepresentativeDoesNotEqualDeclarant()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var commonOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var declarant = commonOrgHeader.Addresses.AddNew();
			var representative = commonOrgHeader.Addresses.AddNew();

			header.AMA_OA_Declarant = declarant.PK;
			header.AMA_OA_Representative = representative.PK;

			header.Validation.ValidateAMA_OA_Representative();
			AssertHasMessageError(header.AMA_OA_RepresentativeInfo, DeclarantCannotBeRepresentativeError);
		}

		public void TestValidateAMA_AgentTypeIsEmptyWhenDeclarantIsImporter()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var commonOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var declarant = commonOrgHeader.Addresses.AddNew();
			var consignee = commonOrgHeader.Addresses.AddNew();

			var bill = header.Bills.AddNew();
			bill.ABL_OA_Consignee = consignee.PK;
			header.AMA_AgentType = ZString.Empty;
			header.AMA_OA_Declarant = declarant.PK;
			header.AMA_OA_Representative = ZGuid.Empty;

			header.Validation.ValidateAMA_AgentType();
			AssertNoMessageErrorContaining(header.AMA_AgentTypeInfo, AgentTypeMustBeEmptyError);

			header.AMA_AgentType = "TST";

			header.Validation.ValidateAMA_AgentType();
			AssertHasMessageError(header.AMA_AgentTypeInfo, AgentTypeMustBeEmptyError);
		}

		public void TestValidateAMA_CustomsProfile_WhenCurrentBranchHasNoBadgeCodeForEmptyPortOfDischargeAndEmptyPortOfArrival_ShouldShowError()
		{
			ClearRegistryBadgeCodes();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfFirstArrival = ZString.Empty;
			header.AMA_RL_NKPortOfDischarge = ZString.Empty;
			header.Validation.ValidateAMA_CustomsProfile();

			AssertHasMessageError(header.AMA_CustomsProfileInfo, EmptyProfileListErrorWithNotSpecifiedPort);
		}

		public void TestCheckAMA_VesselName()
		{
			SetupRegionName();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			header.AMA_RL_NKPortOfDischarge = "GBBEL";
			header.AMA_VesselName = ZString.Empty;
			AssertNoMessageErrorContaining("No enter check errors if VesselName is empty and TransportMode is sea and PortOfLoading is in GB and admin region is in NORTHERN IRELAND", header.AMA_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_RL_NKPortOfDischarge = "GBLON";
			header.AMA_VesselName = "123456789";
			AssertNoMessageErrorContaining("No enter check errors if VesselName is not empty", header.AMA_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_VesselName = ZString.Empty;
			AssertHasMessageErrorContaining("Has enter check errors if VesselName is empty and PortOfLoading is in GB and admin region is not in NORTHERN IRELAND", header.AMA_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_TransportMode = TransportTypeList.Codes.Rail;
			AssertNoMessageErrorContaining("No enter check errors if TransportMode is not SEA", header.AMA_VesselNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		void CreateBadgeCode(ZString badge, ZString cspCode)
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = badge;
			badgeCodeSetting.CSPCode = cspCode;

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);
		}

		void CreatePassword(ZString badge, ZString status, ZDateTime issueDate, ZDateTime? expiryDate = null)
		{
			var extPwd = Factory.New<GlbExternalPassword_GB>();
			extPwd.GP_GC = GlbCompany.CurrentCompany.PK.ToGuid();
			extPwd.EORI = $"{GlbCompany.CurrentCompany.Country.Code}{EORI}";
			extPwd.GP_UserID = $"{extPwd.EORI}.{badge}";
			extPwd.GP_PasswordType = "CDS";
			extPwd.GP_CurrentPassword = "NOWPWD";
			extPwd.GP_NextPassword = "NEXTPWD";
			extPwd.GP_IssueDate = issueDate;
			extPwd.GP_ExpiryDate = expiryDate ?? issueDate.AddDays(10);
			extPwd.GP_PasswordStatus = status;
			extPwd.IsTokenForCDS = true;
		}

		AsycudaManifestHeader CreateManifestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			if (!header.Declarant.Header.CustomsCodes.OfType<OrgCusCode>().Any(x => x.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori))
			{
				header.Declarant.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, EORI);
			}

			return header;
		}

		void ClearRegistryBadgeCodes()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			badgeCodeSettings.RemoveAll();
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);
		}

		void SetupRegionName()
		{
			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var refCountryStates = Factory.New<RefCountryStates>();
				refCountryStates.RW_RN_NKCountryCode = "UK";
				refCountryStates.RW_Code = "XXX";
				refCountryStates.RW_RegionName = "NORTHERN IRELAND";
				belfast.RL_RW = refCountryStates.PK;
			}

			var london = new RefUNLOCO.Loader(Factory).Load("GBLON");
			if (london.CountryStates == null || string.Compare(london.CountryStates.RW_RegionName, "ENGLAND", true) != 0)
			{
				var refCountryStates = Factory.New<RefCountryStates>();
				refCountryStates.RW_RN_NKCountryCode = "UK";
				refCountryStates.RW_Code = "YYY";
				refCountryStates.RW_RegionName = "ENGLAND";
				london.RL_RW = refCountryStates.PK;
			}
		}

		string DeclarantCannotBeRepresentativeError => "A representative must only be declared where the representative differs from the declarant. Remove the representative, or select a different declarant.";
		string RepresentativeShouldBeEmptyError => "When the declarant is the importer, a representative is not allowed. Remove the representative, or select a different importer or declarant.";
		string AgentTypeMustBeEmptyError => "For self-representation, representative status must not be declared.";
		string EmptyProfileListErrorWithNotSpecifiedPort => "No badge code was found in the registry for the specified criteria. Check the port codes and direction. If these are correct then have your administrator check the badges in the registry.";
		string EmptyProfileListErrorForPortOfFirstArrival => "No badge code was found in the registry for the specified Port of First Arrival code. If this is correct then have your administrator check the badges in the registry.";
		string EmptyProfileListErrorForPortOfDischarge => "No badge code was found in the registry for the specified Discharge Port code. If this is correct then have your administrator check the badges in the registry.";
		string InvalidBadgeCodeError => "You must have a valid Badge Code. Badge Codes are setup under Admin -> System -> Registry -> Customs -> Country or Region Specific -> United Kingdom -> Badge Codes";
		string InvalidCSPError => "It is not possible to transmit to Customs when the gateway field is blank. Set this field indirectly by selecting a valid profile.";
		string NoCompanyLevelCDSCredentialsError => "No company-level CDS credentials exist.";
		string InvalidCDSCredentialsError => "Credentials are invalid, await further updates from CDS via eHub.";
		string ExpiredCDSCredentialsError => "Credentials have expired. Ensure that your EHU service task is running, and refresh them if necessary.";
		string AboutToExpireCDSCredentialsWarning => "You willl need to refresh them soon.";
		string DeactivatedCDSCredentialsError => "Credentials are inactive, await further updates from CDS via eHub.";
		string UnexpectedCDSCredentialsError => "Credentials are in an unexpected state";
		string EORI => "EORI0000001";
	}
}

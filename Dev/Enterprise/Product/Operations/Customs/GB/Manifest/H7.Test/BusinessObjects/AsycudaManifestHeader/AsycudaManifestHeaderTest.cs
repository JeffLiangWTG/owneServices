using System;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : EU.H7.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestBills()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertType<AsycudaBillCollection>(header.Bills);
		}

		public void TestAMA_RN_NKCountry_Caption()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var resourceStringDataAttribute = header.AMA_RN_NKCountryInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("State", resourceStringDataAttribute.Caption);
		}

		public void TestAMA_AgentTypeCaption()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var resourceStringDataAttribute = header.AMA_AgentTypeInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Rep. Status", resourceStringDataAttribute.Caption);
		}

		public void TestAMA_CustomsProfile()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var resourceStringDataAttribute = header.AMA_CustomsProfileInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Profile", resourceStringDataAttribute.Caption);

			var listAttribute = header.AMA_CustomsProfileInfo.GetAttribute<ListAttribute>();
			AssertNotNull(listAttribute);
			AssertEquals("Lookups.ProfileList", listAttribute.ListDataSourceMember);
		}

		public void TestCSP()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertEquals(true, header.CSPInfo.ReadOnly);

			var resourceStringDataAttribute = header.CSPInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("CSP", resourceStringDataAttribute.Caption);

			var listAttribute = header.CSPInfo.GetAttribute<ListAttribute>();
			AssertNotNull(listAttribute);
			AssertEquals("Lookups.CSPList", listAttribute.ListDataSourceMember);

			header.CSP = GatewayList.Codes.CCSUKviaNTMsgGW;
			AssertEquals("The Set Value is equal to the Get Value", GatewayList.Codes.CCSUKviaNTMsgGW, header.CSP);
			Factory.Save();

			var headerInNewFactory = NewFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals("Value was persisted", GatewayList.Codes.CCSUKviaNTMsgGW, headerInNewFactory.CSP);
		}

		public void TestAMA_CustomsProfile_Changed_WhenBadgeCodeIsValid_ShouldUpdateCSP()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);

			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR1";
			badgeCodeSetting.CSPCode = "CCSUK";

			badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR2";
			badgeCodeSetting.CSPCode = "CNS";

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_CustomsProfile = "PR2";

			AssertEquals("CNS", header.CSP);
		}

		public void TestAMA_CustomsProfile_Changed_WhenBadgeCodeIsInvalid_ShouldClearCSP()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);

			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR1";
			badgeCodeSetting.CSPCode = "CCSUK";

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_CustomsProfile = "PR1";
			header.AMA_CustomsProfile = "PR3";

			AssertEquals(string.Empty, header.CSP);
		}

		public void TestAMA_CustomsProfile_Changed_WhenBadgeCodeIsEmpty_ShouldClearCSP()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);

			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR1";
			badgeCodeSetting.CSPCode = "CCSUK";

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_CustomsProfile = "PR1";
			header.AMA_CustomsProfile = string.Empty;

			AssertEquals(string.Empty, header.CSP);
		}

		public void TestAMA_CustomsProfile_WhenNoBadgeCodeExists_ShouldSetFirstCodeAsDefaultValue()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR2";

			badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR1";

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertEquals("Profile", "PR1", header.AMA_CustomsProfile);
		}

		public void TestAMA_CustomsProfile_WhenNoBadgeCodeHasValidToken_ShouldSetCodeWithLatestExpiredTokenAsDefaultValue()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR1";

			badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR2";

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			CreatePassword(GlbCompany.CurrentCompany, "PR1", PasswordStatusList.Codes.Invalid, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(-15));
			CreatePassword(GlbCompany.CurrentCompany, "PR2", PasswordStatusList.Codes.Invalid, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(-10));
			CreatePassword(GlbCompany.CurrentCompany, "PR3", PasswordStatusList.Codes.Valid, DateTime.Today.AddDays(-3));

			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertEquals("Profile", "PR2", header.AMA_CustomsProfile);
		}

		public void TestAMA_CustomsProfile_WhenManyBadgeCodesHaveValidToken_ShouldSetCodeWithLatestIssuedTokenAsDefaultValue()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR1";

			badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR2";

			badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR3";

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			CreatePassword(GlbCompany.CurrentCompany, "PR1", PasswordStatusList.Codes.Valid, DateTime.Today.AddDays(-10));
			CreatePassword(GlbCompany.CurrentCompany, "PR2", PasswordStatusList.Codes.Valid, DateTime.Today.AddDays(-5));
			CreatePassword(GlbCompany.CurrentCompany, "PR3", PasswordStatusList.Codes.Invalid, DateTime.Today.AddDays(-3), DateTime.Today.AddDays(-1));

			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			AssertEquals("Profile", "PR2", header.AMA_CustomsProfile);
		}

		public void TestAMA_CustomsProfile_WhenPortOfFirstArrivalChanges_ShouldSetDefaultValue()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR1";
			badgeCodeSetting.RL_PortCode = "GBLIV";

			badgeCodeSetting.BadgeCode = "PR2";
			badgeCodeSetting.RL_PortCode = "GBABD";

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			CreatePassword(GlbCompany.CurrentCompany, "PR1", PasswordStatusList.Codes.Invalid, DateTime.Today.AddDays(-10));
			CreatePassword(GlbCompany.CurrentCompany, "PR2", PasswordStatusList.Codes.Valid, DateTime.Today.AddDays(-5));

			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_CustomsProfile = "PR1";
			header.AMA_RL_NKPortOfFirstArrival = "GBABD";
			header.AMA_RL_NKPortOfDischarge = "GBLIV";

			AssertEquals("Profile", "PR2", header.AMA_CustomsProfile);
		}

		public void TestAMA_CustomsProfile_WhenPortOfDischargeChanges_ShouldSetDefaultValue()
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = "PR1";
			badgeCodeSetting.RL_PortCode = "GBLIV";

			badgeCodeSetting.BadgeCode = "PR2";
			badgeCodeSetting.RL_PortCode = "GBABD";

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);

			CreatePassword(GlbCompany.CurrentCompany, "PR1", PasswordStatusList.Codes.Invalid, DateTime.Today.AddDays(-10));
			CreatePassword(GlbCompany.CurrentCompany, "PR2", PasswordStatusList.Codes.Valid, DateTime.Today.AddDays(-5));

			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			header.AMA_CustomsProfile = "PR1";
			header.AMA_RL_NKPortOfFirstArrival = string.Empty;
			header.AMA_RL_NKPortOfDischarge = "GBABD";

			AssertEquals("Profile", "PR2", header.AMA_CustomsProfile);
		}

		public void TestCurrencyConverterInitialization()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var currencyConverter = header.CurrencyConverter;

			CombineAssertions(() =>
			{
				AssertNotNull(currencyConverter);
				AssertEquals(ZDateTime.Today, currencyConverter.DateForRate);
				AssertEquals(ZArchitecture.Core.ExchangeRateType.Customs, currencyConverter.RateType);
			});
		}

		public void TestCredentialsKey()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";

			var header = GetNewBusinessObject() as AsycudaManifestHeader;

			header.AMA_CustomsProfile = "PR1";
			header.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321ABC");

			AssertEquals("HYECMT.GB987654321ABC.PR1", header.CredentialsKey);
		}

		public void TestSupervisingOffice()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_Declarant = ZGuid.Empty;

			var declarantOrg = Factory.New<OrgHeader>();
			var declarant = declarantOrg.Addresses.AddNew();

			var relatedPartyOrg = Factory.New<OrgHeader>();
			var relatedPartyAddress = relatedPartyOrg.Addresses.AddNew();

			CombineAssertions(() =>
			{
				AssertNull("Supervising Office is null when Declarant is not specified", header.SupervisingOffice);
				AssertNull("Supervising Office Address is null when Declarant is not specified", header.SupervisingOfficeAddress);
				AssertEquals("Supervising Office Address PK is null when Declarant is not specified", ZGuid.Empty, header.SupervisingOfficeAddressPK);

				header.AMA_OA_Declarant = declarant.PK;
				AssertNull("Supervising Office is null when Declarant has no related party", header.SupervisingOffice);

				var relatedParty = declarantOrg.AllRelatedParties.AddNew();
				relatedParty.PR_OH_RelatedParty = relatedPartyOrg.PK;
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.NotifyParty;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
				AssertNull("Supervising Office is null when Declarant has no related party with COF type", header.SupervisingOffice);

				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CustomsOffice;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;
				AssertNull("Supervising Office is null when Declarant has no related party with FWD direction", header.SupervisingOffice);

				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CustomsOffice;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
				AssertEquals("Expected Supervising Office when Declarant has related party with COF type and FWD direction", relatedPartyOrg, header.SupervisingOffice);
				AssertEquals("Expected Supervising Office Address when Declarant has related party with COF type and FWD direction", relatedPartyOrg.MainAddress, header.SupervisingOfficeAddress);
				AssertEquals("Expected Supervising Office Address PK when Declarant has related party with COF type and FWD direction", relatedPartyOrg.MainAddress.PK, header.SupervisingOfficeAddressPK);
			});
		}

		public void TestSupervisingOfficeAddressPK_ZAddress()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var zAddress = header.SupervisingOfficeAddressPK_ZAddress;

			CombineAssertions(() =>
			{
				AssertNotNull("ZAddress should be created", zAddress);
				AssertSame("Should return the same ZAddress", zAddress, header.SupervisingOfficeAddressPK_ZAddress);
			});
		}

		public void TestSupervisingOfficeAddressPKInfo()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var addressInfo = header.SupervisingOfficeAddressPKInfo;

			CombineAssertions(() =>
			{
				var resourceStringDataAttribute = addressInfo.GetAttribute<ResourceStringDataAttribute>();
				AssertEquals("Caption", "Supervising Office", resourceStringDataAttribute.Caption);

				var readonlyAttribute = addressInfo.GetAttribute<ReadOnlyAttribute>();
				AssertEquals("Readonly", true, readonlyAttribute.IsReadOnly);

				var listAttribute = addressInfo.GetAttribute<ListAttribute>();
				AssertEquals("List", "Lookups.SupervisingOfficeList", listAttribute.ListDataSourceMember);

				var relatedBusinessObjectAttribute = addressInfo.GetAttribute<RelatedBusinessObjectAttribute>();
				AssertEquals("RelatedBusinessObject", "SupervisingOfficeAddress", relatedBusinessObjectAttribute.RelatedBizObjName);
			});
		}

		public void TestBIRDSPermitHeader()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;

			var orgHeader = Factory.New<OrgHeader>();
			var declarant1 = orgHeader.Addresses.AddNew();
			declarant1.OA_Address1 = "Test Address 1";
			var declarant2 = orgHeader.Addresses.AddNew();
			declarant2.OA_Address1 = "Test Address 2";

			var permit = Factory.New<CusPermitHeader>();
			permit.CPH_OA_AppliesTo = declarant1.PK;
			permit.CPH_Type = "BRD";

			CombineAssertions(() =>
			{
				AssertNull("The declarant is null", header.BIRDSPermitHeader);

				header.AMA_OA_Declarant = declarant2.PK;
				AssertNull("The declarant is not the authorisation address.", header.BIRDSPermitHeader);

				header.AMA_OA_Declarant = declarant1.PK;
				AssertEquals("The declarant is the authorisation address.", permit, header.BIRDSPermitHeader);
			});
		}

		public void TestRegistrationStatus_MultipleStatusCode()
		{
			var header = GetNewBusinessObject() as AsycudaManifestHeader;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillStatus = "1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillStatus = "2";

			AssertEquals("MLT", header.RegistrationStatus);
		}

		void CreatePassword(GlbCompany company, ZString badge, ZString status, ZDateTime issueDate, ZDateTime? expiryDate = null)
		{
			var collection = GBGlbCompanyWrapper.GetWrapper<GBGlbCompanyWrapper>(company).GBBPasswordCollection;
			var pwd = collection.AddNew();

			pwd.Badge = badge;
			pwd.Status = status;
			pwd.GP_IssueDate = issueDate;
			pwd.GP_ExpiryDate = expiryDate ?? issueDate.AddDays(10);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<AsycudaManifestHeader>();
	}
}

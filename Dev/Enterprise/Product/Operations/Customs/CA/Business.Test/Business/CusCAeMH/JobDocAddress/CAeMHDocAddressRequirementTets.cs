using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAeMHDocAddressRequirementTets : TestCaseWithFactory
	{
		public void TestCheckE2_AddressType()
		{
			var house = Factory.New<CusCAeMHMaster>().HouseBills.AddNew();
			var addressWrapper1 = house.DocAddresses.AddNew();
			addressWrapper1.E2_AddressType = ZString.Empty;
			AssertHasErrorContaining(addressWrapper1.E2_AddressTypeInfo, MandatoryValidation.MustBeEntered);
			addressWrapper1.E2_AddressType = "XXX";
			AssertHasErrorContaining(addressWrapper1.E2_AddressTypeInfo, ListValidation.InvalidCodeError);
			addressWrapper1.E2_AddressType = DocAddressTypes.Codes.Carrier;
			var addressWrapper2 = house.DocAddresses.AddNew();
			addressWrapper2.E2_AddressType = DocAddressTypes.Codes.Carrier;
			AssertHasError(addressWrapper2.E2_AddressTypeInfo, string.Format(CAeMHDocAddressRequirement.MultipleAddressTypesAreNotAllowed, "Carrier"));
		}

		public void TestValidateE2_CompanyName()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.PlaceOfConsolidation;
			docAddress.E2_AddressOverride = true;
			docAddress.Validation.ValidateE2_CompanyName();
			AssertHasMessageErrorContaining(docAddress.E2_CompanyNameInfo, MandatoryValidation.YouHaveNotEntered);
			docAddress.E2_CompanyName = "COMPANY";
			AssertNoMessageErrorContaining(docAddress.E2_CompanyNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestValidateE2_Address1()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.PlaceOfConsolidation;
			docAddress.E2_AddressOverride = true;
			docAddress.Validation.ValidateE2_Address1();
			AssertHasMessageErrorContaining(docAddress.E2_Address1Info, MandatoryValidation.YouHaveNotEntered);
			docAddress.E2_Address1 = "ADDRESS 1";
			AssertNoMessageErrorContaining(docAddress.E2_Address1Info, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestValidateE2_City()
		{
			var addressTypes = new[]
			{
				DocAddressTypes.Codes.PlaceOfConsolidation,
				DocAddressTypes.Codes.Consolidator,
				DocAddressTypes.Codes.ConsigneeDocumentaryAddress,
				DocAddressTypes.Codes.ConsignorDocumentaryAddress,
				DocAddressTypes.Codes.ConsigneePickupDeliveryAddress,
				DocAddressTypes.Codes.NotifyParty,
				DocAddressTypes.Codes.ImportBroker,
				DocAddressTypes.Codes.ReceivingForwarderAddress,
				DocAddressTypes.Codes.Carrier,
				DocAddressTypes.Codes.Warehouse,
			};
			foreach (var addressType in addressTypes)
			{
				docAddress.E2_AddressType = addressType;
				docAddress.E2_AddressOverride = true;
				docAddress.E2_City = ZString.Empty;
				docAddress.Validation.ValidateE2_City();
				AssertHasMessageErrorContaining(docAddress.E2_CityInfo, MandatoryValidation.YouHaveNotEntered);
				docAddress.E2_City = "CITY";
				AssertNoMessageErrorContaining(docAddress.E2_CityInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestValidateE2_RN_NKCoutryCode()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.PlaceOfConsolidation;
			docAddress.E2_AddressOverride = true;
			docAddress.Validation.ValidateE2_RN_NKCountryCode();
			AssertNoMessageErrorContaining(docAddress.E2_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			docAddress.E2_RN_NKCountryCode = "";
			AssertHasMessageErrorContaining(docAddress.E2_RN_NKCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestGetGovRegTypes()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.Carrier;
			AssertEquals(1, docAddress.Lookups.GovRegNumTypes.Count);
			Assert(docAddress.Lookups.GovRegNumTypes.ContainsCode(OrgCusCode.CodeTypes.CarrierCode));
		}

		public void TestGetRegNumResult()
		{
			var org = Factory.New<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "XXX");
			docAddress.E2_AddressType = DocAddressTypes.Codes.Carrier;
			docAddress.OrganisationPK = org.PK;
			AssertEquals(OrgCusCode.CodeTypes.CarrierCode, docAddress.E2_GovRegNumType);
			AssertEquals("XXX", docAddress.E2_GovRegNum);

			var orgAddr1 = org.Addresses.AddNew();
			var orgAddr2 = org.Addresses.AddNew();
			var orgCusCode1 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "XXX1");
			var orgCusCode2 = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "XXX2");
			docAddress.E2_AddressType = DocAddressTypes.Codes.Warehouse;
			docAddress.OrganisationPK = org.PK;
			docAddress.E2_OA_Address = orgAddr2.PK;
			AssertEquals(OrgCusCode.CodeTypes.ControlledPremisesID, docAddress.E2_GovRegNumType);
			AssertEquals("XXX1", docAddress.E2_GovRegNum);
			orgCusCode1.OK_OA_PremisesAddress = orgAddr1.PK;
			orgCusCode2.OK_OA_PremisesAddress = orgAddr2.PK;
			AssertEquals(OrgCusCode.CodeTypes.ControlledPremisesID, docAddress.E2_GovRegNumType);
			AssertEquals("XXX2", docAddress.E2_GovRegNum);
			orgCusCode2.OK_OA_PremisesAddress = orgAddr1.PK;
			AssertEquals(OrgCusCode.CodeTypes.ControlledPremisesID, docAddress.E2_GovRegNumType);
			AssertEquals(ZString.Empty, docAddress.E2_GovRegNum);
		}

		public void TestValidateE2_GovRegNumType()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.Carrier;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_GovRegNumType = "XXX";
			AssertHasMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, ListValidation.InvalidCodeMessageError);
			docAddress.E2_GovRegNumType = ZString.Empty;
			AssertHasMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, MandatoryValidation.YouHaveNotEntered);
			docAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			AssertNoMessageErrorContaining(docAddress.E2_GovRegNumTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestValidateE2_GovRegNum()
		{
			docAddress.E2_AddressType = DocAddressTypes.Codes.Carrier;
			docAddress.E2_AddressOverride = true;
			docAddress.Validation.ValidateE2_GovRegNum();
			AssertHasMessageErrorContaining(docAddress.E2_GovRegNumInfo, MandatoryValidation.YouHaveNotEntered);
			docAddress.E2_GovRegNum = "EEEE";
			AssertNoMessageErrorContaining(docAddress.E2_GovRegNumInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestValidateEnglishStrictCharacters()
		{
			var engHeader = Factory.New<OrgHeader>();
			engHeader.OH_Code = "ENGORG";
			engHeader.OH_FullName = "TEST ENG COMPANY";
			engHeader.MainAddress.OA_Address1 = "TEST ENG ADDRESS 1";
			engHeader.MainAddress.OA_Address2 = "TEST ENG ADDRESS 2";
			engHeader.MainAddress.OA_PostCode = "12345A";
			engHeader.MainAddress.OA_City = "TEST City";
			engHeader.OH_Language = Core.SharedConstants.Languages.EnglishAmerican;

			var frnHeader = Factory.New<OrgHeader>();
			frnHeader.OH_Code = "FREORG";
			frnHeader.OH_FullName = "TEST FRÉ COMPÄNY";
			frnHeader.MainAddress.OA_Address1 = "TêST FRÈ àDDRESS 1";
			frnHeader.MainAddress.OA_Address2 = "TëST FRE ÀDDRESS 2";
			frnHeader.MainAddress.OA_PostCode = "12345Ä";
			frnHeader.MainAddress.OA_City = "TEST CÏTY";
			frnHeader.OH_Language = Core.SharedConstants.Languages.French;

			var chsHeader = Factory.New<OrgHeader>();
			chsHeader.OH_Code = "测试company";
			chsHeader.OH_FullName = "测试 chinese company";
			chsHeader.MainAddress.OA_Address1 = "测试 chinese address 1";
			chsHeader.MainAddress.OA_Address2 = "测试 chinese address 2";
			chsHeader.MainAddress.OA_PostCode = "测试 12345";
			chsHeader.MainAddress.OA_City = "测试 city";
			chsHeader.OH_Language = Core.SharedConstants.Languages.ChineseSimplified;

			docAddress.E2_AddressType = DocAddressTypes.Codes.PlaceOfConsolidation;
			docAddress.OrganisationPK = engHeader.PK;
			AssertNoMessageErrors(docAddress.OrganisationPKInfo);

			docAddress.OrganisationPK = frnHeader.PK;
			AssertNoMessageErrors(docAddress.OrganisationPKInfo);

			docAddress.OrganisationPK = chsHeader.PK;
			AssertHasMessageErrorContaining(docAddress.OrganisationPKInfo, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_CompanyNameInfo));
			AssertHasMessageErrorContaining(docAddress.OrganisationPKInfo, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_Address1Info));
			AssertHasMessageErrorContaining(docAddress.OrganisationPKInfo, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_Address2Info));
			AssertHasMessageErrorContaining(docAddress.OrganisationPKInfo, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_PostcodeInfo));
			AssertHasMessageErrorContaining(docAddress.OrganisationPKInfo, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_CityInfo));

			docAddress.OrganisationPK = ZGuid.Empty;
			docAddress.E2_AddressOverride = true;

			docAddress.E2_CompanyName = "ABCDEF";
			AssertNoMessageErrors(docAddress.E2_CompanyNameInfo);
			docAddress.E2_CompanyName = "ÄBÇDEF";
			AssertNoMessageErrors(docAddress.E2_CompanyNameInfo);
			docAddress.E2_CompanyName = "测试ABCDED";
			AssertHasMessageErrorContaining(docAddress.E2_CompanyNameInfo, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_CompanyNameInfo));

			docAddress.E2_Address1 = "ABCDEF";
			AssertNoMessageErrors(docAddress.E2_Address1Info);
			docAddress.E2_Address1 = "ÄBÇDEF";
			AssertNoMessageErrors(docAddress.E2_Address1Info);
			docAddress.E2_Address1 = "测试ABCDED";
			AssertHasMessageErrorContaining(docAddress.E2_Address1Info, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_Address1Info));

			docAddress.E2_Address2 = "ABCDEF";
			AssertNoMessageErrors(docAddress.E2_Address2Info);
			docAddress.E2_Address2 = "ÄBÇDEF";
			AssertNoMessageErrors(docAddress.E2_Address2Info);
			docAddress.E2_Address2 = "测试ABCDED";
			AssertHasMessageErrorContaining(docAddress.E2_Address2Info, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_Address2Info));

			docAddress.E2_Postcode = "12345A";
			AssertNoMessageErrors(docAddress.E2_PostcodeInfo);
			docAddress.E2_Postcode = "12345Ä";
			AssertNoMessageErrors(docAddress.E2_PostcodeInfo);
			docAddress.E2_Postcode = "测试ABCDED";
			AssertHasMessageErrorContaining(docAddress.E2_PostcodeInfo, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_PostcodeInfo));

			docAddress.E2_City = "ABCDEF";
			AssertNoMessageErrors(docAddress.E2_CityInfo);
			docAddress.E2_City = "ÄBÇDEF";
			AssertNoMessageErrors(docAddress.E2_CityInfo);
			docAddress.E2_City = "测试ABCDED";
			AssertHasMessageErrorContaining(docAddress.E2_CityInfo, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_CityInfo));

			docAddress.E2_Contact = "ABCDEF";
			AssertNoMessageErrors(docAddress.E2_ContactInfo);
			docAddress.E2_Contact = "ÄBÇDEF";
			AssertNoMessageErrors(docAddress.E2_ContactInfo);
			docAddress.E2_Contact = "测试ABCDED";
			AssertHasMessageErrorContaining(docAddress.E2_ContactInfo, EnglishAddressCharactersValidation.GetNotificationMessage(docAddress.E2_ContactInfo));
		}

		JobDocAddressDependentCollection DocAddresses
		{
			get { return fDocAddresses ?? (fDocAddresses = new JobDocAddressDependentCollection(new JobDocAddressParentForTesting(Factory))); }
		}
		JobDocAddressDependentCollection fDocAddresses;

		JobDocAddress docAddress;
		protected override void SetUp()
		{
			base.SetUp();
			docAddress = DocAddresses.CreateWithRequirement(new CAeMHDocAddressRequirement(Factory));
		}
	}
}

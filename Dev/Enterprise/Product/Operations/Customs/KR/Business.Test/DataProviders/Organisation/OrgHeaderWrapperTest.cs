using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(OrgHeaderWrapper))]
	sealed class OrgHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNullIsAllowed()
		{
			AssertNoExceptionThrown(() => OrgHeaderWrapper.New(null));
		}

		public void TestCachedValueUsed()
		{
			var organisation = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(organisation);
			var wrapper2 = OrgHeaderWrapper.New(organisation);
			AssertEquals("A wrapper for an instance of organisation or orgaddress should be instantiated only once per factory", wrapper, wrapper2);
		}

		public void TestCompanyName()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "(주)동부제철";
			AssertEquals("(주)동부제철", organisation.OH_FullName);

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = organisation.PK;
			orgAddress.OA_CompanyNameOverride = "(주)서부제철";
			orgAddress.OA_RN_NKCountryCode = "KR";
			AssertEquals("(주)서부제철", orgAddress.CompanyName);
		}

		public void TestPremiseAddressRequired()
		{
			var requiredMessage = "An address is required for code type";
			var organisation = Factory.New<OrgHeader>();
			var orgCusCode = organisation.CustomsCodes.AddNew();
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			orgCusCode.OK_CodeType = Constants.IdentificationType.BuildingNumber;
			orgCusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasErrorContaining(orgCusCode.OK_OA_PremisesAddressInfo, requiredMessage);

			orgCusCode.OK_CodeType = Constants.IdentificationType.BusinessRegNo;
			orgCusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertNoErrorContaining(orgCusCode.OK_OA_PremisesAddressInfo, requiredMessage);

			orgCusCode.OK_CodeType = Constants.IdentificationType.RoadNameCode;
			orgCusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasErrorContaining(orgCusCode.OK_OA_PremisesAddressInfo, requiredMessage);

			orgCusCode.OK_CodeType = Constants.IdentificationType.KoreanRegNoForResident;
			orgCusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertNoErrorContaining(orgCusCode.OK_OA_PremisesAddressInfo, requiredMessage);

			orgCusCode.OK_CodeType = MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.IndustrialParkCode;
			orgCusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasErrorContaining(orgCusCode.OK_OA_PremisesAddressInfo, requiredMessage);

			orgCusCode.OK_CodeType = Constants.IdentificationType.KoreanRegNoForForeigner;
			orgCusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertNoErrorContaining(orgCusCode.OK_OA_PremisesAddressInfo, requiredMessage);

			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertHasErrorContaining(orgCusCode.OK_OA_PremisesAddressInfo, requiredMessage);

			orgCusCode.OK_CodeType = Constants.IdentificationType.PassportNo;
			orgCusCode.OK_OA_PremisesAddress = ZGuid.Empty;
			AssertNoErrorContaining(orgCusCode.OK_OA_PremisesAddressInfo, requiredMessage);
		}
		public void TestRoadNameCodeAndBuildingNumber()
		{
			var organisation = Factory.New<OrgHeader>();
			var orgAddress = organisation.MainAddress;
			orgAddress.CustomsCodes.AddNew(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.BuildingNumber, "2");
			orgAddress.CustomsCodes.AddNew(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.RoadNameCode, "2234682A");
			AssertEquals("2234682A", orgAddress.GetRoadNameCode());
			AssertEquals("2", orgAddress.GetBuildingNumber());
		}
		public void TestRepresentativeName()
		{
			var organisation = Factory.New<OrgHeader>();
			var contact1 = organisation.Contacts.AddNew();
			contact1.OC_ContactName = "이영희";
			var contact2 = organisation.Contacts.AddNew();
			contact2.OC_ContactName = "김철수";
			var allocation = contact2.Allocations.AddNew();
			allocation.PC_Type = OrgConstants.ContactAllocationType.CEOForKRCustoms;
			AssertEquals("김철수", organisation.GetRepresentativeName());

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = organisation.PK;
			orgAddress.OA_RN_NKCountryCode = "KR";
			AssertEquals("김철수", orgAddress.Header.GetRepresentativeName());
		}

		public void TestGetIDNumbers()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "Test Company";
			organisation.OH_Category = OrgConstants.Category.Business;
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = IdentificationType.BusinessRegNo;
			cusCode.OK_CustomsRegNo = "SUP000";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var cusCode1 = organisation.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = IdentificationType.KoreanRegNoForForeigner;
			cusCode1.OK_CustomsRegNo = "SUP111";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var cusCode2 = organisation.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = IdentificationType.ForeignCompanyID;
			cusCode2.OK_CustomsRegNo = "SUP112";
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var idNumbers = organisation.GetRegistrationIDNumbers(new string[] { IdentificationType.BusinessRegNo, IdentificationType.KoreanRegNoForForeigner });

			AssertEquals(2, idNumbers.Length);
			AssertEquals(IdentificationType.BusinessRegNo, idNumbers[0].Type);
			AssertEquals("SUP000", idNumbers[0].Number);

			AssertEquals(IdentificationType.KoreanRegNoForForeigner, idNumbers[1].Type);
			AssertEquals("SUP111", idNumbers[1].Number);

			cusCode.OK_CustomsRegNo = "SUP000+test";
			cusCode1.OK_CustomsRegNo = "SUP111+test";
			Factory.Save();
			idNumbers = organisation.GetRegistrationIDNumbers(new string[] { IdentificationType.BusinessRegNo, IdentificationType.KoreanRegNoForForeigner });

			AssertEquals("SUP000+test", idNumbers[0].Number);
			AssertEquals("SUP111+test", idNumbers[1].Number);

			var idNumber = organisation.GetRegistrationIDNumbers(new string[] { IdentificationType.ForeignCompanyID });

			AssertEquals(1, idNumber.Length);
			AssertEquals(IdentificationType.ForeignCompanyID, idNumber[0].Type);
			AssertEquals("SUP112", idNumber[0].Number);
		}

		public void TestGetIDNumbersIncludingBusinessIDOrIndividualID()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Category = OrgConstants.Category.Business;
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = IdentificationType.BusinessRegNo;
			cusCode.OK_CustomsRegNo = "SUP000";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var cusCode1 = organisation.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = IdentificationType.KoreanRegNoForForeigner;
			cusCode1.OK_CustomsRegNo = "SUP111";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var cusCode2 = organisation.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = IdentificationType.ForeignCompanyID;
			cusCode2.OK_CustomsRegNo = "SUP112";
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var idNumbers = organisation.GetRegistrationIDNumbersAndFirstMatchedBusinessOrIndividualID(new string[] { IdentificationType.ForeignCompanyID });

			AssertEquals(2, idNumbers.Length);
			AssertEquals(IdentificationType.ForeignCompanyID, idNumbers[0].Type);
			AssertEquals("SUP112", idNumbers[0].Number);

			AssertEquals(IdentificationType.BusinessRegNo, idNumbers[1].Type);
			AssertEquals("SUP000", idNumbers[1].Number);

			idNumbers = organisation.GetRegistrationIDNumbersAndFirstMatchedBusinessOrIndividualID(new string[] { IdentificationType.ForeignCompanyID, IdentificationType.BusinessRegNo });

			AssertEquals("GCR is not added twice", 2, idNumbers.Length);
			AssertEquals(IdentificationType.ForeignCompanyID, idNumbers[0].Type);
			AssertEquals("SUP112", idNumbers[0].Number);

			AssertEquals(IdentificationType.BusinessRegNo, idNumbers[1].Type);
			AssertEquals("SUP000", idNumbers[1].Number);

			organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			idNumbers = organisation.GetRegistrationIDNumbersAndFirstMatchedBusinessOrIndividualID(new string[] { IdentificationType.ForeignCompanyID, IdentificationType.KoreanRegNoForForeigner });
			AssertEquals(2, idNumbers.Length);
			AssertEquals(IdentificationType.ForeignCompanyID, idNumbers[0].Type);
			AssertEquals("SUP112", idNumbers[0].Number);

			AssertEquals(IdentificationType.KoreanRegNoForForeigner, idNumbers[1].Type);
			AssertEquals("SUP111", idNumbers[1].Number);
		}

		public void TestGetFirstMatchedBusinessOrIndividualID()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Category = OrgConstants.Category.Business;
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = IdentificationType.BusinessRegNo;
			cusCode.OK_CustomsRegNo = "SUP000";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var cusCode1 = organisation.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = IdentificationType.KoreanRegNoForForeigner;
			cusCode1.OK_CustomsRegNo = "SUP111";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var cusCode2 = organisation.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = IdentificationType.ForeignCompanyID;
			cusCode2.OK_CustomsRegNo = "SUP112";
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var result = organisation.GetRegistrationFirstMatchedBusinessOrIndividualIDConverted();
			AssertEquals(IdentificationType.BusinessRegNo, result.Type);
			AssertEquals("SUP000", result.Number);

			organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			result = organisation.GetRegistrationFirstMatchedBusinessOrIndividualIDConverted();
			AssertEquals(IdentificationType.KoreanRegNoForForeigner, result.Type);
			AssertEquals("SUP111", result.Number);
		}

		public void TestGetIdNumberAndType()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Category = OrgConstants.Category.Business;
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = IdentificationType.BusinessRegNo;
			cusCode.OK_CustomsRegNo = "SUP000";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var cusCode1 = organisation.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = IdentificationType.KoreanRegNoForForeigner;
			cusCode1.OK_CustomsRegNo = "SUP111";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var cusCode2 = organisation.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = IdentificationType.ForeignCompanyID;
			cusCode2.OK_CustomsRegNo = "SUP112";
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.KoreaSouth;

			var idNumber = organisation.GetRegistrationIDNumber(IdentificationType.BusinessRegNo);

			AssertEquals(IdentificationType.BusinessRegNo, idNumber.Type);
			AssertEquals("SUP000", idNumber.Number);
		}

		public void TestBusinessRegNoOrIndividualIDForOrgHeader()
		{
			var organisation = Factory.New<OrgHeader>();
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = IdentificationType.BusinessRegNo;
			cusCode.OK_CustomsRegNo = "SUP000";

			var cusCode1 = organisation.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = IdentificationType.KoreanRegNoForForeigner;
			cusCode1.OK_CustomsRegNo = "SUP111";

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = organisation.PK;
			orgAddress.OA_RN_NKCountryCode = "KR";
			AssertEquals("SUP000", organisation.GetRegistrationFirstMatchedBusinessOrIndividualID());

			organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			AssertEquals("SUP111", organisation.GetRegistrationFirstMatchedBusinessOrIndividualID());
		}

		public void TestBusinessRegNoOrIndividualIDForOrgAddress()
		{
			var organisation = Factory.New<OrgHeader>();

			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = IdentificationType.BusinessRegNo;
			cusCode.OK_CustomsRegNo = "SUP000";

			var cusCode1 = organisation.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = IdentificationType.KoreanRegNoForForeigner;
			cusCode1.OK_CustomsRegNo = "SUP111";

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = organisation.PK;
			orgAddress.OA_RN_NKCountryCode = "KR";

			AssertEquals("SUP000", orgAddress.GetRegistrationFirstMatchedBusinessOrIndividualIDConverted().Number);

			organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			AssertEquals("SUP111", orgAddress.GetRegistrationFirstMatchedBusinessOrIndividualIDConverted().Number);
		}

		public void TestIsIndividual()
		{
			var organisation = Factory.New<OrgHeader>();
			AssertEquals(false, organisation.GetIsIndividual());

			organisation.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			AssertEquals(true, organisation.GetIsIndividual());

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = organisation.PK;
			orgAddress.OA_RN_NKCountryCode = "KR";
			AssertEquals(true, orgAddress.Header.GetIsIndividual());

			organisation.OH_Category = OrgConstants.Category.Business;
			AssertEquals(false, orgAddress.Header.GetIsIndividual());
		}

		public void TestAddressDetails()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.Address1 = "서울시 중구 동성로 2";
			organisation.MainAddress.Address2 = "신흥빌딩 3호";
			organisation.MainAddress.OA_RN_NKCountryCode = "KR";
			AssertEquals("서울시 중구 동성로 2 신흥빌딩 3호", organisation.MainAddress.GetAddressDetails());

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = organisation.PK;
			orgAddress.Address1 = "서울시 중구 동성로 1";
			orgAddress.Address2 = "신흥빌딩 2호";
			orgAddress.OA_RN_NKCountryCode = "KR";
			AssertEquals("서울시 중구 동성로 1 신흥빌딩 2호", orgAddress.GetAddressDetails());
		}

		public void TestAddressPostcode()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.Postcode = "35682";
			organisation.MainAddress.OA_RN_NKCountryCode = "KR";
			AssertEquals("356-82", organisation.MainAddress.GetFormattedPostcode());

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = organisation.PK;
			orgAddress.Postcode = "35681";
			orgAddress.OA_RN_NKCountryCode = "KR";
			AssertEquals("356-81", orgAddress.GetFormattedPostcode());
		}

		public void TestCusCodesByIdentificationType()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForResident, "1234561234567");
			organisation.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForForeigner, "B2");
			organisation.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "0123456789");
			organisation.CustomsCodes.AddNew(IdentificationType.PassportNo, "D4");
			organisation.CustomsCodes.AddNew(IdentificationType.UnipassIDForIndividual, "E5");
			organisation.CustomsCodes.AddNew(IdentificationType.UnipassIDForOrganization, "크리스챤1971015");
			organisation.CustomsCodes.AddNew(IdentificationType.ForeignCompanyID, "G7");
			organisation.CustomsCodes.AddNew(IdentificationType.OfficeID, "H8");
			organisation.CustomsCodes.AddNew(IdentificationType.CorporationCode, "1234561234567");
			organisation.CustomsCodes.AddNew(IdentificationType.CertificateOfOriginExporterNumber, "J10");
			AssertEquals("123456-1234567", MessageFunctions.GetFormattedNumber(organisation.GetRegistrationIDNumber(IdentificationType.KoreanRegNoForResident).Number.ToString(), new int[] { 0, 6 }));
			AssertEquals("B2", organisation.GetRegistrationIDNumber(IdentificationType.KoreanRegNoForForeigner)?.Number);
			AssertEquals("012-34-56789", organisation.GetRegistrationNumberFormattedIfRequired(IdentificationType.BusinessRegNo));
			AssertEquals("D4", organisation.GetRegistrationIDNumber(IdentificationType.PassportNo)?.Number ?? ZString.Empty);
			AssertEquals("E5", organisation.GetRegistrationIDNumber(IdentificationType.UnipassIDForIndividual)?.Number ?? ZString.Empty);
			AssertEquals("크리스챤1971015", organisation.GetRegistrationIDNumber(IdentificationType.UnipassIDForOrganization)?.Number ?? ZString.Empty);
			AssertEquals("크리스챤-1-97-1-01-5", organisation.GetRegistrationNumberFormattedIfRequired(IdentificationType.UnipassIDForOrganization));
			AssertEquals("G7", organisation.GetRegistrationNumberFormattedIfRequired(IdentificationType.ForeignCompanyID));
			AssertEquals("H8", organisation.GetRegistrationNumberFormattedIfRequired(IdentificationType.OfficeID));
			AssertEquals("123456-1234567", MessageFunctions.GetFormattedNumber(organisation.GetRegistrationIDNumber(IdentificationType.CorporationCode).Number.ToString(), new int[] { 0, 6 }));
			AssertEquals("J10", organisation.GetRegistrationIDNumber(IdentificationType.CertificateOfOriginExporterNumber)?.Number);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			return OrgHeaderWrapper.New(org);
		}
	}
}

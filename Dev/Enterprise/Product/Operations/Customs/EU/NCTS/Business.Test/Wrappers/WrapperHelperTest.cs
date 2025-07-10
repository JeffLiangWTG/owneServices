using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class WrapperHelperTest : TestCaseWithFactory
	{
		[TestDate(2020, 12, 18)]
		public void TestGetLongDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Valid Date", "20201218", ZDateTime.Now.GetLongDate());
				AssertEquals("Invalid Date", ZString.Empty, ZDateTime.Empty.GetLongDate());
			});
		}

		[TestDate(2020, 12, 18, 10, 48, 23)]
		public void TestGetLongDateAndTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Valid Date", "202012181048", ZDateTime.Now.GetLongDateAndTime());
				AssertEquals("Invalid Date", ZString.Empty, ZDateTime.Empty.GetLongDateAndTime());
			});
		}

		public void TestArrivalDeclarantTIN_Declarant_Null()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			AssertEquals(ZString.Empty, nctsHeader.ArrivalDeclarantTIN());
		}

		public void TestArrivalDeclarantTIN_Declarant()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "1111111111";
			nctsHeader.Declarant.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR1111111111", nctsHeader.ArrivalDeclarantTIN());
		}

		public void TestArrivalDeclarantTIN_Consignee_Null()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.Declarant.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertEquals(ZString.Empty, nctsHeader.ArrivalDeclarantTIN());
		}

		public void TestArrivalDeclarantTIN_Consignee()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "2222222222";
			nctsHeader.Consignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR2222222222", nctsHeader.ArrivalDeclarantTIN());
		}

		public void TestDepartureDeclarantTIN_Declarant_Null()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			AssertEquals(ZString.Empty, nctsHeader.DepartureDeclarantTIN());
		}

		public void TestDepartureDeclarantTIN_Declarant()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "3333333333";
			nctsHeader.Declarant.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR3333333333", nctsHeader.DepartureDeclarantTIN());
		}

		public void TestDepartureDeclarantTIN_Consignor_Null()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.Declarant.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			AssertEquals(ZString.Empty, nctsHeader.DepartureDeclarantTIN());
		}

		public void TestDeclarantTIN_Consignor()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "4444444444";
			nctsHeader.Consignor.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR4444444444", nctsHeader.DepartureDeclarantTIN());
		}

		public void TestDeparturePrincipalTIN_Principal_Null()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			AssertEquals(ZString.Empty, nctsHeader.DeparturePrincipalTIN());
		}

		public void TestDeparturePrincipalTIN()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "5555555555";
			nctsHeader.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR5555555555", nctsHeader.DeparturePrincipalTIN());
		}

		public void TestAuthorisedConsigneeTIN()
		{
			var cnr = Factory.NewWithValidTestData<OrgHeader>();
			var cne = Factory.NewWithValidTestData<OrgHeader>();

			var cnrPk = cnr.MainAddress.PK;
			var cnePk = cne.MainAddress.PK;

			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			var org = Factory.New<OrgHeader>();
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();

			var address2 = org.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "FRNIC";

			var consignee = nctsHeader.Consignee;
			consignee.OrganisationPK = cnr.PK;

			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "4444444444";

			authorizationHeader.CPH_OH_PermitHolder = cnr.PK;
			authorizationHeader.CPH_OA_AppliesTo = cnrPk;
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			authorizationHeader.CPH_Number = "4444444444";

			nctsHeader.Consignee.E2_OA_Address = cnrPk;
			AssertEquals("4444444444", nctsHeader.AuthorisedConsigneeTIN());

			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;

			nctsHeader.Consignee.E2_OA_Address = cnrPk;
			AssertEquals(ZString.Empty, nctsHeader.AuthorisedConsigneeTIN());

			authorizationHeader.CPH_OA_AppliesTo = cnePk;
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;

			nctsHeader.Consignee.E2_OA_Address = cnrPk;
			AssertEquals(ZString.Empty, nctsHeader.AuthorisedConsigneeTIN());

			authorizationHeader.CPH_OH_PermitHolder = ZGuid.Empty;
			authorizationHeader.CPH_OA_AppliesTo = cnrPk;
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;

			nctsHeader.Consignee.E2_OA_Address = cnrPk;
			AssertEquals(ZString.Empty, nctsHeader.AuthorisedConsigneeTIN());
		}

		public void TestSecurityCarrierEORI()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "4444444444";
			nctsHeader.BH_OH_Carrier = org.PK;
			AssertEquals("FR4444444444", nctsHeader.GetCarrierEORI());
		}
	}
}

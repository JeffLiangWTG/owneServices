using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CCF15ADeclarationWrapper))]
	class CCF15ADeclarationWrapperTest : DeclarationWrapperAbstractTest<CCF15ADeclarationWrapper>
	{
		public void TestDeclarantTIN_IsDepartureFallback()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "2222222222";
			header.Consignor.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR2222222222", wrapper.DeclarantTIN);
		}

		[TestDate(2020, 12, 16)]
		public void TestValidationDate()
		{
			AssertEquals("20201216", wrapper.ValidationDate);
		}

		public void TestPrincipalTIN_Null()
		{
			header.Principal.OrganisationPK = ZGuid.Empty;
			AssertEquals(ZString.Empty, wrapper.PrincipalTIN);
		}

		public void TestPrincipalTIN()
		{
			var organisation = Factory.New<OrgHeader>();
			header.Principal.OrganisationPK = organisation.PK;
			var code = header.Principal.Organisation.CustomsCodes.AddNew();
			code.OK_CustomsRegNo = "987654";
			code.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			AssertEquals("LV987654", wrapper.PrincipalTIN);
		}

		public void TestAuthorisedLocationOfGoodsCode()
		{
			header.MovementHeader.BM_LocationOfGoodsCode = Core.Constants.CountryCodes.Ireland;
			AssertEquals(Core.Constants.CountryCodes.Ireland, wrapper.AuthorisedLocationOfGoodsCode);
		}

		public void TestAgreedLocationOfGoods()
		{
			header.MovementHeader.BM_LocationOfGoods = Core.Constants.CountryCodes.AlandIslands;
			AssertEquals(Core.Constants.CountryCodes.AlandIslands, wrapper.AgreedLocationOfGoods);
		}

		public void TestAgreedLocationOfGoodsLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.AgreedLocationOfGoodsLanguage);
		}

		public void TestIdentityOfMeansOfTransportAtDeparture()
		{
			header.MovementHeader.BM_TransportAtDeparture = "BJW61Z";
			AssertEquals("BJW61Z", wrapper.IdentityOfMeansOfTransportAtDeparture);
		}

		public void TestIdentityOfMeansOfTransportAtDepartureLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.IdentityOfMeansOfTransportAtDepartureLanguage);
		}

		public void TestNationalityOfMeansOfTransportAtDeparture()
		{
			header.MovementHeader.BM_RN_NKTransportAtDepartureCountry = Core.Constants.CountryCodes.Italy;
			AssertEquals(Core.Constants.CountryCodes.Italy, wrapper.NationalityOfMeansOfTransportAtDeparture);
		}

		[TestDate(2020, 12, 15)]
		public void TestControlResultDateLimit()
		{
			header.MovementHeader.BM_ExportDate = ZDateTime.Now;
			AssertEquals("20201215", wrapper.ControlResultDateLimit);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			wrapper = new CCF15ADeclarationWrapper(header);
		}
		CCF15ADeclarationWrapper wrapper;
		NctsHeader header;

		protected override CCF15ADeclarationWrapper GetProvider() => wrapper;
	}
}

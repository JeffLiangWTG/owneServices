using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusAuthorisation.Testing
{
	public class CusAuthorisationHeaderExtensionsTest : TestCaseWithFactory
	{
		public void TestIsSpecificRegime()
		{
			var address = Factory.New<OrgAddress>();
			var frAuthorisation = address.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_FR");
			var authorizationHeaderTypeList = new CusAuthorizationHeaderTypeList();
			foreach (var authorizationHeaderType in authorizationHeaderTypeList.GetAllCodes())
			{
				frAuthorisation.CPH_Type = authorizationHeaderType;
				var isTypeExpectedToBeSpecificRegime = authorizationHeaderType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP
														|| authorizationHeaderType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing
														|| authorizationHeaderType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing
														|| authorizationHeaderType == CusAuthorizationHeaderTypeList.Codes.OtherThanOpo
														|| authorizationHeaderType == Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission
														|| authorizationHeaderType == CusAuthorizationHeaderTypeList.Codes.TemporaryExportation;

				Assert(frAuthorisation.IsSpecificRegime() == isTypeExpectedToBeSpecificRegime);
			}
		}

		public void TestGetCusAuthorisationHeadersWithApplyingAddressAndType_ShouldReturnEmptyListIfNull()
		{
			AssertEquals(0, ((OrgAddress)null).GetCusAuthorisationHeadersWithApplyingAddressAndType(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, Core.Constants.CountryCodes.France).Count());
		}

		public void TestGetCusAuthorisationHeadersWithApplyingAddressAndType_ShouldGetFrenchOnly()
		{
			var address = Factory.New<OrgAddress>();

			var frAuthorisation = address.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_FR");
			var gbAuthorisation = address.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_GB").WithCountry(Core.Constants.CountryCodes.UnitedKingdom);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				frAuthorisation
			}, address.GetCusAuthorisationHeadersWithApplyingAddressAndType(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, Core.Constants.CountryCodes.France));
		}

		public void TestGetCusAuthorisationHeadersWithApplyingAddressAndType_ShouldGetThoseWithAddressMatched()
		{
			var address1 = Factory.New<OrgAddress>();
			var address2 = Factory.New<OrgAddress>();

			var authorisation1 = address1.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_1");
			var authorisation2 = address2.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_2");

			AssertContainsExactElementsInAnyOrder(new[]
			{
				authorisation1
			}, address1.GetCusAuthorisationHeadersWithApplyingAddressAndType(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, Core.Constants.CountryCodes.France));
		}

		public void TestGetCusAuthorisationHeadersWithApplyingAddressAndType_ShouldGetThoseWithTypeMatched()
		{
			var address1 = Factory.New<OrgAddress>();

			var authorisation1 = address1.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_1");
			var authorisation2 = address1.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1).WithNumber("AUTH_2");

			AssertContainsExactElementsInAnyOrder(new[]
			{
				authorisation1
			}, address1.GetCusAuthorisationHeadersWithApplyingAddressAndType(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, Core.Constants.CountryCodes.France));
		}

		public void TestGetCusAuthorisationHeadersWithApplyingAddress_ShouldReturnEmptyListIfNull()
		{
			AssertEquals(0, ((OrgAddress)null).GetCusAuthorisationHeadersWithApplyingAddress(Core.Constants.CountryCodes.France).Count());
		}

		public void TestGetCusAuthorisationHeadersWithApplyingAddress_ShouldGetFrenchOnly()
		{
			var address = Factory.New<OrgAddress>();

			var frAuthorisation = address.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_FR");
			var gbAuthorisation = address.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_GB").WithCountry(Core.Constants.CountryCodes.UnitedKingdom);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				frAuthorisation
			}, address.GetCusAuthorisationHeadersWithApplyingAddress(Core.Constants.CountryCodes.France));
		}

		public void TestGetCusAuthorisationHeadersWithApplyingAddress_ShouldGetThoseWithAddressMatched()
		{
			var address1 = Factory.New<OrgAddress>();
			var address2 = Factory.New<OrgAddress>();

			var authorisation1 = address1.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_1");
			var authorisation2 = address2.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_2");

			AssertContainsExactElementsInAnyOrder(new[]
			{
				authorisation1
			}, address1.GetCusAuthorisationHeadersWithApplyingAddress(Core.Constants.CountryCodes.France));
		}

		public void TestGetCusAuthorisationHeadersNumberWithApplyingAddressForCustomsWarehouse()
		{
			var address = Factory.New<OrgAddress>();

			var authorisation1 = address.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_1");
			var authorisation2 = address.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_2");

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AUTH_1", "AUTH_2"
			}, address.GetCusAuthorisationHeadersNumberWithApplyingAddressForCustomsWarehouse(Core.Constants.CountryCodes.France));
		}

		public void TestGetCusAuthorisationHeadersWithApplyingAddressForCustomsWarehouse_ShouldGetFrenchOnly()
		{
			var address = Factory.New<OrgAddress>();

			var frAuthorisation = address.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_FR");
			var gbAuthorisation = address.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_GB").WithCountry(Core.Constants.CountryCodes.UnitedKingdom);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				frAuthorisation
			}, address.GetCusAuthorisationHeadersWithApplyingAddressForCustomsWarehouse(Core.Constants.CountryCodes.France));
		}

		public void TestGetCusAuthorisationHeadersWithApplyingAddressForCustomsWarehouse_ShouldGetThoseWithAddressMatched()
		{
			var address1 = Factory.New<OrgAddress>();
			var address2 = Factory.New<OrgAddress>();

			var authorisation1 = address1.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_1");
			var authorisation2 = address2.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_2");

			AssertContainsExactElementsInAnyOrder(new[]
			{
				authorisation1
			}, address1.GetCusAuthorisationHeadersWithApplyingAddressForCustomsWarehouse(Core.Constants.CountryCodes.France));
		}

		public void TestGetCusAuthorisationHeadersWithApplyingAddressForCustomsWarehouse_ShouldGetWarehouseTypesOnly()
		{
			var address = Factory.New<OrgAddress>();

			var cwpAuthorisation = address.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("AUTH_CWP");
			var cw1Authorisation = address.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1).WithNumber("AUTH_CW1");
			var cw2Authorisation = address.SetupAuthorisationHeader(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2).WithNumber("AUTH_CW2");
			var aulAuthorisation = address.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation).WithNumber("AUTH_AUL");

			AssertContainsExactElementsInAnyOrder(new[]
			{
				cwpAuthorisation, cw1Authorisation, cw2Authorisation
			}, address.GetCusAuthorisationHeadersWithApplyingAddressForCustomsWarehouse(Core.Constants.CountryCodes.France));
		}
	}
}

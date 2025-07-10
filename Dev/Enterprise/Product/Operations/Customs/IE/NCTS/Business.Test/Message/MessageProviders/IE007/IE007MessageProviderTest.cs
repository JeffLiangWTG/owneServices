using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE007MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE007MessageProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE007MessageProvider(null));
			});
		}

		protected override IE007MessageProvider GetProvider() => new IE007MessageProvider(nctsHeader);

		public void TestAuthorisations() => CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, MapDirectionList.Codes.OUT, "EUNAU", true);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, "C520", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "C522", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			nctsHeader.CusAuthorizationUsages.RemoveAndDeleteAll();
			Factory.Save();

			var auth1 = nctsHeader.CusAuthorizationUsages.AddNew();
			auth1.AGC_Code = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			auth1.AGC_Number = "ABCD1234";

			var auth2 = nctsHeader.CusAuthorizationUsages.AddNew();
			auth2.AGC_Code = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure;
			auth2.AGC_Number = "WXYZ5678";

			var provider = GetProvider();
			AssertEquals("Authorisation Count", 2, provider.Authorisations.Count);
			var auths = provider.Authorisations.ToArray();

			AssertEquals("Auth 1 Type", "C522", auths[0].IdentificationType);
			AssertEquals("Auth 1 Number", "ABCD1234", auths[0].ReferenceNumber);

			AssertEquals("Auth 2 Type", "C520", auths[1].IdentificationType);
			AssertEquals("Auth 2 Number", "WXYZ5678", auths[1].ReferenceNumber);
		});

		public void TestConsignment()
		{
			var incident1 = nctsHeader.EnRouteIncidents.AddNew();
			incident1.BN_IncidentCode = "1";
			var incident2 = nctsHeader.EnRouteIncidents.AddNew();
			incident2.BN_IncidentCode = "2";
			nctsHeader.ArrivalMovementHeader.GoodsLocation.CGL_Type = "A";
			var provider = GetProvider();
			AssertEquals("Incidents Count", 2, provider.Consignment.Incidents.Count);
			AssertEquals("Location of Goods", "A", provider.Consignment.LocationOfGoods.LocationCodeType);
		}

		public void TestCustomsOfficeOfDestination()
		{
			AssertEquals(string.Empty, Provider.CustomsOfficeOfDestination);
			nctsHeader.ArrivalMovementHeader.CustomsOffices.AddNew("DSA", "IEABC222");

			var provider = GetProvider();
			AssertEquals("Office of Destination", "IEABC222", provider.CustomsOfficeOfDestination);
		}

		public void TestTraderAtDestination()
		{
			nctsHeader.DestinationTrader.OrganisationPK = traderAtDestination.PK;

			var provider = GetProvider();
			var trader = provider.TraderAtDestination;
			AssertEquals("Identification Number", "IE0123456789000", trader.IdentificationNumber);
			AssertEquals("Language at destination", "IE", trader.CommunicationLanguageAtDestination);
		}

		public void TestTransitOperation()
		{
			nctsHeader.ArrivalMrnFromUser = "IE1234567";

			nctsHeader.CusAuthorizationUsages.RemoveAll();
			var auth = nctsHeader.CusAuthorizationUsages.AddNew();
			auth.AGC_Code = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			auth.AGC_Number = "ABCD1234";

			nctsHeader.BH_ExportFlag = "Y";

			var provider = GetProvider();

			AssertNotNull(provider.TransitOperation);
			var transit = provider.TransitOperation;
			AssertEquals("MRN", "IE1234567", transit.MRN);
			AssertEquals("Simplified Procedure", ZBool.True, transit.SimplifiedProcedure);
			AssertEquals("Incident Flag", ZBool.True, transit.IncidentFlag);

			auth.AGC_Code = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure;
			nctsHeader.BH_ExportFlag = "N";

			transit = GetProvider().TransitOperation;
			AssertEquals("Simplified Procedure", ZBool.True, transit.SimplifiedProcedure);
			AssertEquals("Incident Flag", ZBool.False, transit.IncidentFlag);

			auth.AGC_Code = string.Empty;
			transit = GetProvider().TransitOperation;
			AssertEquals("Simplified Procedure", ZBool.False, transit.SimplifiedProcedure);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			traderAtDestination = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", "0123456789000", "TIR123");
			var contact = traderAtDestination.Contacts.AddNew();
			contact.OC_ContactName = "Joe Bloggs";
			contact.OC_Phone = "5551234";
			contact.OC_Email = "test@example.com";
			contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
		}

		NctsHeader nctsHeader;
		OrgHeader traderAtDestination;
	}
}

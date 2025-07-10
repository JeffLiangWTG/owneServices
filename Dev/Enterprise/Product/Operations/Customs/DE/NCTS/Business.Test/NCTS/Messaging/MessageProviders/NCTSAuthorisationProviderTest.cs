using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSAuthorisationProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSAuthorisationProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull(NCTSAuthorisationProvider.NewOrNull(null));
		}

		public void TestType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, MapDirectionList.Codes.OUT, "EUNAU", true);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "C521", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();

			AssertEquals("C521", Provider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("DE1234567", Provider.ReferenceNumber);
		}

		protected override NCTSAuthorisationProvider GetProvider() => NCTSAuthorisationProvider.NewOrNull(authorizationUsage);

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			authorizationUsage = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			authorizationUsage.AGC_Number = "DE1234567";
		}
		CusAuthorizationUsage authorizationUsage;
	}
}

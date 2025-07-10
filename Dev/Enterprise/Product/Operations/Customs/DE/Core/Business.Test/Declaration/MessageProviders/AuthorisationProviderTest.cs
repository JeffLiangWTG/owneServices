using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class AuthorisationProviderTest : Customs.Business.Testing.DataProviderTestCase<AuthorisationProvider>
	{
		public void TestType()
		{
			AssertEquals("C513", DataProvider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("Num123", DataProvider.ReferenceNumber);
		}

		public void TestHolderOfAuthorisation()
		{
			AssertEquals(string.Empty, DataProvider.HolderOfAuthorisation);
		}

		public void TestNew_CusEntryInstruction()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, MapDirectionList.Codes.OUT, "EUNAU", true);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, "C019", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "C512", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();

			var entryInstruction = Factory.New<CusEntryInstruction>();

			var authorizationUsage1 = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			authorizationUsage1.AGC_Number = "111";

			var authorizationUsage2 = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			authorizationUsage2.AGC_Number = "222";

			var dataProviders = AuthorisationProvider.New(entryInstruction);
			CombineAssertions(() =>
			{
				AssertEquals("C019", dataProviders[0].Type);
				AssertEquals("111", dataProviders[0].ReferenceNumber);
				AssertEquals("C512", dataProviders[1].Type);
				AssertEquals("222", dataProviders[1].ReferenceNumber);
			});
		}

		IAuthorisation DataProvider => dataProvider ?? (dataProvider = AuthorisationProvider.New("C513", "Num123"));
		IAuthorisation dataProvider;

		protected override AuthorisationProvider GetProvider() => (AuthorisationProvider)DataProvider;
	}
}

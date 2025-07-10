using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class AuthorisationProviderTest : Customs.Business.Testing.DataProviderTestCase<AuthorisationProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("Authorisation missing", () => new AuthorisationProvider(null));
			});
		}

		public void TestIdentificationType() => CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, MapDirectionList.Codes.OUT, "EUNAU", true);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir, "C520", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, "ACR", "C521", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit, "C522", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, "SSE", "C523", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, "TRD", "C524", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();

			SetUpTestData();

			authorisation.AGC_Number = "1";
			authorisation.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;

			var provider = GetProvider();
			AssertEquals(GetMessage(), "C520", provider.IdentificationType);

			authorisation.AGC_Code = "ACR";
			AssertEquals(GetMessage(), "C521", provider.IdentificationType);

			authorisation.AGC_Code = "ACE";
			AssertEquals(GetMessage(), "C522", provider.IdentificationType);

			authorisation.AGC_Code = "SSE";
			AssertEquals(GetMessage(), "C523", provider.IdentificationType);

			authorisation.AGC_Code = "TRD";
			AssertEquals(GetMessage(), "C524", provider.IdentificationType);

			authorisation.AGC_Code = "123";
			AssertEquals(GetMessage(), "123", provider.IdentificationType);

			string GetMessage() => $"{nameof(authorisation.AGC_Code)} = {authorisation.AGC_Code}";
		});

		public void TestReferenceNumber()
		{
			SetUpTestData();
			authorisation.AGC_Number = "123456";
			AssertEquals("123456", Provider.ReferenceNumber);
		}

		protected override AuthorisationProvider GetProvider()
		{
			SetUpTestData();
			return new AuthorisationProvider(authorisation);
		}

		void SetUpTestData()
		{
			if (authorisation == null)
			{
				authorisation = Factory.NewWithValidTestData<CusAuthorizationUsage>();
			}
		}

		CusAuthorizationUsage authorisation;
	}
}

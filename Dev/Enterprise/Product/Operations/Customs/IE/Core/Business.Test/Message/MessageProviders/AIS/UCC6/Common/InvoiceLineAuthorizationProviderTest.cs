using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class InvoiceLineAuthorizationProviderTest : DataProviderTestCase<InvoiceLineAuthorizationProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("CusAuthorizationUsage missing", () => new InvoiceLineAuthorizationProvider(null));
		}

		public void TestType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunau = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU;
			helper.CreateCusMapType(eunau, MapDirectionList.Codes.OUT, "EUNAU", true);
			helper.CreateCusMap(eunau, "AVC", "C521", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();

			SetUpTestData();
			authorizationUsage.AGC_Code = "AVC";
			var provider = new InvoiceLineAuthorizationProvider(authorizationUsage);
			AssertEquals("Type", "C521", provider.Type);
		}

		public void TestReference()
		{
			SetUpTestData();
			authorizationUsage.AGC_Number = "123";
			AssertEquals("ReferenceNumber", "123", Provider.Reference);
		}

		public void TestHolderOfTheAuthorisation()
		{
			SetUpTestData();
			CombineAssertions(() =>
			{
				AssertEquals("No Customs Code", ZString.Empty, Provider.HolderOfTheAuthorisation);

				var orgHeader = Factory.New<OrgHeader>();
				var cusCode = orgHeader.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Bolivia;
				cusCode.OK_CustomsRegNo = "EU1234567890";
				authorizationUsage.AGC_OH_Owner = orgHeader.PK;
				AssertEquals("Not in current country", ZString.Empty, Provider.HolderOfTheAuthorisation);
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
				AssertEquals("Not Eori", ZString.Empty, GetProvider().HolderOfTheAuthorisation);
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				AssertEquals("Eori and current country", "EU1234567890", GetProvider().HolderOfTheAuthorisation);
			});
		}

		protected override InvoiceLineAuthorizationProvider GetProvider()
		{
			SetUpTestData();
			return new InvoiceLineAuthorizationProvider(authorizationUsage);
		}

		void SetUpTestData()
		{
			if (authorizationUsage == null)
			{
				authorizationUsage = Factory.New<CusAuthorizationUsage>();
			}
		}

		CusAuthorizationUsage authorizationUsage;
	}
}

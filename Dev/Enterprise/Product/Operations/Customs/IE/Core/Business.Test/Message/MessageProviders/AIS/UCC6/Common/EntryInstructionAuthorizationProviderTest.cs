using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class EntryInstructionAuthorizationProviderTest : DataProviderTestCase<EntryInstructionAuthorizationProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("CusAuthorizationUsage missing", () => new EntryInstructionAuthorizationProvider(null));
		}

		public void TestType()
		{
			AssertEquals("Type", "AVC", Provider.Type);
		}

		public void TestReference()
		{
			AssertEquals("ReferenceNumber", "123", Provider.Reference);
		}

		public void TestHolderOfTheAuthorisation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Customs Code", ZString.Empty, Provider.HolderOfTheAuthorisation);

				var cusCode = orgHeader.CustomsCodes.AddNew();
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Bolivia;
				cusCode.OK_CustomsRegNo = "EU1234567890";
				Factory.Save();
				AssertEquals("Not in current country", ZString.Empty, Provider.HolderOfTheAuthorisation);
				cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
				AssertEquals("Not Eori", ZString.Empty, GetProvider().HolderOfTheAuthorisation);
				cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				AssertEquals("Eori and current country", "EU1234567890", GetProvider().HolderOfTheAuthorisation);
			});
		}

		protected override EntryInstructionAuthorizationProvider GetProvider()
		{
			return new EntryInstructionAuthorizationProvider(authorizationUsage);
		}

		protected override void SetUp()
		{
			authorizationUsage = Factory.NewWithValidTestData<CusAuthorizationUsage>();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			authorizationUsage.AGC_Code = "AVC";
			authorizationUsage.AGC_Number = "123";
			authorizationUsage.AGC_OH_Owner = orgHeader.PK;
		}

		CusAuthorizationUsage authorizationUsage;
		OrgHeader orgHeader;
	}
}

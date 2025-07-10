using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	class AuthorisationProviderTest : Customs.Business.Testing.DataProviderTestCase<AuthorisationProvider, CargoWise.Customs.IE.MessageContracts.AES.Interfaces.IAuthorisation>
	{
		public void TestAuthorisatonHolder()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Ireland);
			usage.AGC_OH_Owner = org.PK;
			AssertEquals("AuthorisatonHolder", "IE123456789", IProvider.AuthorisatonHolder);
		}

		public void TestUCR()
		{
			usage.AGC_Number = "TRA123";
			AssertEquals("UCR", "TRA123", IProvider.UCR);
		}

		public void TestIdentificationType()
		{
			usage.AGC_Code = "SAS";
			AssertEquals("IdentificationType", "C515", IProvider.IdentificationType);
		}

		protected override AuthorisationProvider GetProvider() => new AuthorisationProvider(usage);

		protected override void SetUp()
		{
			base.SetUp();
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap("EUNAU", "SAS", "C515", startDate, endDate, "EUN");
			Factory.Save();

			usage = Factory.New<CusAuthorizationUsage>();
		}
		CusAuthorizationUsage usage;
	}
}

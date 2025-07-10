using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	class AuthorisationHolderProviderTest : DataProviderTestCase<AuthorisationHolderProvider>
	{
		public void TestCode()
		{
			cusAuthorizationUsage.AGC_Code = "CWP";
			AssertEquals("Code=>AGC_Code", "CWP", Provider.Code);
		}

		public void TestID()
		{
			var orgHeader = Factory.New<OrgHeader>();
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
			AssertEquals("Owner EORI not set", string.Empty, Provider.ID);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678");
			AssertEquals("Owner EORI set", "IE12345678", GetProvider().ID);
		}

		protected override AuthorisationHolderProvider GetProvider() => AuthorisationHolderProvider.New(cusAuthorizationUsage);

		protected override void SetUp() => SetUpTestDataIfNeeded();

		void SetUpTestDataIfNeeded()
		{
			if (cusAuthorizationUsage == null)
			{
				declaration = Factory.New<JobDeclaration>();
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusAuthorizationUsage cusAuthorizationUsage;
	}
}

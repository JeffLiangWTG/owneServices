using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM414DeclarationTypePartiesProviderTest : DataProviderTestCase<IM414DeclarationTypePartiesProvider>
	{
		public void TestIIM414DeclarationTypeParties()
		{
			Assert("Should implement IIM414DeclarationTypeParties", Provider is IIM414DeclarationTypeParties);
		}

		public void TestDeclarant()
		{
			var declarantAddress = Factory.NewWithValidTestData<OrgAddress>();
			Declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			declarantAddress.OA_CompanyNameOverride = "Declarant Name";
			declarantAddress.OA_Address1 = "Addr1";
			declarantAddress.OA_Address2 = "Addr2";
			declarantAddress.OA_PostCode = "1234";
			declarantAddress.OA_City = "DUBLIN";
			declarantAddress.OA_RN_NKCountryCode = "IE";

			var declarant = Provider.Declarant;
			AssertType<PartyProvider>(declarant);
			AssertSame("Cached", declarant, Provider.Declarant);
		}

		public void TestRepresentative()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("EOR", "IE293847584930295", "IE");
			var address = orgHeader.MainAddress;
			address.OA_CompanyNameOverride = "Company Name";
			address.OA_Address1 = "Address 1";
			address.OA_Address2 = "Address 2";
			address.OA_City = "Dublin";
			address.OA_PostCode = "D02 PD90";
			address.OA_RN_NKCountryCode = "IE";

			Declaration.JE_OA_Representative = address.PK;
			Declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;

			var representative = Provider.Representative;
			AssertType<RepresentativeProvider>(representative);
			AssertSame("Cached", representative, Provider.Representative);
		}

		protected override IM414DeclarationTypePartiesProvider GetProvider() => new IM414DeclarationTypePartiesProvider(Declaration);

		JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
		JobDeclaration declaration;
	}
}

using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class PartiesProviderTest : DataProviderTestCase<PartiesProvider>
	{
		public void TestRepresentative()
		{
			SetUpTestData();
			header.AMA_OA_Representative = ZGuid.Empty;
			var providerWithNullRepresentative = GetProvider();
			AssertNull(providerWithNullRepresentative.Representative);

			var representativeHeader = Factory.New<OrgHeader>();
			var representative = representativeHeader.Addresses.AddNew();

			var customsCode = representative.CustomsCodes.AddNew();
			customsCode.OK_CodeType = "EOR";
			customsCode.OK_CustomsRegNo = "1234412";

			header.AMA_OA_Representative = representative.PK;
			header.AMA_AgentType = "DIR";

			AssertType<MRepresentativeProvider>(Provider.Representative);
			CombineAssertions(() =>
			{
				AssertEquals("Provider.Representative.Status", "2", Provider.Representative.Status);
				AssertEquals("Provider.Representative.Id", "IE1234412", Provider.Representative.Id);
			});
		}

		public void TestDeclarant()
		{
			SetUpTestData();
			header.AMA_OA_Declarant = ZGuid.Empty;
			var providerWithNullDeclarant = GetProvider();
			AssertNull(providerWithNullDeclarant.Declarant);

			var declarant = Factory.New<OrgAddress>();
			declarant.OA_CompanyNameOverride = "declarant";
			declarant.OA_Address1 = "declarantAddress1";
			declarant.OA_Address2 = "declarantAddress2";
			declarant.OA_PostCode = "233333";
			declarant.City = "declarantCity";
			declarant.OA_RN_NKCountryCode = "AU";

			header.AMA_OA_Declarant = declarant.PK;

			AssertType<PartyProvider>(Provider.Declarant);
			CombineAssertions(() =>
			{
				AssertEquals("Provider.Declarant.Id", null, Provider.Declarant.Id);
				AssertEquals("Provider.Declarant.Name", "declarant", Provider.Declarant.Name);
				AssertType<IE.Business.AddressProvider>(Provider.Declarant.Address);
				AssertEquals("Provider.Declarant.Address.StreetAndNumber", "declarantAddress1, declarantAddress2", Provider.Declarant.Address.StreetAndNumber);
				AssertEquals("Provider.Declarant.Address.City", "declarantCity", Provider.Declarant.Address.City);
				AssertEquals("Provider.Declarant.Address.Postcode", "233333", Provider.Declarant.Address.Postcode);
				AssertEquals("Provider.Declarant.Address.Country", "AU", Provider.Declarant.Address.Country);
			});
		}

		protected override PartiesProvider GetProvider()
		{
			SetUpTestData();
			return new PartiesProvider(header);
		}

		void SetUpTestData()
		{
			if (header == null)
			{
				header = Factory.New<AsycudaManifestHeader>();
			}
		}
		AsycudaManifestHeader header;
	}
}

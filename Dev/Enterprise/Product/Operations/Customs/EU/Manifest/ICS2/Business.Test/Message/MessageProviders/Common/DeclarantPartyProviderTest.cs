using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class DeclarantPartyProviderTest : DataProviderTestCase<DeclarantPartyProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull(DeclarantPartyProvider.NewOrNull(null));
			AssertNotNull(DeclarantPartyProvider.NewOrNull(manifestHeader));
			manifestHeader.AMA_OA_DeclarantInfo.ClearValue();
			AssertNull(DeclarantPartyProvider.NewOrNull(manifestHeader));
		}

		public void TestName()
		{
			AssertEquals("Name", "Name123", Provider.Name);
		}

		public void TestIdentificationNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("IdentificationNumber", "DE654321", Provider.IdentificationNumber);
				orgHeader.CustomsCodes.RemoveAndDeleteAll();
				AssertNull("No EORI on Org", Provider.IdentificationNumber);
			});
		}

		public void TestAddress()
		{
			var address = Provider.Address;
			AssertNotNull("Address", address);
			AssertEquals("My Test Address 123", address.Street);
		}

		public void TestCommunications()
		{
			Assert("Communication", Provider.Communications.Count > 1);
		}

		public void TestStatus()
		{
			AssertNull("Status", Provider.Status);
		}

		public void TestTypeOfPerson()
		{
			AssertNull("TypeOfPerson", Provider.TypeOfPerson);
		}

		protected override void SetUp()
		{
			base.SetUp();

			manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;

			orgHeader = Factory.New<OrgHeader>();

			orgHeader.OH_FullName = "Name123";
			var mainAddress = orgHeader.MainAddress;
			mainAddress.Address1 = "My Test Address 123";
			mainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "654321", CountryCodes.Germany);

			manifestHeader.AMA_OA_Declarant = orgHeader.MainAddress.PK;
		}
		AsycudaManifestHeader manifestHeader;
		OrgHeader orgHeader;

		DeclarantPartyProvider GenerateProvider(AsycudaManifestHeader manifestHeader) => DeclarantPartyProvider.NewOrNull(manifestHeader);

		protected override DeclarantPartyProvider GetProvider()
		{
			return GenerateProvider(manifestHeader);
		}
	}
}

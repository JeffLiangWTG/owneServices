using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class IM432PartiesTypeProviderTest : DataProviderTestCase<IM432PartiesTypeProvider>
	{
		public void TestNewOrNull()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertNotNull(header.Declarant);
			AssertNotNull("Return provider when declarant is not null", IM432PartiesTypeProvider.NewOrNull(header));
		}

		public void TestPresentationPerson()
		{
			AssertEquals("PresentationPerson", "IE1234412", Provider.PresentationPerson);
		}

		public void TestRepresentative()
		{
			var representative = Provider.Representative;
			CombineAssertions("Representative", () =>
			{
				Assert("Representative is RepresentativeProvider", representative is MRepresentativeProvider);
				AssertSame("Is cached", representative, Provider.Representative);
				AssertEquals(representative.Id, "IE334215");
			});
		}

		protected override IM432PartiesTypeProvider GetProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var declarantHeader = Factory.New<OrgHeader>();
			var address = declarantHeader.CustomsCodes.AddNew();
			address.OK_CodeType = "EOR";
			address.OK_CustomsRegNo = "1234412";
			header.AMA_OA_Declarant = declarantHeader.MainAddress.PK;

			var representativeHeader = Factory.New<OrgHeader>();
			var representativeAddress = representativeHeader.CustomsCodes.AddNew();
			representativeAddress.OK_CodeType = "EOR";
			representativeAddress.OK_CustomsRegNo = "334215";
			header.AMA_OA_Representative = representativeHeader.MainAddress.PK;

			return IM432PartiesTypeProvider.NewOrNull(header);
		}
	}
}

using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class CustomsOffices02ProviderTest : DataProviderTestCase<CustomsOffices02Provider>
	{
		public void TestCustomsOfficeLodgement()
		{
			AssertEquals("CustomsOfficeLodgement", "TestOffice", Provider.CustomsOfficeLodgement);
		}

		public void TestPresentationCustomsOffice()
		{
			AssertEquals("PresentationCustomsOffice", "Office", Provider.PresentationCustomsOffice);
		}

		protected override CustomsOffices02Provider GetProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsOffice = "TestOffice";
			header.PresentationOffice = "Office";
			return new CustomsOffices02Provider(header);
		}
	}
}

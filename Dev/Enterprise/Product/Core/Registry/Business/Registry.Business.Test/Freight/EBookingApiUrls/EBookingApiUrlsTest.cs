using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EBookingApiUrls))]
	sealed class EBookingApiUrlsTest : RegistryBusinessObjectTemplateTestCase<EBookingApiUrls>
	{
		public void TestSelectedUrl()
		{
			var eBookingApiUrls = new EBookingApiUrls();
			eBookingApiUrls.SelectedEBookingApiUrlCode = EBookingApiUrls.Constants.TestCode;
			AssertEquals("Test URL correct", "https://abe-test.wisegrid.net/v1", eBookingApiUrls.SelectedUrl);
			eBookingApiUrls.SelectedEBookingApiUrlCode = EBookingApiUrls.Constants.ProdCode;
			AssertEquals("Prod URL correct", "https://abe.wisegrid.net/v1", eBookingApiUrls.SelectedUrl);
		}

		protected override EBookingApiUrls GetBusinessObjectToClone()
		{
			var result = new EBookingApiUrls();
			return result;
		}

		protected override EBookingApiUrls GetBusinessObjectToSerialise()
		{
			return new EBookingApiUrls();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}

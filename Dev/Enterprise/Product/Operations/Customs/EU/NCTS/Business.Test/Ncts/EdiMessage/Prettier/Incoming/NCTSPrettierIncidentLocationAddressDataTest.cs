using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NCTSPrettierIncidentLocationAddressDataTest : TestCase
	{
		public void TestIncidentLocationAddressCountry() => AssertEquals("GB", prettierIncidentLocationAddressData.Country);
		public void TestIncidentLocationAddressPostcode() => AssertEquals("2567", prettierIncidentLocationAddressData.Postcode);
		public void TestIncidentLocationAddressStreetAndNumber() => AssertEquals("424 Long Ave", prettierIncidentLocationAddressData.StreetAndNumber);
		protected override void SetUp()
		{
			base.SetUp();

			prettierIncidentLocationAddressData = new NCTSPrettierIncidentLocationAddressData(country: "GB", postCode: "2567", streetAndNumber: "424 Long Ave");
		}
		NCTSPrettierIncidentLocationAddressData prettierIncidentLocationAddressData;
	}
}

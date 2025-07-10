using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusGoodsLocation = Enterprise.Customs.DE.Business.CusGoodsLocation;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationLayoutBuilder))]
	sealed class CusGoodsLocationLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CusGoodsLocationLayoutBuilder, CusGoodsLocation, EU.GUI.CusGoodsLocationControlBag>
	{
		protected override CusGoodsLocationLayoutBuilder GetColumnLayoutBuilderForTesting() => new CusGoodsLocationLayoutBuilder();

		protected override int ExpectedMaxColumns => 1;

		public void TestAdditionalIdentifierDropEditisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.AdditionalIdentifierDropEdit,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestContactTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.ContactTextBox,
				CusGoodsLocationQualifierList.Codes.Address,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber,
				CusGoodsLocationQualifierList.Codes.GnssCoordinates,
				CusGoodsLocationQualifierList.Codes.UnLocode);
		}

		public void TestPhoneTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.PhoneTextBox,
				CusGoodsLocationQualifierList.Codes.Address,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber,
				CusGoodsLocationQualifierList.Codes.GnssCoordinates,
				CusGoodsLocationQualifierList.Codes.UnLocode);
		}

		public void TestEmailTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.EmailTextBox,
				CusGoodsLocationQualifierList.Codes.Address,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber,
				CusGoodsLocationQualifierList.Codes.GnssCoordinates,
				CusGoodsLocationQualifierList.Codes.UnLocode);
		}

		public void TestPostcodeTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.PostcodeTextBox,
				CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestCountryCodeFindBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.CountryCodeFindBox,
				CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestUnlocoCodeFindBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.UnlocoCodeFindBox, CusGoodsLocationQualifierList.Codes.UnLocode);
		}

		public void TestGeoLocationLatitudeTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
		}

		public void TestGeoLocationLongitudeTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
		}

		public void TesLoadingPlaceTextBoxisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.LoadingPlaceTextBox,
				CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestStreetAndNumberWithAddressValidationControlVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestCityTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.CityTextBox, CusGoodsLocationQualifierList.Codes.Address);
		}

		void AssertControlVisibleForCertainQualifiers(ControlReference control, params string[] visibleForQualifiers)
		{
			var location = Factory.New<CusGoodsLocation>();
			var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

			CusGoodsLocationLayoutAssertions.AssertControlVisibleForCertainQualifiers(layout, control, location, visibleForQualifiers);
		}
	}
}

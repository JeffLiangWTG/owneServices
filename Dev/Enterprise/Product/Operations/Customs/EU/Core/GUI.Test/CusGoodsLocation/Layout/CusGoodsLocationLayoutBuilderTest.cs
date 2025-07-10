using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusGoodsLocation = Enterprise.Customs.EU.Business.CusGoodsLocation;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationLayoutBuilder<CusGoodsLocation>))]
	class CusGoodsLocationLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CusGoodsLocationLayoutBuilder<CusGoodsLocation>, CusGoodsLocation, CusGoodsLocationControlBag>
	{
		protected override CusGoodsLocationLayoutBuilder<CusGoodsLocation> GetColumnLayoutBuilderForTesting() => new CusGoodsLocationLayoutBuilder<CusGoodsLocation>();

		protected override int ExpectedMaxColumns => 1;

		public void TestAdditionalIdentifierTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				CusGoodsLocationQualifierList.Codes.EoriNumber,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestContactTextBoxVisibility()
		{
			AssertControlVisibleWhenQualifierIsNotEmpty(CusGoodsLocationControlBag.Instance.ContactTextBox);
		}

		public void TestPhoneTextBoxVisibility()
		{
			AssertControlVisibleWhenQualifierIsNotEmpty(CusGoodsLocationControlBag.Instance.PhoneTextBox);
		}

		public void TestEmailTextBoxVisibility()
		{
			AssertControlVisibleWhenQualifierIsNotEmpty(CusGoodsLocationControlBag.Instance.EmailTextBox);
		}

		public void TestPostcodeTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.PostcodeTextBox,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestCountryCodeFindBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.CountryCodeFindBox,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestUnlocoCodeFindBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.UnlocoCodeFindBox, CusGoodsLocationQualifierList.Codes.UnLocode);
		}

		public void TestCustomsOfficeCodeFindBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.CustomsOfficeCodeFindBox, CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier);
		}

		public void TestGeoLocationLatitudeTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
		}

		public void TestGeoLocationLongitudeTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
		}

		public void TesOrganisationFindBoxisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.OrganisationFindBox,
				CusGoodsLocationQualifierList.Codes.EoriNumber,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestEoriNumberTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.EoriNumberTextBox, CusGoodsLocationQualifierList.Codes.EoriNumber);
		}

		public void TestAuthorizationCodeFindBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestAuthorizationCodeDropEditVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.AuthorizationDropEdit, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
		}

		public void TestStreetAndNumberWithAddressValidationControlVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestCityTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.CityTextBox, CusGoodsLocationQualifierList.Codes.Address);
		}

		void AssertControlVisibleForCertainQualifiers(ControlReference control, params string[] visibleForQualifiers)
		{
			var location = Factory.New<CusGoodsLocation>();
			var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

			CusGoodsLocationLayoutAssertions.AssertControlVisibleForCertainQualifiers(layout, control, location, visibleForQualifiers);
		}

		void AssertControlVisibleWhenQualifierIsNotEmpty(ControlReference control)
		{
			var location = Factory.New<CusGoodsLocation>();
			var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

			CusGoodsLocationLayoutAssertions.AssertControlVisibleWhenQualifierIsNotEmpty(layout, control, location);
		}

		public void TestAdditionalIdentifierTextBoxCaption()
		{
			var location = Factory.New<CusGoodsLocation>();
			var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

			CombineAssertions(() =>
			{
				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.PostcodeAddress;
				layout.TryGetCaption(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, location, out var resourceStringData);
				AssertEquals("CGL_Qualifier = 'T'", "House Number", resourceStringData.Caption);

				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
				layout.TryGetCaption(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, location, out resourceStringData);
				AssertEquals("CGL_Qualifier = 'X'", "Additional Identifier", resourceStringData.Caption);

				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				layout.TryGetCaption(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, location, out resourceStringData);
				AssertEquals("CGL_Qualifier = 'Y'", "Additional Identifier", resourceStringData.Caption);
			});
		}
	}
}

using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(PlaceOfUseOrProcessingLayout))]
	sealed class PlaceOfUseOrProcessingLayoutTest : LayoutsAbstractTest
	{
		public void TestAdditionalIdentifierTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress,
				CusGoodsLocationQualifierList.Codes.EoriNumber,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber,
				CusGoodsLocationQualifierList.Codes.UnLocode,
				CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier,
				CusGoodsLocationQualifierList.Codes.Address);
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

		public void TestStreetAndNumberWithAddressValidationControlVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestCityTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.CityTextBox, CusGoodsLocationQualifierList.Codes.Address);
		}

		public void TestAdditionalIdentifierTextBoxCaption()
		{
			var location = Factory.New<PlaceOfUseOrProcessing>();
			var layout = ((IPanelLayoutProvider)new PlaceOfUseOrProcessingLayout()).Layout;

			CombineAssertions(() =>
			{
				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
				layout.TryGetCaption(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, location, out var resourceStringData);
				AssertEquals("CGL_Qualifier = 'X'", "Additional Identifier", resourceStringData.Caption);

				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				layout.TryGetCaption(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, location, out resourceStringData);
				AssertEquals("CGL_Qualifier = 'Y'", "Additional Identifier", resourceStringData.Caption);
			});
		}

		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CusGoodsLocationControlBag.Instance.QualifierDropEdit, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.TypeDropEdit, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.OrganisationFindBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.EoriNumberTextBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.CityTextBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.PostcodeTextBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.CountryCodeFindBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.UnlocoCodeFindBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CusGoodsLocationLayoutBuilder<PlaceOfUseOrProcessing>();

		void AssertControlVisibleForCertainQualifiers(ControlReference control, params string[] visibleForQualifiers)
		{
			var location = Factory.New<PlaceOfUseOrProcessing>();
			var layout = ((IPanelLayoutProvider)new PlaceOfUseOrProcessingLayout()).Layout;

			PlaceOfUseOrProcessingLayoutAssertions.AssertControlVisibleForCertainQualifiers(layout, control, location, visibleForQualifiers);
		}
	}
}

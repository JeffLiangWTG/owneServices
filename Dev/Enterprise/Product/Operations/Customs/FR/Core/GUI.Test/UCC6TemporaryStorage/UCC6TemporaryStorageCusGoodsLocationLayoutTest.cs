using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.FR.GUI.UCC6TemporaryStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageCusGoodsLocationLayout))]
	sealed class UCC6TemporaryStorageCusGoodsLocationLayoutTest : LayoutsAbstractTest
	{
		public void TestAdditionalIdentifierTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox,
				CusGoodsLocationQualifierList.Codes.EoriNumber,
				CusGoodsLocationQualifierList.Codes.AuthorizationNumber,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress);

			CombineAssertions(() =>
			{
				AssertControlVisibleForCertainQualifiersDependingOnType(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox,
					CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, CusGoodsLocationTypeList.Codes.DesignatedLocation, true);
				AssertControlVisibleForCertainQualifiersDependingOnType(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox,
					CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, CusGoodsLocationTypeList.Codes.AuthorizedPlace, false);
				AssertControlVisibleForCertainQualifiersDependingOnType(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox,
					CusGoodsLocationQualifierList.Codes.EoriNumber, CusGoodsLocationTypeList.Codes.AuthorizedPlace, true);
				AssertControlVisibleForCertainQualifiersDependingOnType(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox,
					CusGoodsLocationQualifierList.Codes.EoriNumber, CusGoodsLocationTypeList.Codes.DesignatedLocation, true);
				AssertControlVisibleForCertainQualifiersDependingOnType(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox,
					CusGoodsLocationQualifierList.Codes.EoriNumber, ZString.Empty, true);
			});
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

		public void TestStreetAndNumberTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, CusGoodsLocationQualifierList.Codes.Address, CusGoodsLocationQualifierList.Codes.PostcodeAddress);
		}

		public void TestCityTextBoxVisibility()
		{
			AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.CityTextBox, CusGoodsLocationQualifierList.Codes.Address);
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

		public void TestStreetAndNumberTextBoxCaption()
		{
			var location = Factory.New<EU.Business.CusGoodsLocation>();
			var layout = ((IPanelLayoutProvider)new UCC6TemporaryStorageCusGoodsLocationLayout()).Layout;

			CombineAssertions(() =>
			{
				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.PostcodeAddress;
				layout.TryGetCaption(CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, location, out var resourceStringData);
				AssertEquals("CGL_Qualifier = 'T'", "House Number", resourceStringData.Caption);

				location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				layout.TryGetCaption(CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, location, out resourceStringData);
				AssertEquals("CGL_Qualifier = 'Z'", "Street + Number", resourceStringData.Caption);
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
				yield return (CusGoodsLocationControlBag.Instance.ContactTextBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.PhoneTextBox, ControlWidthClass.Long);
				yield return (CusGoodsLocationControlBag.Instance.EmailTextBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CusGoodsLocationLayoutBuilder<EU.Business.CusGoodsLocation>();

		void AssertControlVisibleForCertainQualifiers(ControlReference control, params string[] visibleForQualifiers)
		{
			var location = Factory.New<EU.Business.CusGoodsLocation>();
			var layout = ((IPanelLayoutProvider)new UCC6TemporaryStorageCusGoodsLocationLayout()).Layout;

			CombineAssertions(() =>
			{
				foreach (var qualifier in new CusGoodsLocationQualifierList().GetAllCodes())
				{
					location.CGL_Qualifier = qualifier;
					AssertEquals($"CGL_Qualifier = '{qualifier}'", qualifier.In(visibleForQualifiers), layout.IsVisible(control, location));
				}
			});
		}

		void AssertControlVisibleForCertainQualifiersDependingOnType(ControlReference control, ZString visibleForQualifiers, ZString visibleForType, bool shouldBeVisible)
		{
			var location = Factory.New<EU.Business.CusGoodsLocation>();
			var layout = ((IPanelLayoutProvider)new UCC6TemporaryStorageCusGoodsLocationLayout()).Layout;
			location.CGL_Qualifier = visibleForQualifiers;
			location.CGL_Type = visibleForType;
			AssertEquals($"CGL_Qualifier = '{visibleForQualifiers}' and type = '{visibleForType}'", shouldBeVisible, layout.IsVisible(control, location));
		}

		void AssertControlVisibleWhenQualifierIsNotEmpty(ControlReference control)
		{
			var location = Factory.New<EU.Business.CusGoodsLocation>();
			var layout = ((IPanelLayoutProvider)new UCC6TemporaryStorageCusGoodsLocationLayout()).Layout;

			CombineAssertions(() =>
			{
				foreach (var qualifier in new CusGoodsLocationQualifierList().GetAllCodes())
				{
					location.CGL_Qualifier = qualifier;
					AssertEquals($"CGL_Qualifier = '{qualifier}'", true, layout.IsVisible(control, location));
				}

				location.CGL_Qualifier = ZString.Empty;
				AssertEquals("CGL_Qualifier empty", false, layout.IsVisible(control, location));
			});
		}
	}
}

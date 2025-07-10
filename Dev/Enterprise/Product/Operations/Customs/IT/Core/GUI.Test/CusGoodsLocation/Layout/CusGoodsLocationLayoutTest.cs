using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using QualifierList = Enterprise.Customs.Business.CusGoodsLocationQualifierList;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(CusGoodsLocationLayout))]
sealed class CusGoodsLocationLayoutTest : LayoutsAbstractTest
{
	public void TestAdditionalIdentifierTextBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox,
			QualifierList.Codes.PostcodeAddress,
			QualifierList.Codes.EoriNumber);
	}

	public void TestAdditionalIdentifierDropEditVisibility()
	{
		var location = Factory.New<CusGoodsLocation>();
		var layout = CreatePanelLayout();

		location.CGL_Qualifier = QualifierList.Codes.AuthorizationNumber;
		location.CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		AssertEquals("CGL_Qualifier = 'Y' and CGL_Type = 'B'", true, layout.IsVisible(CusGoodsLocationControlBag.Instance.AdditionalIdentifierDropEdit, location));

		location.CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.DesignatedLocation;
		AssertEquals("CGL_Qualifier = 'Y' and CGL_Type = 'B'", false, layout.IsVisible(CusGoodsLocationControlBag.Instance.AdditionalIdentifierDropEdit, location));
	}

	public void TestPostcodeTextBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.PostcodeTextBox,
			QualifierList.Codes.PostcodeAddress,
			QualifierList.Codes.Address);
	}

	public void TestCountryCodeFindBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.CountryCodeFindBox,
			QualifierList.Codes.PostcodeAddress,
			QualifierList.Codes.Address);
	}

	public void TestUnlocoCodeFindBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.UnlocoCodeFindBox, QualifierList.Codes.UnLocode);
	}

	public void TestCustomsOfficeCodeFindBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.CustomsOfficeCodeFindBox, QualifierList.Codes.CustomsOfficeIdentifier);
	}

	public void TestGeoLocationLatitudeTextBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox, QualifierList.Codes.GnssCoordinates);
	}

	public void TestGeoLocationLongitudeTextBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox, QualifierList.Codes.GnssCoordinates);
	}

	public void TestOrganisationFindBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.OrganisationFindBox,
			QualifierList.Codes.EoriNumber,
			QualifierList.Codes.AuthorizationNumber);
	}

	public void TestOrganizationAddressControlVisibility()
	{
		AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.OrganizationAddressControl, QualifierList.Codes.Address);
	}

	public void TestOverrideCheckBoxControlVisibility()
	{
		AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.OverrideCheckBox, QualifierList.Codes.Address);
	}

	public void TestEoriNumberTextBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.EoriNumberTextBox, QualifierList.Codes.EoriNumber);
	}

	public void TestAuthorizationCodeFindBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, QualifierList.Codes.AuthorizationNumber);
	}

	public void TestStreetAndNumberWithAddressValidationControlVisibility()
	{
		AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, QualifierList.Codes.Address);
	}

	public void TestCityTextBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.CityTextBox, QualifierList.Codes.Address);
	}

	public void TestContactTextBoxVisibility()
	{
		AssertControlHiddenForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.ContactTextBox, QualifierList.Codes.CustomsOfficeIdentifier);
		AssertControlHiddenWhenQualifierIsEmpty(EU.GUI.CusGoodsLocationControlBag.Instance.ContactTextBox);
	}

	public void TestPhoneTextBoxVisibility()
	{
		AssertControlHiddenForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.PhoneTextBox, QualifierList.Codes.CustomsOfficeIdentifier);
		AssertControlHiddenWhenQualifierIsEmpty(EU.GUI.CusGoodsLocationControlBag.Instance.PhoneTextBox);
	}

	public void TestEmailTextBoxVisibility()
	{
		AssertControlHiddenForCertainQualifiers(EU.GUI.CusGoodsLocationControlBag.Instance.EmailTextBox, QualifierList.Codes.CustomsOfficeIdentifier);
		AssertControlHiddenWhenQualifierIsEmpty(EU.GUI.CusGoodsLocationControlBag.Instance.EmailTextBox);
	}

	public void TestAdditionalIdentifierTextBoxCaption()
	{
		var location = Factory.New<CusGoodsLocation>();
		var layout = CreatePanelLayout();

		CombineAssertions(() =>
		{
			location.CGL_Qualifier = QualifierList.Codes.PostcodeAddress;
			layout.TryGetCaption(EU.GUI.CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, location, out var resourceStringData);
			AssertEquals("CGL_Qualifier = 'T'", "House Number", resourceStringData.Caption);

			location.CGL_Qualifier = QualifierList.Codes.EoriNumber;
			layout.TryGetCaption(EU.GUI.CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, location, out resourceStringData);
			AssertEquals("CGL_Qualifier = 'X'", "Additional Identifier", resourceStringData.Caption);
		});
	}

	protected override int ControlBagCount => 2;

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
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.QualifierDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.TypeDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.OrganisationFindBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.OrganizationAddressControl, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.OverrideCheckBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.EoriNumberTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.AdditionalIdentifierDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.CityTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.PostcodeTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.CountryCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.UnlocoCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.ContactTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.PhoneTextBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.EmailTextBox, ControlWidthClass.Long);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.CusGoodsLocationLayoutBuilder<CusGoodsLocation>();

	void AssertControlVisibleForCertainQualifiers(ControlReference control, params string[] visibleForQualifiers)
	{
		var location = Factory.New<CusGoodsLocation>();
		var layout = CreatePanelLayout();

		CombineAssertions(() =>
		{
			foreach (var qualifier in new QualifierList().GetAllCodes())
			{
				location.CGL_Qualifier = qualifier;
				AssertEquals($"CGL_Qualifier = '{qualifier}'", qualifier.In(visibleForQualifiers), layout.IsVisible(control, location));
			}
		});
	}

	void AssertControlHiddenForCertainQualifiers(ControlReference control, params string[] hiddenForQualifiers)
	{
		var location = Factory.New<CusGoodsLocation>();
		var layout = CreatePanelLayout();

		CombineAssertions(() =>
		{
			foreach (var qualifier in new QualifierList().GetAllCodes())
			{
				location.CGL_Qualifier = qualifier;
				AssertEquals($"CGL_Qualifier = '{qualifier}'", qualifier.In(hiddenForQualifiers), !layout.IsVisible(control, location));
			}
		});
	}

	void AssertControlHiddenWhenQualifierIsEmpty(ControlReference control)
	{
		var location = Factory.New<CusGoodsLocation>();
		var layout = CreatePanelLayout();

		CombineAssertions(() =>
		{
			location.CGL_Qualifier = ZString.Empty;
			AssertEquals("CGL_Qualifier empty", false, layout.IsVisible(control, location));
		});
	}

	PanelLayout CreatePanelLayout() => ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;
}

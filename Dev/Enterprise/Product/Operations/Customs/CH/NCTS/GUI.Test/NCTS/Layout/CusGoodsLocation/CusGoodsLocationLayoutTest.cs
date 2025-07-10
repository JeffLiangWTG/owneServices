using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusGoodsLocation = Enterprise.Customs.CH.NCTS.Business.CusGoodsLocation;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

[TestedType(typeof(CusGoodsLocationLayout))]
sealed class CusGoodsLocationLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new CusGoodsLocationLayoutBuilder<CusGoodsLocation>();

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

	public void TestAdditionalIdentifierTextBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.AdditionalIdentifierTextBox,
			CusGoodsLocationQualifierList.Codes.PostcodeAddress,
			CusGoodsLocationQualifierList.Codes.EoriNumber,
			CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
	}

	public void TesOrganisationFindBoxisibility()
	{
		AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.OrganisationFindBox,
			CusGoodsLocationQualifierList.Codes.EoriNumber,
			CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
	}

	public void TestAuthorizationCodeFindBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
	}

	public void TestUnlocoCodeFindBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.UnlocoCodeFindBox, CusGoodsLocationQualifierList.Codes.UnLocode);
	}

	public void TestGeoLocationLatitudeTextBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.GeoLocationLatitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
	}

	public void TestGeoLocationLongitudeTextBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.GeoLocationLongitudeTextBox, CusGoodsLocationQualifierList.Codes.GnssCoordinates);
	}

	public void TestStreetAndNumberTextBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, CusGoodsLocationQualifierList.Codes.Address);
	}

	public void TestCityTextBoxVisibility()
	{
		AssertControlVisibleForCertainQualifiers(CusGoodsLocationControlBag.Instance.CityTextBox, CusGoodsLocationQualifierList.Codes.Address);
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

	public void TestContactTextBox()
	{
		AssertControlVisibleWhenParentIsIncident(CusGoodsLocationControlBag.Instance.ContactTextBox, true);
	}

	public void TestPhoneTextBox()
	{
		AssertControlVisibleWhenParentIsIncident(CusGoodsLocationControlBag.Instance.PhoneTextBox, true);
	}

	public void TestEmailTextBox()
	{
		AssertControlVisibleWhenParentIsIncident(CusGoodsLocationControlBag.Instance.EmailTextBox, true);
	}

	public void TestTypeDropEdit()
	{
		AssertControlVisibleWhenParentIsIncident(CusGoodsLocationControlBag.Instance.TypeDropEdit, false);
	}

	void AssertControlVisibleForCertainQualifiers(ControlReference control, params string[] visibleForQualifiers)
	{
		var location = Factory.New<CusGoodsLocation>();
		var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

		CusGoodsLocationLayoutAssertions.AssertControlVisibleForCertainQualifiers(layout, control, location, visibleForQualifiers);
	}

	void AssertControlVisibleWhenParentIsIncident(ControlReference control, bool visibility) => CombineAssertions(() =>
	{
		var layout = ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;

		var nctsHeader = Factory.NewWithValidTestData<Business.NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		AssertEquals("Location on Departure Movement", !visibility, layout.IsVisible(control, nctsHeader.MovementHeader.GoodsLocation));
		AssertEquals("Location on Departure Incident", visibility, layout.IsVisible(control, nctsHeader.EnRouteIncidents.AddNew().GoodsLocation));

		nctsHeader = Factory.NewWithValidTestData<Business.NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		AssertEquals("Location on Arrival Movement", !visibility, layout.IsVisible(control, nctsHeader.ArrivalMovementHeader.GoodsLocation));
		AssertEquals("Location on Arrival Incident", visibility, layout.IsVisible(control, nctsHeader.EnRouteIncidents.AddNew().GoodsLocation));
	});
}

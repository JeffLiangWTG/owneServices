using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using QualifierList = Enterprise.Customs.Business.CusGoodsLocationQualifierList;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(CusGoodsLocationLayout))]
sealed class CusGoodsLocationLayoutTest : LayoutsAbstractTest
{
	public void TestCustomsOfficeCodeFindBoxVisibility()
	{
		var controlReference = EU.GUI.CusGoodsLocationControlBag.Instance.CustomsOfficeCodeFindBox;
		AssertControlVisibleForCertainQualifiers(controlReference, QualifierList.Codes.CustomsOfficeIdentifier);
		AssertControlHiddenForCertainQualifiers(controlReference, QualifierList.Codes.AuthorizationNumber, QualifierList.Codes.Address);
	}

	public void TestNameTextBoxVisibility()
	{
		var controlReference = EU.GUI.CusGoodsLocationControlBag.Instance.ContactTextBox;
		AssertControlVisibleForCertainQualifiers(controlReference, QualifierList.Codes.AuthorizationNumber, QualifierList.Codes.Address);
		AssertControlHiddenForCertainQualifiers(controlReference, QualifierList.Codes.CustomsOfficeIdentifier);
	}

	public void TestPhoneNumberTextBoxVisibility()
	{
		var controlReference = EU.GUI.CusGoodsLocationControlBag.Instance.PhoneTextBox;
		AssertControlVisibleForCertainQualifiers(controlReference, QualifierList.Codes.AuthorizationNumber, QualifierList.Codes.Address);
		AssertControlHiddenForCertainQualifiers(controlReference, QualifierList.Codes.CustomsOfficeIdentifier);
	}

	public void TestEmailTextBoxVisibility()
	{
		var controlReference = EU.GUI.CusGoodsLocationControlBag.Instance.EmailTextBox;
		AssertControlVisibleForCertainQualifiers(controlReference, QualifierList.Codes.AuthorizationNumber, QualifierList.Codes.Address);
		AssertControlHiddenForCertainQualifiers(controlReference, QualifierList.Codes.CustomsOfficeIdentifier);
	}

	public void TestOrganisationFindBoxVisibility()
	{
		var controlReference = EU.GUI.CusGoodsLocationControlBag.Instance.OrganisationFindBox;
		AssertControlVisibleForCertainQualifiers(controlReference, QualifierList.Codes.AuthorizationNumber);
		AssertControlHiddenForCertainQualifiers(controlReference, QualifierList.Codes.CustomsOfficeIdentifier, QualifierList.Codes.Address);
	}

	public void TestAuthorizationCodeFindBoxVisibility()
	{
		var controlReference = EU.GUI.CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox;
		AssertControlVisibleForCertainQualifiers(controlReference, QualifierList.Codes.AuthorizationNumber);
		AssertControlHiddenForCertainQualifiers(controlReference, QualifierList.Codes.CustomsOfficeIdentifier, QualifierList.Codes.Address);
	}

	public void TestAdditionalIdentifierDropEditVisibility()
	{
		var controlReference = CusGoodsLocationControlBag.Instance.AdditionalIdentifierDropEdit;
		AssertControlVisibleForCertainQualifiers(controlReference, QualifierList.Codes.AuthorizationNumber);
		AssertControlHiddenForCertainQualifiers(controlReference, QualifierList.Codes.CustomsOfficeIdentifier, QualifierList.Codes.Address);
	}

	public void TestStreetAndNumberVisibility()
	{
		var controlReference = EU.GUI.CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl;
		AssertControlVisibleForCertainQualifiers(controlReference, QualifierList.Codes.Address);
		AssertControlHiddenForCertainQualifiers(controlReference, QualifierList.Codes.CustomsOfficeIdentifier, QualifierList.Codes.AuthorizationNumber);
	}

	public void TestCityVisibility()
	{
		var controlReference = EU.GUI.CusGoodsLocationControlBag.Instance.CityTextBox;
		AssertControlVisibleForCertainQualifiers(controlReference, QualifierList.Codes.Address);
		AssertControlHiddenForCertainQualifiers(controlReference, QualifierList.Codes.CustomsOfficeIdentifier, QualifierList.Codes.AuthorizationNumber);
	}

	public void TestPostCodeVisibility()
	{
		var controlReference = EU.GUI.CusGoodsLocationControlBag.Instance.PostcodeTextBox;
		AssertControlVisibleForCertainQualifiers(controlReference, QualifierList.Codes.Address);
		AssertControlHiddenForCertainQualifiers(controlReference, QualifierList.Codes.CustomsOfficeIdentifier, QualifierList.Codes.AuthorizationNumber);
	}

	public void TestCountryVisibility()
	{
		var controlReference = EU.GUI.CusGoodsLocationControlBag.Instance.CountryCodeFindBox;
		AssertControlVisibleForCertainQualifiers(controlReference, QualifierList.Codes.Address);
		AssertControlHiddenForCertainQualifiers(controlReference, QualifierList.Codes.CustomsOfficeIdentifier, QualifierList.Codes.AuthorizationNumber);
	}

	public void TestOrganizationAddressControlVisibility()
	{
		var controlReference = IT.GUI.CusGoodsLocationControlBag.Instance.OrganizationAddressControl;
		AssertControlVisibleForCertainQualifiers(controlReference, QualifierList.Codes.Address);
		AssertControlHiddenForCertainQualifiers(controlReference, QualifierList.Codes.CustomsOfficeIdentifier, QualifierList.Codes.AuthorizationNumber);
	}

	public void TestOverrideVisibility()
	{
		var controlReference = IT.GUI.CusGoodsLocationControlBag.Instance.OverrideCheckBox;
		AssertControlVisibleForCertainQualifiers(controlReference, QualifierList.Codes.Address);
		AssertControlHiddenForCertainQualifiers(controlReference, QualifierList.Codes.CustomsOfficeIdentifier, QualifierList.Codes.AuthorizationNumber);
	}

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.CusGoodsLocationLayoutBuilder<CusGoodsLocation>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.QualifierDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.TypeDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Auto);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.OrganisationFindBox, ControlWidthClass.Auto);
			yield return (CusGoodsLocationControlBag.Instance.OrganizationAddressControl, ControlWidthClass.Long);
			yield return (CusGoodsLocationControlBag.Instance.OverrideCheckBox, ControlWidthClass.Long);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.AuthorizationCodeFindBox, ControlWidthClass.Auto);
			yield return (CusGoodsLocationControlBag.Instance.AdditionalIdentifierDropEdit, ControlWidthClass.Auto);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Auto);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.CityTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.PostcodeTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.CountryCodeFindBox, ControlWidthClass.Auto);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.ContactTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.PhoneTextBox, ControlWidthClass.Auto);
			yield return (EU.GUI.CusGoodsLocationControlBag.Instance.EmailTextBox, ControlWidthClass.Auto);
		}
	}

	void AssertControlVisibleForCertainQualifiers(ControlReference control, params string[] visibleForQualifiers)
	{
		var location = Factory.New<CusGoodsLocation>();
		var layout = GetLayout();

		CombineAssertions(() =>
		{
			foreach (var qualifier in location.Lookups.QualifierList.GetAllCodes())
			{
				location.CGL_Qualifier = qualifier;
				AssertEquals($"CGL_Qualifier = '{qualifier}'", qualifier.In(visibleForQualifiers), layout.IsVisible(control, location));
			}
		});
	}

	void AssertControlHiddenForCertainQualifiers(ControlReference controlReference, params string[] hiddenForQualifiers)
	{
		var location = Factory.New<CusGoodsLocation>();
		var layout = GetLayout();

		CombineAssertions(() =>
		{
			foreach (var qualifier in location.Lookups.QualifierList.GetAllCodes())
			{
				location.CGL_Qualifier = qualifier;
				AssertEquals($"CGL_Qualifier = '{qualifier}'", qualifier.In(hiddenForQualifiers), !layout.IsVisible(controlReference, location));
			}
		});
	}

	PanelLayout GetLayout() => ((IPanelLayoutProvider)new CusGoodsLocationLayout()).Layout;
}

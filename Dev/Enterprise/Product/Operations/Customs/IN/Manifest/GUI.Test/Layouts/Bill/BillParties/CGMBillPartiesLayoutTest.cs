using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.GUI.Testing;

[TestedType(typeof(CGMBillPartiesLayout))]
sealed class CGMBillPartiesLayoutTest : LayoutsAbstractTest
{
	public void TestControlsCaption()
	{
		var layout = GetLayout();
		layout.AssertCaption(Bill, CommonBillPartiesControlBag.Instance.BuyerSeparatorUserControl, "Importer Details");
		layout.AssertCaption(Bill, CommonBillPartiesControlBag.Instance.ConvertBuyerToOrganizationButton, "Convert Importer To Org.");
	}

	public void TestImporterDetailsColumnVisibility()
	{
		GetLayout().AssertControlsVisiblilityDependOnTransportMode(Header, Bill, visiblilityForSea: true, visiblilityForAir: false,
			new[]
			{
				CommonBag.BuyerSeparatorUserControl,
				CommonBag.BuyerAddressControl,
				CommonBag.BuyerNameTextBox,
				CommonBag.BuyerStreet1TextBox,
				CommonBag.BuyerStreet2TextBox,
				CommonBag.BuyerCityTextBox,
				CommonBag.BuyerCountryCodeFindBox,
				CommonBag.BuyerStateDropEdit,
				CommonBag.BuyerPhoneTextBox,
				CommonBag.BuyerPostcodeTextBox,
				CommonBag.BuyerRegNoTextBox,
			});
	}

	public void TestConvertBuyerToOrganizationButtonVisibility()
	{
		var layout = GetLayout();
		var convertBuyerToOrganizationButton = CommonBag.ConvertBuyerToOrganizationButton;
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("AIR CGM", false, layout.IsVisible(convertBuyerToOrganizationButton, Bill));

			bill.ABL_BuyerName = "Test";
			AssertEquals("AIR CGM and CanConvertBuyerToOrganization", false, layout.IsVisible(convertBuyerToOrganizationButton, Bill));

			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("SEA CGM and CanConvertBuyerToOrganization", true, layout.IsVisible(convertBuyerToOrganizationButton, Bill));

			bill.ABL_BuyerName = ZString.Empty;
			bill.ABL_BuyerStreet1 = "Test";
			AssertEquals("SEA CGM and CanConvertBuyerToOrganization", true, layout.IsVisible(convertBuyerToOrganizationButton, Bill));

			bill.ABL_BuyerStreet1 = ZString.Empty;
			bill.ABL_BuyerStreet2 = "Test";
			AssertEquals("SEA CGM and CanConvertBuyerToOrganization", true, layout.IsVisible(convertBuyerToOrganizationButton, Bill));

			bill.ABL_BuyerStreet2 = ZString.Empty;
			bill.ABL_BuyerCity = "Test";
			AssertEquals("SEA CGM and CanConvertBuyerToOrganization", true, layout.IsVisible(convertBuyerToOrganizationButton, Bill));

			bill.ABL_BuyerCity = ZString.Empty;
			bill.ABL_BuyerState = "Test";
			AssertEquals("SEA CGM and CanConvertBuyerToOrganization", true, layout.IsVisible(convertBuyerToOrganizationButton, Bill));

			bill.ABL_BuyerState = ZString.Empty;
			bill.ABL_BuyerPostcode = "Test";
			AssertEquals("SEA CGM and CanConvertBuyerToOrganization", true, layout.IsVisible(convertBuyerToOrganizationButton, Bill));

			bill.ABL_BuyerPostcode = ZString.Empty;
			bill.ABL_RN_NKBuyerCountry = Core.Constants.CountryCodes.India;
			AssertEquals("SEA CGM and CanConvertBuyerToOrganization", true, layout.IsVisible(convertBuyerToOrganizationButton, Bill));

			bill.ABL_RN_NKBuyerCountry = ZString.Empty;
			bill.ABL_BuyerPhone = "Test";
			AssertEquals("SEA CGM and CanConvertBuyerToOrganization", true, layout.IsVisible(convertBuyerToOrganizationButton, Bill));

			Header.AMA_TransportMode = "XYZ";
			AssertEquals("Invalid", false, layout.IsVisible(convertBuyerToOrganizationButton, Bill));
		});
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillPartiesLayoutBuilder<CGMAsycudaBill>();

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
			yield return ThirdColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (CommonBillPartiesControlBag.Instance.ShipperSeparatorUserControl, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConvertShipperToOrganizationButton, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperNameTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperStreet1TextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperStreet2TextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperCityTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperCountryCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.ShipperStateDropEdit, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.ShipperPostCodeTextBox, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.ShipperPhoneTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ShipperRegNoTextBox, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeSeparatorUserControl, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConvertConsigneeToOrganizationButton, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeNameTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeStreet1TextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeStreet2TextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeCityTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConigneeCountryCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeStateDropEdit, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneePostcodeTextBox, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneePhoneTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConsigneeRegoNoTextBox, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (CommonBillPartiesControlBag.Instance.BuyerSeparatorUserControl, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.BuyerAddressControl, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.ConvertBuyerToOrganizationButton, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.BuyerNameTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.BuyerStreet1TextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.BuyerStreet2TextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.BuyerCityTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.BuyerCountryCodeFindBox, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.BuyerStateDropEdit, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.BuyerPostcodeTextBox, ControlWidthClass.Auto);
			yield return (CommonBillPartiesControlBag.Instance.BuyerPhoneTextBox, ControlWidthClass.Long);
			yield return (CommonBillPartiesControlBag.Instance.BuyerRegNoTextBox, ControlWidthClass.Long);
		}
	}

	PanelLayout GetLayout() => GetLayoutProvider().Layout;

	IPanelLayoutProvider GetLayoutProvider() => new CGMBillPartiesLayout();

	CGMAsycudaBill Bill => bill ??= Header.Bills.AddNew();
	CGMAsycudaBill bill;

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;

	CommonBillPartiesControlBag CommonBag => CommonBillPartiesControlBag.Instance;
}

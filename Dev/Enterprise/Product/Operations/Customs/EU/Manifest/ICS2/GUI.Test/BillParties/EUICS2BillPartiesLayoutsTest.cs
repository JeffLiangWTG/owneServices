using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	[TestedType(typeof(EUICS2BillPartiesLayouts))]
	sealed class EUICS2BillPartiesLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillPartiesLayoutBuilder<AsycudaBill>();

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
				yield return (EUICS2BillPartiesControlBag.Instance.ShipperPersonTypeDropEdit, ControlWidthClass.Auto);

				yield return (EUICS2BillPartiesControlBag.Instance.SellerSeparatorUserControl, ControlWidthClass.Long);
				yield return (EUICS2BillPartiesControlBag.Instance.SellerAddressControl, ControlWidthClass.Long);
				yield return (EUICS2BillPartiesControlBag.Instance.ConvertSellerToOrganizationButton, ControlWidthClass.Long);
				yield return (EUICS2BillPartiesControlBag.Instance.SellerNameTextBox, ControlWidthClass.Long);
				yield return (EUICS2BillPartiesControlBag.Instance.SellerStreet1TextBox, ControlWidthClass.Long);
				yield return (EUICS2BillPartiesControlBag.Instance.SellerStreet2TextBox, ControlWidthClass.Long);
				yield return (EUICS2BillPartiesControlBag.Instance.SellerCityTextBox, ControlWidthClass.Long);
				yield return (EUICS2BillPartiesControlBag.Instance.SellerCountryCodeFindBox, ControlWidthClass.Auto);
				yield return (EUICS2BillPartiesControlBag.Instance.SellerStateDropEdit, ControlWidthClass.Auto);
				yield return (EUICS2BillPartiesControlBag.Instance.SellerPostCodeTextBox, ControlWidthClass.Auto);
				yield return (EUICS2BillPartiesControlBag.Instance.SellerPhoneTextBox, ControlWidthClass.Long);
				yield return (EUICS2BillPartiesControlBag.Instance.SellerRegNoTextBox, ControlWidthClass.Long);
				yield return (EUICS2BillPartiesControlBag.Instance.SellerPersonTypeDropEdit, ControlWidthClass.Auto);
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
				yield return (EUICS2BillPartiesControlBag.Instance.ConsigneePersonTypeDropEdit, ControlWidthClass.Auto);

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
				yield return (EUICS2BillPartiesControlBag.Instance.BuyerPersonTypeDropEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillPartiesControlBag.Instance.NotifyPartySeparatorUserControl, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ConvertNotifyPartyToOrganizationButton, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.NotifyPartyNameTextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.NotifyPartyStreet1TextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.NotifyPartyStreet2TextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.NotifyPartyCityTextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.NotifyPartyCountryCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonBillPartiesControlBag.Instance.NotifyPartyStateDropEdit, ControlWidthClass.Auto);
				yield return (CommonBillPartiesControlBag.Instance.NotifyPartyPostcodeTextBox, ControlWidthClass.Auto);
				yield return (CommonBillPartiesControlBag.Instance.NotifyPartyPhoneTextBox, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.NotifyPartyRegNoTextBox, ControlWidthClass.Long);
				yield return (EUICS2BillPartiesControlBag.Instance.NotifyPartyPersonTypeDropEdit, ControlWidthClass.Auto);
			}
		}

		public void TestConvertBuyerToOrganizationButtonVisibility()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			var layout = ((IPanelLayoutProvider)new EUICS2BillPartiesLayouts()).Layout;

			AssertEquals(false, layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertBuyerToOrganizationButton, bill));

			bill.ABL_BuyerName = "Test";
			AssertEquals("When Buyer details filled Convert Buyer to Org visible", true, layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertBuyerToOrganizationButton, bill));
		}

		public void TestConvertMasterBillBuyerToOrganizationButtonVisibility()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var masterBill = manifestHeader.MasterBill;

			var layout = ((IPanelLayoutProvider)new EUICS2BillPartiesLayouts()).Layout;

			Assert(!layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertBuyerToOrganizationButton, masterBill));

			masterBill.ABL_BuyerName = "Test";
			Assert("When Buyer details filled Convert Buyer to Org still invisible", !layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertBuyerToOrganizationButton, masterBill));
		}

		public void TestConvertSellerToOrganizationButtonVisibility()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			var layout = ((IPanelLayoutProvider)new EUICS2BillPartiesLayouts()).Layout;

			AssertEquals(false, layout.IsVisible(EUICS2BillPartiesControlBag.Instance.ConvertSellerToOrganizationButton, bill));

			bill.ABL_SellerName = "Test";

			AssertEquals("When Seller details filled Convert Seller to Org visible", true, layout.IsVisible(EUICS2BillPartiesControlBag.Instance.ConvertSellerToOrganizationButton, bill));
		}

		public void TestConvertMasterBillSellerToOrganizationButtonVisibility()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var masterBill = manifestHeader.MasterBill;

			var layout = ((IPanelLayoutProvider)new EUICS2BillPartiesLayouts()).Layout;

			Assert(!layout.IsVisible(EUICS2BillPartiesControlBag.Instance.ConvertSellerToOrganizationButton, masterBill));

			masterBill.ABL_SellerName = "Test";
			Assert("When Seller details filled Convert Seller to Org still invisible", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.ConvertSellerToOrganizationButton, masterBill));
		}

		public void TestConvertNotifyPartyToOrganizationButtonVisibility()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			var layout = ((IPanelLayoutProvider)new EUICS2BillPartiesLayouts()).Layout;

			AssertEquals(false, layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertNotifyPartyToOrganizationButton, bill));

			bill.ABL_NotifyPartyName = "Test";

			AssertEquals("When NotifyParty details filled Convert NotifyParty to Org visible", true, layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertNotifyPartyToOrganizationButton, bill));
		}

		public void TestConvertMasterBillNotifyPartyToOrganizationButtonVisibility()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var masterBill = manifestHeader.MasterBill;

			var layout = ((IPanelLayoutProvider)new EUICS2BillPartiesLayouts()).Layout;

			Assert(!layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertNotifyPartyToOrganizationButton, masterBill));

			masterBill.ABL_NotifyPartyName = "Test";
			Assert("When Seller details filled Convert Seller to Org still invisible", !layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertNotifyPartyToOrganizationButton, masterBill));
		}

		public void TestControlsVisibility()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var masterBill = manifestHeader.MasterBill;
			var layout = ((IPanelLayoutProvider)new EUICS2BillPartiesLayouts()).Layout;

			CombineAssertions("Buyer controls should be hidden for master bill", () =>
			{
				Assert("BuyerSeparatorUserControl", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerSeparatorUserControl, masterBill));
				Assert("BuyerAddressControl", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerAddressControl, masterBill));
				Assert("BuyerNameTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerNameTextBox, masterBill));
				Assert("BuyerStreet1TextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerStreet1TextBox, masterBill));
				Assert("BuyerStreet2TextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerStreet2TextBox, masterBill));
				Assert("BuyerCityTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerCityTextBox, masterBill));
				Assert("BuyerCountryCodeFindBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerCountryCodeFindBox, masterBill));
				Assert("BuyerStateDropEdit", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerStateDropEdit, masterBill));
				Assert("BuyerPostcodeTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerPostcodeTextBox, masterBill));
				Assert("BuyerPhoneTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerPhoneTextBox, masterBill));
				Assert("BuyerRegNoTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerRegNoTextBox, masterBill));
				Assert("BuyerPersonTypeDropEdit", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.BuyerPersonTypeDropEdit, masterBill));
			});

			CombineAssertions("Buyer controls should be hidden for master bill", () =>
			{
				Assert("BuyerSeparatorUserControl", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerSeparatorUserControl, masterBill));
				Assert("BuyerAddressControl", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerAddressControl, masterBill));
				Assert("BuyerNameTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerNameTextBox, masterBill));
				Assert("BuyerStreet1TextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerStreet1TextBox, masterBill));
				Assert("BuyerStreet2TextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerStreet2TextBox, masterBill));
				Assert("BuyerCityTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerCityTextBox, masterBill));
				Assert("BuyerCountryCodeFindBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerCountryCodeFindBox, masterBill));
				Assert("BuyerStateDropEdit", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerStateDropEdit, masterBill));
				Assert("BuyerPostcodeTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerPostcodeTextBox, masterBill));
				Assert("BuyerPhoneTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerPhoneTextBox, masterBill));
				Assert("BuyerRegNoTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerRegNoTextBox, masterBill));
				Assert("BuyerPersonTypeDropEdit", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.BuyerPersonTypeDropEdit, masterBill));
			});

			CombineAssertions("Seller controls should be hidden for master bill", () =>
			{
				Assert("SellerSeparatorUserControl", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerSeparatorUserControl, masterBill));
				Assert("SellerAddressControl", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerAddressControl, masterBill));
				Assert("SellerNameTextBox", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerNameTextBox, masterBill));
				Assert("SellerStreet1TextBox", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerStreet1TextBox, masterBill));
				Assert("SellerStreet2TextBox", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerStreet2TextBox, masterBill));
				Assert("SellerCityTextBox", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerCityTextBox, masterBill));
				Assert("SellerCountryCodeFindBox", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerCountryCodeFindBox, masterBill));
				Assert("SellerStateDropEdit", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerStateDropEdit, masterBill));
				Assert("SellerPostCodeTextBox", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerPostCodeTextBox, masterBill));
				Assert("SellerPhoneTextBox", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerPhoneTextBox, masterBill));
				Assert("SellerRegNoTextBox", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerRegNoTextBox, masterBill));
				Assert("SellerPersonTypeDropEdit", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerPersonTypeDropEdit, masterBill));
			});

			CombineAssertions("Notify Party controls should be hidden for master bill", () =>
			{
				Assert("NotifyPartySeparatorUserControl", !layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartySeparatorUserControl, masterBill));
				Assert("NotifyPartyAddressControl", !layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyAddressControl, masterBill));
				Assert("NotifyPartyNameTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyNameTextBox, masterBill));
				Assert("NotifyPartyStreet1TextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStreet1TextBox, masterBill));
				Assert("NotifyPartyStreet2TextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStreet2TextBox, masterBill));
				Assert("NotifyPartyCityTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyCityTextBox, masterBill));
				Assert("NotifyPartyCountryCodeFindBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyCountryCodeFindBox, masterBill));
				Assert("NotifyPartyStateDropEdit", !layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStateDropEdit, masterBill));
				Assert("NotifyPartyPostcodeTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyPostcodeTextBox, masterBill));
				Assert("NotifyPartyPhoneTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyPhoneTextBox, masterBill));
				Assert("NotifyPartyRegNoTextBox", !layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyRegNoTextBox, masterBill));
				Assert("NotifyPartyPersonTypeDropEdit", !layout.IsVisible(EUICS2BillPartiesControlBag.Instance.NotifyPartyPersonTypeDropEdit, masterBill));
			});

			var bill = manifestHeader.Bills.AddNew();
			CombineAssertions("Buyer controls should be visible for bills", () =>
			{
				Assert("BuyerSeparatorUserControl", layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerSeparatorUserControl, bill));
				Assert("BuyerAddressControl", layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerAddressControl, bill));
				Assert("BuyerNameTextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerNameTextBox, bill));
				Assert("BuyerStreet1TextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerStreet1TextBox, bill));
				Assert("BuyerStreet2TextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerStreet2TextBox, bill));
				Assert("BuyerCityTextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerCityTextBox, bill));
				Assert("BuyerCountryCodeFindBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerCountryCodeFindBox, bill));
				Assert("BuyerStateDropEdit", layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerStateDropEdit, bill));
				Assert("BuyerPostcodeTextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerPostcodeTextBox, bill));
				Assert("BuyerPhoneTextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerPhoneTextBox, bill));
				Assert("BuyerRegNoTextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.BuyerRegNoTextBox, bill));
				Assert("BuyerPersonTypeDropEdit", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.BuyerPersonTypeDropEdit, bill));
			});

			CombineAssertions("Seller controls should be visible for bills", () =>
			{
				Assert("SellerSeparatorUserControl", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerSeparatorUserControl, bill));
				Assert("SellerAddressControl", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerAddressControl, bill));
				Assert("SellerNameTextBox", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerNameTextBox, bill));
				Assert("SellerStreet1TextBox", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerStreet1TextBox, bill));
				Assert("SellerStreet2TextBox", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerStreet2TextBox, bill));
				Assert("SellerCityTextBox", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerCityTextBox, bill));
				Assert("SellerCountryCodeFindBox", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerCountryCodeFindBox, bill));
				Assert("SellerStateDropEdit", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerStateDropEdit, bill));
				Assert("SellerPostCodeTextBox", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerPostCodeTextBox, bill));
				Assert("SellerPhoneTextBox", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerPhoneTextBox, bill));
				Assert("SellerRegNoTextBox", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerRegNoTextBox, bill));
				Assert("SellerPersonTypeDropEdit", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.SellerPersonTypeDropEdit, bill));
			});

			CombineAssertions("Notify Party controls should be visible for bills", () =>
			{
				Assert("NotifyPartySeparatorUserControl", layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartySeparatorUserControl, bill));
				Assert("NotifyPartyAddressControl", layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyAddressControl, bill));
				Assert("NotifyPartyNameTextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyNameTextBox, bill));
				Assert("NotifyPartyStreet1TextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStreet1TextBox, bill));
				Assert("NotifyPartyStreet2TextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStreet2TextBox, bill));
				Assert("NotifyPartyCityTextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyCityTextBox, bill));
				Assert("NotifyPartyCountryCodeFindBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyCountryCodeFindBox, bill));
				Assert("NotifyPartyStateDropEdit", layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStateDropEdit, bill));
				Assert("NotifyPartyPostcodeTextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyPostcodeTextBox, bill));
				Assert("NotifyPartyPhoneTextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyPhoneTextBox, bill));
				Assert("NotifyPartyRegNoTextBox", layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyRegNoTextBox, bill));
				Assert("NotifyPartyPersonTypeDropEdit", layout.IsVisible(EUICS2BillPartiesControlBag.Instance.NotifyPartyPersonTypeDropEdit, bill));
			});
		}

		protected override IPanelLayoutProvider GetNewPanelLayoutProvider()
		{
			return base.GetNewPanelLayoutProvider();
		}
	}
}

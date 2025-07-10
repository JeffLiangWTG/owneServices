using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(JPBillPartiesLayout))]
	sealed class JPBillPartiesLayoutsTest : LayoutsAbstractTest
	{
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
				yield return (JPBillPartiesControlBag.Instance.ShipperRegNoPanel, ControlWidthClass.Long);
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
				yield return (JPBillPartiesControlBag.Instance.ConsigneeRegNoPanel, ControlWidthClass.Long);
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
				yield return (JPBillPartiesControlBag.Instance.NotifyPartyRegNoPanel, ControlWidthClass.Long);
			}
		}

		public void TestVisibility()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;

			AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ShipperRegNoTextBox, bill));
			AssertEquals(false, Layout.IsVisible(JPBillPartiesControlBag.Instance.ShipperRegNoPanel, bill));
			AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConsigneeRegoNoTextBox, bill));
			AssertEquals(false, Layout.IsVisible(JPBillPartiesControlBag.Instance.ConsigneeRegNoPanel, bill));
			AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyRegNoTextBox, bill));
			AssertEquals(false, Layout.IsVisible(JPBillPartiesControlBag.Instance.NotifyPartyRegNoPanel, bill));

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConsigneeRegoNoTextBox, bill));
			AssertEquals(true, Layout.IsVisible(JPBillPartiesControlBag.Instance.ConsigneeRegNoPanel, bill));

			AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertConsigneeToOrganizationButton, bill));
			bill.ABL_ConsigneeRegNoType = ZString.Empty;
			AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertConsigneeToOrganizationButton, bill));

			bill.ABL_ConsigneeRegNo = "1235467890123";
			AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertConsigneeToOrganizationButton, bill));

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ShipperRegNoTextBox, bill));
			AssertEquals(true, Layout.IsVisible(JPBillPartiesControlBag.Instance.ShipperRegNoPanel, bill));
			AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConsigneeRegoNoTextBox, bill));
			AssertEquals(true, Layout.IsVisible(JPBillPartiesControlBag.Instance.ConsigneeRegNoPanel, bill));
			AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyRegNoTextBox, bill));
			AssertEquals(true, Layout.IsVisible(JPBillPartiesControlBag.Instance.NotifyPartyRegNoPanel, bill));

			bill.ABL_ShipperRegNoType = OrgCusCode.JapanCodeTypes.JAS;
			AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertShipperToOrganizationButton, bill));
			bill.ABL_ShipperRegNoType = ZString.Empty;
			AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertShipperToOrganizationButton, bill));
			bill.ABL_ShipperRegNo = "1235467890123";
			AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertShipperToOrganizationButton, bill));

			AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertConsigneeToOrganizationButton, bill));
			bill.ABL_ConsigneeRegNoType = ZString.Empty;
			bill.ABL_ConsigneeRegNo = ZString.Empty;
			AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertConsigneeToOrganizationButton, bill));
			bill.ABL_ConsigneeRegNo = "1235467890123";
			AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertConsigneeToOrganizationButton, bill));

			bill.ABL_NotifyPartyRegNoType = OrgCusCode.JapanCodeTypes.JAS;
			AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertNotifyPartyToOrganizationButton, bill));
			bill.ABL_NotifyPartyRegNoType = ZString.Empty;
			AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertNotifyPartyToOrganizationButton, bill));
			bill.ABL_NotifyPartyRegNo = "1235467890123";
			AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertNotifyPartyToOrganizationButton, bill));
		}

		public void TestNotifyPartyVisibility()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartySeparatorUserControl, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyAddressControl, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertNotifyPartyToOrganizationButton, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyNameTextBox, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStreet1TextBox, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStreet2TextBox, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyCityTextBox, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyCountryCodeFindBox, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStateDropEdit, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyPostcodeTextBox, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyPhoneTextBox, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyRegNoTextBox, bill));
			});

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			CombineAssertions(() =>
			{
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartySeparatorUserControl, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyAddressControl, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertNotifyPartyToOrganizationButton, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyNameTextBox, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStreet1TextBox, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStreet2TextBox, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyCityTextBox, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyCountryCodeFindBox, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStateDropEdit, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyPostcodeTextBox, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyPhoneTextBox, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyRegNoTextBox, bill));
			});

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			CombineAssertions(() =>
			{
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartySeparatorUserControl, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyAddressControl, bill));
				AssertEquals(false, Layout.IsVisible(CommonBillPartiesControlBag.Instance.ConvertNotifyPartyToOrganizationButton, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyNameTextBox, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStreet1TextBox, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStreet2TextBox, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyCityTextBox, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyCountryCodeFindBox, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyStateDropEdit, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyPostcodeTextBox, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyPhoneTextBox, bill));
				AssertEquals(true, Layout.IsVisible(CommonBillPartiesControlBag.Instance.NotifyPartyRegNoTextBox, bill));
			});
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillPartiesLayoutBuilder<AsycudaBill>();

		PanelLayout Layout => layout ??= new JPBillPartiesLayout().Layout;
		PanelLayout layout;
	}
}

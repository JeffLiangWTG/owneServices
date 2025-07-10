using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.CO.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.GUI.Testing
{
	[TestedType(typeof(COBillPartiesLayouts))]
	sealed class COBillPartiesLayoutsTest : LayoutsAbstractTest
	{
		public void TestCOSpecificFieldsVisibilty()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.FillWithValidTestData();
			var bill = manifest.Bills.AddNew();

			Factory.Save();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingleOrDefault<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "mainTabControl");

				var billsAndPacksTabPage = mainTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;

				var billsAndPacksTabControl = mainTabControl.FindSingleOrDefault<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billsTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billPartiesTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;

				var asycudaBillPartiesUserControl = billsTabPage.FindSingleOrDefault<AsycudaBillPartiesUserControl>(c => c.Name == "asycudaBillPartiesUserControl");

				AssertNull("NotifyPartySeparatorUserControl", asycudaBillPartiesUserControl.FindSingleOrDefault<ZUserControl>(c => c.Name == "NotifyPartySeparatorUserControl"));
				AssertNull("NotifyPartyAddressControl", asycudaBillPartiesUserControl.FindSingleOrDefault<ZUserControl>(c => c.Name == "NotifyPartyAddressControl"));
				AssertNull("ConvertNotifyPartyToOrganizationButton", asycudaBillPartiesUserControl.FindSingleOrDefault<ZButton>(c => c.Name == "ConvertNotifyPartyToOrganizationButton"));
				AssertNull("NotifyPartyNameTextBox", asycudaBillPartiesUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "NotifyPartyNameTextBox"));
				AssertNull("NotifyPartyStreet1TextBox", asycudaBillPartiesUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "NotifyPartyStreet1TextBox"));
				AssertNull("NotifyPartyStreet2TextBox", asycudaBillPartiesUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "NotifyPartyStreet2TextBox"));
				AssertNull("NotifyPartyCityTextBox", asycudaBillPartiesUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "NotifyPartyCityTextBox"));
				AssertNull("NotifyPartyCountryCodeFindBox", asycudaBillPartiesUserControl.FindSingleOrDefault<ZCodeFindBox>(c => c.Name == "NotifyPartyCountryCodeFindBox"));
				AssertNull("NotifyPartyStateDropEdit", asycudaBillPartiesUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "NotifyPartyStateDropEdit"));
				AssertNull("NotifyPartyPostcodeTextBox", asycudaBillPartiesUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "NotifyPartyPostcodeTextBox"));
				AssertNull("NotifyPartyPhoneTextBox", asycudaBillPartiesUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "NotifyPartyPhoneTextBox"));
				AssertNull("NotifyPartyRegNoTypeDropEdit", asycudaBillPartiesUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "NotifyPartyRegNoTypeDropEdit"));
				AssertNull("NotifyPartyRegNoTextBox", asycudaBillPartiesUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "NotifyPartyRegNoTextBox"));
			}
		}

		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
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
				yield return (CommonBillPartiesControlBag.Instance.ShipperRegNoTypeDropEdit, ControlWidthClass.Long);
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
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeRegNoTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeRegoNoTextBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillPartiesLayoutBuilder<AsycudaBill>();
	}
}

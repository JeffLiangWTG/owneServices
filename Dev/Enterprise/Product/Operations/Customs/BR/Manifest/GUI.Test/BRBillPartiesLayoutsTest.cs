using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.BR.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Manifest.GUI.Testing
{
	[TestedType(typeof(BRBillPartiesLayouts))]
	sealed class BRBillPartiesLayoutsTest : LayoutsAbstractTest
	{
		public void TestSpecificFieldsVisibilty()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();

			Factory.Save();

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();

				var billsAndPacksTabControl = form.FindSingleOrDefault<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var billsTabPage = form.FindSingleOrDefault<ZTabPage>(c => c.Name == "billPartiesTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;

				var asycudaBillPartiesUserControl = billsTabPage.FindSingleOrDefault<AsycudaBillPartiesUserControl>(c => c.Name == "asycudaBillPartiesUserControl");

				var consigneeRegNoType = asycudaBillPartiesUserControl.FindSingle<ZDropEdit>(nameof(CommonBillPartiesControlBag.ConsigneeRegNoTypeDropEdit));
				var notifyPartyRegNoType = asycudaBillPartiesUserControl.FindSingle<ZDropEdit>(nameof(CommonBillPartiesControlBag.NotifyPartyRegNoTypeDropEdit));

				AssertEquals(consigneeRegNoType.Name, false, consigneeRegNoType.Visible);
				AssertEquals(notifyPartyRegNoType.Name, false, notifyPartyRegNoType.Visible);

				manifest.AMA_ManifestType = BRManifestTypes.Codes.MER;

				AssertEquals(consigneeRegNoType.Name, true, consigneeRegNoType.Visible);
				AssertEquals(notifyPartyRegNoType.Name, true, notifyPartyRegNoType.Visible);
			}
		}

		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
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
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeRegNoTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.ConsigneeRegoNoTextBox, ControlWidthClass.Long);
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
				yield return (CommonBillPartiesControlBag.Instance.NotifyPartyRegNoTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillPartiesControlBag.Instance.NotifyPartyRegNoTextBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillPartiesLayoutBuilder<AsycudaBill>();
	}
}

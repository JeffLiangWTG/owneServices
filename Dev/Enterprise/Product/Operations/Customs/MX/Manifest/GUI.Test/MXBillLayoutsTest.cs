using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.GUI.Testing
{
	[TestedType(typeof(MXBillLayouts))]
	sealed class MXBillLayoutsTest : LayoutsAbstractTest
	{
		public void TestMXSpecificFiedsVisibilty()
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
				var billsTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;
				var asycudaBillUserControl = billsTabPage.FindSingleOrDefault<AsycudaBillUserControl>(c => c.Name == "asycudaBillUserControl");

				var messageStatus = asycudaBillUserControl.FindSingle<ZTextBox>(nameof(CommonBillControlBag.MessageStatusTextBox));
				var customsEntryNumber = asycudaBillUserControl.FindSingle<ZTextBox>(nameof(CommonBillControlBag.CustomsEntryNumberTextBox));
				var billStatus = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(nameof(CommonBillControlBag.BillStatusDropEdit));

				AssertEquals("MessageStatus", true, messageStatus.Visible);
				AssertEquals("CustomsEntryNumber", true, customsEntryNumber.Visible);
				AssertEquals("BillStatus", true, billStatus.Visible);

				AssertEquals("MessageStatus", true, messageStatus.ReadOnly);
				AssertEquals("CustomsEntryNumber", true, customsEntryNumber.ReadOnly);
				AssertEquals("BillStatus", true, billStatus.ReadOnly);
			}
		}

		public void TestSeaManifestSpecificFieldsVisibility()
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
				var billsTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;
				var asycudaBillUserControl = billsTabPage.FindSingleOrDefault<AsycudaBillUserControl>(c => c.Name == "asycudaBillUserControl");

				var freightValueConvertToLocalCurrencyControl = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "FreightValueConvertToLocalCurrencyControl");
				var transportValueConvertToLocalCurrencyControl = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "TransportValueConvertToLocalCurrencyControl");
				var insuranceValueConvertToLocalCurrencyControl = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "InsuranceValueConvertToLocalCurrencyControl");
				var discountValueConvertToLocalCurrencyControl = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "DiscountValueConvertToLocalCurrencyControl");
				var otherChargesValueConvertToLocalCurrencyControl = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "OtherChargesValueConvertToLocalCurrencyControl");
				var customsValueConvertToLocalCurrencyControl = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "CustomsValueConvertToLocalCurrencyControl");

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;

				AssertEquals("FreightValueConvertToLocalCurrencyControl", false, freightValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("TransportValueConvertToLocalCurrencyControl", false, transportValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("InsuranceValueConvertToLocalCurrencyControl", false, insuranceValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("DiscountValueConvertToLocalCurrencyControl", false, discountValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("OtherChargesValueConvertToLocalCurrencyControl", false, otherChargesValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("CustomsValueConvertToLocalCurrencyControl", false, customsValueConvertToLocalCurrencyControl.Visible);
			}
		}

		public void TestAirManifestSpecificFieldsVisibility()
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
				var billsTabPage = billsAndPacksTabControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "billsTabPage");
				billsAndPacksTabControl.SelectedTab = billsTabPage;
				var asycudaBillUserControl = billsTabPage.FindSingleOrDefault<AsycudaBillUserControl>(c => c.Name == "asycudaBillUserControl");

				var freightValueConvertToLocalCurrencyControl = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "FreightValueConvertToLocalCurrencyControl");
				var transportValueConvertToLocalCurrencyControl = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "TransportValueConvertToLocalCurrencyControl");
				var insuranceValueConvertToLocalCurrencyControl = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "InsuranceValueConvertToLocalCurrencyControl");
				var discountValueConvertToLocalCurrencyControl = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "DiscountValueConvertToLocalCurrencyControl");
				var otherChargesValueConvertToLocalCurrencyControl = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "OtherChargesValueConvertToLocalCurrencyControl");
				var customsValueConvertToLocalCurrencyControl = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "CustomsValueConvertToLocalCurrencyControl");

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;

				AssertEquals("FreightValueConvertToLocalCurrencyControl", true, freightValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("TransportValueConvertToLocalCurrencyControl", true, transportValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("InsuranceValueConvertToLocalCurrencyControl", true, insuranceValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("DiscountValueConvertToLocalCurrencyControl", true, discountValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("OtherChargesValueConvertToLocalCurrencyControl", true, otherChargesValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("CustomsValueConvertToLocalCurrencyControl", true, customsValueConvertToLocalCurrencyControl.Visible);

				AssertEquals("Freight Value", freightValueConvertToLocalCurrencyControl.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Goods Value", transportValueConvertToLocalCurrencyControl.GetExtension<LabelCaptionRenderer>().Caption);
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
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsEntryNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillStatusDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.RemarksTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.AgentAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CarrierReferenceTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.PrepaidCollectDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();
	}
}

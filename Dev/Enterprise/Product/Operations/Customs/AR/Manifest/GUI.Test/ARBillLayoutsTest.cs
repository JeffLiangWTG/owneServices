using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Customs.AR.Manifest.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.GUI.Testing
{
	[TestedType(typeof(ARBillLayouts))]
	sealed class ARBillLayoutsTest : LayoutsAbstractTest
	{
		public void TestARSpecificFiedsVisibilty()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
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

				var messageStatus = asycudaBillUserControl.FindSingleOrDefault<ZTextBox>(nameof(CommonBillControlBag.MessageStatusTextBox));
				var billStatus = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(nameof(CommonBillControlBag.BillStatusDropEdit));

				AssertEquals("IsMonitoredTransitCheckBox", false, asycudaBillUserControl.FindSingle<ZCheckBox>(nameof(ARBillControlBag.IsMonitoredTransitCheckBox)).Visible);
				AssertEquals("IsInformedToRenarCheckBox", false, asycudaBillUserControl.FindSingle<ZCheckBox>(nameof(ARBillControlBag.IsInformedToRenarCheckBox)).Visible);

				AssertEquals("FreightValueConvertToLocalCurrencyControl", true, freightValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("TransportValueConvertToLocalCurrencyControl", true, transportValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("InsuranceValueConvertToLocalCurrencyControl", true, insuranceValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("DiscountValueConvertToLocalCurrencyControl", true, discountValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("OtherChargesValueConvertToLocalCurrencyControl", true, otherChargesValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("CustomsValueConvertToLocalCurrencyControl", true, customsValueConvertToLocalCurrencyControl.Visible);

				AssertEquals("MessageStatus", true, messageStatus.Visible);
				AssertEquals("BillStatus", true, billStatus.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;

				AssertEquals("IsMonitoredTransitCheckBox", true, asycudaBillUserControl.FindSingle<ZCheckBox>(nameof(ARBillControlBag.IsMonitoredTransitCheckBox)).Visible);
				AssertEquals("IsInformedToRenarCheckBox", true, asycudaBillUserControl.FindSingle<ZCheckBox>(nameof(ARBillControlBag.IsInformedToRenarCheckBox)).Visible);

				AssertEquals("FreightValueConvertToLocalCurrencyControl", false, freightValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("TransportValueConvertToLocalCurrencyControl", false, transportValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("InsuranceValueConvertToLocalCurrencyControl", false, insuranceValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("DiscountValueConvertToLocalCurrencyControl", false, discountValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("OtherChargesValueConvertToLocalCurrencyControl", false, otherChargesValueConvertToLocalCurrencyControl.Visible);
				AssertEquals("CustomsValueConvertToLocalCurrencyControl", false, customsValueConvertToLocalCurrencyControl.Visible);

				AssertEquals("Freight Value", freightValueConvertToLocalCurrencyControl.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Goods Value", transportValueConvertToLocalCurrencyControl.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.DepartureDateEdit, ControlWidthClass.Auto);
				yield return (CommonBillControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Long);
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
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.PrepaidCollectDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (ARBillControlBag.Instance.IsMonitoredTransitCheckBox, ControlWidthClass.Auto);
				yield return (ARBillControlBag.Instance.IsInformedToRenarCheckBox, ControlWidthClass.Auto);
			}
		}
	}
}

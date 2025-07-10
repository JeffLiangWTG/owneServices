using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.GUI.Testing
{
	[TestedType(typeof(CLBillLayouts))]
	sealed class CLBillLayoutsTest : LayoutsAbstractTest
	{
		public void TestCLSpecificFiedsVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
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
				var billStatus = asycudaBillUserControl.FindSingle<ZDropEdit>(nameof(CommonBillControlBag.BillStatusDropEdit));

				AssertEquals("MessageStatus", true, messageStatus.Visible);
				AssertEquals("CustomsEntryNumber", true, customsEntryNumber.Visible);
				AssertEquals("BillStatus", true, billStatus.Visible);

				AssertEquals("MessageStatus", true, messageStatus.ReadOnly);
				AssertEquals("CustomsEntryNumber", true, customsEntryNumber.ReadOnly);
				AssertEquals("BillStatus", true, billStatus.ReadOnly);

				var goodsLocationsOrg = asycudaBillUserControl.FindSingleOrDefault<ZAddressControl>(c => c.Name == "GoodsLocationAddressControl");

				AssertEquals("GoodsLocationAddressControl", true, goodsLocationsOrg.Visible);

				AssertNull("ShipmentType", asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "ShipmentType"));
				AssertNull("LocationInformation", asycudaBillUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "LocationInformation"));
				AssertNull("AgentAddressControl", asycudaBillUserControl.FindSingleOrDefault<ZAddressControl>(c => c.Name == "AgentAddressControl"));
				AssertNull("ABL_CarrierReferenceTextBox", asycudaBillUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "ABL_CarrierReferenceTextBox"));
				AssertNull("TransportValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "TransportValueConvertToLocalCurrencyControl"));
				AssertNull("InsuranceValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "InsuranceValueConvertToLocalCurrencyControl"));
				AssertNull("DiscountValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "DiscountValueConvertToLocalCurrencyControl"));
				AssertNull("OtherChargesValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "OtherChargesValueConvertToLocalCurrencyControl"));
				AssertNull("CustomsValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "CustomsValueConvertToLocalCurrencyControl"));

				var roro = asycudaBillUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "RoRoCheckBox");
				var freightValue = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "FreightValueConvertToLocalCurrencyControl");

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals(false, roro.Visible);
				AssertEquals(true, freightValue.Visible);

				manifest.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals(true, roro.Visible);
				AssertEquals(false, freightValue.Visible);
				AssertEquals("Freight Value", freightValue.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		protected override int ControlBagCount => 2;

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
				yield return (CommonBillControlBag.Instance.BillNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.OriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FinalDestinationCodeFindBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GrossWeightCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GoodsLocationAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.CustomsEntryNumberTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.BillStatusDropEdit, ControlWidthClass.Long);
				yield return (CLBillControlBag.Instance.RoRoCheckBox, ControlWidthClass.Auto);
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
				yield return (CommonBillControlBag.Instance.PrepaidCollectDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();
	}
}

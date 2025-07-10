using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.CO.Manifest.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.GUI.Testing
{
	[TestedType(typeof(COBillLayouts))]
	sealed class COBillLayoutsTest : LayoutsAbstractTest
	{
		public void TestCOSpecificFiedsVisibilty()
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

				var cargoDisposition = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "CargoDispositionDropEdit");
				var travelDocumentType = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "TravelDocumentTypeDropEdit");
				var multimodal = asycudaBillUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "MultimodalCheckBox");
				var carriersLiability = asycudaBillUserControl.FindSingleOrDefault<ZCheckBox>(c => c.Name == "CarriersLiabilityCheckBox");
				var goodsLocationsOrg = asycudaBillUserControl.FindSingleOrDefault<ZAddressControl>(c => c.Name == "GoodsLocationAddressControl");
				var goodsLocationsOrg2 = asycudaBillUserControl.FindSingleOrDefault<ZDropEditWithFixedWidth>(c => c.Name == "GoodsLocationDropEditWithFixedWidth");
				var containerMode = asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "ContainerModeDropEdit");

				var freightValue = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "FreightValueConvertToLocalCurrencyControl");
				var goodsValue = asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "GoodsValueConvertToLocalCurrencyControl");

				var issueDate = asycudaBillUserControl.FindSingleOrDefault<ZDateEdit>(c => c.Name == "BillIssueDateEdit");

				AssertNull("NotifyPartyAddressControl", asycudaBillUserControl.FindSingleOrDefault<ZAddressControl>(c => c.Name == "NotifyPartyAddressControl"));
				AssertNull("InsuranceValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "InsuranceValueConvertToLocalCurrencyControl"));
				AssertNull("DiscountValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "DiscountValueConvertToLocalCurrencyControl"));
				AssertNull("OtherChargesValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "OtherChargesValueConvertToLocalCurrencyControl"));
				AssertNull("CustomsValueConvertToLocalCurrencyControl", asycudaBillUserControl.FindSingleOrDefault<ConvertToLocalCurrencyControl>(c => c.Name == "CustomsValueConvertToLocalCurrencyControl"));

				AssertNull("ShipmentTypeDropEdit", asycudaBillUserControl.FindSingleOrDefault<ZDropEdit>(c => c.Name == "ShipmentTypeDropEdit"));
				AssertNull("LocationInformationTextBox", asycudaBillUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "LocationInformationTextBox"));
				AssertNull("CarrierReferenceTextBox", asycudaBillUserControl.FindSingleOrDefault<ZTextBox>(c => c.Name == "CarrierReferenceTextBox"));

				manifest.AMA_TransportMode = "SEA";
				AssertEquals("CargoDispositionDropEdit", true, cargoDisposition.Visible);
				AssertEquals("TravelDocumentTypeDropEdit", true, travelDocumentType.Visible);
				AssertEquals("MultimodalCheckBox", true, multimodal.Visible);
				AssertEquals("GoodsLocationAddressControl", true, goodsLocationsOrg.Visible);
				AssertEquals("GoodsLocationDropEditWithFixedWidth", false, goodsLocationsOrg2.Visible);
				AssertEquals("ContainerModeDropEdit", true, containerMode.Visible);

				manifest.AMA_TransportMode = "AIR";
				AssertEquals("CargoDispositionDropEdit", false, cargoDisposition.Visible);
				AssertEquals("TravelDocumentTypeDropEdit", false, travelDocumentType.Visible);
				AssertEquals("MultimodalCheckBox", false, multimodal.Visible);
				AssertEquals("GoodsLocationAddressControl", false, goodsLocationsOrg.Visible);
				AssertEquals("GoodsLocationDropEditWithFixedWidth", true, goodsLocationsOrg2.Visible);
				AssertEquals("ContainerModeDropEdit", false, containerMode.Visible);

				AssertEquals("Freight Value", freightValue.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Goods Value FOB", goodsValue.GetExtension<LabelCaptionRenderer>().Caption);

				AssertEquals("BillIssueDateEdit", true, issueDate.Visible);
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
				yield return (CommonBillControlBag.Instance.GoodsLocationDropEditWithFixedWidth, ControlWidthClass.Long);
				yield return (COBillControlBag.Instance.BillIssueDateEdit, ControlWidthClass.Auto);
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
				yield return (CommonBillControlBag.Instance.AgentAddressControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonBillControlBag.Instance.PrepaidCollectDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (COBillControlBag.Instance.GoodsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
				yield return (COBillControlBag.Instance.TravelDocumentTypeDropEdit, ControlWidthClass.Long);
				yield return (COBillControlBag.Instance.CargoDispositionDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (COBillControlBag.Instance.MultimodalCheckBox, ControlWidthClass.Long);
				yield return (COBillControlBag.Instance.CarriersLiabilityCheckBox, ControlWidthClass.Long);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();
	}
}

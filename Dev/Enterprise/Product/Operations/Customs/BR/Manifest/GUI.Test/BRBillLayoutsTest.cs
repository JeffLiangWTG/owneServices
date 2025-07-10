using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.BR.Manifest.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Manifest.GUI.Testing
{
	[TestedType(typeof(BRBillLayouts))]
	sealed class BRBillLayoutsTest : LayoutsAbstractTest
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();

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
				yield return (CommonBillControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
				yield return (BRBillControlBag.Instance.DocumentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GoodsLocationDropEditWithFixedWidth, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.LocationInformationTextBox, ControlWidthClass.Long);
				yield return (BRBillControlBag.Instance.SellerCountryCodeFindBox, ControlWidthClass.Long);
				yield return (BRBillControlBag.Instance.CEMercanteTextBox, ControlWidthClass.Long);
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
				yield return (BRBillControlBag.Instance.ToOrderCheckBox, ControlWidthClass.Long);
				yield return (BRBillControlBag.Instance.BLServiceCheckBox, ControlWidthClass.Long);
				yield return (BRBillControlBag.Instance.FRTModeDropEdit, ControlWidthClass.Long);
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
			}
		}

		public void TestSpecificFiedsVisibilty()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();

			using (var form = new AsycudaBillForm(bill))
			{
				form.Show();

				var documentTypeDropEdit = form.FindSingle<ZDropEdit>(c => c.Name == "DocumentTypeDropEdit");
				var toOrderCheckBox = form.FindSingle<ZCheckBox>(c => c.Name == "ToOrderCheckBox");
				var blServiceCheckBox = form.FindSingle<ZCheckBox>(c => c.Name == "BLServiceCheckBox");
				var freigtModeDropEdit = form.FindSingle<ZDropEdit>(c => c.Name == "FRTModeDropEdit");
				var sellerCountryCodeFindBox = form.FindSingle<ZCodeFindBox>(c => c.Name == "SellerCountryCodeFindBox");
				var freightValueConvertToLocalCurrencyControl = form.FindSingle<ConvertToLocalCurrencyControl>(c => c.Name == "FreightValueConvertToLocalCurrencyControl");
				var transportValueConvertToLocalCurrencyControl = form.FindSingle<ConvertToLocalCurrencyControl>(c => c.Name == "TransportValueConvertToLocalCurrencyControl");
				var cEMercanteTextBox = form.FindSingle<ZTextBox>(c => c.Name == "CEMercanteTextBox");

				AssertEquals(documentTypeDropEdit.Name, true, documentTypeDropEdit.Visible);
				AssertEquals(toOrderCheckBox.Name, false, toOrderCheckBox.Visible);
				AssertEquals(blServiceCheckBox.Name, false, blServiceCheckBox.Visible);
				AssertEquals(freigtModeDropEdit.Name, false, freigtModeDropEdit.Visible);
				AssertEquals(sellerCountryCodeFindBox.Name, false, sellerCountryCodeFindBox.Visible);
				AssertEquals(transportValueConvertToLocalCurrencyControl.Name, true, transportValueConvertToLocalCurrencyControl.Visible);
				AssertEquals(cEMercanteTextBox.Name, false, cEMercanteTextBox.Visible);

				manifest.AMA_ManifestType = BRManifestTypes.Codes.MER;

				AssertEquals(documentTypeDropEdit.Name, true, documentTypeDropEdit.Visible);
				AssertEquals(toOrderCheckBox.Name, true, toOrderCheckBox.Visible);
				AssertEquals(blServiceCheckBox.Name, true, blServiceCheckBox.Visible);
				AssertEquals(freigtModeDropEdit.Name, true, freigtModeDropEdit.Visible);
				AssertEquals(sellerCountryCodeFindBox.Name, true, sellerCountryCodeFindBox.Visible);
				AssertEquals(transportValueConvertToLocalCurrencyControl.Name, false, transportValueConvertToLocalCurrencyControl.Visible);
				AssertEquals(cEMercanteTextBox.Name, true, cEMercanteTextBox.Visible);

				AssertEquals("Freight Value", freightValueConvertToLocalCurrencyControl.GetExtension<LabelCaptionRenderer>().Caption);
				AssertEquals("Goods Value", transportValueConvertToLocalCurrencyControl.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}
	}
}

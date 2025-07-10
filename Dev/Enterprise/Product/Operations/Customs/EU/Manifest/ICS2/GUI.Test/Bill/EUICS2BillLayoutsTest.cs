using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using AsycudaBill = Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill;
using AsycudaManifestHeader = Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	[TestedType(typeof(EUICS2BillLayouts))]
	sealed class EUICS2BillLayoutsTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
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
				yield return (CommonBillControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.PrepaidCollectDropEdit, ControlWidthClass.Long);
				yield return (EUICS2BillControlBag.Instance.FreightValueAndCurrencyCalcFindBox, ControlWidthClass.Long);
				yield return (EUICS2BillControlBag.Instance.ReceptacleIdTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EUICS2BillControlBag.Instance.TransportDocumentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.MarksAndNumbersTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.RemarksTextBox, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ShipperAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.AgentAddressControl, ControlWidthClass.Long);
			}
		}

		public void TestReceptacleVisibility()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var bill = manifestHeader.Bills.AddNew();
			var layout = new EUICS2BillLayouts().Layout;

			AssertEquals("IsCarrierManifest: ReceptacleIdTextBox NOT visible", false, layout.IsVisible(EUICS2BillControlBag.Instance.ReceptacleIdTextBox, bill));

			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
			AssertEquals("IsForwarderManifest and SpecificCircumstanceIndicator is F44: ReceptacleIdTextBox visible", true, layout.IsVisible(EUICS2BillControlBag.Instance.ReceptacleIdTextBox, bill));

			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;
			AssertEquals("IsForwarderManifest and SpecificCircumstanceIndicator is F43: ReceptacleIdTextBox not visible", false, layout.IsVisible(EUICS2BillControlBag.Instance.ReceptacleIdTextBox, bill));
		}

		public void TestPostalChargesVisibility()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var layout = new EUICS2BillLayouts().Layout;

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Rail;

			CombineAssertions(() =>
			{
				AssertEquals("Default: Postal Charges is not visible", false, layout.IsVisible(EUICS2BillControlBag.Instance.FreightValueAndCurrencyCalcFindBox, bill));

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;
				AssertEquals("When SpecificCircumstanceIndicator is F43 Postal Charges visible", true, layout.IsVisible(EUICS2BillControlBag.Instance.FreightValueAndCurrencyCalcFindBox, bill));

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
				AssertEquals("When SpecificCircumstanceIndicator is not F43 Postal Charges not visible", false, layout.IsVisible(EUICS2BillControlBag.Instance.FreightValueAndCurrencyCalcFindBox, bill));
			});
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();
	}
}

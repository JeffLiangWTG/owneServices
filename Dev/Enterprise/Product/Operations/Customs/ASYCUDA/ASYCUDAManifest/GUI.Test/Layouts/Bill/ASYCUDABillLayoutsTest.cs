using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ASYCUDAManifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDAManifest.GUI.Testing
{
	[TestedType(typeof(ASYCUDABillLayouts))]
	sealed class ASYCUDABillLayoutsTest : LayoutsAbstractTest
	{
		public void TestBDEGMFiledsVisibilty()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var layout = ((IPanelLayoutProvider)new ASYCUDABillLayouts()).Layout;
			CombineAssertions(() =>
			{
				AssertBDEGMVisibility("Not BD country", false, layout, bill);
				manifest.AMA_RN_NKCountry = "BD";
				AssertBDEGMVisibility("Not export manifest", false, layout, bill);
				manifest.AMA_Nature = "EXP";
				AssertBDEGMVisibility("BD country and export manifest", true, layout, bill);
				manifest.AMA_Nature = "IMP";
				AssertBDEGMVisibility("BD country and import manifest", false, layout, bill);
			});
		}

		void AssertBDEGMVisibility(string message, bool expectedVisible, PanelLayout layout, AsycudaBill bill)
		{
			AssertEquals(message + ": BDEGMSeparatorUserControl", expectedVisible, layout.IsVisible(ASYCUDABillControlBag.Instance.BDEGMSeparatorUserControl, bill));
			AssertEquals(message + ": SADOfficeCodeDropEdit", expectedVisible, layout.IsVisible(ASYCUDABillControlBag.Instance.SADOfficeCodeDropEdit, bill));
			AssertEquals(message + ": SADRegistrationSerialTextBox", expectedVisible, layout.IsVisible(ASYCUDABillControlBag.Instance.SADRegistrationSerialTextBox, bill));
			AssertEquals(message + ": SADRegistrationNumberTextBox", expectedVisible, layout.IsVisible(ASYCUDABillControlBag.Instance.SADRegistrationNumberTextBox, bill));
			AssertEquals(message + ": SADRegistrationDateEdit", expectedVisible, layout.IsVisible(ASYCUDABillControlBag.Instance.SADRegistrationDateEdit, bill));
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
				yield return (CommonBillControlBag.Instance.ShipmentTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.GoodsLocationDropEditWithFixedWidth, ControlWidthClass.Long);
				yield return (CommonBillControlBag.Instance.LocationInformationTextBox, ControlWidthClass.Long);
				yield return (ASYCUDABillControlBag.Instance.BDEGMSeparatorUserControl, ControlWidthClass.Long);
				yield return (ASYCUDABillControlBag.Instance.SADOfficeCodeDropEdit, ControlWidthClass.Long);
				yield return (ASYCUDABillControlBag.Instance.SADRegistrationSerialTextBox, ControlWidthClass.Long);
				yield return (ASYCUDABillControlBag.Instance.SADRegistrationNumberTextBox, ControlWidthClass.Long);
				yield return (ASYCUDABillControlBag.Instance.SADRegistrationDateEdit, ControlWidthClass.Long);
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
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();
	}
}

using System.Collections.Generic;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.GUI.Testing;

[TestedType(typeof(BillDetailsLayout))]
sealed class BillDetailsLayoutTest : LayoutsAbstractTest
{
	public void TestCaption()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();

		Layout.TryGetCaption(CommonBag.AgentAddressControl, bill, out var resourceStringData);
		AssertEquals("Origin Agent", resourceStringData.Caption);
	}

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
			yield return (CommonBag.BillNumberTextBox, ControlWidthClass.Long);
			yield return (AEBag.SplitBillCheckBox, ControlWidthClass.Long);
			yield return (AEBag.SplitBillNumberCodeFindBox, ControlWidthClass.Long);
			yield return (CommonBag.OriginCodeFindBox, ControlWidthClass.Long);
			yield return (CommonBag.FinalDestinationCodeFindBox, ControlWidthClass.Long);
			yield return (CommonBag.ManifestQtyCalcDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.GrossWeightCalcDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.VolumeCalcDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.ShipmentTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.CargoTypeDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.SpecialCargoCodesDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.MessageStatusTextBox, ControlWidthClass.Long);
			yield return (CommonBag.BillStatusDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.SenderReferenceTextBox, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CommonBag.ShipperAddressControl, ControlWidthClass.Long);
			yield return (CommonBag.ConsigneeAddressControl, ControlWidthClass.Long);
			yield return (CommonBag.NotifyPartyAddressControl, ControlWidthClass.Long);
			yield return (CommonBag.DeliveryAgentAddressControl, ControlWidthClass.Long);
			yield return (CommonBag.AgentAddressControl, ControlWidthClass.Long);
			yield return (CommonBag.ForwarderAddressControl, ControlWidthClass.Long);
			yield return (AEBag.ForwarderMPCITextBox, ControlWidthClass.Medium);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
	{
		get
		{
			yield return (CommonBag.PrepaidCollectDropEdit, ControlWidthClass.Long);
			yield return (CommonBag.FreightValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			yield return (CommonBag.TransportValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			yield return (CommonBag.InsuranceValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			yield return (CommonBag.DiscountValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			yield return (CommonBag.OtherChargesValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
			yield return (CommonBag.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.Long);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new BillLayoutBuilder<AsycudaBill>();

	public PanelLayout Layout => layout ??= ((IPanelLayoutProvider)new BillDetailsLayout()).Layout;
	PanelLayout layout;

	CommonBillControlBag CommonBag => CommonBillControlBag.Instance;

	BillDetailsControlBag AEBag => BillDetailsControlBag.Instance;
}

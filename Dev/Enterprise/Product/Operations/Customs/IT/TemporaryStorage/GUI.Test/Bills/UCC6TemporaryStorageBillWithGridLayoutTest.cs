using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(UCC6TemporaryStorageBillWithGridLayout))]
sealed class UCC6TemporaryStorageBillWithGridLayoutTest : LayoutsAbstractTest
{
	public void TestDefaultVisibilities_Bill()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
		var layout = ((IPanelLayoutProvider)new UCC6TemporaryStorageBillWithGridLayout()).Layout;

		CombineAssertions(() =>
		{
			AssertEquals("UCRNumberTextEdit", true, layout.ShouldInclude(EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillDetailControlBag.Instance.UCRNumberTextEdit));
			AssertEquals("ConsignorAddressControl", true, layout.ShouldInclude(EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillDetailControlBag.Instance.ConsignorAddressControl));
			AssertEquals("ConsigneeAddressControl", true, layout.ShouldInclude(EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillDetailControlBag.Instance.ConsigneeAddressControl));
			AssertEquals("NotifyPartyAddressControl", true, layout.ShouldInclude(EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillDetailControlBag.Instance.NotifyPartyAddressControl));
			AssertEquals("BillNumberTextEdit", true, layout.ShouldInclude(EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillDetailControlBag.Instance.BillNumberTextEdit));
			AssertEquals("GoodsDescTextBox", true, layout.ShouldInclude(UCC6TemporaryStorageBillDetailControlBag.Instance.GoodsDescTextBox));
			AssertEquals("LrnTextBox", true, layout.ShouldInclude(UCC6TemporaryStorageBillDetailControlBag.Instance.LrnTextBox));
			AssertEquals("TypeDropEdit", false, layout.ShouldInclude(EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillDetailControlBag.Instance.TypeDropEdit));
			AssertEquals("GrossWeightWithUnitUserControl", false, layout.ShouldInclude(EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillDetailControlBag.Instance.GrossWeightWithUnitUserControl));
			AssertEquals("MrnTextBox", true, layout.ShouldInclude(UCC6TemporaryStorageBillDetailControlBag.Instance.MrnTextBox));
			AssertEquals("GrossWeightCalcEdit", true, layout.ShouldInclude(UCC6TemporaryStorageBillDetailControlBag.Instance.GrossWeightCalcEdit));
			AssertEquals("NetWeightCalcEdit", true, layout.ShouldInclude(UCC6TemporaryStorageBillDetailControlBag.Instance.NetWeightCalcEdit));
			AssertEquals("SuppQtyCalcEdit", true, layout.ShouldInclude(UCC6TemporaryStorageBillDetailControlBag.Instance.SuppQtyCalcEdit));
		});
	}

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumn;
			yield return SecondColumn;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumn
	{
		get
		{
			yield return (UCC6TemporaryStorageBillDetailControlBag.Instance.GoodsDescTextBox, ControlWidthClass.Long);
			yield return (UCC6TemporaryStorageBillDetailControlBag.Instance.LrnTextBox, ControlWidthClass.Long);
			yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillDetailControlBag.Instance.BillNumberTextEdit, ControlWidthClass.Long);
			yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillDetailControlBag.Instance.UCRNumberTextEdit, ControlWidthClass.Long);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumn
	{
		get
		{
			yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillDetailControlBag.Instance.ConsignorAddressControl, ControlWidthClass.Long);
			yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillDetailControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
			yield return (EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillDetailControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
			yield return (UCC6TemporaryStorageBillDetailControlBag.Instance.MrnTextBox, ControlWidthClass.Long);
			yield return (UCC6TemporaryStorageBillDetailControlBag.Instance.GrossWeightCalcEdit, ControlWidthClass.Long);
			yield return (UCC6TemporaryStorageBillDetailControlBag.Instance.NetWeightCalcEdit, ControlWidthClass.Long);
			yield return (UCC6TemporaryStorageBillDetailControlBag.Instance.SuppQtyCalcEdit, ControlWidthClass.Long);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new UCC6TemporaryStorageBillDetailBuilder();

	protected override Type ExpectedGridUserControlType => typeof(EU.TemporaryStorage.GUI.UCC6TemporaryStorageBillGridControl);
}

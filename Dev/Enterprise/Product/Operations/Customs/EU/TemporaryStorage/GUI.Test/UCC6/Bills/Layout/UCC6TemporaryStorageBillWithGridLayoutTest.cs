using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageBillWithGridLayout))]
	sealed class UCC6TemporaryStorageBillWithGridLayoutTest : LayoutsAbstractTest
	{
		public void TestDefaultVisibilities_Transfer()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
			var layout = ((IPanelLayoutProvider)new UCC6TemporaryStorageBillWithGridLayout()).Layout;

			CombineAssertions(() =>
			{
				AssertEquals("UCRNumberTextEdit", false, layout.IsVisible(UCC6TemporaryStorageBillDetailControlBag.Instance.UCRNumberTextEdit, header));
				AssertEquals("ConsignorAddressControl", false, layout.IsVisible(UCC6TemporaryStorageBillDetailControlBag.Instance.ConsignorAddressControl, header));
				AssertEquals("ConsigneeAddressControl", false, layout.IsVisible(UCC6TemporaryStorageBillDetailControlBag.Instance.ConsigneeAddressControl, header));
				AssertEquals("NotifyPartyAddressControl", false, layout.IsVisible(UCC6TemporaryStorageBillDetailControlBag.Instance.NotifyPartyAddressControl, header));
				AssertEquals("TypeDropEdit", true, layout.IsVisible(UCC6TemporaryStorageBillDetailControlBag.Instance.TypeDropEdit, header));
				AssertEquals("BillNumberTextEdit", true, layout.IsVisible(UCC6TemporaryStorageBillDetailControlBag.Instance.BillNumberTextEdit, header));
				AssertEquals("GrossWeightWithUnitUserControl", true, layout.IsVisible(UCC6TemporaryStorageBillDetailControlBag.Instance.GrossWeightWithUnitUserControl, header));
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
				yield return (UCC6TemporaryStorageBillDetailControlBag.Instance.TypeDropEdit, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillDetailControlBag.Instance.BillNumberTextEdit, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillDetailControlBag.Instance.UCRNumberTextEdit, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillDetailControlBag.Instance.GrossWeightWithUnitUserControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumn
		{
			get
			{
				yield return (UCC6TemporaryStorageBillDetailControlBag.Instance.ConsignorAddressControl, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillDetailControlBag.Instance.ConsigneeAddressControl, ControlWidthClass.Long);
				yield return (UCC6TemporaryStorageBillDetailControlBag.Instance.NotifyPartyAddressControl, ControlWidthClass.Long);
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TemporaryStorageLayoutBuilder<TemporaryStorageHeader>();

		protected override Type ExpectedGridUserControlType => typeof(UCC6TemporaryStorageBillGridControl);
	}
}

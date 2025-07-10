using System.Collections.Generic;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(WarehouseAdjustmentInvoiceLineDetailsLayout))]
	sealed class WarehouseAdjustmentInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			var invoiceLine = declaration.InvoiceLines.AddNew();

			Layout.TryGetCaption(CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit, invoiceLine, out var resourceStringData);
			AssertEquals("InwardMovementQuantityCalcDropEdit", "Outward Qty.", resourceStringData.Caption);
		}

		protected override int ControlBagCount => 3;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.InvoiceNumberDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.OutwardMRNTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.OutwardDecisiveDateEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PartNoCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Auto);
			}
		}

		PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new WarehouseAdjustmentInvoiceLineDetailsLayout()).Layout);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();

		PanelLayout layout;
	}
}

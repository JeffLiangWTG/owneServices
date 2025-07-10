using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ImportValuationSendingDetailsLayout))]
	sealed class ImportValuationSendingDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ImportValuationSendingDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(1, columns.Count);

			AssertEquals(4, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];

			AssertEquals(4, row1.Parts.Count);
			AssertEquals(nameof(DetailsAndProvisionalPriceControlBag.InstanceForMessageSendingObject.InvoiceNoTextBox), row1.Parts[1].Name);
			AssertEquals(nameof(DetailsAndProvisionalPriceControlBag.InstanceForMessageSendingObject.InvoiceDateEdit), row1.Parts[3].Name);

			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(DetailsAndProvisionalPriceControlBag.InstanceForMessageSendingObject.PurchaseOrderNoTextBox), row2.Parts[1].Name);
			AssertEquals(nameof(DetailsAndProvisionalPriceControlBag.InstanceForMessageSendingObject.PurchaseOrderDateEdit), row2.Parts[3].Name);

			AssertEquals(4, row3.Parts.Count);
			AssertEquals(nameof(DetailsAndProvisionalPriceControlBag.InstanceForMessageSendingObject.ContractNoTextBox), row3.Parts[1].Name);
			AssertEquals(nameof(DetailsAndProvisionalPriceControlBag.InstanceForMessageSendingObject.ContractDateEdit), row3.Parts[3].Name);

			AssertEquals(2, row4.Parts.Count);
			AssertEquals(nameof(DetailsAndProvisionalPriceControlBag.InstanceForMessageSendingObject.TotalCustomsValueCalcEdit), row4.Parts[1].Name);
		}
	}
}

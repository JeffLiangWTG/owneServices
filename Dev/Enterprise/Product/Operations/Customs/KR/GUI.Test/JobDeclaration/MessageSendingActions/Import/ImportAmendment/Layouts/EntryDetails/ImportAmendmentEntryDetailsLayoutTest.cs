using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ImportAmendmentEntryDetailsLayout))]
	sealed class ImportAmendmentEntryDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new ImportAmendmentEntryDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(2, columns.Count);

			AssertEquals(5, columns[0].Rows.Count);
			var row0_1 = columns[0].Rows[0];
			var row0_2 = columns[0].Rows[1];
			var row0_3 = columns[0].Rows[2];
			var row0_4 = columns[0].Rows[3];
			var row0_5 = columns[0].Rows[4];

			AssertEquals(5, row0_1.Parts.Count);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.VersionNoCalcEdit), row0_1.Parts[1].Name);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.AmendmentTypeTextBox), row0_1.Parts[3].Name);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.AmendmentTypeDescriptionTextBox), row0_1.Parts[4].Name);

			AssertEquals(2, row0_2.Parts.Count);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.ReasonCodeDropEdit), row0_2.Parts[1].Name);

			AssertEquals(2, row0_3.Parts.Count);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.AmendmentReasonTextBox), row0_3.Parts[1].Name);

			AssertEquals(2, row0_4.Parts.Count);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.FaultPartyDropEdit), row0_4.Parts[1].Name);

			AssertEquals(2, row0_5.Parts.Count);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.FaultReasonTextBox), row0_5.Parts[1].Name);

			AssertEquals(6, columns[1].Rows.Count);
			var row1_1 = columns[1].Rows[0];
			var row1_2 = columns[1].Rows[1];
			var row1_3 = columns[1].Rows[2];
			var row1_4 = columns[1].Rows[3];
			var row1_5 = columns[1].Rows[4];
			var row1_6 = columns[1].Rows[5];

			AssertEquals(2, row1_1.Parts.Count);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.PenaltyPaymentReasonCodeFindBox), row1_1.Parts[1].Name);

			AssertEquals(4, row1_2.Parts.Count);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.TotalAmendedCountItemCalcEdit), row1_2.Parts[1].Name);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.TotalAmendedCountDutyTaxCalcEdit), row1_2.Parts[3].Name);

			AssertEquals(4, row1_3.Parts.Count);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.BeforeTotalDutyTaxCalcEdit), row1_3.Parts[1].Name);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.AfterTotalDutyTaxCalcEdit), row1_3.Parts[3].Name);

			AssertEquals(2, row1_4.Parts.Count);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.DutyTaxDifferenceCalcEdit), row1_4.Parts[1].Name);

			AssertEquals(4, row1_5.Parts.Count);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.BeforeCustomsValueCalcEdit), row1_5.Parts[1].Name);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.AfterCustomsValueCalcEdit), row1_5.Parts[3].Name);

			AssertEquals(2, row1_6.Parts.Count);
			AssertEquals(nameof(ImportAmendmentEntryDetailsLayoutItemsControlBag.Instance.CustomsValueDifferenceCalcEdit), row1_6.Parts[1].Name);
		}
	}
}

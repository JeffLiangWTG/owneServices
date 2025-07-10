using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DisbursementBillsLinesLayout))]
	sealed class DisbursementBillsLinesLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new DisbursementBillsLinesLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(3, columns.Count);

			AssertEquals(3, columns[0].Rows.Count);
			var row0_1 = columns[0].Rows[0];
			var row0_2 = columns[0].Rows[1];
			var row0_3 = columns[0].Rows[2];

			AssertEquals(2, row0_1.Parts.Count);
			AssertEquals(nameof(StatementLineControlBag.Instance.DutyAmountCalcEdit), row0_1.Parts[1].Name);

			AssertEquals(2, row0_2.Parts.Count);
			AssertEquals(nameof(StatementLineControlBag.Instance.LiquorTaxCalcEdit), row0_2.Parts[1].Name);

			AssertEquals(2, row0_3.Parts.Count);
			AssertEquals(nameof(StatementLineControlBag.Instance.AgricultureTaxCalcEdit), row0_3.Parts[1].Name);

			AssertEquals(3, columns[1].Rows.Count);
			var row1_1 = columns[1].Rows[0];
			var row1_2 = columns[1].Rows[1];
			var row1_3 = columns[1].Rows[2];

			AssertEquals(2, row1_1.Parts.Count);
			AssertEquals(nameof(StatementLineControlBag.Instance.TransportationTaxCalcEdit), row1_1.Parts[1].Name);

			AssertEquals(2, row1_2.Parts.Count);
			AssertEquals(nameof(StatementLineControlBag.Instance.EducationTaxCalcEdit), row1_2.Parts[1].Name);

			AssertEquals(2, row1_3.Parts.Count);
			AssertEquals(nameof(StatementLineControlBag.Instance.InterestCalcEdit), row1_3.Parts[1].Name);

			AssertEquals(3, columns[2].Rows.Count);
			var row2_1 = columns[2].Rows[0];
			var row2_2 = columns[2].Rows[1];
			var row2_3 = columns[2].Rows[2];

			AssertEquals(2, row2_1.Parts.Count);
			AssertEquals(nameof(StatementLineControlBag.Instance.SpecialConsumptionTaxCalcEdit), row2_1.Parts[1].Name);

			AssertEquals(2, row2_2.Parts.Count);
			AssertEquals(nameof(StatementLineControlBag.Instance.VATCalcEdit), row2_2.Parts[1].Name);

			AssertEquals(2, row2_3.Parts.Count);
			AssertEquals(nameof(StatementLineControlBag.Instance.DeclarationPenaltyCalcEdit), row2_3.Parts[1].Name);
		}
	}
}

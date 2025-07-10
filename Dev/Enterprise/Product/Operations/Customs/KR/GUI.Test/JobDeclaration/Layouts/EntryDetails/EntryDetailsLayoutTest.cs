using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(EntryDetailsLayout))]
	sealed class EntryDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new EntryDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(2, columns.Count);

			AssertEquals(13, columns[0].Rows.Count);
			AssertFirstColumn(columns[0].Rows[0], nameof(EntryDetailsControlBag.Instance.EntryNumberTextBox));
			AssertFirstColumn(columns[0].Rows[1], nameof(EntryDetailsControlBag.Instance.MessageTypeDropEdit));
			AssertFirstColumn(columns[0].Rows[2], nameof(EntryDetailsControlBag.Instance.MessageStatusDropEdit));
			AssertFirstColumn(columns[0].Rows[3], nameof(EntryDetailsControlBag.Instance.EntryStatusDropEdit));
			AssertFirstColumn(columns[0].Rows[4], nameof(EntryDetailsControlBag.Instance.EntrySubmittedDateEdit));
			AssertFirstColumn(columns[0].Rows[5], nameof(EntryDetailsControlBag.Instance.AcceptedDateEdit));
			AssertFirstColumn(columns[0].Rows[6], nameof(EntryDetailsControlBag.Instance.ClearedDateEdit));
			AssertFirstColumn(columns[0].Rows[7], nameof(EntryDetailsControlBag.Instance.IncotermTextBox));
			AssertFirstColumn(columns[0].Rows[8], nameof(EntryDetailsControlBag.Instance.TotalInvoiceAmountUserControl));
			AssertFirstColumn(columns[0].Rows[9], nameof(EntryDetailsControlBag.Instance.TotalCustomsValueKRWCalcEdit));
			AssertFirstColumn(columns[0].Rows[10], nameof(EntryDetailsControlBag.Instance.TotalCustomsValueUSDCalcEdit));
			AssertFirstColumn(columns[0].Rows[11], nameof(EntryDetailsControlBag.Instance.FreightCalcEdit));
			AssertFirstColumn(columns[0].Rows[12], nameof(EntryDetailsControlBag.Instance.InsuranceCalcEdit));

			AssertEquals(10, columns[1].Rows.Count);
			AssertSecondColumn(columns[1].Rows[0], nameof(EntryDetailsControlBag.Instance.AdditionalAmountCalcEdit), nameof(EntryDetailsControlBag.Instance.TotalAgricultureTaxCalcEdit));
			AssertSecondColumn(columns[1].Rows[1], nameof(EntryDetailsControlBag.Instance.DeductedAmountCalcEdit), nameof(EntryDetailsControlBag.Instance.TotalVATCalcEdit));
			AssertSecondColumn(columns[1].Rows[2], nameof(EntryDetailsControlBag.Instance.TotalValueForVATCalcEdit), nameof(EntryDetailsControlBag.Instance.TotalPayableAmountCalcEdit));
			AssertSecondColumn(columns[1].Rows[3], nameof(EntryDetailsControlBag.Instance.TotalVATExemptionValueCalcEdit), nameof(EntryDetailsControlBag.Instance.PenaltyForLateDeclarationCalcEdit));
			AssertSecondColumn(columns[1].Rows[4], nameof(EntryDetailsControlBag.Instance.TotalDutyAmountCalcEdit), nameof(EntryDetailsControlBag.Instance.PenaltyForMissedDeclarationCalcEdit));
			AssertSecondColumn(columns[1].Rows[5], nameof(EntryDetailsControlBag.Instance.TotalSpecialConsumptionTaxCalcEdit), nameof(EntryDetailsControlBag.Instance.TotalGrossWeightInKGCalcEdit));
			AssertSecondColumn(columns[1].Rows[6], nameof(EntryDetailsControlBag.Instance.TotalTransportationTaxCalcEdit), nameof(EntryDetailsControlBag.Instance.TotalPackagesDropEdit));
			AssertSecondColumn(columns[1].Rows[7], nameof(EntryDetailsControlBag.Instance.TotalLiquorTaxCalcEdit), nameof(EntryDetailsControlBag.Instance.CustomerOfficerTextBox));
			AssertSecondColumn(columns[1].Rows[8], nameof(EntryDetailsControlBag.Instance.TotalEducationTaxCalcEdit), nameof(EntryDetailsControlBag.Instance.CustomsRemarkLongTextBox));
			AssertEquals(2, columns[1].Rows[9].Parts.Count);
			AssertEquals(nameof(EntryDetailsControlBag.Instance.CustomsDisbursementBillGroupBox), columns[1].Rows[9].Parts[0].Name);
		}

		void AssertFirstColumn(PanelLayoutRow row, ZString control)
		{
			AssertEquals(3, row.Parts.Count);
			AssertEquals(control, row.Parts[1].Name);
		}

		void AssertSecondColumn(PanelLayoutRow row, ZString control1, ZString control2)
		{
			AssertEquals(6, row.Parts.Count);
			AssertEquals(control1, row.Parts[1].Name);
			AssertEquals(control2, row.Parts[4].Name);
		}
	}
}

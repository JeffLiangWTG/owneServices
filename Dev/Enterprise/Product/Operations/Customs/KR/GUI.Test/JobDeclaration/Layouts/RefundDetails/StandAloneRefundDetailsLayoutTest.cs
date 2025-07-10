using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(StandAloneRefundDetailsLayout))]
	sealed class StandAloneRefundDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new StandAloneRefundDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(5, columns.Count);

			var column = columns[0];
			AssertEquals(11, column.Rows.Count);
			AssertForPartsCountTwo(column.Rows[0], nameof(RefundDetailsControlBag.EmptyLabel));
			AssertForPartsCountTwo(column.Rows[1], nameof(RefundDetailsControlBag.TotalTaxLabel));
			AssertForPartsCountTwo(column.Rows[2], nameof(RefundDetailsControlBag.DutyAmountLabel));
			AssertForPartsCountTwo(column.Rows[3], nameof(RefundDetailsControlBag.LiquorTaxLabel));
			AssertForPartsCountTwo(column.Rows[4], nameof(RefundDetailsControlBag.SpecialConsumptionTaxLabel));
			AssertForPartsCountTwo(column.Rows[5], nameof(RefundDetailsControlBag.TransportTaxLabel));
			AssertForPartsCountTwo(column.Rows[6], nameof(RefundDetailsControlBag.EducationTaxLabel));
			AssertForPartsCountTwo(column.Rows[7], nameof(RefundDetailsControlBag.AgricultureTaxLabel));
			AssertForPartsCountTwo(column.Rows[8], nameof(RefundDetailsControlBag.VATLabel));
			AssertForPartsCountTwo(column.Rows[9], nameof(RefundDetailsControlBag.TotalPenaltyLabel));
			AssertForPartsCountTwo(column.Rows[10], nameof(RefundDetailsControlBag.LatePaymentPenaltyLabel));

			column = columns[1];
			AssertEquals(11, column.Rows.Count);
			AssertForPartsCountTwo(column.Rows[0], nameof(RefundDetailsControlBag.PaidLabel));
			AssertForPartsCountTwo(column.Rows[1], nameof(RefundDetailsControlBag.TotalPaidAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[2], nameof(RefundDetailsControlBag.PaidDutyAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[3], nameof(RefundDetailsControlBag.PaidLiquorTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[4], nameof(RefundDetailsControlBag.PaidSpecialConsumptionTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[5], nameof(RefundDetailsControlBag.PaidTransportTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[6], nameof(RefundDetailsControlBag.PaidEducationTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[7], nameof(RefundDetailsControlBag.PaidAgricultureTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[8], nameof(RefundDetailsControlBag.PaidVATAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[9], nameof(RefundDetailsControlBag.PaidTotalPenaltyCalcEdit));
			AssertForPartsCountTwo(column.Rows[10], nameof(RefundDetailsControlBag.PaidLatePaymentPenaltyCalcEdit));
			
			column = columns[2];
			AssertEquals(11, column.Rows.Count);
			AssertForPartsCountTwo(column.Rows[0], nameof(RefundDetailsControlBag.RefundLabel));
			AssertForPartsCountTwo(column.Rows[1], nameof(RefundDetailsControlBag.TotalRefundAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[2], nameof(RefundDetailsControlBag.RefundDutyAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[3], nameof(RefundDetailsControlBag.RefundLiquorTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[4], nameof(RefundDetailsControlBag.RefundSpecialConsumptionTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[5], nameof(RefundDetailsControlBag.RefundTransportTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[6], nameof(RefundDetailsControlBag.RefundEducationTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[7], nameof(RefundDetailsControlBag.RefundAgricultureTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[8], nameof(RefundDetailsControlBag.RefundVATAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[9], nameof(RefundDetailsControlBag.Empty2Label));
			AssertForPartsCountTwo(column.Rows[10], nameof(RefundDetailsControlBag.TotalLateRefundAmountCalcEdit));
			
			column = columns[3];
			AssertEquals(10, column.Rows.Count);
			AssertForPartsCountTwo(column.Rows[0], nameof(RefundDetailsControlBag.PenaltyLabel));
			AssertForPartsCountTwo(column.Rows[1], nameof(RefundDetailsControlBag.Empty3Label));
			AssertForPartsCountTwo(column.Rows[2], nameof(RefundDetailsControlBag.PenaltyToRefundDutyAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[3], nameof(RefundDetailsControlBag.PenaltyToRefundLiquorTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[4], nameof(RefundDetailsControlBag.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[5], nameof(RefundDetailsControlBag.PenaltyToRefundTransportTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[6], nameof(RefundDetailsControlBag.PenaltyToRefundEducationTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[7], nameof(RefundDetailsControlBag.PenaltyToRefundAgricultureTaxAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[8], nameof(RefundDetailsControlBag.PenaltyToRefundVATAmountCalcEdit));
			AssertForPartsCountTwo(column.Rows[9], nameof(RefundDetailsControlBag.TotalPenaltyToRefundCalcEdit));

			column = columns[4];
			AssertEquals(7, column.Rows.Count);
			AssertEquals(2, column.Rows[0].Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.Refund2Label), column.Rows[0].Parts[1].Name);
			AssertForPartsCountFour(column.Rows[1], nameof(RefundDetailsControlBag.ValueForVATLabel), nameof(RefundDetailsControlBag.ValueForVATCalcEdit));
			AssertForPartsCountFour(column.Rows[2], nameof(RefundDetailsControlBag.VATExemptionLabel), nameof(RefundDetailsControlBag.VATExemptionCalcEdit));
			AssertForPartsCountFour(column.Rows[3], nameof(RefundDetailsControlBag.PenaltyForLateDeclarationLabel), nameof(RefundDetailsControlBag.PenaltyForLateDeclarationCalcEdit));
			AssertForPartsCountFour(column.Rows[4], nameof(RefundDetailsControlBag.PenaltyForMissedDeclarationLabel), nameof(RefundDetailsControlBag.PenaltyForMissedDeclarationCalcEdit));
			AssertForPartsCountFour(column.Rows[5], nameof(RefundDetailsControlBag.PenaltyForLatePaymentLabel), nameof(RefundDetailsControlBag.PenaltyForLatePaymentCalcEdit));
			AssertForPartsCountFour(column.Rows[6], nameof(RefundDetailsControlBag.NonDutyTaxRevenueLabel), nameof(RefundDetailsControlBag.NonDutyTaxRevenueCalcEdit));

			void AssertForPartsCountTwo(PanelLayoutRow row, string partName)
			{
				AssertEquals(2, row.Parts.Count);
				AssertEquals(partName, row.Parts[1].Name);
			}
			void AssertForPartsCountFour(PanelLayoutRow row, string firstPartName, string secondPartName)
			{
				AssertEquals(4, row.Parts.Count);
				AssertEquals(firstPartName, row.Parts[1].Name);
				AssertEquals(secondPartName, row.Parts[3].Name);
			}
		}
	}
}

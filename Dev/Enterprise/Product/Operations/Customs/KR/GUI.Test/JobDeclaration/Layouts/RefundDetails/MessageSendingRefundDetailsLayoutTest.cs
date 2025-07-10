using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MessageSendingRefundDetailsLayout))]
	sealed class MessageSendingRefundDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => null;
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn => null;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		public new void TestIncludedControls()
		{
			IPanelLayoutProvider layout = new MessageSendingRefundDetailsLayout();
			var columns = layout.Layout.Columns;
			AssertEquals(2, columns.Count);

			AssertEquals(11, columns[0].Rows.Count);
			var row1 = columns[0].Rows[0];
			var row2 = columns[0].Rows[1];
			var row3 = columns[0].Rows[2];
			var row4 = columns[0].Rows[3];
			var row5 = columns[0].Rows[4];
			var row6 = columns[0].Rows[5];
			var row7 = columns[0].Rows[6];
			var row8 = columns[0].Rows[7];
			var row9 = columns[0].Rows[8];
			var row10 = columns[0].Rows[9];
			var row11 = columns[0].Rows[10];

			AssertEquals(4, row1.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.PaidLabel), row1.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.RefundLabel), row1.Parts[3].Name);

			AssertEquals(6, row2.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.TotalTaxLabel), row2.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.TotalPaidAmountCalcEdit), row2.Parts[3].Name);
			AssertEquals(nameof(RefundDetailsControlBag.TotalRefundAmountCalcEdit), row2.Parts[5].Name);

			AssertEquals(6, row3.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.DutyAmountLabel), row3.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.PaidDutyAmountCalcEdit), row3.Parts[3].Name);
			AssertEquals(nameof(RefundDetailsControlBag.RefundDutyAmountCalcEdit), row3.Parts[5].Name);

			AssertEquals(6, row4.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.LiquorTaxLabel), row4.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.PaidLiquorTaxAmountCalcEdit), row4.Parts[3].Name);
			AssertEquals(nameof(RefundDetailsControlBag.RefundLiquorTaxAmountCalcEdit), row4.Parts[5].Name);

			AssertEquals(6, row5.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.SpecialConsumptionTaxLabel), row5.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.PaidSpecialConsumptionTaxAmountCalcEdit), row5.Parts[3].Name);
			AssertEquals(nameof(RefundDetailsControlBag.RefundSpecialConsumptionTaxAmountCalcEdit), row5.Parts[5].Name);

			AssertEquals(6, row6.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.TransportTaxLabel), row6.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.PaidTransportTaxAmountCalcEdit), row6.Parts[3].Name);
			AssertEquals(nameof(RefundDetailsControlBag.RefundTransportTaxAmountCalcEdit), row6.Parts[5].Name);

			AssertEquals(6, row7.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.EducationTaxLabel), row7.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.PaidEducationTaxAmountCalcEdit), row7.Parts[3].Name);
			AssertEquals(nameof(RefundDetailsControlBag.RefundEducationTaxAmountCalcEdit), row7.Parts[5].Name);

			AssertEquals(6, row8.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.AgricultureTaxLabel), row8.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.PaidAgricultureTaxAmountCalcEdit), row8.Parts[3].Name);
			AssertEquals(nameof(RefundDetailsControlBag.RefundAgricultureTaxAmountCalcEdit), row8.Parts[5].Name);

			AssertEquals(6, row9.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.VATLabel), row9.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.PaidVATAmountCalcEdit), row9.Parts[3].Name);
			AssertEquals(nameof(RefundDetailsControlBag.RefundVATAmountCalcEdit), row9.Parts[5].Name);

			AssertEquals(4, row10.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.TotalPenaltyLabel), row10.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.PaidTotalPenaltyCalcEdit), row10.Parts[3].Name);

			AssertEquals(6, row11.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.LatePaymentPenaltyLabel), row11.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.PaidLatePaymentPenaltyCalcEdit), row11.Parts[3].Name);
			AssertEquals(nameof(RefundDetailsControlBag.TotalLateRefundAmountCalcEdit), row11.Parts[5].Name);

			row1 = columns[1].Rows[0];
			row2 = columns[1].Rows[1];
			row3 = columns[1].Rows[2];
			row4 = columns[1].Rows[3];
			row5 = columns[1].Rows[4];
			AssertEquals(2, row1.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.Refund2Label), row1.Parts[1].Name);
			
			AssertEquals(4, row2.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.PenaltyForLateDeclarationLabel), row2.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.PenaltyForLateDeclarationCalcEdit), row2.Parts[3].Name);

			AssertEquals(4, row3.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.PenaltyForMissedDeclarationLabel), row3.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.PenaltyForMissedDeclarationCalcEdit), row3.Parts[3].Name);

			AssertEquals(4, row4.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.PenaltyForLatePaymentLabel), row4.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.PenaltyForLatePaymentCalcEdit), row4.Parts[3].Name);

			AssertEquals(4, row5.Parts.Count);
			AssertEquals(nameof(RefundDetailsControlBag.NonDutyTaxRevenueLabel), row5.Parts[1].Name);
			AssertEquals(nameof(RefundDetailsControlBag.NonDutyTaxRevenueCalcEdit), row5.Parts[3].Name);
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class Import5FEMessageRefundRequestUserControlTest : TestCaseWithFactory
	{
		public void TestImport5FEMessageRefundRequestUserControl()
		{
			using (var control = new Import5FEMessageRefundRequestUserControl())
			{
				var declarationGroupBox = control.FindSingle<ZGroupBox>("DeclarationGroupBox");
				AssertNotNull(declarationGroupBox);
				AssertNotNull(declarationGroupBox.FindSingle<DynamicLayoutPanel>("DeclarationPanel"));

				var refundHeaderGroupBox = control.FindSingle<ZGroupBox>("RefundHeaderGroupBox");
				AssertNotNull(refundHeaderGroupBox);
				AssertNotNull(refundHeaderGroupBox.FindSingle<DynamicLayoutPanel>("RefundHeaderPanel"));

				var paidAndRefundOfTaxAndPenaltyGroupBox = control.FindSingle<ZGroupBox>("PaidAndRefundOfTaxAndPenaltyGroupBox");
				AssertNotNull(paidAndRefundOfTaxAndPenaltyGroupBox);
				AssertNotNull(paidAndRefundOfTaxAndPenaltyGroupBox.FindSingle<DynamicLayoutPanel>("PaidAndRefundOfTaxAndPenaltyPanel"));
			}
		}
	}
}

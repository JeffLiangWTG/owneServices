using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class MiscOptionsGroupBoxUserControlTest : TestCaseWithFactory
	{
		public void TestGroupBoxUserControl()
		{
			using (var control = new MiscOptionsGroupBoxUserControl())
			{
				var miscellaneousGroupBox = control.FindSingle<ZGroupBox>("MiscellaneousGroupBox");
				AssertEquals(true, miscellaneousGroupBox.Visible);
				AssertNotNull(miscellaneousGroupBox.FindSingle<DynamicLayoutPanel>("MiscellaneousDynamicLayoutPanel"));

				var returnGroupBox = control.FindSingle<ZGroupBox>("ReturnGroupBox");
				AssertEquals(true, returnGroupBox.Visible);
				AssertNotNull(returnGroupBox.FindSingle<DynamicLayoutPanel>("ReturnDynamicLayoutPanel"));

				var southNorthTradeGroupBox = control.FindSingle<ZGroupBox>("SouthNorthTradeGroupBox");
				AssertEquals(true, southNorthTradeGroupBox.Visible);
				AssertNotNull(southNorthTradeGroupBox.FindSingle<DynamicLayoutPanel>("SouthNorthTradeDynamicLayoutPanel"));

				var additionalCargoGroupBox = control.FindSingle<ZGroupBox>("AdditionalCargoGroupBox");
				AssertEquals(true, additionalCargoGroupBox.Visible);
				AssertNotNull(additionalCargoGroupBox.FindSingle<DynamicLayoutPanel>("AdditionalCargoDynamicLayoutPanel"));

				var penaltyDeclarationGroupBox = control.FindSingle<ZGroupBox>("PenaltyDeclarationGroupBox");
				AssertEquals(true, penaltyDeclarationGroupBox.Visible);
				AssertNotNull(penaltyDeclarationGroupBox.FindSingle<DynamicLayoutPanel>("PenaltyDeclarationDynamicLayoutPanel"));

				var refundRequestGroupBox = control.FindSingle<ZGroupBox>("RefundRequestGroupBox");
				AssertEquals(true, refundRequestGroupBox.Visible);
				AssertNotNull(refundRequestGroupBox.FindSingle<DynamicLayoutPanel>("RefundRequestDynamicLayoutPanel"));
			}
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class MiscOptionsUserControlTest : TestCaseWithFactory
	{
		public void TestCheckUserControl()
		{
			using (var control = new MiscOptionsUserControl())
			{
				AssertNotNull(control.FindSingle<ZDropEdit>("ReturnReasonDropEdit"));
				AssertNotNull(control.FindSingle<ZDropEdit>("ReturnTypeDropEdit"));
				AssertNotNull(control.FindSingle<ZDropEdit>("SouthNorthTradeAreaDropEdit"));
				AssertNotNull(control.FindSingle<ZDropEdit>("SouthNorthTradeDropEdit"));
				AssertNotNull(control.FindSingle<ZTextBox>("UCRTextBox"));
				AssertNotNull(control.FindSingle<CalenderUserControl>("BondedTransportationPeriodUserControl"));
				AssertNotNull(control.FindSingle<ZDropEdit>("LateDecPenaltyDateCodeDropEdit"));
				AssertNotNull(control.FindSingle<ZCalcEdit>("MissedDecPenaltyRateCalcEdit"));
				AssertNotNull(control.FindSingle<ZLabel>("PercentageLabel"));
				AssertNotNull(control.FindSingle<ZCodeFindBox>("TaxOfficeCodeFindBox"));
			}
		}
	}
}

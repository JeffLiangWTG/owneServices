using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(CustomsSummaryFilterControl))]
sealed class CustomsSummaryFilterControlTest : TestCaseWithFactory
{
	public void TestGridColumns() => CombineAssertions(() =>
	{
		using var control = new CustomsSummaryFilterControl(new CustomsSummaryLineCollection(Factory), new CustomsSummaryFilterStripBusinessObject());
		var grid = control.Grid;
		AssertType<ZTextBoxColumnStyleInfo>("StatementNumber", grid.GetColumnStyle(nameof(CustomsSummaryLine.StatementNumber)));
		AssertType<ZDateEditColumnStyleInfo>("ProcessDate", grid.GetColumnStyle(nameof(CustomsSummaryLine.ProcessDate)));
		AssertType<ZTextBoxColumnStyleInfo>("AccountNo", grid.GetColumnStyle(nameof(CustomsSummaryLine.AccountNo)));
		AssertType<ZTextBoxColumnStyleInfo>("ChargeType", grid.GetColumnStyle(nameof(CustomsSummaryLine.ChargeType)));
		AssertType<ZTextBoxColumnStyleInfo>("ChargeTypeDescription", grid.GetColumnStyle(nameof(CustomsSummaryLine.ChargeTypeDescription)));
		AssertType<ZTextBoxColumnStyleInfo>("B3_EntryNum", grid.GetColumnStyle(nameof(CustomsSummaryLine.B3_EntryNum)));
		AssertType<ZTextBoxColumnStyleInfo>("B3_BrokerReference", grid.GetColumnStyle(nameof(CustomsSummaryLine.B3_BrokerReference)));
		AssertType<ZCalcEditColumnStyleInfo>("ChargeAmount", grid.GetColumnStyle(nameof(CustomsSummaryLine.ChargeAmount)));
		AssertType<ZTextBoxColumnStyleInfo>("B3_Status", grid.GetColumnStyle(nameof(CustomsSummaryLine.B3_Status)));
		AssertType<ZTextBoxColumnStyleInfo>("StatusDescription", grid.GetColumnStyle(nameof(CustomsSummaryLine.StatusDescription)));
		AssertType<ZTextBoxColumnStyleInfo>("ReferenceNumber", grid.GetColumnStyle(nameof(CustomsSummaryLine.ReferenceNumber)));
	});
}

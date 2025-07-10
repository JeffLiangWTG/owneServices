using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Module.Testing;

[TestedType(typeof(NctsMovementFilterControl))]
sealed class NctsMovementFilterControlTest : TestCaseWithFactory
{
	public void TestColumns() => CombineAssertions("Assert columns", () =>
	{
		using (var userControl = new NctsMovementFilterControl(new EU.NCTS.Business.NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
		{
			var grid = userControl.Grid;
			AssertColumn("Activation Deadline", nameof(NctsHeader.MovementReferenceExpiryDate), grid);
			AssertColumn("Arrival Transport ID", $"{nameof(NctsHeader.ArrivalMovementHeader)}+{nameof(NctsArrivalMovementHeader.BM_TransportAtArrivalID)}", grid);
			AssertColumn("Arrival Transport Nationality", $"{nameof(NctsHeader.ArrivalMovementHeader)}+{nameof(NctsArrivalMovementHeader.BM_RN_NKTransportAtArrivalIDNationality)}", grid);
		}
	});

	void AssertColumn(string humanColumnName, string columnName, ZFilterGrid grid)
	{
		var columnStyle = grid.GetColumnStyle(columnName);

		AssertNotNull($"{humanColumnName} column", columnStyle);
		if (columnStyle != null)
		{
			AssertEquals($"{columnName}.IsVisible", true, columnStyle.IsVisible);
			AssertEquals($"{columnName}.Caption", humanColumnName, columnStyle.CaptionResourceString.Caption);
		}
	}
}

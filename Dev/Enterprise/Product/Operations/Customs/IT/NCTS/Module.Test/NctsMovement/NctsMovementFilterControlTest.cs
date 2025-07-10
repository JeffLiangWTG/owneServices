using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.Module.Testing;

sealed class NctsMovementFilterControlTest : TestCaseWithFactory
{
	public void TestColumns()
	{
		using (var userControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
		{
			var grid = userControl.Grid;
			CombineAssertions("Assert columns", () =>
			{
				AssertColumn("Representation Type", Business.NctsHeader.Schema.RepresentationType, grid);
				AssertColumn("Declarant", Business.NctsHeader.Schema.DeclarantCodeForBinding, grid);
				AssertColumn("Approval Defer No", Business.NctsHeader.Schema.ApprovalDeferNoForBinding, grid);
				AssertColumn("Registration Date", Business.NctsHeader.Schema.RegistrationDateForBinding, grid);
				AssertColumn("Release Date", Business.NctsHeader.Schema.ReleaseDateForBinding, grid);
				AssertColumn("Arrival Date", Business.NctsHeader.Schema.IrildesArrivalDateForBinding, grid);
				AssertColumn("Arrival Office Code", Business.NctsHeader.Schema.IrildesArrivalOfficeCodeForBinding, grid);
				AssertColumn("Arrival Office Description", Business.NctsHeader.Schema.IrildesArrivalOfficeDescriptionForBinding, grid);
				AssertColumn("Arrival Status", Business.NctsHeader.Schema.IrildesArrivalStatusForBinding, grid);
				AssertColumn("MRN Release Date", Business.NctsHeader.Schema.MovementReferenceIssueDate, grid);
			});
		}
	}

	void AssertColumn(string humanColumnName, string columnName, ZFilterGrid grid)
	{
		var columnStyle = grid.GetColumnStyle(columnName);

		AssertNotNull($"{humanColumnName} column", columnStyle);
		if (columnStyle != null)
		{
			AssertEquals($"{columnName} is visible", true, columnStyle.IsVisible);
		}
	}
}

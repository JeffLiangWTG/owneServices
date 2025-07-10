using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Module.Testing
{
	class LPCOFilterStripControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var decs = new CusLPCOHeaderCollection(Factory);
			var filterBO = new LPCOFilterStripBusinessObject();

			using (var filterControl = new LPCOFilterStripControl(decs, filterBO))
			{
				var grid = filterControl.FilteredGrid;

				CombineAssertions(() =>
				{
					AssertColumnStyle(grid, "PermitHolder+OH_Code", true, "LPCO Holder");
					AssertColumnStyle(grid, CusPermitHeaderSchema.Constants.CPH_Number, true, "LPCO Number");
					AssertColumnStyle(grid, CusPermitHeaderSchema.Constants.CPH_StartDate, true, "Start Date");
					AssertColumnStyle(grid, CusPermitHeaderSchema.Constants.CPH_EndDate, true, "End Date");
					AssertColumnStyle(grid, CusPermitHeaderSchema.Constants.CPH_RetroactiveDate, true, "Reference Date");
					AssertColumnStyle(grid, CusPermitHeaderSchema.Constants.CPH_MessageStatus, true, "Message Status");
					AssertColumnStyle(grid, CusPermitHeaderSchema.Constants.CPH_CustomsStatus, true, "Permit Status");
				});
			}
		}

		void AssertColumnStyle(ZGrid grid, string columnName, bool isVisible, string caption)
		{
			var column = grid.GetColumnStyle(columnName);

			AssertNotNull($"Column {columnName} should be added", column);
			AssertEquals($"Visibility of Column {columnName}", isVisible, column.IsVisible);
			AssertEquals($"Caption of Column {columnName}", caption, column.CaptionResourceString.Caption);
		}
	}
}


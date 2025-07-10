using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.BR.Module.Testing
{
	class LicenseFilterStripControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var decs = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new LicenseFilterStripBusinessObject();

			using (var filterControl = new LicenseFilterStripControl(decs, filterBO))
			{
				var grid = filterControl.FilteredGrid;
				CombineAssertions(() =>
				{
					AssertColumnStyle(grid, JobDeclaration.Schema.JE_GB, true, "Declaration Branch");
					AssertColumnStyle(grid, JobDeclaration.Schema.JE_DeclarationReference, true, "Job Number");
					AssertColumnStyle(grid, JobDeclaration.Schema.JE_OH_Importer, true, "Importer");
					AssertColumnStyle(grid, JobDeclaration.Schema.JE_OH_Supplier, true, "Supplier");
					AssertColumnStyle(grid, JobDeclaration.Schema.JE_ScreeningStatus, true, "Screening");
					AssertColumnStyle(grid, JobDeclaration.Schema.EntryNumbersConcatenated, true, "License Number");
					AssertColumnStyle(grid, JobDeclaration.Schema.EntryStatusesConcatenated, true, "License Status");
					AssertColumnStyle(grid, JobDeclaration.Schema.EntryStatusDescriptionsConcatenated, true, "License Status Description");
					AssertColumnStyle(grid, JobDeclaration.Schema.JE_MessageStatus, true, "Message Status");
					AssertColumnStyle(grid, JobDeclaration.Schema.JE_MessageStatusDescription, true, "Message Status Description");
					AssertColumnStyle(grid, JobDeclaration.Schema.JE_EntryAuthorisationDate, false, "License Auth. Date");
					AssertColumnStyle(grid, JobDeclaration.Schema.JE_MessageSubType, false, "License Style");
					AssertColumnStyle(grid, JobDeclaration.Schema.EntrySubmitDateAsString, false, "License Submitted");
					AssertColumnStyle(grid, JobDeclaration.Schema.EntryIssueDateAsString, false, "Issue Date");
					AssertColumnStyle(grid, JobDeclaration.Schema.ImporterName, false, "Importer Name");
					AssertColumnStyle(grid, JobDeclaration.Schema.SupplierName, false, "Supplier Name");
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

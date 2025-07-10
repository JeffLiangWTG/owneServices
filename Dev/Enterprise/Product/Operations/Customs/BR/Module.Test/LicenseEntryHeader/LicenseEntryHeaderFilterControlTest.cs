using System.Collections.Generic;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.BR.Module.Testing
{
	class LicenseEntryHeaderFilterControlTest : Customs.Module.Testing.EntryHeaderFilterUserControlTest
	{
		public void TestColumns()
		{
			using (var filterControl = GetNewEntryHeaderFilterUserControl())
			{
				var grid = filterControl.FilteredGrid;
				CombineAssertions(() =>
				{
					AssertColumnStyle(grid, CusEntryHeader.Schema.EntryReferenceNumber, true, "Reference Number", System.Windows.Forms.CharacterCasing.Normal);
					AssertColumnStyle(grid, LicenseEntryHeaderFilterControl.LicenseSchema.Screening, true, "Screening", System.Windows.Forms.CharacterCasing.Normal);
					AssertColumnStyle(grid, CusEntryHeader.Schema.EntryNumber, true, "License Number", System.Windows.Forms.CharacterCasing.Normal);
					AssertColumnStyle(grid, CusEntryHeader.Schema.CH_EntryStatus, true, "License Status", System.Windows.Forms.CharacterCasing.Normal);
					AssertColumnStyle(grid, CusEntryHeader.Schema.EntryHeaderStatusDescription, true, "License Status Description", System.Windows.Forms.CharacterCasing.Upper);
					AssertColumnStyle(grid, LicenseEntryHeaderFilterControl.LicenseSchema.LicenseAuth, false, "License Auth. Date", System.Windows.Forms.CharacterCasing.Normal);
					AssertColumnStyle(grid, LicenseEntryHeaderFilterControl.LicenseSchema.LicenseStyle, false, "License Style", System.Windows.Forms.CharacterCasing.Normal);
					AssertColumnStyle(grid, LicenseEntryHeaderFilterControl.LicenseSchema.LicenseSubmitted, false, "License Submitted", System.Windows.Forms.CharacterCasing.Normal);
					AssertColumnStyle(grid, LicenseEntryHeaderFilterControl.LicenseSchema.IssueDate, false, "Issue Date", System.Windows.Forms.CharacterCasing.Normal);
				});
			}
		}

		void AssertColumnStyle(ZGrid grid, string columnName, bool isVisible, string caption, System.Windows.Forms.CharacterCasing characterCase)
		{
			var column = grid.GetColumnStyle(columnName);

			AssertNotNull($"Column {columnName} should be added", column);
			AssertEquals($"Visibility of Column {columnName}", isVisible, column.IsVisible);
			AssertEquals($"Caption of Column {columnName}", caption, column.CaptionResourceString.Caption);
			AssertEquals($"CharacterCasing of Column {columnName}", characterCase, column.CharacterCasing);
		}

		protected override string ExpectedEntryNumberCaption => "License Number";

		protected override List<string> FilteredGridColumns => new List<string>()
		{
			EntryHeaderFilterUserControl.Schema.BranchCode,
			EntryHeaderFilterUserControl.Schema.BranchName,
			CusEntryHeader.Schema.DeclarationReference,
			CusEntryHeader.Schema.EntryReferenceNumber,
			EntryHeaderFilterUserControl.Schema.ImporterCode,
			EntryHeaderFilterUserControl.Schema.ImporterName,
			EntryHeaderFilterUserControl.Schema.SupplierCode,
			EntryHeaderFilterUserControl.Schema.SupplierName,
			LicenseEntryHeaderFilterControl.LicenseSchema.Screening,
			CusEntryHeader.Schema.EntryNumber,
			CusEntryHeader.Schema.CH_EntryStatus,
			CusEntryHeader.Schema.EntryHeaderStatusDescription,
			CusEntryHeader.Schema.CH_MessageType,
			CusEntryHeader.Schema.CH_MessageTypeDescription,
			CusEntryHeader.Schema.CH_Status,
			CusEntryHeader.Schema.MessageStatusDescription,
			LicenseEntryHeaderFilterControl.LicenseSchema.IssueDate,
			LicenseEntryHeaderFilterControl.LicenseSchema.LicenseAuth,
			LicenseEntryHeaderFilterControl.LicenseSchema.LicenseStyle,
			LicenseEntryHeaderFilterControl.LicenseSchema.LicenseSubmitted
		};

		protected override List<string> InVisibleFilteredGridColumns => new List<string>()
		{
			EntryHeaderFilterUserControl.Schema.BranchName,
			EntryHeaderFilterUserControl.Schema.ImporterName,
			EntryHeaderFilterUserControl.Schema.SupplierName,
			CusEntryHeader.Schema.CH_MessageType,
			CusEntryHeader.Schema.CH_Status,
			LicenseEntryHeaderFilterControl.LicenseSchema.IssueDate,
			LicenseEntryHeaderFilterControl.LicenseSchema.LicenseAuth,
			LicenseEntryHeaderFilterControl.LicenseSchema.LicenseStyle,
			LicenseEntryHeaderFilterControl.LicenseSchema.LicenseSubmitted
		};

		protected override List<string> ColumnNamesInSortOrder => new List<string>()
		{
			EntryHeaderFilterUserControl.Schema.BranchCode,
			CusEntryHeader.Schema.DeclarationReference,
			CusEntryHeader.Schema.EntryReferenceNumber,
			EntryHeaderFilterUserControl.Schema.ImporterCode,
			EntryHeaderFilterUserControl.Schema.SupplierCode,
			LicenseEntryHeaderFilterControl.LicenseSchema.Screening,
			CusEntryHeader.Schema.EntryNumber,
			CusEntryHeader.Schema.CH_EntryStatus,
			CusEntryHeader.Schema.EntryHeaderStatusDescription,
			CusEntryHeader.Schema.CH_MessageTypeDescription,
			CusEntryHeader.Schema.MessageStatusDescription,
		};

		protected override EntryHeaderFilterUserControl GetNewEntryHeaderFilterUserControl()
		{
			var parentBO = base.Factory.New<JobDeclaration>();
			var gridCollection = new Customs.Business.CusEntryHeaderCollection<CusEntryHeader>(parentBO, base.Factory);
			var filterBusinessObject = new EntryHeaderFilterBusinessObject();
			return new LicenseEntryHeaderFilterControl(gridCollection, filterBusinessObject);
		}
	}
}

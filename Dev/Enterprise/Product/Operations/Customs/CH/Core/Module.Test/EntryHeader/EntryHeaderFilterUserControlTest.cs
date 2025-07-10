using System.Collections.Generic;
using Enterprise.Customs.CH.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(EntryHeaderFilterUserControl))]
sealed class EntryHeaderFilterUserControlTest : Customs.Module.Testing.EntryHeaderFilterUserControlTest
{
	protected override List<string> FilteredGridColumns => AddAdditionalColumns(base.FilteredGridColumns, AdditionalColumns);

	protected override List<string> ColumnNamesInSortOrder => AddAdditionalColumns(base.ColumnNamesInSortOrder, AdditionalInSortOrderColumns);

	protected override List<string> InVisibleFilteredGridColumns => AddAdditionalColumns(base.InVisibleFilteredGridColumns, AdditionalInvisibleColumns);

	List<string> AddAdditionalColumns(List<string> existingColumns, List<string> additionalColumnList)
	{
		existingColumns.AddRange(additionalColumnList);
		return existingColumns;
	}
	static List<string> AdditionalInSortOrderColumns => new List<string>()
	{
		EntryHeaderFilterUserControl.ColumnNames.PhaseStatus,
		EntryHeaderFilterUserControl.ColumnNames.AcceptanceDate,
		EntryHeaderFilterUserControl.ColumnNames.ActivationDeadline,
		EntryHeaderFilterUserControl.ColumnNames.SelectionResult,
		EntryHeaderFilterUserControl.ColumnNames.SelectionResultDescription,
	};

	static List<string> AdditionalColumns => new List<string>()
	{
		EntryHeaderFilterUserControl.ColumnNames.PhaseStatus,
		EntryHeaderFilterUserControl.ColumnNames.AcceptanceDate,
		EntryHeaderFilterUserControl.ColumnNames.ActivationDeadline,
		EntryHeaderFilterUserControl.ColumnNames.LastEComStatus,
		EntryHeaderFilterUserControl.ColumnNames.SelectionResult,
		EntryHeaderFilterUserControl.ColumnNames.SelectionResultDescription,
	};

	static List<string> AdditionalInvisibleColumns => new List<string>()
	{
		EntryHeaderFilterUserControl.ColumnNames.LastEComStatus
	};

	protected override Customs.Module.EntryHeaderFilterUserControl GetNewEntryHeaderFilterUserControl()
	{
		var declaration = Factory.New<JobDeclaration>();
		var gridCollection = new Customs.Business.CusEntryHeaderCollection<CusEntryHeader>(declaration, Factory);
		var filterBusinessObject = new EntryHeaderFilterBusinessObject();
		return new EntryHeaderFilterUserControl(gridCollection, filterBusinessObject);
	}
}

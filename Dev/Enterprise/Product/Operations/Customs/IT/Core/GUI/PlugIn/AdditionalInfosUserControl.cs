using System.Collections.Generic;
using System.Collections.Immutable;
using Enterprise.Core.Forms;

namespace Enterprise.Customs.IT.GUI;

public partial class AdditionalInfosUserControl : EU.GUI.PlugIn.AdditionalInfosUserControl
{
	public AdditionalInfosUserControl()
	{
		InitializeComponent();
		InitializeITSpecificColumns();
	}

	void InitializeITSpecificColumns()
	{
		var columnsToKeep = VisibleColumns;
		AdditionalInfosGrid.ColumnStyles.Clear();

		foreach (var column in columnsToKeep)
		{
			AdditionalInfosGrid.ColumnStyles.Add(column);
		}
	}

	protected virtual IEnumerable<ZGridColumnInfo> VisibleColumns => new List<ZGridColumnInfo>()
	{
		AdditionalInfosGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_Code),
		AdditionalInfosGrid.GetColumnStyle(Customs.Business.AutoCusSupportingInfo.Schema.CSI_Description)
	}.ToImmutableArray();
}

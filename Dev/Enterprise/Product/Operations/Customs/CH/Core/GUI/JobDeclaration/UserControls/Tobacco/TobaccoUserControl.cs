using System;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class TobaccoUserControl : ZUserControl
{
	public TobaccoUserControl()
	{
		InitializeComponent();
		DynamicTobaccoPanel.UpdateLayout(TobaccoPanelLayout);
	}

	bool isExport;

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		if (DataSource != null)
		{
			if (DataSource is JobDeclaration dec)
			{
				isExport = dec.IsExportOrExportDeclarationActivation;
			}
		}

		using (TobaccosGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			TobaccosGrid.SetAllAvailability(false);
			InitializeGridLayout();
			TobaccosGrid.SetAllAvailability(true);
		}
	}

	void InitializeGridLayout()
	{
		if (isExport)
		{
			TobaccosGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
			{
				ColumnName = Tobacco.Schema.CSI_ReferenceNumber,
			});
			TobaccosGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				ColumnName = Tobacco.Schema.CSI_UnitOfQuantity,
			});
		}
	}

	IPanelLayoutProvider TobaccoPanelLayout => tobaccoPanelLayout ?? (tobaccoPanelLayout = GetNewtobaccoPanelLayout());

	IPanelLayoutProvider tobaccoPanelLayout;

	protected virtual IPanelLayoutProvider GetNewtobaccoPanelLayout() => new TobaccoFieldsLayout();
}

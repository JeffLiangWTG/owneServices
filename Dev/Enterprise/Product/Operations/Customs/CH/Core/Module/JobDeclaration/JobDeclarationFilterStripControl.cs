using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Module;

public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
{
	[Obsolete("Do not call. Only for designer use.")]
	public JobDeclarationFilterStripControl()
	{
		InitializeComponent();
	}

	public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
		: base(module, gridCollection, filterBusinessObject)
	{
		InitializeComponent();

		grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
		{
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("5e75bbfe-7256-4872-a898-fd2067f8b72b", "Phase Status"),
				ColumnName = JobDeclaration.Schema.PhaseStatus,
				IsReadOnly = true,
				IsVisible = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("CD1DE234-1B91-45C9-94A1-9AFCAFA6E919", "Phase Status Description"),
				ColumnName = JobDeclaration.Schema.PhaseStatusDescription,
				IsReadOnly = true,
				IsVisible = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("C421C9A4-7025-4DC0-81E0-3CED665B6CDE", "Selection Result"),
				ColumnName = JobDeclaration.Schema.SelectionResult,
				IsReadOnly = true,
				IsVisible = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("56C4675E-187F-4F90-AFB4-C1ABC09B95F0", "Selection Result Description"),
				ColumnName = JobDeclaration.Schema.SelectionResultDescription,
				IsReadOnly = true,
				IsVisible = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			}
		});
	}
}

using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Module;

public partial class EntryHeaderFilterUserControl : Customs.Module.EntryHeaderFilterUserControl
{
	[Obsolete("Use the constructor that takes a collection and/or business object, this constructor is just for the designer")]
	public EntryHeaderFilterUserControl()
	{
		InitializeComponent();
	}

	public EntryHeaderFilterUserControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
	{
		InitializeComponent();
	}

	public static class ColumnNames
	{
		public const string PhaseStatus = "CH_PhaseStatus";
		public const string SelectionResult = "SelectionResult";
		public const string SelectionResultDescription = "SelectionResultDescription";
		public const string AcceptanceDate = "CusEntryNumber+CE_IssueDate";
		public const string ActivationDeadline = "CusEntryNumber+CE_ExpiryDate";
		public const string LastEComStatus = "EComMessageStatusDescription";
	}

	protected override void InitialiseGridCore()
	{
		base.InitialiseGridCore();
		grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
		{
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("E3DD816E-6BA3-4603-9791-216442E2CA1A", "Phase Status"),
				ColumnName = ColumnNames.PhaseStatus,
				IsReadOnly = true,
				IsVisible = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			},
			new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("E2A94B83-9FCF-4042-AA33-BAA7DE7075B3", "Acceptance Date"),
				ColumnName = ColumnNames.AcceptanceDate,
				DateTimeFormat = ZDateTimePickerFormat.Short,
				IsReadOnly = true,
				IsVisible = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZArchitecture.ZDateEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("F740E9B7-9799-4321-8C0A-C3D06B8C9B42", "Activation Deadline"),
				ColumnName = ColumnNames.ActivationDeadline,
				DateTimeFormat = ZDateTimePickerFormat.Short,
				IsReadOnly = true,
				IsVisible = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("6147B309-6312-4503-8126-202613A82B0A", "eCom Status"),
				ColumnName = ColumnNames.LastEComStatus,
				IsReadOnly = true,
				IsVisible = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("1F3E1FA3-22A7-4E65-8996-DEC10BBCF749", "Selection Result"),
				ColumnName = ColumnNames.SelectionResult,
				IsReadOnly = true,
				IsVisible = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("C33F3441-7C96-4F9B-9BF9-B7F69E8223BD", "Selection Result Desc."),
				ColumnName = ColumnNames.SelectionResultDescription,
				IsReadOnly = true,
				IsVisible = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			}
		});
	}
}

using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Module
{
	public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			RemoveGridColumns();
			AddGridColumns();
		}

		protected void RemoveGridColumns()
		{
			grid.SetAvailability(false, JobDeclaration.Schema.JE_EntrySubmittedDate);
		}

		protected void AddGridColumns()
		{
			grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("5e75bbfe-7256-4872-a898-fd2067f8b72a", "Administrative Status"),
						ColumnName = JobDeclaration.Schema.AdminstrativeStatus,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("90eff1e6-b467-489b-8cfb-bf702425c058", "Administrative Status Description"),
						ColumnName = JobDeclaration.Schema.AdminstrativeStatusDescription,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("40e82753-c7aa-4879-a622-b458a0855bf4", "Cargo Status"),
						ColumnName = JobDeclaration.Schema.CargoStatus,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("bfa83538-5ddd-427a-9679-30e7c028a16f", "Cargo Status Description"),
						ColumnName = JobDeclaration.Schema.CargoStatusDescription,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("849265a8-3b61-4700-aee2-50806af75f4f", "Clearance Date"),
						ColumnName = JobDeclaration.Schema.ClearanceDateAsString,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("5E63CE5B-2657-4EFB-B413-B4C8E50A958E", "Entry Submitted"),
						ColumnName = JobDeclaration.Schema.EntrySubmitDateAsString,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},

				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("1D2021F3-7C6C-4B3A-B765-8B9BA6128F59", "Issue Date"),
						ColumnName = JobDeclaration.Schema.EntryIssueDateAsString,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("a6775a37-6e3a-4b68-a165-1401f431c5fa", "Risk Channel"),
						ColumnName = JobDeclaration.Schema.RiskChannel,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("ba56a154-59cf-496c-8226-3b73662837ec", "Risk Channel Description"),
						ColumnName = JobDeclaration.Schema.RiskChannelDescription,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
			});
		}
	}
}

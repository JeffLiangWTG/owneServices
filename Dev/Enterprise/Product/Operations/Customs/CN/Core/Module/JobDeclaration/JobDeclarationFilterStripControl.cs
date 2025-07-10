using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Module
{
	public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			AddGridColumns();

			grid.ColourDeciding += grid_ColourDeciding;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		protected void AddGridColumns()
		{
			grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZDateEditColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("3A61B9E4-16E3-4912-BFA7-371776DC3C4E", "Deadline", "Declaration Deadline", ""),
						ColumnName = ColumnNames.DeclarationDeadline,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
						DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
					},

				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("54F91709-98BC-45EE-A4E8-584E21175B10", "Remaining Days", "Remaining Days For Declaration", ""),
						ColumnName = ColumnNames.RemainingDaysForDeclaration,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
			});
		}
		public static class ColumnNames
		{
			public const string DeclarationDeadline = "DeclarationDeadline";
			public const string RemainingDaysForDeclaration = "RemainingDaysForDeclaration";
		}

		protected void grid_ColourDeciding(object sender, ZArchitecture.ColourDecidingEventArgs e)
		{
			var jobDeclaration = (JobDeclaration)e.ObjectAtRow;
			if (jobDeclaration.JE_ApplicationCode == Customs.Business.DeclarationApplicationCodeList.Codes.Builtin)
			{
				e.Colour = Color.Empty;
				var remainingDays = jobDeclaration.RemainingDaysForDeclaration;
				if (remainingDays != 0)
				{
					e.Colour = CNCustomsDataRegistry.GetColorByRemainingDays(remainingDays, jobDeclaration.TransportMode);
				}
			}
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Module
{
	public partial class JobDeclarationFilterStripControl : EU.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(module, gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}

		protected override void InitializeAdditionalGridColumns()
		{
			base.InitializeAdditionalGridColumns();

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("F5568279-638F-4803-9AF7-8F2EAA32040C", "Fallback Entry Number"),
				ColumnName = JobDeclaration.Schema.FallbackEntryNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("3CF60869-9A66-402A-BF37-74CAD029F9E2", "Fallback Entry Date"),
				ColumnName = JobDeclaration.Schema.FallbackEntryDate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("C1C481D1-8B9D-4C98-B0BE-19534806C22B", "Fallback Entry Status"),
				ColumnName = JobDeclaration.Schema.FallbackEntryStatus,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("773224BB-6F2B-4749-A98B-42793D80A40F", "Delta agreement (profile) number"),
				ColumnName = JobDeclaration.Schema.JE_CustomsProfile,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("C11DCDEC-8ABF-4925-A7A7-E9B942D68854", "Delta Mode"),
				ColumnName = JobDeclaration.Schema.JE_DeltaMode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("525BFA04-DE9E-40D6-818B-DFB0EE264D66", "Is Delta D Step One Sent"),
				ColumnName = JobDeclaration.Schema.IsDeltaDStepOneSentOK,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("348D1D9B-B0D7-4B59-B21E-B30456C13F1C", "Is Delta D Step Two Sent"),
				ColumnName = JobDeclaration.Schema.IsDeltaDStepTwoSentOK,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("118979BC-545D-4CA0-A0A3-5A6D9E7205E6", "Is Delta D Step Two Sent but 0 Liquidation"),
				ColumnName = JobDeclaration.Schema.IsDeltaDStepTwoSentOKButZeroLiquidation,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("A97D4631-1EAF-4418-89C8-C4C6F6DEA6E0", "ECS Status", "Export Control Status"),
				ColumnName = JobDeclaration.Schema.EntryExitedStatus,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("9EE24CA2-70CA-4108-B554-41BDCE545AF7", "Assessment Date"),
				ColumnName = JobDeclaration.Schema.AssessmentDate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("513F7150-6893-41F5-A9C0-857273182C42", "Latest status Date"),
				ColumnName = JobDeclaration.Schema.CustomsLastEntryStatusDate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("204494D9-E447-4665-8126-DDD4E3789C66", "VAA Trig. Point"),
				ColumnName = JobDeclaration.Schema.TriggeringPointForValidation,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("DFE5A3AD-DAC3-41D6-81F3-343397493449", "Export Exit Type"),
				ColumnName = JobDeclaration.Schema.JE_ExportExitType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("7968b002-5fa0-4c28-880e-005c505ad9ad", "LRN/Correlation ID"),
				ColumnName = JobDeclaration.Schema.CorrelationID,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
			});
		}
	}
}

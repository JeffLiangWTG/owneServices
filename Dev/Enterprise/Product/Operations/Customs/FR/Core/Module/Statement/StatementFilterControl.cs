using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module
{
	public partial class StatementFilterControl : ZFilterStripControl<StatementFilterStrip>
	{
		public StatementFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			AddCustomColumns();
		}

		void AddCustomColumns()
		{
			grid.ColumnStyles.AddRange(new ZGridColumnInfo[]
			{
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.B2_StatementNumber), ControlDpiScalingHelper.ScaleToCurrentDpiX(100)),
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.CorrelationID), ControlDpiScalingHelper.ScaleToCurrentDpiX(100)),
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.EntryNumber), ControlDpiScalingHelper.ScaleToCurrentDpiX(100)),
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.B2_PaymentType), ControlDpiScalingHelper.ScaleToCurrentDpiX(100)) { GroupName = Res.GetData("FR.StatementFilterControl.GroupNames.PaymentMethod", "Payment Method") },
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.PaymentTypeDescription), ControlDpiScalingHelper.ScaleToCurrentDpiX(120))
				{
					GroupName = Res.GetData("FR.StatementFilterControl.GroupNames.PaymentMethod", "Payment Method"),
					CharacterCasing = CharacterCasing.Normal
				},
				new ZDateEditColumnStyleInfo(nameof(CusStatementHeader.B2_ProcessDate), ControlDpiScalingHelper.ScaleToCurrentDpiX(80)),
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.B2_StatementType), ControlDpiScalingHelper.ScaleToCurrentDpiX(80)) { GroupName = Res.GetData("FR.StatementFilterControl.GroupNames.Frequency", "Frequency") },
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.StatementTypeDescription), ControlDpiScalingHelper.ScaleToCurrentDpiX(120))
				{
					GroupName = Res.GetData("FR.StatementFilterControl.GroupNames.Frequency", "Frequency"),
					CharacterCasing = CharacterCasing.Normal
				},
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.B2_EntryFilerCode), ControlDpiScalingHelper.ScaleToCurrentDpiX(80)),
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.B2_AccountNo), ControlDpiScalingHelper.ScaleToCurrentDpiX(80)),
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.B2_BranchDesignation), ControlDpiScalingHelper.ScaleToCurrentDpiX(80)) { GroupName = Res.GetData("FR.StatementFilterControl.GroupNames.Type", "Type") },
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.BranchDesignationDescription), ControlDpiScalingHelper.ScaleToCurrentDpiX(80))
				{
					GroupName = Res.GetData("FR.StatementFilterControl.GroupNames.Type", "Type"),
					CharacterCasing = CharacterCasing.Normal
				},
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.B2_ImporterCustomsID), ControlDpiScalingHelper.ScaleToCurrentDpiX(100)),
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.ImporterFullName), ControlDpiScalingHelper.ScaleToCurrentDpiX(120)),
				new ZDateEditColumnStyleInfo(nameof(CusStatementHeader.B2_DueDate), ControlDpiScalingHelper.ScaleToCurrentDpiX(80)),
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.B2_Status), ControlDpiScalingHelper.ScaleToCurrentDpiX(80)) { GroupName = Res.GetData("FR.StatementFilterControl.GroupNames.Status", "Status") },
				new ZTextBoxColumnStyleInfo(nameof(CusStatementHeader.StatusDescription), ControlDpiScalingHelper.ScaleToCurrentDpiX(100))
				{
					GroupName = Res.GetData("FR.StatementFilterControl.GroupNames.Status", "Status"),
					CharacterCasing = CharacterCasing.Normal
				}
			});
		}
	}
}

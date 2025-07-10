using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.FR.GUI
{
	public partial class DeltaGEntryInstructionAuthorisationsUserControl : EU.GUI.EntryInstructionAuthorisationsUserControl
	{
		public DeltaGEntryInstructionAuthorisationsUserControl()
		{
			InitializeComponent();
			UpdateGridLayout();
		}
		void UpdateGridLayout()
		{
			AddCustomsCodeToGrid();
		}

		void AddCustomsCodeToGrid()
		{
			var zTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo.IsReadOnly = true;
			zTextBoxColumnStyleInfo.ColumnName = FR.Business.CusAuthorizationUsage.Schema.AGC_AuthorizationShortCode;
			zTextBoxColumnStyleInfo.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			AuthorisationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo);
		}
	}
}

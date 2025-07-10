using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class NctsPreviousDocumentsUserControl : EU.NCTS.GUI.PreviousDocumentsUserControl
	{
		public NctsPreviousDocumentsUserControl()
		{
			InitializeComponent();
		}

		protected override void InitializeGridLayout()
		{
			base.InitializeGridLayout();
			using (PreviousDocumentsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				PreviousDocumentsGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo()
				{
					ColumnName = NctsPreviousDocument.Schema.CSI_LineNo,
					Decimals = 0,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				});
			}
		}
	}
}

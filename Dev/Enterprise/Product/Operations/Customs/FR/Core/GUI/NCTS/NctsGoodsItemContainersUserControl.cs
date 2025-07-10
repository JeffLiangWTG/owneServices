using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public partial class NctsGoodsItemContainersUserControl : EU.NCTS.GUI.GoodsItemContainersUserControl
	{
		public NctsGoodsItemContainersUserControl()
		{
			InitializeComponent();
		}

		protected override void InitializeGridLayout()
		{
			base.InitializeGridLayout();
			using (SelectedContainersGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				SelectedContainersGrid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo()
				{
					ColumnName = NonPersistentDepartureContainerPivot.Schema.ContainerType,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefContainer
				});

				SelectedContainersGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo()
				{
					ColumnName = NonPersistentDepartureContainerPivot.Schema.ContainerMode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				});
			}
		}
	}
}

using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public partial class FRContainersAndSealsUserControl : ContainersAndSealsUserControl
	{
		public FRContainersAndSealsUserControl()
		{
			InitializeComponent();
			InitializeGridColumns();
		}

		void InitializeGridColumns()
		{
			ContainersGrid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo()
			{
				ColumnName = FRNctsDepartureHeaderContainer.Schema.BC_RC,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefContainer
			});

			ContainersGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo()
			{
				ColumnName = FRNctsDepartureHeaderContainer.Schema.BC_Mode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
			});
		}
	}
}

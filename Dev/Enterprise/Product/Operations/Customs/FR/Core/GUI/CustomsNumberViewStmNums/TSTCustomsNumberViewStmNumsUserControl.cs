using Enterprise.Customs.FR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public partial class TSTCustomsNumberViewStmNumsUserControl : CustomsNumberViewStmNumsUserControl
	{
		public TSTCustomsNumberViewStmNumsUserControl(CustomsNumberViewStmNumsWrapperCollection collection) : base(collection)
		{
			InitializeComponent();
			InitializeNumberRangesGrid();
			HideThresholdRunOutWarningGroupBox();
		}

		void InitializeNumberRangesGrid()
		{
			using (NumberRangesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				NumberRangesGrid.ReOrderColumns(
					[
						TSTCustomsNumberViewStmNumsWrapper.Schema.SN_OwnerForDisplay,
						TSTCustomsNumberViewStmNumsWrapper.Schema.SN_Type,
						TSTCustomsNumberViewStmNumsWrapper.Schema.SN_TypeDescription,
						TSTCustomsNumberViewStmNumsWrapper.Schema.SN_FountainName,
						TSTCustomsNumberViewStmNumsWrapper.Schema.SN_ValueForDisplay,
						TSTCustomsNumberViewStmNumsWrapper.Schema.SN_AvailableNumbers,
						TSTCustomsNumberViewStmNumsWrapper.Schema.SN_MinimumValue,
						TSTCustomsNumberViewStmNumsWrapper.Schema.SN_Count,
						TSTCustomsNumberViewStmNumsWrapper.Schema.SN_MaximumValue,
						TSTCustomsNumberViewStmNumsWrapper.Schema.SN_SystemCreateTimeUtc
					]);
			}
		}

		void HideThresholdRunOutWarningGroupBox()
		{
			ThresholdRunOutWarningGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0, true);
			ThresholdRunOutWarningGroupBox.Visible = false;
		}
	}
}

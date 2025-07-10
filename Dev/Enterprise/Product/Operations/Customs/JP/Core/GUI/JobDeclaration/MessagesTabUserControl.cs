using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.JP.GUI
{
	public partial class MessagesTabUserControl : MessagesUserControl
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected void InitializeGridLayout()
		{
			using (MessagesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				MessagesBoundGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
				{
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = Common.EDIMessage.Schema.EM_Calc_ProcedureCode,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
					},
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = Common.EDIMessage.Schema.EM_Calc_ProcedureName,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104)
					},
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = Common.EDIMessage.Schema.EM_Calc_OutputInformationCode,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
					},
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = Common.EDIMessage.Schema.EM_Calc_OutputInformation,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(124)
					}
				});
			}

			ResetGridDefaultOrderAndVisibleColumns(MessagesBoundGrid, reOrderedColumns);
		}

		void ResetGridDefaultOrderAndVisibleColumns(ZGrid grid, string[] orderColumns)
		{
			using (grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				grid.ReOrderColumns(orderColumns);
			}
		}

		readonly string[] reOrderedColumns =
		{
			Common.EDIMessage.Schema.EM_Calc_ProcedureCode,
			Common.EDIMessage.Schema.EM_Calc_ProcedureName,
			Common.EDIMessage.Schema.EM_Calc_OutputInformationCode,
			Common.EDIMessage.Schema.EM_Calc_OutputInformation,
			Common.EDIMessage.Schema.EM_MessageNum,
			Common.EDIMessage.Schema.EM_MessageType,
			Common.EDIMessage.Schema.EM_MessageSubType,
			Common.EDIMessage.Schema.EM_MessageDateTime,
			Common.EDIMessage.Schema.EM_SystemCreateTimeUtc,
			Common.EDIMessage.Schema.EM_InterchangeNumber,
			Common.EDIMessage.Schema.EM_DateTimeInterchangeSent,
			Common.EDIMessage.Schema.EM_User,
			Common.EDIMessage.Schema.EM_Status,
			Common.EDIMessage.Schema.EM_ReceiveTransmit,
		};
	}
}

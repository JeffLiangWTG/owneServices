using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	[CodeAlive("Unused class as using EU.ExitControl.GUI.PlugIn.ExitControlPlugIn instead of EU.GUI.PlugIn.ExitSummaryPlugIn")]
	public partial class ExitSummaryUserControl : EU.GUI.PlugIn.ExitSummaryUserControl
	{
		public ExitSummaryUserControl()
		{
			InitializeComponent();
			InitializeAdditionalDocumentsControl();
			RemoveControls();
		}

		protected override void InitializeLayoutMovementsGrid()
		{
			base.InitializeLayoutMovementsGrid();
			var arrivalNotificationDateColumnStyle = (ZDateEditColumnStyleInfo)MovementsGrid.GetColumnStyle(CusExitDetailSchema.Constants.CED_ArrivalNotificationDate);
			arrivalNotificationDateColumnStyle.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long;
			arrivalNotificationDateColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);

			var exitDateColumnStyle = (ZDateEditColumnStyleInfo)MovementsGrid.GetColumnStyle(CusExitDetailSchema.Constants.CED_ExitDate);
			exitDateColumnStyle.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long;
			exitDateColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);

			var movementReferenceNumberColumnStyle = MovementsGrid.GetColumnStyle(CusExitDetailSchema.Constants.CED_MovementReferenceNumber);
			movementReferenceNumberColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);

			var transportIDColumnStyle = MovementsGrid.GetColumnStyle(CusExitDetailSchema.Constants.CED_TransportID);
			transportIDColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			var arrivalNotificationPlaceColumnStyle = MovementsGrid.GetColumnStyle(EU.Business.AutoCusExitDetail.Schema.CED_ArrivalNotificationPlace);
			MovementsGrid.ColumnStyles.Remove(arrivalNotificationPlaceColumnStyle);

			MovementsGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = EU.Business.AutoCusExitDetail.Schema.CED_LocationOfGoods,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					CharacterCasing = System.Windows.Forms.CharacterCasing.Normal
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = CusExitDetail.Schema.ReferenceNumberUCR,
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = CusExitDetail.Schema.RegistrationNumberAWB,
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = CusExitDetail.Schema.StatusDescription,
					CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160),
				}
			});

			MovementsGrid.ReOrderColumns(ColumnsInOrder);
		}

		protected override void InitializeLayoutItemsGrid()
		{
			base.InitializeLayoutItemsGrid();

			var lineNumberColumnStyle = ItemsGrid.GetColumnStyle(CusExitItemSchema.Constants.CXI_LineNumber);
			lineNumberColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);

			var netMassColumnStyle = ItemsGrid.GetColumnStyle(CusExitItemSchema.Constants.CXI_NetMass);
			netMassColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(71);

			var grossMassColumnStyle = ItemsGrid.GetColumnStyle(CusExitItemSchema.Constants.CXI_GrossMass);
			grossMassColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(81);

			var statusColumnStyle = ItemsGrid.GetColumnStyle(CusExitItemSchema.Constants.CXI_Status);
			statusColumnStyle.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);

			ItemsGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = CusExitItem.Schema.ReferenceNumberUCR,
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(138),
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = CusExitItem.Schema.RegistrationNumberAWB,
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(153),
				}
			});
		}

		void RemoveControls()
		{
			TopPanel.Controls.Remove(HeaderArrivalNotificationPlaceTextBox);
			MovementDetailsPanel.Controls.Remove(ArrivalNotificationPlaceTextBox);
		}

		void InitializeAdditionalDocumentsControl()
		{
			SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(AdditionalDocumentsUserControl, nameof(CusExitControlHeader.CusExitDetails), SupportingInfoColumnLayoutContext);
		}

		string[] ColumnsInOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					columnNamesInSortOrder = new[]
					{
						CusExitDetail.Schema.CED_MovementReferenceNumber,
						CusExitDetail.Schema.ReferenceNumberUCR,
						CusExitDetail.Schema.RegistrationNumberAWB,
						CusExitDetail.Schema.CED_CustomsOffice,
						CusExitDetail.Schema.CED_ArrivalNotificationDate,
						CusExitDetail.Schema.CED_ExitDate,
						CusExitDetail.Schema.CED_TransportID,
						CusExitDetail.Schema.CED_LocationOfGoods,
						CusExitDetail.Schema.CED_Status,
						CusExitDetail.Schema.StatusDescription
					};
				}
				return columnNamesInSortOrder;
			}
		}
		string[] columnNamesInSortOrder;

		const string SupportingInfoColumnLayoutContext = "INV";
	}
}

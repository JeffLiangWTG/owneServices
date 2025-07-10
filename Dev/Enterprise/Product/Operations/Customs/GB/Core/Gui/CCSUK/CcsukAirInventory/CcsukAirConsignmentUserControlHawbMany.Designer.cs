using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukAirConsignmentUserControlHawbMany : ZUserControl
	{
		CcsukHawbControl ccsukHawbControl;
		ZArchitecture.ZLabel zLabel1;
		ZArchitecture.ZGrid hawbsGrid;

		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.hawbsGrid = new ZArchitecture.ZGrid();
			this.zLabel1 = new ZArchitecture.ZLabel();
			this.ccsukHawbControl = new CcsukHawbControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.hawbsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GB.Ccsuk.AirCargoInventory.BusinessObjects.ShipmentToManyHawbsPluginHelper);
			// 
			// MawbsGrid
			// 
			this.hawbsGrid.AllowNavigation = false;
			this.hawbsGrid.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.hawbsGrid, "Hawbs");
			this.hawbsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("55173f40-452f-4507-8485-086fc36abd33", "Airport & Shed");
			zTextBoxColumnStyleInfo2.ColumnName = "CargoTerminalOperatorAirportAndShed";
			zTextBoxColumnStyleInfo2.GroupName = null;
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("9a52deb2-a2e7-457b-b8db-d9f2e34abab0", "MAWB Number");
			zTextBoxColumnStyleInfo3.ColumnName = "ReferenceNumber";
			zTextBoxColumnStyleInfo3.GroupName = null;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("7cfdd2cb-38de-44e2-b347-96c96c914b68", "", "Customs Action Code", "CAC", "");
			zTextBoxColumnStyleInfo4.ColumnName = "CustomsActionCode";
			zTextBoxColumnStyleInfo4.GroupName = null;
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("176b916c-29f7-42a2-a39d-00ae92c5a60b", "", "Agent Badge", "Agent", "");
			zTextBoxColumnStyleInfo5.ColumnName = "AgentBadge";
			zTextBoxColumnStyleInfo5.GroupName = null;
			this.hawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.hawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.hawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.hawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.hawbsGrid.CopySelectedRowsAllowed = true;
			this.hawbsGrid.GridId = "c24bfa3c-7d24-4014-ab38-055407a2a8c7";
			this.hawbsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.hawbsGrid.LayoutKey = "HawbsGrid";
			this.hawbsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 39, true);
			this.hawbsGrid.Name = "HawbsGrid";
			this.hawbsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1081, 80, true);
			this.hawbsGrid.TabIndex = 0;

			hawbsGrid.ReadOnly = true;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = null;
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(953, 32, true);
			this.zLabel1.TabIndex = 2;
			this.zLabel1.Text = "Multiple inventory records exist for this HAWB number or MAWB number.  Please select from the gr" +
				"id below.  To send messages, ensure you select the correct HAWB from the menu. ";
			// 
			// ccsukAirConsignmentUserControlMawb1
			// 
			this.ccsukHawbControl.AllowDrop = true;
			this.ccsukHawbControl.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.ccsukHawbControl, "Hawbs");
			this.ccsukHawbControl.CaptionResourceString = null;
			this.ccsukHawbControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 125, true);
			this.ccsukHawbControl.Name = "ccsukHawbControl";
			this.ccsukHawbControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1081, 643, true);
			this.ccsukHawbControl.TabIndex = 1;
			// 
			// CcsukAirConsignmentUserControlMawbMany
			// 
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.ccsukHawbControl);
			this.Controls.Add(this.hawbsGrid);
			this.Name = "CcsukAirConsignmentUserControlHawbMany";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1088, 771, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.hawbsGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}

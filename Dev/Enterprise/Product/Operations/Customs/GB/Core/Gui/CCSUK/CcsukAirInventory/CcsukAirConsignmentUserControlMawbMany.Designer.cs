using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CcsukAirConsignmentUserControlMawbMany : ZUserControl
	{
		private ZPanel zPanel2;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZGrid mawbsGrid;
		private CcsukAirConsignmentUserControlMawb ccsukAirConsignmentUserControlMawb1;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.mawbsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.ccsukAirConsignmentUserControlMawb1 = new Enterprise.Customs.GB.GUI.Ccsuk.CcsukAirConsignmentUserControlMawb();
			this.zPanel3 = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mawbsGrid)).BeginInit();
			this.mawbsGrid.SuspendLayout();
			this.ccsukAirConsignmentUserControlMawb1.SuspendLayout();
			this.zPanel3.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.ConsolToManyMawbsPluginHelper);
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.mawbsGrid);
			this.zPanel2.Controls.Add(this.zLabel1);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1081, 100, true);
			this.zPanel2.TabIndex = 3;
			// 
			// mawbsGrid
			// 
			this.mawbsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.mawbsGrid, "Mawbs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.ConsolToManyMawbsPluginHelper)(null)).Mawbs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.ConsolToManyMawbsPluginHelper)(null)).Mawbs)).SyncRoot)).CargoTerminalOperatorAirportAndShed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.ConsolToManyMawbsPluginHelper)(null)).Mawbs)).SyncRoot)).ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.ConsolToManyMawbsPluginHelper)(null)).Mawbs)).SyncRoot)).CustomsActionCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.ConsolToManyMawbsPluginHelper)(null)).Mawbs)).SyncRoot)).AgentBadge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.ConsolToManyMawbsPluginHelper)(null)).Mawbs)).SyncRoot)).CM_SystemCreateTimeUtc)));
			this.mawbsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("55173f40-452f-4507-8485-086fc36abd33", "Airport & Shed");
			zTextBoxColumnStyleInfo1.ColumnName = "CargoTerminalOperatorAirportAndShed";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("9a52deb2-a2e7-457b-b8db-d9f2e34abab0", "MAWB Number");
			zTextBoxColumnStyleInfo2.ColumnName = "ReferenceNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("7cfdd2cb-38de-44e2-b347-96c96c914b68", "", "Customs Action Code", "CAC", "");
			zTextBoxColumnStyleInfo3.ColumnName = "CustomsActionCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("176b916c-29f7-42a2-a39d-00ae92c5a60b", "", "Agent Badge", "Agent", "");
			zTextBoxColumnStyleInfo4.ColumnName = "AgentBadge";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("176b916c-1234-42a2-a39d-00ae92c5a60b", "", "Created Date", "Created", "");
			zDateEditColumnStyleInfo1.ColumnName = "CM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.mawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.mawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.mawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.mawbsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.mawbsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.mawbsGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.mawbsGrid.GridId = "c24bfa3c-7d24-4014-ab38-055407a2a8c7";
			this.mawbsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.mawbsGrid.LayoutKey = "MawbsGrid";
			this.mawbsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.mawbsGrid.Name = "mawbsGrid";
			this.mawbsGrid.ReadOnly = true;
			this.mawbsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1081, 100, true);
			this.mawbsGrid.TabIndex = 1;
			// 
			// zLabel1
			// 
			this.zLabel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1081, 32, true);
			this.zLabel1.TabIndex = 3;
			this.zLabel1.Text = "Multiple inventory records exist for this MAWB number.  Please select from the gr" +
    "id below.  To send messages, ensure you select the correct MAWB from the menu. ";
			// 
			// ccsukAirConsignmentUserControlMawb1
			// 
			this.ccsukAirConsignmentUserControlMawb1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ccsukAirConsignmentUserControlMawb1, "Mawbs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.CusMAWB)(((System.Collections.IList)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.ConsolToManyMawbsPluginHelper)(null)).Mawbs)).SyncRoot)))));
			this.ccsukAirConsignmentUserControlMawb1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ccsukAirConsignmentUserControlMawb1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 100, true);
			this.ccsukAirConsignmentUserControlMawb1.Name = "ccsukAirConsignmentUserControlMawb1";
			this.ccsukAirConsignmentUserControlMawb1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1081, 664, true);
			this.ccsukAirConsignmentUserControlMawb1.TabIndex = 1;
			// 
			// zPanel3
			// 
			this.zPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zPanel3.Controls.Add(this.ccsukAirConsignmentUserControlMawb1);
			this.zPanel3.Controls.Add(this.zPanel2);
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1081, 764, true);
			this.zPanel3.TabIndex = 4;
			// 
			// CcsukAirConsignmentUserControlMawbMany
			// 
			this.Controls.Add(this.zPanel3);
			this.Name = "CcsukAirConsignmentUserControlMawbMany";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1088, 771, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.mawbsGrid)).EndInit();
			this.mawbsGrid.ResumeLayout(false);
			this.mawbsGrid.PerformLayout();
			this.ccsukAirConsignmentUserControlMawb1.ResumeLayout(true);
			this.ccsukAirConsignmentUserControlMawb1.PerformLayout();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZPanel zPanel3;
	}
}

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class CMRAirCargoHouseUserControl
	{
		void InitializeComponent()
		{
			this.cmrHouseDetailsUserControl = new Enterprise.Customs.AU.AirCargo.GUI.CMRHouseDetailsUserControl();
			this.masterDetailsUserControl.SuspendLayout();
			this.CommercialStatusDropEdit.SuspendLayout();
			this.MasterTabControl.SuspendLayout();
			this.HouseDetailsTabPage.SuspendLayout();
			this.airCargoHouseBillPartiesUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.cmrHouseDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// masterDetailsUserControl
			// 
			this.masterDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 116, true);
			// 
			// CommercialStatusDropEdit
			// 
			this.CommercialStatusDropEdit.TabIndex = 3;
			// 
			// CommercialStatusLabel
			// 
			this.CommercialStatusLabel.TabIndex = 2;
			// 
			// MasterTabControl
			// 
			this.MasterTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 116, true);
			this.MasterTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 336, true);
			this.MasterTabControl.TabIndex = 1;
			// 
			// HouseDetailsTabPage
			// 
			this.HouseDetailsTabPage.AutoScroll = true;
			this.HouseDetailsTabPage.Controls.Add(this.cmrHouseDetailsUserControl);
			this.HouseDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1026, 309, true);
			this.HouseDetailsTabPage.Controls.SetChildIndex(this.airCargoHouseBillPartiesUserControl, 0);
			this.HouseDetailsTabPage.Controls.SetChildIndex(this.cmrHouseDetailsUserControl, 0);
			this.HouseDetailsTabPage.Controls.SetChildIndex(this.CommercialStatusLabel, 0);
			this.HouseDetailsTabPage.Controls.SetChildIndex(this.CommercialStatusDropEdit, 0);
			// 
			// airCargoHouseBillPartiesUserControl
			// 
			this.airCargoHouseBillPartiesUserControl.TabIndex = 1;
			// 
			// cmrHouseDetailsUserControl1
			// 
			this.cmrHouseDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cmrHouseDetailsUserControl, ".");
			this.cmrHouseDetailsUserControl.HAWB = null;
			this.cmrHouseDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.cmrHouseDetailsUserControl.Name = "cmrHouseDetailsUserControl";
			this.cmrHouseDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 265, true);
			this.cmrHouseDetailsUserControl.TabIndex = 0;
			// 
			// CMRAirCargoHouseUserControl
			// 
			this.Name = "CMRAirCargoHouseUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 452, true);
			this.masterDetailsUserControl.ResumeLayout(true);
			this.masterDetailsUserControl.PerformLayout();
			this.CommercialStatusDropEdit.ResumeLayout(true);
			this.CommercialStatusDropEdit.PerformLayout();
			this.MasterTabControl.ResumeLayout(false);
			this.MasterTabControl.PerformLayout();
			this.HouseDetailsTabPage.ResumeLayout(false);
			this.HouseDetailsTabPage.PerformLayout();
			this.airCargoHouseBillPartiesUserControl.ResumeLayout(true);
			this.airCargoHouseBillPartiesUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.cmrHouseDetailsUserControl.ResumeLayout(true);
			this.cmrHouseDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal CMRHouseDetailsUserControl cmrHouseDetailsUserControl;
	}
}

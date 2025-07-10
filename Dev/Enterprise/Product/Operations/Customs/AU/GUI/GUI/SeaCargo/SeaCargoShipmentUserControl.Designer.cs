namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoShipmentUserControl
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.UnderbondAndPackingUserControlForCMR = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoUnderbondAndPackingUserControlForCMR();
			this.HouseDetailsUsersControl = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoHouseDetailsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnderbondAndPackingUserControlForCMR.SuspendLayout();
			this.HouseDetailsUsersControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusSCAHouse);
			// 
			// UnderbondAndPackingUserControlForCMR
			// 
			this.UnderbondAndPackingUserControlForCMR.AllowDrop = true;
			this.UnderbondAndPackingUserControlForCMR.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnderbondAndPackingUserControlForCMR.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 272, true);
			this.UnderbondAndPackingUserControlForCMR.Name = "UnderbondAndPackingUserControlForCMR";
			this.UnderbondAndPackingUserControlForCMR.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 482, true);
			this.UnderbondAndPackingUserControlForCMR.TabIndex = 4;
			// 
			// HouseDetailsUsersControl
			// 
			this.HouseDetailsUsersControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseDetailsUsersControl, ".");
			this.HouseDetailsUsersControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.HouseDetailsUsersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.HouseDetailsUsersControl.Name = "HouseDetailsUsersControl";
			this.HouseDetailsUsersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 267, true);
			this.HouseDetailsUsersControl.TabIndex = 1;
			// 
			// SeaCargoShipmentUserControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.UnderbondAndPackingUserControlForCMR);
			this.Controls.Add(this.HouseDetailsUsersControl);
			this.Name = "SeaCargoShipmentUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 759, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnderbondAndPackingUserControlForCMR.ResumeLayout(true);
			this.UnderbondAndPackingUserControlForCMR.PerformLayout();
			this.HouseDetailsUsersControl.ResumeLayout(true);
			this.HouseDetailsUsersControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoUnderbondAndPackingUserControlForCMR UnderbondAndPackingUserControlForCMR;
		private SeaCargoHouseDetailsUserControl HouseDetailsUsersControl;
		private System.ComponentModel.Container components = null;
	}
}

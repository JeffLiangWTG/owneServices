namespace Enterprise.Customs.DE.NCTS.GUI
{
	partial class GoodsItemDetailsLayoutsUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.GoodsItemDetailsItemNoPlusMainPackUserControl = new Enterprise.Customs.DE.NCTS.GUI.GoodsItemDetailsItemNoPlusMainPackUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.GoodsItemDetailsItemNoPlusMainPackUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc);
            // 
            // GoodsItemDetailsItemNoPlusMainPackUserControl
            // 
            this.GoodsItemDetailsItemNoPlusMainPackUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GoodsItemDetailsItemNoPlusMainPackUserControl, ".");
            this.GoodsItemDetailsItemNoPlusMainPackUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 20, true);
            this.GoodsItemDetailsItemNoPlusMainPackUserControl.Name = "GoodsItemDetailsItemNoPlusMainPackUserControl";
            this.GoodsItemDetailsItemNoPlusMainPackUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
            this.GoodsItemDetailsItemNoPlusMainPackUserControl.TabIndex = 2;
            // 
            // GoodsItemDetailsLayoutsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.GoodsItemDetailsItemNoPlusMainPackUserControl);
            this.Name = "GoodsItemDetailsLayoutsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 154, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.GoodsItemDetailsItemNoPlusMainPackUserControl.ResumeLayout(true);
            this.GoodsItemDetailsItemNoPlusMainPackUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
		}

		internal GoodsItemDetailsItemNoPlusMainPackUserControl GoodsItemDetailsItemNoPlusMainPackUserControl;
	}
}

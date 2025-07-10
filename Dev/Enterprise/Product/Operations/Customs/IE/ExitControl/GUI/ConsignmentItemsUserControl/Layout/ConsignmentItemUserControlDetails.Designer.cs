namespace Enterprise.Customs.IE.ExitControl.GUI
{
	partial class ConsignmentItemUserControlDetails
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MainUserControl = new Enterprise.Customs.IE.ExitControl.GUI.ConsignmentItemUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitConsignmentItemCollection<Enterprise.Customs.IE.ExitControl.Business.CusExitConsignmentItem>);
			// 
			// MainUserControl
			// 
			this.MainUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MainUserControl, ".");
			this.MainUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainUserControl.Name = "MainUserControl";
			this.MainUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 480, true);
			this.MainUserControl.TabIndex = 0;
			this.MainUserControl.TabStop = false;
			// 
			// ConsignmentItemUserControlDetails
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainUserControl);
			this.Name = "ConsignmentItemUserControlDetails";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 480, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainUserControl.ResumeLayout(true);
			this.MainUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ConsignmentItemUserControl MainUserControl;
	}
}

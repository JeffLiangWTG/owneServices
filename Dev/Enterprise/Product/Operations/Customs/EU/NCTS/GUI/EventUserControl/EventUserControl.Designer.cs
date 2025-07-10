namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class EventUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
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
			this.Phase5EventTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5EventTabUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Phase5EventTabUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// Phase5EventTabUserControl
			// 
			this.Phase5EventTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Phase5EventTabUserControl, "EnRouteIncidents");
			this.Phase5EventTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Phase5EventTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 36, true);
			this.Phase5EventTabUserControl.Name = "Phase5EventTabUserControl";
			this.Phase5EventTabUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 5, 0, 0, true);
			this.Phase5EventTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1232, 320, true);
			this.Phase5EventTabUserControl.TabIndex = 2;
			// 
			// EventUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Phase5EventTabUserControl);
			this.Name = "EventUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1232, 356, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Phase5EventTabUserControl.ResumeLayout(true);
			this.Phase5EventTabUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal Phase5EventTabUserControl Phase5EventTabUserControl;
	}
}

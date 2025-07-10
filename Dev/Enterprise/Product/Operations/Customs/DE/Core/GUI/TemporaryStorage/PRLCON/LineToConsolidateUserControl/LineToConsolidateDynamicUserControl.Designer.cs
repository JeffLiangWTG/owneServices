using System.Drawing;

namespace Enterprise.Customs.DE.GUI
{
	partial class LineToConsolidateDynamicUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AWBLineToConsolidateUserControl = new Enterprise.Customs.DE.GUI.AWBLineToConsolidateUserControl();
			this.REGLineToConsolidateUserControl = new Enterprise.Customs.DE.GUI.REGLineToConsolidateUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AWBLineToConsolidateUserControl.SuspendLayout();
			this.REGLineToConsolidateUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec);
			// 
			// AWBLineToConsolidateUserControl
			// 
			this.AWBLineToConsolidateUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AWBLineToConsolidateUserControl, ".");
			this.AWBLineToConsolidateUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AWBLineToConsolidateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AWBLineToConsolidateUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 189, true);
			this.AWBLineToConsolidateUserControl.Name = "AWBLineToConsolidateUserControl";
			this.AWBLineToConsolidateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 328, true);
			this.AWBLineToConsolidateUserControl.TabIndex = 0;
			this.AWBLineToConsolidateUserControl.Visible = false;
			// 
			// REGLineToConsolidateUserControl
			// 
			this.REGLineToConsolidateUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.REGLineToConsolidateUserControl, ".");
			this.REGLineToConsolidateUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.REGLineToConsolidateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.REGLineToConsolidateUserControl.Name = "REGLineToConsolidateUserControl";
			this.REGLineToConsolidateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 328, true);
			this.REGLineToConsolidateUserControl.TabIndex = 1;
			// 
			// LineToConsolidateDynamicUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.REGLineToConsolidateUserControl);
			this.Controls.Add(this.AWBLineToConsolidateUserControl);
			this.Name = "LineToConsolidateDynamicUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 328, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AWBLineToConsolidateUserControl.ResumeLayout(true);
			this.AWBLineToConsolidateUserControl.PerformLayout();
			this.REGLineToConsolidateUserControl.ResumeLayout(true);
			this.REGLineToConsolidateUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		protected internal AWBLineToConsolidateUserControl AWBLineToConsolidateUserControl;
		protected internal REGLineToConsolidateUserControl REGLineToConsolidateUserControl;

		#endregion
	}
}

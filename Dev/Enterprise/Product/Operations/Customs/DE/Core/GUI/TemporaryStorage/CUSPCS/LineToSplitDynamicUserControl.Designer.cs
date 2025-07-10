using System.Drawing;

namespace Enterprise.Customs.DE.GUI
{
	partial class LineToSplitDynamicUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AWBLineToSplitUserControl = new Enterprise.Customs.DE.GUI.AWBLineToSplitUserControl();
			this.REGLineToSplitUserControl = new Enterprise.Customs.DE.GUI.REGLineToSplitUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AWBLineToSplitUserControl.SuspendLayout();
			this.REGLineToSplitUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSCusTempStorageDec);
			// 
			// AWBLineToSplitUserControl
			// 
			this.AWBLineToSplitUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AWBLineToSplitUserControl, "ConsolidatedCusTempStorageLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSCusTempStorageDec)(null)).ConsolidatedCusTempStorageLine)));
			this.AWBLineToSplitUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AWBLineToSplitUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AWBLineToSplitUserControl.Name = "AWBLineToSplitUserControl";
			this.AWBLineToSplitUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 123, true);
			this.AWBLineToSplitUserControl.TabIndex = 0;
			// 
			// REGLineToSplitUserControl
			// 
			this.REGLineToSplitUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.REGLineToSplitUserControl, "ConsolidatedCusTempStorageLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSConsolidatedCusTempStorageLine)(((Enterprise.Customs.DE.Business.CusTempStorage.CUSPCSCusTempStorageDec)(null)).ConsolidatedCusTempStorageLine)));
			this.REGLineToSplitUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.REGLineToSplitUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.REGLineToSplitUserControl.Name = "REGLineToSplitUserControl";
			this.REGLineToSplitUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 123, true);
			this.REGLineToSplitUserControl.TabIndex = 0;
			this.REGLineToSplitUserControl.Visible = false;
			// 
			// LineToSplitDynamicUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AWBLineToSplitUserControl);
			this.Controls.Add(this.REGLineToSplitUserControl);
			this.Name = "LineToSplitDynamicUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 123, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AWBLineToSplitUserControl.ResumeLayout(true);
			this.AWBLineToSplitUserControl.PerformLayout();
			this.REGLineToSplitUserControl.ResumeLayout(true);
			this.REGLineToSplitUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		protected AWBLineToSplitUserControl AWBLineToSplitUserControl;
		protected REGLineToSplitUserControl REGLineToSplitUserControl;

		#endregion
	}
}

using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5GoodsItemPackagesAndContainersUserControl
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
			this.DynamicPackagesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.ContainersUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5PackageContainersUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DynamicPackagesUserControl.SuspendLayout();
			this.ContainersUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsPackage);
			// 
			// DynamicPackagesUserControl
			// 
			this.DynamicPackagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DynamicPackagesUserControl, ".");
			this.DynamicPackagesUserControl.UserControlType = LayoutProvider.GoodsItemPackagesUserControlType;
			this.DynamicPackagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicPackagesUserControl.Name = "DynamicPackagesUserControl";
			this.DynamicPackagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 215, true);
			this.DynamicPackagesUserControl.TabIndex = 0;
			// 
			// ContainersUserControl
			// 
			this.ContainersUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainersUserControl, ".");
			this.ContainersUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 0, true);
			this.ContainersUserControl.Name = "ContainersUserControl";
			this.ContainersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 200, true);
			this.ContainersUserControl.TabIndex = 1;
			// 
			// Phase5GoodsItemPackagesAndContainersUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DynamicPackagesUserControl);
			this.Controls.Add(this.ContainersUserControl);
			this.Name = "Phase5GoodsItemPackagesAndContainersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DynamicPackagesUserControl.ResumeLayout(true);
			this.DynamicPackagesUserControl.PerformLayout();
			this.ContainersUserControl.ResumeLayout(true);
			this.ContainersUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZDynamicControlCreationUserControl DynamicPackagesUserControl;
		internal Phase5PackageContainersUserControl ContainersUserControl;
	}
}

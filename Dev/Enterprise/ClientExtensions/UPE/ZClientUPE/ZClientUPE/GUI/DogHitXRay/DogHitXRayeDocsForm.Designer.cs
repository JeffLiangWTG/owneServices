using System;
using System.ComponentModel;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.UPE.GUI
{
	public partial class DogHitXRayeDocsForm : ZChildForm
	{
		private ZTemplateTabControl zTabControl1;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl oPostingButtonsUserControl;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.oPostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 369, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 24, true);
			// 
			// zTabControl1
			// 
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 331, true);
			this.zTabControl1.TabIndex = 1;
			// 
			// oPostingButtonsUserControl
			// 
			this.oPostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oPostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 337, true);
			this.oPostingButtonsUserControl.Name = "oPostingButtonsUserControl";
			this.oPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.oPostingButtonsUserControl.TabIndex = 3;
			// 
			// DogHitXRayeDocsForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 393, true);
			this.Controls.Add(this.oPostingButtonsUserControl);
			this.Controls.Add(this.zTabControl1);
			this.Name = "DogHitXRayeDocsForm";
			this.Controls.SetChildIndex(this.zTabControl1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.oPostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}
	}
}

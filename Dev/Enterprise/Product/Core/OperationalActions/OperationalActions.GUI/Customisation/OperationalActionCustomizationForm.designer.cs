namespace Enterprise.Services.OperationalActions.GUI
{
	partial class OperationalActionCustomizationForm
	{
		protected sealed override void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
			this.postingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.customisationControl = new Enterprise.Services.OperationalActions.GUI.CustomisationControl();
			bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 432, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 24, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.OperationalActionManager);
			// 
			// bottomPanel
			// 
			bottomPanel.Controls.Add(this.postingButtons);
			bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 399, true);
			bottomPanel.Name = "bottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 33, true);
			bottomPanel.TabIndex = 0;
			// 
			// postingButtons
			// 
			this.postingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.postingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 6, true);
			this.postingButtons.Name = "postingButtons";
			this.postingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.postingButtons.TabIndex = 0;
			// 
			// customisationControl
			// 
			this.BindingSource.SetBindingMember(this.customisationControl, ".");
			this.customisationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.customisationControl.Name = "customisationControl";
			this.customisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 399, true);
			this.customisationControl.TabIndex = 2;
			// 
			// OperationalActionCustomizationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 456, true);
			this.Controls.Add(this.customisationControl);
			this.Controls.Add(bottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Services.OperationalActions.Business";
			this.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.OperationalActionManager);
			this.DataSourceTypeName = "Enterprise.Services.OperationalActions.Business.OperationalActionManager";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 483, true);
			this.Name = "OperationalActionCustomizationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(bottomPanel, 0);
			this.Controls.SetChildIndex(this.customisationControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		private Enterprise.Core.Forms.ZPostingButtonsUserControl postingButtons;
		private CustomisationControl customisationControl;
	}
}

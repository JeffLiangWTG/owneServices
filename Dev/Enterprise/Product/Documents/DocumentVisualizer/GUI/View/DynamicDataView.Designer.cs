using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	partial class DynamicDataView
	{
		new void InitializeComponent()
		{
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.refreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.copyToClipboardButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.dataView = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.topPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 372, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentVisualizer.Models.DataViewModel);
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.refreshButton);
			this.topPanel.Controls.Add(this.copyToClipboardButton);
			this.topPanel.Controls.Add(this.closeButton);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 53, true);
			this.topPanel.TabIndex = 1;
			// 
			// refreshButton
			// 
			this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.refreshButton.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("c6654257-6cf8-4045-baa1-bb7a6125d0ad", "Refresh");
			this.refreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(397, 17, true);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.refreshButton.TabIndex = 1;
			this.refreshButton.UseVisualStyleBackColor = true;
			// 
			// copyToClipboardButton
			// 
			this.copyToClipboardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.copyToClipboardButton.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("2d69bd1c-f85c-4609-9fb0-639496092281", "Copy");
			this.copyToClipboardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(476, 17, true);
			this.copyToClipboardButton.Name = "copyToClipboardButton";
			this.copyToClipboardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.copyToClipboardButton.TabIndex = 2;
			this.copyToClipboardButton.UseVisualStyleBackColor = true;
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("4e5c1836-6e35-4906-b4ae-6e19c81311f3", "Close");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(555, 17, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.closeButton.TabIndex = 3;
			this.closeButton.UseVisualStyleBackColor = true;
			// 
			// dataView
			// 
			this.BindingSource.SetBindingMember(this.dataView, "Text");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentVisualizer.Models.DataViewModel)(null)).Text)));
			this.dataView.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.dataView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataView.Font = new System.Drawing.Font("Consolas", 9F);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.dataView, false);
			this.dataView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 53, true);
			this.dataView.Multiline = true;
			this.dataView.Name = "dataView";
			this.dataView.ReadOnly = true;
			this.dataView.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.dataView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 319, true);
			this.dataView.TabIndex = 2;
			// 
			// DynamicDataView
			// 
			this.CancelButton = this.closeButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("c13047c2-a329-43eb-ab9b-9cbafaead7f2", "Document Data");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 396, true);
			this.Controls.Add(this.dataView);
			this.Controls.Add(this.topPanel);
			this.DataSourceType = typeof(Enterprise.DocumentVisualizer.Models.DataViewModel);
			this.Name = "DynamicDataView";
			this.RememberFormPosition = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.topPanel, 0);
			this.Controls.SetChildIndex(this.dataView, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZPanel topPanel;
		private ZArchitecture.ZTextBox dataView;
		private ZArchitecture.GUI.ZButton closeButton;
		private ZArchitecture.GUI.ZButton copyToClipboardButton;
		private ZArchitecture.GUI.ZButton refreshButton;
	}
}
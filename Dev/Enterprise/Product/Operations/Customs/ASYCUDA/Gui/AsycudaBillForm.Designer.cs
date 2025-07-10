namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class AsycudaBillForm
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.billsAndPacksTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.billsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.asycudaBillUserControl = new Enterprise.Customs.ASYCUDA.GUI.AsycudaBillUserControl();
			this.billPartiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.asycudaBillPartiesUserControl = new Enterprise.Customs.ASYCUDA.GUI.AsycudaBillPartiesUserControl();
			this.dutiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.asycudaDutiesUserControl = new Enterprise.Customs.ASYCUDA.GUI.AsycudaDutiesUserControl();
			this.BillCustomTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BillCustomFieldsControl = new Enterprise.Customs.ASYCUDA.GUI.AsycudaBillCustomFieldsControl();
			this.logsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.bottomButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.postingButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.billsAndPacksTabControl.SuspendLayout();
			this.billsTabPage.SuspendLayout();
			this.asycudaBillUserControl.SuspendLayout();
			this.billPartiesTabPage.SuspendLayout();
			this.asycudaBillPartiesUserControl.SuspendLayout();
			this.dutiesTabPage.SuspendLayout();
			this.asycudaDutiesUserControl.SuspendLayout();
			this.BillCustomTabPage.SuspendLayout();
			this.BillCustomFieldsControl.SuspendLayout();
			this.bottomButtonPanel.SuspendLayout();
			this.postingButtonsPanel.SuspendLayout();
			this.postingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 397, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1278, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(501);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaBill);
			// 
			// billsAndPacksTabControl
			// 
			this.billsAndPacksTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.billsAndPacksTabControl.Controls.Add(this.billsTabPage);
			this.billsAndPacksTabControl.Controls.Add(this.billPartiesTabPage);
			this.billsAndPacksTabControl.Controls.Add(this.dutiesTabPage);
			this.billsAndPacksTabControl.Controls.Add(this.BillCustomTabPage);
			this.billsAndPacksTabControl.Controls.Add(this.logsTabPage);
			this.billsAndPacksTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.billsAndPacksTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.billsAndPacksTabControl.Name = "billsAndPacksTabControl";
			this.billsAndPacksTabControl.SelectedIndex = 0;
			this.billsAndPacksTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1278, 369, true);
			this.billsAndPacksTabControl.TabIndex = 0;
			// 
			// billsTabPage
			// 
			this.billsTabPage.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("782a9245-feec-49d9-bf39-6df52968f39e", "Bill Details");
			this.billsTabPage.Controls.Add(this.asycudaBillUserControl);
			this.billsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.billsTabPage.Name = "billsTabPage";
			this.billsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.billsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1176, 342, true);
			this.billsTabPage.TabIndex = 0;
			this.billsTabPage.UseVisualStyleBackColor = true;
			// 
			// asycudaBillUserControl
			// 
			this.asycudaBillUserControl.AllowDrop = true;
			this.asycudaBillUserControl.AutoScroll = true;
			this.asycudaBillUserControl.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 0, true);
			this.BindingSource.SetBindingMember(this.asycudaBillUserControl, ".");
			this.asycudaBillUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.asycudaBillUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.asycudaBillUserControl.Name = "asycudaBillUserControl";
			this.asycudaBillUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1170, 336, true);
			this.asycudaBillUserControl.TabIndex = 0;
			// 
			// billPartiesTabPage
			// 
			this.billPartiesTabPage.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("f0ffe886-a23d-4f54-8ecb-f948359f4d69", "Bill Parties");
			this.billPartiesTabPage.Controls.Add(this.asycudaBillPartiesUserControl);
			this.billPartiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.billPartiesTabPage.Name = "billPartiesTabPage";
			this.billPartiesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.billPartiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1176, 342, true);
			this.billPartiesTabPage.TabIndex = 2;
			this.billPartiesTabPage.UseVisualStyleBackColor = true;
			// 
			// asycudaBillPartiesUserControl
			// 
			this.asycudaBillPartiesUserControl.AllowDrop = true;
			this.asycudaBillPartiesUserControl.AutoScroll = true;
			this.asycudaBillPartiesUserControl.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 0, true);
			this.BindingSource.SetBindingMember(this.asycudaBillPartiesUserControl, ".");
			this.asycudaBillPartiesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.asycudaBillPartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.asycudaBillPartiesUserControl.Name = "asycudaBillPartiesUserControl";
			this.asycudaBillPartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1170, 336, true);
			this.asycudaBillPartiesUserControl.TabIndex = 0;
			// 
			// dutiesTabPage
			// 
			this.dutiesTabPage.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("245B1223-AE20-4628-AF1E-B4FE10E105C5", "Duties");
			this.dutiesTabPage.Controls.Add(this.asycudaDutiesUserControl);
			this.dutiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.dutiesTabPage.Name = "dutiesTabPage";
			this.dutiesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.dutiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1176, 342, true);
			this.dutiesTabPage.TabIndex = 2;
			this.dutiesTabPage.UseVisualStyleBackColor = true;
			// 
			// asycudaDutiesUserControl
			// 
			this.asycudaDutiesUserControl.AllowDrop = true;
			this.asycudaDutiesUserControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.asycudaDutiesUserControl, ".");
			this.asycudaDutiesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.asycudaDutiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.asycudaDutiesUserControl.Name = "asycudaDutiesUserControl";
			this.asycudaDutiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1170, 336, true);
			this.asycudaDutiesUserControl.TabIndex = 0;
			// 
			// BillCustomTabPage
			// 
			this.BillCustomTabPage.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("428ff33f-11cc-4b45-a73d-5b0749ca30d1", "Custom");
			this.BillCustomTabPage.Controls.Add(this.BillCustomFieldsControl);
			this.BillCustomTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BillCustomTabPage.Name = "BillCustomTabPage";
			this.BillCustomTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1270, 342, true);
			this.BillCustomTabPage.TabIndex = 3;
			// 
			// BillCustomFieldsControl
			// 
			this.BillCustomFieldsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BillCustomFieldsControl, ".");
			this.BillCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillCustomFieldsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.BillCustomFieldsControl.Name = "BillCustomFieldsControl";
			this.BillCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1270, 342, true);
			this.BillCustomFieldsControl.TabIndex = 1;
			// 
			// logsTabPage
			// 
			this.logsTabPage.ExcludeFromBindingOnSave = true;
			this.logsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.logsTabPage.Name = "logsTabPage";
			this.logsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.logsTabPage.ShouldBeReadOnlyInViewMode = false;
			this.logsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1176, 342, true);
			this.logsTabPage.TabIndex = 4;
			this.logsTabPage.UseVisualStyleBackColor = true;
			// 
			// bottomButtonPanel
			// 
			this.bottomButtonPanel.Controls.Add(this.postingButtonsPanel);
			this.bottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 369, true);
			this.bottomButtonPanel.Name = "bottomButtonPanel";
			this.bottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1278, 28, true);
			this.bottomButtonPanel.TabIndex = 1;
			// 
			// postingButtonsPanel
			// 
			this.postingButtonsPanel.AutoSize = true;
			this.postingButtonsPanel.Controls.Add(this.postingButtonsUserControl);
			this.postingButtonsPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.postingButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1034, 0, true);
			this.postingButtonsPanel.Name = "postingButtonsPanel";
			this.postingButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 28, true);
			this.postingButtonsPanel.TabIndex = 0;
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.RequiresHacks = true;
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.TabIndex = 0;
			// 
			// AsycudaBillForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("90b4276f-bd96-4ff3-85d7-6cd0354a863c", "Manifest Bill");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1278, 421, true);
			this.Controls.Add(this.billsAndPacksTabControl);
			this.Controls.Add(this.bottomButtonPanel);
			this.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaBill);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1274, 460, true);
			this.Name = "AsycudaBillForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomButtonPanel, 0);
			this.Controls.SetChildIndex(this.billsAndPacksTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.billsAndPacksTabControl.ResumeLayout(false);
			this.billsAndPacksTabControl.PerformLayout();
			this.billsTabPage.ResumeLayout(false);
			this.billsTabPage.PerformLayout();
			this.asycudaBillUserControl.ResumeLayout(true);
			this.asycudaBillUserControl.PerformLayout();
			this.billPartiesTabPage.ResumeLayout(false);
			this.billPartiesTabPage.PerformLayout();
			this.asycudaBillPartiesUserControl.ResumeLayout(true);
			this.asycudaBillPartiesUserControl.PerformLayout();
			this.dutiesTabPage.ResumeLayout(false);
			this.dutiesTabPage.PerformLayout();
			this.asycudaDutiesUserControl.ResumeLayout(true);
			this.asycudaDutiesUserControl.PerformLayout();
			this.BillCustomTabPage.ResumeLayout(false);
			this.BillCustomTabPage.PerformLayout();
			this.BillCustomFieldsControl.ResumeLayout(true);
			this.BillCustomFieldsControl.PerformLayout();
			this.bottomButtonPanel.ResumeLayout(false);
			this.bottomButtonPanel.PerformLayout();
			this.postingButtonsPanel.ResumeLayout(false);
			this.postingButtonsPanel.PerformLayout();
			this.postingButtonsUserControl.ResumeLayout(true);
			this.postingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZTabControl billsAndPacksTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage billsTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage billPartiesTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage dutiesTabPage;
		AsycudaBillPartiesUserControl asycudaBillPartiesUserControl;
		Enterprise.ZArchitecture.GUI.ZPanel bottomButtonPanel;
		Enterprise.ZArchitecture.GUI.ZPanel postingButtonsPanel;
		Enterprise.Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
		Enterprise.ZArchitecture.GUI.ZTabPage BillCustomTabPage;
		AsycudaBillCustomFieldsControl BillCustomFieldsControl;
		Enterprise.ZArchitecture.GUI.ZLogsTabPage logsTabPage;
		AsycudaBillUserControl asycudaBillUserControl;
		AsycudaDutiesUserControl asycudaDutiesUserControl;
	}
}

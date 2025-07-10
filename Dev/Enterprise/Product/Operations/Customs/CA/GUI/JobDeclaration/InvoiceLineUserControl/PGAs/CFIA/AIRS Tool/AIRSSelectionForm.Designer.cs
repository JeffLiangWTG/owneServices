
namespace Enterprise.Customs.CA.GUI
{
	partial class AIRSSelectionForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AIRSSelectionForm));
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RadioGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TopGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExtensionCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MiscellaneousTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EndUseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.MainGroupBox.SuspendLayout();
			this.RadioGroupBox.SuspendLayout();
			this.TopGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 907, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.AIRSWebpageNavigator);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.OKButton);
			this.BottomPanel.Controls.Add(this.CloseButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 931, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 27, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("B3CECACC-1546-4385-B44C-29E905E70D48", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(713, 2, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BBB4280F-458D-413D-A11E-3F460C7287DE", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(782, 2, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BAFB692F-BADE-496D-A03A-5B47AE373AD1", "Documentation and Registration Requirements");
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 87, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 20, true);
			this.MainGroupBox.TabIndex = 2;
			this.MainGroupBox.TabStop = false;
			// 
			// RadioGroupBox
			// 
			this.RadioGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 87, true);
			this.RadioGroupBox.Name = "RadioGroupBox";
			this.RadioGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 400, true);
			this.RadioGroupBox.TabIndex = 2;
			this.RadioGroupBox.TabStop = false;
			// 
			// TopGroupBox
			// 
			this.TopGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0C9D00C6-2941-4A44-B2B6-FDE01CB608BD", "CFIA");
			this.TopGroupBox.Controls.Add(this.ExtensionCodeTextBox);
			this.TopGroupBox.Controls.Add(this.MiscellaneousTextBox);
			this.TopGroupBox.Controls.Add(this.EndUseCodeTextBox);
			this.TopGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopGroupBox.Name = "TopGroupBox";
			this.TopGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 87, true);
			this.TopGroupBox.TabIndex = 0;
			this.TopGroupBox.TabStop = false;
			// 
			// ExtensionCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExtensionCodeTextBox, "AG_ExtensionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.AIRSWebpageNavigator)(null)).AG_ExtensionCode)));
			this.ExtensionCodeTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("ExtensionCodeTextBox.CaptionResourceString")));
			this.ExtensionCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 15, true);
			this.ExtensionCodeTextBox.Name = "ExtensionCodeTextBox";
			this.ExtensionCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 18, true);
			this.ExtensionCodeTextBox.TabIndex = 1;
			// 
			// MiscellaneousTextBox
			// 
			this.BindingSource.SetBindingMember(this.MiscellaneousTextBox, "AG_Miscellaneous");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.AIRSWebpageNavigator)(null)).AG_Miscellaneous)));
			this.MiscellaneousTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("MiscellaneousTextBox.CaptionResourceString")));
			this.MiscellaneousTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 39, true);
			this.MiscellaneousTextBox.Name = "MiscellaneousTextBox";
			this.MiscellaneousTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 18, true);
			this.MiscellaneousTextBox.TabIndex = 2;
			// 
			// EndUseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.EndUseCodeTextBox, "AG_EndUseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.AIRSWebpageNavigator)(null)).AG_EndUseCode)));
			this.EndUseCodeTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("EndUseCodeTextBox.CaptionResourceString")));
			this.EndUseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 65, true);
			this.EndUseCodeTextBox.Name = "EndUseCodeTextBox";
			this.EndUseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 18, true);
			this.EndUseCodeTextBox.TabIndex = 3;
			// 
			// AIRSSelectionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(878, 599, true);
			this.Controls.Add(this.TopGroupBox);
			this.Controls.Add(this.MainGroupBox);
			this.Controls.Add(this.RadioGroupBox);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.AIRSWebpageNavigator);
			this.Name = "AIRSSelectionForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainGroupBox, 0);
			this.Controls.SetChildIndex(this.RadioGroupBox, 0);
			this.Controls.SetChildIndex(this.TopGroupBox, 0);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(878, 696, true);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.RadioGroupBox.ResumeLayout(false);
			this.RadioGroupBox.PerformLayout();
			this.TopGroupBox.ResumeLayout(false);
			this.TopGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox TopGroupBox;
		private ZArchitecture.ZTextBox EndUseCodeTextBox;
		private ZArchitecture.ZTextBox ExtensionCodeTextBox;
		private ZArchitecture.ZTextBox MiscellaneousTextBox;
		internal ZArchitecture.GUI.ZButton OKButton;
		internal ZArchitecture.GUI.ZButton CloseButton;
		internal ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
		private ZArchitecture.GUI.ZGroupBox RadioGroupBox;
	}
}

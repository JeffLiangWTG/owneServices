namespace Enterprise.DocumentEngine.GUI
{
	partial class PrintQueueReplaceForm
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
		protected new void InitializeComponent()
		{
			this.OtherPrintQueueFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.label1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OtherPrintQueueFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 114, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.PrintQueueReplaceBizo);
			// 
			// OtherPrintQueueFindBox
			// 
			this.OtherPrintQueueFindBox.AllowDrop = true;
			this.OtherPrintQueueFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OtherPrintQueueFindBox, "ReplacePrintQueuePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngine.Scheduler.Business.PrintQueueReplaceBizo)(null)).ReplacePrintQueuePK)));
			this.OtherPrintQueueFindBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("E86603F8-ACE7-4259-9737-C4F3868A104D", "Other Print Queue");
			this.OtherPrintQueueFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 62, true);
			this.OtherPrintQueueFindBox.Name = "OtherPrintQueueFindBox";
			this.OtherPrintQueueFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 18, true);
			this.OtherPrintQueueFindBox.TabIndex = 1;
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("21610F8A-47D7-48F0-94FF-9A3724BF1023", "OK");
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 85, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OkButton.TabIndex = 2;
			this.OkButton.Click += OkButton_Click;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("A2844FB7-5021-4820-ADC7-B73D0202BA50", "Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(335, 85, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Cancel_Button.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.label1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("1a7f99d1-f70f-4b6a-98f3-30d5fd08ad8f", "This text will be replaced inside the constructor but we want to see how its formatted on the form");
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 7, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(404, 37, true);
			this.label1.TabIndex = 4;
			// 
			// PrintQueueReplaceForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("9D244767-48FB-4856-8B76-E16C2314A54F", "Replace with another print queue");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 138, true);
			this.Controls.Add(this.OtherPrintQueueFindBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.OkButton);
			this.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.PrintQueueReplaceBizo);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "PrintQueueReplaceForm";
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.OtherPrintQueueFindBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OtherPrintQueueFindBox.ResumeLayout(true);
			this.OtherPrintQueueFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGuidFindBox OtherPrintQueueFindBox;
		private Enterprise.ZArchitecture.ZLabel label1;
		internal Enterprise.ZArchitecture.GUI.ZButton OkButton;
		internal Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
	}
}

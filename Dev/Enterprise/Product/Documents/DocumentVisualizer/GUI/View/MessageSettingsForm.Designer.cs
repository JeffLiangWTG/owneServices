namespace Enterprise.DocumentVisualizer.GUI
{
	partial class MessageSettingsForm
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
		public new void InitializeComponent()
		{
			this.purposeCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.sendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.recipientTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.recipientTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.purposeCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.purposeCodeDropEdit.SuspendLayout();
			this.recipientTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 107, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentVisualizer.Models.MessageSettingsExport);
			// 
			// purposeCodeDropEdit
			// 
			this.purposeCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.purposeCodeDropEdit, "PurposeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentVisualizer.Models.MessageSettingsExport)(null)).PurposeCode)));
			this.purposeCodeDropEdit.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("MessageSettingsForm|35257b3c-06ff-4743-ae34-83368118e643", "Purpose Code");
			this.purposeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 46, true);
			this.purposeCodeDropEdit.Name = "purposeCodeDropEdit";
			this.purposeCodeDropEdit.PreBoundMaxLength = 2;
			this.purposeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 17, true);
			this.purposeCodeDropEdit.TabIndex = 3;
			// 
			// sendButton
			// 
			this.sendButton.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("MessageSettingsForm|dfa33364-9f7c-4ef8-9165-b4e8191fcac6", "Send && Close");
			this.sendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 74, true);
			this.sendButton.Name = "sendButton";
			this.sendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.sendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 23, true);
			this.sendButton.TabIndex = 4;
			this.sendButton.UseVisualStyleBackColor = true;
			this.sendButton.Click += new System.EventHandler(this.OnSendButtonClick);
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("MessageSettingsForm|5b27d113-4070-4053-85ba-fc0e7dd1893d", "Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 74, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 23, true);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.OnCancelButtonClick);
			// 
			// recipientTypeDropEdit
			// 
			this.recipientTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.recipientTypeDropEdit, "RecipientType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentVisualizer.Models.MessageSettingsExport)(null)).RecipientType)));
			this.recipientTypeDropEdit.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("MessageSettingsForm|0d4d654b-9027-4bc6-ab5c-bb76db951710", "Recipient Type");
			this.recipientTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 19, true);
			this.recipientTypeDropEdit.Name = "recipientTypeDropEdit";
			this.recipientTypeDropEdit.PreBoundMaxLength = 2;
			this.recipientTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 17, true);
			this.recipientTypeDropEdit.TabIndex = 1;
			// 
			// recipientTypeLabel
			// 
			this.recipientTypeLabel.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("MessageSettingsForm|bbe20e9d-546f-466f-b932-f6b8c4f8fcd2", "Recipient Type", "Label for Recipient Type");
			this.recipientTypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.recipientTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 19, true);
			this.recipientTypeLabel.Name = "recipientTypeLabel";
			this.recipientTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 17, true);
			this.recipientTypeLabel.TabIndex = 6;
			this.recipientTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// purposeCodeLabel
			// 
			this.purposeCodeLabel.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("MessageSettingsForm|af4afb3d-cc34-4650-a9c7-78741fd7452b", "Purpose Code", "Label for Purpose Code");
			this.purposeCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.purposeCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 47, true);
			this.purposeCodeLabel.Name = "purposeCodeLabel";
			this.purposeCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 13, true);
			this.purposeCodeLabel.TabIndex = 7;
			this.purposeCodeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// MessageSettingsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentVisualizer.GUI.Res.GetData("MessageSettingsForm|f7aac34d-e365-4b9f-bf18-28c1158963ea", "Message Settings Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 131, true);
			this.Controls.Add(this.recipientTypeLabel);
			this.Controls.Add(this.purposeCodeLabel);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.sendButton);
			this.Controls.Add(this.purposeCodeDropEdit);
			this.Controls.Add(this.recipientTypeDropEdit);
			this.DataSourceType = typeof(Enterprise.DocumentVisualizer.Models.MessageSettingsExport);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "MessageSettingsForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Controls.SetChildIndex(this.recipientTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.purposeCodeDropEdit, 0);
			this.Controls.SetChildIndex(this.sendButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.purposeCodeLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.recipientTypeLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.purposeCodeDropEdit.ResumeLayout(true);
			this.purposeCodeDropEdit.PerformLayout();
			this.recipientTypeDropEdit.ResumeLayout(true);
			this.recipientTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZDropEdit purposeCodeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit recipientTypeDropEdit;
		internal ZArchitecture.ZLabel recipientTypeLabel;
		internal ZArchitecture.ZLabel purposeCodeLabel;
		internal ZArchitecture.GUI.ZButton sendButton;
		internal ZArchitecture.GUI.ZButton cancelButton;
	}
}
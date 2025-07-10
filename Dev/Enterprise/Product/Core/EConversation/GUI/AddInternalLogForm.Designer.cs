namespace Enterprise.EConversation.GUI
{
	partial class AddInternalLogForm
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		private new void InitializeComponent()
		{
			this.messageTextbox = new Enterprise.ZArchitecture.GUI.ZAutoCompleteTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 326, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 24, true);
			// 
			// messageTextbox
			// 
			this.messageTextbox.AcceptsTab = true;
			this.messageTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.messageTextbox.AutocompleteManager = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.messageTextbox, false);
			this.messageTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 25, true);
			this.messageTextbox.Multiline = true;
			this.messageTextbox.Name = "messageTextbox";
			this.messageTextbox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.messageTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(714, 266, true);
			this.messageTextbox.TabIndex = 2;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("68b0b907-7378-4efc-902a-f13f4a238513", "Internal Comment for this eConversation");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel1.TabIndex = 3;
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("25fc33a8-4cc4-4105-96b8-8e42c2bdfb3c", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(651, 297, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("80fa5cf3-4ea1-44bc-8aa2-a53abf2b1b36", "OK");
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Enabled = false;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(570, 297, true);
			this.okButton.Name = "okButton";
			this.okButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 5;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// AddInternalLogForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("bd89a3e1-ee43-408b-8072-dc8703d9075b", "Add Internal Message");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 350, true);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.messageTextbox);
			this.Name = "AddInternalLogForm";
			this.Controls.SetChildIndex(this.messageTextbox, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZAutoCompleteTextBox messageTextbox;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZButton okButton;
	}
}
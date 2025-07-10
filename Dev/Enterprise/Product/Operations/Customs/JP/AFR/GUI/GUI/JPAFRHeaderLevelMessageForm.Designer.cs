namespace Enterprise.Customs.JP.AFR.GUI
{
	partial class JPAFRHeaderLevelMessageForm
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
		new void InitializeComponent()
		{
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.HasATDBeenSentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 148, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.MessageSendingAction);
			// 
			// MessageLabel
			// 
			this.MessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MessageLabel.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("4b872c95-99fd-447e-8fa4-05bea55d11ff", "In order to mark the Master Bill as registration completed with all the House Bill. Japan Customs required different message action depends on if the Carrier has sent the ATD message. Please contact the carrier to find out the ATD submission status and use the check box below accordingly.");
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 78, true);
			this.MessageLabel.TabIndex = 1;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("b96d850f-4280-42d6-bea0-e1bf9dcf826a", "&Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 112, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 3;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.sendButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("b6d9fe04-a21e-43b2-a253-65402a48e379", "&Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 112, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// HasATDBeenSentCheckBox
			// 
			this.HasATDBeenSentCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.HasATDBeenSentCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HasATDBeenSentCheckBox, "HasATDBeenSent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.JP.AFR.Business.MessageSendingAction)(null)).HasATDBeenSent)));
			this.HasATDBeenSentCheckBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("c6fc73c2-4cca-42ee-82f5-2dc4c7a32bf4", "Has Carrier sent ATD message?");
			this.HasATDBeenSentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HasATDBeenSentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 90, true);
			this.HasATDBeenSentCheckBox.Name = "HasATDBeenSentCheckBox";
			this.HasATDBeenSentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 17, true);
			this.HasATDBeenSentCheckBox.TabIndex = 2;
			this.HasATDBeenSentCheckBox.UseVisualStyleBackColor = true;
			// 
			// JPAFRHeaderLevelMessageForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 172, true);
			this.Controls.Add(this.HasATDBeenSentCheckBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.MessageLabel);
			this.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.MessageSendingAction);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "JPAFRHeaderLevelMessageForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.HasATDBeenSentCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel MessageLabel;
		private ZArchitecture.GUI.ZButton SendButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZCheckBox HasATDBeenSentCheckBox;
	}
}

namespace Enterprise.Customs.IN.Manifest.Module;

partial class MessageSendingOperationalActionControl
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
	void InitializeComponent()
	{
		this.MessageSendingOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.AllowSendWithMessageErrorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.MessageSendingOptionsGroupBox.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Manifest.Module.MessageSendingOperationalActionMethodApplicator);
		// 
		// MessageSendingOptionsGroupBox
		// 
		this.MessageSendingOptionsGroupBox.CaptionResourceString = Enterprise.Customs.IN.Manifest.Module.Res.GetData("DB87CCAD-FD69-443B-A924-D545F108D893", "Message Sending Options");
		this.MessageSendingOptionsGroupBox.Controls.Add(this.AllowSendWithMessageErrorCheckBox);
		this.MessageSendingOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
		this.MessageSendingOptionsGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.MessageSendingOptionsGroupBox.Name = "MessageSendingOptionsGroupBox";
		this.MessageSendingOptionsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.MessageSendingOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 193, true);
		this.MessageSendingOptionsGroupBox.TabIndex = 0;
		this.MessageSendingOptionsGroupBox.TabStop = false;
		//
		// AllowSendWithMessageErrorCheckBox
		//
		this.AllowSendWithMessageErrorCheckBox.AutoSize = true;
		this.BindingSource.SetBindingMember(this.AllowSendWithMessageErrorCheckBox, "AllowSendWithMessageError");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IN.Manifest.Module.MessageSendingOperationalActionMethodApplicator)(null)).AllowSendWithMessageError)));
		this.AllowSendWithMessageErrorCheckBox.CaptionResourceString = Enterprise.Customs.IN.Manifest.Module.Res.GetData("B3C2141D-16E0-4AB5-8EED-2E58A6FE5ED9", "Continue to send even though the selected message(s) contains validation errors?");
		this.AllowSendWithMessageErrorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.AllowSendWithMessageErrorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 31, true);
		this.AllowSendWithMessageErrorCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.AllowSendWithMessageErrorCheckBox.Name = "AllowSendWithMessageErrorCheckBox";
		this.AllowSendWithMessageErrorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
		this.AllowSendWithMessageErrorCheckBox.TabIndex = 3;
		this.AllowSendWithMessageErrorCheckBox.UseVisualStyleBackColor = true;
		// 
		// MessageSendingOperationalActionControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.Controls.Add(this.MessageSendingOptionsGroupBox);
		this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
		this.Name = "MessageSendingOperationalActionControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 206, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.MessageSendingOptionsGroupBox.ResumeLayout(false);
		this.MessageSendingOptionsGroupBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	private ZArchitecture.GUI.ZGroupBox MessageSendingOptionsGroupBox;
	private ZArchitecture.GUI.ZCheckBox AllowSendWithMessageErrorCheckBox;
}

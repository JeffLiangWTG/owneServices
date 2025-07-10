namespace Enterprise.ZArchitecture.GUI
{
	partial class UnitConversionForm
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
		new void InitializeComponent()
		{
			this.fromDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.toDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.fromGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.toGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.fromDropEdit.SuspendLayout();
			this.toDropEdit.SuspendLayout();
			this.fromGroupBox.SuspendLayout();
			this.toGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 124, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.UnitConversion);
			// 
			// fromDropEdit
			// 
			this.fromDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.fromDropEdit, "FromUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ZArchitecture.Business.UnitConversion)(null)).FromUnit)));
			this.fromDropEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("91ed0cab-0447-461d-9d1b-132136cf9701", "Unit");
			this.fromDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 20, true);
			this.fromDropEdit.Name = "fromDropEdit";
			this.fromDropEdit.PreBoundMaxLength = 2;
			this.fromDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.fromDropEdit.TabIndex = 1;
			// 
			// toDropEdit
			// 
			this.toDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.toDropEdit, "ToUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ZArchitecture.Business.UnitConversion)(null)).ToUnit)));
			this.toDropEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("bcbe7ac7-90cf-4a47-af00-5a094ca9695c", "Unit");
			this.toDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 19, true);
			this.toDropEdit.Name = "toDropEdit";
			this.toDropEdit.PreBoundMaxLength = 2;
			this.toDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.toDropEdit.TabIndex = 4;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("cbbd6aee-d880-4c4e-b664-986885046e10", "OK");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 118, true);
			this.okButton.Name = "okButton";
			this.okButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 7;
			this.okButton.ToolTipCaption = null;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("5e6a7a87-0840-44c0-aa8b-106998fe3e7d", "Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 118, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// fromGroupBox
			// 
			this.fromGroupBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("3003655a-8f6e-47ad-ad76-ffc8a64b405a", "From");
			this.fromGroupBox.Controls.Add(this.fromDropEdit);
			this.fromGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.fromGroupBox.Name = "fromGroupBox";
			this.fromGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 46, true);
			this.fromGroupBox.TabIndex = 9;
			this.fromGroupBox.TabStop = false;
			// 
			// toGroupBox
			// 
			this.toGroupBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("081c190c-90b1-4c0d-afe2-286077d51286", "To");
			this.toGroupBox.Controls.Add(this.toDropEdit);
			this.toGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 64, true);
			this.toGroupBox.Name = "toGroupBox";
			this.toGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 46, true);
			this.toGroupBox.TabIndex = 10;
			this.toGroupBox.TabStop = false;
			// 
			// UnitConversionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("954fc82e-0793-4198-81f7-49b0e0c543b4", "Unit Conversion");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 148, true);
			this.Controls.Add(this.toGroupBox);
			this.Controls.Add(this.fromGroupBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Business.UnitConversion);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "UnitConversionForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.fromGroupBox, 0);
			this.Controls.SetChildIndex(this.toGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.fromDropEdit.ResumeLayout(true);
			this.fromDropEdit.PerformLayout();
			this.toDropEdit.ResumeLayout(true);
			this.toDropEdit.PerformLayout();
			this.fromGroupBox.ResumeLayout(false);
			this.fromGroupBox.PerformLayout();
			this.toGroupBox.ResumeLayout(false);
			this.toGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZDropEdit fromDropEdit;
		private ZDropEdit toDropEdit;
		private ZButton okButton;
		private ZButton cancelButton;
		private ZGroupBox fromGroupBox;
		private ZGroupBox toGroupBox;
	}
}
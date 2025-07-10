namespace Enterprise.Customs.GB.GUI.Organisation
{
	partial class HmrcQueryForm
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
			this.EORITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VATTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VerifyEORICheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.VerifyVATCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CancelQueryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.QueryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 93, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 24, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.Organisation.NonPersistentOrgHeaderChecker);
			// 
			// EORITextBox
			// 
			this.BindingSource.SetBindingMember(this.EORITextBox, "EORI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Organisation.NonPersistentOrgHeaderChecker)(null)).EORI)));
			this.EORITextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("506c83f4-d03a-4d10-8ee0-564a1bc752c6", "EORI");
			this.EORITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 12, true);
			this.EORITextBox.Name = "EORITextBox";
			this.EORITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.EORITextBox.TabIndex = 0;
			// 
			// VATTextBox
			// 
			this.BindingSource.SetBindingMember(this.VATTextBox, "VAT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Organisation.NonPersistentOrgHeaderChecker)(null)).VAT)));
			this.VATTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("bc51e538-c5bd-4313-ba9d-b2357e76493c", "VAT Number");
			this.VATTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 38, true);
			this.VATTextBox.Name = "VATTextBox";
			this.VATTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.VATTextBox.TabIndex = 2;
			// 
			// VerifyEORICheckBox
			// 
			this.VerifyEORICheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.VerifyEORICheckBox, "VerifyEORI");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.Organisation.NonPersistentOrgHeaderChecker)(null)).VerifyEORI)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.VerifyEORICheckBox, false);
			this.VerifyEORICheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 15, true);
			this.VerifyEORICheckBox.Name = "VerifyEORICheckBox";
			this.VerifyEORICheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.VerifyEORICheckBox.TabIndex = 1;
			this.VerifyEORICheckBox.UseVisualStyleBackColor = true;
			// 
			// VerifyVATCheckBox
			// 
			this.VerifyVATCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.VerifyVATCheckBox, "VerifyVAT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.Organisation.NonPersistentOrgHeaderChecker)(null)).VerifyVAT)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.VerifyVATCheckBox, false);
			this.VerifyVATCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 41, true);
			this.VerifyVATCheckBox.Name = "VerifyVATCheckBox";
			this.VerifyVATCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.VerifyVATCheckBox.TabIndex = 3;
			this.VerifyVATCheckBox.UseVisualStyleBackColor = true;
			// 
			// CancelQueryButton
			// 
			this.CancelQueryButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("15cba023-abb9-40bd-b481-d4dd9a1e0cb9", "Cancel");
			this.CancelQueryButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelQueryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 64, true);
			this.CancelQueryButton.Name = "CancelQueryButton";
			this.CancelQueryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.CancelQueryButton.TabIndex = 5;
			this.CancelQueryButton.ToolTipCaption = null;
			this.CancelQueryButton.UseVisualStyleBackColor = true;
			// 
			// QueryButton
			// 
			this.QueryButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("19db991f-255c-4586-a47b-26a67b7b5c22", "Query HMRC");
			this.QueryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 64, true);
			this.QueryButton.Name = "QueryButton";
			this.QueryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.QueryButton.TabIndex = 4;
			this.QueryButton.ToolTipCaption = null;
			this.QueryButton.UseVisualStyleBackColor = true;
			this.QueryButton.Click += new System.EventHandler(this.QueryButton_Click);
			// 
			// HmrcQueryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelQueryButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 117, true);
			this.Controls.Add(this.CancelQueryButton);
			this.Controls.Add(this.QueryButton);
			this.Controls.Add(this.VerifyVATCheckBox);
			this.Controls.Add(this.VerifyEORICheckBox);
			this.Controls.Add(this.VATTextBox);
			this.Controls.Add(this.EORITextBox);
			this.DataSourceType = typeof(Enterprise.Customs.GB.Business.Organisation.NonPersistentOrgHeaderChecker);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 156, true);
			this.Name = "HmrcQueryForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.EORITextBox, 0);
			this.Controls.SetChildIndex(this.VATTextBox, 0);
			this.Controls.SetChildIndex(this.VerifyEORICheckBox, 0);
			this.Controls.SetChildIndex(this.VerifyVATCheckBox, 0);
			this.Controls.SetChildIndex(this.QueryButton, 0);
			this.Controls.SetChildIndex(this.CancelQueryButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox EORITextBox;
		private ZArchitecture.ZTextBox VATTextBox;
		internal ZArchitecture.GUI.ZCheckBox VerifyEORICheckBox;
		internal ZArchitecture.GUI.ZCheckBox VerifyVATCheckBox;
		internal ZArchitecture.GUI.ZButton CancelQueryButton;
		internal ZArchitecture.GUI.ZButton QueryButton;
	}
}

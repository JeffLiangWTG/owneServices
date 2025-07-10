using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class AdditionalProcedureCodesUserControl
	{
		private void InitializeComponent()
		{
			this.AdditionalProcedureCodesAsStringTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalProcedureCodesEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine);
			// 
			// AdditionalProcedureCodesAsStringTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalProcedureCodesAsStringTextBox, "AdditionalProcedureCodesAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(null)).AdditionalProcedureCodesAsString)));
			this.AdditionalProcedureCodesAsStringTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalProcedureCodesAsStringTextBox, false);
			this.AdditionalProcedureCodesAsStringTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalProcedureCodesAsStringTextBox.Name = "AdditionalProcedureCodesAsStringTextBox";
			this.AdditionalProcedureCodesAsStringTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.AdditionalProcedureCodesAsStringTextBox.TabIndex = 0;
			this.AdditionalProcedureCodesAsStringTextBox.TabStop = false;
			// 
			// AdditionalProcedureCodesEditButton
			// 
			this.AdditionalProcedureCodesEditButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("6e95626c-a2d7-410f-a54e-13c4b2b19a71", "More..");
			this.AdditionalProcedureCodesEditButton.IsCaptionOverridden = false;
			this.AdditionalProcedureCodesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 0, true);
			this.AdditionalProcedureCodesEditButton.Name = "AdditionalProcedureCodesEditButton";
			this.AdditionalProcedureCodesEditButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AdditionalProcedureCodesEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.AdditionalProcedureCodesEditButton.TabIndex = 1;
			this.AdditionalProcedureCodesEditButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AdditionalProcedureCodesEditButton.ToolTipCaption = null;
			this.AdditionalProcedureCodesEditButton.Click += new System.EventHandler(this.AdditionalProcedureCodesEditButton_Click);
			// 
			// AdditionalProcedureCodesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalProcedureCodesAsStringTextBox);
			this.Controls.Add(this.AdditionalProcedureCodesEditButton);
			this.Name = "AdditionalProcedureCodesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.ZTextBox AdditionalProcedureCodesAsStringTextBox;
		private ZButton AdditionalProcedureCodesEditButton;
	}
}

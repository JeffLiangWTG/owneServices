namespace Enterprise.Customs.IT.GUI
{
	partial class VatTypeAndDescriptionUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.VatRateDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VatTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VatTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine);
			// 
			// VatRateDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.VatRateDescriptionTextBox, "VatRateDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine)(null)).VatRateDescription)));
			this.VatRateDescriptionTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.VatRateDescriptionTextBox, false);
			this.VatRateDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 0, true);
			this.VatRateDescriptionTextBox.Name = "VatRateDescriptionTextBox";
			this.VatRateDescriptionTextBox.ReadOnly = true;
			this.VatRateDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.VatRateDescriptionTextBox.TabIndex = 29;
			this.VatRateDescriptionTextBox.TabStop = false;
			// 
			// VatTypeDropEdit
			// 
			this.VatTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VatTypeDropEdit, "JI_ZZF_NKTaxType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobComInvoiceLine)(null)).JI_ZZF_NKTaxType)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.VatTypeDropEdit, false);
			this.VatTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VatTypeDropEdit.Name = "VatTypeDropEdit";
			this.VatTypeDropEdit.PreBoundMaxLength = 4;
			this.VatTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.VatTypeDropEdit.TabIndex = 28;
			// 
			// VatTypeAndDescriptionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VatTypeDropEdit);
			this.Controls.Add(this.VatRateDescriptionTextBox);
			this.Name = "VatTypeAndDescriptionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 21, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VatTypeDropEdit.ResumeLayout(true);
			this.VatTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox VatRateDescriptionTextBox;
		internal ZArchitecture.GUI.ZDropEdit VatTypeDropEdit;
	}
}

namespace Enterprise.Customs.IL.GUI
{
	partial class InvoiceLineDetailsUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.InvoiceNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PreferenceDocNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceNumberDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// InvoiceNumberDropEdit
			// 
			this.InvoiceNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceNumberDropEdit, "JI_Calc_Invoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.JobComInvoiceLine)(null)).JI_Calc_Invoice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobComInvoiceLine)(null)).Lookups.SortedInvoiceList)));
			this.InvoiceNumberDropEdit.BindToList = "Lookups.SortedInvoiceList";
			this.InvoiceNumberDropEdit.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("C4FDA296-E943-470D-BF29-1E134F5E84B7", "Invoice Number");
			this.InvoiceNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 23, true);
			this.InvoiceNumberDropEdit.Name = "InvoiceNumberDropEdit";
			this.InvoiceNumberDropEdit.PreBoundMaxLength = 35;
			this.InvoiceNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			this.InvoiceNumberDropEdit.TabIndex = 4;
			// 
			// PreferenceDocNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreferenceDocNumberTextBox, "JI_PreferenceDocNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.JobComInvoiceLine)(null)).JI_PreferenceDocNumber)));
			this.PreferenceDocNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 68, true);
			this.PreferenceDocNumberTextBox.Name = "PreferenceDocNumberTextBox";
			this.PreferenceDocNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.PreferenceDocNumberTextBox.TabIndex = 2;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PreferenceDocNumberTextBox);
			this.Controls.Add(this.InvoiceNumberDropEdit);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 520, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceNumberDropEdit.ResumeLayout(true);
			this.InvoiceNumberDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZDropEdit InvoiceNumberDropEdit;
		internal ZArchitecture.ZTextBox PreferenceDocNumberTextBox;
	}
}

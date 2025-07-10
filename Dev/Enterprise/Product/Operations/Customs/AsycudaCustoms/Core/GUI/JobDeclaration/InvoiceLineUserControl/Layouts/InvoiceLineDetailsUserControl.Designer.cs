namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	partial class InvoiceLineDetailsUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.PreviousEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreviousEntryLineNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AsycudaCustoms.Business.JobComInvoiceLine);
			// 
			// PreviousEntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousEntryNumberTextBox, "JI_PreviousEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.JobComInvoiceLine)(null)).JI_PreviousEntryNumber)));
			this.PreviousEntryNumberTextBox.CaptionResourceString = null;
			this.PreviousEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 226, true);
			this.PreviousEntryNumberTextBox.Name = "PreviousEntryNumberTextBox";
			this.PreviousEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.PreviousEntryNumberTextBox.TabIndex = 0;
			// 
			// PreviousEntryLineNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PreviousEntryLineNumberCalcEdit, "JI_PreviousEntryLineNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AsycudaCustoms.Business.JobComInvoiceLine)(null)).JI_PreviousEntryLineNumber)));
			this.PreviousEntryLineNumberCalcEdit.CaptionResourceString = null;
			this.PreviousEntryLineNumberCalcEdit.DecimalPlaces = 2;
			this.PreviousEntryLineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 226, true);
			this.PreviousEntryLineNumberCalcEdit.Name = "PreviousEntryLineNumberCalcEdit";
			this.PreviousEntryLineNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.PreviousEntryLineNumberCalcEdit.TabIndex = 1;
			this.PreviousEntryLineNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.Controls.Add(this.PreviousEntryNumberTextBox);
			this.Controls.Add(this.PreviousEntryLineNumberCalcEdit);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 302, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.ZTextBox PreviousEntryNumberTextBox;
		internal Enterprise.ZArchitecture.ZCalcEdit PreviousEntryLineNumberCalcEdit;
	}
}

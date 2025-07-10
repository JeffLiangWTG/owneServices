using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI
{
	sealed partial class InvoiceAmountUserControl
	{
		void InitializeComponent()
		{
			this.InvoiceAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InvoiceCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// InvoiceAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InvoiceAmountCalcEdit, "CustomsEntryHeaders.InvoiceAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).InvoiceAmount)));
			this.InvoiceAmountCalcEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InvoiceAmountCalcEdit, false);
			this.InvoiceAmountCalcEdit.DecimalPlaces = 2;
			this.InvoiceAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceAmountCalcEdit.Name = "InvoiceAmountCalcEdit";
			this.InvoiceAmountCalcEdit.ReadOnly = true;
			this.InvoiceAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.InvoiceAmountCalcEdit.TabIndex = 7;
			this.InvoiceAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// InvoiceCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceCurrencyTextBox, "CustomsEntryHeaders.InvoiceAmountCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).InvoiceAmountCurrency)));
			this.InvoiceCurrencyTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InvoiceCurrencyTextBox, false);
			this.InvoiceCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 0, true);
			this.InvoiceCurrencyTextBox.Name = "InvoiceCurrencyTextBox";
			this.InvoiceCurrencyTextBox.ReadOnly = true;
			this.InvoiceCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.InvoiceCurrencyTextBox.TabIndex = 8;
			// 
			// InvoiceAmountUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoiceCurrencyTextBox);
			this.Controls.Add(this.InvoiceAmountCalcEdit);
			this.Name = "InvoiceAmountUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZCalcEdit InvoiceAmountCalcEdit;
		internal ZTextBox InvoiceCurrencyTextBox;
	}
}

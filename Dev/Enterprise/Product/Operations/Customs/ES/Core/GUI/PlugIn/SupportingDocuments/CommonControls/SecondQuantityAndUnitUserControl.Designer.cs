namespace Enterprise.Customs.ES.GUI
{
	public partial class SecondQuantityAndUnitUserControl
	{
		private void InitializeComponent()
		{
			this.Quantity2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnitOfQuantity2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
			// 
			// Quantity2CalcEdit
			// 
			this.Quantity2CalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.Quantity2CalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Quantity2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Quantity2)));
			this.Quantity2CalcEdit.DecimalPlaces = 5;
			this.Quantity2CalcEdit.Decimals = 5;
			this.Quantity2CalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
			this.Quantity2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Quantity2CalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Quantity2CalcEdit.Name = "Quantity2CalcEdit";
			this.Quantity2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.Quantity2CalcEdit.TabIndex = 0;
			this.Quantity2CalcEdit.Text = "0.00000";
			this.Quantity2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnitOfQuantity2TextBox
			// 
			this.UnitOfQuantity2TextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.UnitOfQuantity2TextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_UnitOfQuantity2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity2)));
			this.UnitOfQuantity2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UnitOfQuantity2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 0, true);
			this.UnitOfQuantity2TextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.UnitOfQuantity2TextBox.Name = "UnitOfQuantity2TextBox";
			this.UnitOfQuantity2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 20, true);
			this.UnitOfQuantity2TextBox.TabIndex = 1;
			// 
			// SecondQuantityAndUnitUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnitOfQuantity2TextBox);
			this.Controls.Add(this.Quantity2CalcEdit);
			this.Name = "SecondQuantityAndUnitUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.ZCalcEdit Quantity2CalcEdit;
		internal ZArchitecture.ZTextBox UnitOfQuantity2TextBox;
	}
}

using Enterprise.Customs.FR.GUI.PlugIn;

namespace Enterprise.Customs.FR.GUI
{
	partial class ExportInvoiceLineUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PreviousEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreviousEntryLineNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.BondedWhsQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.LineDetailTabControl.SuspendLayout();
			this.BondedWhsQuantityCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// ClassificationDetailsGroupBox
			// 
			this.ClassificationDetailsGroupBox.Controls.Add(this.BondedWhsQuantityCalcDropEdit);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.BondedWhsQuantityCalcDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.Add(this.PreviousEntryLineNumberCalcEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.PreviousEntryNumberTextBox);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.PreviousEntryNumberTextBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.PreviousEntryLineNumberCalcEdit, 0);
			// 
			// PreviousEntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousEntryNumberTextBox, "FilteredInvoiceLines.JI_PreviousEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PreviousEntryNumber)));
			this.PreviousEntryNumberTextBox.CaptionResourceString = null;
			this.PreviousEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 42, true);
			this.PreviousEntryNumberTextBox.Name = "PreviousEntryNumberTextBox";
			this.PreviousEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PreviousEntryNumberTextBox.TabIndex = 6;
			// 
			// PreviousEntryLineNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PreviousEntryLineNumberCalcEdit, "FilteredInvoiceLines.JI_PreviousEntryLineNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PreviousEntryLineNumber)));
			this.PreviousEntryLineNumberCalcEdit.CaptionResourceString = null;
			this.PreviousEntryLineNumberCalcEdit.DecimalPlaces = 0;
			this.PreviousEntryLineNumberCalcEdit.Decimals = 0;
			this.PreviousEntryLineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 42, true);
			this.PreviousEntryLineNumberCalcEdit.Name = "PreviousEntryLineNumberCalcEdit";
			this.PreviousEntryLineNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.PreviousEntryLineNumberCalcEdit.TabIndex = 7;
			this.PreviousEntryLineNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JI_CustomsQuantityCalcDropEdit
			// 
			this.JI_CustomsQuantityCalcDropEdit.TabIndex = 8;
			// 
			// GDMLink
			// 
			this.GDMLink.TabIndex = 9;
			// 
			// BondedWhsQuantityCalcDropEdit
			// 
			this.BondedWhsQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondedWhsQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_BondedWhsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_BondedWhsUnitQty)));
			this.BondedWhsQuantityCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_BondedWhsQuantity";
			this.BondedWhsQuantityCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_BondedWhsUnitQty";
			this.BondedWhsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 120, true);
			this.BondedWhsQuantityCalcDropEdit.Name = "BondedWhsQuantityCalcDropEdit";
			this.BondedWhsQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.BondedWhsQuantityCalcDropEdit.TabIndex = 13;
			// 
			// ExportInvoiceLineUserControl
			// 
			this.LineDetailTabControl.ResumeLayout(false);
			this.LineDetailTabControl.PerformLayout();
			this.BondedWhsQuantityCalcDropEdit.ResumeLayout(true);
			this.BondedWhsQuantityCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		public Enterprise.ZArchitecture.ZTextBox PreviousEntryNumberTextBox;
		public Enterprise.ZArchitecture.ZCalcEdit PreviousEntryLineNumberCalcEdit;
		public ZArchitecture.GUI.ZCalcDropEdit BondedWhsQuantityCalcDropEdit;
	}
}

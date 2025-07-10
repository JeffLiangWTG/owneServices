namespace Enterprise.Customs.BE.GUI.PlugIn
{
	partial class InvoiceLinePreviousDocumentsUserControlUCC6
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
		private void InitializeComponent()
		{
			this.PackQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TypeOfPackagesDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TypeOfPackagesDropEdit.SuspendLayout();

			// 
			// PrevDocsGroupBox
			// 
			this.PrevDocsGroupBox.Controls.Add(this.PackQuantityCalcEdit);
			this.PrevDocsGroupBox.Controls.Add(this.TypeOfPackagesDropEdit);
			this.PrevDocsGroupBox.CaptionResourceString = Res.GetData("8CE67F03-1B20-4005-AD4C-4F1CC8A45DE8", "[UCC 2/1] Previous documents");

			// 
			// PackQuantityCalcEdit
			// 
			this.PackQuantityCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PackQuantityCalcEdit, "FilteredInvoiceLines.PreviousDocuments.CSI_PackQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_PackQty)));
			this.PackQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 97, true);
			this.PackQuantityCalcEdit.Name = "PackQuantityCalcEdit";
			this.PackQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PackQuantityCalcEdit.TabIndex = 3;
			this.PackQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PackQuantityCalcEdit.MaxLength = 8;
			this.PackQuantityCalcEdit.MaxValue = 99999999;

			// 
			// TypeOfPackagesDropEdit
			// 
			this.TypeOfPackagesDropEdit.AllowDrop = true;
			this.TypeOfPackagesDropEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TypeOfPackagesDropEdit, "FilteredInvoiceLines.PreviousDocuments.CSI_PackType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_PackType)));
			this.TypeOfPackagesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 97, true);
			this.TypeOfPackagesDropEdit.Name = "TypeOfPackagesDropEdit";
			this.TypeOfPackagesDropEdit.ShouldResizeByMaxLength = true;
			this.TypeOfPackagesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.TypeOfPackagesDropEdit.TabIndex = 4;

			// QuantityCalcEdit
			this.QuantityCalcEdit.TabIndex = 6;

			// UnitOfQuantityDropEdit
			this.UnitOfQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 123, true);
			this.UnitOfQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.UnitOfQuantityDropEdit.TabIndex = 7;

			this.TypeOfPackagesDropEdit.ResumeLayout(true);
			this.TypeOfPackagesDropEdit.PerformLayout();
		}

		#endregion

		protected ZArchitecture.ZCalcEdit PackQuantityCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit TypeOfPackagesDropEdit;
	}
}

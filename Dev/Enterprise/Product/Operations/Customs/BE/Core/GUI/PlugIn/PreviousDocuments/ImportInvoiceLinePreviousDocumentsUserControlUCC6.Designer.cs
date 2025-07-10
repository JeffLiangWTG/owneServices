
namespace Enterprise.Customs.BE.GUI.PlugIn
{
	partial class ImportInvoiceLinePreviousDocumentsUserControlUCC6
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
			this.UnitOfQuantity2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Quantity2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnitOfQuantityDropEdit.SuspendLayout();
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnitOfQuantity2DropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// UnitOfQuantityDropEdit
			// 
			this.UnitOfQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 149, true);
			this.UnitOfQuantityDropEdit.TabIndex = 5;
			// 
			// QuantityCalcEdit
			// 
			this.QuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 175, true);
			this.QuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.QuantityCalcEdit.TabIndex = 6;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 201, true);
			this.DescriptionTextBox.TabIndex = 7;
			this.DescriptionTextBox.Visible = false;
			// 
			// PrevDocsGroupBox
			// 
			this.PrevDocsGroupBox.Controls.Add(this.Quantity2CalcEdit);
			this.PrevDocsGroupBox.Controls.Add(this.UnitOfQuantity2DropEdit);
			this.PrevDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 229, true);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.QuantityCalcEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.UnitOfQuantityDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsTypeDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsReferenceTextBox, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.UnitOfQuantity2DropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.Quantity2CalcEdit, 0);
			this.PrevDocsGroupBox.CaptionResourceString = Res.GetData("58EF6377-C06D-43D2-9F36-42E78C7C3A46", "[UCC 2/1] Previous documents");
			// 
			// PreviousDocumentsGrid
			// 
			this.PreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 84, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 84, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 84, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 229, true);
			// 
			// UnitOfQuantity2DropEdit
			// 
			this.UnitOfQuantity2DropEdit.AllowDrop = true;
			this.UnitOfQuantity2DropEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.UnitOfQuantity2DropEdit, "FilteredInvoiceLines.PreviousDocuments.CSI_UnitOfQuantity2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_UnitOfQuantity2)));
			this.UnitOfQuantity2DropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UnitOfQuantity2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 97, true);
			this.UnitOfQuantity2DropEdit.Name = "UnitOfQuantity2DropEdit";
			this.UnitOfQuantity2DropEdit.ShouldResizeByMaxLength = true;
			this.UnitOfQuantity2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.UnitOfQuantity2DropEdit.TabIndex = 3;
			// 
			// Quantity2CalcEdit
			// 
			this.Quantity2CalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.Quantity2CalcEdit, "FilteredInvoiceLines.PreviousDocuments.CSI_Quantity2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_Quantity2)));
			this.Quantity2CalcEdit.CaptionResourceString = null;
			this.Quantity2CalcEdit.DecimalPlaces = 0;
			this.Quantity2CalcEdit.Decimals = 0;
			this.Quantity2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 123, true);
			this.Quantity2CalcEdit.Name = "Quantity2CalcEdit";
			this.Quantity2CalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.Quantity2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.Quantity2CalcEdit.TabIndex = 4;
			this.Quantity2CalcEdit.Text = "0";
			this.Quantity2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ImportInvoiceLinePreviousDocumentsUserControl
			// 
			this.Name = "ImportInvoiceLinePreviousDocumentsUserControl";
			this.UnitOfQuantityDropEdit.ResumeLayout(true);
			this.UnitOfQuantityDropEdit.PerformLayout();
			this.PrevDocsGroupBox.ResumeLayout(false);
			this.PrevDocsGroupBox.PerformLayout();
			this.PrevDocsTypeDropEdit.ResumeLayout(true);
			this.PrevDocsTypeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).EndInit();
			this.PreviousDocumentsGrid.ResumeLayout(false);
			this.PreviousDocumentsGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnitOfQuantity2DropEdit.ResumeLayout(true);
			this.UnitOfQuantity2DropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.GUI.ZDropEdit UnitOfQuantity2DropEdit;
		ZArchitecture.ZCalcEdit Quantity2CalcEdit;

		#endregion
	}
}

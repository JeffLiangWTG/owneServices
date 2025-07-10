
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	partial class LinkedDocumentUserControl
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
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            this.LinkedDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.LinkedDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.LinkedDocumentGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LinkedDocumentGrid)).BeginInit();
            this.LinkedDocumentGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
            // 
            // LinkedDocumentGroupBox
            // 
            this.LinkedDocumentGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("5507ADD9-297C-4DCA-B800-F5E4701DA63D", "Linked Document");
            this.LinkedDocumentGroupBox.Controls.Add(this.LinkedDocumentGrid);
            this.LinkedDocumentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LinkedDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.LinkedDocumentGroupBox.Name = "LinkedDocumentGroupBox";
            this.LinkedDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 228, true);
            this.LinkedDocumentGroupBox.TabIndex = 5;
            this.LinkedDocumentGroupBox.TabStop = false;
            // 
            // LinkedDocumentGrid
            // 
            this.LinkedDocumentGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.LinkedDocumentGrid, "FilteredInvoiceLines.PreviousDocuments");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_ItemNumber)));
            this.LinkedDocumentGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
            zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "CSI_ItemNumber";
            zCalcEditColumnStyleInfo1.Decimals = 0;
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            this.LinkedDocumentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.LinkedDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.LinkedDocumentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.LinkedDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LinkedDocumentGrid.GridId = "eeedf854-f51f-48f7-ab14-125c113d6a00";
            this.LinkedDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.LinkedDocumentGrid.LayoutKey = null;
            this.LinkedDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.LinkedDocumentGrid.Name = "LinkedDocumentGrid";
            this.LinkedDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 209, true);
            this.LinkedDocumentGrid.TabIndex = 2;
            // 
            // LinkedDocumentUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.LinkedDocumentGroupBox);
            this.Name = "LinkedDocumentUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 228, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.LinkedDocumentGroupBox.ResumeLayout(false);
            this.LinkedDocumentGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LinkedDocumentGrid)).EndInit();
            this.LinkedDocumentGrid.ResumeLayout(false);
            this.LinkedDocumentGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox LinkedDocumentGroupBox;
		public ZArchitecture.ZGrid LinkedDocumentGrid;
	}
}

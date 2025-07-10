namespace Enterprise.Customs.BR.GUI
{
	partial class DuimpTaxRegimeUserControl
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
			if (this.LegalGrid != null)
			{
				this.LegalGrid.RowsDeleting -= LegalBaseGrid_RowsDeleting;
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.LegalBasisGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AddLegalBasisButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LegalBasisDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LegalGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TaxRegimeAttributesUserControl = new Enterprise.Customs.BR.GUI.AttributesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.LegalBasisGroupBox.SuspendLayout();
			this.LegalBasisDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LegalGrid)).BeginInit();
			this.LegalGrid.SuspendLayout();
			this.TaxRegimeAttributesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobComInvoiceLine);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.LegalBasisGroupBox);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 261, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(130);
			this.MainSplitContainer.TabIndex = 0;
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.TaxRegimeAttributesUserControl);
			// 
			// LegalBasesGroupBox
			// 
			this.LegalBasisGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("4F0CD5B2-C964-4A47-9906-FD5F828D2F49", "Legal Basis");
			this.LegalBasisGroupBox.Controls.Add(this.AddLegalBasisButton);
			this.LegalBasisGroupBox.Controls.Add(this.LegalBasisDropEdit);
			this.LegalBasisGroupBox.Controls.Add(this.LegalGrid);
			this.LegalBasisGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LegalBasisGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LegalBasisGroupBox.Name = "LegalBasisGroupBox";
			this.LegalBasisGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 130, true);
			this.LegalBasisGroupBox.TabIndex = 1;
			this.LegalBasisGroupBox.TabStop = false;
			// 
			// AddLegalBaseButton
			// 
			this.AddLegalBasisButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("4C4A3D2C-B2E8-4FA0-9DAE-4691EE491223", "Add Legal Basis");
			this.AddLegalBasisButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 19, true);
			this.AddLegalBasisButton.Name = "AddLegalBasisButton";
			this.AddLegalBasisButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 23, true);
			this.AddLegalBasisButton.TabIndex = 3;
			this.AddLegalBasisButton.ToolTipCaption = null;
			this.AddLegalBasisButton.UseVisualStyleBackColor = true;
			this.AddLegalBasisButton.Click += new System.EventHandler(this.AddLegalBaseButton_Click);
			// 
			// LegalBaseDropEdit
			// 
			this.LegalBasisDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LegalBasisDropEdit, "DuimpLegalBase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DuimpLegalBase)));
			this.LegalBasisDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 20, true);
			this.LegalBasisDropEdit.Name = "LegalBasisDropEdit";
			this.LegalBasisDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 20, true);
			this.LegalBasisDropEdit.TabIndex = 2;
			// 
			// LegalGrid
			// 
			this.LegalGrid.AllowNavigation = false;
			this.LegalGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LegalGrid, "DuimpTaxRegimes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DuimpTaxRegimes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.DuimpTaxRegime)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DuimpTaxRegimes)).SyncRoot)).CSI_Procedure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.DuimpTaxRegime)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DuimpTaxRegimes)).SyncRoot)).ProcedureDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.DuimpTaxRegime)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DuimpTaxRegimes)).SyncRoot)).MandatoryDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.DuimpTaxRegime)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DuimpTaxRegimes)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.DuimpTaxRegime)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DuimpTaxRegimes)).SyncRoot)).RateTypeDescription)));
			this.LegalGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_Procedure";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "ProcedureDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.ColumnName = "MandatoryDescription";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "CSI_SubType";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.ColumnName = "RateTypeDescription";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.LegalGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LegalGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LegalGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LegalGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LegalGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LegalGrid.GridId = "b624d090-cbe4-48f5-9a70-26feb5dd28a4";
			this.LegalGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LegalGrid.LayoutKey = "LegalGrid";
			this.LegalGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 48, true);
			this.LegalGrid.Name = "LegalGrid";
			this.LegalGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(732, 79, true);
			this.LegalGrid.TabIndex = 4;
			this.LegalGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.LegalGrid.RowsDeleting += new System.EventHandler<Enterprise.ZArchitecture.RowsDeletingEventArgs>(LegalBaseGrid_RowsDeleting);
			// 
			// TaxRegimeAttributesUserControl
			// 
			this.TaxRegimeAttributesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxRegimeAttributesUserControl, "TaxRegimeAttributes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).TaxRegimeAttributes)));
			this.TaxRegimeAttributesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxRegimeAttributesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TaxRegimeAttributesUserControl.Name = "TaxRegimeAttributesUserControl";
			this.TaxRegimeAttributesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 171, true);
			this.TaxRegimeAttributesUserControl.TabIndex = 5;
			// 
			// DuimpTaxRegimeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainSplitContainer);
			this.Name = "DuimpTaxRegimeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 261, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.LegalBasisGroupBox.ResumeLayout(false);
			this.LegalBasisGroupBox.PerformLayout();
			this.LegalBasisDropEdit.ResumeLayout(true);
			this.LegalBasisDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LegalGrid)).EndInit();
			this.LegalGrid.ResumeLayout(false);
			this.LegalGrid.PerformLayout();
			this.TaxRegimeAttributesUserControl.ResumeLayout(true);
			this.TaxRegimeAttributesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		internal ZArchitecture.GUI.ZGroupBox LegalBasisGroupBox;
		internal ZArchitecture.ZGrid LegalGrid;
		internal ZArchitecture.GUI.ZDropEdit LegalBasisDropEdit;
		internal ZArchitecture.GUI.ZButton AddLegalBasisButton;
		internal AttributesUserControl TaxRegimeAttributesUserControl;
	}
}

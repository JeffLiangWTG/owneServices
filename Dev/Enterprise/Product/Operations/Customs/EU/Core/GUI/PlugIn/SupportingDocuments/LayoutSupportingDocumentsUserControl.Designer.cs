namespace Enterprise.Customs.EU.GUI.PlugIn
{
	partial class LayoutSupportingDocumentsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.SupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SupportingDocumentsSplitter = new CargoWise.Windows.UI.KSplitter();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.gridSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.SupportingDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// SupportingDocumentsGrid
			// 
			this.SupportingDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, "FilteredInvoiceLines.SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_CodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).ReferenceNumberFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).QuantityDecimalPlaces)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).UnitOfQuantityFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Quantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfExpiry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_AdditionalDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ItemNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_RN_NKCountryCode)));
			this.SupportingDocumentsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_CodeDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ReferenceNumberFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Status";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "QuantityDecimalPlaces";
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo1.Decimals = 5;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo2.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo2.ColumnName = "CSI_UnitOfQuantity";
			zMultiControlColumnStyleInfo2.FieldTypeColumnName = "UnitOfQuantityFieldType";
			zMultiControlColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_Quantity2";
			zCalcEditColumnStyleInfo2.Decimals = 5;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_UnitOfQuantity2";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CSI_Value";
			zCalcEditColumnStyleInfo3.Decimals = 5;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CSI_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "CSI_DateOfExpiry";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_AdditionalDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "CSI_ItemNumber";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "CSI_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo2);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.SupportingDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGrid.GridId = "D20B3E1B-C507-49EC-A070-650E5C6449FF";
			this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupportingDocumentsGrid.LayoutKey = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 222, true);
			this.SupportingDocumentsGrid.TabIndex = 0;
			// 
			// SupportingDocumentsSplitter
			// 
			this.SupportingDocumentsSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SupportingDocumentsSplitter.DoNotSaveSplitterLayout = false;
			this.SupportingDocumentsSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 132, true);
			this.SupportingDocumentsSplitter.Name = "SupportingDocumentsSplitter";
			this.SupportingDocumentsSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 3, true);
			this.SupportingDocumentsSplitter.TabIndex = 2;
			this.SupportingDocumentsSplitter.TabStop = false;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 225, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.TabIndex = 2;
			// 
			// gridSplitter
			// 
			this.gridSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.gridSplitter.DoNotSaveSplitterLayout = false;
			this.gridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 222, true);
			this.gridSplitter.Name = "gridSplitter";
			this.gridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 3, true);
			this.gridSplitter.TabIndex = 1;
			this.gridSplitter.TabStop = false;
			// 
			// LayoutSupportingDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingDocumentsGrid);
			this.Controls.Add(this.gridSplitter);
			this.Controls.Add(this.BottomPanel);
			this.Name = "LayoutSupportingDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 316, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.SupportingDocumentsGrid.ResumeLayout(false);
			this.SupportingDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected CargoWise.Windows.UI.KSplitter SupportingDocumentsSplitter;
		protected Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		protected CargoWise.Windows.UI.KSplitter gridSplitter;
		public Enterprise.ZArchitecture.ZGrid SupportingDocumentsGrid;
	}
}

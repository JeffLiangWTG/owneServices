namespace Enterprise.Customs.KR.GUI
{
	partial class ImportFTAInvoiceLineDetailsUserControl
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
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.FTAInvoiceLineDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FTAInvoiceLineDetailsGrid)).BeginInit();
            this.FTAInvoiceLineDetailsGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent);
            // 
            // FTAInvoiceLineDetailsGrid
            // 
            this.FTAInvoiceLineDetailsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.FTAInvoiceLineDetailsGrid, "SendingObjectsCollection.FTAInvoiceLines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTAInvoiceLines)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTAInvoiceLines)).SyncRoot)).EntryLineNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTAInvoiceLines)).SyncRoot)).InvoiceLineNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTAInvoiceLines)).SyncRoot)).CertificateOfOriginNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTAInvoiceLines)).SyncRoot)).CertificateOfOriginSeq)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTAInvoiceLines)).SyncRoot)).CertificateOfOriginUsedQuantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FTAMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).FTAInvoiceLines)).SyncRoot)).CertificateOfOriginUsedUQ)));
            this.FTAInvoiceLineDetailsGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "EntryLineNo";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.IsReadOnly = true;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "InvoiceLineNo";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.IsReadOnly = true;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo1.ColumnName = "CertificateOfOriginNo";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.ColumnName = "CertificateOfOriginSeq";
            zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo3.IsReadOnly = true;
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo4.ColumnName = "CertificateOfOriginUsedQuantity";
            zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo4.IsReadOnly = true;
            zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo2.ColumnName = "CertificateOfOriginUsedUQ";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.IsReadOnly = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.FTAInvoiceLineDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.FTAInvoiceLineDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.FTAInvoiceLineDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.FTAInvoiceLineDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.FTAInvoiceLineDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
            this.FTAInvoiceLineDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.FTAInvoiceLineDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FTAInvoiceLineDetailsGrid.GridId = "d05379d1-9235-4d72-aa17-2008ee13b507";
            this.FTAInvoiceLineDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.FTAInvoiceLineDetailsGrid.LayoutKey = "FTAInvoiceLineDetailsGrid";
            this.FTAInvoiceLineDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.FTAInvoiceLineDetailsGrid.Name = "FTAInvoiceLineDetailsGrid";
            this.FTAInvoiceLineDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 226, true);
            this.FTAInvoiceLineDetailsGrid.TabIndex = 0;
            // 
            // ImportFTAInvoiceLineDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.FTAInvoiceLineDetailsGrid);
            this.Name = "ImportFTAInvoiceLineDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 226, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FTAInvoiceLineDetailsGrid)).EndInit();
            this.FTAInvoiceLineDetailsGrid.ResumeLayout(false);
            this.FTAInvoiceLineDetailsGrid.PerformLayout();
			this.CaptionRenderingEnabled = true;
			this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid FTAInvoiceLineDetailsGrid;
	}
}

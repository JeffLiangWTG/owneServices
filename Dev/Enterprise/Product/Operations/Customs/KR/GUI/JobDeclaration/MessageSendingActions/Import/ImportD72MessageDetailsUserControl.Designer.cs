namespace Enterprise.Customs.KR.GUI
{
	partial class ImportD72MessageDetailsUserControl
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
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.EntryLineGrid = new Enterprise.ZArchitecture.ZGrid();
            this.InvoiceLineGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.InvoiceLineGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).BeginInit();
            this.EntryLineGrid.SuspendLayout();
            this.InvoiceLineGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InvoiceLineGrid)).BeginInit();
            this.InvoiceLineGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject);
            // 
            // EntryLineGrid
            // 
            this.EntryLineGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.EntryLineGrid, "D72EntryLines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).D72EntryLines)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).D72EntryLines)).SyncRoot)).EntryLineNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).D72EntryLines)).SyncRoot)).FormattedHSCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).D72EntryLines)).SyncRoot)).HSDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingEntryLineObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).D72EntryLines)).SyncRoot)).Preference)));
            this.EntryLineGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "EntryLineNo";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo1.ColumnName = "FormattedHSCode";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
            zTextBoxColumnStyleInfo2.ColumnName = "HSDescription";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo3.ColumnName = "Preference";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            this.EntryLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.EntryLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.EntryLineGrid.Dock = System.Windows.Forms.DockStyle.Top;
            this.EntryLineGrid.GridId = "727df695-4a74-4aad-874d-6aa343190f60";
            this.EntryLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.EntryLineGrid.LayoutKey = "EntryLineGrid";
            this.EntryLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.EntryLineGrid.Name = "EntryLineGrid";
            this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 97, true);
            this.EntryLineGrid.TabIndex = 0;
            // 
            // InvoiceLineGroupBox
            // 
            this.InvoiceLineGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("3a0b0f4c-7267-4ef8-9db8-777b160a75c6", "Invoice Lines");
            this.InvoiceLineGroupBox.Controls.Add(this.InvoiceLineGrid);
            this.InvoiceLineGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InvoiceLineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 97, true);
            this.InvoiceLineGroupBox.Name = "InvoiceLineGroupBox";
            this.InvoiceLineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 176, true);
            this.InvoiceLineGroupBox.TabIndex = 1;
            this.InvoiceLineGroupBox.TabStop = false;
            // 
            // InvoiceLineGrid
            // 
            this.InvoiceLineGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.InvoiceLineGrid, "MessageSendingInvoiceLines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).MessageSendingInvoiceLines)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).MessageSendingInvoiceLines)).SyncRoot)).EntryLineNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).MessageSendingInvoiceLines)).SyncRoot)).InvoiceLineNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).MessageSendingInvoiceLines)).SyncRoot)).ItemDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).MessageSendingInvoiceLines)).SyncRoot)).Quantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).MessageSendingInvoiceLines)).SyncRoot)).UQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).MessageSendingInvoiceLines)).SyncRoot)).LinePrice)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).MessageSendingInvoiceLines)).SyncRoot)).AmountCurrency)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.MessageSendingInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.ExtendReExportDateMessageSendingObject)(null)).MessageSendingInvoiceLines)).SyncRoot)).Remark)));
            this.InvoiceLineGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo4.ColumnName = "EntryLineNo";
            zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo4.IsReadOnly = true;
            zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo5.ColumnName = "InvoiceLineNo";
            zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo5.IsReadOnly = true;
            zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo4.ColumnName = "ItemDescription";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.IsReadOnly = true;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo6.ColumnName = "Quantity";
            zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo6.IsReadOnly = true;
            zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.ColumnName = "UQ";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.IsReadOnly = true;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo7.ColumnName = "LinePrice";
            zCalcEditColumnStyleInfo7.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo7.IsReadOnly = true;
            zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo6.ColumnName = "AmountCurrency";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.IsReadOnly = true;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo7.ColumnName = "Remark";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
             this.InvoiceLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
            this.InvoiceLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
            this.InvoiceLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.InvoiceLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
            this.InvoiceLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.InvoiceLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
            this.InvoiceLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.InvoiceLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.InvoiceLineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InvoiceLineGrid.GridId = "4e03ec62-cc76-4c77-a3cc-07cad7c7c772";
            this.InvoiceLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.InvoiceLineGrid.LayoutKey = "InvoiceLineGrid";
            this.InvoiceLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.InvoiceLineGrid.Name = "InvoiceLineGrid";
            this.InvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 159, true);
            this.InvoiceLineGrid.TabIndex = 0;
            // 
            // ImportD72MessageDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.InvoiceLineGroupBox);
            this.Controls.Add(this.EntryLineGrid);
            this.Name = "ImportD72MessageDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 273, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).EndInit();
            this.EntryLineGrid.ResumeLayout(false);
            this.EntryLineGrid.PerformLayout();
            this.InvoiceLineGroupBox.ResumeLayout(false);
            this.InvoiceLineGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InvoiceLineGrid)).EndInit();
            this.InvoiceLineGrid.ResumeLayout(false);
            this.InvoiceLineGrid.PerformLayout();
			this.CaptionRenderingEnabled = true;
			this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid EntryLineGrid;
		private ZArchitecture.GUI.ZGroupBox InvoiceLineGroupBox;
		private ZArchitecture.ZGrid InvoiceLineGrid;
	}
}


namespace Enterprise.Customs.BR.GUI
{
	partial class PermitUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.PermitGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PermitGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PermitGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitGrid)).BeginInit();
			this.PermitGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			//
			// PermitGroupBox
			//
			this.PermitGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("811E3200-EC14-45B7-BB4E-1B504F679A2F", "LPCO");
			this.PermitGroupBox.Controls.Add(this.PermitGrid);
			this.PermitGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PermitGroupBox.Name = "PermitGroupBox";
			this.PermitGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 228, true);
			this.PermitGroupBox.TabIndex = 5;
			this.PermitGroupBox.TabStop = false;
			//
			// PermitGrid
			//
			this.PermitGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PermitGrid, "FilteredInvoiceLines.Permits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Permits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.Permit)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Permits)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.Permit)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Permits)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.Permit)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Permits)).SyncRoot)).CSI_UnitOfQuantity)));
			this.PermitGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.ColumnName = "CSI_UnitOfQuantity";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.PermitGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PermitGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PermitGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PermitGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitGrid.GridId = "35A466DB-08B7-4B4A-947F-A4E83B939D5E";
			this.PermitGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PermitGrid.LayoutKey = "PermitGrid";
			this.PermitGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
			this.PermitGrid.Name = "PermitGrid";
			this.PermitGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 191, true);
			this.PermitGrid.TabIndex = 2;
			//
			// PermitUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PermitGroupBox);
			this.Name = "PermitUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 228, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PermitGroupBox.ResumeLayout(false);
			this.PermitGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitGrid)).EndInit();
			this.PermitGrid.ResumeLayout(false);
			this.PermitGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZGrid PermitGrid;
		internal ZArchitecture.GUI.ZGroupBox PermitGroupBox;
	}
}

using CargoWise.Windows.UI;

namespace Enterprise.Customs.KR.GUI
{
	partial class ValuationDeclarationUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.ValuationLineGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ValuationLineGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.ValuationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ValuationLineGrid)).BeginInit();
			this.ValuationLineGrid.SuspendLayout();
			this.ValuationLineGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.ValuationDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// ValuationLineGrid
			// 
			this.ValuationLineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ValuationLineGrid, "FilteredInvoiceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PartNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.PartsList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Model)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_BrandName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Ingredient)));
			this.ValuationLineGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JI_LineNo";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.ToolTip = "Line Number";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups.PartsList";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JI_PartNo";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.SupplierPart;
			zCodeFindBoxColumnStyleInfo1.ToolTip = "Product";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo1.ColumnName = "JI_FormattedTariff";
			tariffColumnStyleInfo1.DefaultCollectionIndex = 0;
			tariffColumnStyleInfo1.NeedLoadNomenclatureWhenTariffNotFound = false;
			tariffColumnStyleInfo1.NeedLoadParentDataGroup = true;
			tariffColumnStyleInfo1.SelectNomenclatureModes = null;
			tariffColumnStyleInfo1.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			tariffColumnStyleInfo1.TariffType = null;
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo1.ColumnName = "JI_Description";
			zMultiLineTextBoxColumnInfo1.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.ToolTip = "Invoice line description";
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo1.ColumnName = "JI_Model";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "JI_BrandName";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo2.ColumnName = "JI_Ingredient";
			zMultiLineTextBoxColumnInfo2.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ValuationLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ValuationLineGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ValuationLineGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.ValuationLineGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.ValuationLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ValuationLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ValuationLineGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.ValuationLineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValuationLineGrid.GridId = "E0AF3A85-D5BA-4885-9150-3FEC435B2F10";
			this.ValuationLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ValuationLineGrid.LayoutKey = "ValuationLineGrid";
			this.ValuationLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ValuationLineGrid.Name = "ValuationLineGrid";
			this.ValuationLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 101, true);
			this.ValuationLineGrid.TabIndex = 1;
			// 
			// ValuationLineGroupBox
			// 
			this.ValuationLineGroupBox.Controls.Add(this.ValuationLineGrid);
			this.ValuationLineGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ValuationLineGroupBox, false);
			this.ValuationLineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValuationLineGroupBox.Name = "ValuationLineGroupBox";
			this.ValuationLineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 120, true);
			this.ValuationLineGroupBox.TabIndex = 0;
			this.ValuationLineGroupBox.TabStop = false;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.ValuationLineGroupBox);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 469, true);
			this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ValuationDetailsGroupBox);
			this.splitContainer1.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
			this.splitContainer1.TabIndex = 1;
			// 
			// ValuationDetailsGroupBox
			// 
			this.ValuationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("36A45038-5380-4C81-AA35-4D03DCB8649B", "Details");
			this.ValuationDetailsGroupBox.Controls.Add(this.DynamicLayoutPanel);
			this.ValuationDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValuationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValuationDetailsGroupBox.Name = "ValuationDetailsGroupBox";
			this.ValuationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 345, true);
			this.ValuationDetailsGroupBox.TabIndex = 0;
			this.ValuationDetailsGroupBox.TabStop = false;
			// 
			// DynamicLayoutPanel
			// 
			this.DynamicLayoutPanel.AllowDrop = true;
			this.DynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicLayoutPanel.Name = "DynamicLayoutPanel";
			this.DynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 326, true);
			this.DynamicLayoutPanel.TabIndex = 0;
			// 
			// ValuationDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ValuationDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 469, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ValuationLineGrid)).EndInit();
			this.ValuationLineGrid.ResumeLayout(false);
			this.ValuationLineGrid.PerformLayout();
			this.ValuationLineGroupBox.ResumeLayout(false);
			this.ValuationLineGroupBox.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.ValuationDetailsGroupBox.ResumeLayout(false);
			this.ValuationDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private ZArchitecture.ZGrid ValuationLineGrid;
		internal ZArchitecture.GUI.ZGroupBox ValuationLineGroupBox;
		private KSplitContainer splitContainer1;
		private ZArchitecture.GUI.ZGroupBox ValuationDetailsGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel DynamicLayoutPanel;
	}
}

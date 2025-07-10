using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class PackingUserControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			this.ExportLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PackingDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PackingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackingDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.splitter = new CargoWise.Windows.UI.KSplitter();
			this.HouseBillPanel.SuspendLayout();
			this.BillFilterByAndGridPanel.SuspendLayout();
			this.BillGroupBoxPanel.SuspendLayout();
			this.HouseBillsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).BeginInit();
			this.PackingDetailsPanel.SuspendLayout();
			this.PackingDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// HouseBillPanel
			// 
			this.HouseBillPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.HouseBillPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 248, true);
			// 
			// BillFilterByAndGridPanel
			// 
			this.BillFilterByAndGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 248, true);
			// 
			// FilterByPanel
			// 
			this.FilterByPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 45, true);
			// 
			// BillGroupBoxPanel
			// 
			this.BillGroupBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 203, true);
			// 
			// HouseBillsGroupBox
			// 
			this.HouseBillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 203, true);
			// 
			// HouseBillsGrid
			// 
			this.HouseBillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 184, true);
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.Bill)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredBills)))).Lookups.CU_BillTypeList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.Bill)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredBills)))).CU_BillTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.Bill)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredBills)))).CU_BillType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.Bill)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredBills)))).CU_BillNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.Bill)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredBills)))).CU_BillNum)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.Bill)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredBills)))).CU_IssueDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.Bill)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredBills)))).CU_IssueDateInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.Bill)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredBills)))).Lookups.CU_ParentBillList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.Bill)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredBills)))).CU_ParentBillUniqueCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.Bill)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredBills)))).CU_ParentBillUniqueCode)));
			// 
			// ExportLabel
			// 
			this.ExportLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExportLabel.Name = "ExportLabel";
			this.ExportLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 472, true);
			this.ExportLabel.TabIndex = 14;
			this.ExportLabel.Text = "Packing Information not available for the selected declaration Type, Style or Tra" +
				"nsport Mode.";
			this.ExportLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// PackingDetailsPanel
			// 
			this.PackingDetailsPanel.Controls.Add(this.PackingDetailsGroupBox);
			this.PackingDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 251, true);
			this.PackingDetailsPanel.Name = "PackingDetailsPanel";
			this.PackingDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 221, true);
			this.PackingDetailsPanel.TabIndex = 17;
			// 
			// PackingDetailsGroupBox
			// 
			this.PackingDetailsGroupBox.Controls.Add(this.PackingDetailsGrid);
			this.PackingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDetailsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PackingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackingDetailsGroupBox.Name = "PackingDetailsGroupBox";
			this.PackingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 221, true);
			this.PackingDetailsGroupBox.TabIndex = 0;
			this.PackingDetailsGroupBox.TabStop = false;
			this.PackingDetailsGroupBox.Text = "Packing Details";
			// 
			// PackingDetailsGrid
			// 
			this.PackingDetailsGrid.AllowNavigation = false;
			this.PackingDetailsGrid.BindTo = "Packages";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)));
			this.PackingDetailsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups+LowestBills";
			zDropEditColumnStyleInfo1.Caption = "Linked Bill(Lowest Bill)";
			zDropEditColumnStyleInfo1.ColumnName = "CW_HouseBill";
			zDropEditColumnStyleInfo2.BindToList = "ContainersAndEquipmentsOnDeclaration_List";
			zDropEditColumnStyleInfo2.Caption = "Container No.";
			zDropEditColumnStyleInfo2.ColumnName = "CW_ContainerNoOrEquipmentNo";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Dec. Total Pkgs";
			zCalcEditColumnStyleInfo1.ColumnName = "CW_PackQty";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Outer Packing Unit Count";
			zCalcEditColumnStyleInfo2.ColumnName = "CW_OuterPacks";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Warehouse No. of Packages";
			zCalcEditColumnStyleInfo3.ColumnName = "CW_InBondPackQty";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);

			#region Column Initialisation

			zMultiLineTextBoxColumnInfo1.Caption = "Marks And Numbers";
			zMultiLineTextBoxColumnInfo1.ColumnName = "CW_MarksAndNos";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zTextBoxColumnStyleInfo1.Caption = "Click Cargo Status for Details";
			zTextBoxColumnStyleInfo1.ColumnName = "CW_CargoStatus";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.Caption = "Consign. Ref. No.";
			zTextBoxColumnStyleInfo2.ColumnName = "CU_fPartShipConsignmentReference";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.CharacterCasing = CharacterCasing.Upper;
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PackingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PackingDetailsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.PackingDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

			#endregion

			this.PackingDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackingDetailsGrid.LayoutKey = "PackingDetailsGrid";
			this.PackingDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackingDetailsGrid.Name = "PackingDetailsGrid";
			this.PackingDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 202, true);
			this.PackingDetailsGrid.TabIndex = 0;
			this.PackingDetailsGrid.CurrentCellChanged += new System.EventHandler(this.PackingDetailsGrid_Click);
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).Lookups.LowestBills)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_HouseBillInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_HouseBill)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).ContainersAndEquipmentsOnDeclaration_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_ContainerNoOrEquipmentNoInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_ContainerNoOrEquipmentNo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_PackQty)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_PackQtyInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_OuterPacks)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_OuterPacksInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_InBondPackQty)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_InBondPackQtyInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_MarksAndNosInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_MarksAndNos)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).Lookups.DangerousGoods)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_CargoStatusInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.Package)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).Packages)))).CW_CargoStatus)));
			// 
			// Splitter
			// 
			this.splitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 248, true);
			this.splitter.Name = "Splitter";
			this.splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 3, true);
			this.splitter.TabIndex = 18;
			this.splitter.TabStop = false;
			// 
			// PackingUserControl
			// 
			this.Controls.Add(this.PackingDetailsPanel);
			this.Controls.Add(this.splitter);
			this.Controls.Add(this.ExportLabel);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.JobDeclaration";
			this.Name = "PackingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 472, true);
			this.Controls.SetChildIndex(this.ExportLabel, 0);
			this.Controls.SetChildIndex(this.HouseBillPanel, 0);
			this.Controls.SetChildIndex(this.splitter, 0);
			this.Controls.SetChildIndex(this.PackingDetailsPanel, 0);
			this.HouseBillPanel.ResumeLayout(false);
			this.BillFilterByAndGridPanel.ResumeLayout(false);
			this.BillGroupBoxPanel.ResumeLayout(false);
			this.HouseBillsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).EndInit();
			this.PackingDetailsPanel.ResumeLayout(false);
			this.PackingDetailsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).EndInit();
			this.ResumeLayout(false);
		}

		protected internal ZLabel ExportLabel;
		protected internal ZPanel PackingDetailsPanel;
		protected internal ZGroupBox PackingDetailsGroupBox;
		protected internal ZGrid PackingDetailsGrid;
		private CargoWise.Windows.UI.KSplitter splitter;
	}
}

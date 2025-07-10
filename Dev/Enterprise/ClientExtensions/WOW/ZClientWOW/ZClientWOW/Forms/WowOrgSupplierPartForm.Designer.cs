using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.Wow
{
	public partial class WowOrgSupplierPartForm : OrgSupplierPartForm
	{
		Enterprise.ZArchitecture.ZGrid LocationsBoundGrid;
		Enterprise.ZArchitecture.GUI.ZTabPage WarehouseLocationsTabPage;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.WarehouseLocationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LocationsBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.PartUnitsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PartBarcodesGrid)).BeginInit();
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WarehouseLocationsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LocationsBoundGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// PartUnitsGrid
			// 
			this.PartUnitsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 354, true);
			// 
			// PartBarcodesGrid
			// 
			this.PartBarcodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 354, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WarehouseLocationsTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 608, true);
			this.MainTabControl.Controls.SetChildIndex(this.WarehouseLocationsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 581, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 24, true);
			// 
			// WarehouseLocationsTabPage
			// 
			this.WarehouseLocationsTabPage.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("WowOrgSupplierPartForm|65b6393d-c959-454f-8f8e-c2798c0619be", "Warehouse Locations");
			this.WarehouseLocationsTabPage.Controls.Add(this.LocationsBoundGrid);
			this.WarehouseLocationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WarehouseLocationsTabPage.Name = "WarehouseLocationsTabPage";
			this.WarehouseLocationsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WarehouseLocationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 581, true);
			this.WarehouseLocationsTabPage.TabIndex = 6;
			// 
			// LocationsBoundGrid
			// 
			this.LocationsBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LocationsBoundGrid, "Locations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).Locations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartLocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).Locations)).SyncRoot)).OR_Warehouse)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartLocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).Locations)).SyncRoot)).OR_BinLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartLocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).Locations)).SyncRoot)).OR_StockTakeCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartLocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).Locations)).SyncRoot)).OR_InStock)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartLocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).Locations)).SyncRoot)).OR_WeightCostThisLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartLocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).Locations)).SyncRoot)).OR_Ti)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgPartLocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSupplierPart)(null)).Locations)).SyncRoot)).OR_Hi)));
			this.LocationsBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "OR_Warehouse";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.ColumnName = "OR_BinLocation";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "OR_StockTakeCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "OR_InStock";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "OR_WeightCostThisLocation";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "OR_Ti";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "OR_Hi";
			zCalcEditColumnStyleInfo5.Decimals = 0;
			this.LocationsBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LocationsBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LocationsBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LocationsBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LocationsBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LocationsBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.LocationsBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.LocationsBoundGrid.GridId = "cee28001-f2c9-4e7d-851f-62d2aa0cec34";
			this.LocationsBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LocationsBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LocationsBoundGrid.LayoutKey = "LocationsBoundGrid";
			this.LocationsBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LocationsBoundGrid.Name = "LocationsBoundGrid";
			this.LocationsBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(715, 575, true);
			this.LocationsBoundGrid.TabIndex = 1;
			// 
			// WowOrgSupplierPartForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 664, true);
			this.Name = "WowOrgSupplierPartForm";
			((System.ComponentModel.ISupportInitialize)(this.PartUnitsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PartBarcodesGrid)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.WarehouseLocationsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LocationsBoundGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}

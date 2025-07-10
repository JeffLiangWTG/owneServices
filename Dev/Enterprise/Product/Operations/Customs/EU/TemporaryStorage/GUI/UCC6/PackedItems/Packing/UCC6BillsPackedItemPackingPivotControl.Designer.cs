namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class UCC6BillsPackedItemPackingPivotControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected Enterprise.ZArchitecture.ZGrid TemporaryStorageLinkPackagesGrid;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.TemporaryStorageLinkPackagesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TemporaryStorageLinkPackagesGrid)).BeginInit();
			this.TemporaryStorageLinkPackagesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// TemporaryStorageLinkPackagesGrid
			// 
			this.TemporaryStorageLinkPackagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TemporaryStorageLinkPackagesGrid, "Bills.PackedItems.TemporaryStorageLinkPackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).TemporaryStorageLinkPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageLinkPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).TemporaryStorageLinkPackages)).SyncRoot)).IsLinked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageLinkPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).TemporaryStorageLinkPackages)).SyncRoot)).PackageNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageLinkPackage)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)).SyncRoot)).TemporaryStorageLinkPackages)).SyncRoot)).PackQty)));
			this.TemporaryStorageLinkPackagesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "IsLinked";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.ColumnName = "PackageNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "PackQty";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.TemporaryStorageLinkPackagesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TemporaryStorageLinkPackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TemporaryStorageLinkPackagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TemporaryStorageLinkPackagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TemporaryStorageLinkPackagesGrid.GridId = "BA2F3871-F26B-432B-852F-31296AB193DE";
			this.TemporaryStorageLinkPackagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TemporaryStorageLinkPackagesGrid.LayoutKey = "TemporaryStorageLinkPackagesGrid";
			this.TemporaryStorageLinkPackagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TemporaryStorageLinkPackagesGrid.Name = "TemporaryStorageLinkPackagesGrid";
			this.TemporaryStorageLinkPackagesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.TemporaryStorageLinkPackagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 159, true);
			this.TemporaryStorageLinkPackagesGrid.TabIndex = 0;
			// 
			// UCC6BillsPackedItemPackingPivotControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TemporaryStorageLinkPackagesGrid);
			this.Name = "UCC6BillsPackedItemPackingPivotControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 159, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TemporaryStorageLinkPackagesGrid)).EndInit();
			this.TemporaryStorageLinkPackagesGrid.ResumeLayout(false);
			this.TemporaryStorageLinkPackagesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}

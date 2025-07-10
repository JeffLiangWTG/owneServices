namespace Enterprise.Customs.IL.Manifest.GUI
{
	partial class AsycudaPackedItemPackingPivotControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected Enterprise.ZArchitecture.ZGrid AsycudaLinkPackagesGrid;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.AsycudaLinkPackagesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AsycudaLinkPackagesGrid)).BeginInit();
			this.AsycudaLinkPackagesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Manifest.Business.AsycudaBill);
			// 
			// AsycudaLinkPackagesGrid
			// 
			this.AsycudaLinkPackagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AsycudaLinkPackagesGrid, "PackedItems.AsycudaLinkPackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaLinkPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IL.Manifest.Business.AsycudaLinkPackage)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaLinkPackages)).SyncRoot)).IsLinked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaLinkPackage)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaLinkPackages)).SyncRoot)).PackageNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IL.Manifest.Business.AsycudaLinkPackage)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaLinkPackages)).SyncRoot)).ContainerPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Manifest.Business.AsycudaLinkPackage)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).AsycudaLinkPackages)).SyncRoot)).PackQty)));
			this.AsycudaLinkPackagesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "IsLinked";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.ColumnName = "PackageNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			zGuidDropEditColumnStyleInfo1.ColumnName = "ContainerPK";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "PackQty";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.AsycudaLinkPackagesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AsycudaLinkPackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AsycudaLinkPackagesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.AsycudaLinkPackagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AsycudaLinkPackagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AsycudaLinkPackagesGrid.GridId = "9f5a8f73-9eae-42d0-b91b-25f210683fb9";
			this.AsycudaLinkPackagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AsycudaLinkPackagesGrid.LayoutKey = "AsycudaLinkPackagesGrid";
			this.AsycudaLinkPackagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AsycudaLinkPackagesGrid.Name = "AsycudaLinkPackagesGrid";
			this.AsycudaLinkPackagesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.AsycudaLinkPackagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 159, true);
			this.AsycudaLinkPackagesGrid.TabIndex = 0;
			// 
			// AsycudaPackedItemPackingPivotControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AsycudaLinkPackagesGrid);
			this.Name = "AsycudaPackedItemPackingPivotControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 159, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AsycudaLinkPackagesGrid)).EndInit();
			this.AsycudaLinkPackagesGrid.ResumeLayout(false);
			this.AsycudaLinkPackagesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}

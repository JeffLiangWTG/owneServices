namespace Enterprise.Customs.IL.Manifest.GUI
{
	partial class AsycudaPackedItemGridControl
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
			Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.PackedItemGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PackedItemGrid)).BeginInit();
			this.PackedItemGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Manifest.Business.AsycudaBill);
			// 
			// PackedItemGrid
			// 
			this.PackedItemGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackedItemGrid, "PackedItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_GrossWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_PackStatus)));
			this.PackedItemGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "API_LineNo";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo1.ColumnName = "API_FormattedTariff";
			tariffColumnStyleInfo1.DefaultCollectionIndex = 0;
			tariffColumnStyleInfo1.NeedLoadNomenclatureWhenTariffNotFound = false;
			tariffColumnStyleInfo1.NeedLoadParentDataGroup = true;
			tariffColumnStyleInfo1.SelectNomenclatureModes = null;
			tariffColumnStyleInfo1.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			tariffColumnStyleInfo1.TariffType = null;
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "API_GoodsDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "API_GrossWeight";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "API_GrossWeightUQ";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "API_PackStatus";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PackedItemGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackedItemGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.PackedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackedItemGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackedItemGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackedItemGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PackedItemGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackedItemGrid.GridId = "25C2F252-87C6-47B4-9348-6C2CFBA08F4A";
			this.PackedItemGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackedItemGrid.LayoutKey = "zGrid1";
			this.PackedItemGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackedItemGrid.Name = "PackedItemGrid";
			this.PackedItemGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 305, true);
			this.PackedItemGrid.TabIndex = 0;
			// 
			// AsycudaPackedItemGridControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackedItemGrid);
			this.Name = "AsycudaPackedItemGridControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 305, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PackedItemGrid)).EndInit();
			this.PackedItemGrid.ResumeLayout(false);
			this.PackedItemGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid PackedItemGrid;
	}
}

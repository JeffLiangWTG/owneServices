namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class UnloadingItemDifferencesTabUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.UnloadingItemDifferencesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.GoodsItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UnloadedGoodsItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UnloadingItemDifferencesSplitContainer)).BeginInit();
			this.UnloadingItemDifferencesSplitContainer.Panel1.SuspendLayout();
			this.UnloadingItemDifferencesSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GoodsItemsGrid)).BeginInit();
			this.GoodsItemsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnloadedGoodsItemsGrid)).BeginInit();
			this.UnloadedGoodsItemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// UnloadingItemDifferencesSplitContainer
			// 
			this.UnloadingItemDifferencesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingItemDifferencesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnloadingItemDifferencesSplitContainer.Name = "UnloadingItemDifferencesSplitContainer";
			this.UnloadingItemDifferencesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// UnloadingItemDifferencesSplitContainer.Panel1
			// 
			this.UnloadingItemDifferencesSplitContainer.Panel1.Controls.Add(this.kSplitContainer1);
			this.UnloadingItemDifferencesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1297, 646, true);
			this.UnloadingItemDifferencesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(389);
			this.UnloadingItemDifferencesSplitContainer.TabIndex = 0;
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.GoodsItemsGrid);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.UnloadedGoodsItemsGrid);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1297, 389, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(593);
			this.kSplitContainer1.TabIndex = 0;
			// 
			// GoodsItemsGrid
			// 
			this.GoodsItemsGrid.AllowNavigation = false;
			this.GoodsItemsGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.GoodsItemsGrid, "ArrivalMovementHeader.GoodsItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.GoodsItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.GoodsItems)).SyncRoot)).BY_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.GoodsItems)).SyncRoot)).BY_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.GoodsItems)).SyncRoot)).BY_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.GoodsItems)).SyncRoot)).BY_GrossWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.GoodsItems)).SyncRoot)).BY_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.GoodsItems)).SyncRoot)).BY_NetWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.GoodsItems)).SyncRoot)).BY_FormattedHarmonisedTariff)));
			this.GoodsItemsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "BY_LineNo";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.ColumnName = "BY_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "BY_GrossWeight";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("950BF22D-15DC-4E31-8131-2A557D8490EE", "Gross Weight");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "BY_GrossWeightUnit";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("950BF22D-15DC-4E31-8131-2A557D8490EE", "Gross Weight");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "BY_NetWeight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("46872311-9996-4DC4-8AD0-34F311C359CA", "Net Weight");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "BY_NetWeightUnit";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("46872311-9996-4DC4-8AD0-34F311C359CA", "Net Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.ColumnName = "BY_FormattedHarmonisedTariff";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.GoodsItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GoodsItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.GoodsItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.GoodsItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.GoodsItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.GoodsItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.GoodsItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.GoodsItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemsGrid.GridId = "e218cc11-6496-4d5c-805c-1e218905079a";
			this.GoodsItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GoodsItemsGrid.LayoutKey = "GoodsItemsGrid";
			this.GoodsItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemsGrid.Name = "GoodsItemsGrid";
			this.GoodsItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(593, 389, true);
			this.GoodsItemsGrid.TabIndex = 30;
			this.GoodsItemsGrid.SelectedRowsChangedInMouseDown += new System.EventHandler(this.GoodsItemsGrid_SelectedRowsChangedInMouseDown);
			// 
			// UnloadedGoodsItemsGrid
			// 
			this.UnloadedGoodsItemsGrid.AllowNavigation = false;
			this.UnloadedGoodsItemsGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.UnloadedGoodsItemsGrid, "UnloadingMovementHeader.GoodsItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).BY_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).IsChecked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).HasDifferences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).IsMissing)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).IsNew)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).UnloadingMovementHeader.GoodsItems)).SyncRoot)).UnloadingNotes)));
			this.UnloadedGoodsItemsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "BY_LineNo";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo1.ColumnName = "IsChecked";
			zCheckBoxColumnStyleInfo1.IsSortable = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "HasDifferences";
			zCheckBoxColumnStyleInfo2.IsSortable = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "IsMissing";
			zCheckBoxColumnStyleInfo3.IsSortable = false;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.ColumnName = "IsNew";
			zCheckBoxColumnStyleInfo4.IsSortable = false;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo4.ColumnName = "UnloadingNotes";
			zTextBoxColumnStyleInfo4.IsSortable = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.UnloadedGoodsItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.UnloadedGoodsItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.UnloadedGoodsItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.UnloadedGoodsItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.UnloadedGoodsItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.UnloadedGoodsItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.UnloadedGoodsItemsGrid.CopySelectedRowsAllowed = false;
			this.UnloadedGoodsItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadedGoodsItemsGrid.GridId = "e218cc11-6496-4d5c-805c-1e218905079a";
			this.UnloadedGoodsItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UnloadedGoodsItemsGrid.LayoutKey = "UnloadedGoodsItemsGrid";
			this.UnloadedGoodsItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnloadedGoodsItemsGrid.Name = "UnloadedGoodsItemsGrid";
			this.UnloadedGoodsItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 389, true);
			this.UnloadedGoodsItemsGrid.TabIndex = 31;
			this.UnloadedGoodsItemsGrid.RowsDeleting += new System.EventHandler<Enterprise.ZArchitecture.RowsDeletingEventArgs>(this.UnloadedGoodsItemsGrid_RowsDeleting);
			this.UnloadedGoodsItemsGrid.CurrentCellChanged += new System.EventHandler(this.UnloadedGoodsItemsGrid_CurrentCellChanged);
			// 
			// UnloadingItemDifferencesTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnloadingItemDifferencesSplitContainer);
			this.Name = "UnloadingItemDifferencesTabUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1297, 646, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnloadingItemDifferencesSplitContainer.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.UnloadingItemDifferencesSplitContainer)).EndInit();
			this.UnloadingItemDifferencesSplitContainer.ResumeLayout(false);
			this.UnloadingItemDifferencesSplitContainer.PerformLayout();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GoodsItemsGrid)).EndInit();
			this.GoodsItemsGrid.ResumeLayout(false);
			this.GoodsItemsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnloadedGoodsItemsGrid)).EndInit();
			this.UnloadedGoodsItemsGrid.ResumeLayout(false);
			this.UnloadedGoodsItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer UnloadingItemDifferencesSplitContainer;
		private UnloadedItemNewUserControl UnloadedItemNewUserControl;
		private UnloadedItemDetailsUserControl UnloadedItemDetailsUserControl;
		private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
		internal ZArchitecture.ZGrid GoodsItemsGrid;
		public ZArchitecture.ZGrid UnloadedGoodsItemsGrid;

	}
}

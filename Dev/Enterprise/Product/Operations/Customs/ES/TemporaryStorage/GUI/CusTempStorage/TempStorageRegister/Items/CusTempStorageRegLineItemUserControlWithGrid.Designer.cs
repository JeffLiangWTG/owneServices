namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class CusTempStorageRegLineItemUserControlWithGrid
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ItemsDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ItemDetailsLayoutControl = new Enterprise.Customs.ES.TemporaryStorage.GUI.CusTempStorageRegLineItemDetailsLayoutControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.ItemsDetailsGroupBox.SuspendLayout();
			this.ItemDetailsLayoutControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader);
			// 
			// SplitContainer
			//
			this.SplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| (System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right))));
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.SplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 250, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.ItemsGrid);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(70);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.ItemsDetailsGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 250, true);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(155);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			this.SplitContainer.TabIndex = 0;
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItemsGrid, "CusTempStorageRegLines.RegLineItemPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).RegLineItemPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).RegLineItemPivots)).SyncRoot)).RegLineItem.SRI_GoodsItemNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).RegLineItemPivots)).SyncRoot)).RegLineItem.FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).RegLineItemPivots)).SyncRoot)).RegLineItem.SRI_CusC4Number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).RegLineItemPivots)).SyncRoot)).RegLineItem.SRI_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).RegLineItemPivots)).SyncRoot)).SRV_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegLine)(((System.Collections.IList)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).CusTempStorageRegLines)).SyncRoot)).RegLineItemPivots)).SyncRoot)).RegLine.SRL_GrossWeightUQ)));
			this.ItemsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "RegLineItem+SRI_GoodsItemNumber";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "RegLineItem+FormattedTariff";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "RegLineItem+SRI_CusC4Number";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "RegLineItem+SRI_GoodsDescription";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "SRV_GrossWeight";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "RegLine+SRL_GrossWeightUQ";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemsGrid.GridId = "2F532AB6-3666-45F2-B519-2AE31CEE57E0";
			this.ItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItemsGrid.LayoutKey = "ItemsGrid";
			this.ItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemsGrid.Name = "ItemsGrid";
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 100, true);
			this.ItemsGrid.TabIndex = 0;
			// 
			// ItemsDetailsGroupBox
			// 
			this.ItemsDetailsGroupBox.Controls.Add(this.ItemDetailsLayoutControl);
			this.ItemsDetailsGroupBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("338B233F-2920-4006-8B3E-6EE82EC014E3", "Details");
			this.ItemsDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemsDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemsDetailsGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 90, true);
			this.ItemsDetailsGroupBox.Name = "ItemsDetailsGroupBox";
			this.ItemsDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 90, true);
			this.ItemsDetailsGroupBox.TabIndex = 0;
			this.ItemsDetailsGroupBox.TabStop = false;
			// 
			// ItemDetailsLayoutControl
			// 
			this.ItemDetailsLayoutControl.AllowDrop = true;
			this.ItemDetailsLayoutControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.ItemDetailsLayoutControl, ".");
			this.ItemDetailsLayoutControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemDetailsLayoutControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 10, true);
			this.ItemDetailsLayoutControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 80, true);
			this.ItemDetailsLayoutControl.Name = "ItemDetailsLayoutControl";
			this.ItemDetailsLayoutControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 80, true);
			this.ItemDetailsLayoutControl.TabIndex = 0;
			// 
			// CusTempStorageRegLineItemUserControlWithGrid
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 0, true);
			this.Name = "CusTempStorageRegLineItemUserControlWithGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 374, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.ItemsDetailsGroupBox.ResumeLayout(false);
			this.ItemsDetailsGroupBox.PerformLayout();
			this.ItemDetailsLayoutControl.ResumeLayout(true);
			this.ItemDetailsLayoutControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid ItemsGrid;
		internal CargoWise.Windows.UI.KSplitContainer SplitContainer;
		internal CusTempStorageRegLineItemDetailsLayoutControl ItemDetailsLayoutControl;
		internal ZArchitecture.GUI.ZGroupBox ItemsDetailsGroupBox;
	}
}

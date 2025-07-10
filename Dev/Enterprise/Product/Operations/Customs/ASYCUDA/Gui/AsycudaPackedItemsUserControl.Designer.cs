namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class AsycudaPackedItemsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.PackedItemDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.PackedItemDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackedItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DynamicPackedItemDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackedItemsGrid)).BeginInit();
			this.PackedItemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaPack);
			// 
			// PackedItemDetailsTabPage
			// 
			this.PackedItemDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 15, true);
			this.PackedItemDetailsTabPage.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackedItemDetailsTabPage.Name = "PackedItemDetailsTabPage";
			this.PackedItemDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackedItemDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 193, true);
			this.PackedItemDetailsTabPage.TabIndex = 0;
			this.PackedItemDetailsTabPage.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("32B693DC-65DC-45AA-8945-017E06232DA5", "Packed Item Details");
			this.PackedItemDetailsTabPage.UseVisualStyleBackColor = true;
			this.PackedItemDetailsTabPage.Controls.Add(DynamicPackedItemDetailsPanel);
			this.PackedItemDetailsTabPage.Controls.Add(ItemsGroupBox);
			// 
			// PackedItemDeatilsTabControl
			// 
			this.PackedItemDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PackedItemDetailsTabControl.Controls.Add(this.PackedItemDetailsTabPage);
			this.PackedItemDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackedItemDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackedItemDetailsTabControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackedItemDetailsTabControl.Name = "PackedItemDeatilsTabControl";
			this.PackedItemDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 212, true);
			this.PackedItemDetailsTabControl.TabIndex = 0;
			// 
			// ItemsGroupBox
			// 
			this.ItemsGroupBox.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("fdf7f3b5-42b4-42d0-86a4-e6769bb0d36c", "Items");
			this.ItemsGroupBox.Controls.Add(this.PackedItemsGrid);
			this.ItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemsGroupBox.Name = "ItemsGroupBox";
			this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 170, true);
			this.ItemsGroupBox.TabIndex = 1;
			this.ItemsGroupBox.TabStop = false;
			// 
			// PackedItemsGrid
			// 
			this.PackedItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackedItemsGrid, "PackedItemsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_CustomsQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_CustomsUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_CustomsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_RN_NKGoodsOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_GrossWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_GoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_RX_NKGoodsValueCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_DutyAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_PackStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(null)).PackedItemsForBinding)).SyncRoot)).API_TaxAmount)));
			this.PackedItemsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "API_FormattedTariff";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "API_CustomsQty";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("74a06f02-b9e4-4f5d-8f6b-30c00ec3244c", "Customs Qty");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "API_CustomsUQ";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("74a06f02-b9e4-4f5d-8f6b-30c00ec3244c", "Customs Qty");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "API_CustomsValue";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "API_RN_NKGoodsOrigin";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "API_GoodsDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "API_GrossWeight";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("339f5737-6e1e-45f0-afc5-047620aab5fd", "Gross Weight");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "API_GrossWeightUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("339f5737-6e1e-45f0-afc5-047620aab5fd", "Gross Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "API_NetWeight";
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("463d2db5-ead1-4a21-a503-9e615287f777", "Net Weight");
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "API_NetWeightUQ";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("463d2db5-ead1-4a21-a503-9e615287f777", "Net Weight");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "API_GoodsValue";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("a7d16b27-e191-4f6c-8bfc-5b2ed2cca02a", "Goods Value");
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo3.ColumnName = "API_RX_NKGoodsValueCurrency";
			zCodeFindBoxColumnStyleInfo3.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("a7d16b27-e191-4f6c-8bfc-5b2ed2cca02a", "Goods Value");
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "API_DutyAmount";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "API_MessageStatus";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "API_PackStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "API_TaxAmount";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.PackedItemsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PackedItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackedItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackedItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackedItemsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.PackedItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackedItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PackedItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PackedItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PackedItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PackedItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.PackedItemsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.PackedItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.PackedItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackedItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PackedItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.PackedItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackedItemsGrid.GridId = "a4915992-6a40-455d-a910-1946f7d4feb8";
			this.PackedItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackedItemsGrid.LayoutKey = "PackedItemsGrid";
			this.PackedItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.PackedItemsGrid.Name = "PackedItemsGrid";
			this.PackedItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 153, true);
			this.PackedItemsGrid.TabIndex = 0;
			//
			// DynamicPackedItemDetailsPanel
			// 
			this.DynamicPackedItemDetailsPanel.AllowDrop = true;
			this.DynamicPackedItemDetailsPanel.AutoScroll = true;
			this.DynamicPackedItemDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicPackedItemDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicPackedItemDetailsPanel.Name = "DynamicPackedItemPanel";
			this.DynamicPackedItemDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicPackedItemDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 142, true);
			this.DynamicPackedItemDetailsPanel.TabIndex = 1;
			// 
			// AsycudaPackedItemsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.AutoScroll = true;
			this.Controls.Add(this.PackedItemDetailsTabControl);
			this.Name = "AsycudaPackedItemsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackedItemsGrid)).EndInit();
			this.PackedItemsGrid.ResumeLayout(false);
			this.PackedItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox ItemsGroupBox;
		private Enterprise.ZArchitecture.ZGrid PackedItemsGrid;
		private Enterprise.ZArchitecture.GUI.DynamicLayoutPanel DynamicPackedItemDetailsPanel;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl PackedItemDetailsTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage PackedItemDetailsTabPage;
	}
}

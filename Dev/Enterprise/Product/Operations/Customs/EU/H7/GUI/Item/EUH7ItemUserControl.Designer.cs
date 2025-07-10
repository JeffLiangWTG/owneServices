namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7ItemUserControl
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ItemPackSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ItemDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ItemsGrid = new ZArchitecture.ZGrid();
			this.ItemTabControl = new Enterprise.Customs.EU.H7.GUI.EUH7ItemTabControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemPackSplitContainer)).BeginInit();
			this.ItemPackSplitContainer.Panel1.SuspendLayout();
			this.ItemPackSplitContainer.Panel2.SuspendLayout();
			this.ItemPackSplitContainer.SuspendLayout();
			this.ItemDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.ItemTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.H7.Business.AsycudaBill);
			// 
			// ItemPackSplitContainer
			// 
			this.ItemPackSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemPackSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemPackSplitContainer.Name = "ItemPackSplitContainer";
			this.ItemPackSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ItemPackSplitContainer.Panel1
			// 
			this.ItemPackSplitContainer.Panel1.Controls.Add(this.ItemDetailsGroupBox);
			this.ItemPackSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(961, 523, true);
			this.ItemPackSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			// 
			// ItemPackSplitContainer.Panel2
			// 
			this.ItemPackSplitContainer.Panel2.AutoScroll = true;
			this.ItemPackSplitContainer.Panel2.Controls.Add(this.ItemTabControl);
			this.ItemPackSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(280);
			this.ItemPackSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(273);
			this.ItemPackSplitContainer.TabIndex = 0;
			// 
			// ItemDetailsGroupBox
			// 
			this.ItemDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("bc393c11-b440-4cc4-949b-94691b872f23", "General Item Details");
			this.ItemDetailsGroupBox.Controls.Add(this.ItemsGrid);
			this.ItemDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemDetailsGroupBox.Name = "ItemDetailsGroupBox";
			this.ItemDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(961, 273, true);
			this.ItemDetailsGroupBox.TabIndex = 0;
			this.ItemDetailsGroupBox.TabStop = false;
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItemsGrid, "PackedItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_GoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_RX_NKGoodsValueCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_PackStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_RN_NKGoodsOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_CustomsQty2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_CustomsUQ2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_CustomsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_CustomsQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)).API_CustomsUQ)));
			this.ItemsGrid.CaptionVisible = false;
			tariffColumnStyleInfo1.ColumnName = "API_FormattedTariff";
			tariffColumnStyleInfo1.DefaultCollectionIndex = 0;
			tariffColumnStyleInfo1.NeedLoadParentDataGroup = true;
			tariffColumnStyleInfo1.SelectNomenclatureModes = null;
			tariffColumnStyleInfo1.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			tariffColumnStyleInfo1.TariffType = null;
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "API_GoodsValue";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Decimals = 2;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.EU.H7.GUI.Res.GetData("90a80f5e-aea4-44eb-8e9d-dc7259f51e32", "Intrinsic Value");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "API_RX_NKGoodsValueCurrency";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.EU.H7.GUI.Res.GetData("90a80f5e-aea4-44eb-8e9d-dc7259f51e32", "Intrinsic Value");
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo1.ColumnName = "API_GoodsDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			zTextBoxColumnStyleInfo2.ColumnName = "API_MessageStatus";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(163);
			zTextBoxColumnStyleInfo3.ColumnName = "API_PackStatus";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "API_RN_NKGoodsOrigin";
			zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "API_CustomsQty2";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			zCalcEditColumnStyleInfo3.ColumnName = "API_GrossWeight";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			zCalcEditColumnStyleInfo3.Decimals = 3;
			zCalcEditColumnStyleInfo4.ColumnName = "API_NetWeight";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			zCalcEditColumnStyleInfo4.Decimals = 3;
			zCalcEditColumnStyleInfo5.ColumnName = "API_CustomsValue";
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			zCalcEditColumnStyleInfo5.Decimals = 3;
			zCalcEditColumnStyleInfo6.ColumnName = "API_CustomsQty";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			zCalcEditColumnStyleInfo6.Decimals = 3;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "API_CustomsUQ2";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "API_GrossWeightUQ";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "API_NetWeightUQ";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "API_CustomsUQ";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo4.IsVisible = false;
			this.ItemsGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ItemsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.ItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemsGrid.GridId = "e25e21d3-f518-4e04-b7cd-23dc9b5b9ac5";
			this.ItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItemsGrid.LayoutKey = "ItemsGrid";
			this.ItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ItemsGrid.Name = "ItemsGrid";
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(955, 254, true);
			this.ItemsGrid.TabIndex = 0;
			// 
			// ItemTabControl
			// 
			this.ItemTabControl.AllowDrop = true;
			this.ItemTabControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.ItemTabControl, "PackedItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem)(((Enterprise.Customs.EU.H7.Business.AsycudaPackedItem)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).PackedItems)).SyncRoot)))));
			this.ItemTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemTabControl.Name = "ItemTabControl";
			this.ItemTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(961, 246, true);
			this.ItemTabControl.TabIndex = 0;
			// 
			// EUH7ItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ItemPackSplitContainer);
			this.Name = "EUH7ItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(961, 523, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ItemPackSplitContainer.Panel1.ResumeLayout(false);
			this.ItemPackSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ItemPackSplitContainer)).EndInit();
			this.ItemPackSplitContainer.ResumeLayout(false);
			this.ItemPackSplitContainer.PerformLayout();
			this.ItemDetailsGroupBox.ResumeLayout(false);
			this.ItemDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.ItemTabControl.ResumeLayout(true);
			this.ItemTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		ZArchitecture.GUI.ZGroupBox ItemDetailsGroupBox;
		ZArchitecture.ZGrid ItemsGrid;
		CargoWise.Windows.UI.KSplitContainer ItemPackSplitContainer;
		EUH7ItemTabControl ItemTabControl;
	}
}

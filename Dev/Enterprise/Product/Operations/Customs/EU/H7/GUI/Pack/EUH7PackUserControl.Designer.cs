using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7PackUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PacksSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.GeneralDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PacksGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackTabControl = new Enterprise.Customs.EU.H7.GUI.EUH7PackTabControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PacksSplitContainer)).BeginInit();
			this.PacksSplitContainer.Panel1.SuspendLayout();
			this.PacksSplitContainer.Panel2.SuspendLayout();
			this.PacksSplitContainer.SuspendLayout();
			this.GeneralDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PacksGrid)).BeginInit();
			this.PacksGrid.SuspendLayout();
			this.PackTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.H7.Business.AsycudaBill);
			//
			// PacksSplitContainer
			// 
			this.PacksSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PacksSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PacksSplitContainer.Name = "PacksSplitContainer";
			this.PacksSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// PacksSplitContainer.Panel1
			// 
			this.PacksSplitContainer.Panel1.Controls.Add(this.GeneralDetailsGroupBox);
			this.PacksSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(961, 400, true);
			this.PacksSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			// 
			// PacksSplitContainer.Panel2
			// 
			this.PacksSplitContainer.Panel2.AutoScroll = true;
			this.PacksSplitContainer.Panel2.Controls.Add(this.PackTabControl);
			this.PacksSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(280);
			this.PacksSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(273);
			this.PacksSplitContainer.TabIndex = 0;
			// 
			// GeneralDetailsGroupBox
			// 
			this.GeneralDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("e7cd1112-fcfe-4390-9d2c-688c5065d944", "General Pack Details");
			this.GeneralDetailsGroupBox.Controls.Add(this.PacksGrid);
			this.GeneralDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GeneralDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GeneralDetailsGroupBox.Name = "GeneralDetailsGroupBox";
			this.GeneralDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 165, true);
			this.GeneralDetailsGroupBox.TabIndex = 0;
			this.GeneralDetailsGroupBox.TabStop = false;
			// 
			// PacksGrid
			// 
			this.PacksGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PacksGrid, "Packs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).ContainerPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_PackQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_PackUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_CommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_MarksAndNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_VINNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).LinePrice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).LinePriceCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_LineNo)));
			this.PacksGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "APA_PackQty";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			zDropEditColumnStyleInfo1.ColumnName = "APA_PackUQ";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "APA_CommodityCode";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "APA_GoodsDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(163);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "APA_MarksAndNumbers";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "APA_Weight";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "APA_WeightUQ";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "APA_Volume";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "APA_VolumeUQ";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "APA_VINNumber";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "LinePrice";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.GroupName = Enterprise.Customs.EU.H7.GUI.Res.GetData("6eabaaa2-7c90-4849-a5bf-6c8df4c2b998", "Line Price");
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "LinePriceCurrency";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.EU.H7.GUI.Res.GetData("6eabaaa2-7c90-4849-a5bf-6c8df4c2b998", "Line Price");
			zCodeFindBoxColumnStyleInfo1.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.MaxLengthOverride = 3;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "APA_LineNo";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PacksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PacksGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PacksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PacksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PacksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PacksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PacksGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PacksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PacksGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PacksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PacksGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PacksGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PacksGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PacksGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PacksGrid.GridId = "798B3FE8-961E-406E-B0B5-4AAC75DEF524";
			this.PacksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PacksGrid.LayoutKey = "PacksGrid";
			this.PacksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PacksGrid.Name = "PacksGrid";
			this.PacksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 146, true);
			this.PacksGrid.TabIndex = 0;
			//
			// PackTabControl
			// 
			this.PackTabControl.AllowDrop = true;
			this.PackTabControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.PackTabControl, "Packs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((Enterprise.Customs.EU.H7.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).Packs)).SyncRoot)))));
			this.PackTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackTabControl.Name = "PackTabControl";
			this.PackTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(961, 246, true);
			this.PackTabControl.TabIndex = 0;
			// 
			// EUH7PackUserControl
			// 
			this.Controls.Add(this.PacksSplitContainer);
			this.Name = "EUH7PackUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 165, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PacksSplitContainer.Panel1.ResumeLayout(false);
			this.PacksSplitContainer.Panel2.ResumeLayout(false);
			this.PacksSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PacksSplitContainer)).EndInit();
			this.GeneralDetailsGroupBox.ResumeLayout(false);
			this.GeneralDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PacksGrid)).EndInit();
			this.PacksGrid.ResumeLayout(false);
			this.PacksGrid.PerformLayout();
			this.PackTabControl.ResumeLayout(true);
			this.PackTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZGroupBox GeneralDetailsGroupBox;
		ZGrid PacksGrid;
		CargoWise.Windows.UI.KSplitContainer PacksSplitContainer;
		EUH7PackTabControl PackTabControl;
	}
}

using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class AsycudaPackUserControl
	{
		#region InitializeComponent

		void InitializeComponent()
		{
            Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
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
            this.PackCountrySplitContainer = new CargoWise.Windows.UI.KSplitContainer();
            this.GeneralDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.PacksGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PackCountrySplitContainer)).BeginInit();
            this.PackCountrySplitContainer.Panel1.SuspendLayout();
            this.PackCountrySplitContainer.SuspendLayout();
            this.GeneralDetailsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PacksGrid)).BeginInit();
            this.PacksGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaBill);
            // 
            // PackCountrySplitContainer
            // 
            this.PackCountrySplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PackCountrySplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.PackCountrySplitContainer.Name = "PackCountrySplitContainer";
            this.PackCountrySplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // PackCountrySplitContainer.Panel1
            // 
            this.PackCountrySplitContainer.Panel1.Controls.Add(this.GeneralDetailsGroupBox);
            this.PackCountrySplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(961, 431, true);
            this.PackCountrySplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
            // 
            // PackCountrySplitContainer.Panel2
            // 
            this.PackCountrySplitContainer.Panel2.AutoScroll = true;
            this.PackCountrySplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
            this.PackCountrySplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(225);
            this.PackCountrySplitContainer.TabIndex = 0;
            // 
            // GeneralDetailsGroupBox
            // 
            this.GeneralDetailsGroupBox.Controls.Add(this.PacksGrid);
			this.GeneralDetailsGroupBox.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("2bb8f327-d5b6-444b-b5a0-45ac54ccd11d", "General Pack Details");
			this.GeneralDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GeneralDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.GeneralDetailsGroupBox.Name = "GeneralDetailsGroupBox";
            this.GeneralDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(961, 225, true);
            this.GeneralDetailsGroupBox.TabIndex = 0;
            this.GeneralDetailsGroupBox.TabStop = false;
            // 
            // PacksGrid
            // 
            this.PacksGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.PacksGrid, "Packs");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).ContainerPK)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_PackQty)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_PackUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_CommodityCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_GoodsDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_MarksAndNumbers)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_Weight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_WeightUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_Volume)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_VolumeUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_VINNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).LinePrice)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).LinePriceCurrency)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaPack)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).Packs)).SyncRoot)).APA_LineNo)));
            this.PacksGrid.CaptionVisible = false;
            zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("d3aca28e-0c44-4882-947f-55723de95b71", "Container");
            zGuidDropEditColumnStyleInfo1.ColumnName = "ContainerPK";
            zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("611cb91f-5710-4eab-94a4-6c5e7673f40f", "Quantity (on Pack)");
            zCalcEditColumnStyleInfo1.ColumnName = "APA_PackQty";
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("48ab2f76-2ec3-477b-9715-f840b2f0b51b", "Pack Unit");
            zDropEditColumnStyleInfo1.ColumnName = "APA_PackUQ";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("de5ac8d4-3465-4101-a5a7-8121d24c1715", "Commodity Code");
            zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo1.ColumnName = "APA_CommodityCode";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("24539c92-b4c3-43fb-9d9f-3627f174165a", "Goods\' Description (on Pack)");
            zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo2.ColumnName = "APA_GoodsDescription";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(163);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("cac9a328-ff18-480c-affc-bc2df77bf031", "Marks and Numbers (on Pack)");
            zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo3.ColumnName = "APA_MarksAndNumbers";
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("640c99cc-2a3a-4c2d-aedf-4f282fe2c19b", "Weight (on Pack)");
            zCalcEditColumnStyleInfo2.ColumnName = "APA_Weight";
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
            zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("ed28d287-cada-4acd-bf04-1988cbdd134c", "Weight Unit");
            zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo2.ColumnName = "APA_WeightUQ";
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("caec480e-c2c7-4cc6-bc72-0aff86e58f25", "Volume (on Pack)");
            zCalcEditColumnStyleInfo3.ColumnName = "APA_Volume";
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
            zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("b5082c98-221f-4b5e-9872-9dad8ad1411b", "Volume Unit");
            zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo3.ColumnName = "APA_VolumeUQ";
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82);
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("1b3a3fcc-2706-44cf-a1f3-578edd452505", "VIN Number");
            zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo4.ColumnName = "APA_VINNumber";
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("1F349460-EBD4-4B4D-B8A3-49186BCE1094", "Line Price");
            zCalcEditColumnStyleInfo4.ColumnName = "LinePrice";
            zCalcEditColumnStyleInfo4.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("90d26cd2-4909-47e2-9934-0a0b2a52632f", "Line Price");
            zCalcEditColumnStyleInfo4.IsVisible = false;
            zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("34769904-3629-4C91-8E95-20B9793A4175", "Currency");
            zCodeFindBoxColumnStyleInfo1.ColumnName = "LinePriceCurrency";
            zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("90d26cd2-4909-47e2-9934-0a0b2a52632f", "Line Price");
            zCodeFindBoxColumnStyleInfo1.IsVisible = false;
            zCodeFindBoxColumnStyleInfo1.MaxLengthOverride = 3;
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("8E67598A-6154-4053-9CEA-DFEB357C5990", "Seq. No.");
            zTextBoxColumnStyleInfo5.ColumnName = "APA_LineNo";
            zTextBoxColumnStyleInfo5.IsVisible = false;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.PacksGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
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
            this.PacksGrid.GridId = "CBF77BF9-CCE8-4153-893F-0E452166DA57";
            this.PacksGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.PacksGrid.LayoutKey = "PacksGrid";
            this.PacksGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.PacksGrid.Name = "PacksGrid";
            this.PacksGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(955, 206, true);
            this.PacksGrid.TabIndex = 0;
            // 
            // AsycudaPackUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.PackCountrySplitContainer);
            this.Name = "AsycudaPackUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(961, 431, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PackCountrySplitContainer.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PackCountrySplitContainer)).EndInit();
            this.PackCountrySplitContainer.ResumeLayout(false);
            this.PackCountrySplitContainer.PerformLayout();
            this.GeneralDetailsGroupBox.ResumeLayout(false);
            this.GeneralDetailsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PacksGrid)).EndInit();
            this.PacksGrid.ResumeLayout(false);
            this.PacksGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		ZGroupBox GeneralDetailsGroupBox;
		ZGrid PacksGrid;
		CargoWise.Windows.UI.KSplitContainer PackCountrySplitContainer;
	}
}

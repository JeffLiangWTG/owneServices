namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class EUICS2PackedItemDetailsUserControl
	{
		void InitializeComponent()
		{
			this.CusCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PostalValueCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.TypeOfGoodsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CusCodeFindBox.SuspendLayout();
			this.PostalValueCalcFindBox.SuspendLayout();
			this.TypeOfGoodsDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaPack);
			// 
			// CusCodeFindBox
			// 
			this.CusCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CusCodeFindBox, "PackedItem.API_ChemicalSubstanceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaPack)(null)).PackedItem.API_ChemicalSubstanceCode)));
			this.CusCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 18, true);
			this.CusCodeFindBox.Name = "CusCodeFindBox";
			this.CusCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CusCodeFindBox.ParentType = null;
			this.CusCodeFindBox.PreBoundMaxLength = 25;
			this.CusCodeFindBox.ShowDescriptionBox = false;
			this.CusCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.CusCodeFindBox.TabIndex = 0;
			// 
			// PostalValueCalcFindBox
			// 
			this.PostalValueCalcFindBox.AllowDrop = true;
			this.PostalValueCalcFindBox.BindToAmount = "PackedItem.API_GoodsValue";
			this.PostalValueCalcFindBox.BindToList = "PackedItem+Lookups+Currencies";
			this.PostalValueCalcFindBox.BindToUnit = "PackedItem.API_RX_NKGoodsValueCurrency";
			this.PostalValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.PostalValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 45, true);
			this.PostalValueCalcFindBox.Name = "PostalValueCalcFindBox";
			this.PostalValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.PostalValueCalcFindBox.TabIndex = 1;
			// 
			// TypeOfGoodsDropEdit
			// 
			this.TypeOfGoodsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeOfGoodsDropEdit, "PackedItem.API_TypeOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaPack)(null)).PackedItem.API_TypeOfGoods)));
			this.TypeOfGoodsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 72, true);
			this.TypeOfGoodsDropEdit.Name = "TypeOfGoodsDropEdit";
			this.TypeOfGoodsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TypeOfGoodsDropEdit.TabIndex = 2;
			// 
			// EUICS2PackedItemDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TypeOfGoodsDropEdit);
			this.Controls.Add(this.PostalValueCalcFindBox);
			this.Controls.Add(this.CusCodeFindBox);
			this.Name = "EUICS2PackedItemDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 345, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CusCodeFindBox.ResumeLayout(true);
			this.CusCodeFindBox.PerformLayout();
			this.PostalValueCalcFindBox.ResumeLayout(true);
			this.PostalValueCalcFindBox.PerformLayout();
			this.TypeOfGoodsDropEdit.ResumeLayout(true);
			this.TypeOfGoodsDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZCodeFindBox CusCodeFindBox;
		internal ZArchitecture.GUI.ZCalcFindBox PostalValueCalcFindBox;
		internal ZArchitecture.GUI.ZDropEdit TypeOfGoodsDropEdit;
	}
}

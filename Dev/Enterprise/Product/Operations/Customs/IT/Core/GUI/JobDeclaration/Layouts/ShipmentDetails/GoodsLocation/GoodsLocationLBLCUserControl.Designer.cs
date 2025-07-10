namespace Enterprise.Customs.IT.GUI
{
	partial class GoodsLocationLBLCUserControl
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
			this.LocationQualifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SubLocationOfGoodsAsCountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GoodsLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LocationOtherInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LocationQualifierDropEdit.SuspendLayout();
			this.SubLocationOfGoodsAsCountryFindBox.SuspendLayout();
			this.GoodsLocationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// LocationQualifierDropEdit
			// 
			this.LocationQualifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationQualifierDropEdit, "JE_LocationQualifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_LocationQualifier)));
			this.LocationQualifierDropEdit.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("d9731017-273E-4BB1-BF75-F929CF7A5933", "[30] Goods Location");
			this.LocationQualifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LocationQualifierDropEdit.Name = "LocationQualifierDropEdit";
			this.LocationQualifierDropEdit.PreBoundMaxLength = 2;
			this.LocationQualifierDropEdit.ShowDescriptionBox = false;
			this.LocationQualifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.LocationQualifierDropEdit.TabIndex = 0;
			// 
			// SubLocationOfGoodsAsCountryFindBox
			// 
			this.SubLocationOfGoodsAsCountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubLocationOfGoodsAsCountryFindBox, "ImportJE_SubLocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).ImportJE_SubLocationOfGoods)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).Lookups.SubLocationOfGoodsList)));
			this.SubLocationOfGoodsAsCountryFindBox.BindToList = "Lookups+SubLocationOfGoodsList";
			this.SubLocationOfGoodsAsCountryFindBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("3C517345-E80D-48DD-8325-9142D5D80255", "Country");
			this.SubLocationOfGoodsAsCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 0, true);
			this.SubLocationOfGoodsAsCountryFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.SubLocationOfGoodsAsCountryFindBox.Name = "SubLocationOfGoodsAsCountryFindBox";
			this.SubLocationOfGoodsAsCountryFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SubLocationOfGoodsAsCountryFindBox.ParentType = null;
			this.SubLocationOfGoodsAsCountryFindBox.PreBoundMaxLength = 2;
			this.SubLocationOfGoodsAsCountryFindBox.ShouldResize = false;
			this.SubLocationOfGoodsAsCountryFindBox.ShowDescriptionBox = false;
			this.SubLocationOfGoodsAsCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.SubLocationOfGoodsAsCountryFindBox.TabIndex = 1;
			// 
			// GoodsLocationDropEdit
			// 
			this.GoodsLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationDropEdit, "JE_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_LocationOfGoods)));
			this.GoodsLocationDropEdit.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("F7AE2DB3-131B-4F09-B3D8-27DEA2C07395", "Code");
			this.GoodsLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 0, true);
			this.GoodsLocationDropEdit.Name = "GoodsLocationDropEdit";
			this.GoodsLocationDropEdit.ShowDescriptionBox = false;
			this.GoodsLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.GoodsLocationDropEdit.TabIndex = 2;
			// 
			// LocationOtherInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocationOtherInformationTextBox, "ImportJE_LocationOtherInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).ImportJE_LocationOtherInformation)));
			this.LocationOtherInformationTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("19457558-B2B8-418B-A80B-DE746115CFF4", "Supplementary Code");
			this.LocationOtherInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 0, true);
			this.LocationOtherInformationTextBox.Name = "LocationOtherInformationTextBox";
			this.LocationOtherInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.LocationOtherInformationTextBox.TabIndex = 3;
			// 
			// GoodsLocationLBLCUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LocationQualifierDropEdit);
			this.Controls.Add(this.SubLocationOfGoodsAsCountryFindBox);
			this.Controls.Add(this.GoodsLocationDropEdit);
			this.Controls.Add(this.LocationOtherInformationTextBox);
			this.Name = "GoodsLocationLBLCUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LocationQualifierDropEdit.ResumeLayout(true);
			this.LocationQualifierDropEdit.PerformLayout();
			this.SubLocationOfGoodsAsCountryFindBox.ResumeLayout(true);
			this.SubLocationOfGoodsAsCountryFindBox.PerformLayout();
			this.GoodsLocationDropEdit.ResumeLayout(true);
			this.GoodsLocationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit LocationQualifierDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox SubLocationOfGoodsAsCountryFindBox;
		internal ZArchitecture.GUI.ZDropEdit GoodsLocationDropEdit;
		internal ZArchitecture.ZTextBox LocationOtherInformationTextBox;
	}
}

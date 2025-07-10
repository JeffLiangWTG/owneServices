namespace Enterprise.Customs.IT.GUI
{
	partial class GoodsLocationFCUserControl
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
			this.LocationOfGoodsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LocationQualifierDropEdit.SuspendLayout();
			this.SubLocationOfGoodsAsCountryFindBox.SuspendLayout();
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
			this.LocationQualifierDropEdit.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("14D8651B-43AE-435C-ACFC-BAD790F9571B", "[30] Goods Location");
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
			this.SubLocationOfGoodsAsCountryFindBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("6CFEA802-4859-46B3-AED5-1B0A0A37DFF7", "Country");
			this.SubLocationOfGoodsAsCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 0, true);
			this.SubLocationOfGoodsAsCountryFindBox.Name = "SubLocationOfGoodsAsCountryFindBox";
			this.SubLocationOfGoodsAsCountryFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SubLocationOfGoodsAsCountryFindBox.ParentType = null;
			this.SubLocationOfGoodsAsCountryFindBox.PreBoundMaxLength = 2;
			this.SubLocationOfGoodsAsCountryFindBox.ShouldResize = false;
			this.SubLocationOfGoodsAsCountryFindBox.ShowDescriptionBox = false;
			this.SubLocationOfGoodsAsCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.SubLocationOfGoodsAsCountryFindBox.TabIndex = 1;
			// 
			// LocationOfGoodsTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocationOfGoodsTextBox, "JE_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_LocationOfGoods)));
			this.LocationOfGoodsTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("691DF8EB-9AAC-432C-9AF6-5B6470D654CA", "Code");
			this.LocationOfGoodsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 0, true);
			this.LocationOfGoodsTextBox.Name = "LocationOfGoodsTextBox";
			this.LocationOfGoodsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.LocationOfGoodsTextBox.TabIndex = 2;
			// 
			// GoodsLocationFCUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LocationQualifierDropEdit);
			this.Controls.Add(this.SubLocationOfGoodsAsCountryFindBox);
			this.Controls.Add(this.LocationOfGoodsTextBox);
			this.Name = "GoodsLocationFCUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LocationQualifierDropEdit.ResumeLayout(true);
			this.LocationQualifierDropEdit.PerformLayout();
			this.SubLocationOfGoodsAsCountryFindBox.ResumeLayout(true);
			this.SubLocationOfGoodsAsCountryFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit LocationQualifierDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox SubLocationOfGoodsAsCountryFindBox;
		internal ZArchitecture.ZTextBox LocationOfGoodsTextBox;
	}
}

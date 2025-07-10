namespace Enterprise.Customs.IT.GUI
{
	partial class GoodsLocationDUserControl
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
			this.LocationOfGoodsAsCustomsOfficeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LocationQualifierDropEdit.SuspendLayout();
			this.LocationOfGoodsAsCustomsOfficeFindBox.SuspendLayout();
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
			// LocationOfGoodsAsCustomsOfficeFindBox
			// 
			this.LocationOfGoodsAsCustomsOfficeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationOfGoodsAsCustomsOfficeFindBox, "JE_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_LocationOfGoods)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).Lookups.CustomsOffices)));
			this.LocationOfGoodsAsCustomsOfficeFindBox.BindToList = "Lookups+CustomsOffices";
			this.LocationOfGoodsAsCustomsOfficeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 0, true);
			this.LocationOfGoodsAsCustomsOfficeFindBox.Name = "LocationOfGoodsAsCustomsOfficeFindBox";
			this.LocationOfGoodsAsCustomsOfficeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LocationOfGoodsAsCustomsOfficeFindBox.ParentType = null;
			this.LocationOfGoodsAsCustomsOfficeFindBox.PreBoundMaxLength = 6;
			this.LocationOfGoodsAsCustomsOfficeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.LocationOfGoodsAsCustomsOfficeFindBox.TabIndex = 1;
			// 
			// GoodsLocationDUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LocationQualifierDropEdit);
			this.Controls.Add(this.LocationOfGoodsAsCustomsOfficeFindBox);
			this.Name = "GoodsLocationDUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LocationQualifierDropEdit.ResumeLayout(true);
			this.LocationQualifierDropEdit.PerformLayout();
			this.LocationOfGoodsAsCustomsOfficeFindBox.ResumeLayout(true);
			this.LocationOfGoodsAsCustomsOfficeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit LocationQualifierDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox LocationOfGoodsAsCustomsOfficeFindBox;
	}
}

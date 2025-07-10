namespace Enterprise.Customs.GB.GUI
{
	partial class CDSLocationOfGoodsUserControl
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
			this.LocationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LocationQualifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CDSGoodsLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SubLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LocationTypeDropEdit.SuspendLayout();
			this.CountryFindBox.SuspendLayout();
			this.LocationQualifierDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.Declaration.JobDeclaration);
			// 
			// LocationTypeDropEdit
			// 
			this.LocationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationTypeDropEdit, "JE_Calc_LocationOtherInformationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).JE_Calc_LocationOtherInformationType)));
			this.LocationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 2, true);
			this.LocationTypeDropEdit.Name = "LocationTypeDropEdit";
			this.LocationTypeDropEdit.PreBoundMaxLength = 2;
			this.LocationTypeDropEdit.ShowDescriptionBox = false;
			this.LocationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 18, true);
			this.LocationTypeDropEdit.TabIndex = 2;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryFindBox, "JE_Calc_LocationOtherInformationCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).JE_Calc_LocationOtherInformationCountry)));
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(34, 2, true);
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.PreBoundMaxLength = 2;
			this.CountryFindBox.ShouldResize = true;
			this.CountryFindBox.ShowDescriptionBox = false;
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 18, true);
			this.CountryFindBox.TabIndex = 1;
			// 
			// LocationQualifierDropEdit
			// 
			this.LocationQualifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationQualifierDropEdit, "JE_LocationQualifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).JE_LocationQualifier)));
			this.LocationQualifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 2, true);
			this.LocationQualifierDropEdit.Name = "LocationQualifierDropEdit";
			this.LocationQualifierDropEdit.PreBoundMaxLength = 2;
			this.LocationQualifierDropEdit.ShowDescriptionBox = false;
			this.LocationQualifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 18, true);
			this.LocationQualifierDropEdit.TabIndex = 3;
			// 
			// CDSGoodsLocationDropEdit
			// 
			this.CDSGoodsLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CDSGoodsLocationDropEdit, "JE_GoodsLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).JE_GoodsLocation)));
			this.CDSGoodsLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(265, 2, true);
			this.CDSGoodsLocationDropEdit.Name = "CDSGoodsLocationDropEdit";
			this.CDSGoodsLocationDropEdit.PreBoundMaxLength = 9;
			this.CDSGoodsLocationDropEdit.ShowDescriptionBox = false;
			this.CDSGoodsLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 18, true);
			this.CDSGoodsLocationDropEdit.TabIndex = 4;
			// 
			// LocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocationTextBox, "JE_GoodsLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).JE_GoodsLocation)));
			this.LocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(265, 2, true);
			this.LocationTextBox.Name = "LocationTextBox";
			this.LocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 18, true);
			this.LocationTextBox.TabIndex = 4;
			// 
			// SubLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.SubLocationTextBox, "JE_SubLocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).JE_SubLocationOfGoods)));
			this.SubLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 2, true);
			this.SubLocationTextBox.Name = "SubLocationTextBox";
			this.SubLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 18, true);
			this.SubLocationTextBox.TabIndex = 5;
			// 
			// CDSLocationOfGoodsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SubLocationTextBox);
			this.Controls.Add(this.LocationTextBox);
			this.Controls.Add(this.CDSGoodsLocationDropEdit);
			this.Controls.Add(this.LocationQualifierDropEdit);
			this.Controls.Add(this.CountryFindBox);
			this.Controls.Add(this.LocationTypeDropEdit);
			this.Name = "CDSLocationOfGoodsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LocationTypeDropEdit.ResumeLayout(true);
			this.LocationTypeDropEdit.PerformLayout();
			this.CountryFindBox.ResumeLayout(true);
			this.CountryFindBox.PerformLayout();
			this.LocationQualifierDropEdit.ResumeLayout(true);
			this.LocationQualifierDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZDropEdit LocationTypeDropEdit;
		public ZArchitecture.GUI.ZCodeFindBox CountryFindBox;
		public ZArchitecture.GUI.ZDropEdit LocationQualifierDropEdit;
		public ZArchitecture.GUI.ZDropEdit CDSGoodsLocationDropEdit;
		public ZArchitecture.ZTextBox LocationTextBox;
		public ZArchitecture.ZTextBox SubLocationTextBox;
	}
}

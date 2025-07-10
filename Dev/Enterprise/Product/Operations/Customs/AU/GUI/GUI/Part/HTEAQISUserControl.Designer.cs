namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class HTEAQISUserControl
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
			this.AQISDetails = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QL_CategoryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QL_CutCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QL_PreservationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_PackTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_SupplimentaryCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QL_ProductTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QH_ProduceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AQISDetails.SuspendLayout();
			this.QL_CategoryCodeFindBox.SuspendLayout();
			this.QL_CutCodeCodeFindBox.SuspendLayout();
			this.QL_PreservationTypeDropEdit.SuspendLayout();
			this.QL_PackTypeDropEdit.SuspendLayout();
			this.QL_SupplimentaryCodeCodeFindBox.SuspendLayout();
			this.QL_ProductTypeCodeFindBox.SuspendLayout();
			this.QH_ProduceTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot);
			// 
			// AQISDetails
			// 
			this.AQISDetails.Controls.Add(this.QL_CategoryCodeFindBox);
			this.AQISDetails.Controls.Add(this.QL_CutCodeCodeFindBox);
			this.AQISDetails.Controls.Add(this.QL_PreservationTypeDropEdit);
			this.AQISDetails.Controls.Add(this.QL_PackTypeDropEdit);
			this.AQISDetails.Controls.Add(this.QL_SupplimentaryCodeCodeFindBox);
			this.AQISDetails.Controls.Add(this.QL_ProductTypeCodeFindBox);
			this.AQISDetails.Controls.Add(this.QH_ProduceTypeDropEdit);
			this.AQISDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AQISDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AQISDetails.Name = "AQISDetails";
			this.AQISDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 249, true);
			this.AQISDetails.TabIndex = 2;
			this.AQISDetails.TabStop = false;
			this.AQISDetails.Text = "RFP Details";
			// 
			// QL_CategoryCodeFindBox
			// 
			this.QL_CategoryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_CategoryCodeFindBox, "AddInfo.ZA_AQISCategoryCode_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(null)).AddInfo.ZA_AQISCategoryCode_Hidden)));
			this.QL_CategoryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 60, true);
			this.QL_CategoryCodeFindBox.Name = "QL_CategoryCodeFindBox";
			this.QL_CategoryCodeFindBox.ShouldResize = true;
			this.QL_CategoryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 18, true);
			this.QL_CategoryCodeFindBox.TabIndex = 10;
			// 
			// QL_CutCodeCodeFindBox
			// 
			this.QL_CutCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_CutCodeCodeFindBox, "AddInfo.ZA_AQISCutCode_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(null)).AddInfo.ZA_AQISCutCode_Hidden)));
			this.QL_CutCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 147, true);
			this.QL_CutCodeCodeFindBox.Name = "QL_CutCodeCodeFindBox";
			this.QL_CutCodeCodeFindBox.ShouldResize = true;
			this.QL_CutCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 18, true);
			this.QL_CutCodeCodeFindBox.TabIndex = 9;
			// 
			// QL_PreservationTypeDropEdit
			// 
			this.QL_PreservationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_PreservationTypeDropEdit, "AddInfo.ZA_AQISPreservation_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(null)).AddInfo.ZA_AQISPreservation_Hidden)));
			this.QL_PreservationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 126, true);
			this.QL_PreservationTypeDropEdit.Name = "QL_PreservationTypeDropEdit";
			this.QL_PreservationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 18, true);
			this.QL_PreservationTypeDropEdit.TabIndex = 8;
			// 
			// QL_PackTypeDropEdit
			// 
			this.QL_PackTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_PackTypeDropEdit, "AddInfo.ZA_AQISPackType_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(null)).AddInfo.ZA_AQISPackType_Hidden)));
			this.QL_PackTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 104, true);
			this.QL_PackTypeDropEdit.Name = "QL_PackTypeDropEdit";
			this.QL_PackTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 18, true);
			this.QL_PackTypeDropEdit.TabIndex = 7;
			// 
			// QL_SupplimentaryCodeCodeFindBox
			// 
			this.QL_SupplimentaryCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_SupplimentaryCodeCodeFindBox, "AddInfo.ZA_AQISSupplementaryCode_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(null)).AddInfo.ZA_AQISSupplementaryCode_Hidden)));
			this.QL_SupplimentaryCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 82, true);
			this.QL_SupplimentaryCodeCodeFindBox.Name = "QL_SupplimentaryCodeCodeFindBox";
			this.QL_SupplimentaryCodeCodeFindBox.ShouldResize = true;
			this.QL_SupplimentaryCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 18, true);
			this.QL_SupplimentaryCodeCodeFindBox.TabIndex = 4;
			// 
			// QL_ProductTypeCodeFindBox
			// 
			this.QL_ProductTypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_ProductTypeCodeFindBox, "AddInfo.ZA_AQISProduct_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(null)).AddInfo.ZA_AQISProduct_Hidden)));
			this.QL_ProductTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 38, true);
			this.QL_ProductTypeCodeFindBox.Name = "QL_ProductTypeCodeFindBox";
			this.QL_ProductTypeCodeFindBox.ShouldResize = true;
			this.QL_ProductTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 18, true);
			this.QL_ProductTypeCodeFindBox.TabIndex = 3;
			// 
			// QH_ProduceTypeDropEdit
			// 
			this.QH_ProduceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_ProduceTypeDropEdit, "AddInfo.ZA_AQISProduceType_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusClassPartPivot)(null)).AddInfo.ZA_AQISProduceType_Hidden)));
			this.QH_ProduceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 17, true);
			this.QH_ProduceTypeDropEdit.Name = "QH_ProduceTypeDropEdit";
			this.QH_ProduceTypeDropEdit.PreBoundMaxLength = 3;
			this.QH_ProduceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 18, true);
			this.QH_ProduceTypeDropEdit.TabIndex = 2;
			// 
			// HTEAQISUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AQISDetails);
			this.Name = "HTEAQISUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 249, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AQISDetails.ResumeLayout(false);
			this.AQISDetails.PerformLayout();
			this.QL_CategoryCodeFindBox.ResumeLayout(true);
			this.QL_CategoryCodeFindBox.PerformLayout();
			this.QL_CutCodeCodeFindBox.ResumeLayout(true);
			this.QL_CutCodeCodeFindBox.PerformLayout();
			this.QL_PreservationTypeDropEdit.ResumeLayout(true);
			this.QL_PreservationTypeDropEdit.PerformLayout();
			this.QL_PackTypeDropEdit.ResumeLayout(true);
			this.QL_PackTypeDropEdit.PerformLayout();
			this.QL_SupplimentaryCodeCodeFindBox.ResumeLayout(true);
			this.QL_SupplimentaryCodeCodeFindBox.PerformLayout();
			this.QL_ProductTypeCodeFindBox.ResumeLayout(true);
			this.QL_ProductTypeCodeFindBox.PerformLayout();
			this.QH_ProduceTypeDropEdit.ResumeLayout(true);
			this.QH_ProduceTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AQISDetails;
		private ZArchitecture.GUI.ZDropEdit QH_ProduceTypeDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox QL_ProductTypeCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox QL_SupplimentaryCodeCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox QL_CutCodeCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit QL_PreservationTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit QL_PackTypeDropEdit;
		private ZArchitecture.GUI.ZCodeFindBox QL_CategoryCodeFindBox;
	}
}

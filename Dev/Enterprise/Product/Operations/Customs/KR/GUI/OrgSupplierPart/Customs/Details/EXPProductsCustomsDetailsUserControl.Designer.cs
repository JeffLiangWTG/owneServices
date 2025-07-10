namespace Enterprise.Customs.KR.GUI
{
	partial class EXPProductsCustomsDetailsUserControl
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
			this.ModelTradeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrandNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IngredientTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UsageCommentTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.ClassificationDescriptionTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.GoodsOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.COOLabelLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UsageCommentTextBox.SuspendLayout();
			this.ClassificationDescriptionTextBox.SuspendLayout();
			this.GoodsOriginCodeFindBox.SuspendLayout();
			this.COOLabelLocationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.OrgSupplierPart);
			// 
			// ModelTradeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ModelTradeNameTextBox, "OP_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).OP_Model)));
			this.ModelTradeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 57, true);
			this.ModelTradeNameTextBox.Name = "ModelTradeNameTextBox";
			this.ModelTradeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 19, true);
			this.ModelTradeNameTextBox.TabIndex = 0;
			// 
			// BrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandNameTextBox, "OP_Brand");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).OP_Brand)));
			this.BrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 35, true);
			this.BrandNameTextBox.Name = "BrandNameTextBox";
			this.BrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 19, true);
			this.BrandNameTextBox.TabIndex = 1;
			// 
			// IngredientTextBox
			// 
			this.BindingSource.SetBindingMember(this.IngredientTextBox, "PivotsForBinding.KRClassification.CKR_Ingredient");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).KRClassification.CKR_Ingredient)));
			this.IngredientTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 80, true);
			this.IngredientTextBox.Name = "IngredientTextBox";
			this.IngredientTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 19, true);
			this.IngredientTextBox.TabIndex = 2;
			// 
			// UsageCommentTextControl
			// 
			this.UsageCommentTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UsageCommentTextBox, "PivotsForBinding.CI_UsageComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_UsageComment)));
			this.UsageCommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.UsageCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 103, true);
			this.UsageCommentTextBox.Name = "UsageCommentTextBox";
			this.UsageCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.UsageCommentTextBox.TabIndex = 5;
			// 
			// ClassificationDescriptionTextControl
			// 
			this.ClassificationDescriptionTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClassificationDescriptionTextBox, "PivotsForBinding.CI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_Description)));
			this.ClassificationDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ClassificationDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 127, true);
			this.ClassificationDescriptionTextBox.Name = "ClassificationDescriptionTextBox";
			this.ClassificationDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.ClassificationDescriptionTextBox.TabIndex = 6;
			// 
			// GoodsOriginCodeFindBox
			// 
			this.GoodsOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginCodeFindBox, "PivotsForBinding.CI_RN_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_RN_NKCountryOfOrigin)));
			this.GoodsOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("fb3f948d-ff2a-4094-95ad-def756cb189f", "Goods Origin");
			this.GoodsOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(378, 35, true);
			this.GoodsOriginCodeFindBox.Name = "GoodsOriginCodeFindBox";
			this.GoodsOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GoodsOriginCodeFindBox.ParentType = null;
			this.GoodsOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 19, true);
			this.GoodsOriginCodeFindBox.TabIndex = 7;
			// 
			// COOLabelLocationDropEdit
			// 
			this.COOLabelLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.COOLabelLocationDropEdit, "PivotsForBinding.KRClassification.CKR_COOLabelLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).KRClassification.CKR_COOLabelLocation)));
			this.COOLabelLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(378, 57, true);
			this.COOLabelLocationDropEdit.Name = "COOLabelLocationDropEdit";
			this.COOLabelLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 19, true);
			this.COOLabelLocationDropEdit.TabIndex = 8;
			// 
			// ProductsCustomsDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ModelTradeNameTextBox);
			this.Controls.Add(this.BrandNameTextBox);
			this.Controls.Add(this.IngredientTextBox);
			this.Controls.Add(this.UsageCommentTextBox);
			this.Controls.Add(this.ClassificationDescriptionTextBox);
			this.Controls.Add(this.GoodsOriginCodeFindBox);
			this.Controls.Add(this.COOLabelLocationDropEdit);
			this.Name = "ProductsCustomsDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 295, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UsageCommentTextBox.ResumeLayout(true);
			this.UsageCommentTextBox.PerformLayout();
			this.ClassificationDescriptionTextBox.ResumeLayout(true);
			this.ClassificationDescriptionTextBox.PerformLayout();
			this.GoodsOriginCodeFindBox.ResumeLayout(true);
			this.GoodsOriginCodeFindBox.PerformLayout();
			this.COOLabelLocationDropEdit.ResumeLayout(true);
			this.COOLabelLocationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		public ZArchitecture.ZTextBox ModelTradeNameTextBox;
		public ZArchitecture.ZTextBox BrandNameTextBox;
		public ZArchitecture.ZTextBox IngredientTextBox;
		public Customs.GUI.LongTextControl UsageCommentTextBox;
		public Customs.GUI.LongTextControl ClassificationDescriptionTextBox;
		public ZArchitecture.GUI.ZCodeFindBox GoodsOriginCodeFindBox;
		public ZArchitecture.GUI.ZDropEdit COOLabelLocationDropEdit;
	}
}

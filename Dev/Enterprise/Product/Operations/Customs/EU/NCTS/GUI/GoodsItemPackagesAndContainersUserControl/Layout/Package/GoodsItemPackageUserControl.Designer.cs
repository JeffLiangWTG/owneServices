namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class GoodsItemPackageUserControl
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
			this.PackageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NumberOfPackagesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MarksAndNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackageIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrandTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackageTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsPackage);
			// 
			// PackageTypeDropEdit
			// 
			this.PackageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageTypeDropEdit, "B5_UnitType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).B5_UnitType)));
			this.PackageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 25, true);
			this.PackageTypeDropEdit.Name = "PackageTypeDropEdit";
			this.PackageTypeDropEdit.PreBoundMaxLength = 8;
			this.PackageTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PackageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.PackageTypeDropEdit.TabIndex = 0;
			// 
			// NumberOfPackagesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NumberOfPackagesCalcEdit, "B5_UnitCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).B5_UnitCount)));
			this.NumberOfPackagesCalcEdit.CaptionResourceString = null;
			this.NumberOfPackagesCalcEdit.DecimalPlaces = 0;
			this.NumberOfPackagesCalcEdit.Decimals = 0;
			this.NumberOfPackagesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 62, true);
			this.NumberOfPackagesCalcEdit.Name = "NumberOfPackagesCalcEdit";
			this.NumberOfPackagesCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.NumberOfPackagesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.NumberOfPackagesCalcEdit.TabIndex = 1;
			this.NumberOfPackagesCalcEdit.Text = "0";
			this.NumberOfPackagesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.MarksAndNumbersTextBox, "B5_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).B5_MarksAndNumbers)));
			this.MarksAndNumbersTextBox.CaptionResourceString = null;
			this.MarksAndNumbersTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 106, true);
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.MarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.MarksAndNumbersTextBox.TabIndex = 2;
			// 
			// PackageIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.PackageIDTextBox, "B5_PackageID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).B5_PackageID)));
			this.PackageIDTextBox.CaptionResourceString = null;
			this.PackageIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PackageIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 104, true);
			this.PackageIDTextBox.Name = "PackageIDTextBox";
			this.PackageIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.PackageIDTextBox.TabIndex = 3;
			// 
			// BrandTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandTextBox, "B5_Brand");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).B5_Brand)));
			this.BrandTextBox.CaptionResourceString = null;
			this.BrandTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BrandTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 104, true);
			this.BrandTextBox.Name = "BrandTextBox";
			this.BrandTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.BrandTextBox.TabIndex = 4;
			// 
			// ModelTextBox
			// 
			this.BindingSource.SetBindingMember(this.ModelTextBox, "B5_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).B5_Model)));
			this.ModelTextBox.CaptionResourceString = null;
			this.ModelTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 104, true);
			this.ModelTextBox.Name = "ModelTextBox";
			this.ModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.ModelTextBox.TabIndex = 5;
			// 
			// GoodsItemPackageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackageTypeDropEdit);
			this.Controls.Add(this.MarksAndNumbersTextBox);
			this.Controls.Add(this.NumberOfPackagesCalcEdit);
			this.Controls.Add(this.PackageIDTextBox);
			this.Controls.Add(this.BrandTextBox);
			this.Controls.Add(this.ModelTextBox);
			this.Name = "GoodsItemPackageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 153, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackageTypeDropEdit.ResumeLayout(true);
			this.PackageTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZCalcEdit NumberOfPackagesCalcEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit PackageTypeDropEdit;
		internal ZArchitecture.ZTextBox MarksAndNumbersTextBox;
		internal ZArchitecture.ZTextBox PackageIDTextBox;
		internal ZArchitecture.ZTextBox BrandTextBox;
		internal ZArchitecture.ZTextBox ModelTextBox;
	}
}

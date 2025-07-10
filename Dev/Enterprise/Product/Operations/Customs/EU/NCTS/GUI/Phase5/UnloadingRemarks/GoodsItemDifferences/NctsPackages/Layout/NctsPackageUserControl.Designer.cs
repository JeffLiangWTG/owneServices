namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class NctsPackageUserControl
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
			this.SequenceNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnitCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnitTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MarksAndNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackageIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrandTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DifUnitCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DifUnitTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DifMarksAndNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DifPackageIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DifBrandTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DifModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PlaceHolder1Label = new Enterprise.ZArchitecture.ZLabel();
			this.DeclaredValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UnloadedValueLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnitTypeDropEdit.SuspendLayout();
			this.DifUnitTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsPackage);
			// 
			// SequenceNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SequenceNumberCalcEdit, "B5_SequenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).B5_SequenceNumber)));
			this.SequenceNumberCalcEdit.CaptionResourceString = null;
			this.SequenceNumberCalcEdit.DecimalPlaces = 0;
			this.SequenceNumberCalcEdit.Decimals = 0;
			this.SequenceNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 3, true);
			this.SequenceNumberCalcEdit.Name = "SequenceNumberCalcEdit";
			this.SequenceNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.SequenceNumberCalcEdit.TabIndex = 0;
			this.SequenceNumberCalcEdit.Text = "0";
			this.SequenceNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnitCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.UnitCountCalcEdit, "B5_UnitCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).B5_UnitCount)));
			this.UnitCountCalcEdit.CaptionResourceString = null;
			this.UnitCountCalcEdit.DecimalPlaces = 0;
			this.UnitCountCalcEdit.Decimals = 0;
			this.UnitCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 78, true);
			this.UnitCountCalcEdit.Name = "UnitCountCalcEdit";
			this.UnitCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.UnitCountCalcEdit.TabIndex = 2;
			this.UnitCountCalcEdit.Text = "0";
			this.UnitCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnitTypeDropEdit
			// 
			this.UnitTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnitTypeDropEdit, "B5_UnitType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).B5_UnitType)));
			this.UnitTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 52, true);
			this.UnitTypeDropEdit.Name = "UnitTypeDropEdit";
			this.UnitTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.UnitTypeDropEdit.TabIndex = 3;
			// 
			// MarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.MarksAndNumbersTextBox, "B5_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).B5_MarksAndNumbers)));
			this.MarksAndNumbersTextBox.CaptionResourceString = null;
			this.MarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 104, true);
			this.MarksAndNumbersTextBox.Multiline = true;
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 50, true);
			this.MarksAndNumbersTextBox.TabIndex = 4;
			// 
			// PackageIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.PackageIDTextBox, "B5_PackageID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).B5_PackageID)));
			this.PackageIDTextBox.CaptionResourceString = null;
			this.PackageIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 104, true);
			this.PackageIDTextBox.Name = "PackageIDTextBox";
			this.PackageIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.PackageIDTextBox.TabIndex = 9;
			// 
			// BrandTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandTextBox, "B5_Brand");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).B5_Brand)));
			this.BrandTextBox.CaptionResourceString = null;
			this.BrandTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 104, true);
			this.BrandTextBox.Name = "BrandTextBox";
			this.BrandTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.BrandTextBox.TabIndex = 10;
			// 
			// ModelTextBox
			// 
			this.BindingSource.SetBindingMember(this.ModelTextBox, "B5_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).B5_Model)));
			this.ModelTextBox.CaptionResourceString = null;
			this.ModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 104, true);
			this.ModelTextBox.Name = "ModelTextBox";
			this.ModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.ModelTextBox.TabIndex = 11;
			// 
			// DifUnitCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DifUnitCountCalcEdit, "PackDifference.B5_UnitCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).PackDifference.B5_UnitCount)));
			this.DifUnitCountCalcEdit.CaptionResourceString = null;
			this.DifUnitCountCalcEdit.DecimalPlaces = 0;
			this.DifUnitCountCalcEdit.Decimals = 0;
			this.DifUnitCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 78, true);
			this.DifUnitCountCalcEdit.Name = "DifUnitCountCalcEdit";
			this.DifUnitCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.DifUnitCountCalcEdit.TabIndex = 6;
			this.DifUnitCountCalcEdit.Text = "0";
			this.DifUnitCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DifUnitTypeDropEdit
			// 
			this.DifUnitTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DifUnitTypeDropEdit, "PackDifference.B5_UnitType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).PackDifference.B5_UnitType)));
			this.DifUnitTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 52, true);
			this.DifUnitTypeDropEdit.Name = "DifUnitTypeDropEdit";
			this.DifUnitTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.DifUnitTypeDropEdit.TabIndex = 7;
			// 
			// DifMarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.DifMarksAndNumbersTextBox, "PackDifference.B5_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).PackDifference.B5_MarksAndNumbers)));
			this.DifMarksAndNumbersTextBox.CaptionResourceString = null;
			this.DifMarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 104, true);
			this.DifMarksAndNumbersTextBox.Multiline = true;
			this.DifMarksAndNumbersTextBox.Name = "DifMarksAndNumbersTextBox";
			this.DifMarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 50, true);
			this.DifMarksAndNumbersTextBox.TabIndex = 8;
			// 
			// DifPackageIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.DifPackageIDTextBox, "PackDifference.B5_PackageID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).PackDifference.B5_PackageID)));
			this.DifPackageIDTextBox.CaptionResourceString = null;
			this.DifPackageIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 104, true);
			this.DifPackageIDTextBox.Name = "DifPackageIDTextBox";
			this.DifPackageIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.DifPackageIDTextBox.TabIndex = 12;
			// 
			// DifBrandTextBox
			// 
			this.BindingSource.SetBindingMember(this.DifBrandTextBox, "PackDifference.B5_Brand");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).PackDifference.B5_Brand)));
			this.DifBrandTextBox.CaptionResourceString = null;
			this.DifBrandTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 104, true);
			this.DifBrandTextBox.Name = "DifBrandTextBox";
			this.DifBrandTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.DifBrandTextBox.TabIndex = 13;
			// 
			// DifModelTextBox
			// 
			this.BindingSource.SetBindingMember(this.DifModelTextBox, "PackDifference.B5_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).PackDifference.B5_Model)));
			this.DifModelTextBox.CaptionResourceString = null;
			this.DifModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 104, true);
			this.DifModelTextBox.Name = "DifModelTextBox";
			this.DifModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.DifModelTextBox.TabIndex = 14;
			// 
			// PlaceHolder1Label
			// 
			this.PlaceHolder1Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PlaceHolder1Label, false);
			this.PlaceHolder1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 21, true);
			this.PlaceHolder1Label.Name = "PlaceHolder1Label";
			this.PlaceHolder1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 23, true);
			this.PlaceHolder1Label.TabIndex = 0;
			// 
			// DeclaredValueLabel
			// 
			this.DeclaredValueLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("878364CE-B7C2-4596-B010-9A6EF8822403", "Declared Value");
			this.DeclaredValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DeclaredValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 26, true);
			this.DeclaredValueLabel.Name = "DeclaredValueLabel";
			this.DeclaredValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 23, true);
			this.DeclaredValueLabel.TabIndex = 0;
			// 
			// UnloadedValueLabel
			// 
			this.UnloadedValueLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("99A712C9-3DC2-4144-8382-18F656677438", "Unloaded Value");
			this.UnloadedValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UnloadedValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 52, true);
			this.UnloadedValueLabel.Name = "UnloadedValueLabel";
			this.UnloadedValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 23, true);
			this.UnloadedValueLabel.TabIndex = 0;
			// 
			// NctsPackageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ModelTextBox);
			this.Controls.Add(this.BrandTextBox);
			this.Controls.Add(this.PackageIDTextBox);
			this.Controls.Add(this.MarksAndNumbersTextBox);
			this.Controls.Add(this.UnitTypeDropEdit);
			this.Controls.Add(this.UnitCountCalcEdit);
			this.Controls.Add(this.SequenceNumberCalcEdit);
			this.Controls.Add(this.DifModelTextBox);
			this.Controls.Add(this.DifBrandTextBox);
			this.Controls.Add(this.DifPackageIDTextBox);
			this.Controls.Add(this.DifMarksAndNumbersTextBox);
			this.Controls.Add(this.DifUnitTypeDropEdit);
			this.Controls.Add(this.DifUnitCountCalcEdit);
			this.Controls.Add(this.PlaceHolder1Label);
			this.Controls.Add(this.UnloadedValueLabel);
			this.Controls.Add(this.DeclaredValueLabel);
			this.Name = "NctsPackageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 262, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnitTypeDropEdit.ResumeLayout(true);
			this.UnitTypeDropEdit.PerformLayout();
			this.DifUnitTypeDropEdit.ResumeLayout(true);
			this.DifUnitTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZCalcEdit SequenceNumberCalcEdit;
		internal ZArchitecture.ZCalcEdit UnitCountCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit UnitTypeDropEdit;
		internal ZArchitecture.ZTextBox MarksAndNumbersTextBox;
		internal ZArchitecture.ZTextBox PackageIDTextBox;
		internal ZArchitecture.ZTextBox BrandTextBox;
		internal ZArchitecture.ZTextBox ModelTextBox;
		internal ZArchitecture.ZCalcEdit DifUnitCountCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit DifUnitTypeDropEdit;
		internal ZArchitecture.ZTextBox DifMarksAndNumbersTextBox;
		internal ZArchitecture.ZTextBox DifPackageIDTextBox;
		internal ZArchitecture.ZTextBox DifBrandTextBox;
		internal ZArchitecture.ZTextBox DifModelTextBox;
		internal ZArchitecture.ZLabel PlaceHolder1Label;
		internal ZArchitecture.ZLabel DeclaredValueLabel;
		internal ZArchitecture.ZLabel UnloadedValueLabel;
	}
}

using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI
{
	partial class NRCanEEFUserControl
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
			this.modelNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.brandNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.tradeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.countryOfOriginFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.stateOfOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.categoryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.intendedUseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.detailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.countryOfOriginFindBox.SuspendLayout();
			this.stateOfOriginDropEdit.SuspendLayout();
			this.intendedUseDropEdit.SuspendLayout();
			this.detailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.NRCanPGAHeader);
			// 
			// modelNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.modelNameTextBox, "InvoiceLine.JI_Model");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.JI_Model)));
			this.modelNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("F22E0958-9E05-45C7-AD2C-FE064B29F9C2", "Model Name");
			this.modelNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 93, true);
			this.modelNameTextBox.Name = "modelNameTextBox";
			this.modelNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
			this.modelNameTextBox.TabIndex = 3;
			// 
			// brandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.brandNameTextBox, "InvoiceLine.JI_BrandName");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.JI_BrandName)));
			this.brandNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("80EE5C07-06FC-4FDD-A30A-2F4C98B6CBC7", "Brand Name");
			this.brandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 68, true);
			this.brandNameTextBox.Name = "brandNameTextBox";
			this.brandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
			this.brandNameTextBox.TabIndex = 2;
			// 
			// tradeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.tradeNameTextBox, "InvoiceLine.CA_TradeName");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.CA_TradeName)));
			this.tradeNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ECE72939-6616-4338-9273-F437E9317841", "Trade Name");
			this.tradeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 42, true);
			this.tradeNameTextBox.Name = "tradeNameTextBox";
			this.tradeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
			this.tradeNameTextBox.TabIndex = 1;
			// 
			// countryOfOriginFindBox
			// 
			this.BindingSource.SetBindingMember(this.countryOfOriginFindBox, "RN_NKCountryOfOrigin");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).RN_NKCountryOfOrigin)));
			this.countryOfOriginFindBox.AllowDrop = true;
			this.countryOfOriginFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6AD7438F-D020-4AED-8889-FD1E8826BEC1", "Ctry/Rgn. Of Origin");
			this.countryOfOriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(525, 42, true);
			this.countryOfOriginFindBox.Name = "countryOfOriginFindBox";
			this.countryOfOriginFindBox.PreBoundMaxLength = 3;
			this.countryOfOriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 17, true);
			this.countryOfOriginFindBox.TabIndex = 5;
			// 
			// stateOfOriginDropEdit
			// 
			this.BindingSource.SetBindingMember(this.stateOfOriginDropEdit, "RW_NKOriginState");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).RW_NKOriginState)));
			this.stateOfOriginDropEdit.AllowDrop = true;
			this.stateOfOriginDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CA39A388-DFF6-4F72-BBF2-08202020C8F2", "State of Origin");
			this.stateOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(525, 68, true);
			this.stateOfOriginDropEdit.Name = "stateOfOriginDropEdit";
			this.stateOfOriginDropEdit.PreBoundMaxLength = 3;
			this.stateOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 17, true);
			this.stateOfOriginDropEdit.TabIndex = 6;
			// 
			// categoryCheckBox
			// 
			this.categoryCheckBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.categoryCheckBox, "CA_IsNotRegulatedByOEE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).CA_IsNotRegulatedByOEE)));
			this.categoryCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4B801C94-A62F-4DDE-8396-4FCEABA9C297", "Excluded from Regulation");
			this.categoryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.categoryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(525, 15, true);
			this.categoryCheckBox.Name = "categoryCheckBox";
			this.categoryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 19, true);
			this.categoryCheckBox.TabIndex = 4;
			// 
			// intendedUseDropEdit
			// 
			this.intendedUseDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.intendedUseDropEdit, "CA_IntendedUseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).CA_IntendedUseCode)));
			this.intendedUseDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("71E4FEDE-A7CB-44A4-8A14-655BC0A53FCA", "Intended Use Code");
			this.intendedUseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 17, true);
			this.intendedUseDropEdit.Name = "intendedUseDropEdit";
			this.intendedUseDropEdit.PreBoundMaxLength = 4;
			this.intendedUseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
			this.intendedUseDropEdit.TabIndex = 0;
			// 
			// detailsGroupBox
			// 
			this.detailsGroupBox.AutoSize = true;
			this.detailsGroupBox.Controls.Add(this.modelNameTextBox);
			this.detailsGroupBox.Controls.Add(this.brandNameTextBox);
			this.detailsGroupBox.Controls.Add(this.tradeNameTextBox);
			this.detailsGroupBox.Controls.Add(this.countryOfOriginFindBox);
			this.detailsGroupBox.Controls.Add(this.stateOfOriginDropEdit);
			this.detailsGroupBox.Controls.Add(this.categoryCheckBox);
			this.detailsGroupBox.Controls.Add(this.intendedUseDropEdit);
			this.detailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsGroupBox.Name = "detailsGroupBox";
			this.detailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1935, 681, true);
			this.detailsGroupBox.TabIndex = 0;
			this.detailsGroupBox.TabStop = false;
			// 
			// NRCanEEFUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.Controls.Add(this.detailsGroupBox);
			this.Name = "NRCanEEFUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1935, 681, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.countryOfOriginFindBox.ResumeLayout(true);
			this.countryOfOriginFindBox.PerformLayout();
			this.stateOfOriginDropEdit.ResumeLayout(true);
			this.stateOfOriginDropEdit.PerformLayout();
			this.intendedUseDropEdit.ResumeLayout(true);
			this.intendedUseDropEdit.PerformLayout();
			this.detailsGroupBox.ResumeLayout(false);
			this.detailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.ZTextBox modelNameTextBox;
		protected ZArchitecture.ZTextBox brandNameTextBox;
		private ZArchitecture.ZTextBox tradeNameTextBox;
		private ZArchitecture.GUI.ZCodeFindBox countryOfOriginFindBox;
		private ZArchitecture.GUI.ZDropEdit stateOfOriginDropEdit;
		private ZArchitecture.GUI.ZCheckBox categoryCheckBox;
		private ZArchitecture.GUI.ZDropEdit intendedUseDropEdit;
		private ZArchitecture.GUI.ZGroupBox detailsGroupBox;

	}
}

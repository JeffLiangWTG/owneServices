using System.Windows.Forms;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI
{
	partial class ABIProgramUserControl
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
			this.ManufacturerUserControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.HarvestingPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CountryOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CommonNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TradeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			this.IntendedUseCodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IntendedUseFoodCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseAquacultureCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseResearchAndDevelopmentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseSpecialtyChemicalProductionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseEnvironmentalApplicationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseOtherCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseOrnamentalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseEducationalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TSNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SexGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SexOtherCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SexSterileCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SexHermaphroditeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SexMaleCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SexFemaleCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LifeStageAdultCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStageEmbryoCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStagePropagateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStageJuvenileCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GenusOrSpeciesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GeneOrNucleotideSequenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GeneticModificationDescriptionTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NewSubstancesNotificationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GeneticModificationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ManufacturerUserControl.SuspendLayout();
			this.HarvestingPartyAddressControl.SuspendLayout();
			this.CountryOfOriginCodeFindBox.SuspendLayout();
			this.IntendedUseCodeGroupBox.SuspendLayout();
			this.CategoryDropEdit.SuspendLayout();
			this.SexGroupBox.SuspendLayout();
			this.LifeStageGroupBox.SuspendLayout();
			this.GeneticModificationDescriptionTextBox.SuspendLayout();
			this.MainGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.DFOPGAHeader);
			// 
			// ManufacturerUserControl
			// 
			this.ManufacturerUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerUserControl, "OA_Manufacturer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).OA_Manufacturer)));
			this.ManufacturerUserControl.BindToOrgList = "RequirementsParent.ManufacturersLookup";
			this.ManufacturerUserControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5c2e60ab-05a4-4f04-8975-009e3df2b52a", "Manufacturer");
			this.ManufacturerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 141, true);
			this.ManufacturerUserControl.Name = "ManufacturerUserControl";
			this.ManufacturerUserControl.PopupCaption = "";
			this.ManufacturerUserControl.ReadOnly = false;
			this.ManufacturerUserControl.ShowAddress = false;
			this.ManufacturerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ManufacturerUserControl.TabIndex = 6;
			// 
			// HarvestingPartyAddressControl
			// 
			this.HarvestingPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HarvestingPartyAddressControl, "CA_OA_HarvestingParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_OA_HarvestingParty)));
			this.HarvestingPartyAddressControl.BindToOrgList = "AddInfoLookups+HarvestingParties";
			this.HarvestingPartyAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7043c4c0-60b9-40fb-85b1-ea97bfe78f99", "Harvesting Party");
			this.HarvestingPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 167, true);
			this.HarvestingPartyAddressControl.Name = "HarvestingPartyAddressControl";
			this.HarvestingPartyAddressControl.PopupCaption = "";
			this.HarvestingPartyAddressControl.ReadOnly = false;
			this.HarvestingPartyAddressControl.ShowAddress = false;
			this.HarvestingPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.HarvestingPartyAddressControl.TabIndex = 7;
			// 
			// CountryOfOriginCodeFindBox
			// 
			this.CountryOfOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfOriginCodeFindBox, "RN_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).RN_NKCountryOfOrigin)));
			this.CountryOfOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("26d14218-5384-4318-b4a6-19097bc2f613", "Ctry/Rgn. Of Origin");
			this.CountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 115, true);
			this.CountryOfOriginCodeFindBox.Name = "CountryOfOriginCodeFindBox";
			this.CountryOfOriginCodeFindBox.PreBoundMaxLength = 3;
			this.CountryOfOriginCodeFindBox.ShowDescriptionBox = false;
			this.CountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.CountryOfOriginCodeFindBox.TabIndex = 4;
			// 
			// CommonNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommonNameTextBox, "InvoiceLine.JI_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).InvoiceLine.JI_Model)));
			this.CommonNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8abfaa04-7f9a-4fc4-be43-0728d563bdf2", "Model/Common Name");
			this.CommonNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 11, true);
			this.CommonNameTextBox.Name = "CommonNameTextBox";
			this.CommonNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CommonNameTextBox.TabIndex = 0;
			// 
			// TradeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.TradeNameTextBox, "InvoiceLine.CA_TradeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).InvoiceLine.CA_TradeName)));
			this.TradeNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("614da047-801f-4f35-bd71-b92042bb13c9", "Trade Mark Name");
			this.TradeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(657, 89, true);
			this.TradeNameTextBox.Name = "TradeNameTextBox";
			this.TradeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.TradeNameTextBox.TabIndex = 11;
			// 
			// CountIntEdit
			// 
			this.BindingSource.SetBindingMember(this.CountIntEdit, "CA_Count");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_Count)));
			this.CountIntEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("91f7db17-b5a3-4e3e-8956-e1633c09acb8", "Count");
			this.CountIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 89, true);
			this.CountIntEdit.Name = "CountIntEdit";
			this.CountIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.CountIntEdit.TabIndex = 3;
			this.CountIntEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IntendedUseCodeGroupBox
			// 
			this.IntendedUseCodeGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d2758e33-c303-4bdc-aef9-9f1668d1de6b", "Intended Use Code");
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseFoodCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseAquacultureCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseResearchAndDevelopmentCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseSpecialtyChemicalProductionCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseEnvironmentalApplicationCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseOtherCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseOrnamentalCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseEducationalCheckBox);
			this.IntendedUseCodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IntendedUseCodeGroupBox.Name = "IntendedUseCodeGroupBox";
			this.IntendedUseCodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 94, true);
			this.IntendedUseCodeGroupBox.TabIndex = 0;
			this.IntendedUseCodeGroupBox.TabStop = false;
			this.IntendedUseCodeGroupBox.Text = "Intended Use Code";
			// 
			// IntendedUseFoodCheckBox
			// 
			this.IntendedUseFoodCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseFoodCheckBox, "CA_IUF");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IUF)));
			this.IntendedUseFoodCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseFoodCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 22, true);
			this.IntendedUseFoodCheckBox.Name = "IntendedUseFoodCheckBox";
			this.IntendedUseFoodCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			this.IntendedUseFoodCheckBox.TabIndex = 0;
			this.IntendedUseFoodCheckBox.Text = "Food";
			this.IntendedUseFoodCheckBox.UseVisualStyleBackColor = true;
			// 
			// IntendedUseAquacultureCheckBox
			// 
			this.IntendedUseAquacultureCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseAquacultureCheckBox, "CA_IUA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IUA)));
			this.IntendedUseAquacultureCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseAquacultureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 22, true);
			this.IntendedUseAquacultureCheckBox.Name = "IntendedUseAquacultureCheckBox";
			this.IntendedUseAquacultureCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.IntendedUseAquacultureCheckBox.TabIndex = 1;
			this.IntendedUseAquacultureCheckBox.Text = "Aquaculture";
			this.IntendedUseAquacultureCheckBox.UseVisualStyleBackColor = true;
			// 
			// IntendedUseResearchAndDevelopmentCheckBox
			// 
			this.IntendedUseResearchAndDevelopmentCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseResearchAndDevelopmentCheckBox, "CA_IURAD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IURAD)));
			this.IntendedUseResearchAndDevelopmentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseResearchAndDevelopmentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 45, true);
			this.IntendedUseResearchAndDevelopmentCheckBox.Name = "IntendedUseResearchAndDevelopmentCheckBox";
			this.IntendedUseResearchAndDevelopmentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.IntendedUseResearchAndDevelopmentCheckBox.TabIndex = 3;
			this.IntendedUseResearchAndDevelopmentCheckBox.Text = "Research And Development";
			this.IntendedUseResearchAndDevelopmentCheckBox.UseVisualStyleBackColor = true;
			// 
			// IntendedUseSpecialtyChemicalProductionCheckBox
			// 
			this.IntendedUseSpecialtyChemicalProductionCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseSpecialtyChemicalProductionCheckBox, "CA_IUSCP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IUSCP)));
			this.IntendedUseSpecialtyChemicalProductionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseSpecialtyChemicalProductionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 45, true);
			this.IntendedUseSpecialtyChemicalProductionCheckBox.Name = "IntendedUseSpecialtyChemicalProductionCheckBox";
			this.IntendedUseSpecialtyChemicalProductionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 17, true);
			this.IntendedUseSpecialtyChemicalProductionCheckBox.TabIndex = 4;
			this.IntendedUseSpecialtyChemicalProductionCheckBox.Text = "Specialty Chemical Production";
			this.IntendedUseSpecialtyChemicalProductionCheckBox.UseVisualStyleBackColor = true;
			// 
			// IntendedUseEnvironmentalApplicationCheckBox
			// 
			this.IntendedUseEnvironmentalApplicationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseEnvironmentalApplicationCheckBox, "CA_IUEA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IUEA)));
			this.IntendedUseEnvironmentalApplicationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseEnvironmentalApplicationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 22, true);
			this.IntendedUseEnvironmentalApplicationCheckBox.Name = "IntendedUseEnvironmentalApplicationCheckBox";
			this.IntendedUseEnvironmentalApplicationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 17, true);
			this.IntendedUseEnvironmentalApplicationCheckBox.TabIndex = 2;
			this.IntendedUseEnvironmentalApplicationCheckBox.Text = "Environmental Application";
			this.IntendedUseEnvironmentalApplicationCheckBox.UseVisualStyleBackColor = true;
			// 
			// IntendedUseOtherCheckBox
			// 
			this.IntendedUseOtherCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseOtherCheckBox, "CA_IUOTH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IUOTH)));
			this.IntendedUseOtherCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseOtherCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 68, true);
			this.IntendedUseOtherCheckBox.Name = "IntendedUseOtherCheckBox";
			this.IntendedUseOtherCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.IntendedUseOtherCheckBox.TabIndex = 7;
			this.IntendedUseOtherCheckBox.Text = "Other";
			this.IntendedUseOtherCheckBox.UseVisualStyleBackColor = true;
			// 
			// IntendedUseOrnamentalCheckBox
			// 
			this.IntendedUseOrnamentalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseOrnamentalCheckBox, "CA_IUO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IUO)));
			this.IntendedUseOrnamentalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseOrnamentalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 45, true);
			this.IntendedUseOrnamentalCheckBox.Name = "IntendedUseOrnamentalCheckBox";
			this.IntendedUseOrnamentalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.IntendedUseOrnamentalCheckBox.TabIndex = 5;
			this.IntendedUseOrnamentalCheckBox.Text = "Ornamental";
			this.IntendedUseOrnamentalCheckBox.UseVisualStyleBackColor = true;
			// 
			// IntendedUseEducationalCheckBox
			// 
			this.IntendedUseEducationalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseEducationalCheckBox, "CA_IUE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IUE)));
			this.IntendedUseEducationalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseEducationalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 68, true);
			this.IntendedUseEducationalCheckBox.Name = "IntendedUseEducationalCheckBox";
			this.IntendedUseEducationalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 17, true);
			this.IntendedUseEducationalCheckBox.TabIndex = 6;
			this.IntendedUseEducationalCheckBox.Text = "Educational";
			this.IntendedUseEducationalCheckBox.UseVisualStyleBackColor = true;
			// 
			// CategoryDropEdit
			// 
			this.CategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "CA_Category");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_Category)));
			this.CategoryDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("080ac78a-93cf-475f-b95a-4e3cea92b36c", "Genetic Modification Purpose");
			this.CategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(657, 37, true);
			this.CategoryDropEdit.Name = "CategoryDropEdit";
			this.CategoryDropEdit.ShouldResizeByMaxLength = true;
			this.CategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.CategoryDropEdit.TabIndex = 9;
			// 
			// TSNTextBox
			// 
			this.BindingSource.SetBindingMember(this.TSNTextBox, "CA_TSN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_TSN)));
			this.TSNTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8e99a88f-a55b-42cd-a1d7-68d25d61c7e5", "TSN");
			this.TSNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 63, true);
			this.TSNTextBox.Name = "TSNTextBox";
			this.TSNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.TSNTextBox.TabIndex = 2;
			// 
			// SexGroupBox
			// 
			this.SexGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("04306862-cea9-4d3b-9fa3-a94e5a33c646", "Sex");
			this.SexGroupBox.Controls.Add(this.SexOtherCheckBox);
			this.SexGroupBox.Controls.Add(this.SexSterileCheckBox);
			this.SexGroupBox.Controls.Add(this.SexHermaphroditeCheckBox);
			this.SexGroupBox.Controls.Add(this.SexMaleCheckBox);
			this.SexGroupBox.Controls.Add(this.SexFemaleCheckBox);
			this.SexGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 49, true);
			this.SexGroupBox.Name = "SexGroupBox";
			this.SexGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 45, true);
			this.SexGroupBox.TabIndex = 2;
			this.SexGroupBox.TabStop = false;
			this.SexGroupBox.Text = "Sex";
			// 
			// SexOtherCheckBox
			// 
			this.SexOtherCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SexOtherCheckBox, "CA_SexOther");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_SexOther)));
			this.SexOtherCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SexOtherCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 21, true);
			this.SexOtherCheckBox.Name = "SexOtherCheckBox";
			this.SexOtherCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.SexOtherCheckBox.TabIndex = 2;
			this.SexOtherCheckBox.Text = "Other";
			this.SexOtherCheckBox.UseVisualStyleBackColor = true;
			// 
			// SexSterileCheckBox
			// 
			this.SexSterileCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SexSterileCheckBox, "CA_SexSterile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_SexSterile)));
			this.SexSterileCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SexSterileCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 21, true);
			this.SexSterileCheckBox.Name = "SexSterileCheckBox";
			this.SexSterileCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 17, true);
			this.SexSterileCheckBox.TabIndex = 3;
			this.SexSterileCheckBox.Text = "Sterile";
			this.SexSterileCheckBox.UseVisualStyleBackColor = true;
			// 
			// SexHermaphroditeCheckBox
			// 
			this.SexHermaphroditeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SexHermaphroditeCheckBox, "CA_SexHermaphrodite");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_SexHermaphrodite)));
			this.SexHermaphroditeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SexHermaphroditeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 21, true);
			this.SexHermaphroditeCheckBox.Name = "SexHermaphroditeCheckBox";
			this.SexHermaphroditeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 17, true);
			this.SexHermaphroditeCheckBox.TabIndex = 4;
			this.SexHermaphroditeCheckBox.Text = "Hermaphrodite";
			this.SexHermaphroditeCheckBox.UseVisualStyleBackColor = true;
			// 
			// SexMaleCheckBox
			// 
			this.SexMaleCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SexMaleCheckBox, "CA_SexMale");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_SexMale)));
			this.SexMaleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SexMaleCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 21, true);
			this.SexMaleCheckBox.Name = "SexMaleCheckBox";
			this.SexMaleCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 17, true);
			this.SexMaleCheckBox.TabIndex = 0;
			this.SexMaleCheckBox.Text = "Male";
			this.SexMaleCheckBox.UseVisualStyleBackColor = true;
			// 
			// SexFemaleCheckBox
			// 
			this.SexFemaleCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SexFemaleCheckBox, "CA_SexFemale");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_SexFemale)));
			this.SexFemaleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SexFemaleCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 21, true);
			this.SexFemaleCheckBox.Name = "SexFemaleCheckBox";
			this.SexFemaleCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			this.SexFemaleCheckBox.TabIndex = 1;
			this.SexFemaleCheckBox.Text = "Female";
			this.SexFemaleCheckBox.UseVisualStyleBackColor = true;
			// 
			// LifeStageGroupBox
			// 
			this.LifeStageGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ade4faea-6a1e-4f1d-900b-916d91e0df51", "Life Stage");
			this.LifeStageGroupBox.Controls.Add(this.LifeStageAdultCheckBox);
			this.LifeStageGroupBox.Controls.Add(this.LifeStageEmbryoCheckBox);
			this.LifeStageGroupBox.Controls.Add(this.LifeStagePropagateCheckBox);
			this.LifeStageGroupBox.Controls.Add(this.LifeStageJuvenileCheckBox);
			this.LifeStageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 0, true);
			this.LifeStageGroupBox.Name = "LifeStageGroupBox";
			this.LifeStageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 45, true);
			this.LifeStageGroupBox.TabIndex = 1;
			this.LifeStageGroupBox.TabStop = false;
			this.LifeStageGroupBox.Text = "Life Stage";
			// 
			// LifeStageAdultCheckBox
			// 
			this.LifeStageAdultCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LifeStageAdultCheckBox, "CA_LifeStageAdult");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_LifeStageAdult)));
			this.LifeStageAdultCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LifeStageAdultCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 21, true);
			this.LifeStageAdultCheckBox.Name = "LifeStageAdultCheckBox";
			this.LifeStageAdultCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 17, true);
			this.LifeStageAdultCheckBox.TabIndex = 3;
			this.LifeStageAdultCheckBox.Text = "Adult";
			this.LifeStageAdultCheckBox.UseVisualStyleBackColor = true;
			// 
			// LifeStageEmbryoCheckBox
			// 
			this.LifeStageEmbryoCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LifeStageEmbryoCheckBox, "CA_LifeStageEmbryo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_LifeStageEmbryo)));
			this.LifeStageEmbryoCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LifeStageEmbryoCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 21, true);
			this.LifeStageEmbryoCheckBox.Name = "LifeStageEmbryoCheckBox";
			this.LifeStageEmbryoCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 17, true);
			this.LifeStageEmbryoCheckBox.TabIndex = 1;
			this.LifeStageEmbryoCheckBox.Text = "Embryo";
			this.LifeStageEmbryoCheckBox.UseVisualStyleBackColor = true;
			// 
			// LifeStagePropagateCheckBox
			// 
			this.LifeStagePropagateCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LifeStagePropagateCheckBox, "CA_LifeStagePropagate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_LifeStagePropagate)));
			this.LifeStagePropagateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LifeStagePropagateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 21, true);
			this.LifeStagePropagateCheckBox.Name = "LifeStagePropagateCheckBox";
			this.LifeStagePropagateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 17, true);
			this.LifeStagePropagateCheckBox.TabIndex = 0;
			this.LifeStagePropagateCheckBox.Text = "Propagate";
			this.LifeStagePropagateCheckBox.UseVisualStyleBackColor = true;
			// 
			// LifeStageJuvenileCheckBox
			// 
			this.LifeStageJuvenileCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LifeStageJuvenileCheckBox, "CA_LifeStageJuvenile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_LifeStageJuvenile)));
			this.LifeStageJuvenileCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LifeStageJuvenileCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 21, true);
			this.LifeStageJuvenileCheckBox.Name = "LifeStageJuvenileCheckBox";
			this.LifeStageJuvenileCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 17, true);
			this.LifeStageJuvenileCheckBox.TabIndex = 2;
			this.LifeStageJuvenileCheckBox.Text = "Juvenile";
			this.LifeStageJuvenileCheckBox.UseVisualStyleBackColor = true;
			// 
			// GenusOrSpeciesTextBox
			// 
			this.BindingSource.SetBindingMember(this.GenusOrSpeciesTextBox, "CA_GenusOrSpecies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_GenusOrSpecies)));
			this.GenusOrSpeciesTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("32f38ca3-42f8-4ee4-b084-7d4e5580a199", "Genus/Species");
			this.GenusOrSpeciesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 37, true);
			this.GenusOrSpeciesTextBox.Name = "GenusOrSpeciesTextBox";
			this.GenusOrSpeciesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.GenusOrSpeciesTextBox.TabIndex = 1;
			// 
			// GeneOrNucleotideSequenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.GeneOrNucleotideSequenceTextBox, "CA_GeneOrNucleotideSequence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_GeneOrNucleotideSequence)));
			this.GeneOrNucleotideSequenceTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ca709fe3-1f8d-4b0b-8cdd-e673db083089", "Gene or Nucleotide Sequence");
			this.GeneOrNucleotideSequenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(657, 115, true);
			this.GeneOrNucleotideSequenceTextBox.Name = "GeneOrNucleotideSequenceTextBox";
			this.GeneOrNucleotideSequenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.GeneOrNucleotideSequenceTextBox.TabIndex = 12;
			// 
			// GeneticModificationDescriptionTextBox
			// 
			BindingSource.SetBindingMember(GeneticModificationDescriptionTextBox, "CA_GeneticModificationDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_GeneticModificationDescription)));
			this.GeneticModificationDescriptionTextBox.AllowDrop = true;
			this.GeneticModificationDescriptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5de24667-e719-4cc0-ba28-c82ef09499bb", "Genetic Modification Description");
			this.GeneticModificationDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(657, 63, true);
			this.GeneticModificationDescriptionTextBox.Name = "GeneticModificationDescriptionTextBox";
			this.GeneticModificationDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.GeneticModificationDescriptionTextBox.TabIndex = 10;
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("00dba479-d814-4c62-8400-967fcb19a0c4", "Details");
			this.MainGroupBox.Controls.Add(this.NewSubstancesNotificationNumberTextBox);
			this.MainGroupBox.Controls.Add(this.ManufacturerUserControl);
			this.MainGroupBox.Controls.Add(this.HarvestingPartyAddressControl);
			this.MainGroupBox.Controls.Add(this.GeneticModificationCheckBox);
			this.MainGroupBox.Controls.Add(this.CountryOfOriginCodeFindBox);
			this.MainGroupBox.Controls.Add(this.CommonNameTextBox);
			this.MainGroupBox.Controls.Add(this.GeneticModificationDescriptionTextBox);
			this.MainGroupBox.Controls.Add(this.TradeNameTextBox);
			this.MainGroupBox.Controls.Add(this.GeneOrNucleotideSequenceTextBox);
			this.MainGroupBox.Controls.Add(this.CountIntEdit);
			this.MainGroupBox.Controls.Add(this.GenusOrSpeciesTextBox);
			this.MainGroupBox.Controls.Add(this.TSNTextBox);
			this.MainGroupBox.Controls.Add(this.CategoryDropEdit);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 95, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 195, true);
			this.MainGroupBox.TabIndex = 3;
			this.MainGroupBox.TabStop = false;
			this.MainGroupBox.Text = "Details";
			// 
			// NewSubstancesNotificationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.NewSubstancesNotificationNumberTextBox, "NSNNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).NSNNumber)));
			this.NewSubstancesNotificationNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5467caaf-58f5-4739-900f-bacdbdc5e851", "New Substances Notification Number ");
			this.NewSubstancesNotificationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(657, 141, true);
			this.NewSubstancesNotificationNumberTextBox.Name = "NewSubstancesNotificationNumberTextBox";
			this.NewSubstancesNotificationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.NewSubstancesNotificationNumberTextBox.TabIndex = 13;
			// 
			// GeneticModificationCheckBox
			// 
			this.GeneticModificationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GeneticModificationCheckBox, "CA_HasGeneticModification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_HasGeneticModification)));
			this.GeneticModificationCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("841e7b09-0021-4112-99f9-163c291ab337", "Genetic Modification");
			this.GeneticModificationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GeneticModificationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(657, 11, true);
			this.GeneticModificationCheckBox.Name = "GeneticModificationCheckBox";
			this.GeneticModificationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 17, true);
			this.GeneticModificationCheckBox.TabIndex = 8;
			this.GeneticModificationCheckBox.Text = "Genetic Modification";
			this.GeneticModificationCheckBox.UseVisualStyleBackColor = true;
			// 
			// ABIProgramUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGroupBox);
			this.Controls.Add(this.IntendedUseCodeGroupBox);
			this.Controls.Add(this.SexGroupBox);
			this.Controls.Add(this.LifeStageGroupBox);
			this.Name = "ABIProgramUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 290, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManufacturerUserControl.ResumeLayout(true);
			this.ManufacturerUserControl.PerformLayout();
			this.HarvestingPartyAddressControl.ResumeLayout(true);
			this.HarvestingPartyAddressControl.PerformLayout();
			this.CountryOfOriginCodeFindBox.ResumeLayout(true);
			this.CountryOfOriginCodeFindBox.PerformLayout();
			this.IntendedUseCodeGroupBox.ResumeLayout(false);
			this.IntendedUseCodeGroupBox.PerformLayout();
			this.CategoryDropEdit.ResumeLayout(true);
			this.CategoryDropEdit.PerformLayout();
			this.SexGroupBox.ResumeLayout(false);
			this.SexGroupBox.PerformLayout();
			this.LifeStageGroupBox.ResumeLayout(false);
			this.LifeStageGroupBox.PerformLayout();
			this.GeneticModificationDescriptionTextBox.ResumeLayout(true);
			this.GeneticModificationDescriptionTextBox.PerformLayout();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		ZArchitecture.GUI.ZAddressControl ManufacturerUserControl;
		Enterprise.Customs.GUI.LongTextControl GeneticModificationDescriptionTextBox;
		ZArchitecture.ZTextBox GeneOrNucleotideSequenceTextBox;
		ZArchitecture.ZTextBox GenusOrSpeciesTextBox;
		ZArchitecture.GUI.ZCheckBox LifeStageAdultCheckBox;
		ZArchitecture.GUI.ZCheckBox LifeStagePropagateCheckBox;
		ZArchitecture.GUI.ZCheckBox LifeStageJuvenileCheckBox;
		ZArchitecture.GUI.ZCheckBox LifeStageEmbryoCheckBox;
		ZArchitecture.GUI.ZGroupBox LifeStageGroupBox;
		ZArchitecture.GUI.ZGroupBox SexGroupBox;
		ZArchitecture.GUI.ZCheckBox SexMaleCheckBox;
		ZArchitecture.GUI.ZCheckBox SexFemaleCheckBox;
		ZArchitecture.GUI.ZCheckBox SexHermaphroditeCheckBox;
		ZArchitecture.GUI.ZCheckBox SexOtherCheckBox;
		ZArchitecture.GUI.ZCheckBox SexSterileCheckBox;
		ZArchitecture.GUI.ZGroupBox IntendedUseCodeGroupBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseFoodCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseAquacultureCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseResearchAndDevelopmentCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseSpecialtyChemicalProductionCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseEnvironmentalApplicationCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseOrnamentalCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseEducationalCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseOtherCheckBox;
		ZArchitecture.ZTextBox TSNTextBox;
		ZArchitecture.GUI.ZDropEdit CategoryDropEdit;
		ZArchitecture.GUI.ZIntEdit CountIntEdit;
		ZArchitecture.ZTextBox TradeNameTextBox;
		ZArchitecture.ZTextBox CommonNameTextBox;
		ZArchitecture.GUI.ZCodeFindBox CountryOfOriginCodeFindBox;
		ZArchitecture.GUI.ZAddressControl HarvestingPartyAddressControl;
		ZArchitecture.GUI.ZGroupBox MainGroupBox;
		ZArchitecture.GUI.ZCheckBox GeneticModificationCheckBox;
		ZArchitecture.ZTextBox NewSubstancesNotificationNumberTextBox;
	}
}

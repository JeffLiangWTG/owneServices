using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class TTPProgramUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
		void InitializeComponent()
		{
			this.ManufacturerUserControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.LpcoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LPCOGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			this.HarvestingPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CommonNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			this.TSNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SexGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SexUnknownCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SexOtherCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SexSterileCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SexHermaphroditeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SexMaleCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SexFemaleCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LifeStageDeadCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStageAdultCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStageEmbryoCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStagePropagateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStageJuvenileCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GenusOrSpeciesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IntendedUseCodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IntendedUseFoodCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseAquacultureCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseResearchAndDevelopmentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseSpecialtyChemicalProductionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseEnvironmentalApplicationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseOtherCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseOrnamentalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseEducationalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommissionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CommonNameCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProcessorAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.EvisceratedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FillPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ManufacturerUserControl.SuspendLayout();
			this.LpcoGroupBox.SuspendLayout();
			this.LPCOGridUserControl.SuspendLayout();
			this.HarvestingPartyAddressControl.SuspendLayout();
			this.SexGroupBox.SuspendLayout();
			this.LifeStageGroupBox.SuspendLayout();
			this.IntendedUseCodeGroupBox.SuspendLayout();
			this.MainGroupBox.SuspendLayout();
			this.CommissionDropEdit.SuspendLayout();
			this.CommonNameCodeDropEdit.SuspendLayout();
			this.ProcessorAddressControl.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.WeightCalcDropEdit.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.FillPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.DFOPGAHeader);
			// 
			// ManufacturerUserControl
			// 
			this.BindingSource.SetBindingMember(ManufacturerUserControl, "OA_Manufacturer");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).OA_Manufacturer)));
			this.ManufacturerUserControl.BindToOrgList = "RequirementsParent.ManufacturersLookup";
			this.ManufacturerUserControl.AllowDrop = true;
			this.ManufacturerUserControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("303351ce-a488-4ba9-9db3-17cff3b0b4c4", "Manufacturer");
			this.ManufacturerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 114, true);
			this.ManufacturerUserControl.Name = "ManufacturerUserControl";
			this.ManufacturerUserControl.PopupCaption = "";
			this.ManufacturerUserControl.ReadOnly = false;
			this.ManufacturerUserControl.ShowAddress = false;
			this.ManufacturerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ManufacturerUserControl.TabIndex = 4;
			// 
			// LpcoGroupBox
			// 
			this.LpcoGroupBox.AutoSize = true;
			this.LpcoGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3FE74E2F-59A3-4293-90C8-F636ED5CC5F5", "LPCOs");
			this.LpcoGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.LpcoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LpcoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(736, 0, true);
			this.LpcoGroupBox.Name = "LpcoGroupBox";
			this.LpcoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 202, true);
			this.LpcoGroupBox.TabIndex = 4;
			this.LpcoGroupBox.TabStop = false;
			this.LpcoGroupBox.Text = "LPCOs";
			// 
			// LPCOGridUserControl
			// 
			this.LPCOGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).LPCOViews)));
			this.LPCOGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LPCOGridUserControl.Name = "LPCOGridUserControl";
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 183, true);
			this.LPCOGridUserControl.TabIndex = 0;
			// 
			// HarvestingPartyAddressControl
			// 
			this.HarvestingPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HarvestingPartyAddressControl, "CA_OA_HarvestingParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_OA_HarvestingParty)));
			this.HarvestingPartyAddressControl.BindToOrgList = "AddInfoLookups.HarvestingParties";
			this.HarvestingPartyAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("154ffe2c-b88f-47d6-b87f-6630c94174d2", "Harvesting Party");
			this.HarvestingPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 140, true);
			this.HarvestingPartyAddressControl.Name = "HarvestingPartyAddressControl";
			this.HarvestingPartyAddressControl.PopupCaption = "";
			this.HarvestingPartyAddressControl.ReadOnly = false;
			this.HarvestingPartyAddressControl.ShowAddress = false;
			this.HarvestingPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.HarvestingPartyAddressControl.TabIndex = 5;
			// 
			// CommonNameTextBox
			// 
			this.BindingSource.SetBindingMember(CommonNameTextBox, "InvoiceLine.JI_Model");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).InvoiceLine.JI_Model)));
			this.CommonNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("84748853-b6ee-4ea0-b649-12c72755dfff", "Model/Common Name");
			this.CommonNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 38, true);
			this.CommonNameTextBox.Name = "CommonNameTextBox";
			this.CommonNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.CommonNameTextBox.TabIndex = 1;
			// 
			// CountIntEdit
			// 
			this.BindingSource.SetBindingMember(this.CountIntEdit, "CA_Count");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_Count)));
			this.CountIntEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("bbb98df7-3e66-4246-b9d9-46df8c9b3ade", "Count");
			this.CountIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 116, true);
			this.CountIntEdit.Name = "CountIntEdit";
			this.CountIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.CountIntEdit.TabIndex = 11;
			this.CountIntEdit.Text = "0";
			this.CountIntEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TSNTextBox
			// 
			this.BindingSource.SetBindingMember(this.TSNTextBox, "CA_TSN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_TSN)));
			this.TSNTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c352bd2a-3bc9-4ea2-997a-13978213def4", "TSN");
			this.TSNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 88, true);
			this.TSNTextBox.Name = "TSNTextBox";
			this.TSNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.TSNTextBox.TabIndex = 3;
			// 
			// SexGroupBox
			// 
			this.SexGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d631ba97-f5ff-4a14-8495-7e43afe99e68", "Sex");
			this.SexGroupBox.Controls.Add(this.SexUnknownCheckBox);
			this.SexGroupBox.Controls.Add(this.SexOtherCheckBox);
			this.SexGroupBox.Controls.Add(this.SexSterileCheckBox);
			this.SexGroupBox.Controls.Add(this.SexHermaphroditeCheckBox);
			this.SexGroupBox.Controls.Add(this.SexMaleCheckBox);
			this.SexGroupBox.Controls.Add(this.SexFemaleCheckBox);
			this.SexGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(469, 46, true);
			this.SexGroupBox.Name = "SexGroupBox";
			this.SexGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 43, true);
			this.SexGroupBox.TabIndex = 2;
			this.SexGroupBox.TabStop = false;
			this.SexGroupBox.Text = "Sex";
			// 
			// SexUnknownCheckBox
			// 
			this.SexUnknownCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SexUnknownCheckBox, "CA_SexUnknown");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_SexUnknown)));
			this.SexUnknownCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8f1e7626-b722-4930-88f5-fed923f275bc", "Unknown");
			this.SexUnknownCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SexUnknownCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 21, true);
			this.SexUnknownCheckBox.Name = "SexUnknownCheckBox";
			this.SexUnknownCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.SexUnknownCheckBox.TabIndex = 4;
			this.SexUnknownCheckBox.Text = "Unknown";
			this.SexUnknownCheckBox.UseVisualStyleBackColor = true;
			// 
			// SexOtherCheckBox
			// 
			this.SexOtherCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SexOtherCheckBox, "CA_SexOther");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_SexOther)));
			this.SexOtherCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6943a139-dac6-4a6d-b2a1-7a897ff0af80", "Other");
			this.SexOtherCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SexOtherCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 21, true);
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
			this.SexSterileCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("37287c5f-9bed-4358-b945-3015a9435226", "Sterile");
			this.SexSterileCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SexSterileCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 21, true);
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
			this.SexHermaphroditeCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("74d1b38f-6ab9-4c6e-9fdf-ace01a03ab31", "Hermaphrodite");
			this.SexHermaphroditeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SexHermaphroditeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 21, true);
			this.SexHermaphroditeCheckBox.Name = "SexHermaphroditeCheckBox";
			this.SexHermaphroditeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 17, true);
			this.SexHermaphroditeCheckBox.TabIndex = 5;
			this.SexHermaphroditeCheckBox.Text = "Hermaphrodite";
			this.SexHermaphroditeCheckBox.UseVisualStyleBackColor = true;
			// 
			// SexMaleCheckBox
			// 
			this.SexMaleCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SexMaleCheckBox, "CA_SexMale");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_SexMale)));
			this.SexMaleCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("910fba57-be53-4d9b-b172-28abc880438f", "Male");
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
			this.SexFemaleCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e8340c71-e910-4b15-b8dd-2c89ba361b87", "Female");
			this.SexFemaleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SexFemaleCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 21, true);
			this.SexFemaleCheckBox.Name = "SexFemaleCheckBox";
			this.SexFemaleCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			this.SexFemaleCheckBox.TabIndex = 1;
			this.SexFemaleCheckBox.Text = "Female";
			this.SexFemaleCheckBox.UseVisualStyleBackColor = true;
			// 
			// LifeStageGroupBox
			// 
			this.LifeStageGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("acbff1cd-0570-42b1-a770-1fdf21741749", "Life Stage");
			this.LifeStageGroupBox.Controls.Add(this.LifeStageDeadCheckBox);
			this.LifeStageGroupBox.Controls.Add(this.LifeStageAdultCheckBox);
			this.LifeStageGroupBox.Controls.Add(this.LifeStageEmbryoCheckBox);
			this.LifeStageGroupBox.Controls.Add(this.LifeStagePropagateCheckBox);
			this.LifeStageGroupBox.Controls.Add(this.LifeStageJuvenileCheckBox);
			this.LifeStageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(469, 0, true);
			this.LifeStageGroupBox.Name = "LifeStageGroupBox";
			this.LifeStageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 43, true);
			this.LifeStageGroupBox.TabIndex = 1;
			this.LifeStageGroupBox.TabStop = false;
			this.LifeStageGroupBox.Text = "Life Stage";
			// 
			// LifeStageDeadCheckBox
			// 
			this.LifeStageDeadCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LifeStageDeadCheckBox, "CA_LifeStageDead");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_LifeStageDead)));
			this.LifeStageDeadCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("035b1d13-7ae6-445f-b908-c4aa8dddc52e", "Dead");
			this.LifeStageDeadCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LifeStageDeadCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 21, true);
			this.LifeStageDeadCheckBox.Name = "LifeStageDeadCheckBox";
			this.LifeStageDeadCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.LifeStageDeadCheckBox.TabIndex = 4;
			this.LifeStageDeadCheckBox.Text = "Dead";
			this.LifeStageDeadCheckBox.UseVisualStyleBackColor = true;
			// 
			// LifeStageAdultCheckBox
			// 
			this.LifeStageAdultCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LifeStageAdultCheckBox, "CA_LifeStageAdult");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_LifeStageAdult)));
			this.LifeStageAdultCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c4e3c849-b932-4bc3-a765-084ca07d776e", "Adult");
			this.LifeStageAdultCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LifeStageAdultCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 21, true);
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
			this.LifeStageEmbryoCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ed01764b-1144-42a0-9e29-bccde08ff653", "Embryo");
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
			this.LifeStagePropagateCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f0b88cc8-b183-47c0-bf8a-47a59f723cdd", "Propagate");
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
			this.LifeStageJuvenileCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5badddb4-3553-499f-ae30-654ac261f40c", "Juvenile");
			this.LifeStageJuvenileCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LifeStageJuvenileCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 21, true);
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
			this.GenusOrSpeciesTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("efc305e3-7de6-4af7-a4ad-a13c8b80d851", "Genus/Species");
			this.GenusOrSpeciesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 62, true);
			this.GenusOrSpeciesTextBox.Name = "GenusOrSpeciesTextBox";
			this.GenusOrSpeciesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.GenusOrSpeciesTextBox.TabIndex = 2;
			// 
			// IntendedUseCodeGroupBox
			// 
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseFoodCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseAquacultureCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseResearchAndDevelopmentCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseSpecialtyChemicalProductionCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseEnvironmentalApplicationCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseOtherCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseOrnamentalCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseEducationalCheckBox);
			this.IntendedUseCodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.IntendedUseCodeGroupBox.Name = "IntendedUseCodeGroupBox";
			this.IntendedUseCodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 90, true);
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
			this.IntendedUseFoodCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a771e741-89b7-4340-bf11-54c82c930aa0", "Food");
			this.IntendedUseFoodCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseFoodCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 21, true);
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
			this.IntendedUseAquacultureCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("66a111b6-9ab5-4bd5-ae04-0a5be721908d", "Aquaculture");
			this.IntendedUseAquacultureCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseAquacultureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 21, true);
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
			this.IntendedUseResearchAndDevelopmentCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("db559a88-b837-4a89-a914-5eeb7b7beedb", "Research And Development");
			this.IntendedUseResearchAndDevelopmentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseResearchAndDevelopmentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 44, true);
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
			this.IntendedUseSpecialtyChemicalProductionCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("320ae83e-7207-4e72-8e90-c83b63cc7080", "Specialty Chemical Production");
			this.IntendedUseSpecialtyChemicalProductionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseSpecialtyChemicalProductionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 44, true);
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
			this.IntendedUseEnvironmentalApplicationCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3d3c3f03-9dee-4e87-8e86-24374a46200d", "Environmental Application");
			this.IntendedUseEnvironmentalApplicationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseEnvironmentalApplicationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 68, true);
			this.IntendedUseEnvironmentalApplicationCheckBox.Name = "IntendedUseEnvironmentalApplicationCheckBox";
			this.IntendedUseEnvironmentalApplicationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 17, true);
			this.IntendedUseEnvironmentalApplicationCheckBox.TabIndex = 6;
			this.IntendedUseEnvironmentalApplicationCheckBox.Text = "Environmental Application";
			this.IntendedUseEnvironmentalApplicationCheckBox.UseVisualStyleBackColor = true;
			// 
			// IntendedUseOtherCheckBox
			// 
			this.IntendedUseOtherCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseOtherCheckBox, "CA_IUOTH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IUOTH)));
			this.IntendedUseOtherCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e27a317f-d3f3-4820-9852-9efe5712f876", "Other");
			this.IntendedUseOtherCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseOtherCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 44, true);
			this.IntendedUseOtherCheckBox.Name = "IntendedUseOtherCheckBox";
			this.IntendedUseOtherCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.IntendedUseOtherCheckBox.TabIndex = 5;
			this.IntendedUseOtherCheckBox.Text = "Other";
			this.IntendedUseOtherCheckBox.UseVisualStyleBackColor = true;
			// 
			// IntendedUseOrnamentalCheckBox
			// 
			this.IntendedUseOrnamentalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseOrnamentalCheckBox, "CA_IUO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IUO)));
			this.IntendedUseOrnamentalCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("efdb28fd-5ff9-422f-9464-6349f4335bf9", "Ornamental");
			this.IntendedUseOrnamentalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseOrnamentalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 68, true);
			this.IntendedUseOrnamentalCheckBox.Name = "IntendedUseOrnamentalCheckBox";
			this.IntendedUseOrnamentalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.IntendedUseOrnamentalCheckBox.TabIndex = 7;
			this.IntendedUseOrnamentalCheckBox.Text = "Ornamental";
			this.IntendedUseOrnamentalCheckBox.UseVisualStyleBackColor = true;
			// 
			// IntendedUseEducationalCheckBox
			// 
			this.IntendedUseEducationalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseEducationalCheckBox, "CA_IUE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IUE)));
			this.IntendedUseEducationalCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3d9f941c-b233-42e6-942a-8806395c56dc", "Educational");
			this.IntendedUseEducationalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseEducationalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 21, true);
			this.IntendedUseEducationalCheckBox.Name = "IntendedUseEducationalCheckBox";
			this.IntendedUseEducationalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 17, true);
			this.IntendedUseEducationalCheckBox.TabIndex = 2;
			this.IntendedUseEducationalCheckBox.Text = "Educational";
			this.IntendedUseEducationalCheckBox.UseVisualStyleBackColor = true;
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("86830c07-056d-4c74-9881-d0dc34ada2f9", "Details");
			this.MainGroupBox.Controls.Add(this.CommissionDropEdit);
			this.MainGroupBox.Controls.Add(this.CommonNameCodeDropEdit);
			this.MainGroupBox.Controls.Add(this.ManufacturerUserControl);
			this.MainGroupBox.Controls.Add(this.ProcessorAddressControl);
			this.MainGroupBox.Controls.Add(this.HarvestingPartyAddressControl);
			this.MainGroupBox.Controls.Add(this.VolumeCalcDropEdit);
			this.MainGroupBox.Controls.Add(this.WeightCalcDropEdit);
			this.MainGroupBox.Controls.Add(this.EvisceratedCheckBox);
			this.MainGroupBox.Controls.Add(this.CommonNameTextBox);
			this.MainGroupBox.Controls.Add(this.CountIntEdit);
			this.MainGroupBox.Controls.Add(this.GenusOrSpeciesTextBox);
			this.MainGroupBox.Controls.Add(this.TSNTextBox);
			this.MainGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 202, true);
			this.MainGroupBox.TabIndex = 3;
			this.MainGroupBox.TabStop = false;
			this.MainGroupBox.Text = "Details";
			// 
			// CommissionDropEdit
			// 
			this.CommissionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommissionDropEdit, "CA_Commission");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_Commission)));
			this.CommissionDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0e5522fb-c394-4343-8773-06b2757551ff", "Commission");
			this.CommissionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 90, true);
			this.CommissionDropEdit.Name = "CommissionDropEdit";
			this.CommissionDropEdit.ShouldResizeByMaxLength = true;
			this.CommissionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CommissionDropEdit.TabIndex = 10;
			// 
			// CommonNameCodeDropEdit
			// 
			this.CommonNameCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommonNameCodeDropEdit, "CA_CommonNameCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_CommonNameCode)));
			this.CommonNameCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("81e60403-5915-4572-9600-4f6058007871", "Common Name");
			this.CommonNameCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 15, true);
			this.CommonNameCodeDropEdit.Name = "CommonNameCodeDropEdit";
			this.CommonNameCodeDropEdit.ShouldResizeByMaxLength = true;
			this.CommonNameCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.CommonNameCodeDropEdit.TabIndex = 0;
			// 
			// ProcessorAddressControl
			// 
			this.ProcessorAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProcessorAddressControl, "CA_OA_Processor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_OA_Processor)));
			this.ProcessorAddressControl.BindToOrgList = "AddInfoLookups+Processors";
			this.ProcessorAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2752d720-cc75-495c-8375-aecf41c0318e", "Processor");
			this.ProcessorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 166, true);
			this.ProcessorAddressControl.Name = "ProcessorAddressControl";
			this.ProcessorAddressControl.PopupCaption = "";
			this.ProcessorAddressControl.ReadOnly = false;
			this.ProcessorAddressControl.ShowAddress = false;
			this.ProcessorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ProcessorAddressControl.TabIndex = 6;
			// 
			// VolumeCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
			this.VolumeCalcDropEdit.BindToAmount = "InvoiceLine.JI_Volume";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).InvoiceLine.JI_Volume)));
			this.VolumeCalcDropEdit.BindToUnit = "InvoiceLine.JI_VolumeUQ";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).InvoiceLine.JI_VolumeUQ)));
			this.VolumeCalcDropEdit.AllowDrop = true;
			this.VolumeCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("625c9b24-99e5-41f9-b05e-db73ea77f561", "Volume");
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 168, true);
			this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 13;
			this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// WeightCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
			this.WeightCalcDropEdit.BindToAmount = "InvoiceLine.JI_Weight";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).InvoiceLine.JI_Weight)));
			this.WeightCalcDropEdit.BindToUnit = "InvoiceLine.JI_WeightUQ";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).InvoiceLine.JI_WeightUQ)));
			this.WeightCalcDropEdit.AllowDrop = true;
			this.WeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ece3924f-fc36-4203-97ef-816d33efe208", "Gross Weight");
			this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 142, true);
			this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
			this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.WeightCalcDropEdit.TabIndex = 12;
			this.WeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// EvisceratedCheckBox
			// 
			this.EvisceratedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EvisceratedCheckBox, "CA_Eviscerated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_Eviscerated)));
			this.EvisceratedCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c91437e4-4a19-4b0d-9733-695e8b5980c9", "Eviscerated");
			this.EvisceratedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EvisceratedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 17, true);
			this.EvisceratedCheckBox.Name = "EvisceratedCheckBox";
			this.EvisceratedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 17, true);
			this.EvisceratedCheckBox.TabIndex = 7;
			this.EvisceratedCheckBox.Text = "Eviscerated";
			this.EvisceratedCheckBox.UseVisualStyleBackColor = true;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.IntendedUseCodeGroupBox);
			this.TopPanel.Controls.Add(this.LifeStageGroupBox);
			this.TopPanel.Controls.Add(this.SexGroupBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 93, true);
			this.TopPanel.TabIndex = 0;
			// 
			// FillPanel
			// 
			this.FillPanel.Controls.Add(this.LpcoGroupBox);
			this.FillPanel.Controls.Add(this.MainGroupBox);
			this.FillPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FillPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 93, true);
			this.FillPanel.Name = "FillPanel";
			this.FillPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 202, true);
			this.FillPanel.TabIndex = 1;
			// 
			// TTPProgramUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 295, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FillPanel);
			this.Controls.Add(this.TopPanel);
			this.Name = "TTPProgramUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 278, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManufacturerUserControl.ResumeLayout(true);
			this.ManufacturerUserControl.PerformLayout();
			this.LpcoGroupBox.ResumeLayout(false);
			this.LpcoGroupBox.PerformLayout();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.HarvestingPartyAddressControl.ResumeLayout(true);
			this.HarvestingPartyAddressControl.PerformLayout();
			this.SexGroupBox.ResumeLayout(false);
			this.SexGroupBox.PerformLayout();
			this.LifeStageGroupBox.ResumeLayout(false);
			this.LifeStageGroupBox.PerformLayout();
			this.IntendedUseCodeGroupBox.ResumeLayout(false);
			this.IntendedUseCodeGroupBox.PerformLayout();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.CommissionDropEdit.ResumeLayout(true);
			this.CommissionDropEdit.PerformLayout();
			this.CommonNameCodeDropEdit.ResumeLayout(true);
			this.CommonNameCodeDropEdit.PerformLayout();
			this.ProcessorAddressControl.ResumeLayout(true);
			this.ProcessorAddressControl.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.WeightCalcDropEdit.ResumeLayout(true);
			this.WeightCalcDropEdit.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.FillPanel.ResumeLayout(false);
			this.FillPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		ZArchitecture.GUI.ZAddressControl ManufacturerUserControl;
		Enterprise.ZArchitecture.GUI.ZGroupBox LpcoGroupBox;
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
		ZArchitecture.ZTextBox TSNTextBox;
		ZArchitecture.GUI.ZIntEdit CountIntEdit;
		ZArchitecture.ZTextBox CommonNameTextBox;
		ZArchitecture.GUI.ZAddressControl HarvestingPartyAddressControl;
		internal LPCOGridUserControl LPCOGridUserControl;
		ZArchitecture.GUI.ZGroupBox MainGroupBox;
		ZArchitecture.GUI.ZCheckBox LifeStageDeadCheckBox;
		ZArchitecture.GUI.ZCheckBox EvisceratedCheckBox;
		ZArchitecture.GUI.ZAddressControl ProcessorAddressControl;

		ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
		ZArchitecture.GUI.ZCalcDropEdit VolumeCalcDropEdit;
		ZArchitecture.GUI.ZGroupBox IntendedUseCodeGroupBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseFoodCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseAquacultureCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseResearchAndDevelopmentCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseSpecialtyChemicalProductionCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseEnvironmentalApplicationCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseOtherCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseOrnamentalCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseEducationalCheckBox;
		ZArchitecture.GUI.ZCheckBox SexUnknownCheckBox;
		ZArchitecture.GUI.ZDropEdit CommonNameCodeDropEdit;
		ZArchitecture.GUI.ZDropEdit CommissionDropEdit;
		ZPanel TopPanel;
		ZPanel FillPanel;
	}
}

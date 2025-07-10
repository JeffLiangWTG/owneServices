using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class AISProgramUserControl
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
			this.HarvestingPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CommonNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			this.IntendedUseCodeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IntendedUseAquaticInvasiveSpeciesControlCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseScientificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedUseEducationalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TSNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SexGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SexOtherCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SexSterileCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SexUnknownCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SexMaleCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SexFemaleCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LifeStageDeadCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStageLiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStageAdultCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStageEmbryoCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStagePropagateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LifeStageJuvenileCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GenusOrSpeciesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EvisceratedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ProcessorAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.GenusOrSpeciesDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FillPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LpcoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LPCOGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ManufacturerUserControl.SuspendLayout();
			this.HarvestingPartyAddressControl.SuspendLayout();
			this.IntendedUseCodeGroupBox.SuspendLayout();
			this.SexGroupBox.SuspendLayout();
			this.LifeStageGroupBox.SuspendLayout();
			this.MainGroupBox.SuspendLayout();
			this.DirectionDropEdit.SuspendLayout();
			this.ProcessorAddressControl.SuspendLayout();
			this.GenusOrSpeciesDropEdit.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.FillPanel.SuspendLayout();
			this.LpcoGroupBox.SuspendLayout();
			this.LPCOGridUserControl.SuspendLayout();
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
			this.ManufacturerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 147, true);
			this.ManufacturerUserControl.Name = "ManufacturerUserControl";
			this.ManufacturerUserControl.PopupCaption = "";
			this.ManufacturerUserControl.ReadOnly = false;
			this.ManufacturerUserControl.ShowAddress = false;
			this.ManufacturerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ManufacturerUserControl.TabIndex = 11;
			// 
			// HarvestingPartyAddressControl
			// 
			this.HarvestingPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HarvestingPartyAddressControl, "CA_OA_HarvestingParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_OA_HarvestingParty)));
			this.HarvestingPartyAddressControl.BindToOrgList = "AddInfoLookups.HarvestingParties";
			this.HarvestingPartyAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("154ffe2c-b88f-47d6-b87f-6630c94174d2", "Harvesting Party");
			this.HarvestingPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 95, true);
			this.HarvestingPartyAddressControl.Name = "HarvestingPartyAddressControl";
			this.HarvestingPartyAddressControl.PopupCaption = "";
			this.HarvestingPartyAddressControl.ReadOnly = false;
			this.HarvestingPartyAddressControl.ShowAddress = false;
			this.HarvestingPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.HarvestingPartyAddressControl.TabIndex = 9;
			// 
			// CommonNameTextBox
			// 
			this.BindingSource.SetBindingMember(CommonNameTextBox, "InvoiceLine.JI_Model");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).InvoiceLine.JI_Model)));
			this.CommonNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c0fbd38b-3548-48f1-8a24-ecd549cfd5c8", "Model/Common Name");
			this.CommonNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 17, true);
			this.CommonNameTextBox.Name = "CommonNameTextBox";
			this.CommonNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.CommonNameTextBox.TabIndex = 0;
			// 
			// CountIntEdit
			// 
			this.BindingSource.SetBindingMember(this.CountIntEdit, "CA_Count");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_Count)));
			this.CountIntEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("bbb98df7-3e66-4246-b9d9-46df8c9b3ade", "Count");
			this.CountIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 95, true);
			this.CountIntEdit.Name = "CountIntEdit";
			this.CountIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.CountIntEdit.TabIndex = 3;
			this.CountIntEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IntendedUseCodeGroupBox
			// 
			this.IntendedUseCodeGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5ecb31b3-a318-44c6-ac43-7e9aebac1235", "Intended Use Code");
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseAquaticInvasiveSpeciesControlCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseScientificCheckBox);
			this.IntendedUseCodeGroupBox.Controls.Add(this.IntendedUseEducationalCheckBox);
			this.IntendedUseCodeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IntendedUseCodeGroupBox.Name = "IntendedUseCodeGroupBox";
			this.IntendedUseCodeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 70, true);
			this.IntendedUseCodeGroupBox.TabIndex = 0;
			this.IntendedUseCodeGroupBox.TabStop = false;
			this.IntendedUseCodeGroupBox.Text = "Intended Use Code";
			// 
			// IntendedUseAquaticInvasiveSpeciesControlCheckBox
			// 
			this.IntendedUseAquaticInvasiveSpeciesControlCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseAquaticInvasiveSpeciesControlCheckBox, "CA_IUAIS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IUAIS)));
			this.IntendedUseAquaticInvasiveSpeciesControlCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("879cd5da-5a19-44e1-963f-18d6d6d1bc80", "Aquatic Invasive Species Control");
			this.IntendedUseAquaticInvasiveSpeciesControlCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseAquaticInvasiveSpeciesControlCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 21, true);
			this.IntendedUseAquaticInvasiveSpeciesControlCheckBox.Name = "IntendedUseAquaticInvasiveSpeciesControlCheckBox";
			this.IntendedUseAquaticInvasiveSpeciesControlCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 17, true);
			this.IntendedUseAquaticInvasiveSpeciesControlCheckBox.TabIndex = 1;
			this.IntendedUseAquaticInvasiveSpeciesControlCheckBox.Text = "Aquatic Invasive Species Control";
			this.IntendedUseAquaticInvasiveSpeciesControlCheckBox.UseVisualStyleBackColor = true;
			// 
			// IntendedUseScientificCheckBox
			// 
			this.IntendedUseScientificCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseScientificCheckBox, "CA_IUS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IUS)));
			this.IntendedUseScientificCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("db33982d-8e35-4fcc-afbf-49b411ee0d3c", "Scientific");
			this.IntendedUseScientificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseScientificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 21, true);
			this.IntendedUseScientificCheckBox.Name = "IntendedUseScientificCheckBox";
			this.IntendedUseScientificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.IntendedUseScientificCheckBox.TabIndex = 0;
			this.IntendedUseScientificCheckBox.Text = "Scientific";
			this.IntendedUseScientificCheckBox.UseVisualStyleBackColor = true;
			// 
			// IntendedUseEducationalCheckBox
			// 
			this.IntendedUseEducationalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IntendedUseEducationalCheckBox, "CA_IUE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_IUE)));
			this.IntendedUseEducationalCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d4ff2f71-9bf7-4d10-a8c8-8b6b4d7270a0", "Educational");
			this.IntendedUseEducationalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IntendedUseEducationalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 44, true);
			this.IntendedUseEducationalCheckBox.Name = "IntendedUseEducationalCheckBox";
			this.IntendedUseEducationalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 17, true);
			this.IntendedUseEducationalCheckBox.TabIndex = 2;
			this.IntendedUseEducationalCheckBox.Text = "Educational";
			this.IntendedUseEducationalCheckBox.UseVisualStyleBackColor = true;
			// 
			// TSNTextBox
			// 
			this.BindingSource.SetBindingMember(this.TSNTextBox, "CA_TSN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_TSN)));
			this.TSNTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c352bd2a-3bc9-4ea2-997a-13978213def4", "TSN");
			this.TSNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 43, true);
			this.TSNTextBox.Name = "TSNTextBox";
			this.TSNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.TSNTextBox.TabIndex = 7;
			// 
			// SexGroupBox
			// 
			this.SexGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d631ba97-f5ff-4a14-8495-7e43afe99e68", "Sex");
			this.SexGroupBox.Controls.Add(this.SexOtherCheckBox);
			this.SexGroupBox.Controls.Add(this.SexSterileCheckBox);
			this.SexGroupBox.Controls.Add(this.SexUnknownCheckBox);
			this.SexGroupBox.Controls.Add(this.SexMaleCheckBox);
			this.SexGroupBox.Controls.Add(this.SexFemaleCheckBox);
			this.SexGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(622, 0, true);
			this.SexGroupBox.Name = "SexGroupBox";
			this.SexGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 70, true);
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
			this.SexOtherCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6943a139-dac6-4a6d-b2a1-7a897ff0af80", "Other");
			this.SexOtherCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SexOtherCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(198, 21, true);
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
			this.SexSterileCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 21, true);
			this.SexSterileCheckBox.Name = "SexSterileCheckBox";
			this.SexSterileCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 17, true);
			this.SexSterileCheckBox.TabIndex = 3;
			this.SexSterileCheckBox.Text = "Sterile";
			this.SexSterileCheckBox.UseVisualStyleBackColor = true;
			// 
			// SexUnknownCheckBox
			// 
			this.SexUnknownCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SexUnknownCheckBox, "CA_SexUnknown");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_SexUnknown)));
			this.SexUnknownCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("36681a3a-e238-4f49-b1c5-9953fd9d6d9f", "Unknown");
			this.SexUnknownCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SexUnknownCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 44, true);
			this.SexUnknownCheckBox.Name = "SexUnknownCheckBox";
			this.SexUnknownCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.SexUnknownCheckBox.TabIndex = 4;
			this.SexUnknownCheckBox.UseVisualStyleBackColor = true;
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
			this.SexFemaleCheckBox.UseVisualStyleBackColor = true;
			// 
			// LifeStageGroupBox
			// 
			this.LifeStageGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("acbff1cd-0570-42b1-a770-1fdf21741749", "Life Stage");
			this.LifeStageGroupBox.Controls.Add(this.LifeStageDeadCheckBox);
			this.LifeStageGroupBox.Controls.Add(this.LifeStageLiveCheckBox);
			this.LifeStageGroupBox.Controls.Add(this.LifeStageAdultCheckBox);
			this.LifeStageGroupBox.Controls.Add(this.LifeStageEmbryoCheckBox);
			this.LifeStageGroupBox.Controls.Add(this.LifeStagePropagateCheckBox);
			this.LifeStageGroupBox.Controls.Add(this.LifeStageJuvenileCheckBox);
			this.LifeStageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 0, true);
			this.LifeStageGroupBox.Name = "LifeStageGroupBox";
			this.LifeStageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 70, true);
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
			this.LifeStageDeadCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 44, true);
			this.LifeStageDeadCheckBox.Name = "LifeStageDeadCheckBox";
			this.LifeStageDeadCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.LifeStageDeadCheckBox.TabIndex = 5;
			this.LifeStageDeadCheckBox.Text = "Dead";
			this.LifeStageDeadCheckBox.UseVisualStyleBackColor = true;
			// 
			// LifeStageLiveCheckBox
			// 
			this.LifeStageLiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LifeStageLiveCheckBox, "CA_LifeStageLive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_LifeStageLive)));
			this.LifeStageLiveCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d62bc387-d6cc-440d-bc82-681f28a5d0db", "Live");
			this.LifeStageLiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LifeStageLiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 44, true);
			this.LifeStageLiveCheckBox.Name = "LifeStageLiveCheckBox";
			this.LifeStageLiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 17, true);
			this.LifeStageLiveCheckBox.TabIndex = 4;
			this.LifeStageLiveCheckBox.Text = "Live";
			this.LifeStageLiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// LifeStageAdultCheckBox
			// 
			this.LifeStageAdultCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LifeStageAdultCheckBox, "CA_LifeStageAdult");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_LifeStageAdult)));
			this.LifeStageAdultCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c4e3c849-b932-4bc3-a765-084ca07d776e", "Adult");
			this.LifeStageAdultCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LifeStageAdultCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 44, true);
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
			this.LifeStageEmbryoCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 21, true);
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
			this.LifeStageJuvenileCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 21, true);
			this.LifeStageJuvenileCheckBox.Name = "LifeStageJuvenileCheckBox";
			this.LifeStageJuvenileCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 17, true);
			this.LifeStageJuvenileCheckBox.TabIndex = 2;
			this.LifeStageJuvenileCheckBox.Text = "Juvenile";
			this.LifeStageJuvenileCheckBox.UseVisualStyleBackColor = true;
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("86830c07-056d-4c74-9881-d0dc34ada2f9", "Details");
			this.MainGroupBox.Controls.Add(this.GenusOrSpeciesTextBox);
			this.MainGroupBox.Controls.Add(this.DirectionDropEdit);
			this.MainGroupBox.Controls.Add(this.EvisceratedCheckBox);
			this.MainGroupBox.Controls.Add(this.ManufacturerUserControl);
			this.MainGroupBox.Controls.Add(this.ProcessorAddressControl);
			this.MainGroupBox.Controls.Add(this.HarvestingPartyAddressControl);
			this.MainGroupBox.Controls.Add(this.GenusOrSpeciesDropEdit);
			this.MainGroupBox.Controls.Add(this.CommonNameTextBox);
			this.MainGroupBox.Controls.Add(this.CountIntEdit);
			this.MainGroupBox.Controls.Add(this.TSNTextBox);
			this.MainGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 190, true);
			this.MainGroupBox.TabIndex = 3;
			this.MainGroupBox.TabStop = false;
			this.MainGroupBox.Text = "Details";
			// 
			// GenusOrSpeciesTextBox
			// 
			this.BindingSource.SetBindingMember(this.GenusOrSpeciesTextBox, "CA_GenusOrSpecies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_GenusOrSpecies)));
			this.GenusOrSpeciesTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("041c62c0-0d67-4821-ad34-b98ad0bbbb83", "Genus/Species");
			this.GenusOrSpeciesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 69, true);
			this.GenusOrSpeciesTextBox.Name = "GenusOrSpeciesTextBox";
			this.GenusOrSpeciesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.GenusOrSpeciesTextBox.TabIndex = 2;
			// 
			// DirectionDropEdit
			// 
			this.DirectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DirectionDropEdit, "CA_Direction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).AddInfoLookups.DirectionList)));
			this.DirectionDropEdit.BindToList = "AddInfoLookups.DirectionList";
			this.DirectionDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("81a19e15-944c-436a-8026-01046e07a774", "Authority Party");
			this.DirectionDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 69, true);
			this.DirectionDropEdit.Name = "DirectionDropEdit";
			this.DirectionDropEdit.ShouldResizeByMaxLength = true;
			this.DirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.DirectionDropEdit.TabIndex = 8;
			// 
			// EvisceratedCheckBox
			// 
			this.EvisceratedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EvisceratedCheckBox, "CA_Eviscerated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_Eviscerated)));
			this.EvisceratedCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c91437e4-4a19-4b0d-9733-695e8b5980c9", "Eviscerated");
			this.EvisceratedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EvisceratedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 20, true);
			this.EvisceratedCheckBox.Name = "EvisceratedCheckBox";
			this.EvisceratedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 17, true);
			this.EvisceratedCheckBox.TabIndex = 6;
			this.EvisceratedCheckBox.Text = "Eviscerated";
			this.EvisceratedCheckBox.UseVisualStyleBackColor = true;
			// 
			// ProcessorAddressControl
			// 
			this.ProcessorAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProcessorAddressControl, "CA_OA_Processor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_OA_Processor)));
			this.ProcessorAddressControl.BindToOrgList = "AddInfoLookups+Processors";
			this.ProcessorAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2752d720-cc75-495c-8375-aecf41c0318e", "Processor");
			this.ProcessorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 121, true);
			this.ProcessorAddressControl.Name = "ProcessorAddressControl";
			this.ProcessorAddressControl.PopupCaption = "";
			this.ProcessorAddressControl.ReadOnly = false;
			this.ProcessorAddressControl.ShowAddress = false;
			this.ProcessorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ProcessorAddressControl.TabIndex = 10;
			// 
			// GenusOrSpeciesDropEdit
			// 
			this.GenusOrSpeciesDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GenusOrSpeciesDropEdit, "CA_SpeciesCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).CA_SpeciesCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.DFOPGAHeader)(null)).AddInfoLookups.ScientificNames)));
			this.GenusOrSpeciesDropEdit.BindToList = "AddInfoLookups.ScientificNames";
			this.GenusOrSpeciesDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("00b2b92a-b5cf-4659-8de6-6d3933c99668", "Species Code");
			this.GenusOrSpeciesDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GenusOrSpeciesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 43, true);
			this.GenusOrSpeciesDropEdit.Name = "GenusOrSpeciesDropEdit";
			this.GenusOrSpeciesDropEdit.ShouldResizeByMaxLength = true;
			this.GenusOrSpeciesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.GenusOrSpeciesDropEdit.TabIndex = 1;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.IntendedUseCodeGroupBox);
			this.TopPanel.Controls.Add(this.LifeStageGroupBox);
			this.TopPanel.Controls.Add(this.SexGroupBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 70, true);
			this.TopPanel.TabIndex = 0;
			// 
			// FillPanel
			// 
			this.FillPanel.Controls.Add(this.LpcoGroupBox);
			this.FillPanel.Controls.Add(this.MainGroupBox);
			this.FillPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FillPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 70, true);
			this.FillPanel.Name = "FillPanel";
			this.FillPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 190, true);
			this.FillPanel.TabIndex = 1;
			// 
			// LpcoGroupBox
			// 
			this.LpcoGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fc62934f-65f7-4d00-a4ea-688f5316d99c", "LPCOs");
			this.LpcoGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.LpcoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LpcoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 0, true);
			this.LpcoGroupBox.Name = "LpcoGroupBox";
			this.LpcoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 190, true);
			this.LpcoGroupBox.TabIndex = 5;
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
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 171, true);
			this.LPCOGridUserControl.TabIndex = 0;
			// 
			// AISProgramUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 260, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FillPanel);
			this.Controls.Add(this.TopPanel);
			this.Name = "AISProgramUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1025, 243, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManufacturerUserControl.ResumeLayout(true);
			this.ManufacturerUserControl.PerformLayout();
			this.HarvestingPartyAddressControl.ResumeLayout(true);
			this.HarvestingPartyAddressControl.PerformLayout();
			this.IntendedUseCodeGroupBox.ResumeLayout(false);
			this.IntendedUseCodeGroupBox.PerformLayout();
			this.SexGroupBox.ResumeLayout(false);
			this.SexGroupBox.PerformLayout();
			this.LifeStageGroupBox.ResumeLayout(false);
			this.LifeStageGroupBox.PerformLayout();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.DirectionDropEdit.ResumeLayout(true);
			this.DirectionDropEdit.PerformLayout();
			this.ProcessorAddressControl.ResumeLayout(true);
			this.ProcessorAddressControl.PerformLayout();
			this.GenusOrSpeciesDropEdit.ResumeLayout(true);
			this.GenusOrSpeciesDropEdit.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.FillPanel.ResumeLayout(false);
			this.FillPanel.PerformLayout();
			this.LpcoGroupBox.ResumeLayout(false);
			this.LpcoGroupBox.PerformLayout();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		ZArchitecture.GUI.ZAddressControl ManufacturerUserControl;
		ZArchitecture.GUI.ZCheckBox LifeStageAdultCheckBox;
		ZArchitecture.GUI.ZCheckBox LifeStagePropagateCheckBox;
		ZArchitecture.GUI.ZCheckBox LifeStageJuvenileCheckBox;
		ZArchitecture.GUI.ZCheckBox LifeStageEmbryoCheckBox;
		ZArchitecture.GUI.ZGroupBox LifeStageGroupBox;
		ZArchitecture.GUI.ZGroupBox SexGroupBox;
		ZArchitecture.GUI.ZCheckBox SexMaleCheckBox;
		ZArchitecture.GUI.ZCheckBox SexFemaleCheckBox;
		ZArchitecture.GUI.ZCheckBox SexUnknownCheckBox;
		ZArchitecture.GUI.ZCheckBox SexOtherCheckBox;
		ZArchitecture.GUI.ZCheckBox SexSterileCheckBox;
		ZArchitecture.GUI.ZGroupBox IntendedUseCodeGroupBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseScientificCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseEducationalCheckBox;
		ZArchitecture.GUI.ZCheckBox IntendedUseAquaticInvasiveSpeciesControlCheckBox;
		ZArchitecture.ZTextBox TSNTextBox;
		ZArchitecture.GUI.ZIntEdit CountIntEdit;
		ZArchitecture.ZTextBox CommonNameTextBox;
		ZArchitecture.GUI.ZAddressControl HarvestingPartyAddressControl;
		ZArchitecture.GUI.ZGroupBox MainGroupBox;
		ZArchitecture.GUI.ZCheckBox LifeStageLiveCheckBox;
		ZArchitecture.GUI.ZCheckBox LifeStageDeadCheckBox;
		ZArchitecture.GUI.ZCheckBox EvisceratedCheckBox;
		ZArchitecture.GUI.ZAddressControl ProcessorAddressControl;
		ZArchitecture.GUI.ZDropEdit GenusOrSpeciesDropEdit;
		ZDropEdit DirectionDropEdit;
		ZArchitecture.ZTextBox GenusOrSpeciesTextBox;
		ZPanel TopPanel;
		ZPanel FillPanel;
		ZGroupBox LpcoGroupBox;
		internal LPCOGridUserControl LPCOGridUserControl;
	}
}

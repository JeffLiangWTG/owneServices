namespace Enterprise.Customs.CA.GUI
{
	partial class ECCCVehicleUserControl
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
			if (disposing)
			{
				if (DataSource is Enterprise.Customs.CA.Business.ECCCPGAHeader eccc)
				{
					eccc.CA_ProcessCodeInfo.ValueChanged -= CA_ProcessCodeInfo_ValueChanged;
				}

				if (components != null)
				{
					components.Dispose();
				}
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
			this.MachineGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MachineManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.MachineModelYearDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MachineModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MachineMakeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EngineGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EngineManufacturerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EngineModelYearDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EngineModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PowerCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.EngineClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EvaporativeFamilyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TestGroupTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EngineFamilyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EngineIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EngineMakeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AlternativeStandardOfEngineClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EngineLocationAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.EvidenceOfConformityLocationAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.AOSConformityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AOSReplacementDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AOSEvidenceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AOSRetentionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VehicleGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.VehicleManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.VehicleModelYearDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VehicleModelTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.VehicleClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VINTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VehicleMakeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NonCommercialImportCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReplacementEnginesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BulkReportingApprovalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ProcessCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IncompleteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EPACertifiedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TransitionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CanadaUniqueCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NationalMarkCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EngineComplianceStatementGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CA_ENGIncompleteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CA_ENGEPACertifiedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CA_ENGCanadaUniqueCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CA_ENGNationalMarkCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LPCOGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			this.LPCOGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MachineGroupBox.SuspendLayout();
			this.MachineManufacturerAddressControl.SuspendLayout();
			this.MachineModelYearDropEdit.SuspendLayout();
			this.EngineGroupBox.SuspendLayout();
			this.EngineModelYearDropEdit.SuspendLayout();
			this.PowerCalcDropEdit.SuspendLayout();
			this.EngineClassDropEdit.SuspendLayout();
			this.AlternativeStandardOfEngineClassDropEdit.SuspendLayout();
			this.EngineLocationAddressControl.SuspendLayout();
			this.EvidenceOfConformityLocationAddressControl.SuspendLayout();
			this.AOSConformityDropEdit.SuspendLayout();
			this.AOSReplacementDropEdit.SuspendLayout();
			this.AOSEvidenceDropEdit.SuspendLayout();
			this.AOSRetentionDropEdit.SuspendLayout();
			this.VehicleGroupBox.SuspendLayout();
			this.VehicleManufacturerAddressControl.SuspendLayout();
			this.VehicleModelYearDropEdit.SuspendLayout();
			this.VehicleClassDropEdit.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.ProcessCodeDropEdit.SuspendLayout();
			this.EngineComplianceStatementGroupBox.SuspendLayout();
			this.LPCOGridUserControl.SuspendLayout();
			this.LPCOGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.ECCCPGAHeader);
			// 
			// MachineGroupBox
			// 
			this.MachineGroupBox.Controls.Add(this.MachineManufacturerAddressControl);
			this.MachineGroupBox.Controls.Add(this.MachineModelYearDropEdit);
			this.MachineGroupBox.Controls.Add(this.MachineModelTextBox);
			this.MachineGroupBox.Controls.Add(this.MachineMakeTextBox);
			this.MachineGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.MachineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 444, true);
			this.MachineGroupBox.Name = "MachineGroupBox";
			this.MachineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 79, true);
			this.MachineGroupBox.TabIndex = 4;
			this.MachineGroupBox.TabStop = false;
			this.MachineGroupBox.Text = "Vehicle/Machine";
			// 
			// MachineManufacturerAddressControl
			// 
			this.MachineManufacturerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MachineManufacturerAddressControl, "CA_MachineManufacturer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_MachineManufacturer)));
			this.MachineManufacturerAddressControl.BindToOrgList = "AddInfoLookups.Manufacturers";
			this.MachineManufacturerAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1ea70957-0647-4e73-b100-34abdf6662c1", "Manufacturer");
			this.MachineManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(659, 19, true);
			this.MachineManufacturerAddressControl.Name = "MachineManufacturerAddressControl";
			this.MachineManufacturerAddressControl.PopupCaption = "";
			this.MachineManufacturerAddressControl.ReadOnly = false;
			this.MachineManufacturerAddressControl.ShowAddress = false;
			this.MachineManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 15, true);
			this.MachineManufacturerAddressControl.TabIndex = 2;
			// 
			// MachineModelYearDropEdit
			// 
			this.MachineModelYearDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MachineModelYearDropEdit, "CA_MachineModelYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_MachineModelYear)));
			this.MachineModelYearDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e095d116-9d23-482a-b6dc-680e2a3bff55", "Model Year");
			this.MachineModelYearDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 45, true);
			this.MachineModelYearDropEdit.Name = "MachineModelYearDropEdit";
			this.MachineModelYearDropEdit.ShouldResizeByMaxLength = true;
			this.MachineModelYearDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 15, true);
			this.MachineModelYearDropEdit.TabIndex = 3;
			// 
			// MachineModelTextBox
			// 
			this.BindingSource.SetBindingMember(this.MachineModelTextBox, "CA_ModelOfMachine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_ModelOfMachine)));
			this.MachineModelTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4407997e-3718-4c8b-a8cb-fca082e08c2a", "Model");
			this.MachineModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 19, true);
			this.MachineModelTextBox.Name = "MachineModelTextBox";
			this.MachineModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 15, true);
			this.MachineModelTextBox.TabIndex = 1;
			// 
			// MachineMakeTextBox
			// 
			this.BindingSource.SetBindingMember(this.MachineMakeTextBox, "CA_MakeOfMachine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_MakeOfMachine)));
			this.MachineMakeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("16418a49-c4e4-44db-b661-4ce5d0af068a", "Make");
			this.MachineMakeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 19, true);
			this.MachineMakeTextBox.Name = "MachineMakeTextBox";
			this.MachineMakeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 15, true);
			this.MachineMakeTextBox.TabIndex = 0;
			// 
			// EngineGroupBox
			// 
			this.EngineGroupBox.Controls.Add(this.EngineManufacturerTextBox);
			this.EngineGroupBox.Controls.Add(this.EngineModelYearDropEdit);
			this.EngineGroupBox.Controls.Add(this.EngineModelTextBox);
			this.EngineGroupBox.Controls.Add(this.PowerCalcDropEdit);
			this.EngineGroupBox.Controls.Add(this.EngineClassDropEdit);
			this.EngineGroupBox.Controls.Add(this.EvaporativeFamilyTextBox);
			this.EngineGroupBox.Controls.Add(this.TestGroupTextBox);
			this.EngineGroupBox.Controls.Add(this.EngineFamilyTextBox);
			this.EngineGroupBox.Controls.Add(this.EngineIDTextBox);
			this.EngineGroupBox.Controls.Add(this.EngineMakeTextBox);
			this.EngineGroupBox.Controls.Add(this.AlternativeStandardOfEngineClassDropEdit);
			this.EngineGroupBox.Controls.Add(this.EngineLocationAddressControl);
			this.EngineGroupBox.Controls.Add(this.EvidenceOfConformityLocationAddressControl);
			this.EngineGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.EngineGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 289, true);
			this.EngineGroupBox.Name = "EngineGroupBox";
			this.EngineGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 125, true);
			this.EngineGroupBox.TabIndex = 3;
			this.EngineGroupBox.TabStop = false;
			this.EngineGroupBox.Text = "Engine";
			// 
			// EngineManufacturerTextBox
			// 
			this.EngineManufacturerTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EngineManufacturerTextBox, "CA_EngineManufacturer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_EngineManufacturer)));
			this.EngineManufacturerTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("42864351-b18d-456c-be0d-2a3c98f28c1b", "Manufacturer");
			this.EngineManufacturerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(659, 45, true);
			this.EngineManufacturerTextBox.Name = "EngineManufacturerTextBox";
			this.EngineManufacturerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 15, true);
			this.EngineManufacturerTextBox.TabIndex = 5;
			// 
			// EngineModelYearDropEdit
			// 
			this.EngineModelYearDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EngineModelYearDropEdit, "CA_EngineModelYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_EngineModelYear)));
			this.EngineModelYearDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("86208423-56cb-4c79-8acc-bb58ccc35a6b", "Model Year");
			this.EngineModelYearDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 45, true);
			this.EngineModelYearDropEdit.Name = "EngineModelYearDropEdit";
			this.EngineModelYearDropEdit.ShouldResizeByMaxLength = true;
			this.EngineModelYearDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 15, true);
			this.EngineModelYearDropEdit.TabIndex = 3;
			// 
			// EngineModelTextBox
			// 
			this.BindingSource.SetBindingMember(this.EngineModelTextBox, "CA_ModelOfEngine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_ModelOfEngine)));
			this.EngineModelTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8ecbc913-de5c-4433-a065-65c2b21916f6", "Model");
			this.EngineModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(659, 19, true);
			this.EngineModelTextBox.Name = "EngineModelTextBox";
			this.EngineModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 15, true);
			this.EngineModelTextBox.TabIndex = 2;
			// 
			// PowerCalcDropEdit
			// 
			this.PowerCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PowerCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_EnginePowerRating)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_PowerRatingUQ)));
			this.PowerCalcDropEdit.BindToAmount = "CA_EnginePowerRating";
			this.PowerCalcDropEdit.BindToUnit = "CA_PowerRatingUQ";
			this.PowerCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("27687b17-166f-4089-b4cf-b0df23c0ca7c", "Power Rating");
			this.PowerCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(659, 71, true);
			this.PowerCalcDropEdit.Name = "PowerCalcDropEdit";
			this.PowerCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 15, true);
			this.PowerCalcDropEdit.TabIndex = 8;
			// 
			// EngineClassDropEdit
			// 
			this.EngineClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EngineClassDropEdit, "CA_EngineClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_EngineClass)));
			this.EngineClassDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d7b4b11c-36fc-474e-a2ab-fd2d41d448ee", "Class");
			this.EngineClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 19, true);
			this.EngineClassDropEdit.Name = "EngineClassDropEdit";
			this.EngineClassDropEdit.ShouldResizeByMaxLength = true;
			this.EngineClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 15, true);
			this.EngineClassDropEdit.TabIndex = 0;
			// 
			// EvaporativeFamilyTextBox
			// 
			this.BindingSource.SetBindingMember(this.EvaporativeFamilyTextBox, "CA_EvaporativeFamily");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_EvaporativeFamily)));
			this.EvaporativeFamilyTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1c2596e3-9f0d-4ec0-9530-38f1c73db648", "Evaporative Family");
			this.EvaporativeFamilyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 97, true);
			this.EvaporativeFamilyTextBox.Name = "EvaporativeFamilyTextBox";
			this.EvaporativeFamilyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 15, true);
			this.EvaporativeFamilyTextBox.TabIndex = 9;
			// 
			// TestGroupTextBox
			// 
			this.BindingSource.SetBindingMember(this.TestGroupTextBox, "CA_TestGroupName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_TestGroupName)));
			this.TestGroupTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("43ebc17b-ed92-473c-94d0-5aa613ac3dd4", "Test Group");
			this.TestGroupTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 71, true);
			this.TestGroupTextBox.Name = "TestGroupTextBox";
			this.TestGroupTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 15, true);
			this.TestGroupTextBox.TabIndex = 7;
			// 
			// EngineFamilyTextBox
			// 
			this.BindingSource.SetBindingMember(this.EngineFamilyTextBox, "CA_EngineFamilyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_EngineFamilyName)));
			this.EngineFamilyTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e3a3134e-2393-4ea4-bb57-ae1b01e1cf7c", "Family Name");
			this.EngineFamilyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 71, true);
			this.EngineFamilyTextBox.Name = "EngineFamilyTextBox";
			this.EngineFamilyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 15, true);
			this.EngineFamilyTextBox.TabIndex = 6;
			// 
			// EngineIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.EngineIDTextBox, "CA_EngineIDNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_EngineIDNumber)));
			this.EngineIDTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c51c5c4f-0838-44f7-acf1-851509d6bc0d", "ID Number");
			this.EngineIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 45, true);
			this.EngineIDTextBox.Name = "EngineIDTextBox";
			this.EngineIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 15, true);
			this.EngineIDTextBox.TabIndex = 4;
			// 
			// EngineMakeTextBox
			// 
			this.BindingSource.SetBindingMember(this.EngineMakeTextBox, "CA_MakeOfEngine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_MakeOfEngine)));
			this.EngineMakeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7cd0e95b-4ff4-4816-8c93-4e8ae03e4c28", "Make");
			this.EngineMakeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 19, true);
			this.EngineMakeTextBox.Name = "EngineMakeTextBox";
			this.EngineMakeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 15, true);
			this.EngineMakeTextBox.TabIndex = 1;
			//
			// AlternativeStandardOfEngineClassDropEdit
			//
			this.AlternativeStandardOfEngineClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AlternativeStandardOfEngineClassDropEdit, "CA_AlternativeStandardOfEngineClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_AlternativeStandardOfEngineClass)));
			this.AlternativeStandardOfEngineClassDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2F23698A-8459-4239-96B3-10CFF22D830B", "Alternative Standard of Engine Class");
			this.AlternativeStandardOfEngineClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(659, 97, true);
			this.AlternativeStandardOfEngineClassDropEdit.Name = "AlternativeStandardOfEngineClassDropEdit";
			this.AlternativeStandardOfEngineClassDropEdit.ShouldResizeByMaxLength = true;
			this.AlternativeStandardOfEngineClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 15, true);
			this.AlternativeStandardOfEngineClassDropEdit.TabIndex = 10;
			//
			// EngineLocationAddressControl
			//
			this.EngineLocationAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EngineLocationAddressControl, "CA_OA_EngineLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_OA_EngineLocation)));
			this.EngineLocationAddressControl.BindToOrgList = "AddInfoLookups.AllOrganisations";
			this.EngineLocationAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("92DA77D7-6116-4323-99C1-2C2EB13363BB", "Engine Location");
			this.EngineLocationAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 125, true);
			this.EngineLocationAddressControl.Name = "EngineLocationAddressControl";
			this.EngineLocationAddressControl.PopupCaption = "";
			this.EngineLocationAddressControl.ReadOnly = false;
			this.EngineLocationAddressControl.ShowAddress = false;
			this.EngineLocationAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 15, true);
			this.EngineLocationAddressControl.TabIndex = 11;
			//
			// EvidenceOfConformityLocationAddressControl
			//
			this.EvidenceOfConformityLocationAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EvidenceOfConformityLocationAddressControl, "CA_OA_EvidenceOfConformityLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_OA_EvidenceOfConformityLocation)));
			this.EvidenceOfConformityLocationAddressControl.BindToOrgList = "AddInfoLookups.AllOrganisations";
			this.EvidenceOfConformityLocationAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("A8E82FD1-C607-4EF5-BF16-785FBD35587E", "Evidence of Conformity Location");
			this.EvidenceOfConformityLocationAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(659, 125, true);
			this.EvidenceOfConformityLocationAddressControl.Name = "EvidenceOfConformityLocationAddressControl";
			this.EvidenceOfConformityLocationAddressControl.PopupCaption = "";
			this.EvidenceOfConformityLocationAddressControl.ReadOnly = false;
			this.EvidenceOfConformityLocationAddressControl.ShowAddress = false;
			this.EvidenceOfConformityLocationAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 15, true);
			this.EvidenceOfConformityLocationAddressControl.TabIndex = 12;
			// 
			// VehicleGroupBox
			// 
			this.VehicleGroupBox.Controls.Add(this.VehicleManufacturerAddressControl);
			this.VehicleGroupBox.Controls.Add(this.VehicleModelYearDropEdit);
			this.VehicleGroupBox.Controls.Add(this.VehicleModelTextBox2);
			this.VehicleGroupBox.Controls.Add(this.VehicleClassDropEdit);
			this.VehicleGroupBox.Controls.Add(this.VINTextBox);
			this.VehicleGroupBox.Controls.Add(this.VehicleMakeTextBox);
			this.VehicleGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.VehicleGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 311, true);
			this.VehicleGroupBox.Name = "VehicleGroupBox";
			this.VehicleGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 78, true);
			this.VehicleGroupBox.TabIndex = 2;
			this.VehicleGroupBox.TabStop = false;
			this.VehicleGroupBox.Text = "Vehicle";
			// 
			// VehicleManufacturerAddressControl
			// 
			this.VehicleManufacturerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleManufacturerAddressControl, "OA_Manufacturer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).OA_Manufacturer)));
			this.VehicleManufacturerAddressControl.BindToOrgList = "RequirementsParent.ManufacturersLookup";
			this.VehicleManufacturerAddressControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("13406e65-a3e2-4965-aa3f-99766d2be380", "Manufacturer");
			this.VehicleManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(662, 45, true);
			this.VehicleManufacturerAddressControl.Name = "VehicleManufacturerAddressControl";
			this.VehicleManufacturerAddressControl.PopupCaption = "";
			this.VehicleManufacturerAddressControl.ReadOnly = false;
			this.VehicleManufacturerAddressControl.ShowAddress = false;
			this.VehicleManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 15, true);
			this.VehicleManufacturerAddressControl.TabIndex = 6;
			// 
			// VehicleModelYearDropEdit
			// 
			this.VehicleModelYearDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleModelYearDropEdit, "InvoiceLine.CA_ModelYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).InvoiceLine.CA_ModelYear)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).InvoiceLine.AddInfoLookups.ModelYearList)));
			this.VehicleModelYearDropEdit.BindToList = "InvoiceLine.AddInfoLookups.ModelYearList";
			this.VehicleModelYearDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9f94138e-6ae4-466b-ad84-5d4ad495a8bf", "Model Year");
			this.VehicleModelYearDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 45, true);
			this.VehicleModelYearDropEdit.Name = "VehicleModelYearDropEdit";
			this.VehicleModelYearDropEdit.ShouldResizeByMaxLength = true;
			this.VehicleModelYearDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 15, true);
			this.VehicleModelYearDropEdit.TabIndex = 4;
			// 
			// VehicleModelTextBox2
			// 
			this.BindingSource.SetBindingMember(this.VehicleModelTextBox2, "JI_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).JI_Model)));
			this.VehicleModelTextBox2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5f3030c3-a7c1-4e40-b37e-b7cb5943c31d", "Model");
			this.VehicleModelTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(662, 19, true);
			this.VehicleModelTextBox2.Name = "VehicleModelTextBox2";
			this.VehicleModelTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 15, true);
			this.VehicleModelTextBox2.TabIndex = 3;
			// 
			// VehicleClassDropEdit
			// 
			this.VehicleClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VehicleClassDropEdit, "CA_VehicleClass");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_VehicleClass)));
			this.VehicleClassDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d9b718b7-dbe9-458b-b81c-a3ff76a1eb32", "Class");
			this.VehicleClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 19, true);
			this.VehicleClassDropEdit.Name = "VehicleClassDropEdit";
			this.VehicleClassDropEdit.ShouldResizeByMaxLength = true;
			this.VehicleClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 15, true);
			this.VehicleClassDropEdit.TabIndex = 1;
			// 
			// VINTextBox
			// 
			this.BindingSource.SetBindingMember(this.VINTextBox, "InvoiceLine.CA_VINNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).InvoiceLine.CA_VINNumber)));
			this.VINTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("cf0cd0c0-47b1-4001-a73a-0d6b364464b4", "VIN");
			this.VINTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 45, true);
			this.VINTextBox.Name = "VINTextBox";
			this.VINTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 15, true);
			this.VINTextBox.TabIndex = 5;
			// 
			// VehicleMakeTextBox
			// 
			this.BindingSource.SetBindingMember(this.VehicleMakeTextBox, "JI_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).JI_BrandName)));
			this.VehicleMakeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f6ae359f-5bc4-468a-9d05-06dd2d363d32", "Make");
			this.VehicleMakeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 19, true);
			this.VehicleMakeTextBox.Name = "VehicleMakeTextBox";
			this.VehicleMakeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 15, true);
			this.VehicleMakeTextBox.TabIndex = 2;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.NonCommercialImportCheckBox);
			this.DetailsGroupBox.Controls.Add(this.ReplacementEnginesCheckBox);
			this.DetailsGroupBox.Controls.Add(this.BulkReportingApprovalCheckBox);
			this.DetailsGroupBox.Controls.Add(this.ProcessCodeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.IncompleteCheckBox);
			this.DetailsGroupBox.Controls.Add(this.EPACertifiedCheckBox);
			this.DetailsGroupBox.Controls.Add(this.TransitionCheckBox);
			this.DetailsGroupBox.Controls.Add(this.CanadaUniqueCheckBox);
			this.DetailsGroupBox.Controls.Add(this.NationalMarkCheckBox);
			this.DetailsGroupBox.Controls.Add(this.AOSConformityDropEdit);
			this.DetailsGroupBox.Controls.Add(this.AOSReplacementDropEdit);
			this.DetailsGroupBox.Controls.Add(this.AOSEvidenceDropEdit);
			this.DetailsGroupBox.Controls.Add(this.AOSRetentionDropEdit);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 100, true);
			this.DetailsGroupBox.TabIndex = 1;
			this.DetailsGroupBox.TabStop = false;
			this.DetailsGroupBox.Text = "Details";
			// 
			// ProcessCodeDropEdit
			// 
			this.ProcessCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProcessCodeDropEdit, "CA_ProcessCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_ProcessCode)));
			this.ProcessCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a268d9b8-d99a-4521-b43a-9ccf992025fc", "Process Code");
			this.ProcessCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 19, true);
			this.ProcessCodeDropEdit.Name = "ProcessCodeDropEdit";
			this.ProcessCodeDropEdit.ShouldResizeByMaxLength = true;
			this.ProcessCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 15, true);
			this.ProcessCodeDropEdit.TabIndex = 0;
			// 
			// NationalMarkCheckBox
			// 
			this.NationalMarkCheckBox.AllowDrop = true;
			this.NationalMarkCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NationalMarkCheckBox, "CA_NationalMark");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_NationalMark)));
			this.NationalMarkCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("764fa858-36b7-4232-b834-06d44ad61646", "National Emissions Mark");
			this.NationalMarkCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NationalMarkCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 45, true);
			this.NationalMarkCheckBox.Name = "NationalMarkCheckBox";
			this.NationalMarkCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 14, true);
			this.NationalMarkCheckBox.TabIndex = 1;
			// 
			// BulkReportingApprovalCheckBox
			// 
			this.BulkReportingApprovalCheckBox.AllowDrop = true;
			this.BulkReportingApprovalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BulkReportingApprovalCheckBox, "CA_BulkReporting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_BulkReporting)));
			this.BulkReportingApprovalCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ada19960-42d6-40fe-8082-cda09d296554", "Bulk Reporting Approval");
			this.BulkReportingApprovalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BulkReportingApprovalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 45, true);
			this.BulkReportingApprovalCheckBox.Name = "BulkReportingApprovalCheckBox";
			this.BulkReportingApprovalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 14, true);
			this.BulkReportingApprovalCheckBox.TabIndex = 2;
			// 
			// EPACertifiedCheckBox
			// 
			this.EPACertifiedCheckBox.AllowDrop = true;
			this.EPACertifiedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EPACertifiedCheckBox, "CA_EPACertified");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_EPACertified)));
			this.EPACertifiedCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("99f481f1-673f-4bb6-a129-20041a16f16b", "EPA Certified or Considered Equivalent to EPA");
			this.EPACertifiedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EPACertifiedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 45, true);
			this.EPACertifiedCheckBox.Name = "EPACertifiedCheckBox";
			this.EPACertifiedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 14, true);
			this.EPACertifiedCheckBox.TabIndex = 3;
			// 
			// TransitionCheckBox
			// 
			this.TransitionCheckBox.AllowDrop = true;
			this.TransitionCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.TransitionCheckBox, "CA_Transition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_Transition)));
			this.TransitionCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d3bf2680-e220-4b99-ab62-59a1501dda5a", "Transition");
			this.TransitionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TransitionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 45, true);
			this.TransitionCheckBox.Name = "TransitionCheckBox";
			this.TransitionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 14, true);
			this.TransitionCheckBox.TabIndex = 4;
			// 
			// IncompleteCheckBox
			// 
			this.IncompleteCheckBox.AllowDrop = true;
			this.IncompleteCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncompleteCheckBox, "CA_Incomplete");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_Incomplete)));
			this.IncompleteCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ed3a892f-194c-464d-9ff9-89084cf2611d", "Incomplete Vehicles or Engines");
			this.IncompleteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncompleteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 72, true);
			this.IncompleteCheckBox.Name = "IncompleteCheckBox";
			this.IncompleteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 14, true);
			this.IncompleteCheckBox.TabIndex = 5;
			// 
			// NonCommercialImportCheckBox
			// 
			this.NonCommercialImportCheckBox.AllowDrop = true;
			this.NonCommercialImportCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NonCommercialImportCheckBox, "CA_NonCommercialImport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_NonCommercialImport)));
			this.NonCommercialImportCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0a7fd17f-d920-417c-97a6-950e6ef499db", "Non-Commercial Import");
			this.NonCommercialImportCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NonCommercialImportCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 72, true);
			this.NonCommercialImportCheckBox.Name = "NonCommercialImportCheckBox";
			this.NonCommercialImportCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 14, true);
			this.NonCommercialImportCheckBox.TabIndex = 6;
			// 
			// CanadaUniqueCheckBox
			// 
			this.CanadaUniqueCheckBox.AllowDrop = true;
			this.CanadaUniqueCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CanadaUniqueCheckBox, "CA_CanadaUnique");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_CanadaUnique)));
			this.CanadaUniqueCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c7fb1989-9c2e-40d3-9d8f-379c25f475df", "Canada Unique Vehicles, Engines");
			this.CanadaUniqueCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CanadaUniqueCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 72, true);
			this.CanadaUniqueCheckBox.Name = "CanadaUniqueCheckBox";
			this.CanadaUniqueCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 14, true);
			this.CanadaUniqueCheckBox.TabIndex = 7;
			// 
			// ReplacementEnginesCheckBox
			// 
			this.ReplacementEnginesCheckBox.AllowDrop = true;
			this.ReplacementEnginesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ReplacementEnginesCheckBox, "CA_ReplacementEngines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_ReplacementEngines)));
			this.ReplacementEnginesCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1813814A-62DF-4DE8-8666-860FD35416A0", "Replacement Engines");
			this.ReplacementEnginesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReplacementEnginesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 72, true);
			this.ReplacementEnginesCheckBox.Name = "ReplacementEnginesCheckBox";
			this.ReplacementEnginesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 14, true);
			this.ReplacementEnginesCheckBox.TabIndex = 8;
			// 
			// AOSConformityDropEdit
			// 
			this.AOSConformityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AOSConformityDropEdit, "CA_AOSConformity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_AOSConformity)));
			this.AOSConformityDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7F34D03F-37B2-4066-AC16-E0A420CBF839", "Affirmation of Statement Conformity");
			this.AOSConformityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 99, true);
			this.AOSConformityDropEdit.Name = "AOSConformityDropEdit";
			this.AOSConformityDropEdit.ShouldResizeByMaxLength = true;
			this.AOSConformityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
			this.AOSConformityDropEdit.TabIndex = 9;
			// 
			// AOSReplacementDropEdit
			// 
			this.AOSReplacementDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AOSReplacementDropEdit, "CA_AOSReplacement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_AOSReplacement)));
			this.AOSReplacementDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3DA7F268-648D-4A06-BFEF-D6D08F4C9837", "Affirmation of Statement Replacement");
			this.AOSReplacementDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(660, 99, true);
			this.AOSReplacementDropEdit.Name = "AOSReplacementDropEdit";
			this.AOSReplacementDropEdit.ShouldResizeByMaxLength = true;
			this.AOSReplacementDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 17, true);
			this.AOSReplacementDropEdit.TabIndex = 10;
			// 
			// AOSEvidenceDropEdit
			// 
			this.AOSEvidenceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AOSEvidenceDropEdit, "CA_AOSEvidence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_AOSEvidence)));
			this.AOSEvidenceDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5BE18848-E989-4AB6-9D59-B7FB76C0D917", "Affirmation of Statement Evidence");
			this.AOSEvidenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 126, true);
			this.AOSEvidenceDropEdit.Name = "AOSEvidenceDropEdit";
			this.AOSEvidenceDropEdit.ShouldResizeByMaxLength = true;
			this.AOSEvidenceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
			this.AOSEvidenceDropEdit.TabIndex = 11;
			// 
			// AOSRetentionDropEdit
			// 
			this.AOSRetentionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AOSRetentionDropEdit, "CA_AOSRetention");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_AOSRetention)));
			this.AOSRetentionDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("F5CC8AE2-C76B-459D-82A6-011AD791487C", "Affirmation of Statement Retention");
			this.AOSRetentionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(660, 126, true);
			this.AOSRetentionDropEdit.Name = "AOSRetentionDropEdit";
			this.AOSRetentionDropEdit.ShouldResizeByMaxLength = true;
			this.AOSRetentionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 17, true);
			this.AOSRetentionDropEdit.TabIndex = 12;
			// 
			// EngineComplianceStatementGroupBox
			// 
			this.EngineComplianceStatementGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a66d8491-1aae-404f-8291-9d3d699b5b99", "Engine Compliance Statement");
			this.EngineComplianceStatementGroupBox.Controls.Add(this.CA_ENGIncompleteCheckBox);
			this.EngineComplianceStatementGroupBox.Controls.Add(this.CA_ENGEPACertifiedCheckBox);
			this.EngineComplianceStatementGroupBox.Controls.Add(this.CA_ENGCanadaUniqueCheckBox);
			this.EngineComplianceStatementGroupBox.Controls.Add(this.CA_ENGNationalMarkCheckBox);
			this.EngineComplianceStatementGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.EngineComplianceStatementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 493, true);
			this.EngineComplianceStatementGroupBox.Name = "EngineComplianceStatementGroupBox";
			this.EngineComplianceStatementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 79, true);
			this.EngineComplianceStatementGroupBox.TabIndex = 5;
			this.EngineComplianceStatementGroupBox.TabStop = false;
			// 
			// CA_ENGIncompleteCheckBox
			// 
			this.CA_ENGIncompleteCheckBox.AllowDrop = true;
			this.CA_ENGIncompleteCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CA_ENGIncompleteCheckBox, "CA_ENGIncomplete");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_ENGIncomplete)));
			this.CA_ENGIncompleteCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fdd9c3e0-a8f7-4770-b0e3-671439251a11", "Incomplete Vehicles or Engines");
			this.CA_ENGIncompleteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CA_ENGIncompleteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 46, true);
			this.CA_ENGIncompleteCheckBox.Name = "CA_ENGIncompleteCheckBox";
			this.CA_ENGIncompleteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 14, true);
			this.CA_ENGIncompleteCheckBox.TabIndex = 5;
			// 
			// CA_ENGEPACertifiedCheckBox
			// 
			this.CA_ENGEPACertifiedCheckBox.AllowDrop = true;
			this.CA_ENGEPACertifiedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CA_ENGEPACertifiedCheckBox, "CA_ENGEPACertified");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_ENGEPACertified)));
			this.CA_ENGEPACertifiedCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e85ee7d2-e3a7-4f59-99e1-95c918abef0a", "EPA Certified or Considered Equivalent to EPA");
			this.CA_ENGEPACertifiedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CA_ENGEPACertifiedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 19, true);
			this.CA_ENGEPACertifiedCheckBox.Name = "CA_ENGEPACertifiedCheckBox";
			this.CA_ENGEPACertifiedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 14, true);
			this.CA_ENGEPACertifiedCheckBox.TabIndex = 6;
			// 
			// CA_ENGCanadaUniqueCheckBox
			// 
			this.CA_ENGCanadaUniqueCheckBox.AllowDrop = true;
			this.CA_ENGCanadaUniqueCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CA_ENGCanadaUniqueCheckBox, "CA_ENGCanadaUnique");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_ENGCanadaUnique)));
			this.CA_ENGCanadaUniqueCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3a9d5c58-200f-4ec0-8bc6-5e374fac7d7e", "Canada Unique Vehicles, Engines");
			this.CA_ENGCanadaUniqueCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CA_ENGCanadaUniqueCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 46, true);
			this.CA_ENGCanadaUniqueCheckBox.Name = "CA_ENGCanadaUniqueCheckBox";
			this.CA_ENGCanadaUniqueCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 14, true);
			this.CA_ENGCanadaUniqueCheckBox.TabIndex = 8;
			// 
			// CA_ENGNationalMarkCheckBox
			// 
			this.CA_ENGNationalMarkCheckBox.AllowDrop = true;
			this.CA_ENGNationalMarkCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CA_ENGNationalMarkCheckBox, "CA_ENGNationalMark");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_ENGNationalMark)));
			this.CA_ENGNationalMarkCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("08f66ce6-e7f7-46f0-882d-0541fc323a22", "National Emissions Mark");
			this.CA_ENGNationalMarkCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CA_ENGNationalMarkCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 19, true);
			this.CA_ENGNationalMarkCheckBox.Name = "CA_ENGNationalMarkCheckBox";
			this.CA_ENGNationalMarkCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 14, true);
			this.CA_ENGNationalMarkCheckBox.TabIndex = 5;
			// 
			// LPCOGridUserControl
			// 
			this.LPCOGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).LPCOViews)));
			this.LPCOGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.LPCOGridUserControl.Name = "LPCOGridUserControl";
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 90, true);
			this.LPCOGridUserControl.TabIndex = 0;
			// 
			// LPCOGroupBox
			// 
			this.LPCOGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.LPCOGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LPCOGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 450, true);
			this.LPCOGroupBox.Name = "LPCOGroupBox";
			this.LPCOGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 100, true);
			this.LPCOGroupBox.TabIndex = 6;
			this.LPCOGroupBox.TabStop = false;
			this.LPCOGroupBox.Text = "LPCOs";
			// 
			// 
			// ECCCVehicleUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EngineComplianceStatementGroupBox);
			this.Controls.Add(this.MachineGroupBox);
			this.Controls.Add(this.EngineGroupBox);
			this.Controls.Add(this.VehicleGroupBox);
			this.Controls.Add(this.DetailsGroupBox);
			this.Controls.Add(this.LPCOGroupBox);
			this.Name = "ECCCVehicleUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(924, 475, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MachineGroupBox.ResumeLayout(false);
			this.MachineGroupBox.PerformLayout();
			this.MachineManufacturerAddressControl.ResumeLayout(true);
			this.MachineManufacturerAddressControl.PerformLayout();
			this.MachineModelYearDropEdit.ResumeLayout(true);
			this.MachineModelYearDropEdit.PerformLayout();
			this.EngineGroupBox.ResumeLayout(false);
			this.EngineGroupBox.PerformLayout();
			this.EngineModelYearDropEdit.ResumeLayout(true);
			this.EngineModelYearDropEdit.PerformLayout();
			this.PowerCalcDropEdit.ResumeLayout(true);
			this.PowerCalcDropEdit.PerformLayout();
			this.EngineClassDropEdit.ResumeLayout(true);
			this.EngineClassDropEdit.PerformLayout();
			this.AlternativeStandardOfEngineClassDropEdit.ResumeLayout(true);
			this.AlternativeStandardOfEngineClassDropEdit.PerformLayout();
			this.EngineLocationAddressControl.ResumeLayout(true);
			this.EngineLocationAddressControl.PerformLayout();
			this.EvidenceOfConformityLocationAddressControl.ResumeLayout(true);
			this.EvidenceOfConformityLocationAddressControl.PerformLayout();
			this.AOSConformityDropEdit.ResumeLayout(true);
			this.AOSConformityDropEdit.PerformLayout();
			this.AOSReplacementDropEdit.ResumeLayout(true);
			this.AOSReplacementDropEdit.PerformLayout();
			this.AOSEvidenceDropEdit.ResumeLayout(true);
			this.AOSEvidenceDropEdit.PerformLayout();
			this.AOSRetentionDropEdit.ResumeLayout(true);
			this.AOSRetentionDropEdit.PerformLayout();
			this.VehicleGroupBox.ResumeLayout(false);
			this.VehicleGroupBox.PerformLayout();
			this.VehicleManufacturerAddressControl.ResumeLayout(true);
			this.VehicleManufacturerAddressControl.PerformLayout();
			this.VehicleModelYearDropEdit.ResumeLayout(true);
			this.VehicleModelYearDropEdit.PerformLayout();
			this.VehicleClassDropEdit.ResumeLayout(true);
			this.VehicleClassDropEdit.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ProcessCodeDropEdit.ResumeLayout(true);
			this.ProcessCodeDropEdit.PerformLayout();
			this.EngineComplianceStatementGroupBox.ResumeLayout(false);
			this.EngineComplianceStatementGroupBox.PerformLayout();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.LPCOGroupBox.ResumeLayout(false);
			this.LPCOGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit ProcessCodeDropEdit;
		private ZArchitecture.GUI.ZGroupBox VehicleGroupBox;
		private ZArchitecture.GUI.ZGroupBox EngineGroupBox;
		private ZArchitecture.GUI.ZGroupBox MachineGroupBox;
		private ZArchitecture.ZTextBox VehicleMakeTextBox;
		private ZArchitecture.ZTextBox VehicleModelTextBox2;
		private ZArchitecture.ZTextBox EngineMakeTextBox;
		private ZArchitecture.ZTextBox EngineModelTextBox;
		private ZArchitecture.ZTextBox MachineModelTextBox;
		private ZArchitecture.ZTextBox MachineMakeTextBox;
		private ZArchitecture.GUI.ZDropEdit VehicleModelYearDropEdit;
		private ZArchitecture.GUI.ZDropEdit EngineModelYearDropEdit;
		private ZArchitecture.GUI.ZDropEdit MachineModelYearDropEdit;
		private ZArchitecture.GUI.ZAddressControl VehicleManufacturerAddressControl;
		private ZArchitecture.GUI.ZAddressControl MachineManufacturerAddressControl;
		private ZArchitecture.ZTextBox EngineManufacturerTextBox;
		private ZArchitecture.ZTextBox VINTextBox;
		private ZArchitecture.GUI.ZDropEdit VehicleClassDropEdit;
		private ZArchitecture.ZTextBox EngineIDTextBox;
		private ZArchitecture.GUI.ZCalcDropEdit PowerCalcDropEdit;
		private ZArchitecture.GUI.ZDropEdit EngineClassDropEdit;
		private ZArchitecture.ZTextBox EvaporativeFamilyTextBox;
		private ZArchitecture.ZTextBox TestGroupTextBox;
		private ZArchitecture.ZTextBox EngineFamilyTextBox;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZCheckBox NationalMarkCheckBox;
		internal ZArchitecture.GUI.ZCheckBox IncompleteCheckBox;
		private ZArchitecture.GUI.ZCheckBox EPACertifiedCheckBox;
		internal ZArchitecture.GUI.ZCheckBox TransitionCheckBox;
		private ZArchitecture.GUI.ZCheckBox CanadaUniqueCheckBox;
		private ZArchitecture.GUI.ZCheckBox BulkReportingApprovalCheckBox;
		private ZArchitecture.GUI.ZGroupBox EngineComplianceStatementGroupBox;
		private ZArchitecture.GUI.ZCheckBox CA_ENGIncompleteCheckBox;
		private ZArchitecture.GUI.ZCheckBox CA_ENGEPACertifiedCheckBox;
		private ZArchitecture.GUI.ZCheckBox CA_ENGCanadaUniqueCheckBox;
		private ZArchitecture.GUI.ZCheckBox CA_ENGNationalMarkCheckBox;
		private ZArchitecture.GUI.ZCheckBox NonCommercialImportCheckBox;
		private ZArchitecture.GUI.ZCheckBox ReplacementEnginesCheckBox;
		private ZArchitecture.GUI.ZDropEdit AOSConformityDropEdit;
		private ZArchitecture.GUI.ZDropEdit AOSReplacementDropEdit;
		private ZArchitecture.GUI.ZDropEdit AOSEvidenceDropEdit;
		private ZArchitecture.GUI.ZDropEdit AOSRetentionDropEdit;
		private ZArchitecture.GUI.ZDropEdit AlternativeStandardOfEngineClassDropEdit;
		private ZArchitecture.GUI.ZAddressControl EngineLocationAddressControl;
		private ZArchitecture.GUI.ZAddressControl EvidenceOfConformityLocationAddressControl;
		ZArchitecture.GUI.ZGroupBox LPCOGroupBox;
		internal LPCOGridUserControl LPCOGridUserControl;
	}
}

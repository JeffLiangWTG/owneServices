
namespace Enterprise.Customs.IT.NCTS.GUI
{
	partial class DeclarationStatusTabUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.StatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DepartureStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ControlChannelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AcceptanceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CustomsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RegistrationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RegistrationOfficeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RegistrationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MrnTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ArrivalGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ArrivalStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArrivalStatusCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArrivalOfficeDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArrivalOfficeCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.A93NumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.A93Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatusGroupBox.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.DepartureStatusDropEdit.SuspendLayout();
			this.ControlChannelDropEdit.SuspendLayout();
			this.AcceptanceDateEdit.SuspendLayout();
			this.CustomsGroupBox.SuspendLayout();
			this.RegistrationDateEdit.SuspendLayout();
			this.ReleaseDateEdit.SuspendLayout();
			this.ArrivalGroupBox.SuspendLayout();
			this.ArrivalDateEdit.SuspendLayout();
			this.A93NumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.A93Grid)).BeginInit();
			this.A93Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.NCTS.Business.NctsHeader);
			// 
			// StatusGroupBox
			// 
			this.StatusGroupBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("c59af9db-672a-4169-bf3d-01cc5db2b4de", "Status");
			this.StatusGroupBox.Controls.Add(this.MessageStatusDropEdit);
			this.StatusGroupBox.Controls.Add(this.DepartureStatusDropEdit);
			this.StatusGroupBox.Controls.Add(this.ControlChannelDropEdit);
			this.StatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 8, true);
			this.StatusGroupBox.Name = "StatusGroupBox";
			this.StatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 126, true);
			this.StatusGroupBox.TabIndex = 0;
			this.StatusGroupBox.TabStop = false;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "EffectiveMessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).EffectiveMessageStatus)));
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 29, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.MessageStatusDropEdit.TabIndex = 0;
			// 
			// DepartureStatusDropEdit
			// 
			this.DepartureStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartureStatusDropEdit, "MovementHeader.BM_CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_CustomsStatus)));
			this.DepartureStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 54, true);
			this.DepartureStatusDropEdit.Name = "DepartureStatusDropEdit";
			this.DepartureStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DepartureStatusDropEdit.TabIndex = 1;
			// 
			// ControlChannelDropEdit
			// 
			this.ControlChannelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ControlChannelDropEdit, "MovementHeader.BM_ControlChannel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_ControlChannel)));
			this.ControlChannelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 80, true);
			this.ControlChannelDropEdit.Name = "ControlChannelDropEdit";
			this.ControlChannelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ControlChannelDropEdit.TabIndex = 2;
			// 
			// AcceptanceDateEdit
			// 
			this.AcceptanceDateEdit.AllowDrop = true;
			this.AcceptanceDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AcceptanceDateEdit, "MovementHeader.BM_EntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_EntryDate)));
			this.AcceptanceDateEdit.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("644d1fa9-6883-41e9-829c-78f5dd6a6e64", "Acceptance Date");
			this.AcceptanceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 19, true);
			this.AcceptanceDateEdit.Name = "AcceptanceDateEdit";
			this.AcceptanceDateEdit.TabIndex = 3;
			// 
			// CustomsGroupBox
			// 
			this.CustomsGroupBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("7372f669-4d39-49a9-9446-47eff09c2de7", "Customs");
			this.CustomsGroupBox.Controls.Add(this.AcceptanceDateEdit);
			this.CustomsGroupBox.Controls.Add(this.RegistrationNumberTextBox);
			this.CustomsGroupBox.Controls.Add(this.RegistrationOfficeTextBox);
			this.CustomsGroupBox.Controls.Add(this.RegistrationDateEdit);
			this.CustomsGroupBox.Controls.Add(this.MrnTextBox);
			this.CustomsGroupBox.Controls.Add(this.ReleaseCodeTextBox);
			this.CustomsGroupBox.Controls.Add(this.ReleaseDateEdit);
			this.CustomsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 135, true);
			this.CustomsGroupBox.Name = "CustomsGroupBox";
			this.CustomsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 201, true);
			this.CustomsGroupBox.TabIndex = 2;
			this.CustomsGroupBox.TabStop = false;
			// 
			// RegistrationNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegistrationNumberTextBox, "EntryNumbersProvider.RegistrationInfo.CE_EntryNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).EntryNumbersProvider.RegistrationInfo.CE_EntryNum)));
			this.RegistrationNumberTextBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("d550f4f3-b1ee-47d0-a0cf-021918f3a383", "Registration No.");
			this.RegistrationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 44, true);
			this.RegistrationNumberTextBox.Name = "RegistrationNumberTextBox";
			this.RegistrationNumberTextBox.ReadOnly = true;
			this.RegistrationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.RegistrationNumberTextBox.TabIndex = 4;
			// 
			// RegistrationOfficeTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegistrationOfficeTextBox, "EntryNumbersProvider.RegistrationInfo.CE_EntryLineReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).EntryNumbersProvider.RegistrationInfo.CE_EntryLineReference)));
			this.RegistrationOfficeTextBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("ce479a02-03a9-4e6e-8851-5ad095e9df86", "Registration Office");
			this.RegistrationOfficeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 69, true);
			this.RegistrationOfficeTextBox.Name = "RegistrationOfficeTextBox";
			this.RegistrationOfficeTextBox.ReadOnly = true;
			this.RegistrationOfficeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.RegistrationOfficeTextBox.TabIndex = 5;
			// 
			// RegistrationDateEdit
			// 
			this.RegistrationDateEdit.AllowDrop = true;
			this.RegistrationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RegistrationDateEdit, "EntryNumbersProvider.RegistrationInfo.CE_IssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).EntryNumbersProvider.RegistrationInfo.CE_IssueDate)));
			this.RegistrationDateEdit.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("750b2528-e477-4ffe-be7d-378e1bb95d7f", "Registration Date");
			this.RegistrationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 94, true);
			this.RegistrationDateEdit.Name = "RegistrationDateEdit";
			this.RegistrationDateEdit.TabIndex = 6;
			// 
			// MrnTextBox
			// 
			this.BindingSource.SetBindingMember(this.MrnTextBox, "MovementReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementReferenceNumber)));
			this.MrnTextBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("f0c82e2d-fce2-4d39-a300-f72d030500a2", "MRN");
			this.MrnTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 119, true);
			this.MrnTextBox.Name = "MrnTextBox";
			this.MrnTextBox.ReadOnly = true;
			this.MrnTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.MrnTextBox.TabIndex = 7;
			// 
			// ReleaseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReleaseCodeTextBox, "EntryNumbersProvider.ReleaseInfo.CE_EntryNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).EntryNumbersProvider.ReleaseInfo.CE_EntryNum)));
			this.ReleaseCodeTextBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("d7a8b158-bda0-4bba-a4c6-974cf75fce7f", "Release Code");
			this.ReleaseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 144, true);
			this.ReleaseCodeTextBox.Name = "ReleaseCodeTextBox";
			this.ReleaseCodeTextBox.ReadOnly = true;
			this.ReleaseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.ReleaseCodeTextBox.TabIndex = 8;
			// 
			// ReleaseDateEdit
			// 
			this.ReleaseDateEdit.AllowDrop = true;
			this.ReleaseDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ReleaseDateEdit, "EntryNumbersProvider.ReleaseInfo.CE_IssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).EntryNumbersProvider.ReleaseInfo.CE_IssueDate)));
			this.ReleaseDateEdit.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("3cf369aa-5344-4e74-b244-b3abbbc1ad48", "Release Date");
			this.ReleaseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 169, true);
			this.ReleaseDateEdit.Name = "ReleaseDateEdit";
			this.ReleaseDateEdit.TabIndex = 9;
			// 
			// ArrivalGroupBox
			// 
			this.ArrivalGroupBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("c8befe23-6ead-450f-9a18-652c44610fd3", "Arrival");
			this.ArrivalGroupBox.Controls.Add(this.ArrivalStatusDescriptionTextBox);
			this.ArrivalGroupBox.Controls.Add(this.ArrivalStatusCodeTextBox);
			this.ArrivalGroupBox.Controls.Add(this.ArrivalOfficeDescriptionTextBox);
			this.ArrivalGroupBox.Controls.Add(this.ArrivalOfficeCodeTextBox);
			this.ArrivalGroupBox.Controls.Add(this.ArrivalDateEdit);
			this.ArrivalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 8, true);
			this.ArrivalGroupBox.Name = "ArrivalGroupBox";
			this.ArrivalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 126, true);
			this.ArrivalGroupBox.TabIndex = 1;
			this.ArrivalGroupBox.TabStop = false;
			// 
			// ArrivalStatusDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ArrivalStatusDescriptionTextBox, "EntryNumbersProvider.IrildesWrapper.StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).EntryNumbersProvider.IrildesWrapper.StatusDescription)));
			this.ArrivalStatusDescriptionTextBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("35a99756-9789-498d-96e3-cca174015c0c", "Arrival Status Description");
			this.ArrivalStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 80, true);
			this.ArrivalStatusDescriptionTextBox.Name = "ArrivalStatusDescriptionTextBox";
			this.ArrivalStatusDescriptionTextBox.ReadOnly = true;
			this.ArrivalStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.ArrivalStatusDescriptionTextBox.TabIndex = 4;
			// 
			// ArrivalStatusCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ArrivalStatusCodeTextBox, "EntryNumbersProvider.IrildesWrapper.Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).EntryNumbersProvider.IrildesWrapper.Status)));
			this.ArrivalStatusCodeTextBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("418684dd-c14f-41ec-bd76-1cffe51ba140", "Status");
			this.ArrivalStatusCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 80, true);
			this.ArrivalStatusCodeTextBox.Name = "ArrivalStatusCodeTextBox";
			this.ArrivalStatusCodeTextBox.ReadOnly = true;
			this.ArrivalStatusCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.ArrivalStatusCodeTextBox.TabIndex = 3;
			// 
			// ArrivalOfficeDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ArrivalOfficeDescriptionTextBox, "EntryNumbersProvider.IrildesWrapper.OfficeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).EntryNumbersProvider.IrildesWrapper.OfficeDescription)));
			this.ArrivalOfficeDescriptionTextBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("128f7f18-9818-4fad-b1d0-3f769b5349ec", "Arrival Office Description");
			this.ArrivalOfficeDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 54, true);
			this.ArrivalOfficeDescriptionTextBox.Name = "ArrivalOfficeDescriptionTextBox";
			this.ArrivalOfficeDescriptionTextBox.ReadOnly = true;
			this.ArrivalOfficeDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ArrivalOfficeDescriptionTextBox.TabIndex = 2;
			// 
			// ArrivalOfficeCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ArrivalOfficeCodeTextBox, "EntryNumbersProvider.IrildesWrapper.Office");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).EntryNumbersProvider.IrildesWrapper.Office)));
			this.ArrivalOfficeCodeTextBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("8e96f6f2-1cbe-4773-a8d8-20481aa6b173", "Office");
			this.ArrivalOfficeCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 54, true);
			this.ArrivalOfficeCodeTextBox.Name = "ArrivalOfficeCodeTextBox";
			this.ArrivalOfficeCodeTextBox.ReadOnly = true;
			this.ArrivalOfficeCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ArrivalOfficeCodeTextBox.TabIndex = 1;
			// 
			// ArrivalDateEdit
			// 
			this.ArrivalDateEdit.AllowDrop = true;
			this.ArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ArrivalDateEdit, "EntryNumbersProvider.IrildesWrapper.Date");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).EntryNumbersProvider.IrildesWrapper.Date)));
			this.ArrivalDateEdit.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("e4bd9daa-8324-4ada-8e91-8d184f986a3c", "Date");
			this.ArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 29, true);
			this.ArrivalDateEdit.Name = "ArrivalDateEdit";
			this.ArrivalDateEdit.TabIndex = 0;
			// 
			// A93NumbersGroupBox
			// 
			this.A93NumbersGroupBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("758EA684-2E98-41F4-89F9-FC6D9623F6DB", "A93");
			this.A93NumbersGroupBox.Controls.Add(this.A93Grid);
			this.A93NumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 342, true);
			this.A93NumbersGroupBox.Name = "A93NumbersGroupBox";
			this.A93NumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 135, true);
			this.A93NumbersGroupBox.TabIndex = 3;
			this.A93NumbersGroupBox.TabStop = false;
			// 
			// A93Grid
			// 
			this.A93Grid.AllowNavigation = false;
			this.A93Grid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.A93Grid, "MovementHeader.PayInfoCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.PayInfoCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsDeparturePayInfo)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.PayInfoCollection)).SyncRoot)).BPI_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsDeparturePayInfo)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.PayInfoCollection)).SyncRoot)).BPI_IncomingPayResponseNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsDeparturePayInfo)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.PayInfoCollection)).SyncRoot)).BPI_MethodOfPayment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IT.NCTS.Business.NctsDeparturePayInfo)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.PayInfoCollection)).SyncRoot)).BPI_PaymentAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.IT.NCTS.Business.NctsDeparturePayInfo)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.PayInfoCollection)).SyncRoot)).BPI_PaymentDate)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("ff980fda-7f2b-4e60-9e58-023dd710bd58", "Registry");
			zTextBoxColumnStyleInfo1.ColumnName = "BPI_TransactionType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("e023db60-fd26-4d2a-9e0d-032560681f5f", "Number");
			zTextBoxColumnStyleInfo2.ColumnName = "BPI_IncomingPayResponseNo";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("0e58553a-5dc9-4c4c-87c3-1d64f23816cc", "Payment Type");
			zTextBoxColumnStyleInfo3.ColumnName = "BPI_MethodOfPayment";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("5953f398-f911-43a1-9b89-dbf0bb570a50", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "BPI_PaymentAmount";
			zCalcEditColumnStyleInfo1.MaxValue = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("8cf70e7a-0b72-42af-98d3-9184be0b2579", "Expiry Date");
			zDateEditColumnStyleInfo1.ColumnName = "BPI_PaymentDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.A93Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.A93Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.A93Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.A93Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.A93Grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.A93Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.A93Grid.GridId = "05e79b54-9daa-4493-97ef-4902805ff46b";
			this.A93Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.A93Grid.LayoutKey = "A93Grid";
			this.A93Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.A93Grid.Name = "A93Grid";
			this.A93Grid.ReadOnly = true;
			this.A93Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(361, 116, true);
			this.A93Grid.TabIndex = 0;
			// 
			// DeclarationStatusTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ArrivalGroupBox);
			this.Controls.Add(this.StatusGroupBox);
			this.Controls.Add(this.CustomsGroupBox);
			this.Controls.Add(this.A93NumbersGroupBox);
			this.Name = "DeclarationStatusTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 527, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatusGroupBox.ResumeLayout(false);
			this.StatusGroupBox.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.DepartureStatusDropEdit.ResumeLayout(true);
			this.DepartureStatusDropEdit.PerformLayout();
			this.ControlChannelDropEdit.ResumeLayout(true);
			this.ControlChannelDropEdit.PerformLayout();
			this.AcceptanceDateEdit.ResumeLayout(true);
			this.AcceptanceDateEdit.PerformLayout();
			this.CustomsGroupBox.ResumeLayout(false);
			this.CustomsGroupBox.PerformLayout();
			this.RegistrationDateEdit.ResumeLayout(true);
			this.RegistrationDateEdit.PerformLayout();
			this.ReleaseDateEdit.ResumeLayout(true);
			this.ReleaseDateEdit.PerformLayout();
			this.ArrivalGroupBox.ResumeLayout(false);
			this.ArrivalGroupBox.PerformLayout();
			this.ArrivalDateEdit.ResumeLayout(true);
			this.ArrivalDateEdit.PerformLayout();
			this.A93NumbersGroupBox.ResumeLayout(false);
			this.A93NumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.A93Grid)).EndInit();
			this.A93Grid.ResumeLayout(false);
			this.A93Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZGroupBox StatusGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DepartureStatusDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ControlChannelDropEdit;
		private ZArchitecture.GUI.ZGroupBox ArrivalGroupBox;
		private ZArchitecture.GUI.ZDateEdit ArrivalDateEdit;
		private ZArchitecture.ZTextBox ArrivalOfficeCodeTextBox;
		private ZArchitecture.ZTextBox ArrivalOfficeDescriptionTextBox;
		private ZArchitecture.ZTextBox ArrivalStatusDescriptionTextBox;
		private ZArchitecture.ZTextBox ArrivalStatusCodeTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CustomsGroupBox;
		private ZArchitecture.GUI.ZDateEdit AcceptanceDateEdit;
		private ZArchitecture.ZTextBox RegistrationNumberTextBox;
		private ZArchitecture.ZTextBox RegistrationOfficeTextBox;
		private ZArchitecture.GUI.ZDateEdit RegistrationDateEdit;
		private ZArchitecture.ZTextBox MrnTextBox;
		private ZArchitecture.ZTextBox ReleaseCodeTextBox;
		private ZArchitecture.GUI.ZDateEdit ReleaseDateEdit;
		private ZArchitecture.GUI.ZGroupBox A93NumbersGroupBox;
		private ZArchitecture.ZGrid A93Grid;

		#endregion
	}
}

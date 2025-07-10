namespace Enterprise.Customs.AU.GUI
{
	partial class AUCOLSDetailsUserControl
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

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DeliveryUnpackControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ResponsiblePartyControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.LodgmentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeliveryClassificationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LodgementSummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LodgementStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LodgementResultMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LRNStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ApprovedArrangementsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RegistrationIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SupplementaryInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NotifyEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BICONURLTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImportPermitNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LateLodgementGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReasonUserControl = new Enterprise.Customs.AU.GUI.AUCOLSLateLodgementReasonUserControl();
			this.DirectionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DirectionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DirectionsResetButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeliveryUnpackControl.SuspendLayout();
			this.ResponsiblePartyControl.SuspendLayout();
			this.LodgmentDetailsGroupBox.SuspendLayout();
			this.DeliveryClassificationDropEdit.SuspendLayout();
			this.LodgementSummaryGroupBox.SuspendLayout();
			this.ApprovedArrangementsGroupBox.SuspendLayout();
			this.SupplementaryInformationGroupBox.SuspendLayout();
			this.LateLodgementGroupBox.SuspendLayout();
			this.ReasonUserControl.SuspendLayout();
			this.DirectionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DirectionsGrid)).BeginInit();
			this.DirectionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader);
			// 
			// DeliveryUnpackControl
			// 
			this.DeliveryUnpackControl.AddressValidationProcessCmdKey = null;
			this.DeliveryUnpackControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryUnpackControl, "DeliveryOrUnpack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).DeliveryOrUnpack)));
			this.DeliveryUnpackControl.BindToOrganisations = "Lookups.AllOrganisations";
			this.DeliveryUnpackControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|A80F05F0-B8CE-4DB5-AFED-2E08569733CC", "Delivery / Unpack");
			this.DeliveryUnpackControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 207, true);
			this.DeliveryUnpackControl.Name = "DeliveryUnpackControl";
			this.DeliveryUnpackControl.ReadOnly = false;
			this.DeliveryUnpackControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DeliveryUnpackControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.DeliveryUnpackControl.TabIndex = 0;
			this.DeliveryUnpackControl.ValidationJustForced = false;
			// 
			// ResponsiblePartyControl
			// 
			this.ResponsiblePartyControl.AddressValidationProcessCmdKey = null;
			this.ResponsiblePartyControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ResponsiblePartyControl, "ResponsibleParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).ResponsibleParty)));
			this.ResponsiblePartyControl.BindToOrganisations = "Lookups.AllOrganisations";
			this.ResponsiblePartyControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|7A8F016E-038F-4AD8-BD43-D0FEB9492C3B", "Responsible Party");
			this.ResponsiblePartyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.ResponsiblePartyControl.Name = "ResponsiblePartyControl";
			this.ResponsiblePartyControl.ReadOnly = false;
			this.ResponsiblePartyControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ResponsiblePartyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ResponsiblePartyControl.TabIndex = 0;
			this.ResponsiblePartyControl.ValidationJustForced = false;
			// 
			// LodgmentDetailsGroupBox
			// 
			this.LodgmentDetailsGroupBox.Controls.Add(this.DeliveryClassificationDropEdit);
			this.LodgmentDetailsGroupBox.Controls.Add(this.LRNTextBox);
			this.LodgmentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 8, true);
			this.LodgmentDetailsGroupBox.Name = "LodgmentDetailsGroupBox";
			this.LodgmentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 96, true);
			this.LodgmentDetailsGroupBox.TabIndex = 1;
			this.LodgmentDetailsGroupBox.TabStop = false;
			this.LodgmentDetailsGroupBox.Text = "Lodgement Details";
			// 
			// DeliveryClassificationDropEdit
			// 
			this.DeliveryClassificationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryClassificationDropEdit, "QCH_DeliveryClassification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).QCH_DeliveryClassification)));
			this.DeliveryClassificationDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|1AE548B2-E6D8-4127-BDC5-DB69B43D9C73", "Delivery Classification");
			this.DeliveryClassificationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 65, true);
			this.DeliveryClassificationDropEdit.Name = "DeliveryClassificationDropEdit";
			this.DeliveryClassificationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 20, true);
			this.DeliveryClassificationDropEdit.TabIndex = 7;
			// 
			// LRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.LRNTextBox, "LRN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).LRN)));
			this.LRNTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|23ED4C2E-2EC0-4DDC-8528-282A02CE7A39", "LRN");
			this.LRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 33, true);
			this.LRNTextBox.Name = "LRNTextBox";
			this.LRNTextBox.ReadOnly = true;
			this.LRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 20, true);
			this.LRNTextBox.TabIndex = 6;
			// 
			// LodgementSummaryGroupBox
			// 
			this.LodgementSummaryGroupBox.Controls.Add(this.LodgementStatusTextBox);
			this.LodgementSummaryGroupBox.Controls.Add(this.LodgementResultMessageTextBox);
			this.LodgementSummaryGroupBox.Controls.Add(this.LRNStatusTextBox);
			this.LodgementSummaryGroupBox.Controls.Add(this.MessageStatusTextBox);
			this.LodgementSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(771, 8, true);
			this.LodgementSummaryGroupBox.Name = "LodgementSummaryGroupBox";
			this.LodgementSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 162, true);
			this.LodgementSummaryGroupBox.TabIndex = 2;
			this.LodgementSummaryGroupBox.TabStop = false;
			this.LodgementSummaryGroupBox.Text = "Lodgement Summary";
			// 
			// LodgementStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.LodgementStatusTextBox, "LodgementStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).LodgementStatus)));
			this.LodgementStatusTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|A5F2418F-27BD-42CC-A9CA-AC9CE875A65C", "Lodgement Status");
			this.LodgementStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LodgementStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 14, true);
			this.LodgementStatusTextBox.Name = "LodgementStatusTextBox";
			this.LodgementStatusTextBox.ReadOnly = true;
			this.LodgementStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.LodgementStatusTextBox.TabIndex = 10;
			// 
			// LodgementResultMessageTextBox
			// 
			this.BindingSource.SetBindingMember(this.LodgementResultMessageTextBox, "LodgementResultMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).LodgementResultMessage)));
			this.LodgementResultMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LodgementResultMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 32, true);
			this.LodgementResultMessageTextBox.Multiline = true;
			this.LodgementResultMessageTextBox.Name = "LodgementResultMessageTextBox";
			this.LodgementResultMessageTextBox.ReadOnly = true;
			this.LodgementResultMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 44, true);
			this.LodgementResultMessageTextBox.TabIndex = 11;
			// 
			// LRNStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.LRNStatusTextBox, "LRNStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).LRNStatus)));
			this.LRNStatusTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|6E6B7F79-9D54-4B46-BD1C-93AB9F8FE643", "LRN Status");
			this.LRNStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 89, true);
			this.LRNStatusTextBox.Name = "LRNStatusTextBox";
			this.LRNStatusTextBox.ReadOnly = true;
			this.LRNStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.LRNStatusTextBox.TabIndex = 8;
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "HeaderStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).HeaderStatus)));
			this.MessageStatusTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|1AB53DC3-4395-47D3-89D7-31857D87D094", "Message Status");
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 119, true);
			this.MessageStatusTextBox.Multiline = true;
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.ReadOnly = true;
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 31, true);
			this.MessageStatusTextBox.TabIndex = 9;
			// 
			// ApprovedArrangementsGroupBox
			// 
			this.ApprovedArrangementsGroupBox.Controls.Add(this.RegistrationIDTextBox);
			this.ApprovedArrangementsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 120, true);
			this.ApprovedArrangementsGroupBox.Name = "ApprovedArrangementsGroupBox";
			this.ApprovedArrangementsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 50, true);
			this.ApprovedArrangementsGroupBox.TabIndex = 3;
			this.ApprovedArrangementsGroupBox.TabStop = false;
			this.ApprovedArrangementsGroupBox.Text = "Approved Arrangements (AA)";
			// 
			// RegistrationIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegistrationIDTextBox, "QCH_ApprovedArrangementRefNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).QCH_ApprovedArrangementRefNum)));
			this.RegistrationIDTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|53AA3AF2-F723-4750-845B-70A8F0E9CD14", "Registration ID");
			this.RegistrationIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 23, true);
			this.RegistrationIDTextBox.Name = "RegistrationIDTextBox";
			this.RegistrationIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 20, true);
			this.RegistrationIDTextBox.TabIndex = 7;
			// 
			// SupplementaryInformationGroupBox
			// 
			this.SupplementaryInformationGroupBox.Controls.Add(this.NotifyEmailTextBox);
			this.SupplementaryInformationGroupBox.Controls.Add(this.BICONURLTextBox);
			this.SupplementaryInformationGroupBox.Controls.Add(this.ImportPermitNumberTextBox);
			this.SupplementaryInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 188, true);
			this.SupplementaryInformationGroupBox.Name = "SupplementaryInformationGroupBox";
			this.SupplementaryInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 99, true);
			this.SupplementaryInformationGroupBox.TabIndex = 4;
			this.SupplementaryInformationGroupBox.TabStop = false;
			this.SupplementaryInformationGroupBox.Text = "Supplementary Information";
			// 
			// NotifyEmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyEmailTextBox, "QCH_AlsoNotifyEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).QCH_AlsoNotifyEmail)));
			this.NotifyEmailTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|5547D2B1-4688-4D2B-926B-EE4AEA6358D7", "Notify Email");
			this.NotifyEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 73, true);
			this.NotifyEmailTextBox.Name = "NotifyEmailTextBox";
			this.NotifyEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 20, true);
			this.NotifyEmailTextBox.TabIndex = 11;
			// 
			// BICONURLTextBox
			// 
			this.BindingSource.SetBindingMember(this.BICONURLTextBox, "QCH_BiosecurityImportConditionURL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).QCH_BiosecurityImportConditionURL)));
			this.BICONURLTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|FEA3B147-E89B-47A0-A9C1-2ED04C6DBED4", "BICON URL");
			this.BICONURLTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 47, true);
			this.BICONURLTextBox.Name = "BICONURLTextBox";
			this.BICONURLTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 20, true);
			this.BICONURLTextBox.TabIndex = 10;
			// 
			// ImportPermitNumberTextBox
			// 
			this.ImportPermitNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportPermitNumberTextBox, "QCH_ImportPermitNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).QCH_ImportPermitNumber)));
			this.ImportPermitNumberTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|D235BDE0-93A4-401B-820B-3386BF0E7524", "Import Permit Number");
			this.ImportPermitNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 19, true);
			this.ImportPermitNumberTextBox.Name = "ImportPermitNumberTextBox";
			this.ImportPermitNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 20, true);
			this.ImportPermitNumberTextBox.TabIndex = 9;
			// 
			// LateLodgementGroupBox
			// 
			this.LateLodgementGroupBox.Controls.Add(this.DetailsTextBox);
			this.LateLodgementGroupBox.Controls.Add(this.ReasonUserControl);
			this.LateLodgementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 300, true);
			this.LateLodgementGroupBox.Name = "LateLodgementGroupBox";
			this.LateLodgementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 89, true);
			this.LateLodgementGroupBox.TabIndex = 5;
			this.LateLodgementGroupBox.TabStop = false;
			this.LateLodgementGroupBox.Text = "Late Lodgement";
			// 
			// DetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.DetailsTextBox, "QCH_LateLodgementDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).QCH_LateLodgementDetails)));
			this.DetailsTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|5402CBC8-40FD-4E25-BF85-D9AA5E157275", "Details");
			this.DetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 42, true);
			this.DetailsTextBox.Multiline = true;
			this.DetailsTextBox.Name = "DetailsTextBox";
			this.DetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 31, true);
			this.DetailsTextBox.TabIndex = 13;
			// 
			// ReasonUserControl
			// 
			this.ReasonUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReasonUserControl, "QCH_LateLodgementReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).QCH_LateLodgementReason)));
			this.ReasonUserControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|42F4BAB5-1383-4EA5-A778-E89FD6C275AE", "Reason");
			this.ReasonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 18, true);
			this.ReasonUserControl.Name = "ReasonUserControl";
			this.ReasonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 20, true);
			this.ReasonUserControl.TabIndex = 12;
			// 
			// DirectionsGroupBox
			// 
			this.DirectionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left)));
			this.DirectionsGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUCOLSDetailsUserControl|29EEEE31-69F8-408E-8705-35FB22D2A79C", "Directions");
			this.DirectionsGroupBox.Controls.Add(this.DirectionsGrid);
			this.DirectionsGroupBox.Controls.Add(this.DirectionsResetButton);
			this.DirectionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 393, true);
			this.DirectionsGroupBox.Name = "DirectionsGroupBox";
			this.DirectionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1133, 180, true);
			this.DirectionsGroupBox.TabIndex = 6;
			this.DirectionsGroupBox.TabStop = false;
			// 
			// DirectionsGrid
			// 
			this.DirectionsGrid.AllowNavigation = false;
			this.DirectionsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DirectionsGrid, "Directions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).Directions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsDirection)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).Directions)).SyncRoot)).QCD_CO_Container)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsDirection)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).Directions)).SyncRoot)).QCD_CL_CusEntryLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsDirection)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).Directions)).SyncRoot)).QCD_Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsDirection)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).Directions)).SyncRoot)).QCD_TreatmentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsDirection)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).Directions)).SyncRoot)).AAId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsDirection)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).Directions)).SyncRoot)).AAName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsDirection)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.QuarantineColsHeader)(null)).Directions)).SyncRoot)).AALocation)));
			this.DirectionsGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "QCD_CO_Container";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo2.ColumnName = "QCD_CL_CusEntryLine";
			zGuidDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.ColumnName = "QCD_Direction";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "QCD_TreatmentType";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.ColumnName = "AAId";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "AAName";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.ColumnName = "AALocation";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.DirectionsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.DirectionsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.DirectionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DirectionsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.DirectionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DirectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DirectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DirectionsGrid.GridId = "c7c5d01c-85e2-4322-955c-d7c825136631";
			this.DirectionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DirectionsGrid.LayoutKey = "DirectionsGrid";
			this.DirectionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DirectionsGrid.Name = "DirectionsGrid";
			this.DirectionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 122, true);
			this.DirectionsGrid.TabIndex = 0;
			// 
			// DirectionsResetButton
			// 
			this.DirectionsResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DirectionsResetButton.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("0f008b80-5a33-4f73-bc34-bdb3e7727eef", "Reset");
			this.DirectionsResetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1038, 147, true);
			this.DirectionsResetButton.Name = "DirectionsResetButton";
			this.DirectionsResetButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 23, true);
			this.DirectionsResetButton.TabIndex = 1;
			this.DirectionsResetButton.ToolTipCaption = null;
			this.DirectionsResetButton.Click += new System.EventHandler(this.DirectionsResetButton_Click);
			// 
			// AUCOLSDetailsUserControl
			// 
			this.Controls.Add(this.DirectionsGroupBox);
			this.Controls.Add(this.LateLodgementGroupBox);
			this.Controls.Add(this.SupplementaryInformationGroupBox);
			this.Controls.Add(this.ApprovedArrangementsGroupBox);
			this.Controls.Add(this.LodgementSummaryGroupBox);
			this.Controls.Add(this.LodgmentDetailsGroupBox);
			this.Controls.Add(this.DeliveryUnpackControl);
			this.Controls.Add(this.ResponsiblePartyControl);
			this.Name = "AUCOLSDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 580, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeliveryUnpackControl.ResumeLayout(true);
			this.DeliveryUnpackControl.PerformLayout();
			this.ResponsiblePartyControl.ResumeLayout(true);
			this.ResponsiblePartyControl.PerformLayout();
			this.LodgmentDetailsGroupBox.ResumeLayout(false);
			this.LodgmentDetailsGroupBox.PerformLayout();
			this.DeliveryClassificationDropEdit.ResumeLayout(true);
			this.DeliveryClassificationDropEdit.PerformLayout();
			this.LodgementSummaryGroupBox.ResumeLayout(false);
			this.LodgementSummaryGroupBox.PerformLayout();
			this.ApprovedArrangementsGroupBox.ResumeLayout(false);
			this.ApprovedArrangementsGroupBox.PerformLayout();
			this.SupplementaryInformationGroupBox.ResumeLayout(false);
			this.SupplementaryInformationGroupBox.PerformLayout();
			this.LateLodgementGroupBox.ResumeLayout(false);
			this.LateLodgementGroupBox.PerformLayout();
			this.ReasonUserControl.ResumeLayout(true);
			this.ReasonUserControl.PerformLayout();
			this.DirectionsGroupBox.ResumeLayout(false);
			this.DirectionsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DirectionsGrid)).EndInit();
			this.DirectionsGrid.ResumeLayout(false);
			this.DirectionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		MasterFiles.GUI.ZDocAddressControl DeliveryUnpackControl;
		MasterFiles.GUI.ZDocAddressControl ResponsiblePartyControl;
		ZArchitecture.GUI.ZGroupBox LodgmentDetailsGroupBox;
		ZArchitecture.GUI.ZGroupBox LodgementSummaryGroupBox;
		ZArchitecture.GUI.ZGroupBox ApprovedArrangementsGroupBox;
		ZArchitecture.GUI.ZGroupBox SupplementaryInformationGroupBox;
		ZArchitecture.GUI.ZGroupBox LateLodgementGroupBox;
		ZArchitecture.ZTextBox LRNTextBox;
		ZArchitecture.GUI.ZDropEdit DeliveryClassificationDropEdit;
		ZArchitecture.ZTextBox LRNStatusTextBox;
		ZArchitecture.ZTextBox MessageStatusTextBox;
		ZArchitecture.ZTextBox LodgementStatusTextBox;
		ZArchitecture.ZTextBox RegistrationIDTextBox;
		ZArchitecture.ZTextBox AddressTextBox;
		ZArchitecture.ZTextBox ImportPermitNumberTextBox;
		ZArchitecture.ZTextBox BICONURLTextBox;
		ZArchitecture.ZTextBox NotifyEmailTextBox;
		internal Enterprise.Customs.AU.GUI.AUCOLSLateLodgementReasonUserControl ReasonUserControl;
		ZArchitecture.ZTextBox DetailsTextBox;
		internal ZArchitecture.GUI.ZGroupBox DirectionsGroupBox;
		ZArchitecture.ZGrid DirectionsGrid;
		internal ZArchitecture.GUI.ZButton DirectionsResetButton;
		ZArchitecture.ZTextBox LodgementResultMessageTextBox;
	}
}

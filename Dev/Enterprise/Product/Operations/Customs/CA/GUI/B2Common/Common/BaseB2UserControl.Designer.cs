namespace Enterprise.Customs.CA.GUI
{
	partial class BaseB2UserControl
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
				if (JobDeclaration != null)
				{
					JobDeclaration.JE_OH_ImporterInfo.ValueChanged -= JE_OH_ImporterInfo_ValueChanged;
				}

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ImporterOrganisationControlWithMiscellaneous = new Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous();
			this.MailToOrganisationControlWithMiscellaneous = new Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous();
			this.B2TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AccountSecurityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SequentialNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CheckDigitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AmountDueImporterCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AmountDueCBSACalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AnySightDepositAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ClaimedInterestAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DocsAttachedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SecurityNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorisationDateDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OriginalAccountingDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ClearancePortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CarrierCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OriginalTransactionNumberCodeFindBox = new Enterprise.Customs.CA.GUI.OriginalB3TransactionNumberCodeFindBox();
			this.LongTextDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExplanationLongTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.UndertTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RequestJustificationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AcceptedZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ConfirmedZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SubmittedZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SubmittedOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AcceptedOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsOurFaultCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.InitiatedByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ChequeNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ChequeDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AmendmentToDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FormattedTransactionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImporterOrganisationControlWithMiscellaneous.SuspendLayout();
			this.MailToOrganisationControlWithMiscellaneous.SuspendLayout();
			this.B2TypeDropEdit.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.CarrierCodeFindBox.SuspendLayout();
			this.AuthorisationDateDateEdit1.SuspendLayout();
			this.OriginalAccountingDateDateEdit.SuspendLayout();
			this.ClearancePortCodeFindBox.SuspendLayout();
			this.OriginalTransactionNumberCodeFindBox.SuspendLayout();
			this.LongTextDetailsGroupBox.SuspendLayout();
			this.ExplanationLongTextBox.SuspendLayout();
			this.DatesGroupBox.SuspendLayout();
			this.AcceptedZDateEdit.SuspendLayout();
			this.ConfirmedZDateEdit.SuspendLayout();
			this.SubmittedZDateEdit.SuspendLayout();
			this.InitiatedByDropEdit.SuspendLayout();
			this.ChequeDateDateEdit.SuspendLayout();
			this.AmendmentToDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Controls.Add(this.AccountSecurityCodeTextBox);
			this.DeclarationDetailsGroupBox.Controls.Add(this.SequentialNumberTextBox);
			this.DeclarationDetailsGroupBox.Controls.Add(this.CheckDigitTextBox);
			this.DeclarationDetailsGroupBox.Controls.Add(this.FormattedTransactionNumberTextBox);
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 48, true);
			this.DeclarationDetailsGroupBox.TabIndex = 3;
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.FormattedTransactionNumberTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.CheckDigitTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.StatusTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.SequentialNumberTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.ExportDeclarationNumberBoundTextBox, 0);
			this.DeclarationDetailsGroupBox.Controls.SetChildIndex(this.AccountSecurityCodeTextBox, 0);
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 19, true);
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 20, true);
			this.StatusTextBox.TabIndex = 7;
			// 
			// ExportDeclarationNumberBoundTextBox
			// 
			this.ExportDeclarationNumberBoundTextBox.Enabled = false;
			this.ExportDeclarationNumberBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(709, 10, true);
			this.ExportDeclarationNumberBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 20, true);
			this.ExportDeclarationNumberBoundTextBox.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			// 
			// ImporterOrganisationControlWithMiscellaneous
			// 
			this.ImporterOrganisationControlWithMiscellaneous.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterOrganisationControlWithMiscellaneous, "JE_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_OH_Importer)));
			this.ImporterOrganisationControlWithMiscellaneous.BindToMiscellaneousFields = "JE_ImporterMiscFields";
			this.ImporterOrganisationControlWithMiscellaneous.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fed8d2f9-2a8d-4c05-ac35-e35b89764288", "Importer");
			this.ImporterOrganisationControlWithMiscellaneous.Captions = new string[] { "Importer" };
			this.ImporterOrganisationControlWithMiscellaneous.IsCaptionOverridden = false;
			this.ImporterOrganisationControlWithMiscellaneous.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 8, true);
			this.ImporterOrganisationControlWithMiscellaneous.Name = "ImporterOrganisationControlWithMiscellaneous";
			this.ImporterOrganisationControlWithMiscellaneous.PopupCaption = "";
			this.ImporterOrganisationControlWithMiscellaneous.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.ImporterOrganisationControlWithMiscellaneous.TabIndex = 1;
			// 
			// MailToOrganisationControlWithMiscellaneous
			// 
			this.MailToOrganisationControlWithMiscellaneous.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MailToOrganisationControlWithMiscellaneous, "JE_OH_NotifyParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_OH_NotifyParty)));
			this.MailToOrganisationControlWithMiscellaneous.BindToMiscellaneousFields = "CA_MailToMiscFields";
			this.MailToOrganisationControlWithMiscellaneous.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("119425c5-889e-4ff2-abf3-ce54c4e0cf9b", "Mail To");
			this.MailToOrganisationControlWithMiscellaneous.Captions = new string[] { "Mail To" };
			this.MailToOrganisationControlWithMiscellaneous.IsCaptionOverridden = false;
			this.MailToOrganisationControlWithMiscellaneous.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 195, true);
			this.MailToOrganisationControlWithMiscellaneous.Name = "MailToOrganisationControlWithMiscellaneous";
			this.MailToOrganisationControlWithMiscellaneous.PopupCaption = "";
			this.MailToOrganisationControlWithMiscellaneous.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.MailToOrganisationControlWithMiscellaneous.TabIndex = 2;
			// 
			// B2TypeDropEdit
			// 
			this.B2TypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.B2TypeDropEdit, "CA_B2Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_B2Type)));
			this.B2TypeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("eb76f124-7eeb-44f7-9b1d-e6b8525c0df5", "B2 Type");
			this.B2TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 11, true);
			this.B2TypeDropEdit.Name = "B2TypeDropEdit";
			this.B2TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.B2TypeDropEdit.TabIndex = 9;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "JE_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_TransportMode)));
			this.TransportModeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8B70A98A-36A5-41AB-A8DD-50DF975802D8", "Transport");
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 85, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.ShouldResizeByMaxLength = true;
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.TransportModeDropEdit.TabIndex = 12;
			// 
			// AccountSecurityCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccountSecurityCodeTextBox, "TransactionNumber.AccountSecurityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.AccountSecurityCode)));
			this.AccountSecurityCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("595e0e06-2a53-4fb1-9a78-067334e28404", "Transaction Number");
			this.AccountSecurityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 19, true);
			this.AccountSecurityCodeTextBox.Name = "AccountSecurityCodeTextBox";
			this.AccountSecurityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.AccountSecurityCodeTextBox.TabIndex = 4;
			// 
			// SequentialNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.SequentialNumberTextBox, "TransactionNumber.SequentialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.SequentialNumber)));
			this.SequentialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 19, true);
			this.SequentialNumberTextBox.Name = "SequentialNumberTextBox";
			this.SequentialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.SequentialNumberTextBox.TabIndex = 5;
			// 
			// CheckDigitTextBox
			// 
			this.BindingSource.SetBindingMember(this.CheckDigitTextBox, "TransactionNumber.CheckDigit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.CheckDigit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CheckDigitTextBox, false);
			this.CheckDigitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(221, 19, true);
			this.CheckDigitTextBox.Name = "CheckDigitTextBox";
			this.CheckDigitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 20, true);
			this.CheckDigitTextBox.TabIndex = 6;
			// 
			// FormattedTransactionNumberTextBox
			// 
			this.FormattedTransactionNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FormattedTransactionNumberTextBox, "TransactionNumber.FormattedTransactionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.FormattedTransactionNumber)));
			this.FormattedTransactionNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|d50d8243-17b7-4156-bdde-447c14229331", "Transaction Number");
			this.FormattedTransactionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 19, true);
			this.FormattedTransactionNumberTextBox.Name = "FormattedTransactionNumberTextBox";
			this.FormattedTransactionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.FormattedTransactionNumberTextBox.TabIndex = 4;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9e9de0fe-bf03-4be0-bf3f-f7fdd08b1184", "Details");
			this.DetailsGroupBox.Controls.Add(this.AmendmentToDropEdit);
			this.DetailsGroupBox.Controls.Add(this.AnySightDepositAmountCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.ClaimedInterestAmountCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.B2TypeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.DocsAttachedCheckBox);
			this.DetailsGroupBox.Controls.Add(this.IsOurFaultCheckBox);
			this.DetailsGroupBox.Controls.Add(this.SecurityNoTextBox);
			this.DetailsGroupBox.Controls.Add(this.AuthorisationDateDateEdit1);
			this.DetailsGroupBox.Controls.Add(this.OriginalAccountingDateDateEdit);
			this.DetailsGroupBox.Controls.Add(this.ClearancePortCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.TransportModeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.CarrierCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.OriginalTransactionNumberCodeFindBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 62, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 292, true);
			this.DetailsGroupBox.TabIndex = 8;
			this.DetailsGroupBox.TabStop = false;
			// 
			// AmountDueCBSACalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountDueCBSACalcEdit, "AmountDueCBSA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).AmountDueCBSA)));
			this.AmountDueCBSACalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("892EDCB0-CA97-4880-976F-B71ACF0F3AE7", "Amount Due CBSA");
			this.AmountDueCBSACalcEdit.DecimalPlaces = 2;
			this.AmountDueCBSACalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(803, 323, true);
			this.AmountDueCBSACalcEdit.Name = "AmountDueCBSACalcEdit";
			this.AmountDueCBSACalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.AmountDueCBSACalcEdit.TabIndex = 28;
			this.AmountDueCBSACalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AmountDueImporterCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountDueImporterCalcEdit, "AmountDueImporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).AmountDueImporter)));
			this.AmountDueImporterCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("97662EE8-90BC-4DF4-9458-E531954436B9", "Amount Due Importer");
			this.AmountDueImporterCalcEdit.DecimalPlaces = 2;
			this.AmountDueImporterCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(803, 297, true);
			this.AmountDueImporterCalcEdit.Name = "AmountDueImporterCalcEdit";
			this.AmountDueImporterCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.AmountDueImporterCalcEdit.TabIndex = 27;
			this.AmountDueImporterCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AnySightDepositAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AnySightDepositAmountCalcEdit, "CA_AnySightDepositAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_AnySightDepositAmount)));
			this.AnySightDepositAmountCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7d168ef9-e43c-47f2-9375-e620c63386e7", "Any Sight Deposit Amount");
			this.AnySightDepositAmountCalcEdit.DecimalPlaces = 2;
			this.AnySightDepositAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 197, true);
			this.AnySightDepositAmountCalcEdit.Name = "AnySightDepositAmountCalcEdit";
			this.AnySightDepositAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.AnySightDepositAmountCalcEdit.TabIndex = 20;
			this.AnySightDepositAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ClaimedInterestAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ClaimedInterestAmountCalcEdit, "CA_ClaimedInterestAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_ClaimedInterestAmount)));
			this.ClaimedInterestAmountCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("72ef0bdf-7865-4f4e-8206-c85c04def4c6", "Claimed Interest Amount");
			this.ClaimedInterestAmountCalcEdit.DecimalPlaces = 2;
			this.ClaimedInterestAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 167, true);
			this.ClaimedInterestAmountCalcEdit.Name = "ClaimedInterestAmountCalcEdit";
			this.ClaimedInterestAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.ClaimedInterestAmountCalcEdit.TabIndex = 19;
			this.ClaimedInterestAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DocsAttachedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DocsAttachedCheckBox, "CA_IsDocAttached");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_IsDocAttached)));
			this.DocsAttachedCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("67ebd504-1dec-4aea-8387-ca80ccacd74c", "Docs Attached");
			this.DocsAttachedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DocsAttachedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DocsAttachedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 141, true);
			this.DocsAttachedCheckBox.Name = "DocsAttachedCheckBox";
			this.DocsAttachedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.DocsAttachedCheckBox.TabIndex = 17;
			this.DocsAttachedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DocsAttachedCheckBox.UseVisualStyleBackColor = true;
			// 
			// SecurityNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.SecurityNoTextBox, "CA_SecurityNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_SecurityNo)));
			this.SecurityNoTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d248fdbf-e837-4b02-9348-bf44a9c2b361", "Security No.");
			this.SecurityNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 161, true);
			this.SecurityNoTextBox.Name = "SecurityNoTextBox";
			this.SecurityNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.SecurityNoTextBox.TabIndex = 16;
			// 
			// AuthorisationDateDateEdit1
			// 
			this.AuthorisationDateDateEdit1.AllowDrop = true;
			this.AuthorisationDateDateEdit1.AutoCompleteMonthThreshold = 1;
			this.AuthorisationDateDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AuthorisationDateDateEdit1, "JE_EntryAuthorisationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_EntryAuthorisationDate)));
			this.AuthorisationDateDateEdit1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0f2997d1-0bdb-46a4-94ce-630ee6562114", "Release Date");
			this.AuthorisationDateDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 135, true);
			this.AuthorisationDateDateEdit1.Name = "AuthorisationDateDateEdit1";
			this.AuthorisationDateDateEdit1.TabIndex = 15;
			// 
			// OriginalAccountingDateDateEdit
			// 
			this.OriginalAccountingDateDateEdit.AllowDrop = true;
			this.OriginalAccountingDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.OriginalAccountingDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.OriginalAccountingDateDateEdit, "CA_OriginalAccountingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_OriginalAccountingDate)));
			this.OriginalAccountingDateDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c7c82f05-8668-45dd-8a68-784736beb86a", "Original Accounting Date");
			this.OriginalAccountingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 135, true);
			this.OriginalAccountingDateDateEdit.Name = "OriginalAccountingDateDateEdit";
			this.OriginalAccountingDateDateEdit.TabIndex = 14;
			// 
			// ClearancePortCodeFindBox
			// 
			this.ClearancePortCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClearancePortCodeFindBox, "JE_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_CustomsOffice)));
			this.ClearancePortCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("098b965e-5560-4904-b68a-59c60dc401a8", "Cust. Port Of Clearance", "Customs Port Of Clearance", "");
			this.ClearancePortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 61, true);
			this.ClearancePortCodeFindBox.Name = "ClearancePortCodeFindBox";
			this.ClearancePortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.ClearancePortCodeFindBox.TabIndex = 11;
			// 
			// CarrierCodeFindBox
			// 
			this.CarrierCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierCodeFindBox, "JE_CarrierCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_CarrierCode)));
			this.CarrierCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAD147B9-2940-4F28-A414-FC649DEFAA5F", "Carrier at Import.", "Carrier at Import.", "");
			this.CarrierCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 109, true);
			this.CarrierCodeFindBox.Name = "CarrierCodeFindBox";
			this.CarrierCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CarrierCodeFindBox.ParentType = null;
			this.CarrierCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.CarrierCodeFindBox.TabIndex = 13;
			// 
			// OriginalTransactionNumberCodeFindBox
			// 
			this.OriginalTransactionNumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginalTransactionNumberCodeFindBox, "CA_OriginalTransactionNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_OriginalTransactionNo)));
			this.OriginalTransactionNumberCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("82253d9f-4d5f-4d97-a3f0-0bf98769c48d", "Original Trans No.", "Original Transaction Number", "");
			this.OriginalTransactionNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 37, true);
			this.OriginalTransactionNumberCodeFindBox.Name = "OriginalTransactionNumberCodeFindBox";
			this.OriginalTransactionNumberCodeFindBox.ShowDescriptionBox = false;
			this.OriginalTransactionNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.OriginalTransactionNumberCodeFindBox.TabIndex = 10;
			// 
			// LongTextDetailsGroupBox
			// 
			this.LongTextDetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ce36f616-1fa6-422e-b1af-5b52362fe015", "Additional Details");
			this.LongTextDetailsGroupBox.Controls.Add(this.ExplanationLongTextBox);
			this.LongTextDetailsGroupBox.Controls.Add(this.UndertTextBox);
			this.LongTextDetailsGroupBox.Controls.Add(this.RequestJustificationTextBox);
			this.LongTextDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 362, true);
			this.LongTextDetailsGroupBox.Name = "LongTextDetailsGroupBox";
			this.LongTextDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 97, true);
			this.LongTextDetailsGroupBox.TabIndex = 29;
			this.LongTextDetailsGroupBox.TabStop = false;
			// 
			// ExplanationLongTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExplanationLongTextBox, "CA_B2Explanation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_B2Explanation)));
			this.ExplanationLongTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("aebee559-ce96-407a-b2be-9429b1972ba1", "Explanation");
			this.ExplanationLongTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 71, true);
			this.ExplanationLongTextBox.Name = "ExplanationLongTextBox";
			this.ExplanationLongTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 20, true);
			this.ExplanationLongTextBox.TabIndex = 32;
			// 
			// UndertTextBox
			// 
			this.BindingSource.SetBindingMember(this.UndertTextBox, "CA_Under");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_Under)));
			this.UndertTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ad9e7bfb-6f17-4c1c-ae78-2fdf94efc119", "Under");
			this.UndertTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UndertTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 45, true);
			this.UndertTextBox.Name = "UndertTextBox";
			this.UndertTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 20, true);
			this.UndertTextBox.TabIndex = 31;
			// 
			// RequestJustificationTextBox
			// 
			this.BindingSource.SetBindingMember(this.RequestJustificationTextBox, "CA_JustificationForRequest");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_JustificationForRequest)));
			this.RequestJustificationTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b3a33993-1e4e-4540-b4a1-ef3c67618ba1", "Req. Justification", "Justification For Request", "");
			this.RequestJustificationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RequestJustificationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 19, true);
			this.RequestJustificationTextBox.Name = "RequestJustificationTextBox";
			this.RequestJustificationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 20, true);
			this.RequestJustificationTextBox.TabIndex = 30;
			// 
			// DatesGroupBox
			// 
			this.DatesGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("C74ECAD3-14DE-4FBA-B4FD-05D03B555F0E", "Dates.");
			this.DatesGroupBox.Controls.Add(this.AcceptedZDateEdit);
			this.DatesGroupBox.Controls.Add(this.ConfirmedZDateEdit);
			this.DatesGroupBox.Controls.Add(this.SubmittedZDateEdit);
			this.DatesGroupBox.Controls.Add(this.SubmittedOverrideCheckBox);
			this.DatesGroupBox.Controls.Add(this.AcceptedOverrideCheckBox);
			this.DatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(685, 134, true);
			this.DatesGroupBox.Name = "DatesGroupBox";
			this.DatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 86, true);
			this.DatesGroupBox.TabIndex = 18;
			this.DatesGroupBox.TabStop = false;
			// 
			// AcceptedOverrideCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AcceptedOverrideCheckBox, "CA_B2AcceptedDateOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_B2AcceptedDateOverride)));
			this.AcceptedOverrideCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BC8F957A-9949-4948-B3FC-9D07FA96C741", "Override");
			this.AcceptedOverrideCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Left;
			this.AcceptedOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AcceptedOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 59, true);
			this.AcceptedOverrideCheckBox.Name = "AcceptedOverrideCheckBox";
			this.AcceptedOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.AcceptedOverrideCheckBox.TabIndex = 26;
			this.AcceptedOverrideCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AcceptedOverrideCheckBox.UseVisualStyleBackColor = true;
			// 
			// AcceptedZDateEdit
			// 
			this.AcceptedZDateEdit.AllowDrop = true;
			this.AcceptedZDateEdit.AutoCompleteMonthThreshold = 1;
			this.AcceptedZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AcceptedZDateEdit, "CA_B2AcceptedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_B2AcceptedDate)));
			this.AcceptedZDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8B7A6216-0162-48F8-8DDE-628A253CD13D", "Date Accepted");
			this.AcceptedZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 59, true);
			this.AcceptedZDateEdit.Name = "AcceptedZDateEdit";
			this.AcceptedZDateEdit.TabIndex = 25;
			// 
			// ConfirmedZDateEdit
			// 
			this.ConfirmedZDateEdit.AllowDrop = true;
			this.ConfirmedZDateEdit.AutoCompleteMonthThreshold = 1;
			this.ConfirmedZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ConfirmedZDateEdit, "CA_ConfirmedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_ConfirmedDate)));
			this.ConfirmedZDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("F6C6DDD4-D061-420B-B3B6-43F9A1AF57DC", "Date Confirmed");
			this.ConfirmedZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 35, true);
			this.ConfirmedZDateEdit.Name = "ConfirmedZDateEdit";
			this.ConfirmedZDateEdit.TabIndex = 24;
			// 
			// SubmittedOverrideCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SubmittedOverrideCheckBox, "CA_B2SubmissionDateOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_B2SubmissionDateOverride)));
			this.SubmittedOverrideCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BE85FC79-A829-470B-99E9-798C487B49F5", "Override");
			this.SubmittedOverrideCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Left;
			this.SubmittedOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SubmittedOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 11, true);
			this.SubmittedOverrideCheckBox.Name = "SubmittedOverrideCheckBox";
			this.SubmittedOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.SubmittedOverrideCheckBox.TabIndex = 23;
			this.SubmittedOverrideCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.SubmittedOverrideCheckBox.UseVisualStyleBackColor = true;
			// 
			// SubmittedZDateEdit
			// 
			this.SubmittedZDateEdit.AllowDrop = true;
			this.SubmittedZDateEdit.AutoCompleteMonthThreshold = 1;
			this.SubmittedZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SubmittedZDateEdit, "CA_B2SubmissionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_B2SubmissionDate)));
			this.SubmittedZDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("566E8E36-DDA2-4423-B17D-64E1FE66EDE5", "Date Submitted");
			this.SubmittedZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 11, true);
			this.SubmittedZDateEdit.Name = "SubmittedZDateEdit";
			this.SubmittedZDateEdit.TabIndex = 22;
			// 
			// IsOurFaultCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsOurFaultCheckBox, "CA_IsOurFault");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_IsOurFault)));
			this.IsOurFaultCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("AF055598-A291-47FA-96E6-C708AC923C4A", "Broker Issue");
			this.IsOurFaultCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsOurFaultCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsOurFaultCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 141, true);
			this.IsOurFaultCheckBox.Name = "IsOurFaultCheckBox";
			this.IsOurFaultCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.IsOurFaultCheckBox.TabIndex = 18;
			this.IsOurFaultCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.IsOurFaultCheckBox.UseVisualStyleBackColor = true;
			// 
			// InitiatedByDropEdit
			// 
			this.InitiatedByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InitiatedByDropEdit, "CA_InitiatedBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_InitiatedBy)));
			this.InitiatedByDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("05AE2299-FC34-48C2-B82F-7E1F226F955B", "Initiated By");
			this.InitiatedByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(803, 222, true);
			this.InitiatedByDropEdit.Name = "InitiatedByDropEdit";
			this.InitiatedByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.InitiatedByDropEdit.TabIndex = 24;
			// 
			// ChequeNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChequeNumberTextBox, "CA_ChequeNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_ChequeNo)));
			this.ChequeNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("975C7101-4827-4D3D-A896-74E9385EA9F1", "DN/Cheque No.");
			this.ChequeNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(803, 247, true);
			this.ChequeNumberTextBox.Name = "ChequeNumberTextBox";
			this.ChequeNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.ChequeNumberTextBox.TabIndex = 25;
			// 
			// ChequeDateDateEdit
			// 
			this.ChequeDateDateEdit.AllowDrop = true;
			this.ChequeDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ChequeDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ChequeDateDateEdit, "CA_ChequeDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_ChequeDate)));
			this.ChequeDateDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("D58F60F3-EFCE-4CD6-86D2-85399FB73303", "Cheque Date");
			this.ChequeDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(803, 272, true);
			this.ChequeDateDateEdit.Name = "ChequeDateDateEdit";
			this.ChequeDateDateEdit.TabIndex = 26;
			// 
			// AmendmentToDropEdit
			// 
			this.AmendmentToDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AmendmentToDropEdit, "CA_AmendmentTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_AmendmentTo)));
			this.AmendmentToDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4227EAA7-FFA5-4BA3-8CD1-1F5B56475060", "Amendment to");
			this.AmendmentToDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 187, true);
			this.AmendmentToDropEdit.Name = "AmendmentToDropEdit";
			this.AmendmentToDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.AmendmentToDropEdit.TabIndex = 21;
			// 
			// BaseB2UserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ImporterOrganisationControlWithMiscellaneous);
			this.Controls.Add(this.DetailsGroupBox);
			this.Controls.Add(this.MailToOrganisationControlWithMiscellaneous);
			this.Controls.Add(this.LongTextDetailsGroupBox);
			this.Controls.Add(this.DatesGroupBox);
			this.Controls.Add(this.InitiatedByDropEdit);
			this.Controls.Add(this.ChequeNumberTextBox);
			this.Controls.Add(this.ChequeDateDateEdit);
			this.Controls.Add(this.AmountDueImporterCalcEdit);
			this.Controls.Add(this.AmountDueCBSACalcEdit);
			this.Name = "BaseB2UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 467, true);
			this.Controls.SetChildIndex(this.ChequeDateDateEdit, 0);
			this.Controls.SetChildIndex(this.ChequeNumberTextBox, 0);
			this.Controls.SetChildIndex(this.InitiatedByDropEdit, 0);
			this.Controls.SetChildIndex(this.AmountDueImporterCalcEdit, 0);
			this.Controls.SetChildIndex(this.AmountDueCBSACalcEdit, 0);
			this.Controls.SetChildIndex(this.DatesGroupBox, 0);
			this.Controls.SetChildIndex(this.LongTextDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.MailToOrganisationControlWithMiscellaneous, 0);
			this.Controls.SetChildIndex(this.DetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.ImporterOrganisationControlWithMiscellaneous, 0);
			this.Controls.SetChildIndex(this.DeclarationDetailsGroupBox, 0);
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImporterOrganisationControlWithMiscellaneous.ResumeLayout(true);
			this.ImporterOrganisationControlWithMiscellaneous.PerformLayout();
			this.MailToOrganisationControlWithMiscellaneous.ResumeLayout(true);
			this.MailToOrganisationControlWithMiscellaneous.PerformLayout();
			this.B2TypeDropEdit.ResumeLayout(true);
			this.B2TypeDropEdit.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.AuthorisationDateDateEdit1.ResumeLayout(true);
			this.AuthorisationDateDateEdit1.PerformLayout();
			this.OriginalAccountingDateDateEdit.ResumeLayout(true);
			this.OriginalAccountingDateDateEdit.PerformLayout();
			this.ClearancePortCodeFindBox.ResumeLayout(true);
			this.ClearancePortCodeFindBox.PerformLayout();
			this.CarrierCodeFindBox.ResumeLayout(true);
			this.CarrierCodeFindBox.PerformLayout();
			this.OriginalTransactionNumberCodeFindBox.ResumeLayout(true);
			this.OriginalTransactionNumberCodeFindBox.PerformLayout();
			this.LongTextDetailsGroupBox.ResumeLayout(false);
			this.LongTextDetailsGroupBox.PerformLayout();
			this.ExplanationLongTextBox.ResumeLayout(true);
			this.ExplanationLongTextBox.PerformLayout();
			this.DatesGroupBox.ResumeLayout(false);
			this.DatesGroupBox.PerformLayout();
			this.AcceptedZDateEdit.ResumeLayout(true);
			this.AcceptedZDateEdit.PerformLayout();
			this.ConfirmedZDateEdit.ResumeLayout(true);
			this.ConfirmedZDateEdit.PerformLayout();
			this.SubmittedZDateEdit.ResumeLayout(true);
			this.SubmittedZDateEdit.PerformLayout();
			this.InitiatedByDropEdit.ResumeLayout(true);
			this.InitiatedByDropEdit.PerformLayout();
			this.AmendmentToDropEdit.ResumeLayout(true);
			this.AmendmentToDropEdit.PerformLayout();
			this.ChequeDateDateEdit.ResumeLayout(true);
			this.ChequeDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Customs.GUI.ZOrganisationControlWithMiscellaneous ImporterOrganisationControlWithMiscellaneous;
		protected Customs.GUI.ZOrganisationControlWithMiscellaneous MailToOrganisationControlWithMiscellaneous;
		protected ZArchitecture.GUI.ZDropEdit B2TypeDropEdit;
		protected ZArchitecture.ZTextBox CheckDigitTextBox;
		protected ZArchitecture.ZTextBox SequentialNumberTextBox;
		protected ZArchitecture.ZTextBox AccountSecurityCodeTextBox;
		protected ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		protected OriginalB3TransactionNumberCodeFindBox OriginalTransactionNumberCodeFindBox;
		protected ZArchitecture.GUI.ZDateEdit OriginalAccountingDateDateEdit;
		protected ZArchitecture.GUI.ZCodeFindBox ClearancePortCodeFindBox;
		protected ZArchitecture.ZTextBox SecurityNoTextBox;
		protected ZArchitecture.GUI.ZDateEdit AuthorisationDateDateEdit1;
		protected ZArchitecture.ZCalcEdit AmountDueCBSACalcEdit;
		protected ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
		protected ZArchitecture.ZCalcEdit AmountDueImporterCalcEdit;
		protected ZArchitecture.GUI.ZCodeFindBox CarrierCodeFindBox;
		protected ZArchitecture.ZCalcEdit AnySightDepositAmountCalcEdit;
		protected ZArchitecture.ZCalcEdit ClaimedInterestAmountCalcEdit;
		protected ZArchitecture.GUI.ZCheckBox DocsAttachedCheckBox;
		protected ZArchitecture.GUI.ZGroupBox LongTextDetailsGroupBox;
		protected ZArchitecture.ZTextBox UndertTextBox;
		protected ZArchitecture.ZTextBox RequestJustificationTextBox;
		protected Enterprise.Customs.GUI.LongTextControl ExplanationLongTextBox;
		protected ZArchitecture.GUI.ZGroupBox DatesGroupBox;
		protected ZArchitecture.GUI.ZDateEdit SubmittedZDateEdit;
		protected ZArchitecture.GUI.ZCheckBox SubmittedOverrideCheckBox;
		protected ZArchitecture.GUI.ZDateEdit ConfirmedZDateEdit;
		protected ZArchitecture.GUI.ZDateEdit AcceptedZDateEdit;
		protected ZArchitecture.GUI.ZCheckBox AcceptedOverrideCheckBox;
		protected ZArchitecture.GUI.ZCheckBox IsOurFaultCheckBox;
		protected ZArchitecture.GUI.ZDropEdit InitiatedByDropEdit;
		protected ZArchitecture.ZTextBox ChequeNumberTextBox;
		protected ZArchitecture.GUI.ZDateEdit ChequeDateDateEdit;
		protected ZArchitecture.GUI.ZDropEdit AmendmentToDropEdit;
		protected Enterprise.ZArchitecture.ZTextBox FormattedTransactionNumberTextBox;
	}
}

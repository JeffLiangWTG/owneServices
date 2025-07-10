namespace Enterprise.Customs.CA.GUI
{
	partial class K84DeclarationDetailsUserControl
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
			this.B3EntryMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.B3EntryStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransactionNumberPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CheckDigitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SequentialNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SecurityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FormattedTransactionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JE_MessageStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ScheduledB3DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.B3AcceptedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LowValueShipmentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CA_OGDStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntrySubmittedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CA_AccountingAgeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CADVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.B3EntrySubmittedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LatestNoticeStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LatestNoticeProcessingDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.WarehouseTransactionStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransactionNumberPanel.SuspendLayout();
			this.ScheduledB3DateEdit.SuspendLayout();
			this.B3AcceptedDateEdit.SuspendLayout();
			this.EntrySubmittedDateEdit.SuspendLayout();
			this.B3EntrySubmittedDateEdit.SuspendLayout();
			this.LatestNoticeProcessingDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			// 
			// B3EntryMessageTextBox
			// 
			this.BindingSource.SetBindingMember(this.B3EntryMessageTextBox, "B3EntryHeader.MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B3EntryHeader.MessageStatusDescription)));
			this.B3EntryMessageTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b1508ea9-c5da-4c3e-a86f-b9b597694cf6", "Entry (B3) Message");
			this.B3EntryMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.B3EntryMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 128, true);
			this.B3EntryMessageTextBox.Name = "B3EntryMessageTextBox";
			this.B3EntryMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.B3EntryMessageTextBox.TabIndex = 11;
			// 
			// B3EntryStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.B3EntryStatusTextBox, "B3EntryHeader.EntryHeaderStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B3EntryHeader.EntryHeaderStatusDescription)));
			this.B3EntryStatusTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2c667ca4-cb46-4398-a2cf-2d11400c31eb", "Entry (B3) Status");
			this.B3EntryStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.B3EntryStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 103, true);
			this.B3EntryStatusTextBox.Name = "B3EntryStatusTextBox";
			this.B3EntryStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.B3EntryStatusTextBox.TabIndex = 8;
			// 
			// ReleaseMessageTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReleaseMessageTextBox, "ReleaseEntryHeader.MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseEntryHeader.MessageStatusDescription)));
			this.ReleaseMessageTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2c175ac7-a672-45b9-ac4f-e5d75417ae47", "Release Message Status");
			this.ReleaseMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReleaseMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 53, true);
			this.ReleaseMessageTextBox.Name = "ReleaseMessageTextBox";
			this.ReleaseMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.ReleaseMessageTextBox.TabIndex = 3;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "JE_EntryStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_EntryStatusDescription)));
			this.StatusTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|67A0C6D7-6B98-4897-AB35-AD6734A9E400", "Release Status");
			this.StatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 28, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.StatusTextBox.TabIndex = 2;
			// 
			// TransactionNumberPanel
			// 
			this.TransactionNumberPanel.Controls.Add(this.CheckDigitTextBox);
			this.TransactionNumberPanel.Controls.Add(this.SequentialNumberTextBox);
			this.TransactionNumberPanel.Controls.Add(this.SecurityCodeTextBox);
			this.TransactionNumberPanel.Controls.Add(this.FormattedTransactionNumberTextBox);
			this.TransactionNumberPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.TransactionNumberPanel.Name = "TransactionNumberPanel";
			this.TransactionNumberPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 24, true);
			this.TransactionNumberPanel.TabIndex = 0;
			// 
			// CheckDigitTextBox
			// 
			this.CheckDigitTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CheckDigitTextBox, "TransactionNumber.CheckDigit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.CheckDigit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CheckDigitTextBox, false);
			this.CheckDigitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 1, true);
			this.CheckDigitTextBox.Name = "CheckDigitTextBox";
			this.CheckDigitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 20, true);
			this.CheckDigitTextBox.TabIndex = 2;
			this.CheckDigitTextBox.Text = "1";
			this.CheckDigitTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SequentialNumberTextBox
			// 
			this.SequentialNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SequentialNumberTextBox, "TransactionNumber.SequentialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.SequentialNumber)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SequentialNumberTextBox, false);
			this.SequentialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 1, true);
			this.SequentialNumberTextBox.Name = "SequentialNumberTextBox";
			this.SequentialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.SequentialNumberTextBox.TabIndex = 1;
			this.SequentialNumberTextBox.Text = "12345678";
			this.SequentialNumberTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SecurityCodeTextBox
			// 
			this.SecurityCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SecurityCodeTextBox, "TransactionNumber.AccountSecurityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.AccountSecurityCode)));
			this.SecurityCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|94464C57-8D17-4A38-8FEB-B9EA63552DBE", "Transaction Number");
			this.SecurityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 1, true);
			this.SecurityCodeTextBox.Name = "SecurityCodeTextBox";
			this.SecurityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.SecurityCodeTextBox.TabIndex = 0;
			this.SecurityCodeTextBox.Text = "12345000000012";
			this.SecurityCodeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FormattedTransactionNumberTextBox
			// 
			this.FormattedTransactionNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FormattedTransactionNumberTextBox, "TransactionNumber.FormattedTransactionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).TransactionNumber.FormattedTransactionNumber)));
			this.FormattedTransactionNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAJobDeclarationUserControl|d50d8243-17b7-4156-bdde-447c14229331", "Transaction Number");
			this.FormattedTransactionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 2, true);
			this.FormattedTransactionNumberTextBox.Name = "FormattedTransactionNumberTextBox";
			this.FormattedTransactionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.FormattedTransactionNumberTextBox.TabIndex = 0;
			this.FormattedTransactionNumberTextBox.Text = "12345";
			this.FormattedTransactionNumberTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// JE_MessageStatusDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.JE_MessageStatusDescriptionTextBox, "JE_MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_MessageStatusDescription)));
			this.JE_MessageStatusDescriptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("50dc2dc9-a0ec-4a2a-9fb2-3b0b3a053508", "Last Message");
			this.JE_MessageStatusDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.JE_MessageStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 3, true);
			this.JE_MessageStatusDescriptionTextBox.Name = "JE_MessageStatusDescriptionTextBox";
			this.JE_MessageStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.JE_MessageStatusDescriptionTextBox.TabIndex = 1;
			// 
			// ScheduledB3DateEdit
			// 
			this.ScheduledB3DateEdit.AllowDrop = true;
			this.ScheduledB3DateEdit.AutoCompleteMonthThreshold = 1;
			this.ScheduledB3DateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ScheduledB3DateEdit, "ScheduledB3SendingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ScheduledB3SendingDate)));
			this.ScheduledB3DateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6D22C7AC-3B6D-44D4-9AB5-5452D0A1DC15", "Entry Scheduled Time", "Scheduled Sending Time For Entry Message", "");
			this.ScheduledB3DateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ScheduledB3DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 128, true);
			this.ScheduledB3DateEdit.Name = "ScheduledB3DateEdit";
			this.ScheduledB3DateEdit.TabIndex = 12;
			// 
			// B3AcceptedDateEdit
			// 
			this.B3AcceptedDateEdit.AllowDrop = true;
			this.B3AcceptedDateEdit.AutoCompleteMonthThreshold = 1;
			this.B3AcceptedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.B3AcceptedDateEdit, "B3AcceptedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B3AcceptedDate)));
			this.B3AcceptedDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3ED2A8AB-B9A1-4139-ABCF-4F92D8B0E2A8", "Entry Accepted Date");
			this.B3AcceptedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.B3AcceptedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 128, true);
			this.B3AcceptedDateEdit.Name = "B3AcceptedDateEdit";
			this.B3AcceptedDateEdit.TabIndex = 12;
			// 
			// LowValueShipmentLabel
			// 
			this.LowValueShipmentLabel.BackColor = System.Drawing.Color.LightSalmon;
			this.LowValueShipmentLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("13DA0F31-71B3-4FE0-A6B0-584DD652E913", "Low Value Shipment");
			this.LowValueShipmentLabel.IsFontBold = true;
			this.LowValueShipmentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 26, true);
			this.LowValueShipmentLabel.Name = "LowValueShipmentLabel";
			this.LowValueShipmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 22, true);
			this.LowValueShipmentLabel.TabIndex = 0;
			this.LowValueShipmentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// CA_OGDStatusDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_OGDStatusDescriptionTextBox, "CA_OGDStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_OGDStatusDescription)));
			this.CA_OGDStatusDescriptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("41119b7d-e56a-4263-bcab-cceb7c80c54a", "OGD Status");
			this.CA_OGDStatusDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CA_OGDStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 153, true);
			this.CA_OGDStatusDescriptionTextBox.Name = "CA_OGDStatusDescriptionTextBox";
			this.CA_OGDStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.CA_OGDStatusDescriptionTextBox.TabIndex = 13;
			// 
			// EntrySubmittedDateEdit
			// 
			this.EntrySubmittedDateEdit.AllowDrop = true;
			this.EntrySubmittedDateEdit.AutoCompleteMonthThreshold = 1;
			this.EntrySubmittedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EntrySubmittedDateEdit, "RelEntrySubmittedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).RelEntrySubmittedDate)));
			this.EntrySubmittedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EntrySubmittedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 53, true);
			this.EntrySubmittedDateEdit.Name = "EntrySubmittedDateEdit";
			this.EntrySubmittedDateEdit.TabIndex = 4;
			// 
			// CA_AccountingAgeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_AccountingAgeTextBox, "CA_AccountingAge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_AccountingAge)));
			this.CA_AccountingAgeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("83bf40bb-4047-426f-a785-3d1b7c31b15f", "Days Since Release");
			this.CA_AccountingAgeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CA_AccountingAgeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 103, true);
			this.CA_AccountingAgeTextBox.Name = "CA_AccountingAgeTextBox";
			this.CA_AccountingAgeTextBox.ReadOnly = true;
			this.CA_AccountingAgeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.CA_AccountingAgeTextBox.TabIndex = 7;
			// 
			// CADVersionTextBox
			// 
			this.BindingSource.SetBindingMember(this.CADVersionTextBox, "CADVersionID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CADVersionID)));
			this.CADVersionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4B38610C-9B22-40F9-85BC-C1D1874427B3", "CAD Version");
			this.CADVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CADVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 128, true);
			this.CADVersionTextBox.Name = "CADVersionTextBox";
			this.CADVersionTextBox.ReadOnly = true;
			this.CADVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.CADVersionTextBox.TabIndex = 10;
			// 
			// B3EntrySubmittedDateEdit
			// 
			this.B3EntrySubmittedDateEdit.AllowDrop = true;
			this.B3EntrySubmittedDateEdit.AutoCompleteMonthThreshold = 1;
			this.B3EntrySubmittedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.B3EntrySubmittedDateEdit, "B3EntrySubmittedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).B3EntrySubmittedDate)));
			this.B3EntrySubmittedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.B3EntrySubmittedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 103, true);
			this.B3EntrySubmittedDateEdit.Name = "B3EntrySubmittedDateEdit";
			this.B3EntrySubmittedDateEdit.TabIndex = 9;
			// 
			// LatestNoticeStatusDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.LatestNoticeStatusDescriptionTextBox, "LatestNoticeMessage.StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).LatestNoticeMessage.StatusDescription)));
			this.LatestNoticeStatusDescriptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("968e12a3-3de3-4c1f-b465-1beb0174d087", "Latest Notice");
			this.LatestNoticeStatusDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LatestNoticeStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 78, true);
			this.LatestNoticeStatusDescriptionTextBox.Name = "LatestNoticeStatusDescriptionTextBox";
			this.LatestNoticeStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.LatestNoticeStatusDescriptionTextBox.TabIndex = 5;
			// 
			// LatestNoticeProcessingDateEdit
			// 
			this.LatestNoticeProcessingDateEdit.AllowDrop = true;
			this.LatestNoticeProcessingDateEdit.AutoCompleteMonthThreshold = 1;
			this.LatestNoticeProcessingDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LatestNoticeProcessingDateEdit, "LatestNoticeMessage.RNSProcessingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).LatestNoticeMessage.RNSProcessingDate)));
			this.LatestNoticeProcessingDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f2df39c5-ea10-40a0-9fca-86bfeef15686", "Processing Date");
			this.LatestNoticeProcessingDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LatestNoticeProcessingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 78, true);
			this.LatestNoticeProcessingDateEdit.Name = "LatestNoticeProcessingDateEdit";
			this.LatestNoticeProcessingDateEdit.TabIndex = 6;
			// 
			// WarehouseTransactionStatusDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.WarehouseTransactionStatusDescriptionTextBox, "WarehouseTransactionStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).WarehouseTransactionStatusDescription)));
			this.WarehouseTransactionStatusDescriptionTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8502754d-f731-4ca8-8c40-e5fb28464f33", "Warehouse Status");
			this.WarehouseTransactionStatusDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.WarehouseTransactionStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 179, true);
			this.WarehouseTransactionStatusDescriptionTextBox.Name = "WarehouseTransactionStatusDescriptionTextBox";
			this.WarehouseTransactionStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.WarehouseTransactionStatusDescriptionTextBox.TabIndex = 14;
			// 
			// K84DeclarationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.WarehouseTransactionStatusDescriptionTextBox);
			this.Controls.Add(this.LatestNoticeProcessingDateEdit);
			this.Controls.Add(this.LatestNoticeStatusDescriptionTextBox);
			this.Controls.Add(this.B3EntrySubmittedDateEdit);
			this.Controls.Add(this.CA_AccountingAgeTextBox);
			this.Controls.Add(this.CADVersionTextBox);
			this.Controls.Add(this.CA_OGDStatusDescriptionTextBox);
			this.Controls.Add(this.B3EntryMessageTextBox);
			this.Controls.Add(this.B3EntryStatusTextBox);
			this.Controls.Add(this.ReleaseMessageTextBox);
			this.Controls.Add(this.StatusTextBox);
			this.Controls.Add(this.TransactionNumberPanel);
			this.Controls.Add(this.JE_MessageStatusDescriptionTextBox);
			this.Controls.Add(this.EntrySubmittedDateEdit);
			this.Controls.Add(this.ScheduledB3DateEdit);
			this.Controls.Add(this.B3AcceptedDateEdit);
			this.Controls.Add(this.LowValueShipmentLabel);
			this.Name = "K84DeclarationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 204, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransactionNumberPanel.ResumeLayout(false);
			this.TransactionNumberPanel.PerformLayout();
			this.ScheduledB3DateEdit.ResumeLayout(true);
			this.ScheduledB3DateEdit.PerformLayout();
			this.B3AcceptedDateEdit.ResumeLayout(true);
			this.B3AcceptedDateEdit.PerformLayout();
			this.EntrySubmittedDateEdit.ResumeLayout(true);
			this.EntrySubmittedDateEdit.PerformLayout();
			this.B3EntrySubmittedDateEdit.ResumeLayout(true);
			this.B3EntrySubmittedDateEdit.PerformLayout();
			this.LatestNoticeProcessingDateEdit.ResumeLayout(true);
			this.LatestNoticeProcessingDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox FormattedTransactionNumberTextBox;
		private Enterprise.ZArchitecture.ZTextBox SecurityCodeTextBox;
		private Enterprise.ZArchitecture.ZTextBox CheckDigitTextBox;
		private Enterprise.ZArchitecture.ZTextBox SequentialNumberTextBox;
		private Enterprise.ZArchitecture.ZTextBox JE_MessageStatusDescriptionTextBox;
		private Enterprise.ZArchitecture.ZTextBox StatusTextBox;
		private Enterprise.ZArchitecture.GUI.ZPanel TransactionNumberPanel;
		private ZArchitecture.ZTextBox ReleaseMessageTextBox;
		private ZArchitecture.ZTextBox B3EntryMessageTextBox;
		private ZArchitecture.ZTextBox B3EntryStatusTextBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ScheduledB3DateEdit;
		private Enterprise.ZArchitecture.ZLabel LowValueShipmentLabel;
		private ZArchitecture.ZTextBox CA_OGDStatusDescriptionTextBox;
		private ZArchitecture.ZTextBox CA_AccountingAgeTextBox;
		private ZArchitecture.ZTextBox CADVersionTextBox;
		private ZArchitecture.GUI.ZDateEdit EntrySubmittedDateEdit;
		private ZArchitecture.GUI.ZDateEdit B3EntrySubmittedDateEdit;
		private ZArchitecture.ZTextBox LatestNoticeStatusDescriptionTextBox;
		private ZArchitecture.GUI.ZDateEdit LatestNoticeProcessingDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit B3AcceptedDateEdit;
		private ZArchitecture.ZTextBox WarehouseTransactionStatusDescriptionTextBox;
	}
}

using ExternalMailManager = MailManager.Module;

namespace Enterprise.MailManager.GUI
{
	public partial class MailItemForm
	{
		#region Windows Designer Generated Code

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BodyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BodyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FromTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReplyToTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SendDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReceivedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RecipientsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RecipientsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AttachmentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AttachmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CreatedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CreatedByTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HeaderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SaveOrAddAttachmentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveAllAttachmentsOrRemoveAttachmentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.SaveEntireObjectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ApplicationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BodyGroupBox.SuspendLayout();
			this.DirectionDropEdit.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.SendDateEdit.SuspendLayout();
			this.ReceivedDateEdit.SuspendLayout();
			this.RecipientsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).BeginInit();
			this.RecipientsGrid.SuspendLayout();
			this.AttachmentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttachmentsGrid)).BeginInit();
			this.AttachmentsGrid.SuspendLayout();
			this.CreatedDateEdit.SuspendLayout();
			this.HeaderGroupBox.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 514, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 22, true);
			this.MainStatusBar.TabIndex = 25;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(734);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MailManager.Business.MailItem);
			//
			// BodyGroupBox
			//
			this.BodyGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BodyGroupBox.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|a034b88c-f622-4480-a5c6-89c6a4e2cd93", "Message Body");
			this.BodyGroupBox.Controls.Add(this.BodyTextBox);
			this.BodyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 364, true);
			this.BodyGroupBox.Name = "BodyGroupBox";
			this.BodyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 117, true);
			this.BodyGroupBox.TabIndex = 23;
			this.BodyGroupBox.TabStop = false;
			//
			// BodyTextBox
			//
			this.BodyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BodyTextBox, "MI_Body");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailItem)(null)).MI_Body)));
			this.BodyTextBox.CaptionResourceString = null;
			this.BodyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BodyTextBox, false);
			this.BodyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.BodyTextBox.Multiline = true;
			this.BodyTextBox.Name = "BodyTextBox";
			this.BodyTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.BodyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 95, true);
			this.BodyTextBox.TabIndex = 0;
			//
			// DirectionDropEdit
			//
			this.DirectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DirectionDropEdit, "MI_Direction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MailManager.Business.MailItem)(null)).MI_Direction)));
			this.DirectionDropEdit.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|3ee94eef-12a7-4ba5-89e0-8eb714385b3c", "Direction");
			this.DirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 7, true);
			this.DirectionDropEdit.Name = "DirectionDropEdit";
			this.DirectionDropEdit.ShouldResizeByMaxLength = true;
			this.DirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.DirectionDropEdit.TabIndex = 1;
			//
			// StatusDropEdit
			//
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "MI_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MailManager.Business.MailItem)(null)).MI_Status)));
			this.StatusDropEdit.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|e7d57f03-ad01-4815-bc4e-106ef8d443bd", "Status");
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 7, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.ShouldResizeByMaxLength = true;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.StatusDropEdit.TabIndex = 3;
			//
			// FromTextBox
			//
			this.BindingSource.SetBindingMember(this.FromTextBox, "MI_From");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailItem)(null)).MI_From)));
			this.FromTextBox.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|b0326f40-7d14-47ef-9e4f-ad59038bcec0", "From");
			this.FromTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FromTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 30, true);
			this.FromTextBox.Name = "FromTextBox";
			this.FromTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 20, true);
			this.FromTextBox.TabIndex = 9;
			//
			// ReplyToTextBox
			//
			this.BindingSource.SetBindingMember(this.ReplyToTextBox, "MI_ReplyTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailItem)(null)).MI_ReplyTo)));
			this.ReplyToTextBox.CaptionResourceString = null;
			this.ReplyToTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReplyToTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 52, true);
			this.ReplyToTextBox.Name = "ReplyToTextBox";
			this.ReplyToTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 20, true);
			this.ReplyToTextBox.TabIndex = 13;
			//
			// SendDateEdit
			//
			this.SendDateEdit.AllowDrop = true;
			this.SendDateEdit.AutoCompleteMonthThreshold = 1;
			this.SendDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SendDateEdit, "CalcSendDateTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MailManager.Business.MailItem)(null)).CalcSendDateTimeLocal)));
			this.SendDateEdit.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|4c0800a6-2752-4aa6-abae-a6097dd24a8a", "Send");
			this.SendDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.SendDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(616, 30, true);
			this.SendDateEdit.Name = "SendDateEdit";
			this.SendDateEdit.TabIndex = 11;
			//
			// ReceivedDateEdit
			//
			this.ReceivedDateEdit.AllowDrop = true;
			this.ReceivedDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReceivedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReceivedDateEdit, "CalcReceivedDateTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MailManager.Business.MailItem)(null)).CalcReceivedDateTimeLocal)));
			this.ReceivedDateEdit.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|15c64b84-e22d-4e3f-9bed-4fef2a0345e8", "Received");
			this.ReceivedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReceivedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(616, 52, true);
			this.ReceivedDateEdit.Name = "ReceivedDateEdit";
			this.ReceivedDateEdit.TabIndex = 15;
			//
			// RecipientsGroupBox
			//
			this.RecipientsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.RecipientsGroupBox.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|77763a3c-762d-465e-b24e-323bb7875185", "Recipients");
			this.RecipientsGroupBox.Controls.Add(this.RecipientsGrid);
			this.RecipientsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 104, true);
			this.RecipientsGroupBox.Name = "RecipientsGroupBox";
			this.RecipientsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 93, true);
			this.RecipientsGroupBox.TabIndex = 18;
			this.RecipientsGroupBox.TabStop = false;
			//
			// RecipientsGrid
			//
			this.RecipientsGrid.AllowNavigation = false;
			this.RecipientsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RecipientsGrid, "MailRecipientsForGUIBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MailManager.Business.MailItem)(null)).MailRecipientsForGUIBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailRecipient)(((System.Collections.IList)(((Enterprise.MailManager.Business.MailItem)(null)).MailRecipientsForGUIBinding)).SyncRoot)).MR_RecipientType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailRecipient)(((System.Collections.IList)(((Enterprise.MailManager.Business.MailItem)(null)).MailRecipientsForGUIBinding)).SyncRoot)).MR_RecipientMailAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MailManager.Business.MailRecipient)(((System.Collections.IList)(((Enterprise.MailManager.Business.MailItem)(null)).MailRecipientsForGUIBinding)).SyncRoot)).CalcDeliveredTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MailManager.Business.MailRecipient)(((System.Collections.IList)(((Enterprise.MailManager.Business.MailItem)(null)).MailRecipientsForGUIBinding)).SyncRoot)).MR_AckAttempt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MailManager.Business.MailRecipient)(((System.Collections.IList)(((Enterprise.MailManager.Business.MailItem)(null)).MailRecipientsForGUIBinding)).SyncRoot)).MR_LastAttempt)));
			this.RecipientsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.ColumnName = "MR_RecipientType";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo6.ColumnName = "MR_RecipientMailAddress";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zDateEditColumnStyleInfo3.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|03ae473f-a6ef-4ade-83f8-edcf24e94539", "Delivery Time");
			zDateEditColumnStyleInfo3.ColumnName = "CalcDeliveredTimeLocal";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "MR_AckAttempt";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo4.ColumnName = "MR_LastAttempt";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.RecipientsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.RecipientsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.RecipientsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RecipientsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.RecipientsGrid.GridId = "bdc51340-3d70-4b85-b0b4-ec8853b5effa";
			this.RecipientsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RecipientsGrid.LayoutKey = "RecipientsGrid";
			this.RecipientsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.RecipientsGrid.Name = "RecipientsGrid";
			this.RecipientsGrid.ReadOnly = true;
			this.RecipientsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 74, true);
			this.RecipientsGrid.TabIndex = 0;
			//
			// SubjectTextBox
			//
			this.BindingSource.SetBindingMember(this.SubjectTextBox, "MI_Subject");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailItem)(null)).MI_Subject)));
			this.SubjectTextBox.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|aa2e662e-86cb-4f0f-8b61-1b0948ea7225", "Subject");
			this.SubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 74, true);
			this.SubjectTextBox.Name = "SubjectTextBox";
			this.SubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 20, true);
			this.SubjectTextBox.TabIndex = 17;
			//
			// AttachmentsGroupBox
			//
			this.AttachmentsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AttachmentsGroupBox.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|3e9d1607-fc9d-4e94-b357-64ba89b5a9ae", "Attachments");
			this.AttachmentsGroupBox.Controls.Add(this.AttachmentsGrid);
			this.AttachmentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 201, true);
			this.AttachmentsGroupBox.Name = "AttachmentsGroupBox";
			this.AttachmentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 74, true);
			this.AttachmentsGroupBox.TabIndex = 19;
			this.AttachmentsGroupBox.TabStop = false;
			//
			// AttachmentsGrid
			//
			this.AttachmentsGrid.AllowNavigation = false;
			this.AttachmentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AttachmentsGrid, "MailAttachments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MailManager.Business.MailItem)(null)).MailAttachments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailAttachment)(((System.Collections.IList)(((Enterprise.MailManager.Business.MailItem)(null)).MailAttachments)).SyncRoot)).MA_FileName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailAttachment)(((System.Collections.IList)(((Enterprise.MailManager.Business.MailItem)(null)).MailAttachments)).SyncRoot)).HumanReadableAttachmentSize)));
			this.AttachmentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "MA_FileName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(450);
			zTextBoxColumnStyleInfo2.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|HumanReadableAttachmentSize", "Size");
			zTextBoxColumnStyleInfo2.ColumnName = "HumanReadableAttachmentSize";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AttachmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AttachmentsGrid.GridId = "881a490a-269f-4441-8172-0f312a7ad5fe";
			this.AttachmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AttachmentsGrid.IsWholeRowSelectedOnClick = true;
			this.AttachmentsGrid.LayoutKey = "AttachmentsGrid";
			this.AttachmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.AttachmentsGrid.Name = "AttachmentsGrid";
			this.AttachmentsGrid.ReadOnly = true;
			this.AttachmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 56, true);
			this.AttachmentsGrid.TabIndex = 0;
			//
			// CreatedDateEdit
			//
			this.CreatedDateEdit.AllowDrop = true;
			this.CreatedDateEdit.AutoCompleteMonthThreshold = 1;
			this.CreatedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CreatedDateEdit, "MI_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MailManager.Business.MailItem)(null)).MI_SystemCreateTimeUtc)));
			this.CreatedDateEdit.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|07ce63a5-2531-4f29-80bf-1d5e352ed07c", "Created");
			this.CreatedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.CreatedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 7, true);
			this.CreatedDateEdit.Name = "CreatedDateEdit";
			this.CreatedDateEdit.TabIndex = 5;
			//
			// CreatedByTextBox
			//
			this.BindingSource.SetBindingMember(this.CreatedByTextBox, "MI_SystemCreateUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailItem)(null)).MI_SystemCreateUser)));
			this.CreatedByTextBox.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|d6d3899d-7026-4c69-afab-ebc1229c71e4", "By");
			this.CreatedByTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CreatedByTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(575, 7, true);
			this.CreatedByTextBox.Name = "CreatedByTextBox";
			this.CreatedByTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
			this.CreatedByTextBox.TabIndex = 7;
			//
			// HeaderGroupBox
			//
			this.HeaderGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.HeaderGroupBox.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|3e4b49ae-63b6-4a1d-b0b4-deafad4d5acf", "Message Header");
			this.HeaderGroupBox.Controls.Add(this.HeaderTextBox);
			this.HeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 275, true);
			this.HeaderGroupBox.Name = "HeaderGroupBox";
			this.HeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 89, true);
			this.HeaderGroupBox.TabIndex = 22;
			this.HeaderGroupBox.TabStop = false;
			//
			// HeaderTextBox
			//
			this.HeaderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.HeaderTextBox, "MI_Header");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailItem)(null)).MI_Header)));
			this.HeaderTextBox.CaptionResourceString = null;
			this.HeaderTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.HeaderTextBox, false);
			this.HeaderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.HeaderTextBox.Multiline = true;
			this.HeaderTextBox.Name = "HeaderTextBox";
			this.HeaderTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.HeaderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 67, true);
			this.HeaderTextBox.TabIndex = 0;
			//
			// SaveOrAddAttachmentButton
			//
			this.SaveOrAddAttachmentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveOrAddAttachmentButton.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|5e0a09d5-3d87-4279-b61e-5aafea069458", "Save &Selected Attachments");
			this.SaveOrAddAttachmentButton.IsCaptionOverridden = false;
			this.SaveOrAddAttachmentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 200, true);
			this.SaveOrAddAttachmentButton.Name = "SaveOrAddAttachmentButton";
			this.SaveOrAddAttachmentButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveOrAddAttachmentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 22, true);
			this.SaveOrAddAttachmentButton.TabIndex = 20;
			this.SaveOrAddAttachmentButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SaveOrAddAttachmentButton.ToolTipCaption = null;
			this.SaveOrAddAttachmentButton.Click += new System.EventHandler(this.SaveAddAttachmentButton_Click);
			//
			// SaveAllAttachmentsOrRemoveAttachmentButton
			//
			this.SaveAllAttachmentsOrRemoveAttachmentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveAllAttachmentsOrRemoveAttachmentButton.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|765d8686-5210-4238-a3cf-3c61d5e782aa", "Save &All Attachments");
			this.SaveAllAttachmentsOrRemoveAttachmentButton.IsCaptionOverridden = false;
			this.SaveAllAttachmentsOrRemoveAttachmentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 226, true);
			this.SaveAllAttachmentsOrRemoveAttachmentButton.Name = "SaveAllAttachmentsOrRemoveAttachmentButton";
			this.SaveAllAttachmentsOrRemoveAttachmentButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveAllAttachmentsOrRemoveAttachmentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 21, true);
			this.SaveAllAttachmentsOrRemoveAttachmentButton.TabIndex = 21;
			this.SaveAllAttachmentsOrRemoveAttachmentButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SaveAllAttachmentsOrRemoveAttachmentButton.ToolTipCaption = null;
			this.SaveAllAttachmentsOrRemoveAttachmentButton.Click += new System.EventHandler(this.SaveAllAttachmentsOrRemoveAttachmentButton_Click);
			//
			// PostingButtonsUserControl
			//
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 487, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 24;
			//
			// SaveEntireObjectButton
			//
			this.SaveEntireObjectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveEntireObjectButton.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|4345cb9a-0ac2-4eb2-95e0-580cec9e7f78", "Save whole email as .eml");
			this.SaveEntireObjectButton.IsCaptionOverridden = false;
			this.SaveEntireObjectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 251, true);
			this.SaveEntireObjectButton.Name = "SaveEntireObjectButton";
			this.SaveEntireObjectButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveEntireObjectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 21, true);
			this.SaveEntireObjectButton.TabIndex = 26;
			this.SaveEntireObjectButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SaveEntireObjectButton.ToolTipCaption = null;
			this.SaveEntireObjectButton.Click += new System.EventHandler(this.SaveEntireObjectButton_Click);
			//
			// ApplicationTextBox
			//
			this.BindingSource.SetBindingMember(this.ApplicationTextBox, "MI_Application");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailItem)(null)).MI_Application)));
			this.ApplicationTextBox.CaptionResourceString = ExternalMailManager.Res.GetData("7c26b475-544b-42ef-8c4f-b8355c240ff2", "Application");
			this.ApplicationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ApplicationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(616, 74, true);
			this.ApplicationTextBox.Name = "ApplicationTextBox";
			this.ApplicationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.ApplicationTextBox.TabIndex = 27;
			//
			// MailItemForm
			//
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemForm|4c8a6b55-eb66-44b7-ad6f-0ddf706ad62f", "Email");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 536, true);
			this.Controls.Add(this.ApplicationTextBox);
			this.Controls.Add(this.SaveEntireObjectButton);
			this.Controls.Add(this.SaveAllAttachmentsOrRemoveAttachmentButton);
			this.Controls.Add(this.SaveOrAddAttachmentButton);
			this.Controls.Add(this.HeaderGroupBox);
			this.Controls.Add(this.CreatedByTextBox);
			this.Controls.Add(this.CreatedDateEdit);
			this.Controls.Add(this.AttachmentsGroupBox);
			this.Controls.Add(this.SubjectTextBox);
			this.Controls.Add(this.RecipientsGroupBox);
			this.Controls.Add(this.ReceivedDateEdit);
			this.Controls.Add(this.SendDateEdit);
			this.Controls.Add(this.ReplyToTextBox);
			this.Controls.Add(this.FromTextBox);
			this.Controls.Add(this.StatusDropEdit);
			this.Controls.Add(this.DirectionDropEdit);
			this.Controls.Add(this.BodyGroupBox);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceAssemblyName = "MailManager";
			this.DataSourceType = typeof(Enterprise.MailManager.Business.MailItem);
			this.DataSourceTypeName = "Enterprise.MailManager.Business.MailItem";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 574, true);
			this.Name = "MailItemForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.BodyGroupBox, 0);
			this.Controls.SetChildIndex(this.DirectionDropEdit, 0);
			this.Controls.SetChildIndex(this.StatusDropEdit, 0);
			this.Controls.SetChildIndex(this.FromTextBox, 0);
			this.Controls.SetChildIndex(this.ReplyToTextBox, 0);
			this.Controls.SetChildIndex(this.SendDateEdit, 0);
			this.Controls.SetChildIndex(this.ReceivedDateEdit, 0);
			this.Controls.SetChildIndex(this.RecipientsGroupBox, 0);
			this.Controls.SetChildIndex(this.SubjectTextBox, 0);
			this.Controls.SetChildIndex(this.AttachmentsGroupBox, 0);
			this.Controls.SetChildIndex(this.CreatedDateEdit, 0);
			this.Controls.SetChildIndex(this.CreatedByTextBox, 0);
			this.Controls.SetChildIndex(this.HeaderGroupBox, 0);
			this.Controls.SetChildIndex(this.SaveOrAddAttachmentButton, 0);
			this.Controls.SetChildIndex(this.SaveAllAttachmentsOrRemoveAttachmentButton, 0);
			this.Controls.SetChildIndex(this.SaveEntireObjectButton, 0);
			this.Controls.SetChildIndex(this.ApplicationTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BodyGroupBox.ResumeLayout(false);
			this.BodyGroupBox.PerformLayout();
			this.DirectionDropEdit.ResumeLayout(true);
			this.DirectionDropEdit.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.SendDateEdit.ResumeLayout(true);
			this.SendDateEdit.PerformLayout();
			this.ReceivedDateEdit.ResumeLayout(true);
			this.ReceivedDateEdit.PerformLayout();
			this.RecipientsGroupBox.ResumeLayout(false);
			this.RecipientsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).EndInit();
			this.RecipientsGrid.ResumeLayout(false);
			this.RecipientsGrid.PerformLayout();
			this.AttachmentsGroupBox.ResumeLayout(false);
			this.AttachmentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttachmentsGrid)).EndInit();
			this.AttachmentsGrid.ResumeLayout(false);
			this.AttachmentsGrid.PerformLayout();
			this.CreatedDateEdit.ResumeLayout(true);
			this.CreatedDateEdit.PerformLayout();
			this.HeaderGroupBox.ResumeLayout(false);
			this.HeaderGroupBox.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		Enterprise.ZArchitecture.GUI.ZGroupBox BodyGroupBox;
		Enterprise.ZArchitecture.ZTextBox BodyTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit DirectionDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		Enterprise.ZArchitecture.ZTextBox FromTextBox;
		Enterprise.ZArchitecture.ZTextBox ReplyToTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit SendDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit ReceivedDateEdit;
		Enterprise.ZArchitecture.GUI.ZGroupBox RecipientsGroupBox;
		Enterprise.ZArchitecture.ZGrid RecipientsGrid;
		Enterprise.ZArchitecture.ZTextBox SubjectTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox AttachmentsGroupBox;
		internal Enterprise.ZArchitecture.ZGrid AttachmentsGrid;
		Enterprise.ZArchitecture.GUI.ZDateEdit CreatedDateEdit;
		Enterprise.ZArchitecture.ZTextBox CreatedByTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox HeaderGroupBox;
		Enterprise.ZArchitecture.ZTextBox HeaderTextBox;
		Enterprise.ZArchitecture.GUI.ZButton SaveOrAddAttachmentButton;
		Enterprise.ZArchitecture.GUI.ZButton SaveAllAttachmentsOrRemoveAttachmentButton;
		Enterprise.ZArchitecture.GUI.ZButton SaveEntireObjectButton;
		ZArchitecture.ZTextBox ApplicationTextBox;
	}
}

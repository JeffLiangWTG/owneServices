namespace Enterprise.EConversation.GUI
{
	partial class EConversationFullControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.broadcastButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MessagePanelSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.sendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddInternalCommentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.econversationMessageTextBox = new Enterprise.ZArchitecture.GUI.ZAutoCompleteTextBox();
			this.chatboxControl = new Enterprise.EConversation.GUI.EConversationMessageListUserControl();
			this.subscribersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.pictureBox2 = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.pictureBox1 = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.pictureBox3 = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.contactGrid = new Enterprise.ZArchitecture.ZGrid();
			this.groupGrid = new Enterprise.ZArchitecture.ZGrid();
			this.staffGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagePanelSplitContainer)).BeginInit();
			this.MessagePanelSplitContainer.Panel1.SuspendLayout();
			this.MessagePanelSplitContainer.Panel2.SuspendLayout();
			this.MessagePanelSplitContainer.SuspendLayout();
			this.chatboxControl.SuspendLayout();
			this.subscribersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.contactGrid)).BeginInit();
			this.contactGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.groupGrid)).BeginInit();
			this.groupGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.staffGrid)).BeginInit();
			this.staffGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.EConversation.Business.JobConversation);
			// 
			// broadcastButton
			// 
			this.broadcastButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.broadcastButton.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("a3c1be3e-fefe-4079-b796-fcfac5027d7c", "Broadcast");
			this.broadcastButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.broadcastButton.IsCaptionOverridden = false;
			this.broadcastButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(503, 120, true);
			this.broadcastButton.Name = "broadcastButton";
			this.broadcastButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.broadcastButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 42, true);
			this.broadcastButton.TabIndex = 4;
			this.broadcastButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.broadcastButton.ToolTipCaption = null;
			this.broadcastButton.UseVisualStyleBackColor = true;
			this.broadcastButton.Visible = false;
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.MessagePanelSplitContainer);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1147, 340, true);
			this.MainSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.subscribersGroupBox);
			this.MainSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(621);
			this.MainSplitContainer.TabIndex = 0;
			// 
			// MessagePanelSplitContainer
			// 
			this.MessagePanelSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagePanelSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagePanelSplitContainer.Name = "MessagePanelSplitContainer";
			this.MessagePanelSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MessagePanelSplitContainer.Panel1
			// 
			this.MessagePanelSplitContainer.Panel1.Controls.Add(this.sendButton);
			this.MessagePanelSplitContainer.Panel1.Controls.Add(this.AddInternalCommentButton);
			this.MessagePanelSplitContainer.Panel1.Controls.Add(this.broadcastButton);
			this.MessagePanelSplitContainer.Panel1.Controls.Add(this.econversationMessageTextBox);			
			this.MessagePanelSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 340, true);
			this.MessagePanelSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(160);
			// 
			// MessagePanelSplitContainer.Panel2
			// 
			this.MessagePanelSplitContainer.Panel2.Controls.Add(this.chatboxControl);
			this.MessagePanelSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
			this.MessagePanelSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(157);
			this.MessagePanelSplitContainer.TabIndex = 5;
			// 
			// sendButton
			// 
			this.sendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.sendButton.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("1d19c95f-9e56-4a0c-89bc-b7de4ead7b68", "Send");
			this.sendButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.sendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(503, 30, true);
			this.sendButton.Name = "sendButton";
			this.sendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.sendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 42, true);
			this.sendButton.TabIndex = 2;
			this.sendButton.ToolTipCaption = null;
			this.sendButton.UseVisualStyleBackColor = true;
			// 
			// AddInternalCommentButton
			// 
			this.AddInternalCommentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.AddInternalCommentButton.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("53996c67-8677-4c2a-aa27-fa5634099bd0", "Add Internal Comment");
			this.AddInternalCommentButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.AddInternalCommentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(503, 75, true);
			this.AddInternalCommentButton.Name = "AddInternalCommentButton";
			this.AddInternalCommentButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AddInternalCommentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 42, true);
			this.AddInternalCommentButton.TabIndex = 3;
			this.AddInternalCommentButton.ToolTipCaption = null;
			this.AddInternalCommentButton.UseVisualStyleBackColor = true;
			this.AddInternalCommentButton.Visible = false;
			// 
			// econversationMessageTextBox
			// 
			this.econversationMessageTextBox.AcceptsTab = true;
			this.econversationMessageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.econversationMessageTextBox.AutocompleteManager = null;
			this.econversationMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.econversationMessageTextBox.Name = "econversationMessageTextBox";
			this.econversationMessageTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.econversationMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 152, true);
			this.econversationMessageTextBox.TabIndex = 1;
			this.econversationMessageTextBox.Text = "";
			// 
			// chatboxControl
			// 
			this.chatboxControl.AllowDrop = true;
			this.chatboxControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.chatboxControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.EConversation.Business.IConversation)(((Enterprise.EConversation.Business.JobConversation)(null)))));
			this.chatboxControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.chatboxControl.Name = "chatboxControl";
			this.chatboxControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 173, true);
			this.chatboxControl.TabIndex = 4;
			// 
			// subscribersGroupBox
			// 
			this.subscribersGroupBox.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("385405f0-69fc-453b-b470-cac4d73c097c", "Participants");
			this.subscribersGroupBox.Controls.Add(this.zLabel3);
			this.subscribersGroupBox.Controls.Add(this.zLabel2);
			this.subscribersGroupBox.Controls.Add(this.zLabel1);
			this.subscribersGroupBox.Controls.Add(this.pictureBox2);
			this.subscribersGroupBox.Controls.Add(this.pictureBox1);
			this.subscribersGroupBox.Controls.Add(this.pictureBox3);
			this.subscribersGroupBox.Controls.Add(this.contactGrid);
			this.subscribersGroupBox.Controls.Add(this.groupGrid);
			this.subscribersGroupBox.Controls.Add(this.staffGrid);
			this.subscribersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.subscribersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.subscribersGroupBox.Name = "subscribersGroupBox";
			this.subscribersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 540, true);
			this.subscribersGroupBox.TabIndex = 12;
			this.subscribersGroupBox.TabStop = false;
			// 
			// zLabel3
			// 
			this.zLabel3.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("b194a6f6-1b7f-4da3-9bea-e58b618a3ead", "Groups");
			this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Small | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 243, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 40, true);
			this.zLabel3.TabIndex = 10;
			this.zLabel3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("a83a814e-97de-4448-9b43-961334532795", "Related Parties");
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Small | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 410, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 40, true);
			this.zLabel2.TabIndex = 9;
			this.zLabel2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("655470ed-02fc-4bf5-b57c-5aecee7bb9e6", "Staff");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Small | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 77, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 40, true);
			this.zLabel1.TabIndex = 8;
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// pictureBox2
			// 
			this.pictureBox2.ErrorImage = null;
			this.pictureBox2.Image = global::Enterprise.EConversation.GUI.Properties.Resources.staff_icon;
			this.pictureBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 42, true);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 32, true);
			this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox2.TabIndex = 7;
			this.pictureBox2.TabStop = false;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::Enterprise.EConversation.GUI.Properties.Resources.group_icon;
			this.pictureBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 208, true);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 32, true);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 6;
			this.pictureBox1.TabStop = false;
			// 
			// pictureBox3
			// 
			this.pictureBox3.Image = global::Enterprise.EConversation.GUI.Properties.Resources.group_icon;
			this.pictureBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 375, true);
			this.pictureBox3.Name = "pictureBox3";
			this.pictureBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 32, true);
			this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox3.TabIndex = 5;
			this.pictureBox3.TabStop = false;
			// 
			// contactGrid
			// 
			this.contactGrid.AllowNavigation = false;
			this.contactGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.contactGrid, "RelatedParties");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).RelatedParties)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).RelatedParties)).SyncRoot)).RelatedPartyTypeName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).RelatedParties)).SyncRoot)).ParentKey)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).RelatedParties)).SyncRoot)).Parent.Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).RelatedParties)).SyncRoot)).Parent.Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).RelatedParties)).SyncRoot)).Parent.OrganisationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).RelatedParties)).SyncRoot)).Parent.IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).RelatedParties)).SyncRoot)).JCP_IsSubscribed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).RelatedParties)).SyncRoot)).JCP_Relation)));
			this.contactGrid.CaptionVisible = false;
			this.contactGrid.ColorContextKey = "eConvoRelatedPartiesParticipant";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("04b557c2-8f00-47d3-8cff-d588a441e72b", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "RelatedPartyTypeName";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("082b39a1-58f2-4d86-996b-fa461316deaa", "Name");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ParentKey";
			zCodeFindBoxColumnStyleInfo1.ShowNewFormWhenEmpty = false;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("93f1d19f-1927-4c96-b82b-e9036d26d7dc", "Full Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Parent+Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("aa9acc16-10f2-44a4-9520-9bf40d072803", "Location");
			zTextBoxColumnStyleInfo2.ColumnName = "Parent+Location";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("e0be9f3e-bf41-46b6-ba60-1b5e2e829df2", "Organization");
			zTextBoxColumnStyleInfo3.ColumnName = "Parent+OrganisationName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("341d6761-5d61-4534-b186-04bcc423af0a", "E-Mail");
			zTextBoxColumnStyleInfo4.ColumnName = "EmailAddress";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("5ef93058-ee3f-4ed0-ac80-e732d3ca6384", "Active?");
			zCheckBoxColumnStyleInfo1.ColumnName = "Parent+IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("e3029e34-e3c2-473b-9b04-6a2495f46c06", "Subscribed?");
			zCheckBoxColumnStyleInfo2.ColumnName = "JCP_IsSubscribed";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("894aee86-3e84-4c90-8d01-f2f35d1a6143", "Relation");
			zTextBoxColumnStyleInfo10.ColumnName = "JCP_Relation";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.contactGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.contactGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.contactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.contactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.contactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.contactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.contactGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.contactGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.contactGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.contactGrid.GridId = "3e9f8c3b-c4c9-412e-809c-16aaeade1ae2";
			this.contactGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.contactGrid.LayoutKey = "zGrid1";
			this.contactGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 375, true);
			this.contactGrid.Name = "contactGrid";
			this.contactGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 161, true);
			this.contactGrid.TabIndex = 7;
			// 
			// groupGrid
			// 
			this.groupGrid.AllowNavigation = false;
			this.groupGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.groupGrid, "Groups");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).Groups)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).Groups)).SyncRoot)).ParentKey)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).Groups)).SyncRoot)).Parent.Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).Groups)).SyncRoot)).Parent.Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).Groups)).SyncRoot)).Parent.IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).Groups)).SyncRoot)).JCP_IsSubscribed)));
			this.groupGrid.CaptionVisible = false;
			this.groupGrid.ColorContextKey = "eConvoGroupParticipant";
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("ea6ce419-58d6-441e-9020-ff92cbf1a3f9", "Code");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ParentKey";
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbGroup;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("15f96f64-1e11-477a-9bfc-667034a60b27", "Name");
			zTextBoxColumnStyleInfo5.ColumnName = "Parent+Name";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("b2761ff5-3a46-49f0-a61a-e809cc8b6528", "Location");
			zTextBoxColumnStyleInfo6.ColumnName = "Parent+Location";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("821391b2-f830-433e-a592-12031fdd41a7", "Active?");
			zCheckBoxColumnStyleInfo3.ColumnName = "Parent+IsActive";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("a8032567-61be-4316-aeb7-c841551dcbb4", "Subscribed?");
			zCheckBoxColumnStyleInfo4.ColumnName = "JCP_IsSubscribed";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.groupGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.groupGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.groupGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.groupGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.groupGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.groupGrid.GridId = "3e9f8c3b-c4c9-412e-809c-16aaeade1ae2";
			this.groupGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.groupGrid.LayoutKey = "zGrid1";
			this.groupGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 208, true);
			this.groupGrid.Name = "groupGrid";
			this.groupGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 161, true);
			this.groupGrid.TabIndex = 6;
			// 
			// staffGrid
			// 
			this.staffGrid.AllowNavigation = false;
			this.staffGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.staffGrid, "Staff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).Staff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).Staff)).SyncRoot)).ParentKey)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).Staff)).SyncRoot)).Parent.Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).Staff)).SyncRoot)).Parent.Location)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).Staff)).SyncRoot)).Parent.JobTitle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).Staff)).SyncRoot)).Parent.IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.EConversation.Business.JobConversationParticipant)(((System.Collections.IList)(((Enterprise.EConversation.Business.JobConversation)(null)).Staff)).SyncRoot)).JCP_IsSubscribed)));
			this.staffGrid.CaptionVisible = false;
			this.staffGrid.ColorContextKey = "eConvoStaffParticipant";
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("f80bc6ab-c3f2-45aa-9d7c-9a2c743e052a", "Code");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "ParentKey";
			zCodeFindBoxColumnStyleInfo3.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("7816ce7c-8b8f-4d32-a968-f53f20be31c1", "Name");
			zTextBoxColumnStyleInfo7.ColumnName = "Parent+Name";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("2e9a04fb-29ef-4602-a43e-e860b6d11a9f", "Location");
			zTextBoxColumnStyleInfo8.ColumnName = "Parent+Location";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("b6b5381d-1c2a-41d7-abff-f47f66212d85", "Job Title");
			zTextBoxColumnStyleInfo9.ColumnName = "Parent+JobTitle";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("cc69daa7-0caa-45fd-8666-92aca8241e4d", "Active?");
			zCheckBoxColumnStyleInfo5.ColumnName = "Parent+IsActive";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.EConversation.GUI.Res.GetData("0ec6e1a7-3dff-47b4-acb0-e0b27a53a307", "Subscribed?");
			zCheckBoxColumnStyleInfo6.ColumnName = "JCP_IsSubscribed";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.staffGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.staffGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.staffGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.staffGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.staffGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.staffGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.staffGrid.GridId = "3e9f8c3b-c4c9-412e-809c-16aaeade1ae2";
			this.staffGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.staffGrid.LayoutKey = "staffGrid";
			this.staffGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 41, true);
			this.staffGrid.Name = "staffGrid";
			this.staffGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 161, true);
			this.staffGrid.TabIndex = 5;
			// 
			// EConversationFullControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainSplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 310, true);
			this.Name = "EConversationFullControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1147, 340, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.MessagePanelSplitContainer.Panel1.ResumeLayout(false);
			this.MessagePanelSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagePanelSplitContainer)).EndInit();
			this.MessagePanelSplitContainer.ResumeLayout(false);
			this.MessagePanelSplitContainer.PerformLayout();
			this.chatboxControl.ResumeLayout(true);
			this.chatboxControl.PerformLayout();
			this.subscribersGroupBox.ResumeLayout(false);
			this.subscribersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.contactGrid)).EndInit();
			this.contactGrid.ResumeLayout(false);
			this.contactGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.groupGrid)).EndInit();
			this.groupGrid.ResumeLayout(false);
			this.groupGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.staffGrid)).EndInit();
			this.staffGrid.ResumeLayout(false);
			this.staffGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		private EConversationMessageListUserControl chatboxControl;
		private ZArchitecture.GUI.ZButton AddInternalCommentButton;
		private ZArchitecture.GUI.ZButton sendButton;
		private ZArchitecture.GUI.ZAutoCompleteTextBox econversationMessageTextBox;
		private ZArchitecture.GUI.ZGroupBox subscribersGroupBox;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZGrid contactGrid;
		private ZArchitecture.ZGrid groupGrid;
		private ZArchitecture.ZGrid staffGrid;
		private Enterprise.ZArchitecture.GUI.ZPictureBox pictureBox3;
		private Enterprise.ZArchitecture.GUI.ZPictureBox pictureBox1;
		private Enterprise.ZArchitecture.GUI.ZPictureBox pictureBox2;
		private ZArchitecture.ZLabel zLabel3;
		private CargoWise.Windows.UI.KSplitContainer MessagePanelSplitContainer;
		private ZArchitecture.GUI.ZButton broadcastButton;
	}
}

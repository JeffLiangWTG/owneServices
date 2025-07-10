namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class MessagesUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MessagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessagesGrid = new Enterprise.Messaging.GUI.MessageZGrid();
			this.MessageTextAndInterpretedMessagePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InterpretedMessageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InterpretedMessageWebBrowser = new Enterprise.ZArchitecture.GUI.ZWebBrowser();
			this.InterpretedMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TextAndInterpreterSplitter = new CargoWise.Windows.UI.KSplitter();
			this.MessageTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessagesAndTextSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessagesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessagesGrid.SuspendLayout();
			this.MessageTextAndInterpretedMessagePanel.SuspendLayout();
			this.InterpretedMessageGroupBox.SuspendLayout();
			this.MessageTextGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesAndTextSplitContainer)).BeginInit();
			this.MessagesAndTextSplitContainer.Panel1.SuspendLayout();
			this.MessagesAndTextSplitContainer.Panel2.SuspendLayout();
			this.MessagesAndTextSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Messaging.Business.EDIMessageCollection);
			// 
			// MessagesGroupBox
			// 
			this.MessagesGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("696B8D4C-521A-4776-A10B-F0C798BE2809", "Messages");
			this.MessagesGroupBox.Controls.Add(this.MessagesGrid);
			this.MessagesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesGroupBox.Name = "MessagesGroupBox";
			this.MessagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1208, 182, true);
			this.MessagesGroupBox.TabIndex = 0;
			this.MessagesGroupBox.TabStop = false;
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Messaging.Business.EDIMessage)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_SendOrReceiveHumanReadable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_ApplicationReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_InterchangeStatus)));
			this.MessagesGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.Caption = "";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("68E9BB15-01A4-4B0D-93BD-31C10144A772", "Created");
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(91);
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("8DAF8D72-91A8-4854-BD81-3A4D2A3ACA8E", "User");
			zTextBoxColumnStyleInfo1.ColumnName = "EM_SystemCreateUser";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDateEditColumnStyleInfo2.Caption = "";
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("20C2D537-AC8E-440E-AD34-EF2D0AEF655A", "Created (UTC)");
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("2499F274-6D4D-48C8-853E-526F3F1B20A5", "Direction");
			zTextBoxColumnStyleInfo2.ColumnName = "EM_SendOrReceiveHumanReadable";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zTextBoxColumnStyleInfo3.Caption = "";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("7C419467-086E-4682-86E1-E9CD77E47C44", "Type", "Message Type", "");
			zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("86397717-1B86-4EF1-8842-04E0D941BBB4", "Sub Type", "Message Sub Type", "");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(69);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("7831C1C9-C33F-4916-A7AC-F5C94DDAEEAD", "App. Ref.", "App. Reference", "Application Reference", "");
			zTextBoxColumnStyleInfo5.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zTextBoxColumnStyleInfo6.Caption = "";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("44B84F94-86DF-4B4E-8C9F-20DA91E08182", "Message Number");
			zTextBoxColumnStyleInfo6.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(223);
			zTextBoxColumnStyleInfo7.Caption = "";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("D96DED9B-3A90-448B-B9FB-2BE2EF6B9634", "Message Status");
			zTextBoxColumnStyleInfo7.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zTextBoxColumnStyleInfo8.Caption = "";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("C7A50EA1-BAE2-4F5B-B3DE-D003D1721746", "Interchange Number");
			zTextBoxColumnStyleInfo8.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(124);
			zTextBoxColumnStyleInfo9.Caption = "";
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("FBD24456-40CE-45A0-8C61-F4392FD5AC9E", "Interchange Status");
			zTextBoxColumnStyleInfo9.ColumnName = "EM_InterchangeStatus";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.GridId = "01d3b37f-7081-4768-a501-85ec20a1aead";
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "MessagesGrid";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1202, 163, true);
			this.MessagesGrid.TabIndex = 0;
			// 
			// MessageTextAndInterpretedMessagePanel
			// 
			this.MessageTextAndInterpretedMessagePanel.Controls.Add(this.InterpretedMessageGroupBox);
			this.MessageTextAndInterpretedMessagePanel.Controls.Add(this.TextAndInterpreterSplitter);
			this.MessageTextAndInterpretedMessagePanel.Controls.Add(this.MessageTextGroupBox);
			this.MessageTextAndInterpretedMessagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextAndInterpretedMessagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageTextAndInterpretedMessagePanel.Name = "MessageTextAndInterpretedMessagePanel";
			this.MessageTextAndInterpretedMessagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1208, 376, true);
			this.MessageTextAndInterpretedMessagePanel.TabIndex = 9;
			// 
			// InterpretedMessageGroupBox
			// 
			this.InterpretedMessageGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("e6249df7-9589-43fe-bd1a-1eac4244d28c", "Interpreted Message");
			this.InterpretedMessageGroupBox.Controls.Add(this.InterpretedMessageWebBrowser);
			this.InterpretedMessageGroupBox.Controls.Add(this.InterpretedMessageTextBox);
			this.InterpretedMessageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InterpretedMessageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true);
			this.InterpretedMessageGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100, true);
			this.InterpretedMessageGroupBox.Name = "InterpretedMessageGroupBox";
			this.InterpretedMessageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(921, 376, true);
			this.InterpretedMessageGroupBox.TabIndex = 1;
			this.InterpretedMessageGroupBox.TabStop = false;
			// 
			// InterpretedMessageWebBrowser
			// 
			this.InterpretedMessageWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InterpretedMessageWebBrowser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InterpretedMessageWebBrowser.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.InterpretedMessageWebBrowser.Name = "InterpretedMessageWebBrowser";
			this.InterpretedMessageWebBrowser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 357, true);
			this.InterpretedMessageWebBrowser.TabIndex = 0;
			this.InterpretedMessageWebBrowser.Url = new System.Uri("about:blank", System.UriKind.Absolute);
			// 
			// InterpretedMessageTextBox
			// 
			this.BindingSource.SetBindingMember(this.InterpretedMessageTextBox, "EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageInterpretation)));
			this.InterpretedMessageTextBox.CaptionResourceString = null;
			this.InterpretedMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.InterpretedMessageTextBox.Dock = System.Windows.Forms.DockStyle.None;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InterpretedMessageTextBox, false);
			this.InterpretedMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 100, true);
			this.InterpretedMessageTextBox.Multiline = true;
			this.InterpretedMessageTextBox.Name = "InterpretedMessageTextBox";
			this.InterpretedMessageTextBox.ReadOnly = true;
			this.InterpretedMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.InterpretedMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.InterpretedMessageTextBox.TabIndex = 1;
			// 
			// TextAndInterpreterSplitter
			// 
			this.TextAndInterpreterSplitter.DoNotSaveSplitterLayout = false;
			this.TextAndInterpreterSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 0, true);
			this.TextAndInterpreterSplitter.Name = "TextAndInterpreterSplitter";
			this.TextAndInterpreterSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 376, true);
			this.TextAndInterpreterSplitter.TabIndex = 0;
			this.TextAndInterpreterSplitter.TabStop = false;
			// 
			// MessageTextGroupBox
			// 
			this.MessageTextGroupBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("3F88F0F8-C996-44FD-8F4A-8CE968D9FFA1", "Message Text");
			this.MessageTextGroupBox.Controls.Add(this.MessageTextTextBox);
			this.MessageTextGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.MessageTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageTextGroupBox.Name = "MessageTextGroupBox";
			this.MessageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 376, true);
			this.MessageTextGroupBox.TabIndex = 0;
			this.MessageTextGroupBox.TabStop = false;
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "HumanReadableMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).HumanReadableMessage)));
			this.MessageTextTextBox.CaptionResourceString = null;
			this.MessageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageTextTextBox, false);
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ReadOnly = true;
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 357, true);
			this.MessageTextTextBox.TabIndex = 0;
			// 
			// MessagesAndTextSplitContainer
			// 
			this.MessagesAndTextSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesAndTextSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesAndTextSplitContainer.Name = "MessagesAndTextSplitContainer";
			this.MessagesAndTextSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MessagesAndTextSplitContainer.Panel1
			// 
			this.MessagesAndTextSplitContainer.Panel1.Controls.Add(this.MessagesGroupBox);
			// 
			// MessagesAndTextSplitContainer.Panel2
			// 
			this.MessagesAndTextSplitContainer.Panel2.Controls.Add(this.MessageTextAndInterpretedMessagePanel);
			this.MessagesAndTextSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1208, 566, true);
			this.MessagesAndTextSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(182);
			this.MessagesAndTextSplitContainer.SplitterWidth = 8;
			this.MessagesAndTextSplitContainer.TabIndex = 0;
			// 
			// MessagesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessagesAndTextSplitContainer);
			this.Name = "MessagesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1208, 566, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagesGroupBox.ResumeLayout(false);
			this.MessagesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessagesGrid.ResumeLayout(false);
			this.MessagesGrid.PerformLayout();
			this.MessageTextAndInterpretedMessagePanel.ResumeLayout(false);
			this.MessageTextAndInterpretedMessagePanel.PerformLayout();
			this.InterpretedMessageGroupBox.ResumeLayout(false);
			this.InterpretedMessageGroupBox.PerformLayout();
			this.MessageTextGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.PerformLayout();
			this.MessagesAndTextSplitContainer.Panel1.ResumeLayout(false);
			this.MessagesAndTextSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesAndTextSplitContainer)).EndInit();
			this.MessagesAndTextSplitContainer.ResumeLayout(false);
			this.MessagesAndTextSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MessagesGroupBox;
		private ZArchitecture.GUI.ZPanel MessageTextAndInterpretedMessagePanel;
		private ZArchitecture.GUI.ZGroupBox InterpretedMessageGroupBox;
		internal ZArchitecture.ZTextBox InterpretedMessageTextBox;
		private CargoWise.Windows.UI.KSplitter TextAndInterpreterSplitter;
		private ZArchitecture.GUI.ZGroupBox MessageTextGroupBox;
		private ZArchitecture.ZTextBox MessageTextTextBox;
		private CargoWise.Windows.UI.KSplitContainer MessagesAndTextSplitContainer;
		protected internal Messaging.GUI.MessageZGrid MessagesGrid;
		internal ZArchitecture.GUI.ZWebBrowser InterpretedMessageWebBrowser;
	}
}

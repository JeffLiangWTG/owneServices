namespace Enterprise.Customs.DE.GUI
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
			this.MessagesGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("5e2138c1-9a33-415c-9b6f-17d4040ce888", "Messages");
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
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("5632ea51-cff0-4321-9e90-89a0852a3781", "Created");
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(91);
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("6ebe441a-0122-4aad-8a07-3085cb8d8af4", "User");
			zTextBoxColumnStyleInfo1.ColumnName = "EM_SystemCreateUser";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDateEditColumnStyleInfo2.Caption = "";
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("7a236746-3ed4-4419-90c0-5297f7122431", "Created (UTC)");
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("91743ea6-6e2e-4302-9376-1ba541d01433", "Direction");
			zTextBoxColumnStyleInfo2.ColumnName = "EM_SendOrReceiveHumanReadable";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zTextBoxColumnStyleInfo3.Caption = "";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("d9acebb0-2cbb-401a-8cec-0699140c5b4f", "Type", "Message Type", "");
			zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("46d975ae-25d8-40c9-824f-85c776939413", "Sub Type", "Message Sub Type", "");
			zTextBoxColumnStyleInfo4.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(69);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("485f4e2d-6151-4a2d-84e2-0626ee7b144d", "App. Ref.", "App. Reference", "Application Reference", "");
			zTextBoxColumnStyleInfo5.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zTextBoxColumnStyleInfo6.Caption = "";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("ebcd5f87-a4a3-413e-bee8-23b514ad35af", "Message Number");
			zTextBoxColumnStyleInfo6.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(223);
			zTextBoxColumnStyleInfo7.Caption = "";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("126b7813-b876-46ad-b4c9-4dde7bf487a4", "Message Status");
			zTextBoxColumnStyleInfo7.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			zTextBoxColumnStyleInfo8.Caption = "";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("8b24c3d1-0da2-433e-80d0-66f133c6aeda", "Interchange Number");
			zTextBoxColumnStyleInfo8.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(124);
			zTextBoxColumnStyleInfo9.Caption = "";
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("957ff89c-b269-4cb0-81bd-0fd6a5d77e24", "Interchange Status");
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
			this.MessageTextAndInterpretedMessagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1208, 380, true);
			this.MessageTextAndInterpretedMessagePanel.TabIndex = 9;
			// 
			// InterpretedMessageGroupBox
			//
			this.InterpretedMessageGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("181fdd04-5b7e-4470-952a-25dd596ac323", "Interpreted Message");
			this.InterpretedMessageGroupBox.Controls.Add(this.InterpretedMessageTextBox);
			this.InterpretedMessageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InterpretedMessageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true);
			this.InterpretedMessageGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100, true);
			this.InterpretedMessageGroupBox.Name = "InterpretedMessageGroupBox";
			this.InterpretedMessageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(921, 380, true);
			this.InterpretedMessageGroupBox.TabIndex = 1;
			this.InterpretedMessageGroupBox.TabStop = false;
			// 
			// InterpretedMessageTextBox
			// 
			this.BindingSource.SetBindingMember(this.InterpretedMessageTextBox, "EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageInterpretation)));
			this.InterpretedMessageTextBox.CaptionResourceString = null;
			this.InterpretedMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.InterpretedMessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InterpretedMessageTextBox, false);
			this.InterpretedMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InterpretedMessageTextBox.Multiline = true;
			this.InterpretedMessageTextBox.Name = "InterpretedMessageTextBox";
			this.InterpretedMessageTextBox.ReadOnly = true;
			this.InterpretedMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.InterpretedMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 361, true);
			this.InterpretedMessageTextBox.TabIndex = 0;
			// 
			// TextAndInterpreterSplitter
			// 
			this.TextAndInterpreterSplitter.DoNotSaveSplitterLayout = false;
			this.TextAndInterpreterSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 0, true);
			this.TextAndInterpreterSplitter.Name = "TextAndInterpreterSplitter";
			this.TextAndInterpreterSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 380, true);
			this.TextAndInterpreterSplitter.TabIndex = 0;
			this.TextAndInterpreterSplitter.TabStop = false;
			// 
			// MessageTextGroupBox
			// 
			this.MessageTextGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("40124111-a3b6-40cb-b643-f778c8126260", "Message Text");
			this.MessageTextGroupBox.Controls.Add(this.MessageTextTextBox);
			this.MessageTextGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.MessageTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageTextGroupBox.Name = "MessageTextGroupBox";
			this.MessageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 380, true);
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
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 361, true);
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
		private ZArchitecture.ZTextBox InterpretedMessageTextBox;
		private CargoWise.Windows.UI.KSplitter TextAndInterpreterSplitter;
		private ZArchitecture.GUI.ZGroupBox MessageTextGroupBox;
		private ZArchitecture.ZTextBox MessageTextTextBox;
		protected Enterprise.Messaging.GUI.MessageZGrid MessagesGrid;
		private CargoWise.Windows.UI.KSplitContainer MessagesAndTextSplitContainer;
	}
}

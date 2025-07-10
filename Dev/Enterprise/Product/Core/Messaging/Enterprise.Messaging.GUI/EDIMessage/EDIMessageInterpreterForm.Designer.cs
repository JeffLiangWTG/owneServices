namespace Enterprise.Messaging.GUI
{
	partial class EDIMessageInterpreterForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MessageSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MessageTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ApplicationReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InterpretButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReceiveTransmitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageSubTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ApplicationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageInterpretationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageInterpretationTextBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageSplitContainer)).BeginInit();
			this.MessageSplitContainer.Panel1.SuspendLayout();
			this.MessageSplitContainer.Panel2.SuspendLayout();
			this.MessageSplitContainer.SuspendLayout();
			this.MessageTextGroupBox.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.ReceiveTransmitDropEdit.SuspendLayout();
			this.ApplicationCodeDropEdit.SuspendLayout();
			this.MessageInterpretationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 454, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 24, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Messaging.Business.EDIMessageInterpreter);
			// 
			// MessageSplitContainer
			// 
			this.MessageSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageSplitContainer.Name = "MessageSplitContainer";
			this.MessageSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MessageSplitContainer.Panel1
			// 
			this.MessageSplitContainer.Panel1.Controls.Add(this.MessageTextGroupBox);
			this.MessageSplitContainer.Panel1.Controls.Add(this.TopPanel);
			this.MessageSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 454, true);
			this.MessageSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(230);
			// 
			// MessageSplitContainer.Panel2
			// 
			this.MessageSplitContainer.Panel2.Controls.Add(this.MessageInterpretationGroupBox);
			this.MessageSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(230);
			this.MessageSplitContainer.TabIndex = 0;
			// 
			// MessageTextGroupBox
			// 
			this.MessageTextGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("dd9ed861-996f-4f24-a0bc-e45ac52a5d10", "Message Text");
			this.MessageTextGroupBox.Controls.Add(this.MessageTextTextBox);
			this.MessageTextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(291, 0, true);
			this.MessageTextGroupBox.Name = "MessageTextGroupBox";
			this.MessageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 230, true);
			this.MessageTextGroupBox.TabIndex = 1;
			this.MessageTextGroupBox.TabStop = false;
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "MessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessageInterpreter)(null)).MessageText)));
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 211, true);
			this.MessageTextTextBox.TabIndex = 0;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.ApplicationReferenceTextBox);
			this.TopPanel.Controls.Add(this.InterpretButton);
			this.TopPanel.Controls.Add(this.ReceiveTransmitDropEdit);
			this.TopPanel.Controls.Add(this.MessageSubTypeTextBox);
			this.TopPanel.Controls.Add(this.MessageTypeTextBox);
			this.TopPanel.Controls.Add(this.ApplicationCodeDropEdit);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 230, true);
			this.TopPanel.TabIndex = 0;
			// 
			// ApplicationReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ApplicationReferenceTextBox, "ApplicationReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessageInterpreter)(null)).ApplicationReference)));
			this.ApplicationReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 38, true);
			this.ApplicationReferenceTextBox.Name = "ApplicationReferenceTextBox";
			this.ApplicationReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.ApplicationReferenceTextBox.TabIndex = 1;
			// 
			// InterpretButton
			// 
			this.InterpretButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.InterpretButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("736eb57b-1fa6-4a15-a21e-f1667039a7a6", "&Interpret Message");
			this.InterpretButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 195, true);
			this.InterpretButton.Name = "InterpretButton";
			this.InterpretButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.InterpretButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 23, true);
			this.InterpretButton.TabIndex = 5;
			this.InterpretButton.UseVisualStyleBackColor = true;
			this.InterpretButton.Click += new System.EventHandler(this.InterpretButton_Click);
			// 
			// ReceiveTransmitDropEdit
			// 
			this.ReceiveTransmitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceiveTransmitDropEdit, "ReceiveTransmit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Messaging.Business.EDIMessageInterpreter)(null)).ReceiveTransmit)));
			this.ReceiveTransmitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 116, true);
			this.ReceiveTransmitDropEdit.Name = "ReceiveTransmitDropEdit";
			this.ReceiveTransmitDropEdit.PreBoundMaxLength = 3;
			this.ReceiveTransmitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.ReceiveTransmitDropEdit.TabIndex = 4;
			// 
			// MessageSubTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageSubTypeTextBox, "MessageSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessageInterpreter)(null)).MessageSubType)));
			this.MessageSubTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 90, true);
			this.MessageSubTypeTextBox.Name = "MessageSubTypeTextBox";
			this.MessageSubTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MessageSubTypeTextBox.TabIndex = 3;
			// 
			// MessageTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTypeTextBox, "MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessageInterpreter)(null)).MessageType)));
			this.MessageTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 64, true);
			this.MessageTypeTextBox.Name = "MessageTypeTextBox";
			this.MessageTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MessageTypeTextBox.TabIndex = 2;
			// 
			// ApplicationCodeDropEdit
			// 
			this.ApplicationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApplicationCodeDropEdit, "ApplicationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Messaging.Business.EDIMessageInterpreter)(null)).ApplicationCode)));
			this.ApplicationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 12, true);
			this.ApplicationCodeDropEdit.Name = "ApplicationCodeDropEdit";
			this.ApplicationCodeDropEdit.PreBoundMaxLength = 3;
			this.ApplicationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			this.ApplicationCodeDropEdit.TabIndex = 0;
			// 
			// MessageInterpretationGroupBox
			// 
			this.MessageInterpretationGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("901cd373-c84c-4cc8-8816-a7ce07029f6a", "Message Interpretation");
			this.MessageInterpretationGroupBox.Controls.Add(this.MessageInterpretationTextBox);
			this.MessageInterpretationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageInterpretationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageInterpretationGroupBox.Name = "MessageInterpretationGroupBox";
			this.MessageInterpretationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 220, true);
			this.MessageInterpretationGroupBox.TabIndex = 0;
			this.MessageInterpretationGroupBox.TabStop = false;
			// 
			// MessageInterpretationTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageInterpretationTextBox, "MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessageInterpreter)(null)).MessageInterpretation)));
			this.MessageInterpretationTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageInterpretationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageInterpretationTextBox.Name = "MessageInterpretationTextBox";
			this.MessageInterpretationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 201, true);
			this.MessageInterpretationTextBox.TabIndex = 0;
			// 
			// EDIMessageInterpreterForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("d784855c-acd7-41a6-90aa-64c0d4d448f2", "Message Interpretation");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 478, true);
			this.Controls.Add(this.MessageSplitContainer);
			this.DataSourceType = typeof(Enterprise.Messaging.Business.EDIMessageInterpreter);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(713, 517, true);
			this.Name = "EDIMessageInterpreterForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MessageSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageSplitContainer.Panel1.ResumeLayout(false);
			this.MessageSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageSplitContainer)).EndInit();
			this.MessageSplitContainer.ResumeLayout(false);
			this.MessageSplitContainer.PerformLayout();
			this.MessageTextGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.ReceiveTransmitDropEdit.ResumeLayout(true);
			this.ReceiveTransmitDropEdit.PerformLayout();
			this.ApplicationCodeDropEdit.ResumeLayout(true);
			this.ApplicationCodeDropEdit.PerformLayout();
			this.MessageInterpretationGroupBox.ResumeLayout(false);
			this.MessageInterpretationGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private CargoWise.Windows.UI.KSplitContainer MessageSplitContainer;
		private ZArchitecture.GUI.ZGroupBox MessageTextGroupBox;
		private ZArchitecture.ZTextBox MessageTextTextBox;
		private ZArchitecture.GUI.ZPanel TopPanel;
		private ZArchitecture.GUI.ZButton InterpretButton;
		private ZArchitecture.GUI.ZDropEdit ReceiveTransmitDropEdit;
		private ZArchitecture.ZTextBox MessageSubTypeTextBox;
		private ZArchitecture.ZTextBox MessageTypeTextBox;
		private ZArchitecture.GUI.ZDropEdit ApplicationCodeDropEdit;
		private ZArchitecture.GUI.ZGroupBox MessageInterpretationGroupBox;
		private HtmlInterpretationBox MessageInterpretationTextBox;
		private ZArchitecture.ZTextBox ApplicationReferenceTextBox;
	}
}

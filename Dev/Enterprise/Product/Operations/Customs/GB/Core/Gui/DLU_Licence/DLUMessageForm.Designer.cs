using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.DLU
{
	partial class DLUMessageForm
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
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.PimaTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageOwnerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zWebBrowser1 = new Enterprise.ZArchitecture.GUI.ZWebBrowser();
			this.DirectionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrgCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContrlTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zWebBrowser2 = new Enterprise.ZArchitecture.GUI.ZWebBrowser();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 535, true);
			this.MainTabControl.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
			// 
			// MainTabPage
			//
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("5efb729d-7ccd-41e8-b4c3-dd563639a647", "Message");
			this.MainTabPage.Controls.Add(this.ContrlTextBox);
			this.MainTabPage.Controls.Add(this.zWebBrowser2);
			this.MainTabPage.Controls.Add(this.OrgCodeTextBox);
			this.MainTabPage.Controls.Add(this.StatusTextBox);
			this.MainTabPage.Controls.Add(this.DirectionTextBox);
			this.MainTabPage.Controls.Add(this.zWebBrowser1);
			this.MainTabPage.Controls.Add(this.MessageTypeTextBox);
			this.MainTabPage.Controls.Add(this.MessageOwnerTextBox);
			this.MainTabPage.Controls.Add(this.PimaTextBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 514, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 514, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(833, 514, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 535, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Chief.Messaging.DLU.DLUMessage);
			// 
			// PimaTextBox
			// 
			this.BindingSource.SetBindingMember(this.PimaTextBox, "EM_ApplicationReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.Messaging.DLU.DLUMessage)(null)).EM_ApplicationReference)));
			this.PimaTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("DLUMessageForm|901ef731-5545-4e88-be92-066435328fd6", "License Number");
			this.PimaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 17, true);
			this.PimaTextBox.Name = "PimaTextBox";
			this.PimaTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.PimaTextBox.TabIndex = 0;
			// 
			// MessageOwnerTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageOwnerTextBox, "EM_MessageOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.Messaging.DLU.DLUMessage)(null)).EM_MessageOwner)));
			this.MessageOwnerTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("DLUMessageForm|1ac81a1a-d4a8-482a-9df9-e758b9898bbb", "Badge");
			this.MessageOwnerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 43, true);
			this.MessageOwnerTextBox.Name = "MessageOwnerTextBox";
			this.MessageOwnerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 17, true);
			this.MessageOwnerTextBox.TabIndex = 4;
			// 
			// MessageTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTypeTextBox, "EM_MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.Messaging.DLU.DLUMessage)(null)).EM_MessageType)));
			this.MessageTypeTextBox.CaptionResourceString = null;
			this.MessageTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 43, true);
			this.MessageTypeTextBox.Name = "MessageTypeTextBox";
			this.MessageTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
			this.MessageTypeTextBox.TabIndex = 3;
			// 
			// zWebBrowser1
			// 
			this.zWebBrowser1.AllowWebBrowserDrop = false;
			this.zWebBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.zWebBrowser1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 80, true);
			this.zWebBrowser1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.zWebBrowser1.Name = "zWebBrowser1";
			this.zWebBrowser1.ScriptErrorsSuppressed = true;
			this.zWebBrowser1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 405, true);
			this.zWebBrowser1.TabIndex = 6;
			this.zWebBrowser1.Url = new System.Uri("about:blank", System.UriKind.Absolute);
			// 
			// DirectionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DirectionTextBox, "EM_ReceiveTransmit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.Messaging.DLU.DLUMessage)(null)).EM_ReceiveTransmit)));
			this.DirectionTextBox.CaptionResourceString = null;
			this.DirectionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(327, 17, true);
			this.DirectionTextBox.Name = "DirectionTextBox";
			this.DirectionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 17, true);
			this.DirectionTextBox.TabIndex = 1;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "EM_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.Messaging.DLU.DLUMessage)(null)).EM_Status)));
			this.StatusTextBox.CaptionResourceString = null;
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 17, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 17, true);
			this.StatusTextBox.TabIndex = 2;
			// 
			// OrgCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.OrgCodeTextBox, "OrgCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.Messaging.DLU.DLUMessage)(null)).OrgCode)));
			this.OrgCodeTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("DLUMessageForm|a0fb6faa-4d7c-4568-b841-360be384e520", "Organization");
			this.OrgCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 42, true);
			this.OrgCodeTextBox.Name = "OrgCodeTextBox";
			this.OrgCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.OrgCodeTextBox.TabIndex = 7;
			// 
			// ContrlTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContrlTextBox, "LinkedMessage.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Chief.Messaging.DLU.DLUMessage)(null)).LinkedMessage.EM_MessageInterpretation)));
			this.ContrlTextBox.CaptionResourceString = null;
			this.ContrlTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(596, 17, true);
			this.ContrlTextBox.Multiline = true;
			this.ContrlTextBox.Name = "ContrlTextBox";
			this.ContrlTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ContrlTextBox.TabIndex = 10;
			this.ContrlTextBox.Visible = false;
			// 
			// zWebBrowser2
			// 
			this.zWebBrowser2.AllowWebBrowserDrop = false;
			this.zWebBrowser2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zWebBrowser2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(307, 80, true);
			this.zWebBrowser2.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.zWebBrowser2.Name = "zWebBrowser2";
			this.zWebBrowser2.ScriptErrorsSuppressed = true;
			this.zWebBrowser2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 405, true);
			this.zWebBrowser2.TabIndex = 9;
			this.zWebBrowser2.Url = new System.Uri("about:blank", System.UriKind.Absolute);
			// 
			// DLUMessageForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("DLUMessageForm|948e8d12-7563-40a8-a0c3-6c7990a9824c", "Display License Usage");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 591, true);
			this.DataSourceType = typeof(Enterprise.Customs.GB.Chief.Messaging.DLU.DLUMessage);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(851, 628, true);
			this.Name = "DLUMessageForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox MessageTypeTextBox;
		private ZArchitecture.ZTextBox MessageOwnerTextBox;
		private ZArchitecture.ZTextBox PimaTextBox;
		private ZWebBrowser zWebBrowser1;
		private ZArchitecture.ZTextBox StatusTextBox;
		private ZArchitecture.ZTextBox DirectionTextBox;
		private ZArchitecture.ZTextBox OrgCodeTextBox;
		private ZArchitecture.ZTextBox ContrlTextBox;
		private ZWebBrowser zWebBrowser2;
	}
}

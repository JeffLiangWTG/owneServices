using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class CcsukGenralMessageForm
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
		protected override void InitializeComponent()
		{
			this.PimaTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageSubTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zWebBrowser1 = new Enterprise.ZArchitecture.GUI.ZWebBrowser();
			this.DirectionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContrlTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContrlTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zWebBrowser2 = new Enterprise.ZArchitecture.GUI.ZWebBrowser();
			this.SendingBadgeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.zTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zTabPage2 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContrlTab.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			this.zTabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 210, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ContrlTab);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 437, true);
			this.MainTabControl.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ContrlTab, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			//
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("6bd3384d-30ba-48f5-a70a-50b7b23e28ec", "Original Message");
			this.MainTabPage.Controls.Add(this.zTabControl1);
			this.MainTabPage.Controls.Add(this.zGroupBox1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 410, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.GenralEdiMessage);
			// 
			// PimaTextBox
			// 
			this.BindingSource.SetBindingMember(this.PimaTextBox, "EM_ApplicationReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.GenralEdiMessage)(null)).EM_ApplicationReference)));
			this.PimaTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukGenralMessageForm|901ef731-5545-4e88-be92-066435328fd6", "PIMA of other party");
			this.PimaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 13, true);
			this.PimaTextBox.Name = "PimaTextBox";
			this.PimaTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.PimaTextBox.TabIndex = 0;
			// 
			// MessageSubTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageSubTypeTextBox, "EM_MessageSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.GenralEdiMessage)(null)).EM_MessageSubType)));
			this.MessageSubTypeTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukGenralMessageForm|1ac81a1a-d4a8-482a-9df9-e758b9898bbb", "Sub-Type");
			this.MessageSubTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 39, true);
			this.MessageSubTypeTextBox.Name = "MessageSubTypeTextBox";
			this.MessageSubTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.MessageSubTypeTextBox.TabIndex = 5;
			// 
			// MessageTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTypeTextBox, "EM_MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.GenralEdiMessage)(null)).EM_MessageType)));
			this.MessageTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 39, true);
			this.MessageTypeTextBox.Name = "MessageTypeTextBox";
			this.MessageTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.MessageTypeTextBox.TabIndex = 4;
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "EM_MessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.GenralEdiMessage)(null)).EM_MessageText)));
			this.MessageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 264, true);
			this.MessageTextTextBox.TabIndex = 6;
			this.MessageTextTextBox.TextChanged += new System.EventHandler(this.MessageTextTextBox_TextChanged);
			// 
			// zWebBrowser1
			// 
			this.zWebBrowser1.AllowWebBrowserDrop = false;
			this.zWebBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zWebBrowser1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zWebBrowser1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.zWebBrowser1.Name = "zWebBrowser1";
			this.zWebBrowser1.ScriptErrorsSuppressed = true;
			this.zWebBrowser1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 292, true);
			this.zWebBrowser1.TabIndex = 7;
			this.zWebBrowser1.Url = new System.Uri("about:blank", System.UriKind.Absolute);
			// 
			// DirectionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DirectionTextBox, "EM_ReceiveTransmit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.GenralEdiMessage)(null)).EM_ReceiveTransmit)));
			this.DirectionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 13, true);
			this.DirectionTextBox.Name = "DirectionTextBox";
			this.DirectionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.DirectionTextBox.TabIndex = 1;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "EM_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.GenralEdiMessage)(null)).EM_Status)));
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 13, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.StatusTextBox.TabIndex = 2;
			// 
			// ContrlTab
			//
			this.ContrlTab.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("d0ebe03f-a7e1-4bd4-bd91-21c9a04a2634", "CONTRL");
			this.ContrlTab.Controls.Add(this.ContrlTextBox);
			this.ContrlTab.Controls.Add(this.zWebBrowser2);
			this.ContrlTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContrlTab.Name = "ContrlTab";
			this.ContrlTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ContrlTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 210, true);
			this.ContrlTab.TabIndex = 3;
			this.ContrlTab.UseVisualStyleBackColor = true;
			// 
			// ContrlTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContrlTextBox, "LinkedMessage.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.GenralEdiMessage)(null)).LinkedMessage.EM_MessageInterpretation)));
			this.ContrlTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 28, true);
			this.ContrlTextBox.Multiline = true;
			this.ContrlTextBox.Name = "ContrlTextBox";
			this.ContrlTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ContrlTextBox.TabIndex = 8;
			this.ContrlTextBox.Visible = false;
			this.ContrlTextBox.TextChanged += new System.EventHandler(this.ContrlTextBox_TextChanged);
			// 
			// zWebBrowser2
			// 
			this.zWebBrowser2.AllowWebBrowserDrop = false;
			this.zWebBrowser2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zWebBrowser2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zWebBrowser2.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.zWebBrowser2.Name = "zWebBrowser2";
			this.zWebBrowser2.ScriptErrorsSuppressed = true;
			this.zWebBrowser2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(774, 437, true);
			this.zWebBrowser2.TabIndex = 7;
			this.zWebBrowser2.Url = new System.Uri("about:blank", System.UriKind.Absolute);
			// 
			// SendingBadgeTextBox
			// 
			this.BindingSource.SetBindingMember(this.SendingBadgeTextBox, "EM_MessageOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.GenralEdiMessage)(null)).EM_MessageOwner)));
			this.SendingBadgeTextBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukGenralMessageForm|9caf5a9d-b08a-476e-a1db-e14d3d8bbb98", "Local Profile");
			this.SendingBadgeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 39, true);
			this.SendingBadgeTextBox.Name = "SendingBadgeTextBox";
			this.SendingBadgeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
			this.SendingBadgeTextBox.TabIndex = 3;
			// 
			// zGroupBox1
			//
			this.zGroupBox1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("fde6c186-b5a1-4326-9cf6-615758757a1b", "Details");
			this.zGroupBox1.Controls.Add(this.SendingBadgeTextBox);
			this.zGroupBox1.Controls.Add(this.PimaTextBox);
			this.zGroupBox1.Controls.Add(this.StatusTextBox);
			this.zGroupBox1.Controls.Add(this.MessageSubTypeTextBox);
			this.zGroupBox1.Controls.Add(this.DirectionTextBox);
			this.zGroupBox1.Controls.Add(this.MessageTypeTextBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 72, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Controls.Add(this.zTabPage1);
			this.zTabControl1.Controls.Add(this.zTabPage2);
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 83, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 325, true);
			this.zTabControl1.TabIndex = 9;
			// 
			// zTabPage1
			//
			this.zTabPage1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("38944e78-f4bb-43c0-bf81-ee95d664b912", "Interpretation");
			this.zTabPage1.Controls.Add(this.zWebBrowser1);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(653, 298, true);
			this.zTabPage1.TabIndex = 0;
			this.zTabPage1.UseVisualStyleBackColor = true;
			// 
			// zTabPage2
			//
			this.zTabPage2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("3be31255-231e-4672-af96-d3886a523f27", "Text");
			this.zTabPage2.Controls.Add(this.MessageTextTextBox);
			this.zTabPage2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage2.Name = "zTabPage2";
			this.zTabPage2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 270, true);
			this.zTabPage2.TabIndex = 1;
			this.zTabPage2.UseVisualStyleBackColor = true;
			// 
			// CcsukGenralMessageForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CcsukGenralMessageForm|948e8d12-7563-40a8-a0c3-6c7990a9824c", "CCS-UK GENRAL Message");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 493, true);
			this.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral.GenralEdiMessage);
			this.IsPostOnly = true;
			this.Name = "CcsukGenralMessageForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContrlTab.ResumeLayout(false);
			this.ContrlTab.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zTabControl1.ResumeLayout(false);
			this.zTabPage1.ResumeLayout(false);
			this.zTabPage2.ResumeLayout(false);
			this.zTabPage2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZTextBox MessageTypeTextBox;
		private ZArchitecture.ZTextBox MessageSubTypeTextBox;
		private ZArchitecture.ZTextBox PimaTextBox;
		private ZWebBrowser zWebBrowser1;
		private ZArchitecture.ZTextBox MessageTextTextBox;
		private ZArchitecture.ZTextBox StatusTextBox;
		private ZArchitecture.ZTextBox DirectionTextBox;
		private ZTabPage ContrlTab;
		private ZArchitecture.ZTextBox ContrlTextBox;
		private ZWebBrowser zWebBrowser2;
		private ZArchitecture.ZTextBox SendingBadgeTextBox;
		private ZTabControl zTabControl1;
		private ZTabPage zTabPage1;
		private ZTabPage zTabPage2;
		private ZGroupBox zGroupBox1;
	}
}

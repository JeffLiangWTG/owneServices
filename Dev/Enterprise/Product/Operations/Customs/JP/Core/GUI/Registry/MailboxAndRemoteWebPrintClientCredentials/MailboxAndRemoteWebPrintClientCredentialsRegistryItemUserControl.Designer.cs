using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class MailboxAndRemoteWebPrintClientCredentialsRegistryItemUserControl
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
			this.LocalComputerAliasTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DomainNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReceivingIntervalIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			this.SendingIntervalIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VerboseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FailureNotificationGroupFindbox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DownTimeGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DownTimeStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DownTimeEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VerboseCheckBox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.DownTimeGroupbox.SuspendLayout();
			this.DownTimeStartDateEdit.SuspendLayout();
			this.DownTimeEndDateEdit.SuspendLayout();
			this.FailureNotificationGroupFindbox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Common.MailboxAndRemoteWebPrintClientCredentials);
			// 
			// LocalComputerAliasTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalComputerAliasTextBox, "LocalComputerAlias");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Common.MailboxAndRemoteWebPrintClientCredentials)(null)).LocalComputerAlias)));
			this.LocalComputerAliasTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 3, true);
			this.LocalComputerAliasTextBox.Name = "LocalComputerAliasTextBox";
			this.LocalComputerAliasTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.LocalComputerAliasTextBox.TabIndex = 0;
			// 
			// DomainNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.DomainNameTextBox, "DomainName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Common.MailboxAndRemoteWebPrintClientCredentials)(null)).DomainName)));
			this.DomainNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 29, true);
			this.DomainNameTextBox.Name = "DomainNameTextBox";
			this.DomainNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.DomainNameTextBox.TabIndex = 1;
			// 
			// ReceivingIntervalIntEdit
			//
			this.BindingSource.SetBindingMember(this.ReceivingIntervalIntEdit, "ReceivingInterval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Common.MailboxAndRemoteWebPrintClientCredentials)(null)).ReceivingInterval)));
			this.ReceivingIntervalIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 55, true);
			this.ReceivingIntervalIntEdit.Name = "ReceivingIntervalIntEdit";
			this.ReceivingIntervalIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ReceivingIntervalIntEdit.TabIndex = 2;
			// 
			// SendingIntervalIntEdit
			//
			this.BindingSource.SetBindingMember(this.SendingIntervalIntEdit, "SendingInterval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Common.MailboxAndRemoteWebPrintClientCredentials)(null)).SendingInterval)));
			this.SendingIntervalIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 81, true);
			this.SendingIntervalIntEdit.Name = "SendingIntervalIntEdit";
			this.SendingIntervalIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.SendingIntervalIntEdit.TabIndex = 3;
			// 
			// VerboseCheckBox
			// 
			this.VerboseCheckBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VerboseCheckBox, "Verbose");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Common.MailboxAndRemoteWebPrintClientCredentials)(null)).Verbose)));
			this.VerboseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 107, true);
			this.VerboseCheckBox.Name = "VerboseCheckBox";
			this.VerboseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.VerboseCheckBox.TabIndex = 4;
			// 
			// FailureNotificationGroupFindbox
			// 
			this.FailureNotificationGroupFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FailureNotificationGroupFindbox, "FailureNotificationGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Common.MailboxAndRemoteWebPrintClientCredentials)(null)).FailureNotificationGroup)));
			this.FailureNotificationGroupFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 133, true);
			this.FailureNotificationGroupFindbox.Name = "FailureNotificationGroupFindbox";
			this.FailureNotificationGroupFindbox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FailureNotificationGroupFindbox.ParentType = null;
			this.FailureNotificationGroupFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.FailureNotificationGroupFindbox.TabIndex = 5;
			// 
			// DownTimeGroupbox
			this.DownTimeGroupbox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("87114C24-B867-4813-A50A-D0701CCC0866", "Down Time");
			this.DownTimeGroupbox.Controls.Add(this.DownTimeStartDateEdit);
			this.DownTimeGroupbox.Controls.Add(this.DownTimeEndDateEdit);
			this.DownTimeGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 159, true);
			this.DownTimeGroupbox.Name = "DownTimeGroupbox";
			this.DownTimeGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 58, true);
			this.DownTimeGroupbox.TabIndex = 6;
			this.DownTimeGroupbox.TabStop = false;
			// 
			// DownTimeStartDateEdit
			// 
			this.DownTimeStartDateEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DownTimeStartDateEdit, "DownTimeStart");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Common.MailboxAndRemoteWebPrintClientCredentials)(null)).DownTimeStart)));
			this.DownTimeStartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DownTimeStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 10, true);
			this.DownTimeStartDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.DownTimeStartDateEdit.Name = "DownTimeStartDateEdit";
			this.DownTimeStartDateEdit.TabIndex = 7;
			// 
			// DownTimeEndDateEdit
			// 
			this.DownTimeEndDateEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DownTimeEndDateEdit, "DownTimeEnd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Common.MailboxAndRemoteWebPrintClientCredentials)(null)).DownTimeEnd)));
			this.DownTimeEndDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DownTimeEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 36, true);
			this.DownTimeEndDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.DownTimeEndDateEdit.Name = "DownTimeEndDateEdit";
			this.DownTimeEndDateEdit.TabIndex = 8;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Common.MailboxAndRemoteWebPrintClientCredentials)(null)).Status)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 223, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.ShowDescriptionBox = true;
			this.StatusDropEdit.ShouldResizeByMaxLength = true;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.StatusDropEdit.TabIndex = 9;
			// 
			// MailboxAndRemoteWebPrintClientCredentialsRegistryItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LocalComputerAliasTextBox);
			this.Controls.Add(this.DomainNameTextBox);
			this.Controls.Add(this.ReceivingIntervalIntEdit);
			this.Controls.Add(this.SendingIntervalIntEdit);
			this.Controls.Add(this.VerboseCheckBox);
			this.Controls.Add(this.FailureNotificationGroupFindbox);
			this.Controls.Add(this.StatusDropEdit);
			this.Controls.Add(this.DownTimeGroupbox);
			this.Name = "MailboxAndRemoteWebPrintClientCredentialsRegistryItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 260, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VerboseCheckBox.ResumeLayout(true);
			this.StatusDropEdit.ResumeLayout(true);
			this.VerboseCheckBox.PerformLayout();
			this.StatusDropEdit.PerformLayout();
			this.FailureNotificationGroupFindbox.ResumeLayout(true);
			this.FailureNotificationGroupFindbox.PerformLayout();
			this.DownTimeGroupbox.ResumeLayout(false);
			this.DownTimeGroupbox.PerformLayout();
			this.DownTimeStartDateEdit.ResumeLayout(true);
			this.DownTimeStartDateEdit.PerformLayout();
			this.DownTimeEndDateEdit.ResumeLayout(true);
			this.DownTimeEndDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZTextBox LocalComputerAliasTextBox;
		ZTextBox DomainNameTextBox;
		ZIntEdit ReceivingIntervalIntEdit;
		ZIntEdit SendingIntervalIntEdit;
		ZDropEdit StatusDropEdit;
		ZCheckBox VerboseCheckBox;
		ZCodeFindBox FailureNotificationGroupFindbox;
		ZGroupBox DownTimeGroupbox;
		ZDateEdit DownTimeStartDateEdit;
		ZDateEdit DownTimeEndDateEdit;
	}
}

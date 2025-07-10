using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.eNett;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class AllocateToOrganisationForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.AllocateGroupBox = new ZGroupBox();
			this.ECNTextBox = new ZArchitecture.ZTextBox();
			this.RelatedOrganisationsTextBox = new ZArchitecture.ZTextBox();
			this.FaxTextBox = new ZArchitecture.ZTextBox();
			this.PhoneTextBox = new ZArchitecture.ZTextBox();
			this.CountryTextBox = new ZArchitecture.ZTextBox();
			this.PostcodeTextBox = new ZArchitecture.ZTextBox();
			this.StateTextBox = new ZArchitecture.ZTextBox();
			this.SuburbTextBox = new ZArchitecture.ZTextBox();
			this.TerminalCodeTextBox = new ZArchitecture.ZTextBox();
			this.ABNTextBox = new ZArchitecture.ZTextBox();
			this.Address2TextBox = new ZArchitecture.ZTextBox();
			this.Address1TextBox = new ZArchitecture.ZTextBox();
			this.RegistrationDateDateEdit = new ZDateEdit();
			this.ClientNameTextBox = new ZArchitecture.ZTextBox();
			this.OrgFindBox = new ZGuidFindBox();
			this.CloseButton = new ZButton();
			this.AllocateButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AllocateGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 276, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 20, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ComPayRegisteredOrganisation);
			// 
			// AllocateGroupBox
			// 
			this.AllocateGroupBox.Controls.Add(this.ECNTextBox);
			this.AllocateGroupBox.Controls.Add(this.RelatedOrganisationsTextBox);
			this.AllocateGroupBox.Controls.Add(this.FaxTextBox);
			this.AllocateGroupBox.Controls.Add(this.PhoneTextBox);
			this.AllocateGroupBox.Controls.Add(this.CountryTextBox);
			this.AllocateGroupBox.Controls.Add(this.PostcodeTextBox);
			this.AllocateGroupBox.Controls.Add(this.StateTextBox);
			this.AllocateGroupBox.Controls.Add(this.SuburbTextBox);
			this.AllocateGroupBox.Controls.Add(this.TerminalCodeTextBox);
			this.AllocateGroupBox.Controls.Add(this.ABNTextBox);
			this.AllocateGroupBox.Controls.Add(this.Address2TextBox);
			this.AllocateGroupBox.Controls.Add(this.Address1TextBox);
			this.AllocateGroupBox.Controls.Add(this.RegistrationDateDateEdit);
			this.AllocateGroupBox.Controls.Add(this.ClientNameTextBox);
			this.AllocateGroupBox.Controls.Add(this.OrgFindBox);
			this.AllocateGroupBox.Controls.Add(this.CloseButton);
			this.AllocateGroupBox.Controls.Add(this.AllocateButton);
			this.AllocateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AllocateGroupBox, false);
			this.AllocateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AllocateGroupBox.Name = "AllocateGroupBox";
			this.AllocateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 276, true);
			this.AllocateGroupBox.TabIndex = 4;
			this.AllocateGroupBox.TabStop = false;
			// 
			// ECNTextBox
			// 
			this.BindingSource.SetBindingMember(this.ECNTextBox, "ECN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZInt)(((ComPayRegisteredOrganisation)(null)).ECN)));
			this.ECNTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ECNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 15, true);
			this.ECNTextBox.Name = "ECNTextBox";
			this.ECNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.ECNTextBox.TabIndex = 2;
			// 
			// RelatedOrganisationsTextBox
			// 
			this.BindingSource.SetBindingMember(this.RelatedOrganisationsTextBox, "RelatedOrganisations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(null)).RelatedOrganisations)));
			this.RelatedOrganisationsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RelatedOrganisationsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 222, true);
			this.RelatedOrganisationsTextBox.Name = "RelatedOrganisationsTextBox";
			this.RelatedOrganisationsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 20, true);
			this.RelatedOrganisationsTextBox.TabIndex = 15;
			// 
			// FaxTextBox
			// 
			this.BindingSource.SetBindingMember(this.FaxTextBox, "Fax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(null)).Fax)));
			this.FaxTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FaxTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 92, true);
			this.FaxTextBox.Name = "FaxTextBox";
			this.FaxTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.FaxTextBox.TabIndex = 8;
			// 
			// PhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.PhoneTextBox, "Phone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(null)).Phone)));
			this.PhoneTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 92, true);
			this.PhoneTextBox.Name = "PhoneTextBox";
			this.PhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.PhoneTextBox.TabIndex = 7;
			// 
			// CountryTextBox
			// 
			this.BindingSource.SetBindingMember(this.CountryTextBox, "Country");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(null)).Country)));
			this.CountryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 196, true);
			this.CountryTextBox.Name = "CountryTextBox";
			this.CountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.CountryTextBox.TabIndex = 14;
			// 
			// PostcodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PostcodeTextBox, "Postcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(null)).Postcode)));
			this.PostcodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PostcodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 196, true);
			this.PostcodeTextBox.Name = "PostcodeTextBox";
			this.PostcodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.PostcodeTextBox.TabIndex = 13;
			// 
			// StateTextBox
			// 
			this.BindingSource.SetBindingMember(this.StateTextBox, "State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(null)).State)));
			this.StateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 170, true);
			this.StateTextBox.Name = "StateTextBox";
			this.StateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.StateTextBox.TabIndex = 12;
			// 
			// SuburbTextBox
			// 
			this.BindingSource.SetBindingMember(this.SuburbTextBox, "Suburb");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(null)).Suburb)));
			this.SuburbTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SuburbTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 170, true);
			this.SuburbTextBox.Name = "SuburbTextBox";
			this.SuburbTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.SuburbTextBox.TabIndex = 11;
			// 
			// TerminalCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.TerminalCodeTextBox, "TerminalCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(null)).TerminalCode)));
			this.TerminalCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TerminalCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 67, true);
			this.TerminalCodeTextBox.Name = "TerminalCodeTextBox";
			this.TerminalCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.TerminalCodeTextBox.TabIndex = 6;
			// 
			// ABNTextBox
			// 
			this.BindingSource.SetBindingMember(this.ABNTextBox, "ABN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(null)).ABN)));
			this.ABNTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ABNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 41, true);
			this.ABNTextBox.Name = "ABNTextBox";
			this.ABNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.ABNTextBox.TabIndex = 4;
			// 
			// Address2TextBox
			// 
			this.BindingSource.SetBindingMember(this.Address2TextBox, "Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(null)).Address2)));
			this.Address2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Address2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 144, true);
			this.Address2TextBox.Name = "Address2TextBox";
			this.Address2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 20, true);
			this.Address2TextBox.TabIndex = 10;
			// 
			// Address1TextBox
			// 
			this.BindingSource.SetBindingMember(this.Address1TextBox, "Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(null)).Address1)));
			this.Address1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Address1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 118, true);
			this.Address1TextBox.Name = "Address1TextBox";
			this.Address1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 20, true);
			this.Address1TextBox.TabIndex = 9;
			// 
			// RegistrationDateDateEdit
			// 
			this.RegistrationDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.RegistrationDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RegistrationDateDateEdit, "RegistrationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ComPayRegisteredOrganisation)(null)).RegistrationDate)));
			this.RegistrationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 67, true);
			this.RegistrationDateDateEdit.Name = "RegistrationDateDateEdit";
			this.RegistrationDateDateEdit.TabIndex = 5;
			// 
			// ClientNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientNameTextBox, "ClientName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ComPayRegisteredOrganisation)(null)).ClientName)));
			this.ClientNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 41, true);
			this.ClientNameTextBox.Name = "ClientNameTextBox";
			this.ClientNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.ClientNameTextBox.TabIndex = 3;
			// 
			// OrgFindBox
			// 
			this.BindingSource.SetBindingMember(this.OrgFindBox, "CusCode.OK_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((ComPayRegisteredOrganisation)(null)).CusCode.OK_OH)));
			this.OrgFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AllocateToOrganisationForm|1931b96c-926f-4a1a-966a-b74a8cea9179", "Organization");
			this.OrgFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 15, true);
			this.OrgFindBox.Name = "OrgFindBox";
			this.OrgFindBox.ShowDescriptionBox = false;
			this.OrgFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.OrgFindBox.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AllocateToOrganisationForm|2cfbe676-98b4-415e-8621-65867948a8d7", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 248, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 22, true);
			this.CloseButton.TabIndex = 17;
			this.CloseButton.Click += new EventHandler(this.CancelButton_Click);
			// 
			// AllocateButton
			// 
			this.AllocateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AllocateToOrganisationForm|21fc6aec-415d-41bb-af71-b56a1ce1e8cb", "Save");
			this.AllocateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 248, true);
			this.AllocateButton.Name = "AllocateButton";
			this.AllocateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 22, true);
			this.AllocateButton.TabIndex = 16;
			this.AllocateButton.Click += new EventHandler(this.AllocateButton_Click);
			// 
			// AllocateToOrganisationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 296, true);
			this.Controls.Add(this.AllocateGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(ComPayRegisteredOrganisation);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "AllocateToOrganisationForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.AllocateGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AllocateGroupBox.ResumeLayout(false);
			this.AllocateGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
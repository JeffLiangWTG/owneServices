using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ZClientEDI.GUI.UserManagement;

namespace Enterprise.Client.EDI.UserManagement.GUI
{
	public partial class SystemUserAccountsWizardForm : ZChildForm
	{
		readonly SystemUserAccountsWizard BusinessEntityWizard;
		protected CustomerUserAccountWizardFilterControl customerUserAccountWizardFilterControl;
		protected UserAccountLinkedToContactUserControl userAccountLinkedToContactUserControl;
		protected ZToolStripButton SaveStripButton;
		protected ZToolStripButton SaveCloseStripButton;
		protected ZToolStripButton CancelStripButton;
		readonly List<EdiCustomerUserAccount> changedEdiCustomerUserAccounts = new();
		readonly bool AllowEdit;

		EdiContactSendEmailSetResetPassword ContactSendEmailSetResetPassword => contactSendEmailSetResetPassword ?? (contactSendEmailSetResetPassword = new EdiContactSendEmailSetResetPassword());
		EdiContactSendEmailSetResetPassword contactSendEmailSetResetPassword;

		EdiCustomerUserAccount selectedEdiCustomerUserAccount => customerUserAccountWizardFilterControl.Grid.ListManager.GetCurrent() as EdiCustomerUserAccount;

		public SystemUserAccountsWizardForm(SystemUserAccountsWizard systemUserAccountsWizard)
			: base()
		{
			BusinessEntityWizard = systemUserAccountsWizard;
			AllowEdit = EDISecurityCheckpoints.SystemUserAccountsEdit.IsAllowed;

			if (!DesignModeFinder.IsDesigning)
			{
				this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor; // Set BackColor before calling InitializeComponent() so that child checkboxes inherit the BackColor
			}

			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			AddCustomerUserAccountWizardFilterControl();
			AddUserAccountLinkedToContactUserControl(null);

			CancelStripButton = new ZToolStripButton();
			InitialiseButton(CancelStripButton, Res.GetString("0ce76cce-fd30-40c8-ab20-4bb8957edb6a", "Cancel"), Icons.GetImage(IconTypes.BlackWhite_Cancel), CancelButton_Click);

			SaveCloseStripButton = new ZToolStripButton();
			InitialiseButton(SaveCloseStripButton, Res.GetString("f120be45-1a26-4681-b577-f8af6447852e", "Save && Close"), Icons.GetImage(IconTypes.BlackWhite_SaveClose), SaveCloseButton_Click);

			SaveStripButton = new ZToolStripButton();
			InitialiseButton(SaveStripButton, Res.GetString("cb02f0d3-dce2-4524-8fe3-7ff30d9273c0", "Save"), Icons.GetImage(IconTypes.BlackWhite_Save), SaveButton_Click);

			EnableSaveButtons(false);

			BusinessEntityWizard.HasChangesChanged += BusinessEntity_HasChangesChanged;
			FormClosing += SystemUserAccountsWizardForm_FormClosing;

			CheckPermission();
		}

		#region CustomerUserAccountWizardFilterControl

		void AddCustomerUserAccountWizardFilterControl()
		{
			customerUserAccountWizardFilterControl = new CustomerUserAccountWizardFilterControl(BusinessEntityWizard.EdiCustomerUserAccountCollection, new CustomerUserAccountWizardFilterBusinessObject()) { Dock = DockStyle.Fill };
			customerUserAccountWizardFilterControl.PerformSearch += RefreshWizardEdiCustomerUserAccountCollection;
			customerUserAccountWizardFilterControl.Dock = DockStyle.Fill;
			EdiCustomerUserAccountBottomPanel.Controls.Add(customerUserAccountWizardFilterControl);
		}

		void RefreshWizardEdiCustomerUserAccountCollection(object sender, EventArgs e)
		{
			var filter = customerUserAccountWizardFilterControl.filterStrip.Filter;
			filter.MaximumRows = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;

			var result = customerUserAccountWizardFilterControl.SearchManager.PerformSearch(BusinessEntityWizard.Factory, typeof(EdiCustomerUserAccount), filter);
			customerUserAccountWizardFilterControl.SearchManager.PushItemsIntoCollection(BusinessEntityWizard.EdiCustomerUserAccountCollection, result, null);
		}

		void EdiCustomerUserAccountFilterControl_CurrentChanged(object sender, EventArgs e)
		{	
			var current = selectedEdiCustomerUserAccount;
			if (current != null)
			{
				WebAccessContactGuidFindBox.SetDataBinding(current, EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact.Name);
				WebAccessContactGuidFindBox.Enabled = AllowEdit;
				current.EUA_OC_WebAccessContactInfo.ValueChanged += EUA_OC_WebAccessContactInfo_ValueChanged;
				AddUserAccountLinkedToContactUserControl(current.EDIWebAccessContact);

				GetSystemUserInformation(current);
			}
			else
			{
				ClearFormData();
			}
		}

		void EUA_OC_WebAccessContactInfo_ValueChanged(object sender, EventArgs e)
		{
			var ediCustomerUserAccount = sender as EdiCustomerUserAccount;
			AddUserAccountLinkedToContactUserControl(ediCustomerUserAccount?.EDIWebAccessContact);

			if (ediCustomerUserAccount.HasChanges && !changedEdiCustomerUserAccounts.Contains(ediCustomerUserAccount))
			{
				changedEdiCustomerUserAccounts.Add(ediCustomerUserAccount);
			}
		}

		void GetSystemUserInformation(EdiCustomerUserAccount ediCustomerUserAccount)
		{
			var licenceDatabase = ediCustomerUserAccount.Database;
			if (licenceDatabase != null)
			{
				EnterpriseIdValueLabel.Text = licenceDatabase.EnterpriseID;
				EnterpriseCodeValueLabel.Text = licenceDatabase.EnterpriseCode;
				ProductValueLabel.Text = licenceDatabase.ProductCodeDescription;
				ServerCodeValueLabel.Text = licenceDatabase.LD_ServerCode;
				DatabaseNumberValueLabel.Text = licenceDatabase.LD_DatabaseNumber.ToString();
				SystemIdValueLabel.Text = licenceDatabase.TrustedSystem?.ETS_SystemID;
				TenantIDValueLabel.Text = licenceDatabase.LD_TenantID;
			}

			ContactOrganisationValueLabel.Text = ediCustomerUserAccount.ContactOrganisation?.NameAndCode;
			ContactEmailValueLabel.Text = ediCustomerUserAccount.WebAccessContact?.OC_Email;

			UserIDValueLabel.Text = ediCustomerUserAccount.EUA_UserID;
			UserStatusValueLabel.Text = ediCustomerUserAccount.EUA_ContactRelationshipStatus;
			UserEmailValueLabel.Text = ediCustomerUserAccount.EUA_Email;

			var webAccessContact = ediCustomerUserAccount.EDIWebAccessContact;

			if (webAccessContact != null)
			{
				UserNameValueLabel.Text = webAccessContact.Name;

				WebAccessEnabledCheckBox.Checked = webAccessContact.OC_WebAccessEnabled;
				EnableSendPasswordInstructionsButton(webAccessContact.OC_WebAccessEnabled);

				if (webAccessContact.PersonPasswordHashIsSet)
				{
					PasswordSetCheckBox.Text = Res.GetString("b9cde5e7-7699-4095-90fe-5647c15d678d", "Person");
					PasswordSetCheckBox.Checked = true;
				}
				else if (webAccessContact.PasswordHashIsSet)
				{
					PasswordSetCheckBox.Text = Res.GetString("a82a3152-c0a4-4dc6-9bdf-71babc211bc4", "Contact");
					PasswordSetCheckBox.Checked = true;
				}
				else
				{
					PasswordSetCheckBox.Text = "";
					PasswordSetCheckBox.Checked = false;
				}

				var passwordLog = webAccessContact.GetPasswordChangedOrSentLog();
				InstructionsLastSentValueLabel.Text = passwordLog != null ? passwordLog.PostedLocalBranchTime.ToLongTimeString() : "";
			}
			else
			{
				UserNameValueLabel.Text = string.Empty;
				WebAccessEnabledCheckBox.Enabled = false;
				WebAccessEnabledCheckBox.Checked = false;
				PasswordSetCheckBox.Checked = false;
				InstructionsLastSentValueLabel.Text = string.Empty;
				EnableSendPasswordInstructionsButton(false);
			}
		}

		void SendPasswordInstructionsButton_Click(object sender, EventArgs e)
		{
			var selectedContact = selectedEdiCustomerUserAccount.WebAccessContact;
			if (selectedContact != null)
			{
				if (!selectedContact.OC_IsActive)
				{
					var userConfirmation = Globals.Message.Show(
						Res.GetString("3cfd0daf-63a5-49f9-89d5-8e96b5040655", "This contact is not active. Would you like to activate the contact and send the password instruction?"),
						Res.GetString("a105ed49-aa94-4316-80c1-17d0a0a14818", "Send password instructions"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Question);

					if (userConfirmation == DialogResult.No)
					{
						return;
					}

					selectedContact.OC_IsActive = true;
					EnableSaveButtons(true);
				}

				if (ContactSendEmailSetResetPassword.SendPasswordInstructions(selectedContact))
				{
					SetSetPasswordInstructionsButton();
				}
			}
		}

		void SetSetPasswordInstructionsButton()
		{
			var selectedContact = selectedEdiCustomerUserAccount.EDIWebAccessContact;

			EnableSendPasswordInstructionsButton(selectedContact != null && (ZBool)selectedContact.OC_WebAccessEnabledInfo.OriginalValue);

			if (selectedContact != null)
			{
				PasswordSetCheckBox.Checked = selectedContact.PersonPasswordHashIsSet || selectedContact.PasswordHashIsSet;

				var passwordLog = selectedContact.GetPasswordChangedOrSentLog();
				InstructionsLastSentValueLabel.Text = passwordLog != null ? passwordLog.PostedLocalBranchTime.ToLongTimeString() : "";
			}
		}

		void ClearFormData()
		{
			WebAccessContactGuidFindBox.CodeBox.Text = string.Empty;
			WebAccessContactGuidFindBox.SetDataBinding(null, EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact.Name);
			AddUserAccountLinkedToContactUserControl(null);
			ClearSystemUserInformationData();
			WebAccessContactGuidFindBox.Enabled = false;
		}

		void ClearSystemUserInformationData()
		{
			EnterpriseIdValueLabel.Text = string.Empty;
			EnterpriseCodeValueLabel.Text = string.Empty;
			ProductValueLabel.Text = string.Empty;
			ServerCodeValueLabel.Text = string.Empty;
			DatabaseNumberValueLabel.Text = string.Empty;
			SystemIdValueLabel.Text = string.Empty;
			TenantIDValueLabel.Text = string.Empty;
			ContactOrganisationValueLabel.Text = string.Empty;
			ContactEmailValueLabel.Text = string.Empty;
			UserIDValueLabel.Text = string.Empty;
			UserStatusValueLabel.Text = string.Empty;
			UserNameValueLabel.Text = string.Empty;
			UserEmailValueLabel.Text = string.Empty;
			WebAccessEnabledCheckBox.Enabled = false;
			WebAccessEnabledCheckBox.Checked = false;
			PasswordSetCheckBox.Checked = false;
			InstructionsLastSentValueLabel.Text = string.Empty;
			EnableSendPasswordInstructionsButton(false);
		}

		#endregion

		#region UserAccountLinkedToContactUserControl

		void AddUserAccountLinkedToContactUserControl(EDIOrgContact orgContact)
		{
			if (userAccountLinkedToContactUserControl != null)
			{
				ContactGroupBox.Controls.Remove(userAccountLinkedToContactUserControl);
				userAccountLinkedToContactUserControl.Dispose();
				userAccountLinkedToContactUserControl = null;
			}

			var relatedUser = BusinessEntityWizard.UserAccountLinkedToContact(orgContact);
			userAccountLinkedToContactUserControl = GetUserAccountLinkedToContactUserControl(relatedUser);
			relatedUser.Load();

			userAccountLinkedToContactUserControl.Dock = DockStyle.Fill;
			ContactGroupBox.Controls.Add(userAccountLinkedToContactUserControl);
		}

		public virtual UserAccountLinkedToContactUserControl GetUserAccountLinkedToContactUserControl(EdiCustomerUserAccountCollection ediCustomerUserAccountCollection)
		{
			return new UserAccountLinkedToContactUserControl(ediCustomerUserAccountCollection);
		}

		#endregion

		#region Events

		void InitialiseButton(ToolStripItem button, string text, Image image, EventHandler clickHandler)
		{
			button.Image = image;
			button.ImageScaling = ToolStripItemImageScaling.SizeToFit;
			button.Text = text;
			button.Margin = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0);
			button.AutoToolTip = false;
			button.Alignment = ToolStripItemAlignment.Right;
			button.Click += clickHandler;
			SaveToolStrip.Items.Add(button);
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			if (SaveSelection(true))
			{
				EnableSaveButtons(false);
				changedEdiCustomerUserAccounts.Clear();
				customerUserAccountWizardFilterControl.FirePerformSearch();
			}
		}

		void SaveCloseButton_Click(object sender, EventArgs e)
		{
			if (SaveSelection(true))
			{
				changedEdiCustomerUserAccounts.Clear();
				Close();
			}
		}

		bool SaveSelection(bool showConfirmationMessage)
		{
			var result = false;

			if (!changedEdiCustomerUserAccounts.Any())
			{
				return true;
			}

			if (!ValidateErrors(changedEdiCustomerUserAccounts))
			{
				var caption = Res.GetString("233b1a68-f009-44d3-95bb-088f5e57a5d2", "Save");
				var message = Res.GetString("5a993c11-b73d-4aad-aedf-083e93693d70", "Would you like to save the changes?");
				if (!showConfirmationMessage || Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(BusinessEntityWizard.Factory.Save, null, true);
					result = true;
					EnableSaveButtons(false);
				}
			}

			return result;
		}

		bool ValidateErrors(List<EdiCustomerUserAccount> ediCustomerUserAccounts)
		{
			var result = false;
			var hasErrors = ediCustomerUserAccounts.Where(l => l.HasErrors);
			if (hasErrors.Any())
			{
				var message = new StringBuilder();
				foreach (var item in hasErrors)
				{
					message.AppendLine(FormattableString.Invariant($"{item.EUA_FullName} - {item.EUA_Email} - {item.EUA_UserID}"));
				}

				result = true;
				Globals.Message.ShowError(Res.GetString("b5165a55-06fc-47d6-9622-625c02688ec3", "The following user(s) couldn't be saved:\r\n{0}", message.ToString()));
			}

			return result;
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			var caption = Res.GetString("8450c889-89a9-4cc7-a22c-1d818b0d52c9", "Cancel");
			var message = Res.GetString("a527cde9-461f-41f0-ba1e-8544b070dcad", "The pending changes will be discarded, do you wish to proceed?");
			if (DialogResult.Yes == Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No))
			{
				BusinessEntityWizard.CancelChanges();
				BusinessEntityWizard.ClearHasChanges();
				BusinessEntityWizard.ClearAllNotifications();

				AddUserAccountLinkedToContactUserControl(null);
				customerUserAccountWizardFilterControl.FirePerformSearch();
			}
		}

		void SystemUserAccountsWizardForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (BusinessEntityWizard.HasChanges)
			{
				var caption = Res.GetString("9f1c7c71-e1fe-4491-8550-f3bc41ecfe43", "Close");
				var message = Res.GetString("90d5a1d6-da09-4623-a13d-d9f016cf808f", "Do you wish to save pending changes before close?");
				if (DialogResult.Yes == Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No))
				{
					if (!SaveSelection(false))
					{
						e.Cancel = true;
					}
				}
			}
		}

		void BusinessEntity_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			EnableSaveButtons(e.ObjectJustWasChanged);
		}

		void EnableSaveButtons(bool enable)
		{
			if (!AllowEdit)
			{
				enable = false;
			}

			SaveStripButton.Enabled = enable;
			SaveCloseStripButton.Enabled = enable;
			CancelStripButton.Enabled = enable;
		}

		#endregion

		void CheckPermission()
		{
			if (!AllowEdit)
			{
				EnableSaveButtons(false);
				WebAccessContactGuidFindBox.Enabled = false;
				EnableSendPasswordInstructionsButton(AllowEdit);
			}
		}

		void EnableSendPasswordInstructionsButton(bool enable)
		{
			if (!AllowEdit)
			{
				enable = false;
			}
			SendPasswordInstructionsButton.Enabled = enable;
		}

		#region Implementation

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			customerUserAccountWizardFilterControl.Grid.ListManager.CurrentChanged += EdiCustomerUserAccountFilterControl_CurrentChanged;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}

			PostingButtonsUserControl?.Dispose();
			base.Dispose(disposing);
		}

		#endregion
	}
}


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Core.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ZClientEDI.GUI.Licencing;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	public partial class LicenceDatabaseRegistrationWizardForm : ZChildForm
	{
		readonly LicenceDatabaseRegistrationWizard BusinessEntityWizard;
		protected LicenceDatabaseRegistrationWizardFilterControl licenceDatabaseRegistrationWizardFilterControl;
		protected OrganisationWizardFilterControl organisationWizardFilterControl;
		protected DatabaseDetailsWizardUserControl databaseDetailsWizardUserControl;
		protected DatabaseOrgSuggestionWizardUserControl databaseOrgSuggestionWizardUserControl;
		protected ZToolStripButton SaveStripButton;
		protected ZToolStripButton SaveCloseStripButton;
		protected ZToolStripButton CancelStripButton;

		LicenceDatabase selectedLicenceDatabase => licenceDatabaseRegistrationWizardFilterControl.Grid.ListManager.GetCurrent() as LicenceDatabase;

		public LicenceDatabaseRegistrationWizardForm(LicenceDatabaseRegistrationWizard licenceDatabaseRegistrationWizard)
			: base()
		{
			BusinessEntityWizard = licenceDatabaseRegistrationWizard;

			if (!DesignModeFinder.IsDesigning)
			{
				this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor; // Set BackColor before calling InitializeComponent() so that child checkboxes inherit the BackColor
			}

			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			AddLicenceDatabaseRegistrationWizardFilterControl();
			AddOrganisationWizardFilterControl();
			AddDatabaseDetailsWizardUserControl(null);
			AddDatabaseOrgSuggestionWizardUserControl(null);

			CancelStripButton = new ZToolStripButton();
			InitialiseButton(CancelStripButton, Res.GetString("218A8CD2-E75F-4DFF-BC37-81677A7627B0", "Cancel"), Icons.GetImage(IconTypes.BlackWhite_Cancel), CancelButton_Click);

			SaveCloseStripButton = new ZToolStripButton();
			InitialiseButton(SaveCloseStripButton, Res.GetString("E4609031-0D7F-4707-9811-774931B236E4", "Save && Close"), Icons.GetImage(IconTypes.BlackWhite_SaveClose), SaveCloseButton_Click);

			SaveStripButton = new ZToolStripButton();
			InitialiseButton(SaveStripButton, Res.GetString("00AA6104-1F9B-4949-B3B4-FDC187B67ABD", "Save"), Icons.GetImage(IconTypes.BlackWhite_Save), SaveButton_Click);

			EnableSaveButtons(false);
			ShowFiltersLicenceDatabaseCheckBox_CheckedChanged(null, null);
			ShowFiltersOrganisationCheckBox_CheckedChanged(null, null);

			BusinessEntityWizard.HasChangesChanged += BusinessEntity_HasChangesChanged;
			FormClosing += LicenceDatabaseRegistrationWizardForm_FormClosing;
		}

		#region LicenceDatabaseRegistrationWizardFilterControl

		void AddLicenceDatabaseRegistrationWizardFilterControl()
		{
			licenceDatabaseRegistrationWizardFilterControl = new LicenceDatabaseRegistrationWizardFilterControl(BusinessEntityWizard.LicenceDatabaseCollection, new LicenceDatabaseWizardFilterBusinessObject()) { Dock = DockStyle.Fill };
			licenceDatabaseRegistrationWizardFilterControl.Dock = DockStyle.Fill;
			licenceDatabaseBottomPanel.Controls.Add(licenceDatabaseRegistrationWizardFilterControl);
		}

		void LicenceDatabaseRegistrationWizardFilterControl_CurrentChanged(object sender, EventArgs e)
		{
			var current = selectedLicenceDatabase;
			if (current != null)
			{
				WebAccessOrgGuidFindBox.SetDataBinding(current, LicenceDatabaseSchema.LD_OH_WebAccessOrg.Name);
				current.LD_OH_WebAccessOrgInfo.ValueChanged += LD_OH_WebAccessOrgInfo_ValueChanged;
				AddDatabaseDetailsWizardUserControl(current.EDIWebAccessOrg);
				AddDatabaseOrgSuggestionWizardUserControl(current);

				GetAdditionalInformation(current);
			}
			else
			{
				ClearFormData();
			}

			if (selectedLicenceDatabase == null || selectedLicenceDatabase.LD_OH_WebAccessOrg.IsEmpty)
			{
				organisationWizardFilterControl.SelectFirstRow();
			}
		}

		void LD_OH_WebAccessOrgInfo_ValueChanged(object sender, EventArgs e)
		{
			var licenceDatabase = sender as LicenceDatabase;
			licenceDatabase.IgnoreValidationSuspended = true;
			licenceDatabase.Validation.ValidateLD_OH_WebAccessOrg();

			var orgHeader = licenceDatabase.EDIWebAccessOrg;
			if (orgHeader != null && orgHeader.LicEnterprise != null)
			{
				licenceDatabase.LD_LE = orgHeader.LicEnterprise.PK;// Temporary set, will create a new one if needed on Save
			}
			AddDatabaseDetailsWizardUserControl(orgHeader);
		}

		void GetAdditionalInformation(LicenceDatabase licenceDatabase)
		{
			ClearAdditionalLicenceInformation();

			EnterpriseIdTextBox.Text = licenceDatabase.EnterpriseID;

			var notes = licenceDatabase.Notes.GetAllNotes().Cast<StmNote>();

			var importNote = GetAdditionalInfoNote(notes);
			if (importNote != null)
			{
				var text = importNote.ST_NoteDataAsText;
				var result = Regex.Split(text, "\r\n|\r|\n");
				foreach (var line in result)
				{
					if (XmlStartWith(line,"OrgName"))
					{
						OwnerNameValueLabel.Text = XmlReplaceString(line, "OrgName");
					}
					else if (XmlStartWith(line, "Address1"))
					{
						Address1ValueLabel.Text = XmlReplaceString(line, "Address1");
					}
					else if (XmlStartWith(line, "Address2"))
					{
						Address2ValueLabel.Text = XmlReplaceString(line, "Address2");
					}
					else if (XmlStartWith(line, "City"))
					{
						CityValueLabel.Text = XmlReplaceString(line, "City");
					}
					else if (XmlStartWith(line, "State"))
					{
						StateValueLabel.Text = XmlReplaceString(line, "State");
					}
					else if (XmlStartWith(line, "Postcode"))
					{
						PostCodeValueLabel.Text = XmlReplaceString(line, "Postcode");
					}
					else if (XmlStartWith(line, "OrgCountry"))
					{
						var country = XmlReplaceString(line, "OrgCountry");
						CountryValueLabel.Text = GetCountryCode(country);
					}
					else if (XmlStartWith(line, "Product"))
					{
						ProductValueLabel.Text = XmlReplaceString(line, "Product");
					}
					else if (XmlStartWith(line, "SystemId"))
					{
						SystemIdValueLabel.Text = XmlReplaceString(line, "SystemId");
					}
					else if (XmlStartWith(line, "TenantId"))
					{
						TenantIdValueLabel.Text = XmlReplaceString(line, "TenantId");
					}
					else if (XmlStartWith(line, "BusinessRegoNo"))
					{
						BusinessRegoNoValueLabel.Text = XmlReplaceString(line, "BusinessRegoNo");
					}
					else if (XmlStartWith(line, "EnterpriseCode"))
					{
						EnterpriseCodeValueLabel.Text = XmlReplaceString(line, "EnterpriseCode");
					}
					else if (XmlStartWith(line, "ServerCode"))
					{
						ServerCodeValueLabel.Text = XmlReplaceString(line, "ServerCode");
					}
					else if (XmlStartWith(line, "CargowiseCompanyCode"))
					{
						CargowiseCompanyCodeValueLabel.Text = XmlReplaceString(line, "CargowiseCompanyCode");
					}
					else if (XmlStartWith(line, "DatabaseNumber"))
					{
						DatabaseNumberValueLabel.Text = XmlReplaceString(line, "DatabaseNumber");
					}
					else if (XmlStartWith(line, "InfoExpires"))
					{
						InfoExpiresValueLabel.Text = XmlReplaceString(line, "InfoExpires");
					}
				}
			}
		}

		string GetCountryCode(string country)
		{
			var result = country;
			if (result?.Length > 2)
			{
				var refCountry = BusinessEntityWizard.Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Desc, result));
				result = refCountry?.RN_Code;
			}
			return result;
		}

		StmNote GetAdditionalInfoNote(IEnumerable<StmNote> notes)
		{
			StmNote note = null;
			var importNote = notes.LastOrDefault(n => n.ST_Description == EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description);
			var additionalInfoNote = notes.LastOrDefault(n => n.ST_Description == EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationAdditionalInfoNote.Description);

			if (importNote != null && additionalInfoNote != null)
			{
				note = importNote.ST_CreatedDateUtc > additionalInfoNote.ST_CreatedDateUtc ? importNote : additionalInfoNote;
			}
			else if (importNote != null)
			{
				note = importNote;
			}
			else if (additionalInfoNote != null)
			{
				note = additionalInfoNote;
			}

			return note;
		}

		bool XmlStartWith(string line, string value)
		{
			return line.StartsWith(value + ":", StringComparison.OrdinalIgnoreCase);
		}

		string XmlReplaceString(string line, string value)
		{
			return Regex.Replace(line, value + ":", "", RegexOptions.IgnoreCase).Trim();
		}

		protected void ShowFiltersLicenceDatabaseCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			licenceDatabaseRegistrationWizardFilterControl.IsFilterVisible = ShowFiltersLicenceDatabaseCheckBox.Checked;
			licenceDatabaseTopPanel.Visible = ShowFiltersLicenceDatabaseCheckBox.Checked;
		}

		void ClearFormData()
		{
			WebAccessOrgGuidFindBox.CodeBox.Text = string.Empty;
			WebAccessOrgGuidFindBox.SetDataBinding(null, EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact.Name);
			AddDatabaseDetailsWizardUserControl(null);
			AddDatabaseOrgSuggestionWizardUserControl(null);
			ClearAdditionalLicenceInformation();
		}

		void ClearAdditionalLicenceInformation()
		{
			EnterpriseIdTextBox.Text = string.Empty;
			OwnerNameValueLabel.Text = string.Empty;
			Address1ValueLabel.Text = string.Empty;
			Address2ValueLabel.Text = string.Empty;
			CityValueLabel.Text = string.Empty;
			StateValueLabel.Text = string.Empty;
			PostCodeValueLabel.Text = string.Empty;
			CountryValueLabel.Text = string.Empty;
			ProductValueLabel.Text = string.Empty;
			SystemIdValueLabel.Text = string.Empty;
			TenantIdValueLabel.Text = string.Empty;
			BusinessRegoNoValueLabel.Text = string.Empty;
			EnterpriseCodeValueLabel.Text = string.Empty;
			ServerCodeValueLabel.Text = string.Empty;
			CargowiseCompanyCodeValueLabel.Text = string.Empty;
			DatabaseNumberValueLabel.Text = string.Empty;
			InfoExpiresValueLabel.Text = string.Empty;
		}

		#endregion

		#region OrganisationWizardFilterControl

		void AddOrganisationWizardFilterControl()
		{
			organisationWizardFilterControl = new OrganisationWizardFilterControl(BusinessEntityWizard, new OrganisationWizardFilterBusinessObject()) { Dock = DockStyle.Fill };
			organisationWizardFilterControl.Dock = DockStyle.Fill;
			organisationWizardFilterControl.CurrentItemChanged += OrganisationWizardFilterControl_CurrentItemChanged;
			OrganisationDetailsGridPanel.Controls.Add(organisationWizardFilterControl);
		}

		protected void ShowFiltersOrganisationCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			OrganisationDetailsSplitContainer.Panel1Collapsed = !ShowFiltersOrganisationCheckBox.Checked;
		}

		void OrganisationWizardFilterControl_CurrentItemChanged(object sender, EventArgs e)
		{
			if (selectedLicenceDatabase == null || selectedLicenceDatabase.LD_OH_WebAccessOrg.IsEmpty)
			{
				AddDatabaseDetailsWizardUserControl(sender as EDIOrgHeader);
			}
		}

		#endregion

		#region DatabaseDetailsWizardUserControl

		void AddDatabaseDetailsWizardUserControl(EDIOrgHeader orgHeader)
		{
			if (databaseDetailsWizardUserControl == null)
			{
				databaseDetailsWizardUserControl = new DatabaseDetailsWizardUserControl(new LicenceCompanyLicenceDatabaseCollection(TemporaryLicenceCompany));
				databaseDetailsWizardUserControl.Dock = DockStyle.Fill;
				OrganisationDetailsTableLayoutPanel.Controls.Add(databaseDetailsWizardUserControl, 0, 0);
			}

			var licenceDatabaseCollection = orgHeader?.LicCompany?.LicDatabases ?? new LicenceCompanyLicenceDatabaseCollection(TemporaryLicenceCompany);
			databaseDetailsWizardUserControl.BindingSource.DataSource = licenceDatabaseCollection;
			databaseDetailsWizardUserControl.Visible = orgHeader != null;
		}

		LicenceCompany TemporaryLicenceCompany
		{
			get
			{
				if (temporaryLicenceCompany == null)
				{
					temporaryLicenceCompany = new BusinessObjectFactory().New<LicenceCompany>();
				}
				return temporaryLicenceCompany;
			}
		}
		LicenceCompany temporaryLicenceCompany;

		#endregion

		#region DatabaseOrgSuggestionWizardUserControl

		void AddDatabaseOrgSuggestionWizardUserControl(LicenceDatabase licenceDatabase)
		{
			if (databaseOrgSuggestionWizardUserControl == null)
			{
				databaseOrgSuggestionWizardUserControl = new DatabaseOrgSuggestionWizardUserControl(new EdiLicenceDatabaseOrgSuggestionCollection(TemporaryLicenceDatabase));
				databaseOrgSuggestionWizardUserControl.Dock = DockStyle.Fill;
				databaseOrgSuggestionWizardUserControl.DatabaseOrgSuggestionSelected += (s, e) =>
				{
					var ld = selectedLicenceDatabase;

					if (ld != null && e?.Header != null && ld.LD_OH_WebAccessOrg != e.Header.PK)
					{
						ld.LD_OH_WebAccessOrg = e.Header.PK;
					}
				};
				OrganisationDetailsTableLayoutPanel.Controls.Add(databaseOrgSuggestionWizardUserControl, 0, 1);
			}

			databaseOrgSuggestionWizardUserControl.BindingSource.DataSource = licenceDatabase?.OrgSuggestionCollections ?? new EdiLicenceDatabaseOrgSuggestionCollection(TemporaryLicenceDatabase);
			databaseOrgSuggestionWizardUserControl.Visible = licenceDatabase != null;
		}

		LicenceDatabase TemporaryLicenceDatabase
		{
			get
			{
				if (temporaryLicenceDatabase == null)
				{
					temporaryLicenceDatabase = new BusinessObjectFactory().New<LicenceDatabase>();
				}
				return temporaryLicenceDatabase;
			}
		}
		LicenceDatabase temporaryLicenceDatabase;

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
			saveToolStrip.Items.Add(button);
		}

		protected void SetSelectOrganisationButton_Click(object sender, EventArgs e)
		{
			var orgHeader = organisationWizardFilterControl.Grid.ListManager.GetCurrent() as EDIOrgHeader;
			var licenceDatabase = selectedLicenceDatabase;

			if (licenceDatabase != null && orgHeader != null && licenceDatabase.LD_OH_WebAccessOrg != orgHeader.PK)
			{
				licenceDatabase.LD_OH_WebAccessOrg = orgHeader.PK;
			}
		}

		protected void SaveButton_Click(object sender, EventArgs e)
		{
			if (SaveSelection(true))
			{
				EnableSaveButtons(false);
				licenceDatabaseRegistrationWizardFilterControl.FirePerformSearch();
			}
		}

		void SaveCloseButton_Click(object sender, EventArgs e)
		{
			if (SaveSelection(true))
			{
				Close();
			}
		}

		bool SaveSelection(bool showConfirmationMessage)
		{
			var result = false;

			var changedLicenceDatabases = BusinessEntityWizard.LicenceDatabaseCollection.Cast<LicenceDatabase>().Where(l => l.HasChanges).ToList();
			if (!changedLicenceDatabases.Any())
			{
				return true;
			}

			if (!ValidateErrors(changedLicenceDatabases))
			{
				if (!showConfirmationMessage || ShowConfirmationDialog(changedLicenceDatabases))
				{
					if (ConfigureLicenceDatabase(changedLicenceDatabases))
					{
						ZExceptionReporting.ProcessWithSaveExceptionHandling(BusinessEntityWizard.Factory.Save, null, true);
						LicenceDatabaseMergeContacts(changedLicenceDatabases);
						result = true;
						EnableSaveButtons(false);
					}
				}
			}
			return result;
		}

		protected virtual bool ShowConfirmationDialog(List<LicenceDatabase> licenceDatabases)
		{
			var result = false;

			var caption1 = Res.GetString("8482ED01-3252-42BA-AD1C-1F4019D7F97C", "Save");
			var message1 = Res.GetString("4C130C5D-F2BC-4462-BF8E-D169B562D899", "Would you like to save the changes?");
			if (Globals.Message.Show(message1, caption1, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				if (licenceDatabases.Any(i => i.ShouldCloneContactOnWebAccessOrgChanged))
				{
					var message2 = Res.GetString("63801964-5333-4c41-b365-f43f8b909dee", "You are about to change master organisation. This will affect contacts for MyAccount and eRequest login. Contacts will be copied to another organisation. Are you sure you want to proceed?");
					var caption2 = Res.GetString("66d63cc3-be58-4939-b922-b47f4e3a8d1a", "Change Master Organisation Warning");
					result = UserNotification.Instance.ShowConfirmation(message2, caption2, "confirm", MessageBoxIcon.Warning) == DialogResult.OK;
				}
				else
				{
					result = true;
				}
			}
			return result;
		}

		protected virtual bool ValidateErrors(List<LicenceDatabase> licenceDatabases)
		{
			var result = false;
			var hasErrors = licenceDatabases.Where(l => l.HasErrors);
			if (hasErrors.Any())
			{
				var message = new StringBuilder();
				foreach (var item in hasErrors)
				{
					item.RunPreSaveValidation();
					message.AppendLine(FormattableString.Invariant($"{item.LD_Product} - {item.LD_LicenceType} - {item.LD_ServerCode} ({GetLicenceDatabaseErrorMessage(item)})"));
				}

				result = true;
				UserNotification.Instance.ShowError(Res.GetString("C6770AC8-658E-4E2D-9192-90E55E5A3B68", "The following licence database(s) couldn't be saved:\r\n{0}", message.ToString()));
			}

			return result;
		}

		string GetLicenceDatabaseErrorMessage(LicenceDatabase licenceDatabase)
		{
			var result = string.Empty;

			if (licenceDatabase.HasErrors)
			{
				var webAccessOrgError = licenceDatabase.LD_OH_WebAccessOrgInfo.Notifications.FirstOrDefault(n => n.Type == CargoWise.ComponentModel.NotificationType.Error);
				if (webAccessOrgError != null)
				{
					result = webAccessOrgError.Message;
				}
				else
				{
					result = licenceDatabase.Notifications.First(n => n.Type == CargoWise.ComponentModel.NotificationType.Error).Message;
				}
			}

			return result;
		}

		bool ConfigureLicenceDatabase(List<LicenceDatabase> licenceDatabases)
		{
			foreach (var item in licenceDatabases)
			{
				item.SetEnterpriseServerCode();
				item.CloneContacts(item.ShouldCloneContactOnWebAccessOrgChanged);

				if (item.LD_OH_WebAccessOrgInfo.OriginalValue.IsEmpty)
				{
					item.LD_AllowAutoLogin = true;
					if (!item.IsEnterpriseFamilyDatabase)
					{
						item.LD_Status = DatabaseStatusList.Codes.REG;
					}
				}
			}

			return !ValidateErrors(licenceDatabases);
		}

		void LicenceDatabaseMergeContacts(List<LicenceDatabase> licenceDatabases)
		{
			var message = new StringBuilder();
			foreach (var item in licenceDatabases)
			{
				var errorMessage = item.MergeContactsPerson();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					message.AppendLine(errorMessage);
				}
			}

			if (!string.IsNullOrEmpty(message.ToString()))
			{
				UserNotification.Instance.ShowError(message.ToString(), "Failed to Merge Person");
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			var caption = Res.GetString("7638611E-94C1-4206-9B3D-17F172805C5C", "Cancel");
			var message = Res.GetString("7315B8F7-5580-449C-8B25-AF93A0D5A882", "The pending changes will be discarded, do you wish to proceed?");
			if (DialogResult.Yes == UserNotification.Instance.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No))
			{
				var changed = BusinessEntityWizard.LicenceDatabaseCollection.Cast<LicenceDatabase>().Where(l => l.HasChanges);
				foreach (var item in changed)
				{
					item.CancelChanges();
					item.ClearHasChanges();
					item.WebAccessOrgClearAllNotifications();
					item.LD_OH_WebAccessOrgInfo.RefreshBinding();
				}

				BusinessEntityWizard.CancelChanges();
				BusinessEntityWizard.ClearHasChanges();
				BusinessEntityWizard.ClearNotifications();

				AddDatabaseDetailsWizardUserControl(null);
				AddDatabaseOrgSuggestionWizardUserControl(null);
				licenceDatabaseRegistrationWizardFilterControl.FirePerformSearch();
			}
		}

		void LicenceDatabaseRegistrationWizardForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (BusinessEntityWizard.HasChanges)
			{
				var caption = Res.GetString("BAD2BECA-ABAC-4A79-BF48-C83A4FBF9EFF", "Close");
				var message = Res.GetString("4E6FF497-8BB4-4E68-8C13-BC771F74EE32", "Do you wish to save pending changes before close?");
				if (DialogResult.Yes == UserNotification.Instance.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No))
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
			SaveStripButton.Enabled = enable;
			SaveCloseStripButton.Enabled = enable;
			CancelStripButton.Enabled = enable;
		}

		#endregion

		#region Modules

		protected LicenceDatabaseModuleForRegistration LicenceDatabaseModule
		{
			get
			{
				if (licenceDatabaseModule == null)
				{
					var filterControl = licenceDatabaseRegistrationWizardFilterControl;
					licenceDatabaseModule = new LicenceDatabaseModuleForRegistration(filterControl, filterControl.GridCollection, filterControl.FilterBusinessObject);
					licenceDatabaseModule.ShowNewFormClickEvent += LicenceDatabaseModule_ShowNewFormClickEvent;
				}
				return licenceDatabaseModule;
			}
		}
		LicenceDatabaseModuleForRegistration licenceDatabaseModule;

		protected OrganisationModuleForRegistration OrganisationModule
		{
			get
			{
				if (organisationModule == null)
				{
					var filterControl = organisationWizardFilterControl;
					organisationModule = new OrganisationModuleForRegistration(filterControl, filterControl.GridCollection, filterControl.FilterBusinessObject);
					organisationModule.ShowNewFormClickEvent += OrganisationModule_ShowNewFormClickEvent;
				}
				return organisationModule;
			}
		}
		OrganisationModuleForRegistration organisationModule;

		#endregion

		#region Show New Organisation MenuItem Click

		protected void OrganisationModule_ShowNewFormClickEvent(object sender, EventArgs e)
		{
			if (selectedLicenceDatabase != null)
			{
				if (sender is EDIOrganisationForm form)
				{
					if (form.BusinessEntity is EDIOrgHeader orgHeader)
					{
						orgHeader.OH_FullName = OwnerNameValueLabel.Text;
						orgHeader.MainAddress.OA_Address1 = Address1ValueLabel.Text;
						orgHeader.MainAddress.OA_Address2 = Address2ValueLabel.Text;
						orgHeader.MainAddress.OA_City = CityValueLabel.Text;
						orgHeader.MainAddress.OA_PostCode = PostCodeValueLabel.Text;
						orgHeader.MainAddress.OA_RN_NKCountryCode = CountryValueLabel.Text;
						orgHeader.MainAddress.OA_State = StateValueLabel.Text;
					}
					form.FormClosed += OrganisationForm_Closed;
				}
			}
		}

		protected void OrganisationForm_Closed(object sender, EventArgs e)
		{
			if (selectedLicenceDatabase != null && sender is EDIOrganisationForm form)
			{
				if (form.BusinessEntity.IsInDatabase && form.BusinessEntity is EDIOrgHeader orgHeader)
				{
					selectedLicenceDatabase.LD_OH_WebAccessOrg = orgHeader.PK;
				}
			}
		}

		#endregion

		#region Show New Licence Database MenuItem Click

		protected void LicenceDatabaseModule_ShowNewFormClickEvent(object sender, EventArgs e)
		{
			if (sender is LicenceDatabaseForm licenceDatabaseForm &&
				licenceDatabaseForm.BusinessEntity is LicenceDatabase licenceDatabase)
			{
				licenceDatabase.AllowEditWebAccessOrg = true;
			}
		}

		#endregion

		#region Implementation

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			licenceDatabaseRegistrationWizardFilterControl.Grid.ListManager.CurrentChanged += LicenceDatabaseRegistrationWizardFilterControl_CurrentChanged;

			if (!DesignModeFinder.IsDesigning)
			{
				foreach (var menuItem in LicenceDatabaseModule.FormActionMenu)
				{
					LicenceDatabaseToolStrip.Items.Add(MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(menuItem, LicenceDatabaseModule));
				}

				foreach (var menuItem in OrganisationModule.FormActionMenu)
				{
					OrganisationToolStrip.Items.Add(MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(menuItem, OrganisationModule));
				}
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Code analysis doesnt like the ?. syntax on the dispose calls")]
		protected override void Dispose(bool disposing)
		{
			if (temporaryLicenceCompany != null)
			{
				temporaryLicenceCompany.Delete();
				temporaryLicenceCompany = null;
			}

			if (disposing)
			{
				components?.Dispose();
			}

			PostingButtonsUserControl?.Dispose();
			LicenceDatabaseModule?.Dispose();
			OrganisationModule?.Dispose();
			base.Dispose(disposing);
		}

		#endregion
	}
}


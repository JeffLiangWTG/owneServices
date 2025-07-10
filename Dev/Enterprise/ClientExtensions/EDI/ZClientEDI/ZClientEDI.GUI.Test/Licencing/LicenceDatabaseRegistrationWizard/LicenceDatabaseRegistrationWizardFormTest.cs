using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Core.Environment;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	[TestedType(typeof(LicenceDatabaseRegistrationWizardForm))]
	public class LicenceDatabaseRegistrationWizardFormTest : ZFormBasherTest
	{
		public void TestGetAdicionalInformation()
		{
			var licence = CreateLicenceDatabase(false);
			CreateAdditionalInfoNotes(licence);
			Factory.Save(); // I need to save twice to have notes with different created dates

			Factory.NewWithValidTestData<LicenceDatabase>();
			CreateNotes(licence);
			Factory.Save(); // I need to save twice to have notes with different created dates

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				form.Show();

				// Guarantee the CurrentChanged is triggered and the selected Licence is the one with data
				form.LicenceDatabaseRegistrationWizard_Exposed.Grid.CurrentRowIndex = 1;
				if (form.EnterpriseIdTextBox_Exposed.Text != "ENT123")
				{
					form.LicenceDatabaseRegistrationWizard_Exposed.Grid.CurrentRowIndex = 0;
				}

				AssertEquals("ENT123", form.EnterpriseIdTextBox_Exposed.Text);

				AssertEquals("Org1", form.OwnerNameValueLabel_Exposed.Text);
				AssertEquals("addr1", form.Address1ValueLabel_Exposed.Text);
				AssertEquals("addr2", form.Address2ValueLabel_Exposed.Text);
				AssertEquals("Sydney", form.CityValueLabel_Exposed.Text);
				AssertEquals("NSW", form.StateValueLabel_Exposed.Text);
				AssertEquals("2000", form.PostCodeValueLabel_Exposed.Text);
				AssertEquals("AU", form.CountryValueLabel_Exposed.Text);
			}
		}

		public void TestGetAdicionalInformation_AdditionalInfoNote()
		{
			var licence = CreateLicenceDatabase();
			Factory.Save(); // I need to save twice to have notes with different created dates

			Factory.NewWithValidTestData<LicenceDatabase>();
			CreateAdditionalInfoNotes(licence);
			Factory.Save(); // I need to save twice to have notes with different created dates

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				form.Show();

				// Guarantee the CurrentChanged is triggered and the selected Licence is the one with data
				form.LicenceDatabaseRegistrationWizard_Exposed.Grid.CurrentRowIndex = 1;
				if (form.EnterpriseIdTextBox_Exposed.Text != "ENT123")
				{
					form.LicenceDatabaseRegistrationWizard_Exposed.Grid.CurrentRowIndex = 0;
				}

				AssertEquals("ENT123", form.EnterpriseIdTextBox_Exposed.Text);
				AssertEquals("NewOrg", form.OwnerNameValueLabel_Exposed.Text);
				AssertEquals("NewAddr1", form.Address1ValueLabel_Exposed.Text);
				AssertEquals("NewAddr2", form.Address2ValueLabel_Exposed.Text);
				AssertEquals("Auckland", form.CityValueLabel_Exposed.Text);
				AssertEquals("TEMP", form.StateValueLabel_Exposed.Text);
				AssertEquals("3333", form.PostCodeValueLabel_Exposed.Text);
				AssertEquals("NZ", form.CountryValueLabel_Exposed.Text);
				AssertEquals("CSP", form.ProductValueLabel_Exposed.Text);
				AssertEquals("PGV", form.SystemIDValueLabel_Exposed.Text);
				AssertEquals("#654", form.BusinessRegoNoValueLabel_Exposed.Text);
				AssertEquals("Ten1", form.TenantIdValueLabel_Exposed.Text);
				AssertEquals("ENT123", form.EnterpriseCodeValueLabel_Exposed.Text);
				AssertEquals("CWCode1", form.CargowiseCompanyCodeValueLabel_Exposed.Text);
				AssertEquals("3689", form.DatabaseNumberValueLabel_Exposed.Text);
				AssertEquals("03/11/2020", form.InfoExpiresValueLabel_Exposed.Text);
				AssertEquals("Serv1", form.ServerCodeValueLabel_Exposed.Text);
			}
		}

		public void TestGetAdicionalInformation_GetCountry()
		{
			var licence = CreateLicenceDatabase(false);
			CreateAdditionalInfoNotes(licence);
			Factory.Save(); // I need to save twice to have notes with different created dates

			Factory.NewWithValidTestData<LicenceDatabase>();

			var registration = @"Product:ABU
SystemID:C524897875544
OrgName:Org1
OrgCountry:Australia
Address1:addr1
Address2:addr2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:#123";
			licence.Notes.AddNew(true, EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description, registration);

			Factory.Save(); // I need to save twice to have notes with different created dates

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				form.Show();

				// Guarantee the CurrentChanged is triggered and the selected Licence is the one with data
				form.LicenceDatabaseRegistrationWizard_Exposed.Grid.CurrentRowIndex = 1;
				if (form.EnterpriseIdTextBox_Exposed.Text != "ENT123")
				{
					form.LicenceDatabaseRegistrationWizard_Exposed.Grid.CurrentRowIndex = 0;
				}

				AssertEquals("ENT123", form.EnterpriseIdTextBox_Exposed.Text);

				AssertEquals("Org1", form.OwnerNameValueLabel_Exposed.Text);
				AssertEquals("addr1", form.Address1ValueLabel_Exposed.Text);
				AssertEquals("addr2", form.Address2ValueLabel_Exposed.Text);
				AssertEquals("Sydney", form.CityValueLabel_Exposed.Text);
				AssertEquals("NSW", form.StateValueLabel_Exposed.Text);
				AssertEquals("2000", form.PostCodeValueLabel_Exposed.Text);
				AssertEquals("AU", form.CountryValueLabel_Exposed.Text);
			}
		}

		public void TestSetSelectOrganisationButtonClick()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "TESTORG9";
			org.CreateAndLoadLicenceForOrg();
			org.GenerateNewLicenceCode();
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_OH_WebAccessOrg = ZGuid.Empty;
			licenceDatabase.LD_LE = ZGuid.Empty;

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				form.Show();

				form.ShowFiltersOrganisationCheckBox_Exposed.Checked = true;
				form.ShowFiltersOrganisationCheckBoxCheckedChangedClick();
				form.LicenceDatabaseRegistrationWizard_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.LicenceDatabaseRegistrationWizard_Exposed.Grid.ListManager.GetCurrent() as LicenceDatabase;

				AssertEquals(ZGuid.Empty, current.LD_OH_WebAccessOrg);

				wizard.OrgHeaderCollection.Load(new ZQuery(OrgHeaderSchema.OH_Code, "TESTORG9"));
				form.OrganisationWizardFilterControl_Exposed.Grid.CurrentRowIndex = 0;

				form.PerformSetSelectOrganisationButtonClick();

				AssertEquals(org.PK, current.LD_OH_WebAccessOrg);
			}
		}

		public void TestAddNewOrganisationButton()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_DatabaseNumber = 2508;
			licenceDatabase.LD_OH_WebAccessOrg = ZGuid.Empty;
			licenceDatabase.LD_LE = ZGuid.Empty;
			CreateNotes(licenceDatabase);

			var duplicationFinder = new Mock<ISupportDuplicationFinder>();
			var masterDataProvider = new Mock<IMasterDataProvider>();
			masterDataProvider.Setup(p => p.CreateOrgDuplicationFinder(It.IsAny<OrgHeader>())).Returns(duplicationFinder.Object);
			using (ObjectFactory.Substitute(() => masterDataProvider.Object))
			{
				var wizard = new LicenceDatabaseRegistrationWizard(Factory);
				using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
				{
					form.Show();
					Application.DoEvents();
					UserIdleWorker.Flush();

					form.ShowFiltersOrganisationCheckBox_Exposed.Checked = true;
					form.ShowFiltersOrganisationCheckBoxCheckedChangedClick();
					form.LicenceDatabaseRegistrationWizard_Exposed.FilteredGrid.CurrentRowIndex = 0;

					var current = form.LicenceDatabaseRegistrationWizard_Exposed.Grid.ListManager.GetCurrent() as LicenceDatabase;
					AssertEquals(ZGuid.Empty, current.LD_OH_WebAccessOrg);
					AssertEquals(2508, current.LD_DatabaseNumber);

					var newFactory = new BusinessObjectFactory();
					var orgHeader = newFactory.NewWithValidTestData<EDIOrgHeader>();
					orgHeader.CreateAndLoadLicenceForOrg();
					orgHeader.GenerateNewLicenceCode();
					orgHeader.LicEnterprise.LE_EnterpriseID = "TEMP25";

					using (var organisationForm = new EDIOrganisationForm(orgHeader, null))
					{
						Application.DoEvents();
						UserIdleWorker.Flush();

						form.OrganisationModuleShowNewFormClickEvent_Exposed(organisationForm);

						var orgFormBizo = organisationForm.BusinessEntity as EDIOrgHeader;

						AssertEquals("Org1", orgFormBizo.OH_FullName);
						AssertEquals("addr1", orgFormBizo.MainAddress.OA_Address1);
						AssertEquals("addr2", orgFormBizo.MainAddress.OA_Address2);
						AssertEquals("Sydney", orgFormBizo.MainAddress.OA_City);
						AssertEquals("2000", orgFormBizo.MainAddress.OA_PostCode);
						AssertEquals("AU", orgFormBizo.MainAddress.OA_RN_NKCountryCode);
						AssertEquals("NSW", orgFormBizo.MainAddress.OA_State);

						duplicationFinder.Verify(p => p.FindDuplicates());

						orgFormBizo.Factory.Save();
						Application.DoEvents();

						orgHeader.CancelNonStartedTasksAndClosePartiallyCompletedTasks();
						form.OrganisationFormClosed_Exposed(organisationForm);
					}

					AssertEquals(orgHeader.PK, current.LD_OH_WebAccessOrg);
					orgHeader.CancelNonStartedTasksAndClosePartiallyCompletedTasks();
				}
			}

			Application.DoEvents();
			UserIdleWorker.Flush();
		}

		public void TestWebAccessOrgGuidFindBoxReadOnly()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "TESTORG9";
			org.CreateAndLoadLicenceForOrg();
			org.GenerateNewLicenceCode();
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_OH_WebAccessOrg = ZGuid.Empty;
			licenceDatabase.LD_LE = ZGuid.Empty;

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				form.Show();

				form.ShowFiltersOrganisationCheckBox_Exposed.Checked = true;
				form.ShowFiltersOrganisationCheckBoxCheckedChangedClick();
				form.LicenceDatabaseRegistrationWizard_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.LicenceDatabaseRegistrationWizard_Exposed.Grid.ListManager.GetCurrent() as LicenceDatabase;

				AssertEquals(ZGuid.Empty, current.LD_OH_WebAccessOrg);
				AssertEquals(false, form.WebAccessOrgGuidFindBox_Exposed.ReadOnly);
			}
		}

		public void TestLicenceDatabaseUpdateMasterOrg()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "TESTORG9";
			org.CreateAndLoadLicenceForOrg();
			org.GenerateNewLicenceCode();

			var newOrganisation = Factory.NewWithValidTestData<EDIOrgHeader>();
			newOrganisation.OH_Code = "NEWTEST";
			newOrganisation.CreateAndLoadLicenceForOrg();
			newOrganisation.GenerateNewLicenceCode();

			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_OH_WebAccessOrg = newOrganisation.PK;
			licenceDatabase.LD_LE = newOrganisation.LicEnterprise.PK;

			Factory.Save();

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				form.Show();

				form.ShowFiltersOrganisationCheckBox_Exposed.Checked = true;
				form.ShowFiltersOrganisationCheckBoxCheckedChangedClick();
				form.LicenceDatabaseRegistrationWizard_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.LicenceDatabaseRegistrationWizard_Exposed.Grid.ListManager.GetCurrent() as LicenceDatabase;

				wizard.OrgHeaderCollection.Load(new ZQuery(OrgHeaderSchema.OH_Code, "TESTORG9"));
				form.OrganisationWizardFilterControl_Exposed.Grid.CurrentRowIndex = 0;

				current.ResumeValidation();
				form.PerformSetSelectOrganisationButtonClick();

				form.SaveButtonClick_Exposed();
				AssertHasWarning(licenceDatabase.LD_OH_WebAccessOrgInfo, "The selected Master Org doesn't have relationship with the database");
				Assert("We should be able to save with a Master Org warning", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Would you like to save the changes?"));
			}
		}

		public void TestAddNewLicenceDatabaseButton()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				form.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				form.ShowFiltersLicenceDatabaseCheckBox_Exposed.Checked = true;
				form.ShowFiltersLicenceDatabaseCheckBoxCheckedChangedClick();

				AssertEquals(false, licenceDatabase.AllowEditWebAccessOrg);

				using (var licenceDatabaseForm = new LicenceDatabaseForm(licenceDatabase, null))
				{
					Application.DoEvents();
					UserIdleWorker.Flush();

					form.LicenceDatabaseModule_ShowNewFormClickEvent_Exposed(licenceDatabaseForm);

					var bizo = licenceDatabaseForm.BusinessEntity as LicenceDatabase;
					AssertEquals(true, bizo.AllowEditWebAccessOrg);
				}
			}

			Application.DoEvents();
			UserIdleWorker.Flush();
		}

		public void TestWebAccessOrg_CloneContact()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence1.Database;

			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			database.LD_StaffFirstReportUtc = new ZDateTime(2019, 1, 1);
			database.LD_OH_WebAccessOrg = org1.PK;

			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";
			contact1a.OC_WebAccessEnabled = true;
			contact1a.SetHashedPassword("123456");
			contact1a.OC_OA_OrgAddress = org1.MainAddress.PK;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.OH_Code = "TESTORG9";
			org2.Contacts.RemoveAndDeleteAll();

			var licence3 = BillingTestHelper.CreateLicence(Factory, "EEE", "TTT", "SYD");
			var org3 = licence3.Company.Header;
			org3.Contacts.RemoveAndDeleteAll();

			Factory.Save();

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				form.Show();

				form.ShowFiltersOrganisationCheckBox_Exposed.Checked = true;
				form.ShowFiltersOrganisationCheckBoxCheckedChangedClick();

				wizard.LicenceDatabaseCollection.Load(new ZQuery(LicenceDatabaseSchema.PK, database.PK));

				form.LicenceDatabaseRegistrationWizard_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.LicenceDatabaseRegistrationWizard_Exposed.Grid.ListManager.GetCurrent() as LicenceDatabase;

				AssertEquals(org1.PK, current.LD_OH_WebAccessOrg);

				wizard.OrgHeaderCollection.Load(new ZQuery(OrgHeaderSchema.OH_Code, "TESTORG9"));
				form.OrganisationWizardFilterControl_Exposed.Grid.CurrentRowIndex = 0;

				form.PerformSetSelectOrganisationButtonClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals(org2.PK, current.LD_OH_WebAccessOrg);
				AssertEquals(0, org2.Contacts.Count);
				form.SaveButtonClick_Exposed();

				var dialog = (UserConfirmationDialog)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals("You are about to change master organisation. This will affect contacts for MyAccount and eRequest login. Contacts will be copied to another organisation. Are you sure you want to proceed?", dialog.MessageMultilingual.ToString());

				AssertEquals("Contact is cloned to new web access org", 1, org2.Contacts.Count);
				var contact2a = org2.Contacts[0];
				AssertEquals(true, contact2a.OC_IsActive);
				AssertEquals("User One", contact2a.OC_ContactName);
				AssertEquals("user.one@test.org", contact2a.OC_Email);
				AssertEquals(contact2a.PK, userAccount1.EUA_OC_WebAccessContact);
			}
		}

		public void TestSaveErrorMessage()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "TESTORG9";
			org.CreateAndLoadLicenceForOrg();
			org.GenerateNewLicenceCode();
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_OH_WebAccessOrg = org.PK;
			licenceDatabase.LD_LE = org.LicEnterprise.PK;
			Factory.Save();

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				form.Show();

				form.ShowFiltersOrganisationCheckBox_Exposed.Checked = true;
				form.ShowFiltersOrganisationCheckBoxCheckedChangedClick();
				form.LicenceDatabaseRegistrationWizard_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.LicenceDatabaseRegistrationWizard_Exposed.Grid.ListManager.GetCurrent() as LicenceDatabase;

				current.LD_OH_WebAccessOrg = new Guid("9e8dca76-5555-4444-0000-e98b88f07d9f");

				AssertExceptionThrown(typeof(ZSaveException), () => form.SaveButtonClick_Exposed());
			}
		}

		public void TestGridSelectionChange_ClearFormData()
		{
			var licenceDatabase = CreateLicenceDatabase();
			Factory.Save();

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				form.Show();

				form.LicenceDatabaseRegistrationWizardFilterControl_Exposed.FirePerformSearch();
				form.LicenceDatabaseRegistrationWizardFilterControl_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.LicenceDatabaseRegistrationWizardFilterControl_Exposed.Grid.ListManager.GetCurrent() as LicenceDatabase;
				AssertEquals(licenceDatabase.WebAccessOrg.PK, current.LD_OH_WebAccessOrg);
				AssertEquals("ENT123", form.EnterpriseIdTextBox_Exposed.Text);
				AssertEquals("Org1", form.OwnerNameValueLabel_Exposed.Text);
				AssertEquals("addr1", form.Address1ValueLabel_Exposed.Text);
				AssertEquals("addr2", form.Address2ValueLabel_Exposed.Text);
				AssertEquals("Sydney", form.CityValueLabel_Exposed.Text);
				AssertEquals("NSW", form.StateValueLabel_Exposed.Text);
				AssertEquals("2000", form.PostCodeValueLabel_Exposed.Text);
				AssertEquals("AU", form.CountryValueLabel_Exposed.Text);
				AssertEquals(licenceDatabase.WebAccessOrg.OH_Code, form.WebAccessOrgGuidFindBox_Exposed.CodeBox.Text);

				licenceDatabase.Delete();

				form.LicenceDatabaseRegistrationWizardFilterControl_Exposed.FirePerformSearch();

				AssertEquals(string.Empty, form.EnterpriseIdTextBox_Exposed.Text);
				AssertEquals(string.Empty, form.OwnerNameValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.Address1ValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.Address2ValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.CityValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.StateValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.PostCodeValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.CountryValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.WebAccessOrgGuidFindBox_Exposed.CodeBox.Text);
			}
		}

		public void TestSetAllowAutoLoginAndStatus()
		{
			var collection = new SystemProductCollection();
			collection.AddNew(ProductTypes.Codes.GLOW, "test", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "TESTORG9";
			org.CreateAndLoadLicenceForOrg();
			org.GenerateNewLicenceCode();

			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_Product = ProductTypes.Codes.GLOW;
			licenceDatabase.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Blocked;
			licenceDatabase.LD_OH_WebAccessOrg = ZGuid.Empty;
			licenceDatabase.LD_AllowAutoLogin = false;
			licenceDatabase.LD_Status = string.Empty;

			Factory.Save();

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				form.IgnoreErrorValidation = true;
				form.Show();

				form.ShowFiltersOrganisationCheckBox_Exposed.Checked = true;
				form.ShowFiltersOrganisationCheckBoxCheckedChangedClick();
				form.LicenceDatabaseRegistrationWizard_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.LicenceDatabaseRegistrationWizard_Exposed.Grid.ListManager.GetCurrent() as LicenceDatabase;

				AssertEquals(ZGuid.Empty, current.LD_OH_WebAccessOrg);
				AssertEquals(false, licenceDatabase.LD_AllowAutoLogin);
				AssertEquals(string.Empty, licenceDatabase.LD_Status);

				wizard.OrgHeaderCollection.Load(new ZQuery(OrgHeaderSchema.OH_Code, "TESTORG9"));
				form.OrganisationWizardFilterControl_Exposed.Grid.CurrentRowIndex = 0;

				form.PerformSetSelectOrganisationButtonClick();

				AssertEquals(org.PK, current.LD_OH_WebAccessOrg);

				form.SaveButtonClick_Exposed();
				AssertEquals(true, licenceDatabase.LD_AllowAutoLogin);
				AssertEquals(DatabaseStatusList.Codes.REG, licenceDatabase.LD_Status);
			}
		}

		public void TestSetAllowAutoLoginAndStatus_IsEnterpriseOrCargoWiseOneDatabase()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "TESTORG9";
			org.CreateAndLoadLicenceForOrg();
			org.GenerateNewLicenceCode();
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_OH_WebAccessOrg = ZGuid.Empty;
			licenceDatabase.LD_AllowAutoLogin = false;
			licenceDatabase.LD_Status = string.Empty;
			Factory.Save();

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				form.IgnoreErrorValidation = true;
				form.Show();

				form.ShowFiltersOrganisationCheckBox_Exposed.Checked = true;
				form.ShowFiltersOrganisationCheckBoxCheckedChangedClick();
				form.LicenceDatabaseRegistrationWizard_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.LicenceDatabaseRegistrationWizard_Exposed.Grid.ListManager.GetCurrent() as LicenceDatabase;

				AssertEquals(ZGuid.Empty, current.LD_OH_WebAccessOrg);
				AssertEquals(false, licenceDatabase.LD_AllowAutoLogin);
				AssertEquals(string.Empty, licenceDatabase.LD_Status);

				wizard.OrgHeaderCollection.Load(new ZQuery(OrgHeaderSchema.OH_Code, "TESTORG9"));
				form.OrganisationWizardFilterControl_Exposed.Grid.CurrentRowIndex = 0;

				form.PerformSetSelectOrganisationButtonClick();

				AssertEquals(org.PK, current.LD_OH_WebAccessOrg);

				form.SaveButtonClick_Exposed();
				AssertEquals(true, licenceDatabase.LD_AllowAutoLogin);
				AssertEquals("", licenceDatabase.LD_Status);
			}
		}

		public void TestMasterOrganisationGrid_ShowLicenceDatabase()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "TESTORG9";
			org.CreateAndLoadLicenceForOrg();
			org.GenerateNewLicenceCode();
			var database1 = org.LicCompany.LicDatabases.AddNew();
			database1.LD_DatabaseNumber = 2020;

			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_DatabaseNumber = 1010;
			licenceDatabase.LD_OH_WebAccessOrg = ZGuid.Empty;
			licenceDatabase.LD_LE = ZGuid.Empty;

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				form.DatabaseDetailsWizardUserControl_Exposed.Visible = true;
				form.Show();

				form.ShowFiltersOrganisationCheckBox_Exposed.Checked = true;
				form.ShowFiltersOrganisationCheckBoxCheckedChangedClick();

				wizard.LicenceDatabaseCollection.Load(new ZQuery(LicenceDatabaseSchema.LD_DatabaseNumber, 1010));
				form.LicenceDatabaseRegistrationWizard_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.LicenceDatabaseRegistrationWizard_Exposed.Grid.ListManager.GetCurrent() as LicenceDatabase;

				var licenceDatabaseCollection = form.DatabaseDetailsWizardUserControl_Exposed.LicenceGrid.ListManager.List as LicenceCompanyLicenceDatabaseCollection;

				AssertEquals(0, licenceDatabaseCollection.Count);
				AssertEquals(ZGuid.Empty, current.LD_OH_WebAccessOrg);

				wizard.OrgHeaderCollection.Load(new ZQuery(OrgHeaderSchema.OH_Code, "TESTORG9"));
				form.OrganisationWizardFilterControl_Exposed.Grid.CurrentRowIndex = 0;

				licenceDatabaseCollection = form.DatabaseDetailsWizardUserControl_Exposed.LicenceGrid.ListManager.List as LicenceCompanyLicenceDatabaseCollection;

				AssertEquals(1, licenceDatabaseCollection.Count);
				AssertEquals(2020, licenceDatabaseCollection[0].LD_DatabaseNumber);
				form.DatabaseDetailsWizardUserControl_Exposed.Dispose();
			}
		}

		public void TestOrgDetailSplitContainerControlsInCorrectOrder()
		{
			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new LicenceDatabaseRegistrationWizardFormForTest(wizard))
			{
				var foundNonFill = false;
				foreach (Control control in form.OrganisationDetailsSplitContainer_Exposed.Panel1.Controls)
				{
					if (control.Dock == DockStyle.Fill)
					{
						Assert("Fill dockstyle controls should be added before any non-fill dockstyle controls", !foundNonFill);
						continue;
					}
					foundNonFill = true;
				}
			}
			Assert("Passed test", true);
		}

		LicenceDatabase CreateLicenceDatabase(bool createNotes = true)
		{
			var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgHeader.CreateAndLoadLicenceForOrg();
			orgHeader.GenerateNewLicenceCode();
			orgHeader.LicEnterprise.LE_EnterpriseID = "ENT123";

			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_OH_WebAccessOrg = orgHeader.PK;
			licenceDatabase.LD_LE = orgHeader.LicEnterprise.PK;

			if (createNotes)
			{
				CreateNotes(licenceDatabase);
			}

			return licenceDatabase;
		}

		void CreateNotes(LicenceDatabase licenceDatabase)
		{
			var registration = @"Product:ABU
SystemID:C524897875544
OrgName:Org1
OrgCountry:AU
Address1:addr1
Address2:addr2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:#123";
			licenceDatabase.Notes.AddNew(true, EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description, registration);
		}

		void CreateAdditionalInfoNotes(LicenceDatabase licenceDatabase)
		{
			var registration = @"Product:CSP
SystemID:PGV
OrgName:NewOrg
OrgCountry:NZ
Address1:NewAddr1
Address2:NewAddr2
City:Auckland
Postcode:3333
State:TEMP
BusinessRegoNo:#654
TenantId:Ten1
EnterpriseCode:ENT123
CargowiseCompanyCode:CWCode1
DatabaseNumber:3689
InfoExpires:03/11/2020
EnterpriseID:EntId1
ServerCode:Serv1";
			licenceDatabase.Notes.AddNew(true, EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationAdditionalInfoNote.Description, registration);
		}

		#region Implementation

		public override void TestBashingForm()
		{
			Assert(true); //custom form
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert(true); //custom form
		}

		protected override Form GetFormToBashCore()
		{
			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			return new LicenceDatabaseRegistrationWizardForm(wizard);
		}

		class LicenceDatabaseRegistrationWizardFormForTest : LicenceDatabaseRegistrationWizardForm
		{
			public bool IgnoreErrorValidation { get; set; }

			public LicenceDatabaseRegistrationWizardFormForTest(LicenceDatabaseRegistrationWizard licenceDatabaseRegistrationWizard)
				: base(licenceDatabaseRegistrationWizard)
			{
			}

			public LicenceDatabaseRegistrationWizardFilterControl LicenceDatabaseRegistrationWizard_Exposed
						=> licenceDatabaseRegistrationWizardFilterControl;

			public OrganisationWizardFilterControl OrganisationWizardFilterControl_Exposed
						=> organisationWizardFilterControl;

			public DatabaseDetailsWizardUserControl DatabaseDetailsWizardUserControl_Exposed
						=> databaseDetailsWizardUserControl;

			public CargoWise.Windows.UI.KSplitContainer OrganisationDetailsSplitContainer_Exposed
						=> OrganisationDetailsSplitContainer;

			public void PerformSetSelectOrganisationButtonClick()
			{
				SetSelectOrganisationButton_Click(null, new EventArgs());
			}

			public void ShowFiltersOrganisationCheckBoxCheckedChangedClick()
			{
				ShowFiltersOrganisationCheckBox_CheckedChanged(null, null);
			}

			public void ShowFiltersLicenceDatabaseCheckBoxCheckedChangedClick()
			{
				ShowFiltersLicenceDatabaseCheckBox_CheckedChanged(null, null);
			}

			public void OrganisationModuleShowNewFormClickEvent_Exposed(EDIOrganisationForm form)
			{
				OrganisationModule_ShowNewFormClickEvent(form, null);
			}

			public void OrganisationFormClosed_Exposed(EDIOrganisationForm form)
			{
				OrganisationForm_Closed(form, null);
			}

			public void LicenceDatabaseModule_ShowNewFormClickEvent_Exposed(LicenceDatabaseForm form)
			{
				LicenceDatabaseModule_ShowNewFormClickEvent(form, null);
			}

			public void SaveButtonClick_Exposed()
			{
				SaveButton_Click(null, null);
			}

			protected override bool ShowConfirmationDialog(List<LicenceDatabase> licenceDatabases)
			{
				base.ShowConfirmationDialog(licenceDatabases);
				return true;
			}

			protected override bool ValidateErrors(List<LicenceDatabase> licenceDatabases)
			{
				if (IgnoreErrorValidation)
				{
					return false;
				}
				return base.ValidateErrors(licenceDatabases);
			}

			public ZArchitecture.ZTextBox EnterpriseIdTextBox_Exposed => EnterpriseIdTextBox;
			public ZArchitecture.ZLabel OwnerNameValueLabel_Exposed => OwnerNameValueLabel;
			public ZArchitecture.ZLabel Address1ValueLabel_Exposed => Address1ValueLabel;
			public ZArchitecture.ZLabel Address2ValueLabel_Exposed => Address2ValueLabel;
			public ZArchitecture.ZLabel CityValueLabel_Exposed => CityValueLabel;
			public ZArchitecture.ZLabel StateValueLabel_Exposed => StateValueLabel;
			public ZArchitecture.ZLabel PostCodeValueLabel_Exposed => PostCodeValueLabel;
			public ZArchitecture.ZLabel CountryValueLabel_Exposed => CountryValueLabel;
			public ZCheckBox ShowFiltersOrganisationCheckBox_Exposed => ShowFiltersOrganisationCheckBox;
			public ZCheckBox ShowFiltersLicenceDatabaseCheckBox_Exposed => ShowFiltersLicenceDatabaseCheckBox;
			public ZGuidFindBox WebAccessOrgGuidFindBox_Exposed => WebAccessOrgGuidFindBox;
			public LicenceDatabaseRegistrationWizardFilterControl LicenceDatabaseRegistrationWizardFilterControl_Exposed => licenceDatabaseRegistrationWizardFilterControl;
			public ZArchitecture.ZLabel ProductValueLabel_Exposed => ProductValueLabel;
			public ZArchitecture.ZLabel SystemIDValueLabel_Exposed => SystemIdValueLabel;
			public ZArchitecture.ZLabel BusinessRegoNoValueLabel_Exposed => BusinessRegoNoValueLabel;
			public ZArchitecture.ZLabel TenantIdValueLabel_Exposed => TenantIdValueLabel;
			public ZArchitecture.ZLabel EnterpriseCodeValueLabel_Exposed => EnterpriseCodeValueLabel;
			public ZArchitecture.ZLabel CargowiseCompanyCodeValueLabel_Exposed => CargowiseCompanyCodeValueLabel;
			public ZArchitecture.ZLabel DatabaseNumberValueLabel_Exposed => DatabaseNumberValueLabel;
			public ZArchitecture.ZLabel InfoExpiresValueLabel_Exposed => InfoExpiresValueLabel;
			public ZArchitecture.ZLabel ServerCodeValueLabel_Exposed => ServerCodeValueLabel;
		}

		#endregion
	}
}

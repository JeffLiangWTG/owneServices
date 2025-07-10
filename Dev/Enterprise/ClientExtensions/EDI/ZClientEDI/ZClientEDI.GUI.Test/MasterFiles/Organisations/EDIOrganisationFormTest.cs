using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	[TestedType(typeof(EDIOrganisationFormForTest))]
	public class EDIOrganisationFormTest : ZFormBasherTest
	{
		#region Form For Test

		public class EDIOrganisationFormForTest : EDIOrganisationForm
		{
			public EDIOrganisationFormForTest(EDIOrgHeader organisation, IEdiOrgViewController viewController = null)
				: base(organisation, viewController)
			{
			}

			public ZTemplateTabControl OrgTabControl
			{
				get { return OrganisationsTabControl; }
			}

			public ZTemplateTabControl OrgDetailsTabControl
			{
				get => DetailsControl.FindSingle<ZTemplateTabControl>("DetailsTabControl");
			}

			public ZTabPage FindOrgDetailsTab(string name)
			{
				return OrgDetailsTabControl?.TabPages.Cast<ZTabPage>().FirstOrDefault(x => x.Name == name);
			}

			protected override DialogResult ShowConfirmationForDelete()
			{
				return DialogResult.OK;
			}

			public string GSTRegisteredChangedForTest = GSTRegisteredChanged;

			public void Call_OnMoveLicencesToNewEnterpriseCode()
			{
				this.OnMoveLicencesToNewEnterpriseCode(this, new EventArgs());
			}

			public void Call_OnCreateLicenceEnterprise()
			{
				this.OnCreateLicenceEnterprise(this, new EventArgs());
			}
		}

		#endregion

		#region EDIOrgHeader For Test

		public EDIOrgHeader GetOrgHeaderForFormBasher()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();

			using (org.SuspendSettingHasChanges())
			using (org.CompanyData.SuspendSettingHasChanges())
			using (org.MiscServ.SuspendSettingHasChanges())
			using (org.MainAddress.SuspendSettingHasChanges())
			{
				org.OH_IsCreditor = true;
				org.OH_IsDebtor = true;
				org.OH_IsConsignee = true;
				org.OH_IsConsignor = true;
				org.OH_IsTransportClient = true;
				org.OH_IsWarehouseClient = true;
				org.OH_IsShippingProvider = true;
				org.OH_IsForwarder = true;
				org.OH_IsBroker = true;
				org.OH_IsMiscFreightServices = true;
				org.OH_IsCompetitor = true;
				org.OH_IsSalesLead = true;

				org.OH_Code = "XXX";
				org.OH_RL_NKClosestPort = "AUSYD";
			}
			return org;
		}

		#endregion

		public override void TestBashingForm()
		{
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				base.TestBashingForm();
			}
		}

		public override void TestMinimumSizeNotTooBig()
		{
			string formName;
			using (Form testForm = GetFormToBash())
			{
				formName = testForm.Name;

				int minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1200);
				int minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(768);
				int typicalTaskbarHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(43);

				int maxSizeWidth = minScreenWidthSupported;
				int maxSizeHeight = minScreenHeightSupported - typicalTaskbarHeight;

				Assert("Form min size too wide (" + testForm.MinimumSize.Width.ToString() + ") for the screen. Should be less than or equal to " + maxSizeWidth.ToString(), testForm.MinimumSize.Width <= maxSizeWidth);
				Assert("Form min size too high (" + testForm.MinimumSize.Height.ToString() + ") for the screen. Should be less than or equal to " + maxSizeHeight.ToString(), testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}

		protected override Form GetFormToBashCore()
		{
			EDIOrgHeader org = GetOrgHeaderForFormBasher();
			org.CreateAndLoadLicenceForOrg();
			return new EDIOrganisationFormForTest(org);
		}

		protected override bool AllowHasChangesOnFormOpen
		{
			get { return true; }
		}

		public void TestOrderOfTabPages()
		{
			EDIOrgHeader org = GetOrgHeaderForFormBasher();
			EDIOrganisationFormForTest form;
			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (form = new EDIOrganisationFormForTest(org))
			{
				form.Show();

				AssertEquals("OrganisationTabControl.TabPages[0]", "DetailsTabPage", form.OrgTabControl.TabPages[0].Name);
				AssertEquals("OrganisationTabControl.TabPages[1]", "AddressesTabPage", form.OrgTabControl.TabPages[1].Name);
				AssertEquals("OrganisationTabControl.TabPages[2]", "ContactsTabPage", form.OrgTabControl.TabPages[2].Name);
				AssertEquals("OrganisationTabControl.TabPages[3]", "ReceivablesTabPage", form.OrgTabControl.TabPages[3].Name);
				AssertEquals("OrganisationTabControl.TabPages[4]", "PayablesTabPage", form.OrgTabControl.TabPages[4].Name);
				AssertEquals("OrganisationTabControl.TabPages[5]", "ConsignorTabPage", form.OrgTabControl.TabPages[5].Name);
				AssertEquals("OrganisationTabControl.TabPages[6]", "ConsigneeTabPage", form.OrgTabControl.TabPages[6].Name);
				AssertEquals("OrganisationTabControl.TabPages[7]", "WhsFacilityTabPage", form.OrgTabControl.TabPages[7].Name);
				AssertEquals("OrganisationTabControl.TabPages[8]", "ForwarderTabPage", form.OrgTabControl.TabPages[8].Name);
				AssertEquals("OrganisationTabControl.TabPages[9]", "TransportTabPage", form.OrgTabControl.TabPages[9].Name);
				AssertEquals("OrganisationTabControl.TabPages[11]", "MiscServicesTabPage", form.OrgTabControl.TabPages[10].Name);
				AssertEquals("OrganisationTabControl.TabPages[12]", "SalesTabPage", form.OrgTabControl.TabPages[11].Name);
				AssertEquals("OrganisationTabControl.TabPages[13]", "CompetitorTabPage", form.OrgTabControl.TabPages[12].Name);
				AssertEquals("OrganisationTabControl.TabPages[14]", "WorkflowTabPage", form.OrgTabControl.TabPages[13].Name);
				AssertEquals("OrganisationTabControl.TabPages[15]", "UserDefinedTabPage", form.OrgTabControl.TabPages[14].Name);
				AssertEquals("OrganisationTabControl.TabPages[16]", "LicenceKeyBuilderTabPage", form.OrgTabControl.TabPages[15].Name);
			}
		}

		public void TestCreateLicence()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			Factory.Save();
			EDIOrganisationFormForTest formForTest;
			using (formForTest = new EDIOrganisationFormForTest(testHeader))
			{
				formForTest.Show();

				AssertNull("Licence Enterprise is null before creation", testHeader.LicEnterprise);
				AssertNull("Licence Company is null before creation", testHeader.LicCompany);

				formForTest.OrgTabControl.SelectedTab = (ZTabPage)formForTest.OrgTabControl.TabPages[16];

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				formForTest.Call_OnCreateLicenceEnterprise();

				AssertNotNull("Licence Enterprise created", testHeader.LicEnterprise);
				AssertNotNull("Licence Company created", testHeader.LicCompany);

				AssertEquals("Generate New Licence Code Shows to User on creating new licence enterprise", "Create New Licence", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		public void TestGenerateTokenButtonIsInvisible()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = testHeader.PK;
			Factory.Save();
			using (var form = new EDIOrganisationFormForTest(testHeader))
			{
				form.Show();
				var licenseTabPage = form.Controls.Find("LicenceKeyBuilderTabPage", true).FirstOrDefault() as ZTabPage;
				form.OrgTabControl.SelectedTab = licenseTabPage;
				var generateButton = (ZButton)licenseTabPage.Controls.Find("GenerateLoginTokenButton", true).FirstOrDefault();
				AssertNotNull(generateButton);
				AssertEquals(false, generateButton.Visible);
			}
		}

		void ShowPopupMenu(MenuItem menuItem)
		{
			var popupMethod = menuItem.GetType().GetMethod("OnPopup", BindingFlags.Instance | BindingFlags.NonPublic);
			popupMethod.Invoke(menuItem, new object[] { EventArgs.Empty });
		}

		public void TestSyncLicenceCompanyCountry()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org = HeaderForTest;
			org.OH_RL_NKClosestPort = "USCHI";
			Factory.Save();

			using (var form = new EDIOrganisationFormForTest(org))
			{
				form.Show();
				var actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				ShowPopupMenu(actionsMenuItem);
				AssertEquals("No licence so disabled", false, actionsMenuItem.MenuItems.FindByText("Sync Licence Company Country").Enabled);
			}

			org.CreateAndLoadLicenceForOrg();
			Factory.Save();
			org.OH_RL_NKClosestPort = "AUSYD";

			using (var form = new EDIOrganisationFormForTest(org))
			{
				form.Show();

				var actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				ShowPopupMenu(actionsMenuItem);
				var syncMenuItem = actionsMenuItem.MenuItems.FindByText("Sync Licence Company Country");
				AssertEquals("Not synced so enabled", true, syncMenuItem.Enabled);

				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = false;
				syncMenuItem.PerformClick();
				AssertEquals(EDISecurityCheckpoints.OrgLicenceModify.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = true;

				AssertEquals("Precondition", "US", org.LicCompany.LC_CompanyCountry);
				AssertEquals("Precondition", false, org.LicCompany.LC_IsGSTRegistered);
				AssertEquals("Precondition", "USD", org.LicCompany.LC_RX_NKCurrency);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				syncMenuItem.PerformClick();

				AssertEquals("US", org.LicCompany.LC_CompanyCountry);
				AssertEquals(false, org.LicCompany.LC_IsGSTRegistered);
				AssertEquals("USD", org.LicCompany.LC_RX_NKCurrency);
				AssertEquals("Sync Licence Company Country", UnitTestUserNotification.Instance.LastMessage.Caption);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				syncMenuItem.PerformClick();

				AssertEquals("Synced country", "AU", org.LicCompany.LC_CompanyCountry);
				AssertEquals("Updated IsGSTRegistered", true, org.LicCompany.LC_IsGSTRegistered);
				AssertEquals("Updated Currency", "AUD", org.LicCompany.LC_RX_NKCurrency);
				AssertEquals("Sync Licence Company Country", UnitTestUserNotification.Instance.LastMessage.Caption);

				actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				ShowPopupMenu(actionsMenuItem);
				AssertEquals("Synced now so should be disabled", false, actionsMenuItem.MenuItems.FindByText("Sync Licence Company Country").Enabled);
			}

			Factory.Save();

			using (var form = new EDIOrganisationFormForTest(org))
			{
				form.Show();
				var actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				ShowPopupMenu(actionsMenuItem);
				AssertEquals("Synced so disabled", false, actionsMenuItem.MenuItems.FindByText("Sync Licence Company Country").Enabled);
			}
		}

		[ExpectNoExceptions]
		public void TestGenerateLicenceTab()
		{
			EDIOrgHeader testHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();
			testHeader.CreateAndLoadLicenceForOrg();
			Factory.Save();
			testHeader.LicenceEnterpriseCode = "AAA";
			Factory.Save();
			EDIOrganisationFormForTest formForTest;

			using (formForTest = new EDIOrganisationFormForTest(testHeader))
			{
				formForTest.Show();
				formForTest.OrgTabControl.SelectedTab = (ZTabPage)formForTest.OrgTabControl.TabPages[15];
				testHeader.LicenceEnterpriseCode = "";
				testHeader.LicenceEnterpriseCode = "AA0";
				Factory.Save();
			}
		}

		public void TestNotChangingGSTRegistrationDoesNotPromptUser()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_RL_NKClosestPort = "MYBAL";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.LicenceEnterpriseCode = "AAA";
			testHeader.OH_IsSalesLead = true;
			testHeader.MainAddress.OA_PostCode = "123";

			Factory.Save();

			using (EDIOrganisationFormForTest formForTest = new EDIOrganisationFormForTest(testHeader))
			{
				formForTest.Show();
				formForTest.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedTab = (ZTabPage)formForTest.OrgTabControl.TabPages[15];

				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				formForTest.ButtonsUserControl.SaveButton.PerformClick();
				AssertNull("Last Message Shown", Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should be saved", !testHeader.HasChanges);
			}
		}

		public void TestChangingGSTRegistrationAndDoNotSave()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_RL_NKClosestPort = "MYBAL";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.LicenceEnterpriseCode = "AAA";
			testHeader.MainAddress.OA_PostCode = "123";
			Factory.Save();

			using (EDIOrganisationFormForTest formForTest = new EDIOrganisationFormForTest(testHeader))
			{
				formForTest.Show();
				formForTest.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedTab = (ZTabPage)formForTest.OrgTabControl.TabPages[15];
				testHeader.LicCompany.LC_IsGSTRegistered = !testHeader.LicCompany.LC_IsGSTRegistered;

				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				formForTest.ButtonsUserControl.SaveButton.PerformClick();
				AssertEquals("Last Message Shown", formForTest.GSTRegisteredChangedForTest, Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should be unsaved", testHeader.HasChanges);
			}
		}

		public void TestChangingGSTRegistrationAndSave()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_RL_NKClosestPort = "MYBAL";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.LicenceEnterpriseCode = "AAA";
			testHeader.MainAddress.OA_PostCode = "123";
			Factory.Save();

			using (EDIOrganisationFormForTest formForTest = new EDIOrganisationFormForTest(testHeader))
			{
				formForTest.Show();
				formForTest.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedTab = (ZTabPage)formForTest.OrgTabControl.TabPages[15];
				testHeader.LicCompany.LC_IsGSTRegistered = !testHeader.LicCompany.LC_IsGSTRegistered;

				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				formForTest.ButtonsUserControl.SaveButton.PerformClick();
				AssertEquals("Last Message Shown", formForTest.GSTRegisteredChangedForTest, Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should be saved", !testHeader.HasChanges);
			}
		}

		public void TestChangingGSTRegistrationOnHeaderNotInDatabaseAndSave()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.OH_RL_NKClosestPort = "MYBAL";
			testHeader.CreateAndLoadLicenceForOrg();
			testHeader.LicenceEnterpriseCode = "AAA";
			testHeader.MainAddress.OA_PostCode = "123";

			using (EDIOrganisationFormForTest formForTest = new EDIOrganisationFormForTest(testHeader))
			{
				formForTest.Show();
				formForTest.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedTab = (ZTabPage)formForTest.OrgTabControl.TabPages[15];
				testHeader.LicCompany.LC_IsGSTRegistered = !testHeader.LicCompany.LC_IsGSTRegistered;

				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				formForTest.ButtonsUserControl.SaveButton.PerformClick();
				AssertEquals("Last Message Shown", formForTest.GSTRegisteredChangedForTest, Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should be saved", !testHeader.HasChanges);

				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				testHeader.MainAddress.OA_PostCode = "CRP"; // Some random change
				formForTest.ButtonsUserControl.SaveButton.PerformClick();
				AssertNull("Last Message Shown", Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMoveLicencesToNewEnterpriseCodeMenuItemSecurity()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			EDIOrgHeader header = HeaderForTest;
			CreateLicenceCompanyForOrg(header);
			LicenceEnterprise licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			header.LicCompany.LC_LE = licEnt.PK;
			EDIOrganisationFormForTest form = new EDIOrganisationFormForTest(header);

			bool oldValue = EDISecurityCheckpoints.OrgLicenceMoveLicencesToNewEnterprise.IsAllowed;

			try
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				EDISecurityCheckpoints.OrgLicenceMoveLicencesToNewEnterprise.IsAllowed = false;
				form.Call_OnMoveLicencesToNewEnterpriseCode();
				Factory.Save();
				AssertEquals("MoveLicencesToNewEnterpriseCodePopupForm should not be shown", false, form.MoveLicencesToNewEnterpriseCodePopupFormWasShown);
				AssertEquals("security error message should be shown instead of prompt to delete unused licence database", EDISecurityCheckpoints.OrgLicenceMoveLicencesToNewEnterprise.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				EDISecurityCheckpoints.OrgLicenceMoveLicencesToNewEnterprise.IsAllowed = true;
				form.Call_OnMoveLicencesToNewEnterpriseCode();
				Factory.Save();
				AssertEquals("MoveLicencesToNewEnterpriseCodePopupForm should be shown", true, form.MoveLicencesToNewEnterpriseCodePopupFormWasShown);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed = oldValue;
				form.Hide();
				form.Dispose();
			}
		}

		public void TestMoveLicencesToNewEnterpriseCodeMenuItemNoEnterprise()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			EDIOrgHeader header = HeaderForTest;

			using (EDIOrganisationFormForTest form = new EDIOrganisationFormForTest(header))
			{
				MenuItem actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				AssertEquals(false, actionsMenuItem.MenuItems.FindByText("Move Licences To New Enterprise...").Enabled);
			}

			CreateLicenceCompanyForOrg(header);
			LicenceEnterprise licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			header.LicCompany.LC_LE = licEnt.PK;
			using (EDIOrganisationFormForTest form = new EDIOrganisationFormForTest(header))
			{
				MenuItem actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				AssertEquals(true, actionsMenuItem.MenuItems.FindByText("Move Licences To New Enterprise...").Enabled);
			}
		}

		public void TestStaffAssignmentsTab_ShouldContainCompanyNameColumn()
		{
			using (var form = new EDIOrganisationFormForTest(HeaderForTest))
			{
				form.Show();
				Application.DoEvents();
				var staffAssignmentsTabPage = form.FindOrgDetailsTab("StaffAssignmentsTabPage");
				form.OrgDetailsTabControl.SelectedTab = staffAssignmentsTabPage;
				var staffAssignmentsGrid = staffAssignmentsTabPage.FindSingle<ZGrid>("StaffAssignmentsGrid");

				var columnStyle = staffAssignmentsGrid.GetColumnStyle("Company+GC_Name");
				AssertNotNull(columnStyle);
				Assert(columnStyle.IsReadOnly);
				AssertType<ZTextBoxColumnStyleInfo>(columnStyle);

				var productColumnStyle = staffAssignmentsGrid.GetColumnStyle("O8_Product");
				AssertNotNull(productColumnStyle);
				Assert(!productColumnStyle.IsReadOnly);
				AssertType<ZDropEditColumnStyleInfo>(productColumnStyle);

				var productDescriptionColumnStyle = staffAssignmentsGrid.GetColumnStyle("ProductDescription");
				AssertNotNull(productDescriptionColumnStyle);
				Assert(productDescriptionColumnStyle.IsReadOnly);
				AssertType<ZTextBoxColumnStyleInfo>(productDescriptionColumnStyle);
			}
		}
		[TestDate(2020, 2, 25, 21, 30, 0)]
		public void TestContactsTab_NavigatingBetweenRows_DbHits()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();
			var contact3 = org.Contacts.AddNew();

			contact1.FillWithValidTestData();
			contact2.FillWithValidTestData();
			contact3.FillWithValidTestData();

			ShowOrgFormAndNavigateThroughContactsGrid(org);

			// Should not hit BorderWiseLicence table at all since the org isn't saved.
			// TODO: reduce pretty much all these hits to zero. An un-saved org likely won't have related entities in the db.

			AssertDbHits(new Dictionary<string, int>
			{
				{ ClientBranchSchema.Constants.TableName, 1 },
				{ GenCustomAddOnRuleAckSchema.Constants.TableName, 3 },
				{ GlbCompanyCampaignSubscriptionSchema.Constants.TableName, 2 },
				{ GlbEmailAddressSchema.Constants.TableName, 3 },
				{ LicenceCompanySchema.Constants.TableName, 1 },
				{ OrgContactItemSchema.Constants.TableName, 6 }, // Should probably be reduced.
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgCusCodeSchema.Constants.TableName, 1 },
				{ RefComplianceListSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 0 },
				{ OrgCarrierServiceLevelSchema.Constants.TableName, 1 },
				{ StmDataSchema.Constants.TableName, 6 },
			}, Factory);

			Factory.Save();

			RowFactory.ResetCacheAfterDbUpgrade();

			var newFactory = Factory.CreateNewFactory();
			var loadedOrg = newFactory.Load<EDIOrgHeader>(org.PK);

			ShowOrgFormAndNavigateThroughContactsGrid(loadedOrg);

			AssertDbHits(new Dictionary<string, int>
			{
				{ ClientBranchSchema.Constants.TableName, 1 },
				{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ GlbCompanyCampaignSubscriptionSchema.Constants.TableName, 2 },
				{ GlbEmailAddressSchema.Constants.TableName, 3 },
				{ GlbPersonSchema.Constants.TableName, 3 },
				{ LicenceCompanySchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 2 },
				{ OrgContactItemSchema.Constants.TableName, 6 }, // Should probably be reduced.
				{ OrgContactSchema.Constants.TableName, 1 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgCusCodeSchema.Constants.TableName, 2 },
				{ OrgDocumentSchema.Constants.TableName, 3 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgOpportunitySchema.Constants.TableName, 1 },
				{ OrgWebURLSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ RefComplianceListSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 5 },
				{ StmNoteSchema.Constants.TableName, 1 },
				{ OrgCarrierServiceLevelSchema.Constants.TableName, 1 },
				{ StmDataSchema.Constants.TableName, 6 },
			}, newFactory);

			void ShowOrgFormAndNavigateThroughContactsGrid(EDIOrgHeader orgToShow)
			{
				using (var form = new EDIOrganisationFormForTest(orgToShow))
				{
					form.Show();
					var workflowTabPage = form.FindSingle<ZWorkflowTabPage>();
					workflowTabPage.ClearNotificationImage();
					form.OrgTabControl.SelectedTab = form.ContactsTabPage;

					Application.DoEvents();

					var contactsGrid = form.ContactsTabPage.FindSingle<ZGrid>("OrgContactBoundGrid");

					for (var i = 0; i < orgToShow.Contacts.Count; i++)
					{
						contactsGrid.ListManager.Position = i;
						Application.DoEvents();
					}
				}
			}
		}

		public void TestMembershipsTabPage()
		{
			var header = HeaderForTest;
			using (var form = new EDIOrganisationFormForTest(header))
			{
				form.Show();
				var membershipTab = form.FindOrgDetailsTab("Memberships");
				form.OrgDetailsTabControl.SelectedTab = membershipTab;
				AssertEquals(typeof(EdiOrgMembershipControl), membershipTab.Controls[0].GetType());
			}
		}

		public void TestValidAgreementVersionDate()
		{
			var org = HeaderForTest;
			org.Memberships.Add(Factory.New<EdiOrgMembership>());

			org.Memberships[0].SetPropertyValue("EOR_AgreementVersion", ZDate.Today.AddDays(1));
			AssertEquals("Value should be changed if the date is valid, but not throw an error", ZDate.Today.AddDays(1), org.Memberships[0].EOR_AgreementVersion);
			AssertHasError("ValidFrom date must be set before the AgreementVersion", org.Memberships[0].EOR_AgreementVersionInfo, "The Valid-From date must be set before the agreement version");

			org.Memberships[0].EOR_ValidFrom = ZDate.Today;

			org.Memberships[0].SetPropertyValue("EOR_AgreementVersion", ZDate.Today.AddDays(-1));
			AssertEquals("Value should be changed if the date is invalid, but throw an error", ZDate.Today.AddDays(-1), org.Memberships[0].EOR_AgreementVersion);
			AssertHasError("Should throw an error when agreement version is earlier than ValidFrom", org.Memberships[0].EOR_AgreementVersionInfo, "Agreement version date cannot be earlier than the Valid-From date");

			org.Memberships[0].SetPropertyValue("EOR_AgreementVersion", ZDate.Today.AddDays(1));
			AssertEquals("Value should be changed if the date is valid, but not throw an error", ZDate.Today.AddDays(1), org.Memberships[0].EOR_AgreementVersion);
			AssertNoErrors(org.Memberships[0].EOR_AgreementVersionInfo);

			org.Memberships[0].EOR_ValidTo = ZDate.Today.AddDays(2);

			org.Memberships[0].SetPropertyValue("EOR_AgreementVersion", ZDate.Today.AddDays(-1));
			AssertEquals("Value should be changed if the date is invalid, but throw an error", ZDate.Today.AddDays(-1), org.Memberships[0].EOR_AgreementVersion);
			AssertHasError("Should throw an error when agreement version is earlier than ValidFrom", org.Memberships[0].EOR_AgreementVersionInfo, "Agreement version date must be between the Valid-From and Valid-To dates");

			org.Memberships[0].SetPropertyValue("EOR_AgreementVersion", ZDate.Today.AddDays(4));
			AssertEquals("Value should be changed if the date is invalid, but throw an error", ZDate.Today.AddDays(4), org.Memberships[0].EOR_AgreementVersion);
			AssertHasError("Should throw an error when agreement version is later than ValidTo", org.Memberships[0].EOR_AgreementVersionInfo, "Agreement version date must be between the Valid-From and Valid-To dates");

			org.Memberships[0].SetPropertyValue("EOR_AgreementVersion", ZDate.Today.AddDays(1));
			AssertEquals("Value should be changed if the date is valid, but not throw an error", ZDate.Today.AddDays(1), org.Memberships[0].EOR_AgreementVersion);
			AssertNoErrors(org.Memberships[0].EOR_AgreementVersionInfo);
		}

		public void TestTranslateTextElementsMenu()
		{
			var header = HeaderForTest;
			CreateLicenceCompanyForOrg(header);
			header.LicCompany.SelfBilling.L4_InvoiceComment = "A";

			using (var form = new EDIOrganisationFormForTest(header))
			{
				form.Show();

				var actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				ShowPopupMenu(actionsMenuItem);
				var menu = actionsMenuItem.MenuItems.FindByText("Translate Text Elements");

				Form formCreated = null;
				var formCreatedHandler = new EventHandler((sender, args) =>
				{ formCreated = sender as Form; });

				try
				{
					ZForm.FormCreated += formCreatedHandler;
					menu.PerformClick();
				}
				finally
				{
					ZForm.FormCreated -= formCreatedHandler;
				}

				AssertNotNull(formCreated);
				AssertEquals("CustomizableDataTranslationForm", formCreated.Name);
				formCreated.Close();
			}
		}

		#region EDI OrgHeader send message on ValidateAndSave

		public void TestSendMessageToEHubAndUpdateSnapshotIfNeeded_RegistryDisabled()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = false;
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (EDIDataRegistry.Instance.SendOrganizationDataToCertCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SendOrganizationDataToCertCapture { EnableSend = false }))
			{
				using (var form = new EDIOrganisationFormForTest(InitDataForTest()))
				{
					AssertNull(form.Organisation.Snapshot);
					form.Show();
					var result = form.FireSaveButton();
					AssertEquals(ContinueWithSave.Yes, result);
					AssertNull(form.Organisation.Snapshot);
				}

				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(0, ediMessages.Length);
			}
		}

		public void TestSendMessageToEHubAndUpdateSnapshotIfNeeded_RegistryEnabled()
		{
			DataRegistry.Instance.EnableAddressValidationWebService = false;
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (EDIDataRegistry.Instance.SendOrganizationDataToCertCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SendOrganizationDataToCertCapture { EnableSend = true }))
			{
				var org = InitDataForTest();
				using (var form = new EDIOrganisationFormForTest(org))
				{
					AssertNull(org.Snapshot);
					form.Show();

					var result = form.FireSaveButton();
					AssertEquals(ContinueWithSave.Yes, result);
					AssertNotNull(org.Snapshot);
				}

				var ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(1, ediMessages.Length);

				org.Snapshot = null;
				using (var form = new EDIOrganisationFormForTest(org))
				{
					AssertNotNull(org.Snapshot);
				}
			}
		}

		public void TestSendMessageToEHubAndUpdateSnapshotIfNeeded_ValueChanged()
		{
			var orgHeader = InitDataForTest();

			var code = orgHeader.CustomsCodes.AddNew("EIN", "12-1234567");
			code.OK_RN_NKCodeCountry = "US";
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			contact.OC_Email = "Test.Contact@edi.com";

			var doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.Receivables.Code;
			doc.OD_DefaultContact = true;

			DataRegistry.Instance.EnableAddressValidationWebService = false;
			using (OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (EDIDataRegistry.Instance.SendOrganizationDataToCertCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SendOrganizationDataToCertCapture { EnableSend = true }))
			{
				using (var form = new EDIOrganisationFormForTest(orgHeader))
				{
					form.Show();
					var result = form.FireSaveButton();
					AssertEquals(ContinueWithSave.Yes, result);
					var ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(1, ediMessages.Length);

					code.OK_CustomsRegNo = "21-1234567";
					result = form.FireSaveButton();
					AssertEquals(ContinueWithSave.Yes, result);
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(2, ediMessages.Length);

					var lastMessage = ediMessages.OrderByDescending(u => u.EM_MessageNum).First();
					AssertContains(code.OK_CustomsRegNo, lastMessage.EM_MessageTextDetail);

					orgHeader.CompanyData.OB_APPaymentTerms = "CCC";
					result = form.FireSaveButton();
					AssertEquals(ContinueWithSave.Yes, result);
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals(3, ediMessages.Length);

					orgHeader.MainAddress.OA_Email = "dummy@123.com";
					result = form.FireSaveButton();
					AssertEquals(ContinueWithSave.Yes, result);
					ediMessages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals("No new messages created", 3, ediMessages.Length);
				}
			}
		}

		public void TestShouldShowPopupWarningOnSave()
		{
			var org1 = InitDataForTest();
			org1.LicenceEnterpriseCode = "LE1";
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_FullName = "OrgRelatedParty";
			org2.OH_Code = "ORG2";
			org2.LicenceEnterpriseCode = "LE2";
			Factory.Save();

			var relation1 = Factory.New<EDIOrgRelatedParty>();
			relation1.PR_OH_Parent = org1.PK;
			relation1.PR_OH_RelatedParty = org2.PK;
			relation1.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			using (var form = new EDIOrganisationFormForTest(org1))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var result = form.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertContains("An organization you are attempting to add has a different enterprise license code than New Org (NEWORGORD). As this is a rare scenario, please only proceed after verifying that it is correct.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		EDIOrgHeader InitDataForTest()
		{
			var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgHeader.OH_Code = "ORGTOSEND";
			orgHeader.OH_FullName = "New Org";
			orgHeader.OH_RL_NKClosestPort = "USORD";
			orgHeader.OH_IsDebtor = true;

			var orgDebtorGroup = Factory.NewWithValidTestData<OrgDebtorGroup>();
			orgDebtorGroup.OJ_Code = "ABC";
			orgHeader.CompanyData.OB_OJ_ARDebtorGroup = orgDebtorGroup.PK;

			var mainAddress = orgHeader.Addresses.MainAddress;
			mainAddress.OA_City = "Sydney";
			mainAddress.OA_Address1 = "8 George Street";
			mainAddress.OA_RL_NKRelatedPortCode = "USORD";
			mainAddress.OA_RN_NKCountryCode = "US";
			mainAddress.OA_PostCode = "2000";

			return orgHeader;
		}

		#endregion

		#region Set Up

		public EDIOrgHeader HeaderForTest
		{
			get
			{
				EDIOrgHeader fHeaderForTest = Factory.NewWithValidTestData<EDIOrgHeader>();
				fHeaderForTest.OH_FullName = "Test Header";
				fHeaderForTest.OH_Code = "TGBLOG";
				fHeaderForTest.OH_RL_NKClosestPort = "AUBNE";
				fHeaderForTest.MainAddress.OA_Address1 = "Address";
				fHeaderForTest.MainAddress.OA_City = "City";
				fHeaderForTest.MainAddress.OA_Phone_Formatted = "0426829924";
				fHeaderForTest.OH_IsSalesLead = true;

				return fHeaderForTest;
			}
		}

		void CreateLicenceCompanyForOrg(EDIOrgHeader orgHeader)
		{
			LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			using (licenceCompany.SuspendSettingHasChanges())
			{
				licenceCompany.LC_CompanyCode = orgHeader.OH_Code.Right(3);
				licenceCompany.LC_CompanyCountry = orgHeader.UNLOCO != null ? orgHeader.UNLOCO.RL_RN_NKCountryCode : ZString.Empty;
				licenceCompany.LC_RX_NKCurrency = orgHeader.UNLOCO != null ? orgHeader.UNLOCO.Country.RN_RX_NKLocalCurrency : ZString.Empty;
				licenceCompany.LC_OH = orgHeader.PK;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			WebUrlLauncher.ClearLastUrlLaunched();
		}

		protected override void TearDown()
		{
			base.TearDown();
			WebUrlLauncher.ClearLastUrlLaunched();
		}

		#endregion
	}
}

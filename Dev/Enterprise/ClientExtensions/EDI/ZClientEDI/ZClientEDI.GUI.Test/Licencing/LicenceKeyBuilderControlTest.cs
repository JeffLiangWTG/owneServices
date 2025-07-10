using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.GUI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Module;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI.Test
{
	[TestedType(typeof(LicenceKeyBuilderControlForTest))]
	public class LicenceKeyBuilderControlTest : OrganisationSecurityContainerControlBaseTest
	{
		#region Test Objects

		#region EDIOrgForm For Test

		public class EDIOrganisationFormForTest : EDIOrganisationForm
		{
			public EDIOrganisationFormForTest(EDIOrgHeader organisation, IEdiOrgViewController viewController = null)
				: base(organisation, viewController)
			{
			}

			public ZTabControl OrgTabControl
			{
				get { return OrganisationsTabControl; }
			}
		}

		#endregion

		#region Overridden LicenceKeyBuilderControl

		public class LicenceKeyBuilderControlForTest : LicenceKeyBuilderControl
		{
			public LicenceKeyBuilderControlForTest(ILicenceViewController viewController = null)
				: base(viewController)
			{
			}

			public void AutoDeployLicenceButton_ClickForTest() => base.AutoDeployLicenceButton_Click(null, null);
			public void GenerateLicenceKeyButton_ClickForTest() => base.GenerateLicenceKeyButton_Click(null, null);

			DialogResult fNextDialogResult;
			public DialogResult NextDialogResult
			{
				get { return fNextDialogResult; }
				set { fNextDialogResult = value; }
			}

			string fNextDirectoryName;
			public string NextDirectoryName
			{
				get { return fNextDirectoryName; }
				set { fNextDirectoryName = value; }
			}

			protected override DialogResult ShowDialog(ZFolderBrowserDialog browser)
			{
				return NextDialogResult;
			}

			protected override string GetPath(ZFolderBrowserDialog browser)
			{
				return NextDirectoryName;
			}

			public void ConnectToClientButtonClickForTest()
			{
				this.ConnectToClient(this, EventArgs.Empty);
			}

			public bool GetDirectory(ref string directory)
			{
				return base.GetDirectoryForLicenceKeyFile(ref directory);
			}

			public ZTabControl LicenceTabControlForTest => base.LicenceTabControl;
			public ZGrid TrainingCourseGridForTest => base.TrainingCourseGrid;

			public ZTabPage TrainingSchedulesTabPageForTest => base.TrainingSchedulesTabPage;
			public ZTabPage ProjectsTabPageForTest => base.ProjectsTabPage;
			public ZTabPage ConnectionsTabPageForTest => base.ConnectionsTabPage;

			public ZButton AutoDeployLicenceButtonForTest => base.AutoDeployLicenceButton;
			public ZButton GenerateLicenceKeyButtonForTest => base.GenerateLicenceKeyButton;
			public ZButton SendButtonForTest => base.SendButton;
			public ZGrid ProjectsGridForTest => base.ProjectsGrid;
			public ModuleButtonGridForLicencing DatabasesModuleButtonGridForTest => base.DatabasesModuleButtonGrid;
			public void SendButton_ClickForTest(object sender, EventArgs e) => base.SendButton_Click(sender, e);
		}

		#endregion

		#region Overriden Detach button for test

		public class ModuleButtonGridForDetach : ModuleButtonGridForLicencing
		{
			public void DetachForTest(BusinessObject selected)
			{
				base.Detach(selected);
			}

			public bool DatabaseDetached;
			public LicenceDatabase DatabasePassedForDetachment;

			protected override void DetachDatabase(BusinessObject selected)
			{
				DatabaseDetached = true;
				DatabasePassedForDetachment = (LicenceDatabase)selected;
			}

			public void Call_DetachButton_Click()
			{
				DetachButton_Click(this, new EventArgs());
			}
		}

		#endregion

		#region Overridden ModuleButtonGrid With Test Environment Attaching

		public class ModuleButtonGridForLicencingForTest : ModuleButtonGridForLicencing
		{
			public void AttachButton_ClickForTest()
			{
				base.AttachButton_Click(this, EventArgs.Empty);
			}

			public bool DatabaseAttached;
			protected override void AttachDatabase(object sender, EventArgs e)
			{
				DatabaseAttached = true;
			}
		}

		#endregion

		#region PopulatedOrganisation

		EDIOrgHeader PopulatedOrganisation
		{
			get
			{
				if (populatedOrganisation == null)
				{
					populatedOrganisation = Factory.New<EDIOrgHeader>();
					populatedOrganisation.CreateAndLoadLicenceForOrg();
					populatedOrganisation.OH_Code = "ABCZYV";
					populatedOrganisation.MainAddress.OA_Address1 = "Address1";
					populatedOrganisation.LicenceEnterpriseCode = "ABC";
					populatedOrganisation.LicCompany.LC_CompanyCode = "ZYV";
					populatedOrganisation.LicCompany.LicDatabases.AddNew();
					populatedOrganisation.LicCompany.LicDatabases[0].LD_Product = ProductTypes.Codes.Enterprise;
				}
				return populatedOrganisation;
			}
		}

		EDIOrgHeader populatedOrganisation;

		#endregion

		#endregion

		#region

		public void TestTokenAuthenticationInTheGridOfLicenceKeyBuilderControl()
		{
			Organisation.OH_Code = "XXY";
			Organisation.CreateAndLoadLicenceForOrg();
			Organisation.Factory.Save();

			using (var formForTest = new EDIOrganisationForm(Organisation, null))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				formForTest.Controls.Add(controlForTest);
				formForTest.Show();
				var column = controlForTest.DatabasesModuleButtonGridForTest.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(column => column.Caption == "Token Authentication");
				AssertNotNull(column);
			}
		}

		#endregion

		#region

		public void TestFeatureSetNameInTheGridOfLicenceKeyBuilderControl()
		{
			Organisation.OH_Code = "XXY";
			Organisation.CreateAndLoadLicenceForOrg();
			Organisation.Factory.Save();

			using (var formForTest = new EDIOrganisationForm(Organisation, null))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				formForTest.Controls.Add(controlForTest);
				formForTest.Show();
				var column = controlForTest.DatabasesModuleButtonGridForTest.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(column => column.Caption == "Feature Set Name");
				AssertNotNull(column);
			}
		}

		#endregion

		#region Disabled Buttons

		public void TestSendUpgradeButtonUsesUniqueLicenceCheckPoint()
		{
			Organisation.OH_Code = "XXY";
			Organisation.CreateAndLoadLicenceForOrg();
			Organisation.Factory.Save();
			LicenceDatabase licDB = Organisation.LicCompany.LicDatabases.AddNew();
			licDB.LD_Product = ProductTypes.Codes.Enterprise;

			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed;
			try
			{
				EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed = true;
				AssertSendUpgradeButton(Organisation, true);

				EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed = false;
				AssertSendUpgradeButton(Organisation, false);

				licDB.LD_Product = ProductTypes.Codes.CargoWiseOne;

				EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed = true;
				AssertSendUpgradeButton(Organisation, true);

				EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed = false;
				AssertSendUpgradeButton(Organisation, false);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed = oldValue;
			}
		}

		static void AssertSendUpgradeButton(EDIOrgHeader header, bool shouldBeEnabled)
		{
			using (EDIOrganisationForm formForTest = new EDIOrganisationForm(header, null))
			{
				using (LicenceKeyBuilderControlForTest controlForTest = new LicenceKeyBuilderControlForTest())
				{
					formForTest.Controls.Add(controlForTest);
					formForTest.Show();

					formForTest.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;

					var sendButton = controlForTest.SendButtonForTest;
					AssertEquals("SendButton is " + (!shouldBeEnabled ? "NOT " : "") + " enabled", shouldBeEnabled, sendButton.Enabled);
				}
			}
		}

		public void TestButtonDisability()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			Organisation.OH_Code = "XXY";
			Organisation.CreateAndLoadLicenceForOrg();
			Organisation.Factory.Save();
			Organisation.LicCompany.LicDatabases.RemoveAndDeleteAll();
			Organisation.LicCompany.LicHeadersForAllDatabases.RemoveAndDeleteAll();
			CheckFormButtonDisability(0, false);

			var dB = Organisation.LicCompany.LicDatabases.AddNew();
			dB.LD_Product = "BBB";
			Organisation.LicCompany.LicHeadersForAllDatabases.Load();
			AssertEquals(true, dB.LD_IsActive);
			AssertEquals(1, Organisation.LicCompany.LicHeadersForAllDatabases.Count);
			CheckFormButtonDisability(1, false);

			dB.LD_Product = ProductTypes.Codes.ProductivityWise;
			dB.LD_LicenceType = DatabaseTypes.Codes.Production;
			CheckFormButtonDisability(1, true);

			dB.LD_Product = ProductTypes.Codes.Enterprise;
			dB.LD_LicenceType = DatabaseTypes.Codes.Production;
			CheckFormButtonDisability(1, true);

			var dB2 = Organisation.LicCompany.LicDatabases.AddNew();
			dB2.LD_Product = "AAA";
			dB2.LD_LicenceType = DatabaseTypes.Codes.Production;
			Organisation.LicCompany.LicHeadersForAllDatabases.Load();
			AssertEquals(2, Organisation.LicCompany.LicHeadersForAllDatabases.Count);
			CheckFormButtonDisability(2, false); // Legacy should be first and be false - prod is second and therefore true.
		}

		void CheckFormButtonDisability(int expectedItemCount, bool expectEnabled)
		{
			using (var formForTest = new EDIOrganisationFormForTest(Organisation))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.DisplayMode = ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				var databasesGrid = controlForTest.DatabasesModuleButtonGridForTest;

				if (expectedItemCount > 1)
				{
					// If the first is NOT legacy and the first is prod
					if (controlForTest.SelectedLicenceDatabase.LD_LicenceType == DatabaseTypes.Codes.Production)
					{
						expectEnabled = !expectEnabled;
					}
				}

				AssertEquals("Num of Databases in collection", expectedItemCount, databasesGrid.InnerGrid.List.Count);
				var generateLicenceKeyButton = controlForTest.GenerateLicenceKeyButtonForTest;
				var autoDeployLicenceButton = controlForTest.AutoDeployLicenceButtonForTest;
				var sendButton = controlForTest.SendButtonForTest;
				AssertEquals("GenerateLicenceKeyButton is " + (!expectEnabled ? "NOT " : "") + " enabled", expectEnabled, generateLicenceKeyButton.Enabled);
				AssertEquals("AutoDeployLicenceButton is " + (!expectEnabled ? "NOT " : "") + " enabled", expectEnabled, autoDeployLicenceButton.Enabled);
				AssertEquals("SendButton is " + (!expectEnabled ? "NOT " : "") + " enabled", expectEnabled, sendButton.Enabled);

				if (expectedItemCount > 1)
				{
					databasesGrid.InnerGrid.ListManager.Position = databasesGrid.InnerGrid.ListManager.Position + 1;
					AssertEquals("GenerateLicenceKeyButton is " + (expectEnabled ? "NOT " : "") + " enabled (Reverse Logic b/c of Production licence)", !expectEnabled, generateLicenceKeyButton.Enabled);
					AssertEquals("AutoDeployLicenceButton is " + (expectEnabled ? "NOT " : "") + " enabled (Reverse Logic b/c of Production licence)", !expectEnabled, autoDeployLicenceButton.Enabled);
					AssertEquals("SendButton is " + (expectEnabled ? "NOT " : "") + " enabled (Reverse Logic b/c of Production licence)", !expectEnabled, sendButton.Enabled);
				}
			}
		}

		#endregion

		#region Read-only Checkboxes

		public void TestModifyExchangeRatesAndTaxCheckPoint()
		{
			Organisation.CreateAndLoadLicenceForOrg();
			Organisation.Factory.Save();
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed;
			try
			{
				EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed = false;
				AssertExchangeRateAndTaxElements(Organisation, true);

				EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed = true;
				AssertExchangeRateAndTaxElements(Organisation, false);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifyExchangeRatesAndTax.IsAllowed = oldValue;
			}
		}

		void AssertExchangeRateAndTaxElements(EDIOrgHeader org, bool isReadOnly)
		{
			using (var formForTest = new EDIOrganisationFormForTest(org))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.DisplayMode = ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				foreach (Control control in (FindSubControlByName(licenceTabPage, "ExchangeRateGroupBox")).Controls)
				{
					if (control as ZCheckBox != null)
					{
						AssertEquals(control.Name + " is read-only", isReadOnly, ((ZCheckBox)control).ReadOnly);
					}
				}

				foreach (Control control in (FindSubControlByName(licenceTabPage, "GSTGroupBox")).Controls)
				{
					if (control as ZCheckBox != null)
					{
						AssertEquals(control.Name + " is read-only", isReadOnly, ((ZCheckBox)control).ReadOnly);
					}
				}
			}
		}

		#endregion

		#region CodeFindBoxes

		public void TestLicenceEnterpriseCodeFindBox()
		{
			Organisation.CreateAndLoadLicenceForOrg();
			Organisation.Factory.Save();
			using (EDIOrganisationFormForTest formForTest = new EDIOrganisationFormForTest(Organisation))
			using (LicenceKeyBuilderControlForTest controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.DisplayMode = ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				var testControl = FindSubControlByName(licenceTabPage, "LicenceEnterpriseCodeFindBox") as ZCodeFindBox;
				AssertNotNull("Control is found", testControl);
				AssertEquals("Max length", testControl.MaxLength, LicenceEnterprise.Schema.LE_EnterpriseCodeMaxLength);
			}
		}

		static int GetLicenceKeyBuilderTabPageIndex(EDIOrganisationFormForTest form)
			=> form.OrgTabControl.TabPages.Cast<ZTabPage>().IndexOf(tabPage => tabPage.Name == "LicenceKeyBuilderTabPage");

		static LicenceKeyBuilderTabPage GetLicenceKeyBuilderTabPage(EDIOrganisationFormForTest form)
			=> (LicenceKeyBuilderTabPage)form.OrgTabControl.TabPages.Cast<ZTabPage>().Single(tabPage => tabPage.Name == "LicenceKeyBuilderTabPage");

		#endregion

		#region Database Module Button Grid

		[ExpectNoExceptions()]
		public void TestEditDatabaseWithoutItems()
		{
			PopulatedOrganisation.LicCompany.LicDatabases.RemoveAndDeleteAll();
			Factory.Save();

			using (var formForTest = new EDIOrganisationFormForTest(PopulatedOrganisation))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				var databasesGrid = controlForTest.DatabasesModuleButtonGridForTest;

				MenuItem editDBMenuItem = AssertMenuContains(databasesGrid.InnerGrid.ContextMenu.MenuItems, "Edit Database");
				editDBMenuItem.PerformClick();
			}
		}

		public void TestEditDatabaseWithItems()
		{
			PopulatedOrganisation.Factory.Save();
			AssertEquals(1, PopulatedOrganisation.LicCompany.ActiveOrAllLicDatabases.Count);

			using (var formForTest = new EDIOrganisationFormForTest(PopulatedOrganisation))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				var databasesGrid = controlForTest.DatabasesModuleButtonGridForTest;
				MenuItem editDBMenuItem = AssertMenuContains(databasesGrid.InnerGrid.ContextMenu.MenuItems, "Edit Database");
				editDBMenuItem.PerformClick();
				AssertType<LicenceDatabaseForm>("LicenceDatabase Form shown when clicked", ZFormModaliser.ActiveForm);
				ZFormModaliser.ActiveForm.Close();

				var db = PopulatedOrganisation.LicCompany.ActiveOrAllLicDatabases[0];
				AssertEquals(false, db.HasChanges);
				db.LD_TenantID = "Ref#001";
				AssertEquals(true, db.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				editDBMenuItem.PerformClick();
				AssertEquals("Please save the form before editing the database.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEditDatabaseMenuItems()
		{
			PopulatedOrganisation.Factory.Save();

			using (var formForTest = new EDIOrganisationFormForTest(PopulatedOrganisation))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				var databasesGrid = controlForTest.DatabasesModuleButtonGridForTest;

				AssertMenuContains(databasesGrid.InnerGrid.ContextMenu.MenuItems, "Edit Database");
			}
		}

		Control FindSubControlByName(Control parentControl, string controlName)
		{
			Control result = null;

			if (parentControl.Name == controlName)
			{
				result = parentControl;
			}
			else if (parentControl.Controls.Count > 0)
			{
				foreach (Control subControl in parentControl.Controls)
				{
					result = FindSubControlByName(subControl, controlName);
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		MenuItem AssertMenuContains(Menu.MenuItemCollection menuItems, string menuItemToFind)
		{
			MenuItem result = FindMenuItem(menuItems, menuItemToFind);
			AssertNotNull("Menu Item " + menuItemToFind + " should have been found", result);
			return result;
		}

		MenuItem FindMenuItem(Menu.MenuItemCollection menuItems, string menuItemToFind)
		{
			MenuItem result = null;

			foreach (MenuItem menu in menuItems)
			{
				if (menu.Text == menuItemToFind)
				{
					result = menu;
					break;
				}
			}

			return result;
		}

		public void TestDetachDatabase()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "XXY";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicDatabases.RemoveAndDeleteAll();

			using (var testForm = new EDIOrganisationFormForTest(org, null))
			{
				ModuleButtonGridForDetach testControl = new ModuleButtonGridForDetach();
				var licenceTabPage = GetLicenceKeyBuilderTabPage(testForm);
				licenceTabPage.HideCoveringLabel();
				testControl.ParentControl = licenceTabPage.LicenceControl;
				testForm.Controls.Add(testControl);
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				LicenceDatabase databaseForDetach = org.LicCompany.LicDatabases.AddNew();
				databaseForDetach.LD_ServerCode = "DB2";
				org.LicCompany.LicDatabases.AddNew();
				org.Factory.Save();
				testControl.DetachForTest(databaseForDetach);
				Assert("Database should be detached", testControl.DatabaseDetached);
				AssertEquals("Selected database should be detached", databaseForDetach, testControl.DatabasePassedForDetachment);
			}
			Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestDetachDatabase_DeleteUnusedDatabase()
		{
			EDIOrgHeader organisation = Factory.New<EDIOrgHeader>();
			organisation.OH_Code = "XXY";
			organisation.CreateAndLoadLicenceForOrg();
			organisation.LicCompany.LicDatabases.RemoveAndDeleteAll();
			organisation.LicenceEnterpriseCode = "LEC";

			using (var testForm = new EDIOrganisationFormForTest(organisation, null))
			{
				EDISecurityCheckpoints.OrgLicenceRemoveLicenceDatabase.IsAllowed = true;

				ModuleButtonGridForDetach testControl = new ModuleButtonGridForDetach();
				var licenceTabPage = GetLicenceKeyBuilderTabPage(testForm);
				licenceTabPage.HideCoveringLabel();
				testControl.ParentControl = licenceTabPage.LicenceControl;
				testForm.Controls.Add(testControl);

				LicenceDatabase databaseForDetach = organisation.LicCompany.LicDatabases.AddNew();
				ZGuid databaseForDetachPK = databaseForDetach.PK;
				ZString detachedDatabaseLD_ServerCode = databaseForDetach.LD_ServerCode;
				Factory.Save();
				AssertEquals("Precondition: test licence database should have 1 license header pointing to it", 1, Factory.Load<LicenceHeader>(new ZQuery(LicenceHeaderSchema.LA_LD, databaseForDetachPK)).Length);

				int numOfLogsBeforeDetachmentAttempt = organisation.Logs.Find(new ZQuery()).Length;
				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testControl.DetachForTest(databaseForDetach);
				Factory.Save();
				AssertEquals("licence database should not be deleted", databaseForDetach, Factory.Load<LicenceDatabase>(databaseForDetachPK));
				AssertEquals("No logs should be added", 0, organisation.Logs.Find(new ZQuery()).Length - numOfLogsBeforeDetachmentAttempt);

				Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testControl.DetachForTest(databaseForDetach);
				Factory.Save();
				AssertEquals("licence database should be deleted", null, Factory.Load<LicenceDatabase>(databaseForDetachPK));
				AssertEquals("Log about deleted licence database should be added", "deleted license database server '" + detachedDatabaseLD_ServerCode + "'", organisation.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, "deleted license database server '" + detachedDatabaseLD_ServerCode + "'"))[0].SL_Reference);

				databaseForDetach = organisation.LicCompany.LicDatabases.AddNew();
				databaseForDetachPK = databaseForDetach.PK;
				Factory.Save();

				BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
				EDIOrgHeader afOrganisation = anotherFactory.New<EDIOrgHeader>();
				afOrganisation.OH_Code = "QCQ";
				afOrganisation.LicenceEnterpriseCode = "LEC";
				AssertNotNull("Precondition: has licence company", afOrganisation.LicCompany);
				afOrganisation.LicCompany.LicDatabases.AddFromDatabase(databaseForDetachPK);
				anotherFactory.Save();

				AssertEquals("Precondition: test licence database should have 2 licence headers pointing to it", 2, Factory.Load<LicenceHeader>(new ZQuery(LicenceHeaderSchema.LA_LD, databaseForDetachPK)).Length);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testControl.DetachForTest(databaseForDetach);
				Factory.Save();
				AssertEquals("licence database should never be deleted when LicenceHeader's pointing to it still exist", databaseForDetach, Factory.Load<LicenceDatabase>(databaseForDetachPK));
				AssertEquals("No dialog should be shown", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestDetachDatabaseSecurity()
		{
			EDIOrgHeader organisation = Factory.New<EDIOrgHeader>();
			organisation.OH_Code = "XXY";
			organisation.CreateAndLoadLicenceForOrg();
			organisation.LicCompany.LicDatabases.RemoveAndDeleteAll();
			organisation.LicenceEnterpriseCode = "LEC";

			bool oldValue = EDISecurityCheckpoints.OrgLicenceRemoveLicenceDatabase.IsAllowed;
			try
			{
				using (EDIOrganisationForm testForm = new EDIOrganisationForm(organisation, null))
				{
					ModuleButtonGridForDetach testControl = new ModuleButtonGridForDetach();
					testForm.Controls.Add(testControl);
					LicenceDatabase databaseForDetach = organisation.LicCompany.LicDatabases.AddNew();
					Factory.Save();

					EDISecurityCheckpoints.OrgLicenceRemoveLicenceDatabase.IsAllowed = false;
					Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					testControl.Call_DetachButton_Click();
					AssertEquals("security error message should be shown", EDISecurityCheckpoints.OrgLicenceRemoveLicenceDatabase.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

					EDISecurityCheckpoints.OrgLicenceRemoveLicenceDatabase.IsAllowed = true;
					testControl.Call_DetachButton_Click();
					Factory.Save();
					AssertEquals("'not seleted' message should be shown indicting that security check was passed", "Please select an item in the grid by first clicking in the left-hand gutter to highlight the whole row.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceRemoveLicenceDatabase.IsAllowed = oldValue;
			}
		}

		public void TestModuleButtonGridForLicencing()
		{
			PopulatedOrganisation.Factory.Save();

			using (ZForm testForm = new ZForm(PopulatedOrganisation))
			{
				ModuleButtonGridForLicencingForTest testControl = new ModuleButtonGridForLicencingForTest();
				testForm.Controls.Add(testControl);
				testControl.AttachButton_ClickForTest();
				Assert("Database was attached", testControl.DatabaseAttached);

				PopulatedOrganisation.LicEnterprise.Delete();
				testControl.AttachButton_ClickForTest();
				testControl.DatabaseAttached = false;
				AssertEquals("Cannot Add - changed Licence Enterprise", "You must first save your changes, before attempting to add Databases to the Organization.", Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Database was NOT attached", !testControl.DatabaseAttached);
			}

			Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestDatabaseGridButtonSecurity()
		{
			PopulatedOrganisation.Factory.Save();

			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed;
			try
			{
				EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed = true;
				AssertAttachButton(PopulatedOrganisation, true);

				EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed = false;
				AssertAttachButton(PopulatedOrganisation, false);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails.IsAllowed = oldValue;
			}
		}

		public void TestLicenceDeploymentButtonSecurity()
		{
			PopulatedOrganisation.Factory.Save();

			bool oldValue = EDISecurityCheckpoints.OrgLicenceModifyLicenceKey.IsAllowed;
			try
			{
				EDISecurityCheckpoints.OrgLicenceCreateAndEmailLicenceKey.IsAllowed = true;
				AssertLicenceKeyDeploymentDeploymentButtons(PopulatedOrganisation, true);

				EDISecurityCheckpoints.OrgLicenceCreateAndEmailLicenceKey.IsAllowed = false;
				AssertLicenceKeyDeploymentDeploymentButtons(PopulatedOrganisation, false);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceCreateAndEmailLicenceKey.IsAllowed = oldValue;
			}
		}

		void AssertAttachButton(EDIOrgHeader header, bool shouldBeEnabled)
		{
			using (var formForTest = new EDIOrganisationFormForTest(header))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				var databasesGrid = controlForTest.DatabasesModuleButtonGridForTest;

				var toolStrip = (ZToolStrip)FindSubControlByName(databasesGrid, "toolStrip");
				var attachButton = toolStrip.Items.Cast<ZToolStripButton>().FirstOrDefault(x => x.Name == "AttachButton");
				AssertEquals("Attach Button should " + (!shouldBeEnabled ? "NOT " : "") + " be enabled", shouldBeEnabled, attachButton.Enabled);
			}
		}

		void AssertLicenceKeyDeploymentDeploymentButtons(EDIOrgHeader header, bool shouldBeEnabled)
		{
			using (var formForTest = new EDIOrganisationFormForTest(header))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				var autoDeployLicenceButton = licenceTabPage.LicenceControl.Controls.Find("AutoDeployLicenceButton", true)[0];
				var generateLicenceKeyButton = licenceTabPage.LicenceControl.Controls.Find("GenerateLicenceKeyButton", true)[0];
				AssertEquals("Auto Deploy Licence Key button should " + (!shouldBeEnabled ? "NOT " : "") + " be enabled", shouldBeEnabled, autoDeployLicenceButton.Enabled);
				AssertEquals("Generate Licence Key button should " + (!shouldBeEnabled ? "NOT " : "") + " be enabled", shouldBeEnabled, generateLicenceKeyButton.Enabled);
			}
		}

		#endregion

		#region Projects

		public void TestProjectsDoubleClick()
		{
			Organisation.OH_Code = "XXY";
			Organisation.CreateAndLoadLicenceForOrg();
			LicenceDatabase dB = Organisation.LicCompany.LicDatabases.AddNew();

			EDIProject project = Factory.New<EDIProject>();
			project.ChangeClientOrganisation(Organisation);
			Factory.Save();

			using (EDIOrganisationForm formForTest = new EDIOrganisationForm(Organisation, null))
			{
				using (LicenceKeyBuilderControlForTest controlForTest = new LicenceKeyBuilderControlForTest())
				{
					formForTest.Controls.Add(controlForTest);
					formForTest.Show();
					controlForTest.SetDataBinding(Organisation, ""); // M.K - We do not test here how binding works, it runtime this control is bound, so this is quickest solution to fixt this test
					TabControl tab = (TabControl)controlForTest.ProjectsTabPageForTest.Parent;
					tab.SelectedIndex = tab.Controls.GetChildIndex(controlForTest.ProjectsTabPageForTest);

					AssertEquals("1 project in list", 1, controlForTest.ProjectsGridForTest.List.Count);

					controlForTest.ProjectsGridForTest.Select(0);
					try
					{
						MethodInfo methodInfo = typeof(ZGrid).GetMethod("OnDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance);
						methodInfo.Invoke(controlForTest.ProjectsGridForTest, new object[] { EventArgs.Empty });
						AssertNotNull("Form should have been shown on double click event", ZFormModaliser.ActiveForm);
						AssertEquals("Install Project form shown", "EDIProjectForm", ZFormModaliser.ActiveForm.GetType().Name);
						AssertEquals(true, controlForTest.ProjectsGridForTest.ReadOnly);
					}
					finally
					{
						if (ZFormModaliser.ActiveForm != null)
						{
							((ZForm)ZFormModaliser.ActiveForm).Close();
						}
					}
				}
			}
		}

		#endregion

		#region Email Licence Key

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		public void TestGetDirectoryForLicenceKeyFile()
		{
			Organisation.OH_Code = "XXY";

			using (ZForm formForTest = new ZForm(Organisation))
			using (LicenceKeyBuilderControlForTest controlForTest = new LicenceKeyBuilderControlForTest())
			{
				((ICompositeControlBindingSourceProvider)formForTest).BindingSource.SetBindingMember(controlForTest, "");
				formForTest.Controls.Add(controlForTest);
				formForTest.Show();

				string outputDirectory = "";
				controlForTest.NextDialogResult = DialogResult.OK;
				controlForTest.NextDirectoryName = BaseSourcePath + "test\\";

				bool result = controlForTest.GetDirectory(ref outputDirectory);
				Assert("Got the directory successfully", result);
				AssertEquals("Output Directory Set Properly", controlForTest.NextDirectoryName, outputDirectory);

				outputDirectory = "";
				controlForTest.NextDialogResult = DialogResult.Cancel;
				controlForTest.NextDirectoryName = BaseSourcePath + "test2\\";
				result = controlForTest.GetDirectory(ref outputDirectory);
				Assert("Did Not get the directory successfully", !result);
				AssertEquals("Output Directory empty", "", outputDirectory);
			}
		}

		#endregion

		#region Licence In Sync Status

		public void TestLicenceInSyncStatusForOrgWithLicences()
		{
			Organisation.OH_Code = "XXY";
			Organisation.CreateAndLoadLicenceForOrg();
			LicenceDatabase dbWithLicenceKeyInSync = Organisation.LicCompany.LicDatabases.AddNew();
			LicenceDatabase dbWithLicenceKeyNotInSync = Organisation.LicCompany.LicDatabases.AddNew();
			dbWithLicenceKeyInSync.LD_Product = ProductTypes.Codes.Enterprise;
			dbWithLicenceKeyNotInSync.LD_Product = ProductTypes.Codes.Enterprise;
			Organisation.LicCompany.GetHeader(dbWithLicenceKeyInSync).LA_LastLicenceCheckInSync = true;
			Organisation.LicCompany.GetHeader(dbWithLicenceKeyNotInSync).LA_LastLicenceCheckInSync = false;
			dbWithLicenceKeyInSync.LD_ServerCode = "DB1";
			dbWithLicenceKeyNotInSync.LD_ServerCode = "DB2";
			Factory.Save();

			LicenceDatabase dbNew = Organisation.LicCompany.LicDatabases.AddNew();
			dbNew.LD_Product = ProductTypes.Codes.Enterprise;
			dbNew.LD_ServerCode = "DB3";

			using (var formForTest = new EDIOrganisationFormForTest(Organisation))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				AssertEquals("3 DB in Org Licence", 3, controlForTest.DatabasesModuleButtonGridForTest.InnerGrid.List.Count);

				controlForTest.LicenceTabControlForTest.SelectedIndex = 1;

				Application.DoEvents();

				controlForTest.DatabasesModuleButtonGridForTest.InnerGrid.ListManager.Position = 0;
				AssertEquals("DB 1 should change label to in sync", LicenceModulesControl.InSyncMessage, licenceTabPage.LicenceControl.LicenceKeyInSyncLabel.Text);
				AssertEquals(Color.Black, licenceTabPage.LicenceControl.LicenceKeyInSyncLabel.ForeColor);
				AssertEquals(false, licenceTabPage.LicenceControl.LicenceKeyInSyncLabel.IsFontBold);

				controlForTest.DatabasesModuleButtonGridForTest.InnerGrid.ListManager.Position = 1;
				AssertEquals("DB 2 should change label to not in sync", LicenceModulesControl.NotInSyncMessage, licenceTabPage.LicenceControl.LicenceKeyInSyncLabel.Text);
				AssertEquals(Color.Red, licenceTabPage.LicenceControl.LicenceKeyInSyncLabel.ForeColor);
				AssertEquals(true, licenceTabPage.LicenceControl.LicenceKeyInSyncLabel.IsFontBold);

				controlForTest.DatabasesModuleButtonGridForTest.InnerGrid.ListManager.Position = 2;
				AssertEquals("DB 3 is new Licence and should change label to not in sync", LicenceModulesControl.NotInSyncMessage, licenceTabPage.LicenceControl.LicenceKeyInSyncLabel.Text);
				AssertEquals(Color.Red, licenceTabPage.LicenceControl.LicenceKeyInSyncLabel.ForeColor);
				AssertEquals(true, licenceTabPage.LicenceControl.LicenceKeyInSyncLabel.IsFontBold);
			}
		}

		public void TestLicenceInSyncStatusForOrgWithOutLicences()
		{
			Organisation.OH_Code = "XXY";
			Organisation.CreateAndLoadLicenceForOrg();
			Factory.Save();

			using (var formForTest = new EDIOrganisationFormForTest(Organisation))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				AssertEquals("0 DB in Ord Licence", 0, controlForTest.DatabasesModuleButtonGridForTest.InnerGrid.List.Count);

				controlForTest.LicenceTabControlForTest.SelectedIndex = 0;
				Application.DoEvents();
			}
		}

		public void TestLicenceInSyncStatusWithInactiveDb()
		{
			Organisation.OH_Code = "XXY";
			Organisation.CreateAndLoadLicenceForOrg();
			LicenceDatabase inactiveDBWithLicenceKeyNotInSync = Organisation.LicCompany.LicDatabases.AddNew();
			LicenceDatabase activeDBWithLicenceKeyInSync = Organisation.LicCompany.LicDatabases.AddNew();
			inactiveDBWithLicenceKeyNotInSync.LD_Product = ProductTypes.Codes.Enterprise;
			activeDBWithLicenceKeyInSync.LD_Product = ProductTypes.Codes.Enterprise;
			Organisation.LicCompany.GetHeader(activeDBWithLicenceKeyInSync).LA_LastLicenceCheckInSync = true;
			Organisation.LicCompany.GetHeader(inactiveDBWithLicenceKeyNotInSync).LA_LastLicenceCheckInSync = false;
			inactiveDBWithLicenceKeyNotInSync.LD_IsActive = false;
			inactiveDBWithLicenceKeyNotInSync.LD_ServerCode = "DB1";
			activeDBWithLicenceKeyInSync.LD_ServerCode = "DB2";
			Factory.Save();

			using (var formForTest = new EDIOrganisationFormForTest(Organisation))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				AssertEquals("active DB in Org Licence", 1, controlForTest.DatabasesModuleButtonGridForTest.InnerGrid.List.Count);

				controlForTest.LicenceTabControlForTest.SelectedIndex = 1;

				Application.DoEvents();

				controlForTest.DatabasesModuleButtonGridForTest.InnerGrid.ListManager.Position = 0;
				AssertEquals("DB should change label to in sync", LicenceModulesControl.InSyncMessage, licenceTabPage.LicenceControl.LicenceKeyInSyncLabel.Text);
				AssertEquals(Color.Black, licenceTabPage.LicenceControl.LicenceKeyInSyncLabel.ForeColor);
				AssertEquals(false, licenceTabPage.LicenceControl.LicenceKeyInSyncLabel.IsFontBold);
			}
		}

		#endregion

		#region Auto-Deploy Licence Key

		#endregion

		#region Select Licence Database

		public void TestSelectedLicenceDatabase()
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			LicenceDatabase database1 = org.LicCompany.LicDatabases.AddNew();
			database1.FillWithValidTestData();
			database1.LD_HostedLocation = "SYD";
			database1.LicEnterprise.LE_EnterpriseCode = "ABC";
			LicenceDatabase database2 = org.LicCompany.LicDatabases.AddNew();
			database2.FillWithValidTestData();
			database2.LD_HostedLocation = "CHI";
			database2.LicEnterprise.LE_EnterpriseCode = "DEF";
			LicenceDatabase database3 = org.LicCompany.LicDatabases.AddNew();
			database3.FillWithValidTestData();
			database3.LD_HostedLocation = "CHI";
			database3.LicEnterprise.LE_EnterpriseCode = "GHI";

			EDIOrgHeader unrelatedOrg = Factory.NewWithValidTestData<EDIOrgHeader>();
			unrelatedOrg.CreateAndLoadLicenceForOrg();
			var unrelatedDatabase = BillingTestHelper.CreateLicence(Factory, "JKL", false).Database;
			unrelatedDatabase.LD_HostedLocation = "SYD";
			Factory.Save();

			using (var form = new EDIOrganisationFormForTest(org))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(form);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				var databasesGrid = controlForTest.DatabasesModuleButtonGridForTest;

				form.Show();
				form.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(form);
				org.Factory.Save();

				LicenceDatabase licenceDatabase = (LicenceDatabase)databasesGrid.InnerGrid.ListManager.List[1];
				AssertNotEquals(licenceDatabase.PK, controlForTest.SelectedLicenceDatabase.PK);
				AssertEquals(controlForTest.LicenceDatabasesListManager.GetCurrent(), controlForTest.SelectedLicenceDatabase);
				Factory.Save();

				controlForTest.SelectedLicenceDatabase = licenceDatabase;
				AssertEquals(licenceDatabase, controlForTest.LicenceDatabasesListManager.GetCurrent());

				licenceDatabase = (LicenceDatabase)databasesGrid.InnerGrid.ListManager.List[2];
				controlForTest.SelectedLicenceDatabase = licenceDatabase;
				AssertEquals(licenceDatabase, controlForTest.LicenceDatabasesListManager.GetCurrent());

				controlForTest.SelectedLicenceDatabase = unrelatedDatabase;
				AssertNotEquals(unrelatedDatabase, controlForTest.LicenceDatabasesListManager.GetCurrent());
			}
		}

		#endregion

		#region SendButton_Click

		public void TestSendButton_Click()
		{
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = LicenceDatabase.DateFromWhichLicenceIsAutoDeployable;

			Organisation.OH_FullName = "Zubins Org";
			Organisation.OH_Code = "PPP";
			Organisation.OH_RL_NKClosestPort = "AUSYD";
			Organisation.CreateAndLoadLicenceForOrg();
			Organisation.LicEnterprise.LE_EnterpriseCode = "ZUB";
			Organisation.LicCompany.LC_CompanyCode = "CCC";
			LicenceDatabase database = Organisation.LicEnterprise.Databases.AddNew();
			database.LD_HostServerName = "ZUBIN";
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "XerxesTest@edi.com.au";
			Organisation.LicCompany.LC_CompanyCountry = "AU";
			Organisation.LicCompany.LicDatabases.Add(database);
			database.LD_LE = Organisation.LicEnterprise.PK;
			Factory.Save();

			using (var formForTest = new EDIOrganisationFormForTest(Organisation))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;

				SupportIncident incident = Factory.New<SupportIncident>();
				incident.IM_IncidentNumber = "CS1092039";
				controlForTest.ContextBusinessEntity = incident;

				ZFormModaliser.LastFormShownForTest = null;
				AssertNull("SendButton_Click", ZFormModaliser.LastFormShownForTest);
				controlForTest.SendButton_ClickForTest(null, EventArgs.Empty);
				UpgradeForm form = ZFormModaliser.LastFormShownForTest as UpgradeForm;
				AssertNotNull(form);
				AssertEquals("CS1092039: ", form.BusinessEntity.NotificationSubjectPrefix);
			}
		}

		#endregion

		public void TestGetColumnsForExport()
		{
			PopulatedOrganisation.Factory.Save();
			AssertEquals(1, PopulatedOrganisation.LicCompany.ActiveOrAllLicDatabases.Count);

			using (var formForTest = new EDIOrganisationFormForTest(PopulatedOrganisation))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				var testControl = FindSubControlByName(licenceTabPage, "Grid") as ZGrid;
				var columns = testControl.GetNewColumnsForExport();

				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestLicenceHeaderBinding()
		{
			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "COM");
			var db1 = licCompany.LicDatabases.AddNew();
			var db2 = licCompany.LicDatabases.AddNew();
			var lic1 = licCompany.GetHeader(db1);
			var lic2 = licCompany.GetHeader(db2);
			db1.LD_ServerCode = "DB1";
			db2.LD_ServerCode = "DB2";
			Factory.Save();

			using (var formForTest = new EDIOrganisationFormForTest(lic1.Company.Header))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;

				formForTest.Show();
				formForTest.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				var stlLicenceControl = (StlLicenceControl)controlForTest.Controls.Find("stlLicenceControl", true)[0];
				controlForTest.LicenceTabControlForTest.SelectedIndex = controlForTest.LicenceTabControlForTest.TabPages.IndexOf(stlLicenceControl.Parent as ZTabPage);

				Application.DoEvents();

				controlForTest.DatabasesModuleButtonGridForTest.InnerGrid.ListManager.Position = 0;
				AssertEquals(lic1, stlLicenceControl.CurrentDataItem);

				controlForTest.DatabasesModuleButtonGridForTest.InnerGrid.ListManager.Position = 1;
				AssertEquals(lic2, stlLicenceControl.CurrentDataItem);
			}
		}

		public void TestLicenceTabControlShouldHideConnectionsTabPage()
		{
			var licCompany = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "COM");
			var db1 = licCompany.LicDatabases.AddNew();
			var lic1 = licCompany.GetHeader(db1);
			db1.LD_ServerCode = "DB1";
			Factory.Save();
			using (var formForTest = new EDIOrganisationFormForTest(lic1.Company.Header))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				var tabPage = controlForTest.ConnectionsTabPageForTest;
				AssertEquals("ConnectionsTabPage TabVisible should be false", false, tabPage.TabVisible);
			}
		}

		#region Always Active Controls

		public void TestAlwaysActiveControls()
		{
			Organisation.CreateAndLoadLicenceForOrg();
			Organisation.Factory.Save();
			var oldValue1 = EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed;
			var oldValue2 = EDISecurityCheckpoints.OrgLicenceModify.IsAllowed;
			try
			{
				EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = true;
				AssertAlwaysActiveControls(Organisation);

				EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = false;
				AssertAlwaysActiveControls(Organisation);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = oldValue1;
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = oldValue2;
			}
		}

		void AssertAlwaysActiveControls(EDIOrgHeader org)
		{
			using (var formForTest = new EDIOrganisationFormForTest(org))
			using (var controlForTest = new LicenceKeyBuilderControlForTest())
			{
				var licenceTabPage = GetLicenceKeyBuilderTabPage(formForTest);
				licenceTabPage.Controls.Add(controlForTest);
				licenceTabPage.LicenceControl = controlForTest;
				formForTest.Show();
				formForTest.DisplayMode = ODisplayMode.Edit;
				formForTest.OrgTabControl.SelectedIndex = GetLicenceKeyBuilderTabPageIndex(formForTest);

				var checkbox = FindSubControlByName(licenceTabPage, "ShowInactiveDatabasesCheckBox") as ZCheckBox;
				AssertEquals(false, checkbox.ReadOnly);
				AssertEquals(true, checkbox.Enabled);
				AssertEquals(true, checkbox.Visible);
			}
		}

		#endregion

		#region Implementation

		EDIOrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.NewWithValidTestData<EDIOrgHeader>();
				}
				return fOrganisation;
			}
		}
		EDIOrgHeader fOrganisation;

		protected override OrgHeader LoadAndSetupDEMOrgHeaderForTest()
		{
			EDIOrgHeader dEMORG = (EDIOrgHeader)base.LoadAndSetupDEMOrgHeaderForTest();
			dEMORG.CreateAndLoadLicenceForOrg();
			return dEMORG;
		}

		#endregion

		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new LicenceKeyBuilderControl(null);
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyLicence", "IsModifyLicenceConnectionDetails", "IsModifyLicenceInstallationDetails", "IsModifyLicenceLicenceKey", "IsModifyLicenceSendUpgrade", "IsModifyLicenceSendUpgradeToHigherRingDB", "IsModifyLicence3rdPartySoftware", "IsModifyLicenceDatabaseConfiguration" }; }
		}
	}
}

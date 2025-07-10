using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.GUI;
using Enterprise.Messaging.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ZClientEDI.GUI.Licencing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module.Testing
{
	[TestedType(typeof(LicenceEnterpriseModule))]
	public class LicenceDatabaseModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.LicenceEnterprise;
		}

		#region TestIImportCollectionInfoProvider

		public void TestIImportCollectionInfoProvider()
		{
			using (var module = new LicenceDatabaseModule())
			{
				var provider = (IImportCollectionInfoProvider)module;
				AssertEquals("LicenceDatabaseModuleImportWizard", provider.ContextKey);
				var info = provider.ImportCollectionInfo;
				AssertEquals(typeof(LicenceDatabaseImportInfo), info.GetType());
				var info2 = provider.ImportCollectionInfo;
				AssertNotEquals("Must be a different instance so the import starts fresh", info, info2);
				AssertNotEquals("Must be a different instance so the import starts fresh", info.Collection, info2.Collection);
			}
		}

		#endregion

		public void TestImportStlCustomerSettingsMenu()
		{
			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			using (var testModule = new LicenceDatabaseModuleForTest())
			{
				var menuItems = new List<MenuItem>(testModule.GetNewActionMenuItems_Exposed());
				var menu = menuItems.Find(x => x.Text == "Import STL Customer Settings");
				AssertNotNull(menu);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.PerformClick();
				AssertEquals(EDISecurityCheckpoints.OrgLicenceBilling.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				Application.DoEvents();
				var form = Application.OpenForms.OfType<MultistepDataImportWizardForm>().Single();
				AssertNotNull(form);
				form.Close();
			}
		}

		public void TestMoveDatabaseMenu()
		{
			EDISecurityCheckpoints.OrgLicenceRemoveLicenceDatabase.IsAllowed = false;
			using (var testModule = new LicenceDatabaseModuleForTest())
			{
				var menuItems = new List<MenuItem>(testModule.GetNewActionMenuItems_Exposed());
				var menu = menuItems.Find(x => x.Text == "Set Enterprise ID/Code");
				AssertNotNull(menu);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.PerformClick();
				AssertEquals(EDISecurityCheckpoints.OrgLicenceRemoveLicenceDatabase.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				EDISecurityCheckpoints.OrgLicenceRemoveLicenceDatabase.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.PerformClick();
				AssertEquals("Please select at least one database.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestLicenceDatabaseModuleForRegistration()
		{
			using (var filterControl = new LicenceDatabaseRegistrationWizardFilterControl(new LicenceDatabaseRegistrationWizard(Factory).LicenceDatabaseCollection, new LicenceDatabaseWizardFilterBusinessObject()))
			using (var module = new LicenceDatabaseModule())
			using (var moduleRegistration = new LicenceDatabaseModuleForRegistration(filterControl, filterControl.GridCollection, filterControl.FilterBusinessObject))
			{
				AssertEquals(module.ID, moduleRegistration.ID);
				AssertEquals(module.SecurityCheckpoint, moduleRegistration.SecurityCheckpoint);
				AssertEquals(module.LicenceCheckPoint, moduleRegistration.LicenceCheckPoint);
			}
		}

		public void TestRequestStaffListMenu()
		{
			var lic1 = Factory.NewWithValidTestData<LicenceDatabase>();
			lic1.LD_ServerCode = "TS1";
			lic1.LD_Product = "PRD";
			var licHeader1 = Factory.NewWithValidTestData<LicenceHeader>();
			licHeader1.LA_IsActive = true;
			licHeader1.LA_LD = lic1.PK;
			Factory.Save();

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			using (var testModule = new LicenceDatabaseModuleForTest())
			using (var formTest = new ZForm())
			{
				formTest.Controls.Add(testModule.EmbeddedControl);
				formTest.Show();
				testModule.SetFormsModalTo(formTest);
				Application.DoEvents();

				testModule.PerformSearch_ForTest();
				testModule.LicenceDatabaseEmbeddedControl_Exposed.FilteredGrid.SelectSingleElementByPK(lic1.PK);

				var menuItems = new List<MenuItem>(testModule.GetNewStandardMenuItems_Exposed());
				var menu = menuItems.Find(x => x.Text == "Request Staff List");
				AssertNotNull(menu);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains($"A request for Staff List has been sent to the licence(s) below:\r\nTS1 - PRD - {licHeader1.LicenceCode}"));

				var query = new ZQuery();
				query.AddToFilter(EDIInterchangeSchema.EI_To, lic1.LicenceCodeForSystemMessage);
				Assert(Factory.Exists(typeof(EDIInterchange), query));

				menu.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains($"A request for Staff List for the licence(s) below was recently sent, please wait and try again later:\r\nTS1 - PRD - {licHeader1.LicenceCode}"));
			}
		}

		class LicenceDatabaseModuleForTest : LicenceDatabaseModule
		{
			internal LicenceDatabaseFilterControl LicenceDatabaseEmbeddedControl_Exposed => (LicenceDatabaseFilterControl)EmbeddedControl;
			internal MenuItem[] GetNewActionMenuItems_Exposed() => base.GetNewActionMenuItems();
			internal MenuItem[] GetNewStandardMenuItems_Exposed() => base.GetNewStandardMenuItems();
		}

		[TestedType(typeof(LicenceDatabaseOpAccSupporter))]
		internal sealed class LicenceDatabaseOpAccSupporterTest : OperationalActionSupporterTest<LicenceDatabaseOpAccSupporter>
		{
			protected override SecurityCheckpoint ExpectedCustomizationSecurityCheckpoint => EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails;
			protected override SecurityCheckpoint ExpectedRunSecurityCheckpoint => EDISecurityCheckpoints.OrgLicenceModifyDatabaseDetails;
			protected override ModuleIdentifier ModuleID => Modules.ClientModuleRegistration.LicenceDatabase;
			public override bool ShouldSupportDocuments => false;
			public new void TestSecurityCheckPoint()
			{
				//Unit Test failed. Is it necessary to create the new Security Checkpoints?
				//Message: The CustomizationSecurityCheckpoint needs to be a grandchild checkpoint of the module security checkpoint and needs to be named "Customize Actions".
				//Enterprise.Security.SecurityCheckpoint:
				//Maintain -> Reference Files -> Organization -> View -> * -> Customize Actions
				AssertEquals("CustomizationSecurityCheckpoint", ExpectedCustomizationSecurityCheckpoint, Supporter.CustomizationSecurityCheckpoint);
				AssertEquals("RunSecurityCheckpoint", ExpectedRunSecurityCheckpoint, Supporter.RunSecurityCheckpoint);
			}
		}
	}
}

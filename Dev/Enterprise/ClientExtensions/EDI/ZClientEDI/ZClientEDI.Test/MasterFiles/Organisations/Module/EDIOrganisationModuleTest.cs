using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using ZClientEDI.GUI.Licencing;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDIOrganisationModule))]
	public class EDIOrganisationModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Organisation;
		}

		public void TestUpgradeMenuShown()
		{
			bool oldSend = EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed;
			EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = true;
			try
			{
				EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed = true;
				using (var testModule = new EdiOrgModuleForTest())
				{
					var menuItems = new List<MenuItem>(testModule.GetNewActionMenuItems_Exposed());
					var foundItem = menuItems.Find(MenuItemIsUpgrade);
					AssertNotNull("The Item for Upgrades was found", foundItem);
				}

				EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed = false;
				using (var testModule = new EdiOrgModuleForTest())
				{
					var menuItems = new List<MenuItem>(testModule.GetNewActionMenuItems_Exposed());
					var foundItem = menuItems.Find(MenuItemIsUpgrade);
					AssertNull("The Item for Upgrades was null because it was not found", foundItem);
				}
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed = oldSend;
			}
		}

		public void TestImportLicenceBillingMenu()
		{
			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			using (var testModule = new EdiOrgModuleForTest())
			{
				var menuItems = new List<MenuItem>(testModule.GetNewActionMenuItems_Exposed());
				var menu = menuItems.Find(x => x.Text == "Import Licence Billing");
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

		public void TestOrganisationModuleForRegistration()
		{
			using (var filterControl = new OrganisationWizardFilterControl(new LicenceDatabaseRegistrationWizard(Factory), new OrganisationWizardFilterBusinessObject()))
			using (var module = new OrganisationModule())
			using (var moduleRegistration = new OrganisationModuleForRegistration(filterControl, filterControl.GridCollection, filterControl.FilterBusinessObject))
			{
				AssertEquals(module.ID, moduleRegistration.ID);
				AssertEquals(module.SecurityCheckpoint, moduleRegistration.SecurityCheckpoint);
				AssertEquals(module.LicenceCheckPoint, moduleRegistration.LicenceCheckPoint);
			}
		}

		bool MenuItemIsUpgrade(MenuItem currentItem)
		{
			return currentItem.Text == "Upgrades";
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			// The test db will be full of OrgHeaders, so an additional identifier is needed to filter by in order to avoid test problems.
			var org = (OrgHeader)factory.NewWithValidTestData(businessObjectType);
			org.OH_FullName = filterStripHelperTestOrgName;
			return org;
		}

		protected override void CustomiseFilterForFilterStripsHelperTests(FilterStripBusinessObject filterBusinessObject)
		{
			base.CustomiseFilterForFilterStripsHelperTests(filterBusinessObject);
			var filter = (ModuleTextFilter)filterBusinessObject["Name"];
			filter.IsActive = true;
			filter.Property = filterStripHelperTestOrgName;
		}

		const string filterStripHelperTestOrgName = "MODULE BASHER TEST";
		class EdiOrgModuleForTest : EDIOrganisationModule
		{
			internal MenuItem[] GetNewActionMenuItems_Exposed() => base.GetNewActionMenuItems();
		}
	}
}

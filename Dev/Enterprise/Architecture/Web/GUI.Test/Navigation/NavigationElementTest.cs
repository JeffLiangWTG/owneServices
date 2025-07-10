using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using WTG.WebSecurityRight;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class NavigationElementTest : TestCaseWithFactory
	{
		#region Setup

		ZWebTestHelper Helper;

		protected override void SetUp()
		{
			base.SetUp();

			Helper = new ZWebTestHelper(Factory);
			Factory.Save();

			Helper.TestSiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
		}
		#endregion

		#region TestIsDisabledWithNullArgs

		public void TestIsDisabledWithNullArgs()
		{
			NavigationElement testElement = new NavigationElement("Test", "TestPage.aspx", null, null, null);

			Assert("Element should not be disabled when security args are null", testElement.Visible);
		}
		#endregion

		#region TestIsDisabledWithRegistryItem

		public void TestIsDisabledWithRegistryItem()
		{
			BooleanRegistryItem testRegistryItem = new BooleanRegistryItem("Test", (NoResString)"TestCategory", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.System, true);

			NavigationElement testElement = new NavigationElement("Test", "TestPage.aspx", testRegistryItem, null, null);
			Assert("Element should not be disabled", testElement.Visible);

			testRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert("Element should be disabled", !testElement.Visible);
		}
		#endregion

		#region TestIsDisabledWithRegistryItemAndSecurityRight

		public void TestIsDisabledWithRegistryItemAndSecurityRight()
		{
			OrgSecurity orgRight = Helper.TestOrg.SecurityRights[0];
			BooleanRegistryItem testRegistryItem = new BooleanRegistryItem(orgRight.OX_SecurityItemName, (NoResString)"TestCategory", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.System, true);
			WebSecurityRight testWebRight = new WebSecurityRight(orgRight.OX_SecurityItemName, (NoResString)orgRight.OX_SecurityItemName, WebSecurityApplication.EdiWebTracker);

			Helper.TestContact.OC_WebAccessEnabled = true;

			NavigationElement testElement = new NavigationElement(orgRight.OX_SecurityItemName, "TestPage.aspx", testRegistryItem, testWebRight, null);
			Assert("Element should not be disabled", testElement.Visible);

			orgRight.OX_Granted = true;

			OrgSecurityContacts contactRight = Helper.TestContact.SecurityRightsForBindingOnly[0];
			contactRight.OZ_Granted = true;
			contactRight.OZ_OX = orgRight.PK;

			Assert("Contact should have security rights", Helper.TestContact.SecurityRightsForBindingOnly.IsRightGranted(testWebRight));
			Assert("Org doesn't have new security right granted", Helper.TestOrg.SecurityRights.IsRightGranted(testWebRight));
			Assert("Element should not be disabled", testElement.Visible);

			orgRight.OX_Granted = false;
			contactRight.OZ_Granted = true;

			Factory.Save();
			Assert("Org security right should now be removed", !Helper.TestOrg.SecurityRights.IsRightGranted(testWebRight));
			Assert("Contact security right should still be enabled", Helper.TestContact.SecurityRightsForBindingOnly.IsRightGranted(testWebRight));
			Assert("Element should still be enabled as Contact is null", testElement.Visible);

			contactRight.OZ_Granted = false;

			Factory.Save();
			Assert("Org security right should now be removed", !Helper.TestOrg.SecurityRights.IsRightGranted(testWebRight));
			Assert("Contact security right should now be removed", !Helper.TestContact.SecurityRightsForBindingOnly.IsRightGranted(testWebRight));
			Assert("Element should still be enabled as Contact is null", testElement.Visible);
		}
		#endregion

		#region TestIsDisabledWithRegistryItemSecurityRightAndValidContactWithNoExplicitRights

		public void TestIsDisabledWithRegistryItemSecurityRightAndValidContactWithNoExplicitRights()
		{
			OrgSecurity orgRight = Helper.TestOrg.SecurityRights[0];
			BooleanRegistryItem testRegistryItem = new BooleanRegistryItem(orgRight.OX_SecurityItemName, (NoResString)"TestCategory", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.System, true);
			WebSecurityRight testWebRight = new WebSecurityRight(orgRight.OX_SecurityItemName, (NoResString)orgRight.OX_SecurityItemName, WebSecurityApplication.EdiWebTracker);

			NavigationElement testElement = new NavigationElement(orgRight.OX_SecurityItemName, "TestPage.aspx", testRegistryItem, testWebRight, Helper.TestSiteUser);
			Assert("Element should not be disabled", testElement.Visible);

			orgRight.OX_Granted = true;

			Factory.Save();
			Helper.TestSiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
			testElement = new NavigationElement("Test", "TestPage.aspx", testRegistryItem, testWebRight, Helper.TestSiteUser);

			Assert("Contact should have security rights", Helper.TestContact.SecurityRightsForBindingOnly.IsRightGranted(testWebRight));
			Assert("Org should have security right granted", Helper.TestOrg.SecurityRights.IsRightGranted(testWebRight));
			Assert("Element should not be disabled", testElement.Visible);

			orgRight.ContactSecurityRights.ToString(); // Hack
			orgRight.OX_Granted = false;

			Factory.Save();
			Helper.TestSiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
			testElement = new NavigationElement("Test", "TestPage.aspx", testRegistryItem, testWebRight, Helper.TestSiteUser);

			Assert("Org security right should now be removed", !Helper.TestOrg.SecurityRights.IsRightGranted(testWebRight));
			Assert("Contact should have also lost security rights", !Helper.TestContact.SecurityRightsForBindingOnly.IsRightGranted(testWebRight));
			Assert("Element should now be disabled", !testElement.Visible);
		}
		#endregion

		#region TestIsDisabledWithRegistryItemSecurityRightAndValidOrgWithExplicitRights

		public void TestIsDisabledWithRegistryItemSecurityRightAndValidOrgWithExplicitRights()
		{
			OrgSecurity orgRight = Helper.TestOrg.SecurityRights[0];
			BooleanRegistryItem testRegistryItem = new BooleanRegistryItem(orgRight.OX_SecurityItemName, (NoResString)"TestCategory", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.System, true);
			WebSecurityRight testWebRight = new WebSecurityRight(orgRight.OX_SecurityItemName, (NoResString)orgRight.OX_SecurityItemName, WebSecurityApplication.EdiWebTracker);

			NavigationElement testElement = new NavigationElement("Test", "TestPage.aspx", testRegistryItem, testWebRight, Helper.TestSiteUser);
			Assert("Element should not be disabled", testElement.Visible);

			orgRight.OX_Granted = true;

			OrgSecurityContacts contactRight = Helper.TestContact.SecurityRightsForBindingOnly[0];
			contactRight.OZ_Granted = true;
			contactRight.OZ_OX = orgRight.PK;

			Factory.Save();
			Helper.TestSiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
			testElement = new NavigationElement(orgRight.OX_SecurityItemName, "TestPage.aspx", testRegistryItem, testWebRight, Helper.TestSiteUser);

			Assert("Contact should have security right granted", Helper.TestContact.SecurityRightsForBindingOnly.IsRightGranted(testWebRight));
			Assert("Org should have security right granted", Helper.TestOrg.SecurityRights.IsRightGranted(testWebRight));
			Assert("Element should not be disabled", testElement.Visible);

			orgRight.OX_Granted = false;
			contactRight.OZ_Granted = true;

			Factory.Save();
			Helper.TestSiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
			testElement = new NavigationElement(orgRight.OX_SecurityItemName, "TestPage.aspx", testRegistryItem, testWebRight, Helper.TestSiteUser);

			Assert("Org security right should now be denied", !Helper.TestOrg.SecurityRights.IsRightGranted(testWebRight));
			Assert("Contact security right should still be granted", Helper.TestContact.SecurityRightsForBindingOnly.IsRightGranted(testWebRight));
			Assert("Element should still be enabled as Contact has security right enabled", testElement.Visible);

			orgRight.OX_Granted = true;
			contactRight.OZ_Granted = false;

			Factory.Save();
			Helper.TestSiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
			testElement = new NavigationElement(orgRight.OX_SecurityItemName, "TestPage.aspx", testRegistryItem, testWebRight, Helper.TestSiteUser);

			Assert("Org security right should be granted", Helper.TestOrg.SecurityRights.IsRightGranted(testWebRight));
			Assert("Contact security right should now be denied", !Helper.TestContact.SecurityRightsForBindingOnly.IsRightGranted(testWebRight));
			Assert("Element should not be enabled as contact does not have security right granted", !testElement.Visible);

			orgRight.OX_Granted = false;

			Factory.Save();
			Helper.TestSiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
			testElement = new NavigationElement(orgRight.OX_SecurityItemName, "TestPage.aspx", testRegistryItem, testWebRight, Helper.TestSiteUser);

			Assert("Org security right should now be removed", !Helper.TestOrg.SecurityRights.IsRightGranted(testWebRight));
			Assert("Contact security right should now be removed", !Helper.TestContact.SecurityRightsForBindingOnly.IsRightGranted(testWebRight));
			Assert("Element should now be disabled as both rights are removed", !testElement.Visible);
		}

		#endregion
	}
}

using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AccessControlRegistryItem))]
	sealed class AccessControlRegistryItemTest : StronglyTypedRegistryItemTestCase<OrgsRoleAccessCollection>
	{
		public void TestTextNeedsToBeSuppressed()
		{
			AccessControlRegistryItem testItem = new AccessControlRegistryItem("testName", (NoResString)"testCategory", (NoResString)"testCaption", (NoResString)"testHint", new DummyAccessRules(), RegistryOptions.Default);

			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyAccessControlled bizO = factory.New<DummyAccessControlled>();
			bizO.SuppressionItem = testItem;
			bizO.IsInTextSuppressionMode = true;
			bizO.LoggedInOrgsRoles = new[] { DummyAccessRules.Roles.role2, DummyAccessRules.Roles.role3 };

			Assert("BizO isn't in DB and shouldn't be suppressed", !testItem.TextNeedsToBeSuppressed(bizO, DummyAccessControlled.Schema.Z0_Date));
			Assert("BizO isn't in DB and shouldn't be suppressed", !testItem.TextNeedsToBeSuppressed(bizO, DummyAccessControlled.Schema.Z0_VarCharMax));

			factory.Save();

			Assert("Should be suppressed according to DummyAccessRules", testItem.TextNeedsToBeSuppressed(bizO, DummyAccessControlled.Schema.Z0_Date));
			Assert("Should be suppressed according to DummyAccessRules", testItem.TextNeedsToBeSuppressed(bizO, DummyAccessControlled.Schema.Z0_VarCharMax));
			Assert("Shouldn't be suppressed according to DummyAccessRules", !testItem.TextNeedsToBeSuppressed(bizO, DummyAccessControlled.Schema.Z0_Code));

			string bla = ".bla";

			Assert("Should be suppressed according to DummyAccessRules", testItem.TextNeedsToBeSuppressed(bizO, DummyAccessControlled.Schema.Z0_Date + bla));
			Assert("Should be suppressed according to DummyAccessRules", testItem.TextNeedsToBeSuppressed(bizO, DummyAccessControlled.Schema.Z0_VarCharMax + bla));
			Assert("Shouldn't be suppressed according to DummyAccessRules", !testItem.TextNeedsToBeSuppressed(bizO, DummyAccessControlled.Schema.Z0_Code + bla));

			bla = "bla.";

			Assert("Shouldn't be suppressed according to DummyAccessRules", !testItem.TextNeedsToBeSuppressed(bizO, bla + DummyAccessControlled.Schema.Z0_Date));
			Assert("Shouldn't be suppressed according to DummyAccessRules", !testItem.TextNeedsToBeSuppressed(bizO, bla + DummyAccessControlled.Schema.Z0_VarCharMax));
			Assert("Shouldn't be suppressed according to DummyAccessRules", !testItem.TextNeedsToBeSuppressed(bizO, bla + DummyAccessControlled.Schema.Z0_Code));

			Assert("Shouldn't be suppressed if not found", !testItem.TextNeedsToBeSuppressed(bizO, "bla"));

			bizO.IsInTextSuppressionMode = false;

			Assert("Shouldn't be suppressed if isn't in TextSuppressionMode", !testItem.TextNeedsToBeSuppressed(bizO, DummyAccessControlled.Schema.Z0_Date));
			Assert("Shouldn't be suppressed if isn't in TextSuppressionMode", !testItem.TextNeedsToBeSuppressed(bizO, DummyAccessControlled.Schema.Z0_VarCharMax));
			Assert("Shouldn't be suppressed if isn't in TextSuppressionMode", !testItem.TextNeedsToBeSuppressed(bizO, DummyAccessControlled.Schema.Z0_Code));
		}

		public void TestRolesWithAccess()
		{
			AccessControlRegistryItem testItem = new AccessControlRegistryItem("testName", (NoResString)"testCategory", (NoResString)"testCaption", (NoResString)"testHint", new DummyAccessRules(), RegistryOptions.Default);
			AssertCollectionContains(DummyAccessRules.Roles.role1, testItem.RolesWithAccess());
			AssertCollectionContains(DummyAccessRules.Roles.role2, testItem.RolesWithAccess());
			AssertCollectionNotContains(DummyAccessRules.Roles.role3, testItem.RolesWithAccess());
		}

		public void TestCaptionsToHide()
		{
			AccessControlRegistryItem testItem = new AccessControlRegistryItem("testName", (NoResString)"testCategory", (NoResString)"testCaption", (NoResString)"testHint", new DummyAccessRules(), RegistryOptions.Default);

			BusinessObjectFactory factory = new BusinessObjectFactory();

			DummyAccessControlled bizO = factory.New<DummyAccessControlled>();
			bizO.SuppressionItem = testItem;
			bizO.LoggedInOrgsRoles = new[] { DummyAccessRules.Roles.role2, DummyAccessRules.Roles.role3 };

			string[] captionsToHide = testItem.CaptionsToHide(bizO);
			AssertEquals("Nothing to hide on new business objects", 0, captionsToHide.Length);

			factory.Save();

			captionsToHide = testItem.CaptionsToHide(bizO);
			AssertEquals(2, captionsToHide.Length);
			AssertCollectionContains("caption1 should be hidden according to given roles and DummyAccessRules", DummyAccessRules.Captions.caption1, captionsToHide);
			AssertCollectionContains("caption2 should be hidden according to given roles and DummyAccessRules", DummyAccessRules.Captions.caption2, captionsToHide);

			bizO.LoggedInOrgsRoles = System.Array.Empty<string>();
			captionsToHide = testItem.CaptionsToHide(bizO);

			AssertEquals(4, captionsToHide.Length);
			AssertCollectionContains("All captions should be hidden when the org is not in any role", DummyAccessRules.Captions.caption1, captionsToHide);
			AssertCollectionContains("All captions should be hidden when the org is not in any role", DummyAccessRules.Captions.caption2, captionsToHide);
			AssertCollectionContains("All captions should be hidden when the org is not in any role", DummyAccessRules.Captions.caption3, captionsToHide);
			AssertEquals("'*EverythingIsSuppressed*' const should be added to a collection when all captions are hidden. Then the page will be aware and should show appropriate authorisation message", AccessControlRegistryItem.EverythingIsSuppressed, captionsToHide[0]);
		}

		protected override StronglyTypedRegistryItem<OrgsRoleAccessCollection, OrgsRoleAccessCollection> GetNewRegistryItem()
		{
			return new AccessControlRegistryItem(string.Empty, null, null, null, new DummyAccessRules(), RegistryOptions.Default);
		}
	}
}

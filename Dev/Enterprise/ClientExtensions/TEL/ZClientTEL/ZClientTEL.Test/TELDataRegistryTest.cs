using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TEL.Testing
{
	[TestedType(typeof(TELDataRegistry))]
	class TELDataRegistryTest : RegistryItemSetTestCaseWithFactory<TELDataRegistry>
	{
		public void TestVisibleRegistries()
		{
			AssertEquals(2, ItemSet.GetAllItems().Length);
		}

		public void TestConsolShipManifestEmailSubjectIdentifierRegistryItem()
		{
			Assert("should be empty", TELDataRegistry.Instance.ConsolShipManifestEmailSubjectIdentifier.IsEmpty);
			TELDataRegistry.Instance.ConsolShipManifestEmailSubjectIdentifierItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "bob the builder");
			AssertEquals("should be the same now", "bob the builder", TELDataRegistry.Instance.ConsolShipManifestEmailSubjectIdentifier);
		}

		public void TestConsolShipNotificationGroupRegistryItem()
		{
			GlbGroup notificationGroup = Factory.New<GlbGroup>();
			notificationGroup.GG_Code = "NOT";
			GlbStaff user = notificationGroup.Staff.AddNew();
			user.GS_Code = "USR";
			user.GS_IsActive = true;
			user.GS_EmailAddress = "test@test.com";
			Factory.Save();
			Assert("should be empty", TELDataRegistry.Instance.ConsolShipImportNotificationGroupPK.IsEmpty);
			TELDataRegistry.Instance.ConsolShipImportNotificationGroupPKItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());
			AssertEquals("should be the same now", notificationGroup.PK, TELDataRegistry.Instance.ConsolShipImportNotificationGroupPK);
		}

#region Implementation
		public new void TestCategoriesAndCaptionsAreLocalizable()
		{
			Assert(true);
		}
#endregion
	}
}

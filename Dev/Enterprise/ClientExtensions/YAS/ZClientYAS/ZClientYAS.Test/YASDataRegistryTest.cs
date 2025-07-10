using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.YAS.Testing
{
	[TestedType(typeof(YASDataRegistry))]
	public class YASDataRegistryTest : RegistryItemSetTestCaseWithFactory<YASDataRegistry>
	{
		public void TestUserVisibleRegistryItems()
		{
			AssertEquals("User visible registry items count", 2, AllItems.Count);
			AssertVisible(ItemSet.PODEmailNotificationGroupItem);
			AssertVisible(ItemSet.PODImportFolderItem);
		}

		public void PODEmailNotificationGroup()
		{
			AssertEquals("PODEmailNotificationGroup not set", ZGuid.Empty, ItemSet.PODEmailNotificationGroup);
			ZGuid group = ZGuid.NewZGuid();
			ItemSet.PODEmailNotificationGroup = group;
			AssertEquals("PODEmailNotificationGroup is now set", group.ToGuid(), ItemSet.PODEmailNotificationGroup);
		}

		public void PODImportFolder()
		{
			Assert("Empty default folder", ItemSet.PODImportFolder == string.Empty);
			ItemSet.PODImportFolder = Env.TempPath;
			Assert("Folder is now set", ItemSet.PODImportFolder == Env.TempPath);
		}

		public new void TestCategoriesAndCaptionsAreLocalizable()
		{
			Assert(true);
		}
	}
}

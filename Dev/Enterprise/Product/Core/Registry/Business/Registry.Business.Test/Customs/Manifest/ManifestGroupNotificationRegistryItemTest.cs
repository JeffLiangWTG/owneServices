using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(ManifestGroupNotificationRegistryItem))]
	sealed class ManifestGroupNotificationRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<ManifestGroupNotification>
	{
		protected override StronglyTypedRegistryItem<ManifestGroupNotification, ManifestGroupNotification> GetNewRegistryItem()
		{
			return new ManifestGroupNotificationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, ManifestGroupNotification.Default);
		}

		public void TestSendErrorOnlyOption()
		{
			var amsRegistry = new ManifestGroupNotificationRegistryItem("DUM", (NoResString)"DUM", (NoResString)"DUM", (NoResString)"DUM", RegistryStorageFlags.System, RegistryOptions.Default, ManifestGroupNotification.Default);
			AssertEquals(false, ((ISupportMessageSuppressRegistry)amsRegistry).ShouldSEndErrorsOnly(Guid.Empty, Guid.Empty, Guid.Empty));

			amsRegistry = new ManifestGroupNotificationRegistryItem("DUM", (NoResString)"DUM", (NoResString)"DUM", (NoResString)"DUM", RegistryStorageFlags.System, RegistryOptions.Default, new ManifestGroupNotification("DUM", ZGuid.Empty, true));
			AssertEquals(true, ((ISupportMessageSuppressRegistry)amsRegistry).ShouldSEndErrorsOnly(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestSendErrorOnly_AlwaysReturnsFalseWhenSendModeIsNOE()
		{
			var registry = new ManifestGroupNotificationRegistryItem("DUM", (NoResString)"DUM", (NoResString)"DUM", (NoResString)"DUM", RegistryStorageFlags.System, RegistryOptions.Default, new ManifestGroupNotification("ESG", ZGuid.Empty, true));
			AssertEquals(true, ((ISupportMessageSuppressRegistry)registry).ShouldSEndErrorsOnly(Guid.Empty, Guid.Empty, Guid.Empty));

			registry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new ManifestGroupNotification(Core.Constants.EmailTo.NoEmails, ZGuid.Empty, true));
			AssertEquals(false, ((ISupportMessageSuppressRegistry)registry).ShouldSEndErrorsOnly(Guid.Empty, Guid.Empty, Guid.Empty));
		}
	}
}

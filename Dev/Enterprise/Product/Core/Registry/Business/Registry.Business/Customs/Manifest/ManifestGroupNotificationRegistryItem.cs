using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs
{
	public class ManifestGroupNotificationRegistryItem : StronglyTypedRegistryItem<ManifestGroupNotification>, ISupportMessageSuppressRegistry
	{
		public ManifestGroupNotificationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ManifestGroupNotification defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ManifestGroupNotificationRegistryDataType(), storage, options, defaultValue))
		{
		}

		[RegistryEditor("Enterprise.Registry.GUI.ManifestGroupNotificationRegistryItemEditor, Enterprise.Registry.GUI")]
		public class ManifestGroupNotificationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ManifestGroupNotification>
		{
		}

		#region ISupportMessageSuppressRegistry Members

		ZBool ISupportMessageSuppressRegistry.ShouldSEndErrorsOnly(ZGuid companyPK, ZGuid branchPK, ZGuid departmentPK)
		{
			var companyGuid = companyPK == ZGuid.Empty ? Guid.Empty : companyPK.ToGuid();
			var branchGuid = branchPK == ZGuid.Empty ? Guid.Empty : branchPK.ToGuid();
			var departmentGuid = departmentPK == ZGuid.Empty ? Guid.Empty : departmentPK.ToGuid();

			return GetFallBackValueAtAllLevels(companyGuid, branchGuid, departmentGuid).SendErrorOnly;
		}

		#endregion
	}
}

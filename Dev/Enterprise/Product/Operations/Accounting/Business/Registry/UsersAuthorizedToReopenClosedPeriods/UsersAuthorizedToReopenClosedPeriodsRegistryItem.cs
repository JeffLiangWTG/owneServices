using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class UsersAuthorizedToReopenClosedPeriodsRegistryItem : StronglyTypedRegistryItem<UsersAuthorizedToReopenClosedPeriodsCollection>
	{
		public UsersAuthorizedToReopenClosedPeriodsRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage, bool isCWSupportOnly)
			: base(isCWSupportOnly ?
						new RegistryItemImpl(name,
											 category,
											 (NoResString)caption,
											 (NoResString)hint,
											 new UsersAuthorizedToReopenClosedPeriodssRegistryDataType(),
											 storage,
											 RegistryOptions.IsOnlyForDevelopers)
						:

						new RegistryItemImpl(name,
											 category,
											 ResString.GetMultilingualString("4417afba-68c8-48ab-af0a-ab5e1780f702", "{0}", caption),
											 ResString.GetMultilingualString("e36852b0-d58d-4c94-9374-41935e901176", "{0}", hint),
											 new UsersAuthorizedToReopenClosedPeriodssRegistryDataType(),
											 storage,
											 RegistryOptions.Default)
				  )
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.UsersAuthorizedToReopenClosedPeriodsRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class UsersAuthorizedToReopenClosedPeriodssRegistryDataType : NonPersistentBusinessObjectRegistryDataType<UsersAuthorizedToReopenClosedPeriodsCollection>
	{
		public UsersAuthorizedToReopenClosedPeriodssRegistryDataType()
		{
		}
	}
}

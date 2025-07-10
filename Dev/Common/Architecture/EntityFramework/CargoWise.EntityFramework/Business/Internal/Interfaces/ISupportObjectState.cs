using System;

namespace CargoWise.EntityFramework
{
	public interface IBusinessObjectState
	{
		void IncrementReadOnlyIncludingChildren();
		void DecrementReadOnlyIncludingChildren(bool decrementToZero = false);
		bool HasChanges { get; set; }
		bool HasChangesNotIncludingChildren { get; }
		uint LastChangeNumber { get; }
		bool IsInDatabase { get; }
		bool IsInDatabaseIncludingChildren { get; }
		void RefreshBindingIncludingChildren();
		void ClearHasChangesIncludingChildren();

		event EventHandler<HasChangesChangedEventArgs> HasChangesChanged;
		[Obsolete("This will not work on ActiveBusinessObjectCollections. Consider overriding OnUpdatedByDataRefresh on the business objects that will be refreshed instead.")]
		event EventHandler UpdatedByDataRefreshIncludingChildren;
		event EventHandler<NotificationsChangedEventArgs> NotificationsChanged;
	}

	public static class IBusinessObjectStateExtensions
	{
		public static void SetCountedReadOnlyIncludingChildren(this IBusinessObjectState business, bool readOnly)
		{
			if (readOnly)
			{
				business.IncrementReadOnlyIncludingChildren();
			}
			else
			{
				business.DecrementReadOnlyIncludingChildren();
			}
		}
	}

	public interface ICanSuspendSettingHasChanges
	{
		IDisposable SuspendSettingHasChanges();
	}
}

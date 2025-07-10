using CargoWise.EntityFramework;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class BindableNotificationsCollection : NonPersistentBusinessObjectCollection<BindableNotification>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return null;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}

using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public abstract class ControlCustomisationBaseCollection<T> : NonPersistentBusinessObjectCollection<T>
		where T : ControlCustomisationBase
	{
		protected ControlCustomisationBaseCollection(BMControlCustomisation parent)
		{
			this.parent = parent;
		}

		protected BMControlCustomisation Parent
		{
			get { return parent; }
		}

		readonly BMControlCustomisation parent;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var bizo = (T)bizOAdded;
			using (bizo.SuspendSettingHasChanges())
			{
				bizo.SetDefaultsAfterAddedToCollection();
			}
		}
	}
}

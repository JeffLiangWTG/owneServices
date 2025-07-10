#if DEBUG

namespace CargoWise.EntityFramework.Testing
{
	public class DummyDependentBusinessObjectCollection : DependentBusinessObjectCollection<DummyDependantBusinessObject, DummyBaseBusinessObject>
	{
		public DummyDependentBusinessObjectCollection(DummyBaseBusinessObject parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		public DummyDependentBusinessObjectCollection(DummyBaseBusinessObject parent, ZQuery filter) : base(parent, filter)
		{
		}

		public DummyDependentBusinessObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			OnAddedCalledByDataRefresh = IsUpdatingByDataRefreshBus;
		}

		public bool? OnAddedCalledByDataRefresh;
	}
}


#endif

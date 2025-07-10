using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GridTestFieldBusinessObjectCollection : NonPersistentBusinessObjectCollection<GridTestFieldBusinessObject>
	{
		public GridTestFieldBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GridTestFieldBusinessObject(Factory);
		}
	}
}

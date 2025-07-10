using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GridTestParentBusinessObject : NonPersistentBusinessObject
	{
		public GridTestParentBusinessObject()
		{
		}

		public GridTestParentBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GridTestFieldBusinessObjectCollection Fields
		{
			get
			{
				if (fields == null)
				{
					fields = new GridTestFieldBusinessObjectCollection(Factory);
				}
				return fields;
			}
		}
		GridTestFieldBusinessObjectCollection fields;
	}
}

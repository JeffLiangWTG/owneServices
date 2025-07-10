using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentLinkDependentCollection : ActiveBusinessObjectCollection<BMComponentLink>
	{
		public BMComponentLinkDependentCollection(BMComponent component, SchemaGuidColumn directionColumn)
			: base(component.Factory, component, new ZQuery(), directionColumn)
		{
		}

		protected override void SetDefaultsForNewElementCore(BMComponentLink newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			var componentFrom = newElement.ComponentFrom;
			if (componentFrom != null)
			{
				newElement.ComponentToSystemPK = componentFrom.FC_FS_System;
			}
		}
	}
}

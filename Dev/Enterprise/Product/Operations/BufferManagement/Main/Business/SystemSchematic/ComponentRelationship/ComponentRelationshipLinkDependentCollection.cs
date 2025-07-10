using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ComponentRelationshipLinkDependentCollection : ActiveBusinessObjectCollection<ComponentRelationshipLink>
	{
		public ComponentRelationshipLinkDependentCollection(ComponentRelationship component, SchemaGuidColumn directionColumn)
			: base(component.Factory, component, new ZQuery(), directionColumn)
		{
		}
	}
}

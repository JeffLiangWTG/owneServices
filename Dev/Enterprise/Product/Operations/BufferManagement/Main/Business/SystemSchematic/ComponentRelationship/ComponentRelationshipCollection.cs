using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.ComponentRelationship)]
	public class ComponentRelationshipCollection : ActiveBusinessObjectCollection<ComponentRelationship>
	{
		public ComponentRelationshipCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.ComponentRelationship);
		}
	}
}

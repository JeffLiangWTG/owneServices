using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.BMTagDefinition)]
	public class TagDefinitionCollection : ActiveBusinessObjectCollection<TagDefinition>, ITagDefinitionCollection
	{
		public TagDefinitionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TagDefinitionCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}

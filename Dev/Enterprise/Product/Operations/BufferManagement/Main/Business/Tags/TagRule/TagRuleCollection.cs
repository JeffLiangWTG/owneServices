using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.BMTagRule)]
	public class TagRuleCollection : ActiveBusinessObjectCollection<TagRule>
	{
		public TagRuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

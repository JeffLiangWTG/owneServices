using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.BMReleaseSequence)]
	public class BMReleaseSequenceCollection : ActiveBusinessObjectCollection<BMReleaseSequence>
	{
		public BMReleaseSequenceCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		public BMReleaseSequenceCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}
	}
}

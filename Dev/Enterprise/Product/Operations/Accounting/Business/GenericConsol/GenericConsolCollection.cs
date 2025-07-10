using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.GenericConsol
{
	[ModuleID(ModuleId.GenericConsol)]
	public class GenericConsolCollection : ActiveBusinessObjectCollection<GenericConsol>
	{
		public GenericConsolCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		public GenericConsolCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			return result;
		}

		protected override bool AllowNew
		{
			get	{ return false; }
		}
	}
}

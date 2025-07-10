using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.AcceptabilityBand)]
	public class BMComponentAcceptabilityBandCollection : ActiveBusinessObjectCollection<BMComponentAcceptabilityBand>
	{
		public BMComponentAcceptabilityBandCollection(BMComponent parent)
			: base(parent.Factory, parent, new ZQuery(), BMComponentAcceptabilityBandSchema.BAB_FC_Component)
		{
		}

		public BMComponentAcceptabilityBandCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public BMComponentAcceptabilityBandCollection(BusinessObjectFactory factory, IEnumerable<BMComponent> components)
			: base(factory, GetQuery(components))
		{
		}

		static ZQuery GetQuery(IEnumerable<BMComponent> components)
		{
			if (components.Any())
			{
				return new ZQuery(BMComponentAcceptabilityBandSchema.BAB_FC_Component, components.Select(c => c.PK));
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}
	}
}

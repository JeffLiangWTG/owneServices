using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WhsItemDispatchTransportationUnitWrapperCollection : GenericWrapperCollection<WhsItemDispatchTransportationUnitWrapper>
	{
		public WhsItemDispatchTransportationUnitWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsItemDispatchTransportationUnitWrapperCollection(IEnumerable<WhsItemDispatchTransportationUnit> collection, BusinessObjectFactory factory)
			: base(factory)
		{
			AddDTUsFrom(collection ?? Enumerable.Empty<WhsItemDispatchTransportationUnit>());
		}

		void AddDTUsFrom(IEnumerable<WhsItemDispatchTransportationUnit> loadLists)
		{
			foreach (WhsItemDispatchTransportationUnit dtu in loadLists)
			{
				Add(new WhsItemDispatchTransportationUnitWrapper(dtu, Factory));
			}
		}
	}
}

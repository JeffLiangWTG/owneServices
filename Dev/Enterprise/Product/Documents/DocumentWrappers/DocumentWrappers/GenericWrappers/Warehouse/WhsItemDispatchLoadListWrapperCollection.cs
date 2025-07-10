using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WhsItemDispatchLoadListWrapperCollection : GenericWrapperCollection<WhsItemDispatchLoadListWrapper>
	{
		public WhsItemDispatchLoadListWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsItemDispatchLoadListWrapperCollection(IEnumerable<WhsItemDispatchLoadList> collection, BusinessObjectFactory factory)
			: base(factory)
		{
			AddLoadListFrom(collection ?? Enumerable.Empty<WhsItemDispatchLoadList>());
		}

		void AddLoadListFrom(IEnumerable<WhsItemDispatchLoadList> loadLists)
		{
			foreach (WhsItemDispatchLoadList loadList in loadLists)
			{
				Add(new WhsItemDispatchLoadListWrapper(loadList, Factory));
			}
		}
	}
}

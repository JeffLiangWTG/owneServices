using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.Mapping;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	public class TableWrapperCollection : GenericWrapperCollection<TableWrapper>
	{
		public TableWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TableWrapperCollection(IEnumerable<MapTable> tables, BusinessObjectFactory factory)
			: base(factory)
		{
			if (tables != null)
			{
				foreach (MapTable table in tables)
				{
					this.Add(new TableWrapper(table, factory));
				}
			}
		}
	}
}

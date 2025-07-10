using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Warehouse
{
	public class WhsItemTransferLineWrapperCollection : WarehouseGenericWrapperCollection<WhsItemTransferLineWrapper>
	{
		public WhsItemTransferLineWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsItemTransferLineWrapperCollection(WhsItemTransferLineCollection collection, BusinessObjectFactory factory)
			: base(factory)
		{
			AddLinesFrom(collection ?? Enumerable.Empty<WhsItemTransferLine>(), factory);
		}

		#region Implementation

		void AddLinesFrom(IEnumerable<WhsItemTransferLine> transferLine, BusinessObjectFactory factory)
		{
			foreach (var line in transferLine)
			{
				Add(new WhsItemTransferLineWrapper(line, factory));
			}
		}

		#endregion
	}
}

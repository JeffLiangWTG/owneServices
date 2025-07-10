using System.Collections.Generic;
using System.Data;

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.Wow.Business
{
	public class WoolworthsImportedOrder : WoolworthsOrder
	{
		public WoolworthsImportedOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			OrderLineStates = new Dictionary<ZGuid, bool>();
		}

		public Dictionary<ZGuid, bool> OrderLineStates { get; set; }

		public bool IsUpdated { get; set; }
	}
}

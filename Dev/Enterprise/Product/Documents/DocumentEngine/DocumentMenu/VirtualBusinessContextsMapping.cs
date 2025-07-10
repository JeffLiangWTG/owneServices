using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;

namespace Enterprise.DocumentEngine
{
	public sealed class VirtualBusinessContextsMapping : Dictionary<ZString, ZString>
	{
		public VirtualBusinessContextsMapping()
		{
			Add(nameof(BusinessContext.SubShipment), nameof(BusinessContext.Shipment));
		}
	}
}

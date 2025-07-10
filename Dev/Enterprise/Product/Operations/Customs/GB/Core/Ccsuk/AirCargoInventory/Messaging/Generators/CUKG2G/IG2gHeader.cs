using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators
{
	public interface IG2gHeader
	{
		ZString MasterUcr { get; }
		ZString MawpAndMawn { get; }
		ZString CustomsAuthorisationReference { get; }
		ZString MasterSOE { get; }
		ZString Airport { get; }
		ZString Shed { get; }
		ZString AgentBadge { get; }
		ZString AgentType { get; }
		IEnumerable<IG2gConsignment> Consignments { get; }
	}
}

using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators
{
	public interface IG2gConsignment
	{
		ZString HouseAWBNumber { get; }
		ZString CustomsAuthorisationReference { get; }
		IEnumerable<IG2gDeclaration> Declarations { get; }
		ZString CargoWiseNumber { get; }
	}
}

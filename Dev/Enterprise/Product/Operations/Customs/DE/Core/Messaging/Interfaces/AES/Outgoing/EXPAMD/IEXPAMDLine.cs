using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface IEXPAMDLine : IAESLine
	{
		ZString CommercialReferenceNumber { get; }
		bool CommoditySpecified { get; }
		ZDecimal GrossMass { get; }
		ZDecimal NetMass { get; }
		IReadOnlyCollection<IPackage> Packages { get; }
	}
}

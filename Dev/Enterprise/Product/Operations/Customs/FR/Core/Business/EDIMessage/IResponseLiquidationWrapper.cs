using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public interface IResponseLiquidationWrapper
	{
		ZShort numart { get; }
		IEnumerable<ITaxDetailWrapper> TaxDetails { get; }
	}
}

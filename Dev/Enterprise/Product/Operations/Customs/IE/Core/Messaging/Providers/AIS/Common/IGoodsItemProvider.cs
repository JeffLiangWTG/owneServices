using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public interface IGoodsItemProvider
	{
		ZString DeclarationGoodsItemNumber { get; }

		IReadOnlyCollection<ITaxTypeProvider> TaxTypes { get; }
	}
}

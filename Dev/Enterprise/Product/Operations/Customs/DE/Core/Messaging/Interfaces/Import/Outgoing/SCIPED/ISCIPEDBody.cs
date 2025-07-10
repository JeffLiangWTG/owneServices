using System;
using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ISCIPEDBody : IMonthlyClosingDecBody
	{
		IImportParty Consignor { get; }

		Guid ConsignorPK { get; }

		IReadOnlyCollection<ISCIPEDLine> Lines { get; }
	}
}

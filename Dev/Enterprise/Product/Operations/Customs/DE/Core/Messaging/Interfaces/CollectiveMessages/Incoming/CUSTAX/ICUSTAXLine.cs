using System;
using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSTAXLine : IInboundProvider
	{
		string LineNumber { get; }

		string LineCompletionFlag { get; }

		decimal? CustomsValue { get; }

		IReadOnlyCollection<ICUSTAXLineDuty> Duties { get; }

		DateTime? ExportLimitDate { get; }
	}
}

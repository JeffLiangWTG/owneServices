using System;
using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSTAX : IDataProvider
	{
		ZString ReferenceNumber { get; }

		string MRN { get; }

		ZString CompletionFlag { get; }

		ZString LocalReferenceNumber { get; }

		DateTime? RegistrationDate { get; }

		ZDecimal TotalCustomsDutyAmount { get; }

		IReadOnlyCollection<ICUSTAXLine> Lines { get; }

		DateTime? AcceptanceDate { get; }
	}
}

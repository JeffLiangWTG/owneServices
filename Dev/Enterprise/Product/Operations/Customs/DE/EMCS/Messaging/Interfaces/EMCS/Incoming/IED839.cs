using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED839 : IEmcsDataProvider
	{
		ZString SendingCustomsOffice { get; }
		ZDate IssuanceDate { get; }
		ZString MRN { get; }
		ZString RejectionReasonCode { get; }
		IReadOnlyCollection<IEMCSEvent> RejectedEads { get; }
		ZString LocalReferenceNumber { get; }
	}
}

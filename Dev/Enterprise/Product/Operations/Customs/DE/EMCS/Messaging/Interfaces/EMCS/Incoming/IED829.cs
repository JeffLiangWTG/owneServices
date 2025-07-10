using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED829 : IEmcsDataProvider
	{
		ZString SendingCustomsOffice { get; }
		ZDate AcceptanceDate { get; }
		ZString MRN { get; }
		IReadOnlyCollection<IEMCSEvent> ExciseMovementEads { get; }
	}
}

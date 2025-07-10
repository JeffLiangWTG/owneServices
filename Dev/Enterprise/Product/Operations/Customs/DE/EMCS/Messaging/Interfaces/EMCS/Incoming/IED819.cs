using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED819 : IEmcsDataProvider
	{
		IEMCSEvent ExciseMovement { get; }

		IReadOnlyCollection<IED819Reason> AlertOrRejectionReasons { get; }
	}

	public interface IED819Reason
	{
		ZString ReasonCode { get; }
		ZString ComplementaryInformation { get; }
	}
}

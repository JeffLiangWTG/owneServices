using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging
{
	public interface IIE819 : IEMCSInboundProvider
	{
		IEMCSEvent ExciseMovementEad { get; }

		IReadOnlyCollection<IIE819Reason> AlertOrRejectionReasons { get; }
	}

	public interface IIE819Reason
	{
		ZString ReasonCode { get; }
		ZString ComplementaryInformation { get; }
	}
}

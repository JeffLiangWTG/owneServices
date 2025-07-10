using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging
{
	public interface IIE704 : IEMCSInboundProvider
	{
		ZString LocalReferenceNumber { get; }

		ZString AdministrativeReferenceCode { get; }

		IReadOnlyCollection<IFunctionalError> Errors { get; }
	}

	public interface IFunctionalError
	{
		ZString ErrorLocation { get; }

		ZString ErrorType { get; }

		ZString ErrorReason { get; }

		ZString OriginalAttributeValue { get; }
	}
}

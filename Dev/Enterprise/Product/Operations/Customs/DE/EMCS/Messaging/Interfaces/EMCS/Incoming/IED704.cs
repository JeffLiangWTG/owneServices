using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED704 : IEmcsDataProvider
	{
		ZString CorrelationIdentifier { get; }

		ZString LocalReferenceNumber { get; }

		ZString AdministrativeReferenceCode { get; }

		IReadOnlyCollection<IEMCSError> Errors { get; }
	}

	public interface IEMCSError
	{
		ZString ErrorNumber { get; }

		ZString LineNumber { get; }

		ZString ColumnNumber { get; }

		ZString ErrorType { get; }

		ZString ErrorReason { get; }
	}
}

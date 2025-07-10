using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE704 : IEMCSInboundProvider
	{
		ZString LocalReferenceNumber { get; }
		ZString AdministrativeReferenceCode { get; }
		IReadOnlyCollection<IIE704FunctionalError> Errors { get; }
	}
}

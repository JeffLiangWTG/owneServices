using System.Collections.Generic;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE819 : IEMCSInboundProvider
	{
		IEMCSEvent ExciseMovementEad { get; }
		IReadOnlyCollection<IIE819Reason> AlertOrRejectionReasons { get; }
	}
}

using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE871 : IEMCSInboundProvider
	{
		IEMCSEvent ExciseMovementEad { get; }

		ZString GlobalExplanation { get; }

		IReadOnlyCollection<IIE871BodyAnalysis> Lines { get; }
	}
}

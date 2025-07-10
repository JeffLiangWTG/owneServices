using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE818 : IEMCSInboundProvider
	{
		ZString GlobalConclusionOfReceipt { get; }
		IReadOnlyCollection<IIE818ReportOfReceipt> ReportOfReceipts { get; }
		IEMCSEvent ExciseMovementEad { get; }
	}
}

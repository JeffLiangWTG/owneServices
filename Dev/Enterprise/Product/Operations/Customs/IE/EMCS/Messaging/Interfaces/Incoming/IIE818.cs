using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging
{
	public interface IIE818 : IEMCSInboundProvider
	{
		ZString GlobalConclusionOfReceipt { get; }
		IReadOnlyCollection<IIE818ReportOfReceipt> ReportOfReceipts { get; }
		IEMCSEvent ExciseMovementEad { get; }
	}

	public interface IIE818ReportOfReceipt
	{
		ZString LineNumber { get; }
		ZString IndicatorOfShortageOrExcess { get; }
		ZDecimal ObservedQuantity { get; }
		ZDecimal RefusedQuantity { get; }
		IReadOnlyCollection<IIE818UnsatisfactoryReason> UnsatisfactoryReasons { get; }
	}

	public interface IIE818UnsatisfactoryReason
	{
		ZString ReasonCode { get; }
		ZString ComplementaryInformation { get; }
	}
}

using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED818 : IEmcsDataProvider
	{
		ZString GlobalConclusionOfReceipt { get; }
		IReadOnlyCollection<IED818ReportOfReceipt> ReportOfReceipts { get; }
		IEMCSEvent ExciseMovement { get; }
	}

	public interface IED818ReportOfReceipt : IInboundProvider
	{
		ZString LineNumber { get; }
		ZString IndicatorOfShortageOrExcess { get; }
		ZDecimal ObservedQuantity { get; }
		ZDecimal RefusedQuantity { get; }
		IReadOnlyCollection<IED818UnsatisfactoryReason> UnsatisfactoryReasons { get; }
	}

	public interface IED818UnsatisfactoryReason : IInboundProvider
	{
		ZString ReasonCode { get; }
		ZString ComplementaryInformation { get; }
	}
}

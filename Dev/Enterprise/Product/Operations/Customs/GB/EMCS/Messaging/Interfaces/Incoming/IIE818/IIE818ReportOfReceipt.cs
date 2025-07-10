using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE818ReportOfReceipt
	{
		ZString LineNumber { get; }
		ZString IndicatorOfShortageOrExcess { get; }
		ZDecimal ObservedQuantity { get; }
		ZDecimal RefusedQuantity { get; }
		IReadOnlyCollection<IIE818UnsatisfactoryReason> UnsatisfactoryReasons { get; }
	}
}

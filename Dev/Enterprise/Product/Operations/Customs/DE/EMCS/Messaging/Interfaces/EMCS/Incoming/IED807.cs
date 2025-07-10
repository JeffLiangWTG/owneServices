using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IED807 : IEmcsDataProvider
	{
		IED807Event ExciseMovementEad { get; }
		IReadOnlyCollection<ZString> ControlReportNumbers { get; }
		IReadOnlyCollection<ZString> EventReportNumbers { get; }
	}

	public interface IED807Event : IEMCSEvent
	{
		ZString Reason { get; }
		ZString ComplementaryInformation { get; }
	}
}

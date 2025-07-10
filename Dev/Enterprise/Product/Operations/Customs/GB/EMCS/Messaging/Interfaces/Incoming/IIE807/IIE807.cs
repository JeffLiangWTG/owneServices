using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE807 : IEMCSInboundProvider
	{
		IIE807Event ExciseMovementEad { get; }
		IReadOnlyCollection<ZString> ControlReportNumbers { get; }
		IReadOnlyCollection<ZString> EventReportNumbers { get; }
	}
}

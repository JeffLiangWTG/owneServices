using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISeaOutturnReportHeaderInformation : IOutturnReportHeaderInformation
	{
		ZString VoyageNumber { get; }
		ZString VesselID { get; }
		IEnumerable<ISeaOutturnReportLineInformation> Lines { get; }
		IEnumerable<ISeaOutturnReportLineInformation> MessageLines { get; }
		IEDIMessageCollectionProvider MessagesProvider { get; }
	}
}

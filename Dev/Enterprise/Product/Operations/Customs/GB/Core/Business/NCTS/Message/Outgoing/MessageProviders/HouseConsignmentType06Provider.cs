using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class HouseConsignmentType06Provider : IHouseConsignmentType06
	{
		public HouseConsignmentType06Provider(NctsBill bill, int sequenceNumber)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			SequenceNumber = sequenceNumber;
		}
		readonly NctsBill bill;

		public int SequenceNumber { get; }

		public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = bill.DepartureTransportInfos.Select(ccd => new NCTSBillDepartureTransportMeansProvider(ccd, bill.InlandTransportModeAtDeparture)).ToArray<IDepartureTransportMeans>());
		IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;
	}
}

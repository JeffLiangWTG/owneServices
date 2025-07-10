using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManArrivalPortCARLSTAmendmentGenerator : CMRAmendmentGenerator
	{
		public CusSeaManArrivalPortCARLSTAmendmentGenerator(CusSeaManArrivalPort arrival)
			: base(arrival)
		{
			Arrival = arrival;
		}

		protected readonly CusSeaManArrivalPort Arrival;

		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo) => new CARLSTMessageBuilder(bizo as CusSeaManArrivalPort);

		protected internal override EDIMessageCollection GetMesssageCollection(BusinessObject bizo) => (bizo as CusSeaManArrivalPort).Messages;

		protected override ZPropertyInfo[] UniqueIdentifierInfos => new ZPropertyInfo[] { Arrival.Header.BT_VesselNameInfo, Arrival.Header.BT_VoyageNumInfo, Arrival.BA_RL_NKArrivalPortInfo };
	}
}

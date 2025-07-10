using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManArrivalPortSEAAARAmendmentGenerator : CMRAmendmentGenerator
	{
		public CusSeaManArrivalPortSEAAARAmendmentGenerator(CusSeaManArrivalPort arrival)
			: base(arrival)
		{
			this.arrival = arrival;
		}

		readonly CusSeaManArrivalPort arrival;

		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo) => new SEAAARMessageBuilder(bizo as CusSeaManArrivalPort);

		protected internal override EDIMessageCollection GetMesssageCollection(BusinessObject bizo) => (bizo as CusSeaManArrivalPort).Messages;

		protected override ZPropertyInfo[] UniqueIdentifierInfos => new ZPropertyInfo[] { arrival.Header.BT_VesselNameInfo, arrival.Header.BT_VoyageNumInfo, arrival.BA_RL_NKArrivalPortInfo, arrival.BA_OA_CTOAddressInfo };
	}
}

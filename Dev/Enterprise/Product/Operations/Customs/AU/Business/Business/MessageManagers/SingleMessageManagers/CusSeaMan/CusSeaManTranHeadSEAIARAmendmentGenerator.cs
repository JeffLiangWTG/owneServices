using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManTranHeadSEAIARAmendmentGenerator : CMRAmendmentGenerator
	{
		public CusSeaManTranHeadSEAIARAmendmentGenerator(CusSeaManTranHead transportHeader)
			: base(transportHeader)
		{
			header = transportHeader;
		}

		readonly CusSeaManTranHead header;

		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo) => new SEAIARMessageBuilder(bizo as CusSeaManTranHead);

		protected internal override EDIMessageCollection GetMesssageCollection(BusinessObject bizo) => (bizo as CusSeaManTranHead).Messages;

		protected override ZPropertyInfo[] UniqueIdentifierInfos => new ZPropertyInfo[] { header.BT_VesselNameInfo, header.BT_VoyageNumInfo };
	}
}

using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLHeaderSEACRAmendmentGenerator : CMRAmendmentGenerator
	{
		public CusSeaManOBLHeaderSEACRAmendmentGenerator(CusSeaManOBLHeader transportHeader)
			: base(transportHeader)
		{
			header = transportHeader;
		}

		readonly CusSeaManOBLHeader header;

		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo) => new SEACRMessageBuilder(bizo as CusSeaManOBLHeader);

		protected internal override EDIMessageCollection GetMesssageCollection(BusinessObject bizo) => (bizo as CusSeaManOBLHeader).Messages;

		protected override ZPropertyInfo[] UniqueIdentifierInfos => new ZPropertyInfo[] { header.TransportHeader.BT_VesselNameInfo, header.TransportHeader.BT_VoyageNumInfo, header.BO_OceanBillInfo };
	}
}

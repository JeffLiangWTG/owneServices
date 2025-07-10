using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAHouseSEACRAmendmentGenerator : CMRAmendmentGenerator
	{
		public CusSCAHouseSEACRAmendmentGenerator(CusSCAHouse house)
			: base(house)
		{
			this.house = house;
		}

		readonly CusSCAHouse house;

		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo) => new SEACRMessageBuilder(bizo as CusSCAHouse);

		protected internal override EDIMessageCollection GetMesssageCollection(BusinessObject bizo) => (bizo as CusSCAHouse).Messages;

		protected override ZPropertyInfo[] UniqueIdentifierInfos => new ZPropertyInfo[] { house.OceanBill.CB_LloydsIMOInfo, house.OceanBill.CB_VoyageInfo, house.OceanBill.CB_OceanBillInfo, house.CA_HouseBillInfo };
	}
}

using System;
using CargoWise.EntityFramework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SeaCargoXmlDataAdapterAddressSuburbTest : AddressSuburbTest<CusSCAOceanBill, Xsd.Consol>
	{
		protected override Type AddressBusinessObjectType => typeof(CusSCAHouse);

		protected override BaseCargoXmlDataAdapter<CusSCAOceanBill, Xsd.Consol> GetNewCargoDataAdapter() => new SeaCargoXmlDataAdapter<CusSCAOceanBill, Xsd.Consol>();

		protected override ZPropertyInfo Address2Property => HouseBill.CA_ConsigneeAddress2Info;

		protected override ZPropertyInfo SuburbProperty => HouseBill.CA_ConsigneeSuburbInfo;

		CusSCAHouse HouseBill => AddressBusinessObject as CusSCAHouse;
	}
}

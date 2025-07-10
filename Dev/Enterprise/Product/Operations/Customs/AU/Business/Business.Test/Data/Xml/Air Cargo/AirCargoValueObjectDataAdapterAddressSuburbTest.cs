using System;
using CargoWise.EntityFramework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirCargoValueObjectDataAdapterAddressSuburbTest : AddressSuburbTest<CusMAWB, Xsd.Consol>
	{
		protected override Type AddressBusinessObjectType => typeof(CusHAWB);

		protected override BaseCargoXmlDataAdapter<CusMAWB, Xsd.Consol> GetNewCargoDataAdapter() => new AirCargoValueObjectDataAdapter();

		protected override ZPropertyInfo Address2Property => HouseBill.CS_ConsigneeStreet2Info;

		protected override ZPropertyInfo SuburbProperty => HouseBill.CS_ConsignorCityInfo;

		CusHAWB HouseBill => AddressBusinessObject as CusHAWB;
	}
}

using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaCargoXmlDataAdapter<CusSCAOceanBill, Xsd.Consol>))]
	sealed class SeaCargoXmlDataAdapterTest : SeaCargoXmlDataAdapterAbstractTest<CusSCAOceanBill, Xsd.Consol>
	{
	}
}

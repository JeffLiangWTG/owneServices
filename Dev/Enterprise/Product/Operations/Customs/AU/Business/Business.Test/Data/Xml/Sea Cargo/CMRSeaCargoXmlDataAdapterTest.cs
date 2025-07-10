using Enterprise.DataTransfer.DataAdapters;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRSeaCargoXmlDataAdapter))]
	sealed class CMRSeaCargoXmlDataAdapterTest : SeaCargoXmlDataAdapterAbstractTest<CusSCAOceanBill, Xsd.Consol>
	{
		protected override ValueObjectDataAdapter<CusSCAOceanBill, Xsd.Consol> GetNewBizObjXmlDataAdapter() => new CMRSeaCargoXmlDataAdapter();
	}
}

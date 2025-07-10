using CargoWise.EntityFramework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSeaCargoFlatFileDataImporter : SeaCargoFlatFileDataImporter
	{
		public CMRSeaCargoFlatFileDataImporter()
		{
		}

		public CMRSeaCargoFlatFileDataImporter(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
		}

		protected override SeaCargoXmlDataAdapter<CusSCAOceanBill, Xsd.Consol> GetNewSeaCargoXmlDataAdapter()
		{
			return new CMRSeaCargoXmlDataAdapter();
		}
	}
}

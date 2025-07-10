using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SeaCargoFlatFileDataImporter : FlatFileDataImporter
	{
		public SeaCargoFlatFileDataImporter()
		{
		}

		public SeaCargoFlatFileDataImporter(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new SeaCargoFlatFileConverter(notificationSubscriber, FactoryProvider.Current);
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.Consol();
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			bool result = false;

			Xsd.Consol xsdConsol = xSD as Xsd.Consol;
			if (xsdConsol != null)
			{
				Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
				interchange.InterchangeInfo = new Xsd.InterchangeInfo();
				interchange.InterchangeInfo.EDIOrganisation = new Xsd.Organisation();
				interchange.InterchangeInfo.EDIOrganisation.OwnerCode = "CTI";

				SeaCargoXmlDataAdapter<CusSCAOceanBill, Xsd.Consol> adapter = GetNewSeaCargoXmlDataAdapter();
				ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider, interchange, notifications);
				result = adapter.CreateOrUpdateFromValueObject(xsdConsol, importContext) != null;
			}

			return result;
		}

		protected virtual SeaCargoXmlDataAdapter<CusSCAOceanBill, Xsd.Consol> GetNewSeaCargoXmlDataAdapter()
		{
			return new CMRSeaCargoXmlDataAdapter();
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new SeaCargoFlatFileFormat(); }
		}

		class SeaCargoFlatFileFormat : AutoDetectFlatFileFormat
		{
			public override FileExtensionType FileExtensionForImport
			{
				get { return FileExtensionType.All | FileExtensionType.Csv; }
			}

			public override FileExtensionType FileExtensionForExport
			{
				get { return FileExtensionType.All | FileExtensionType.Csv; }
			}
		}
	}
}

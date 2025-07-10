
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	public partial class GLHeaderAndChargeCodeFlatFileDataImporter : FlatFileDataImporter
	{
		public GLHeaderAndChargeCodeFlatFileDataImporter()
		{
		}

		protected GLHeaderAndChargeCodeFlatFileDataImporter(BusinessObjectFactory factory)
			: base(new SingleBusinessObjectFactoryProvider(factory))
		{
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new GLHeaderAndChargeCodesCSVFlatFileConverter(notificationSubscriber, FactoryProvider.Current);
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.GLHeadersAndChargeCodes();
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			Xsd.GLHeadersAndChargeCodes gLHeadersAndChargeCodes = (Xsd.GLHeadersAndChargeCodes)xSD;

			GLHeadersAndChargeCodesDataAdapter adapter = new GLHeadersAndChargeCodesDataAdapter();

			if (gLHeadersAndChargeCodes != null)
			{
				ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider, notifications);
				adapter.ImportFromValueObject(new BusinessObjectThatDoesntSave(FactoryProvider.Current), gLHeadersAndChargeCodes, importContext);
				return true;
			}
			else
			{
				return false;
			}
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new CsvFlatFileFormat(false); }
		}

		protected override bool ShouldSuspendValidation
		{
			get
			{
				return false;
			}
		}
	}
}


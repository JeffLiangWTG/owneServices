using CargoWise.ComponentModel;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.YAS.YASInvoiceImporter
{
	public class YASFlatFileDataImporter : FlatFileDataImporter
	{
		public YASFlatFileDataImporter(BaseJobDeclaration declaration) : base(declaration)
		{
			this.Declaration = declaration;
		}

		readonly BaseJobDeclaration Declaration;

		#region Overrides

		protected override IFlatFileConverter CreateConverter(INotifications notify)
		{
			return new YASInvoiceConverter(notify, FactoryProvider.Current);
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.InvoiceHeaderCollection();
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			Xsd.InvoiceHeaderCollection invoiceHeaders = (Xsd.InvoiceHeaderCollection)xSD;

			IValueObjectDataAdapter dataAdapter = new AUInvoiceValueObjectDataAdapter(Declaration);
			ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider, notifications);
			foreach (Xsd.InvoiceHeader header in invoiceHeaders)
			{
				dataAdapter.CreateOrUpdateFromValueObject(header, importContext);
			}

			return true;
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new YASInvoiceFlatFileFormat(); }
		}

		#endregion
	}
}

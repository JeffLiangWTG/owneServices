using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.Data
{
	public class EStatementImporter : TxnHeaderFlatFileDataImporter
	{
		public EStatementImporter()
		{
		}

		protected EStatementImporter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new CsvFlatFileFormat(false); }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new EStatementConverter(notificationSubscriber, FactoryProvider.Current);
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.TxnHeaderCollection();
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			NotificationBuffer buffer = new NotificationBuffer(notifications);
			base.ExtractToDataAdapter(xSD, buffer);
			ZBool result = !buffer.HasErrors;
			return result;
		}

#pragma warning disable CS0109 // Member does not hide an inherited member; new keyword is not required
#pragma warning restore CS0109 // Member does not hide an inherited member; new keyword is not required

		internal IFlatFileFormat InternalIFlatFileFormatTest => new CsvFlatFileFormat(false);
		internal IFlatFileConverter InternalCreateConverterTest(INotifications notificationSubscriber) => CreateConverter(notificationSubscriber);
		internal IValueObject InternalCreateXsdTest() => CreateXsd();
	}
}

using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.ReportingBook.AlternateGLAccountWithAttribute
{
	public partial class AlternateGLAccountWithAttributeFlatFileDataImporter : FlatFileDataImporter
	{
		public AlternateGLAccountWithAttributeFlatFileDataImporter()
		{
		}

		protected AlternateGLAccountWithAttributeFlatFileDataImporter(BusinessObjectFactory factory)
			: base(new SingleBusinessObjectFactoryProvider(factory))
		{
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new AlternateGLAccountWithAttributeCSVFlatFileConverter(notificationSubscriber, FactoryProvider.Current);
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.AlternateGLAccounts();
		}

		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			var result = base.ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);
			result &= !(notifications is NotificationBuffer notificationBuffer && notificationBuffer.HasErrors);
			if (result)
			{
				notifications.Notify(new InfoNotification(Res.GetString("19CBD44A-EA60-489C-8456-FC46CFED1A27", $"Alternate GL Accounts Processed Successfully")));
			}
			return result;
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			var alternateGLAccounts = (Xsd.AlternateGLAccounts)xSD;

			var adapter = new AlternateGLAccountWithAttributeDataAdapter();

			if (alternateGLAccounts != null)
			{
				var importContext = new ValueObjectImportContext(FactoryProvider, notifications);
				adapter.ImportFromValueObject(new BusinessObjectThatDoesntSave(FactoryProvider.Current), alternateGLAccounts, importContext);
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


using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile.Testing;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	sealed class NZCustomsAPInvoiceImporterTest : TxnHeaderFlatFileDataImporterTest
	{
		[ExpectNoExceptions]
		public void TestImportDataToFactoryCore()
		{
			var dataImporter = new NZCustomsAPInvoiceImporter();
			var additionalTransactionParticipants = System.Array.Empty<ITransactionParticipant>();
			var notifications = new notifications();
			var textReader = new StringReader("rubbish");
			dataImporter.ImportDataToFactory(textReader, "", notifications, SourceInfo.EmptySourceInfo, out additionalTransactionParticipants);
		}

		public class notifications : INotifications
		{
			void INotifications.Add(INotification notification)
			{
			}
		}

		public void TestFlatFileFormatProperty()
		{
			TestImporter importer = new TestImporter();
			Assert("FlatFileFormat type", importer.FlatFileFormat is NZCustomsAPInvoiceFileFormat);
		}

		public void TestConverterCreation()
		{
			TestImporter importer = new TestImporter();
			IFlatFileConverter converter = importer.CreateConverter(new NotificationBuffer());
			Assert("Converter type", converter is NZCustomsAPInvoiceConverter);
		}

		public class TestImporter : NZCustomsAPInvoiceImporter
		{
			public new IFlatFileFormat FlatFileFormat
			{
				get { return base.FlatFileFormat; }
			}

			public new IFlatFileConverter CreateConverter(INotifications notifications)
			{
				return base.CreateConverter(notifications);
			}
		}
	}
}

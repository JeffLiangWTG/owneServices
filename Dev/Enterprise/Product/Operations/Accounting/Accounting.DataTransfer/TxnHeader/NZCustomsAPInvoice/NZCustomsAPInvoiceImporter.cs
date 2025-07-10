using CargoWise.ComponentModel;
using CargoWise.Integration;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class NZCustomsAPInvoiceImporter : TxnHeaderFlatFileDataImporter
	{
		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new NZCustomsAPInvoiceFileFormat(); }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			if (fConverter == null)
			{
				fConverter = new NZCustomsAPInvoiceConverter(notifications, FactoryProvider.Current);
			}

			return fConverter;
		}

		protected override bool ImportDataToFactoryCore(System.IO.TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			bool result = base.ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);

			if (fConverter != null && ImportedInvoice != null)
			{
				ImportedInvoice.ExpectedInvoiceTotal = fConverter.ExpectedInvoiceTotal;
				ImportedInvoice.ValidateExpectedInvoiceTotal = true;
			}

			return result;
		}

		NZCustomsAPInvoiceConverter fConverter;
	}
}

using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class NZCustomsAPInvoiceConverterTest : TestCaseWithFactory
	{
		public void TestValidFileHeader()
		{
			Xsd.TxnHeaderCollection valueObject = new Xsd.TxnHeaderCollection();
			NZCustomsAPInvoiceFileFormat fileFormat = new NZCustomsAPInvoiceFileFormat();
			NotificationBuffer notifications = new NotificationBuffer();

			using (TextReader reader = new StringReader(ValidCsvHeader))
			{
				NZCustomsAPInvoiceConverter converter = new NZCustomsAPInvoiceConverter(notifications, Factory);
				converter.ImportFlatFile(valueObject, fileFormat, reader);
			}

			Assert("No errors expected", !notifications.HasErrors);
		}

		public void TestInvalidFileHeader()
		{
			Xsd.TxnHeaderCollection valueObject = new Xsd.TxnHeaderCollection();
			NZCustomsAPInvoiceFileFormat fileFormat = new NZCustomsAPInvoiceFileFormat();
			NotificationBuffer notifications = new NotificationBuffer();

			using (TextReader reader = new StringReader(InvalidCsvHeader))
			{
				NZCustomsAPInvoiceConverter converter = new NZCustomsAPInvoiceConverter(notifications, Factory);
				converter.ImportFlatFile(valueObject, fileFormat, reader);
			}

			Assert("Errors expected", notifications.HasErrors);
			Assert("Invalid file header", notifications.AsString.Contains("Column names do not match the list of column names expected"));
			Assert("Invalid file header", notifications.AsString.ToLower().Contains(ValidCsvHeader.ToLower()));
		}

		public void TestIncorrectTotal()
		{
			Xsd.TxnHeaderCollection valueObject = new Xsd.TxnHeaderCollection();
			NZCustomsAPInvoiceFileFormat fileFormat = new NZCustomsAPInvoiceFileFormat();
			NotificationBuffer notifications = new NotificationBuffer();

			using (TextReader reader = new StringReader(ValidCsvHeader + "\nSkipped\nSkipped\n" + RawDataIncorrectTotal))
			{
				NZCustomsAPInvoiceConverter converter = new NZCustomsAPInvoiceConverter(notifications, Factory);
				converter.ImportFlatFile(valueObject, fileFormat, reader);
			}

			Assert("Errors expected", notifications.HasErrors);
			Assert("Incorrect total", notifications.AsString.Contains("Error: Job Number 'S00004635'. Sum of amounts does not equal to total."));
		}

		const string ValidCsvHeader = @"Business Unit, Customer, Broker Code, Broker or Client Name, Accounting Date, CusMod Entry Num, Entry Type, Entry Reason, Job Number, Fees & Levies, Duty, GST On Fees & Levies, GST On Imports, Total Incl GST, Stmt Num, Stmt Date, Bal Fwd Due Dt";
		const string InvalidCsvHeader = @"Business Unit, Customer, Broker Code, Broker or Client Name, Accounting Date, CusMod Entry Num, Entry Type, Entry Reason, Job Number, Net Charges Excl GST, GST, Total Incl GST, Stmt Num, Stmt Date, Bal Fwd Due Dt";
		const string RawDataIncorrectTotal = @"GCRB,40185183G,40010438H,ALGATAC INDUSTRIES,18-Apr-11,8395767401,INV,EE,S00004635,100.00,100.00,222.00,1159,30-Apr-11,20/05/2011";
	}
}

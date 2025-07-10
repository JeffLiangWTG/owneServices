using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
	sealed class GLJournalFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			Xsd.GLJournal valueObject = new Xsd.GLJournal();
			GLJournalFlatFileConverter converter = new GLJournalFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\ValidGJLJournal.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("", valueObject.GLDetail.Branch.ToString());
			AssertEquals("", valueObject.GLDetail.Department.ToString());
			AssertEquals("GJL Test", valueObject.GLDetail.Description.ToString());
			AssertEquals("200501", valueObject.GLDetail.InPeriod.ToString());
			AssertEquals("0", valueObject.GLDetail.OutPeriod.ToString());

			AssertEquals(2, valueObject.JournalLines.Count);

			AssertEquals("2010.00.00", valueObject.JournalLines[0].Account.ToString());
			AssertEquals(new ZDecimal(10), valueObject.JournalLines[0].LocalAmount.Value);
			AssertEquals("BNE", valueObject.JournalLines[0].Branch.ToString());
			AssertEquals("BRN", valueObject.JournalLines[0].Department.ToString());
			AssertEquals("GJL Test Line 1", valueObject.JournalLines[0].Description.ToString());
			AssertEquals("DR", valueObject.JournalLines[0].DRCR.ToString());

			AssertEquals("2020.00.00", valueObject.JournalLines[1].Account.ToString());
			AssertEquals(new ZDecimal(10), valueObject.JournalLines[1].LocalAmount.Value);
			AssertEquals("SYD", valueObject.JournalLines[1].Branch.ToString());
			AssertEquals("CIA", valueObject.JournalLines[1].Department.ToString());
			AssertEquals("GJL Test Line 2", valueObject.JournalLines[1].Description.ToString());
			AssertEquals("CR", valueObject.JournalLines[1].DRCR.ToString());
		}

		public void TestImport_GJL()
		{
			Xsd.GLJournal valueObject = new Xsd.GLJournal();
			GLJournalFlatFileConverter converter = new GLJournalFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\ValidGJLJournal.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("GJL", valueObject.GLDetail.JournalType.ToString());
		}

		public void TestImport_AJL()
		{
			Xsd.GLJournal valueObject = new Xsd.GLJournal();
			GLJournalFlatFileConverter converter = new GLJournalFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\ValidAJLJournal.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("AJL", valueObject.GLDetail.JournalType.ToString());
			AssertEquals("200501", valueObject.GLDetail.InPeriod.ToString());
			AssertEquals("200503", valueObject.GLDetail.OutPeriod.ToString());
		}

		public void TestImport_RJL()
		{
			Xsd.GLJournal valueObject = new Xsd.GLJournal();
			GLJournalFlatFileConverter converter = new GLJournalFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\ValidRJLJournal.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("RJL", valueObject.GLDetail.JournalType.ToString());
			AssertEquals("200501", valueObject.GLDetail.InPeriod.ToString());
			AssertEquals("200503", valueObject.GLDetail.OutPeriod.ToString());
		}

		public void TestImport_NJL()
		{
			Xsd.GLJournal valueObject = new Xsd.GLJournal();
			GLJournalFlatFileConverter converter = new GLJournalFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\ValidNJLJournal.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("", valueObject.GLDetail.Branch.ToString());
			AssertEquals("", valueObject.GLDetail.Department.ToString());
			AssertEquals("NJL Test", valueObject.GLDetail.Description.ToString());
			AssertEquals("200501", valueObject.GLDetail.InPeriod.ToString());
			AssertEquals("0", valueObject.GLDetail.OutPeriod.ToString());

			AssertEquals(2, valueObject.JournalLines.Count);

			AssertEquals("2010.00.00", valueObject.JournalLines[0].Account.ToString());
			AssertEquals(new ZDecimal(10), valueObject.JournalLines[0].LocalAmount.Value);
			AssertEquals("BNE", valueObject.JournalLines[0].Branch.ToString());
			AssertEquals("BRN", valueObject.JournalLines[0].Department.ToString());
			AssertEquals("NJL Test Line 1", valueObject.JournalLines[0].Description.ToString());
			AssertEquals("DR", valueObject.JournalLines[0].DRCR.ToString());

			AssertEquals("2020.00.00", valueObject.JournalLines[1].Account.ToString());
			AssertEquals(new ZDecimal(10), valueObject.JournalLines[1].LocalAmount.Value);
			AssertEquals("SYD", valueObject.JournalLines[1].Branch.ToString());
			AssertEquals("CIA", valueObject.JournalLines[1].Department.ToString());
			AssertEquals("NJL Test Line 2", valueObject.JournalLines[1].Description.ToString());
			AssertEquals("CR", valueObject.JournalLines[1].DRCR.ToString());
		}

		public void TestImport_MoreThanOneJournals()
		{
			Xsd.GLJournal valueObject = new Xsd.GLJournal();
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			GLJournalFlatFileConverter converter = new GLJournalFlatFileConverter(notificationBuffer, Factory);

			AssertEquals(false, notificationBuffer.HasErrors);

			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\ValidGJL2Journals.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(true, notificationBuffer.HasErrors);
			AssertEquals("Error: " + GLJournalFlatFileConverter.MoreThanOneJournalErrorMsg + "\r\n", notificationBuffer.AsString);
		}

		public void TestImport_InvalidCSV()
		{
			Xsd.GLJournal valueObject = new Xsd.GLJournal();
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			GLJournalFlatFileConverter converter = new GLJournalFlatFileConverter(notificationBuffer, Factory);

			AssertEquals(false, notificationBuffer.HasErrors);

			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\InvalidGJLJournal.csv"))
			{
				converter.ImportFlatFile(valueObject, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(true, notificationBuffer.HasErrors);
			AssertEquals("Error: " + GLJournalFlatFileConverter.InvalidCsvFileErrorMessage + "\r\n", notificationBuffer.AsString);
		}
	}
}

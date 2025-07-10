using System;
using System.Collections;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.Matching.Testing
{
	[TestedType(typeof(MatchedDataImporter))]
	public class MatchedDataImporterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImportFileNotExist()
		{
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFileDir = resourceRetriever.SaveAllResourcesToFiles() + "\\";
				string fileName = "FileNotExist.txt";
				Importer.Import(testFileDir + fileName);
				AssertNull("No lines imported", Lines);
				AssertEquals(string.Format("The File '{0}{1}' does not exist", testFileDir, fileName), Importer.ErrorMessage);
			}
		}

		public void TestImportEmptyFile()
		{
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string fileName = "Enterprise.Client.JAS.Testing.Business.Matching.MatchedDataImporter.TestFiles.EmptyFile.out";
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				AssertEquals(false, Completed);
				Importer.Import(testFilePath);
				AssertEquals(true, Completed);
				AssertNull("No lines imported", Lines);
				AssertEquals("", Importer.ErrorMessage);
				AssertEquals("-----------------\nImport Successful", ImportLog);
				AssertEquals(0, ProcessedCount);
				AssertEquals(0, FailureCount);
				AssertEquals(100, PercentageComplete);
			}
		}

		public void TestImportFileWithSomeInvalidLines()
		{
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string fileName = "Enterprise.Client.JAS.Testing.Business.Matching.MatchedDataImporter.TestFiles.FileWithSomeInvalidLines.out";
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				AssertEquals(false, Completed);
				Importer.Import(testFilePath);
				AssertEquals(true, Completed);
				AssertEquals(2, Lines.Length);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine1), Lines[0]);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine3), Lines[1]);
				AssertEquals("", Importer.ErrorMessage);
				string expectedFormat = "{0}{1}{2}{0}{3}{2}-----------------\nImport Successful";
				AssertEquals(string.Format(expectedFormat, "Invalid line, row excluded. Content: ", ErrorLine1, System.Environment.NewLine, ErrorLine2), ImportLog);
				AssertEquals(4, ProcessedCount);
				AssertEquals(2, FailureCount);
				AssertEquals(100, PercentageComplete);
			}
		}

		public void TestImportFileAUCOR_out()
		{
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string fileName = "Enterprise.Client.JAS.Testing.Business.Matching.MatchedDataImporter.TestFiles.AUCOR.out";
				string testFilePath = resourceRetriever.SaveResourceToFile(fileName);
				AssertEquals(false, Completed);
				Importer.Import(testFilePath);
				AssertEquals(true, Completed);
				AssertEquals(12, Lines.Length);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine1), Lines[0]);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine2), Lines[1]);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine3), Lines[2]);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine4), Lines[3]);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine5), Lines[4]);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine6), Lines[5]);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine7), Lines[6]);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine8), Lines[7]);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine9), Lines[8]);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine10), Lines[9]);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine11), Lines[10]);
				CompareMatchedDataLines(GenerateExpectedMatchedDataLine(ExpectedLine12), Lines[11]);
				AssertEquals("", Importer.ErrorMessage);
				AssertEquals("-----------------\nImport Successful", ImportLog);
				AssertEquals(12, ProcessedCount);
				AssertEquals(0, FailureCount);
				AssertEquals(100, PercentageComplete);
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Importer = new MatchedDataImporter();
			Importer.Processed += new ProcessedEventHandler(Importer_Processed);
			Importer.ProcessCompleted += new EventHandler(Importer_ProcessCompleted);
			Importer.MatchedDataImported += new MatchedDataImportedEventHandler(Importer_MatchedDataImported);
		}

		MatchedDataLine GenerateExpectedMatchedDataLine(string rawString)
		{
			return new MatchedDataLine(rawString);
		}

		protected void CompareMatchedDataLines(MatchedDataLine expected, MatchedDataLine generated)
		{
			AssertEquals(expected.Amount, generated.Amount);
			AssertEquals(expected.Category, generated.Category);
			AssertEquals(expected.CounterpartSubsidiary, generated.CounterpartSubsidiary);
			AssertEquals(expected.CreditNote, generated.CreditNote);
			AssertEquals(expected.CurrencyCode, generated.CurrencyCode);
			AssertEquals(expected.CurrencyOfExchangeValue, generated.CurrencyOfExchangeValue);
			AssertEquals(expected.Denomination, generated.Denomination);
			AssertEquals(expected.DestinationSubsidiary, generated.DestinationSubsidiary);
			AssertEquals(expected.ExchangeValue, generated.ExchangeValue);
			AssertEquals(expected.FullInvoiceNumber, generated.FullInvoiceNumber);
			AssertEquals(expected.HouseBill, generated.HouseBill);
			AssertEquals(expected.InvoiceDate, generated.InvoiceDate);
			AssertEquals(expected.Ledger, generated.Ledger);
			AssertEquals(expected.MasterBill, generated.MasterBill);
			AssertEquals(expected.MaturityDate, generated.MaturityDate);
			AssertEquals(expected.OtherRefNumber, generated.OtherRefNumber);
		}

		void Importer_Processed(object sender, ProcessedEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.LogEntry))
			{
				ImportLog = (ImportLog == null) ? e.LogEntry : string.Concat(ImportLog, System.Environment.NewLine, e.LogEntry);
			}

			PercentageComplete = e.PercentageComplete;
			ProcessedCount = e.ProcessedCount;
			FailureCount = e.FailureCount;
		}

		void Importer_ProcessCompleted(object sender, EventArgs e)
		{
			PercentageComplete = 100;
			Completed = true;
		}

		void Importer_MatchedDataImported(MatchedDataLine line)
		{
			if (LineList == null)
			{
				LineList = new ArrayList();
			}

			LineList.Add(line);
		}

		const string ErrorLine1 = "asdf 234234234";
		const string ErrorLine2 = "AUCORBRSAO        2050.00+25/03/2004USD      VIXSYD                                           M25/01/2004MSCUSV295916  EMVIX04-01/050DN0104030087  R           0.00+A";
		const string ExpectedLine1 = "AUCORBRSAO         172.40-13/11/2003USD      SHAGRULH521  081103                              A14/10/200302043354441   SSHA0310001   IA0104040001  P           0.00-AUD";
		const string ExpectedLine2 = "AUCORBRSAO         172.40+13/11/2003USD      SHAGRULH521  081103                              A14/10/200302043354441   SSHA0310001   IA0104010094  P           0.00+AUD";
		const string ExpectedLine3 = "AUCORBRSAO         581.07+03/04/2004USD      GRUSYDJL047  110304                              A04/03/200413166244124   EAGRU040302401DN0104030186  P           0.00+AUD";
		const string ExpectedLine4 = "AUCORBRSAO        2050.00+25/03/2004USD      VIXSYD                                           M25/01/2004MSCUSV295916  EMVIX04-01/050DN0104030087  R           0.00+AUD";
		const string ExpectedLine5 = "AUCORBRSAO         704.27+13/04/2004USD      RIGMEL                                           M13/02/2004RIGMEL0402001 EMRIG040202901DN0104030036  R           0.00+AUD";
		const string ExpectedLine6 = "AUCORBRSAO         211.41+13/04/2004USD      RIGMEL                                           M13/02/2004RIGMEL0402002 EMRIG040204301DN0104030037  P           0.00+AUD";
		const string ExpectedLine7 = "AUCORBRSAO          75.00-10/04/2004USD      SSZSYD                                           M10/02/2004SUDU6020028090EMSSZ0402014  CN0104030004  P           0.00-AUD";
		const string ExpectedLine8 = "AUCORCAYTO         281.25+30/04/2004USD                                                       M25/02/2004FRES3009048                 113875        R           0.00+AUD";
		const string ExpectedLine9 = "AUCORCHJAS         571.40+26/03/2004CHFMP9171                                                 A27/02/200412934221456   JZH08060424   504014        P           0.00+AUD";
		const string ExpectedLine10 = "AUCORCHJAS         365.50+09/04/2004CHFCX101                                                  A10/03/200416030019581   JZH08060467   504113        R           0.00+AUD";
		const string ExpectedLine11 = "AUCORCHJAS         250.00+07/02/2004CHF                                                       M08/01/2004   703968291                31344         P           0.00+AUD";
		const string ExpectedLine12 = "AUCORCHJAS          90.00+07/02/2004USD                                                       M08/01/2004   703968291                31345         P           0.00+AUD";
		ArrayList LineList;
		MatchedDataLine[] Lines
		{
			get
			{
				return (LineList == null) ? null : (MatchedDataLine[])LineList.ToArray(typeof(MatchedDataLine));
			}
		}

		string ImportLog;
		int PercentageComplete;
		int ProcessedCount;
		int FailureCount;
		bool Completed;
		MatchedDataImporter Importer;
		#endregion
	}
}

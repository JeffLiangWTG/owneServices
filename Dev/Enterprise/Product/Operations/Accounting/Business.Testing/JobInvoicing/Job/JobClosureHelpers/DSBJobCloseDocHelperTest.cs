using System.Data;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class DSBJobCloseDocHelperTest : TestCaseWithFactory
	{
		[TestDate(2020, 6, 20)]
		public void TestCreateDocumentForDsbClosureJob()
		{
			var journalCreator = new DSBJobCloseGJLHelper(Factory);
			var documentCreator = new DSBJobCloseDocHelper(Factory);

			TestObjectCreator.PrepareDsbJobCloseBatchEnvironment(
				out Job job1, out Job job2, out DsbJobCloseBatch batch1,
				out InvoiceLine line11, out InvoiceLine line12, out InvoiceLine line21, out InvoiceLine line22,
				out AccChargeCode chargeCode1, out AccChargeCode chargeCode2,
				out Charge charge1Cst, out Charge charge1Rev, out Charge charge2Cst, out Charge charge2Rev);

			var journal = journalCreator.CreateGLJournal(batch1.PK);

			var lines = journal.Lines.Cast<GLJournalLine>();
			AssertEquals("Pre-condition", 8, lines.Count());
			AssertEquals(0, journal.DocManagerInfo.AllEDocs.Count);

			documentCreator.CreateAndAttachBatchDocument(batch1, journal);

			Factory.Save();

			var reloadedJournal = Factory.CreateNewFactory().Load<GLJournal>(journal.PK);
			AssertEquals("Batch document attached to EDoc. The other one is auto-created GJL transaction document.", 2, reloadedJournal.DocManagerInfo.AllEDocs.Count);

			var files = reloadedJournal.DocManagerInfo.AllEDocs.OfType<StorageFile>();
			var documentGJL = files.Single(x => x.DocType.RT_DocType == Core.Constants.DocManagerCodes.GLJournal);
			var documentBatch = files.Single(x => x.DocType.RT_DocType == Core.Constants.RefDocTypes.MiscellaneousDocument);

			AssertEquals("Disbursement Job Close Batch Document - B001.pdf", documentBatch.SC_FileNameWithExtension);
			AssertArrayEqualsByElements("Should be valid PDF file", new byte[4] { 37, 80, 68, 70 }, new byte[4] { documentBatch.SC_ImageData[0], documentBatch.SC_ImageData[1], documentBatch.SC_ImageData[2], documentBatch.SC_ImageData[3] });
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}

				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;
	}
}

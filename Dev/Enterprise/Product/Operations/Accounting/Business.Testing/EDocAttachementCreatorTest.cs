using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	public class EDocAttachementCreatorTest : TestCaseWithFactory
	{
		public void TestAttachmentCreator()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			var journal = objectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, Env.Time.CurrentLocalDate, Env.Time.CurrentLocalDate);
			Factory.Save();

			var eDocsCount = journal.DocManagerInfo.AllEDocs.Count;

			var fileName = string.Format("GL {0} {1} {2}.pdf", journal.AH_TransactionType, journal.AH_TransactionNum, Env.Time.CurrentLocalDateTime);
			var utility = new EDocAttachementCreator<GLJournal>(journal, fileName, Core.Constants.DocManagerCodes.GLJournal, true);
			utility.PrintDocument(journal, "General Ledger Journal", DocumentEngine.AllowedDeliveryOptions.All);
			AssertEquals("There Should be one more edoc", eDocsCount + 1, journal.DocManagerInfo.AllEDocs.Count);

			Assert(!utility.EDocCreatedUniqueKey.IsEmpty);
			AssertEquals(utility.EDocCreatedUniqueKey, journal.DocManagerInfo.AllEDocs.GetMostRecentEDoc(Core.Constants.DocManagerCodes.GLJournal).UniqueKey);
		}

		public void TestErrorMessageShown_WhenPrintByDefaultSetToFalse()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			var journal = objectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, Env.Time.CurrentLocalDate, Env.Time.CurrentLocalDate);

			StmMenuTemplatePivot generalLedgerJournalTemplate = Factory.LoadTop1<StmMenuTemplatePivot>(new ZQuery(StmMenuTemplatePivotSchema.SI_DocumentTitle, "General Ledger Journal"));
			generalLedgerJournalTemplate.SI_PrintByDefault = false;

			Factory.Save();

			var eDocsCount = journal.DocManagerInfo.AllEDocs.Count;

			var fileName = string.Format("GL {0} {1} {2}.pdf", journal.AH_TransactionType, journal.AH_TransactionNum, Env.Time.CurrentLocalDateTime);
			var utility = new EDocAttachementCreator<GLJournal>(journal, fileName, Core.Constants.DocManagerCodes.GLJournal, true);
			AssertExceptionThrown(typeof(ZCannotSaveException), () => { utility.PrintDocument(journal, "General Ledger Journal", DocumentEngine.AllowedDeliveryOptions.All); });
			AssertEquals("No eDoc added", eDocsCount, journal.DocManagerInfo.AllEDocs.Count);
		}
	}
}

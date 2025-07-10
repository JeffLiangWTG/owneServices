using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Journal.Testing
{
	internal class JournalLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBranches()
		{
			GlbBranchCollection allBranchCollection = new GlbBranchCollection(Factory);
			allBranchCollection.Load();

			allBranchCollection[0].GB_IsActive = false;
			allBranchCollection.Load(new ZQuery(GlbBranchSchema.GB_IsActive, true));
			int activeBranchCount = allBranchCollection.Count;

			APJournal parent = Factory.New<APJournal>();
			JournalLookups lookups = new JournalLookups(parent);
			GlbBranchCollection collection = lookups.Branches;
			collection.Load();

			AssertEquals(activeBranchCount, collection.Count);
		}

		public void TestCashAdvanceJournalTypes()
		{
			var apJournal = Factory.New<APJournal>();
			var lookups = new JournalLookups(apJournal);
			Assert(!lookups.TransactionCategories.ContainsCode(TransactionCategory.Codes.CashAdvanceReceived));
			Assert(!lookups.TransactionCategories.ContainsCode(TransactionCategory.Codes.CashAdvancePaid));

			var arJournal = Factory.New<ARJournal>();
			lookups = new JournalLookups(arJournal);
			Assert(!lookups.TransactionCategories.ContainsCode(TransactionCategory.Codes.CashAdvanceReceived));
			Assert(!lookups.TransactionCategories.ContainsCode(TransactionCategory.Codes.CashAdvancePaid));

			apJournal.AH_TransactionCategory = TransactionCategory.Codes.CashAdvancePaid;
			lookups = new JournalLookups(apJournal);
			Assert(!lookups.TransactionCategories.ContainsCode(TransactionCategory.Codes.CashAdvanceReceived));
			Assert(lookups.TransactionCategories.ContainsCode(TransactionCategory.Codes.CashAdvancePaid));

			arJournal.AH_TransactionCategory = TransactionCategory.Codes.CashAdvanceReceived;
			lookups = new JournalLookups(arJournal);
			Assert(lookups.TransactionCategories.ContainsCode(TransactionCategory.Codes.CashAdvanceReceived));
			Assert(!lookups.TransactionCategories.ContainsCode(TransactionCategory.Codes.CashAdvancePaid));
		}
	}
}

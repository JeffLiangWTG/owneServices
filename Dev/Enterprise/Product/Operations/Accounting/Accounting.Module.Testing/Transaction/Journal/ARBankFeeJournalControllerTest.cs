using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARBankFeeJournalController))]
	class ARBankFeeJournalControllerTest : BankFeeJournalControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARBankFeeJournal;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestARJNL; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestARJNL = Factory.New<ARJournal>();
			TestARJNL.AH_OSExTaxAmount = 10M;
			TestARJNL.AH_LocalExTaxAmount = TestARJNL.AH_OSExTaxAmount;
			TestARJNL.AH_LocalOutstandingAmount = 0M;

			TransactionMatchLinkGroup matchLinks = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink matchLink = matchLinks.AddNew();
			matchLink.AP_AH = TestARJNL.PK;
			matchLink.AP_Amount = TestARJNL.AH_InvoiceAmount;

			APJournal journalForMatching = Factory.NewWithValidTestData<APJournal>();
			journalForMatching.AH_OSExTaxAmount = TestARJNL.AH_OSExTaxAmount;
			journalForMatching.AH_LocalExTaxAmount = journalForMatching.AH_OSExTaxAmount;
			journalForMatching.AH_LocalOutstandingAmount = 0M;
			matchLink = matchLinks.AddNew();
			matchLink.AP_AH = journalForMatching.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinks);
			matchLink.AP_Amount = journalForMatching.AH_InvoiceAmount;
			Factory.Save();
		}

		protected ARJournal TestARJNL;
	}
}

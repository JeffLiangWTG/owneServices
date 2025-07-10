using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARBalancingJournalController))]
	class ARBalancingJournalControllerTest : MiscellaneousTransactionControllerTest
	{
		protected override BusinessObject ParentTransactionHeaderRow => TestARJNL;

		protected override string GetExpectedCantReverseMesaage => "This transaction cannot be reversed because it has been matched with other transactions.";

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARBalancingJournal;
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestARJNL = Factory.New<ARJournal>();
			TestARJNL.AH_OSExTaxAmount = 10M;
			TestARJNL.AH_LocalExTaxAmount = 10M;
			TestARJNL.AH_LocalOutstandingAmount = 0M;

			var journalForMatching = Factory.NewWithValidTestData<APJournal>();
			journalForMatching.AH_OSExTaxAmount = 10M;
			journalForMatching.AH_LocalExTaxAmount = 10M;
			journalForMatching.AH_LocalOutstandingAmount = 0M;

			var matchLinks = new TransactionMatchLinkGroup(Factory);
			var matchLink = matchLinks.AddNew();
			matchLink.AP_AH = TestARJNL.PK;
			matchLink.AP_Amount = TestARJNL.AH_InvoiceAmount;
			matchLink = matchLinks.AddNew();
			matchLink.AP_AH = journalForMatching.PK;
			matchLink.AP_Amount = journalForMatching.AH_InvoiceAmount;

			TestObjectCreator.SetupMatchLinkMatchDate(matchLinks);
			Factory.Save();
		}

		protected ARJournal TestARJNL;
	}
}

using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APBalancingJournalController))]
	class APBalancingJournalControllerTest : MiscellaneousTransactionControllerTest
	{
		protected override BusinessObject ParentTransactionHeaderRow => TestAPJNL;

		protected override string GetExpectedCantReverseMesaage => "This transaction cannot be reversed because it has been matched with other transactions.";

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APBalancingJournal;
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestAPJNL = Factory.New<APJournal>();
			TestAPJNL.AH_OSExTaxAmount = 10M;
			TestAPJNL.AH_LocalExTaxAmount = 10M;
			TestAPJNL.AH_LocalOutstandingAmount = 0M;

			var journalForMatching = Factory.NewWithValidTestData<ARJournal>();
			journalForMatching.AH_OSExTaxAmount = 10M;
			journalForMatching.AH_LocalExTaxAmount = 10M;
			journalForMatching.AH_LocalOutstandingAmount = 0M;

			var matchLinks = new TransactionMatchLinkGroup(Factory);
			var matchLink = matchLinks.AddNew();
			matchLink.AP_AH = TestAPJNL.PK;
			matchLink.AP_Amount = TestAPJNL.AH_InvoiceAmount;
			matchLink = matchLinks.AddNew();
			matchLink.AP_AH = journalForMatching.PK;
			matchLink.AP_Amount = journalForMatching.AH_InvoiceAmount;

			TestObjectCreator.SetupMatchLinkMatchDate(matchLinks);
			Factory.Save();
		}

		protected APJournal TestAPJNL;
	}
}

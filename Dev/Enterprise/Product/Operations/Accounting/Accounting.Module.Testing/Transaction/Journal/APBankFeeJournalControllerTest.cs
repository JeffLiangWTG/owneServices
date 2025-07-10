using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APBankFeeJournalController))]
	class APBankFeeJournalControllerTest : BankFeeJournalControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APBankFeeJournal;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestAPJNL; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestAPJNL = Factory.New<APJournal>();
			TestAPJNL.AH_OSExTaxAmount = 10M;
			TestAPJNL.AH_LocalExTaxAmount = TestAPJNL.AH_OSExTaxAmount;
			TestAPJNL.AH_LocalOutstandingAmount = 0M;

			TransactionMatchLinkGroup matchLinks = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink matchLink = matchLinks.AddNew();
			matchLink.AP_AH = TestAPJNL.PK;
			matchLink.AP_Amount = TestAPJNL.AH_InvoiceAmount;

			ARJournal journalForMatching = Factory.NewWithValidTestData<ARJournal>();
			journalForMatching.AH_OSExTaxAmount = TestAPJNL.AH_OSExTaxAmount;
			journalForMatching.AH_LocalExTaxAmount = journalForMatching.AH_OSExTaxAmount;
			journalForMatching.AH_LocalOutstandingAmount = 0M;
			matchLink = matchLinks.AddNew();
			matchLink.AP_AH = journalForMatching.PK;
			matchLink.AP_Amount = journalForMatching.AH_InvoiceAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinks);
			Factory.Save();
		}

		protected APJournal TestAPJNL;
	}
}

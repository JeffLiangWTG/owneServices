using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.TransactionApproval.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(GLJournalApprovalRequestCollection))]
	public class GLJournalApprovalRequestCollectionTest : TransactionApprovalRequestCollectionTest<GLJournalApprovalRequestCollection, GLJournalApprovalRequest>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var journal = JournalCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			JournalCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			JournalCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			JournalCreator.Factory.Save();

			var request = (GLJournalApprovalRequest)base.GetNewElementToAddToTheCollection();
			request.Initialize(journal);

			return request;
		}

		protected override void SetUp()
		{
			base.SetUp();
			JournalCreator = new TestObjectCreator(new BusinessObjectFactory());
		}

		protected override void TearDown()
		{
			base.TearDown();
			JournalCreator = null;
		}

		TestObjectCreator JournalCreator;
	}
}

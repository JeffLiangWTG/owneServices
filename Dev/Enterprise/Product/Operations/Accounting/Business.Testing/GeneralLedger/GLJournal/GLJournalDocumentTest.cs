using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(GLJournalDocumentSupporter))]
	public class GLJournalDocumentTest : TransactionHeaderDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<GLJournal>();
		}
	}
}

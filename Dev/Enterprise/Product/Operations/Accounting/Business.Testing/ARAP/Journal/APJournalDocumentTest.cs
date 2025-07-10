using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Journal.Testing
{
	[TestedType(typeof(APJournal))]
	public class APJournalDocumentTest : JournalDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<APJournal>();
		}
	}
}

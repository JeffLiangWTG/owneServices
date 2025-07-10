using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Journal.Testing
{
	[TestedType(typeof(ARJournal))]
	public class ARJournalDocumentTest : JournalDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<ARJournal>();
		}
	}
}

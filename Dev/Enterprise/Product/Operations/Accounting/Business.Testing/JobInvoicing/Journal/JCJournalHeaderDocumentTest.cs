using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JCJournalHeader))]
	public class JCJournalHeaderDocumentTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<JCJournalHeader>();
		}
	}
}

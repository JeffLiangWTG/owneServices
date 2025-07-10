using Enterprise.Accounting.Netting;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocNettingMatchedInvoice))]
	sealed class DocNettingMatchedInvoiceTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocNettingMatchedInvoice[] { DocNettingMatchedInvoice.New(new NettingMatchedInvoice(Factory), Factory) };
		}
	}
}

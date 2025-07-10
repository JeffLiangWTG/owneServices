using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Client.GCG.DocWrappers.Testing
{
	class TestDocumentWrapper : DocumentWrapper
	{
		protected TestDocumentWrapper() : base()
		{
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public static TestDocumentWrapper New(InvoicingBase invoice)
		{
			return new TestDocumentWrapper();
		}
	}
}

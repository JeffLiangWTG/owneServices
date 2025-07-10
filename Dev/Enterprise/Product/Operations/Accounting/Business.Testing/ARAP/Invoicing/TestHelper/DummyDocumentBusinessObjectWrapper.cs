using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class DummyDocumentBusinessObjectWrapper : DocumentWrapper
	{
		// Simple DocumentBusinessObjectWrapper used for testing the .
		// We cannot use normal DocARInvoice because DocumentWrappers is built after Accounting
		protected DummyDocumentBusinessObjectWrapper()
			: base()
		{
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public static DummyDocumentBusinessObjectWrapper New(InvoicingBase invoice)
		{
			return new DummyDocumentBusinessObjectWrapper();
		}
	}
}

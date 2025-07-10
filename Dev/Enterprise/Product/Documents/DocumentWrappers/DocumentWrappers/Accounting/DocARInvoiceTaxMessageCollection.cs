using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocARInvoiceTaxMessageCollection : DocumentWrapperCollection
	{
		public DocARInvoiceTaxMessageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocARInvoiceTaxMessage this[int index]
		{
			get { return (DocARInvoiceTaxMessage)Elements[index]; }
		}
	}
}

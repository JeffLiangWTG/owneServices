using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Accounting.KR
{
	public class DocElectronicInvoiceLineCollection : DocumentWrapperCollection
	{
		public DocElectronicInvoiceLineCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new DocElectronicInvoiceLine this[int index]
		{
			get { return (DocElectronicInvoiceLine)base[index]; }
		}
	}
}

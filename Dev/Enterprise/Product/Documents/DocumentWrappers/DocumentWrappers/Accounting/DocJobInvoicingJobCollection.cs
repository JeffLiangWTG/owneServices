using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocJobInvoicingJobCollection : DocumentWrapperCollection
	{
		public DocJobInvoicingJobCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocJobInvoicingJob this[int index]
		{
			get { return (DocJobInvoicingJob)base[index]; }
		}
	}
}


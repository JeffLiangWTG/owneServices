using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocPaymentApprovalItemCollection : DocumentWrapperCollection
	{
		public DocPaymentApprovalItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocPaymentApprovalItem this[int index]
		{
			get { return (DocPaymentApprovalItem)base[index]; }
		}
	}
}

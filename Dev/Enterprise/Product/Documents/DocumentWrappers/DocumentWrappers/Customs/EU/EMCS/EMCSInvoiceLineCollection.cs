using CargoWise.EntityFramework;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.EMCS
{
	public class EMCSInvoiceLineCollection : DocBaseWrapperCollection<EMCSInvoiceLine>
	{
		public EMCSInvoiceLineCollection(EMCSInvoiceLineCompleteCollection collection, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (EMCSJobComInvoiceLine line in collection)
			{
				Add(EMCSInvoiceLine.New(line, factory));
			}
		}

		public EMCSInvoiceLineCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}

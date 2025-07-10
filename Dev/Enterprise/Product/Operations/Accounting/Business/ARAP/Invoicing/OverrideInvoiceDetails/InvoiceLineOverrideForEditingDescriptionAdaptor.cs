using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoiceLineOverrideForEditingDescriptionAdaptor : InvoiceLineOverrideAdaptor<InvoiceLineOverrideForEditingDescription>
	{
		public InvoiceLineOverrideForEditingDescriptionAdaptor(BusinessObjectFactory factory, params ZGuid[] linePks) : base(factory, linePks) { }

		protected override InvoiceLineOverrideForEditingDescription Wrap(BusinessObject bizo)
		{
			return new InvoiceLineOverrideForEditingDescription((DependentTransactionLine)bizo);
		}
	}
}

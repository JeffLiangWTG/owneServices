using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoiceLineOverrideForEditingSequenceAdaptor : InvoiceLineOverrideAdaptor<InvoiceLineOverrideForEditingSequence>
	{
		public InvoiceLineOverrideForEditingSequenceAdaptor(BusinessObjectFactory factory, params ZGuid[] linePks) : base(factory, linePks) { }

		protected override InvoiceLineOverrideForEditingSequence Wrap(BusinessObject bizo)
		{
			return new InvoiceLineOverrideForEditingSequence((DependentTransactionLine)bizo);
		}
	}
}

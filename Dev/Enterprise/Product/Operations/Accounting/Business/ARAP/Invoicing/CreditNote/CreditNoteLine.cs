using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class CreditNoteLine : InvoicingLineBase
	{
		public CreditNoteLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CreditNote CreditNote
		{
			get { return (CreditNote)MasterTransactionHeader; }
		}

		protected override MasterFiles.Business.AccTransactionLinesValidation GetNewValidationCore()
		{
			return new CreditNoteLineValidation(this);
		}
	}
}
using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class APInvoiceChargesCollection : NonPersistentBusinessObjectCollection<APInvoiceCharges>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return ZString.Empty; }
		}
	}
}

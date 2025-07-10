using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public class ConsolidationBatchExportRowCollection : NonPersistentBusinessObjectCollection<ConsolidationBatchExportRow>
	{
		public ConsolidationBatchExportRowCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("You cannot directly add to this collection.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("You cannot directly add to this collection.");
		}
	}
}
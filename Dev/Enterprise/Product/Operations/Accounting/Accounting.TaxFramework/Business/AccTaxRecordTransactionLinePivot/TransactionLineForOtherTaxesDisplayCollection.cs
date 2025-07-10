using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class TransactionLineForOtherTaxesDisplayCollection : NonPersistentBusinessObjectCollection<TransactionLineForOtherTaxesDisplay>
	{
		public TransactionLineForOtherTaxesDisplayCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			throw new NotSupportedException("Loading to this collection is not supported.");
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Creating new objects is not supported.");
		}
	}
}

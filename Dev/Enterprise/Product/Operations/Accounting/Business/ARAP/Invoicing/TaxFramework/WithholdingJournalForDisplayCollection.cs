using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public class WithholdingJournalForDisplayCollection : NonPersistentBusinessObjectCollection<WithholdingJournalForDisplay>
	{
		public WithholdingJournalForDisplayCollection(BusinessObjectFactory factory) : base(factory)
		{ }

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			throw new NotSupportedException("Loading to this collection is not supported.");
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Creating new objects is not supported.");
		}

		protected override ZString HumanReadableNameCore => Res.GetString("6D6117EB-1F9F-49BA-B269-E5A91D63F9D7", "Withholding Journals collection");
	}
}

using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.EPayment
{
	public class EPaymentQuoteForDisplayCollection : NonPersistentBusinessObjectCollection<EPaymentQuoteForDisplay>
	{
		public EPaymentQuoteForDisplayCollection(EPaymentQuoteCollection realCollection = null)
			: base(realCollection?.Factory ?? new BusinessObjectFactory())
		{
			this.realCollection = realCollection;
		}

		readonly EPaymentQuoteCollection realCollection;

		public override void Load()
		{
			RemoveAll();
			realCollection?.ForEach(x => {
				var quote = new EPaymentQuoteForDisplay(x as EPaymentQuote);
				quote.RunPreSaveValidation();
				Add(quote);
				}
			);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();
		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}

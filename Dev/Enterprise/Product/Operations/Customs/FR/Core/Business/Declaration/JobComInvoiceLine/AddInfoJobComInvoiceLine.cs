using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class AddInfoJobComInvoiceLine : EU.Business.Declaration.AddInfoJobComInvoiceLine
	{
		public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty)
		{
		}

		public new AddInfoJobComInvoiceLineLookups Lookups => (AddInfoJobComInvoiceLineLookups)base.Lookups;
		protected override EUAddInfoLookups GetNewLookups()
		{
			return new AddInfoJobComInvoiceLineLookups(this);
		}

		public new AddInfoJobComInvoiceLineValidation Validation => (AddInfoJobComInvoiceLineValidation)base.Validation;
		protected override EUAddInfoValidation GetNewValidation()
		{
			return new AddInfoJobComInvoiceLineValidation(this);
		}

		protected override IValueSetStrategy GetValueSetStrategy()
		{
			return new AddInfoJobComInvoiceLineValueSetStrategy(this);
		}
	}
}

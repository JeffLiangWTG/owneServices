using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DK.Business.Declaration
{
	public class AddInfoJobComInvoiceLine : EU.Business.Declaration.AddInfoJobComInvoiceLine
	{
		public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		public new AddInfoJobComInvoiceLineLookups Lookups => (AddInfoJobComInvoiceLineLookups)base.Lookups;

		public new AddInfoJobComInvoiceLineValidation Validation => (AddInfoJobComInvoiceLineValidation)base.Validation;

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceLineLookups(this);

		protected override EUAddInfoValidation GetNewValidation() => new AddInfoJobComInvoiceLineValidation(this);
	}
}

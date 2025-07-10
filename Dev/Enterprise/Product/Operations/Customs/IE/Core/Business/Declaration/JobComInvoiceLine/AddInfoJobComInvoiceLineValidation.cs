using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class AddInfoJobComInvoiceLineValidation : EU.Business.Declaration.AddInfoJobComInvoiceLineValidation
	{
		public AddInfoJobComInvoiceLineValidation(EU.Business.Declaration.AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		//this is to ensure that the add info properties are not accessed - because they may not have values - Look at ZG_CountryOfDestination on InvoiceLine as an example as to why
		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent.Parent;

		protected override void CheckZG_RegionOfDestination()
		{
			var parent = Parent;
			var targetInfo = parent.ZG_RegionOfDestinationInfo;
			if (parent.Declaration != null)
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
		}
	}
}

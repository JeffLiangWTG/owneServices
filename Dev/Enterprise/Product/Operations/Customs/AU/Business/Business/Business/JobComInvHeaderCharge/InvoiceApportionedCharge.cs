using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class InvoiceApportionedCharge : BaseApportionedCharge, Integration.Customs.AU.IInvoiceApportionedCharge
	{
		public InvoiceApportionedCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new JobComInvHeaderChargeLookups(this);
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceApportionedChargeValidation(this);
		}
	}
}

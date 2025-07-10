using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class InvoiceCharge : BaseInvoiceCharge, Integration.Customs.AU.IInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new JobComInvHeaderChargeLookups(this);
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceChargeValidation(this);
		}
	}
}

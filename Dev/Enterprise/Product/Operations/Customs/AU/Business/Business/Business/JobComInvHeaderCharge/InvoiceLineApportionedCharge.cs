using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class InvoiceLineApportionedCharge : Customs.Business.BaseInvoiceLineApportionedCharge, Integration.Customs.AU.IInvoiceLineApportionedCharge
	{
		public InvoiceLineApportionedCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups()
		{
			return new JobComInvHeaderChargeLookups(this);
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new InvoiceLineApportionedChargeValidation(this);
		}
	}
}

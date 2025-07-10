using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.KR.Business
{
	public class InvoiceCharge : Customs.Business.BaseInvoiceCharge, Integration.Customs.KR.IInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new InvoiceChargeLookups Lookups => (InvoiceChargeLookups)base.Lookups;
		protected override JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceChargeLookups(this);
		protected override bool ShouldResetDefaultIsIncludedInAmount(ZString incoTerm, ICustomsChargeCode charge) => true;
		public new InvoiceChargeValidation Validation => (InvoiceChargeValidation)base.Validation;
		protected override JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceChargeValidation(this);

		protected override bool GetIncludedInITOTReadOnly()
		{
			return base.GetIncludedInITOTReadOnly() || (!J7_Calc_IsIncludedInInvoiceAmount && J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
		}
	}
}

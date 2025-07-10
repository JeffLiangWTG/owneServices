using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.KR.Business
{
	public class InvoiceLineCharge : Customs.Business.BaseInvoiceLineCharge, Integration.Customs.KR.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new InvoiceLineChargeLookups Lookups => (InvoiceLineChargeLookups)base.Lookups;
		protected override JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceLineChargeLookups(this);
		protected override bool ShouldResetDefaultIsIncludedInAmount(ZString incoTerm, ICustomsChargeCode charge) => true;
		public new InvoiceLineChargeValidation Validation => (InvoiceLineChargeValidation)base.Validation;
		protected override JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineChargeValidation(this);
		protected override bool GetIncludedInITOTReadOnly()
		{
			return base.GetIncludedInITOTReadOnly() || (!J7_Calc_IsIncludedInInvoiceAmount && J7_Calc_IsIncludedInInvoiceAmount_ReadOnly);
		}
	}
}

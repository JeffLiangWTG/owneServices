using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class InvoiceCharge : EU.Business.Declaration.InvoiceCharge, Integration.Customs.ES.IInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;

		public new InvoiceChargeLookups Lookups => (InvoiceChargeLookups)base.Lookups;

		protected override JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceChargeLookups(this);

		public new InvoiceChargeValidation Validation => (InvoiceChargeValidation)base.Validation;

		protected override JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceChargeValidation(this);

		protected override bool GetIncludedInITOTReadOnly()
		{
			return !IsExport && base.GetIncludedInITOTReadOnly();
		}

		protected override void DefaultIsIncludedInInvoice(Common.IncoTermAndCustomsChargeFactory incoTermAndChargeFactory, ICustomsChargeCode charge)
		{
			if (IsExport || IsIncludedInInvoiceAmountFixed || ShouldResetDefaultIsIncludedInAmount(IncoTerm, charge))
			{
				J7_Calc_IsIncludedInInvoiceAmount = charge != null && incoTermAndChargeFactory.GetDefaultIsIncludedInInvoice(Parent.IncoTerm, charge);
			}
		}

		protected override ZBool GetDefaultIsIncludedInITOT(ICustomsChargeCode customsChargeCode)
		{
			if (IsExport)
			{
				var incoTermAndChargeFactory = IncoTermAndChargeFactory;
				return customsChargeCode != null && incoTermAndChargeFactory.GetDefaultIsIncludedInInvoice(Parent.IncoTerm, customsChargeCode);
			}
			else
			{
				return base.GetDefaultIsIncludedInITOT(customsChargeCode);
			}
		}

		ZBool IsExport => Invoice?.IsExport ?? ZBool.False;
	}
}

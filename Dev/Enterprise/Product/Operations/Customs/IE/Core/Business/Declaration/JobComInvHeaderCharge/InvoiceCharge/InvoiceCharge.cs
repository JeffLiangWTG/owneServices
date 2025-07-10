using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class InvoiceCharge : EU.Business.Declaration.InvoiceCharge, Integration.Customs.IE.IInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;

		protected override JobComInvHeaderChargeLookups GetNewLookups() => Invoice != null && Invoice.IsImport ? new ImportInvoiceChargeLookups(this) : base.GetNewLookups();

		protected override bool ShouldResetDefaultIsIncludedInAmount(ZString incoTerm, ICustomsChargeCode charge) => true;

		public new InvoiceChargeValidation Validation => base.Validation;

		protected override JobComInvHeaderChargeValidation GetNewValidation()
		{
			if (Invoice?.IsImport ?? false)
			{
				return new ImportInvoiceChargeValidation(this);
			}
			else
			{
				return base.GetNewValidation();
			}
		}

		public override ZBool J7_IsIncludedInITOT
		{
			get => base.J7_IsIncludedInITOT;
			set
			{
				var oldValue = J7_IsIncludedInITOT;
				base.J7_IsIncludedInITOT = value;
				if (!IsCopying && oldValue != J7_IsIncludedInITOT)
				{
					Invoice?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZDecimal J7_Percentage
		{
			get => base.J7_Percentage;
			set
			{
				var oldValue = J7_Percentage;
				base.J7_Percentage = value;
				if (!IsCopying && oldValue != J7_Percentage)
				{
					Invoice?.MarkAsNeedingValidation();
				}
			}
		}
	}
}

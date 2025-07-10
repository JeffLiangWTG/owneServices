using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class InvoiceLineCharge : EU.Business.Declaration.InvoiceLineCharge, Integration.Customs.FR.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new InvoiceLineChargeLookups Lookups => (InvoiceLineChargeLookups)base.Lookups;

		protected override JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceLineChargeLookups(this);

		public new InvoiceLineChargeValidation Validation => (InvoiceLineChargeValidation)base.Validation;

		protected override JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineChargeValidation(this);

		public override ZString J7_ChargeType
		{
			get => base.J7_ChargeType;
			set
			{
				base.J7_ChargeType = value;
				if (value == EU.Business.UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge)
				{
					var entryInstruction = ((JobComInvoiceLine)Parent)?.EntryInstruction;
					if (entryInstruction != null)
					{
						entryInstruction.ZG_BypassCode = ValuationBypassCodeList.Codes.VBC_J;
					}
				}
			}
		}

		protected override void DefaultIsIncludedInInvoice(Common.IncoTermAndCustomsChargeFactory incoTermAndChargeFactory, ICustomsChargeCode charge)
		{
			if (charge is FlagManagedCharge chargeCode)
			{
				J7_Calc_IsIncludedInInvoiceAmount = chargeCode.IsIncludedInInvoice;
			}
			else if (charge != null && charge.Code == EU.Business.UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge)
			{
				J7_Calc_IsIncludedInInvoiceAmount = false;
			}
			else
			{
				base.DefaultIsIncludedInInvoice(incoTermAndChargeFactory, charge);
			}
		}
	}
}

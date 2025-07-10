using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public class ARCashAdvanceRequirement : CashAdvanceRequirement
	{
		public ARCashAdvanceRequirement(BaseCharge jobCharge) : base(jobCharge)
		{
		}

		protected override ZGuid LinePK => Charge.JR_CAL_ARLine;

		protected override ZBool CashAdvanceRequired => Charge.JR_IsARCashAdvance;

		protected override ZDecimal JobChargeOSAmount => Charge.JR_OSSellAmt;

		protected override ZDecimal JobChargeLocalAmount => Charge.JR_LocalSellAmt;

		protected override ZString JobChargeOSCurrency => Charge.JR_RX_NKSellCurrency;

		protected override void RemoveCashAdvanceRequirementCore()
		{
			Charge.RemoveCashAdvanceRequirement(LedgerTypes.AccountsReceivable);
		}

		protected override bool IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed
		{
			get
			{
				var cashAdvanceFunctionalityChecker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				return cashAdvanceFunctionalityChecker != null &&
					cashAdvanceFunctionalityChecker.IsManualSettingOfReceivablesCashAdvanceRequestStatusToPaidAllowed;
			}
		}

		protected override bool IsFunctionalityEnabled
		{
			get
			{
				var cashAdvanceFunctionalityChecker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				return cashAdvanceFunctionalityChecker != null &&
					cashAdvanceFunctionalityChecker.IsReceivablesCashAdvanceFunctionalityEnabled;
			}
		}

		protected override int Multiplier => 1;
	}
}

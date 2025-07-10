using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public class APCashAdvanceRequirement : CashAdvanceRequirement
	{
		public APCashAdvanceRequirement(BaseCharge jobCharge) : base(jobCharge)
		{
		}

		protected override ZGuid LinePK => Charge.JR_CAL_APLine;

		protected override ZBool CashAdvanceRequired => Charge.JR_IsAPCashAdvance;

		protected override ZDecimal JobChargeOSAmount => Charge.JR_OSCostAmt;

		protected override ZDecimal JobChargeLocalAmount => Charge.JR_LocalCostAmt;

		protected override ZString JobChargeOSCurrency => Charge.JR_RX_NKCostCurrency;

		protected override bool IsManualSettingOfCashAdvanceRequestStatusToPaidAllowed
		{
			get
			{
				var cashAdvanceFunctionalityChecker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				return cashAdvanceFunctionalityChecker != null &&
					cashAdvanceFunctionalityChecker.IsManualSettingOfPayablesCashAdvanceRequestStatusToPaidAllowed;
			}
		}

		protected override bool IsFunctionalityEnabled
		{
			get
			{
				var cashAdvanceFunctionalityChecker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				return cashAdvanceFunctionalityChecker != null &&
					cashAdvanceFunctionalityChecker.IsPayablesCashAdvanceFunctionalityEnabled;
			}
		}

		protected override int Multiplier => -1;

		protected override void RemoveCashAdvanceRequirementCore()
		{
			Charge.RemoveCashAdvanceRequirement(LedgerTypes.AccountsPayable);
		}
	}
}

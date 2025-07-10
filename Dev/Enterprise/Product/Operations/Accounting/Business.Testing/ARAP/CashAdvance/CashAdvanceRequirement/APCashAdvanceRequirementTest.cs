using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance.Testing
{
	public class APCashAdvanceRequirementTest : CashAdvanceRequirementTest
	{
		protected override string LedgerType => LedgerTypes.AccountsPayable;

		protected override ICashAdvanceRequirement SetCashAdvanceRequirement(Charge charge)
		{
			charge.JR_IsAPCashAdvance = true;
			AssertNotNull(charge.APCashAdvanceRequirement);
			return charge.APCashAdvanceRequirement;
		}

		protected override void LinkChargeWithRequestLine(Charge charge, ZGuid linePK)
		{
			charge.JR_CAL_APLine = linePK;
		}
	}
}

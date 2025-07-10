using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance.Testing
{
	public class ARCashAdvanceRequirementTest : CashAdvanceRequirementTest
	{
		protected override ICashAdvanceRequirement SetCashAdvanceRequirement(Charge charge)
		{
			charge.JR_IsARCashAdvance = true;
			AssertNotNull(charge.ARCashAdvanceRequirement);
			return charge.ARCashAdvanceRequirement;
		}

		protected override void LinkChargeWithRequestLine(Charge charge, ZGuid linePK)
		{
			charge.JR_CAL_ARLine = linePK;
		}

		protected override string LedgerType => LedgerTypes.AccountsReceivable;
	}
}

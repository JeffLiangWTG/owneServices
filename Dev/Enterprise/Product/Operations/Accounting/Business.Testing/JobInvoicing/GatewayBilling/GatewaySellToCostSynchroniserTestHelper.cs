using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.GatewayBilling.Testing
{
	public static class GatewaySellToCostSynchroniserTestHelper
	{
		public static Charge GetValidGatewayCharge(Job job, ZGuid chargeCode, ZDecimal amount, bool jrjFieldsMatch = true)
		{
			var charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode;
			charge.JR_GE = new TestObjectCreator(job.Factory).GEADepartment.PK;
			charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
			charge.JR_LocalSellAmt = amount;

			if (jrjFieldsMatch)
			{
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GE_InternalDept = charge.JR_GE;
				charge.JR_GB_InternalBranch = charge.JR_GB;
			}

			return charge;
		}
	}
}

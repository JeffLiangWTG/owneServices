using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal abstract class AirProfitShareLineBaseTestCase : MessageLineTestCase
	{
		protected void AddCharge(JASForwardingShipment shipment, ZGuid chargeCodePK, ZDecimal localCost, ZDecimal agentDeclaredLocalCost, ZDecimal localRevenue, ZDecimal agentDeclaredLocalRevenue, JASOrgHeader sellAccount)
		{
			AddShipmentJobIfNotExist(shipment);
			Charge charge = shipment.Job.Charges.AddNew();
			charge.JR_AC = chargeCodePK;
			charge.JR_LocalCostAmt = localCost;
			charge.JR_AgentDeclaredCostAmtLocal = agentDeclaredLocalCost;
			charge.JR_LocalSellAmt = localRevenue;
			charge.JR_AgentDeclaredSellAmtLocal = agentDeclaredLocalRevenue;
			charge.JR_OH_SellAccount = sellAccount.PK;
		}

		void AddShipmentJobIfNotExist(JASForwardingShipment shipment)
		{
			if (shipment.Job == null)
			{
				JASJob job = Factory.NewJobForTesting<JASJob>();
				job.JH_ParentID = shipment.PK;
				job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				job.JH_GC = GlbCompany.CurrentCompany.PK;
			}
		}
	}
}

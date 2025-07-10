using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Business.BusinessObjects;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceChargeLine(BusinessObjectFactory factory, DataRow row) : AutoDashAPInvoiceChargeLine(factory, row), IDashAPInvoiceChargeLine
	{
		#region DPL_DPC_ClusterID

		[RelatedBusinessObject("DashAPInvoiceCluster")]
		public override ZGuid DPL_DPC_ClusterID
		{
			get => base.DPL_DPC_ClusterID;
			set => base.DPL_DPC_ClusterID = value;
		}

		#endregion

		public DashAPInvoiceCluster DashAPInvoiceCluster
		{
			get
			{
				if (dashAPInvoiceCluster == null)
				{
					dashAPInvoiceCluster = Factory.Load<DashAPInvoiceCluster>(DPL_DPC_ClusterID);
				}

				return dashAPInvoiceCluster;
			}
		}
		DashAPInvoiceCluster dashAPInvoiceCluster;

		public DashAPInvoiceChargeLineRefCollection DashAPInvoiceChargeLineRefs
		{
			get
			{
				if (dashAPInvoiceChargeLineRefs == null)
				{
					dashAPInvoiceChargeLineRefs = new DashAPInvoiceChargeLineRefCollection(this);
					dashAPInvoiceChargeLineRefs.Load();
				}

				return dashAPInvoiceChargeLineRefs;
			}
		}
		DashAPInvoiceChargeLineRefCollection dashAPInvoiceChargeLineRefs;

		public void SetDashAPInvoiceChargeLineRefs(DashAPInvoiceChargeLineRefCollection dashAPInvoiceChargeLineRefs)
		{
			this.dashAPInvoiceChargeLineRefs = dashAPInvoiceChargeLineRefs;
		}
	}
}

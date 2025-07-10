using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceClusterRef : AutoDashAPInvoiceClusterRef, IDashAPInvoiceClusterRef
	{
		public DashAPInvoiceClusterRef(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("DashAPInvoiceRef")]
		public override ZGuid DRC_DPR_RefID
		{
			get => base.DRC_DPR_RefID;
			set => base.DRC_DPR_RefID = value;
		}

		[RelatedBusinessObject("DashAPInvoiceCluster")]
		public override ZGuid DRC_DPC_ClusterID
		{
			get => base.DRC_DPC_ClusterID;
			set => base.DRC_DPC_ClusterID = value;
		}

		public DashAPInvoiceCluster DashAPInvoiceCluster
		{
			get
			{
				if (dashAPInvoiceCluster == null)
				{
					dashAPInvoiceCluster = Factory.Load<DashAPInvoiceCluster>(DRC_DPC_ClusterID);
				}

				return dashAPInvoiceCluster;
			}
		}
		DashAPInvoiceCluster dashAPInvoiceCluster;

		public DashAPInvoiceRef DashAPInvoiceRef
		{
			get
			{
				if (dashAPInvoiceRef == null)
				{
					dashAPInvoiceRef = Factory.Load<DashAPInvoiceRef>(DRC_DPR_RefID);
				}

				return dashAPInvoiceRef;
			}
		}
		DashAPInvoiceRef dashAPInvoiceRef;
	}
}

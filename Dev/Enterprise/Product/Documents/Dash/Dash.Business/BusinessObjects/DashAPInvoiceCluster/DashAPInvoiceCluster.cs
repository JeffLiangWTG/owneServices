using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Business.BusinessObjects;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceCluster(BusinessObjectFactory factory, DataRow row) : AutoDashAPInvoiceCluster(factory, row), IDashAPInvoiceCluster
	{
		[RelatedBusinessObject("DashAPInvoice")]
		public override ZGuid DPC_DPI_HeaderID
		{
			get => base.DPC_DPI_HeaderID;
			set => base.DPC_DPI_HeaderID = value;
		}

		public DashAPInvoice DashAPInvoice
		{
			get
			{
				if (dashAPInvoice == null)
				{
					dashAPInvoice = Factory.Load<DashAPInvoice>(DPC_DPI_HeaderID);
				}

				return dashAPInvoice;
			}
		}
		DashAPInvoice dashAPInvoice;

		public DashAPInvoiceChargeLineCollection DashAPInvoiceChargeLines
		{
			get
			{
				if (dashAPInvoiceChargeLines == null)
				{
					dashAPInvoiceChargeLines = new DashAPInvoiceChargeLineCollection(this);
					dashAPInvoiceChargeLines.Load();
				}

				return dashAPInvoiceChargeLines;
			}
		}
		DashAPInvoiceChargeLineCollection dashAPInvoiceChargeLines;

		public DashAPInvoiceClusterRefCollection DashAPInvoiceClusterRefs
		{
			get
			{
				if (dashAPInvoiceClusterRefs == null)
				{
					dashAPInvoiceClusterRefs = new DashAPInvoiceClusterRefCollection(this);
					dashAPInvoiceClusterRefs.Load();
				}

				return dashAPInvoiceClusterRefs;
			}
		}
		DashAPInvoiceClusterRefCollection dashAPInvoiceClusterRefs;
	}
}

using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Business.BusinessObjects;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashAPInvoice(BusinessObjectFactory factory, DataRow row) : AutoDashAPInvoice(factory, row), IDashAPInvoice
	{
		[RelatedBusinessObject("DashDocument")]
		public override ZGuid DPI_DDD_DashDocID
		{
			get => base.DPI_DDD_DashDocID;
			set => base.DPI_DDD_DashDocID = value;
		}

		public DashDocument DashDocument
		{
			get
			{
				if (dashDocument == null)
				{
					dashDocument = Factory.Load<DashDocument>(DPI_DDD_DashDocID);
				}

				return dashDocument;
			}
		}
		DashDocument dashDocument;

		public DashAPInvoiceClusterCollection DashAPInvoiceClusters
		{
			get
			{
				if (dashAPInvoiceClusterCollection == null)
				{
					dashAPInvoiceClusterCollection = new DashAPInvoiceClusterCollection(this);
					dashAPInvoiceClusterCollection.Load();
				}

				return dashAPInvoiceClusterCollection;
			}
		}
		DashAPInvoiceClusterCollection dashAPInvoiceClusterCollection;

		public DashAPInvoiceRefCollection DashAPInvoiceRefs
		{
			get
			{
				if (dashAPInvoiceRefs == null)
				{
					dashAPInvoiceRefs = new DashAPInvoiceRefCollection(this);
					dashAPInvoiceRefs.Load();
				}

				return dashAPInvoiceRefs;
			}
		}
		DashAPInvoiceRefCollection dashAPInvoiceRefs;

		public void SetDashDocument(DashDocument dashDocument)
		{
			this.dashDocument = dashDocument;
		}
	}
}

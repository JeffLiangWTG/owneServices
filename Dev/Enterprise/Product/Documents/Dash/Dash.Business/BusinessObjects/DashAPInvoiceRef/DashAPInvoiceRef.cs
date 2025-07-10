using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceRef : AutoDashAPInvoiceRef, IDashAPInvoiceRef
	{
		public DashAPInvoiceRef(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("DashAPInvoice")]
		public override ZGuid DPR_DPI_HeaderID
		{
			get => base.DPR_DPI_HeaderID;
			set => base.DPR_DPI_HeaderID = value;
		}

		public DashAPInvoice DashAPInvoice
		{
			get
			{
				if (dashAPInvoice == null)
				{
					dashAPInvoice = Factory.Load<DashAPInvoice>(DPR_DPI_HeaderID);
				}

				return dashAPInvoice;
			}
		}
		DashAPInvoice dashAPInvoice;
	}
}

using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Dash.Integration.BusinessObjects;
using Enterprise.ZArchitecture.Schema;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business
{
	public class DashDocument : AutoDashDocument, IDashDocument
	{
		public DashDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public DashAPInvoice DashAPInvoice
		{
			get
			{
				if (!string.Equals(DDD_ParseType, SharedConstants.ParseType.Code.AccountPayableInvoice, StringComparison.OrdinalIgnoreCase))
				{
					return null;
				}

				if (dashAPInvoice == null)
				{
					var dashAPInvoiceQuery = new ZQuery(DashAPInvoiceSchema.DPI_DDD_DashDocID, PK);
					dashAPInvoice = Factory.LoadTop1<DashAPInvoice>(dashAPInvoiceQuery);
				}

				return dashAPInvoice;
			}
		}
		DashAPInvoice dashAPInvoice;

		public DashCommercialInvoice DashCommercialInvoice
		{
			get
			{
				if (!string.Equals(DDD_ParseType, SharedConstants.ParseType.Code.CommercialInvoice, StringComparison.OrdinalIgnoreCase))
				{
					return null;
				}

				if (dashCommercialInvoice == null)
				{
					var dashCommercialInvoiceQuery = new ZQuery(DashCommercialInvoiceSchema.DCI_DDD_DashDocID, PK);
					dashCommercialInvoice = Factory.LoadTop1<DashCommercialInvoice>(dashCommercialInvoiceQuery);
				}

				return dashCommercialInvoice;
			}
		}
		DashCommercialInvoice dashCommercialInvoice;

		public void SetDashAPInvoice(DashAPInvoice dashAPInvoice)
		{
			this.dashAPInvoice = dashAPInvoice;
		}
	}
}

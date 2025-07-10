using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Business.BusinessObjects;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashCommercialInvoice : AutoDashCommercialInvoice, IDashCommercialInvoice
	{
		public DashCommercialInvoice(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region DCI_DDD_DashDocID

		[RelatedBusinessObject("DashDocument")]
		public override ZGuid DCI_DDD_DashDocID
		{
			get => base.DCI_DDD_DashDocID;
			set => base.DCI_DDD_DashDocID = value;
		}

		public DashDocument DashDocument
		{
			get => Factory.Load<DashDocument>(DCI_DDD_DashDocID);
		}

		#endregion

		public DashCommercialInvoiceLineItemCollection CommercialInvoiceLineItems
		{
			get
			{
				if (commercialInvoiceLineItems == null)
				{
					commercialInvoiceLineItems = new DashCommercialInvoiceLineItemCollection(this);
					commercialInvoiceLineItems.Load();
				}

				return commercialInvoiceLineItems;
			}
		}
		DashCommercialInvoiceLineItemCollection commercialInvoiceLineItems;
	}
}

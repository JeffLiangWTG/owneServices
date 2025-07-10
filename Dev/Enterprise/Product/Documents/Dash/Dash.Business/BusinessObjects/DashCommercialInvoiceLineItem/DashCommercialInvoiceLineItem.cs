using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashCommercialInvoiceLineItem : AutoDashCommercialInvoiceLineItem, IDashCommercialInvoiceLineItem
	{
		public DashCommercialInvoiceLineItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region DLI_DCI_HeaderID

		public DashCommercialInvoice DashCommercialInvoice
		{
			get => Factory.Load<DashCommercialInvoice>(DLI_DCI_HeaderID);
		}

		[RelatedBusinessObject("DashCommercialInvoice")]
		public override ZGuid DLI_DCI_HeaderID
		{
			get => base.DLI_DCI_HeaderID;
			set => base.DLI_DCI_HeaderID = value;
		}

		#endregion
	}
}

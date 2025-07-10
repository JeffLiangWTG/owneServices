using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.CLE
{
	public class CLEOrder : Order
	{
		public CLEOrder(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override bool CanBeUpdatedByImportCore
		{
			get { return !(IsShipmentWithDeclarationAttached || IsDeclarationAttached || IsAttachedToCommercialInvoice); }
		}

		ZBool IsShipmentWithDeclarationAttached
		{
			get
			{
				return IsShipmentAttached &&
					Shipment.Declarations != null &&
					Shipment.Declarations.Length > 0;
			}
		}

		public bool IsAttachedToCommercialInvoice
		{
			get
			{
				return OrderLines.Any((orderLine) => Factory.LoadTop1<BaseJobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_JO, orderLine.PK)) != null);
			}
		}
	}
}

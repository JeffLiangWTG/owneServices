using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Dash.Integration.BusinessObjects;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Dash.Business.BusinessObjects
{
	public class DashCommercialInvoiceLineItemCollection(IDashCommercialInvoice master) : DependentBusinessObjectCollection<DashCommercialInvoiceLineItem, DashCommercialInvoice>((DashCommercialInvoice)master)
	{
		#region Implementation

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(DashCommercialInvoiceLineItem);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return DashCommercialInvoiceLineItemSchema.DLI_DCI_HeaderID; }
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			var dashCommercialInvoiceLineItem = (DashCommercialInvoiceLineItem)child;
			base.SetCollectionRelationships(dashCommercialInvoiceLineItem);
		}

		#endregion
	}
}

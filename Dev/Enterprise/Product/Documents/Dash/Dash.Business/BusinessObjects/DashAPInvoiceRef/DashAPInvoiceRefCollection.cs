using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Dash.Integration.BusinessObjects;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Dash.Business.BusinessObjects
{
	public class DashAPInvoiceRefCollection(IDashAPInvoice master) : DependentBusinessObjectCollection<DashAPInvoiceRef, DashAPInvoice>((DashAPInvoice)master)
	{
		#region Implementation

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(DashAPInvoiceRef);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return DashAPInvoiceRefSchema.DPR_DPI_HeaderID; }
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			var dashAPInvoiceRef = (DashAPInvoiceRef)child;
			base.SetCollectionRelationships(dashAPInvoiceRef);
		}

		#endregion
	}
}

using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Dash.Integration.BusinessObjects;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Dash.Business.BusinessObjects
{
	public class DashAPInvoiceClusterCollection(IDashAPInvoice master) : DependentBusinessObjectCollection<DashAPInvoiceCluster, DashAPInvoice>((DashAPInvoice)master)
	{
		#region Implementation

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(DashAPInvoiceCluster);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return DashAPInvoiceClusterSchema.DPC_DPI_HeaderID; }
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			var dashAPInvoiceCluster = (DashAPInvoiceCluster)child;
			base.SetCollectionRelationships(dashAPInvoiceCluster);
		}

		#endregion
	}
}

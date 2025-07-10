using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Dash.Integration.BusinessObjects;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Dash.Business.BusinessObjects
{
	public class DashAPInvoiceClusterRefCollection(IDashAPInvoiceCluster master) : DependentBusinessObjectCollection<DashAPInvoiceClusterRef, DashAPInvoiceCluster>((DashAPInvoiceCluster)master)
	{
		#region Implementation

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(DashAPInvoiceClusterRef);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return DashAPInvoiceClusterRefSchema.DRC_DPC_ClusterID; }
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			var dashAPInvoiceChargeLineRef = (DashAPInvoiceClusterRef)child;
			base.SetCollectionRelationships(dashAPInvoiceChargeLineRef);
		}

		#endregion
	}
}

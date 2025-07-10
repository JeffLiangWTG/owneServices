using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Dash.Integration.BusinessObjects;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Dash.Business.BusinessObjects
{
	public class DashAPInvoiceChargeLineCollection(IDashAPInvoiceCluster master) : DependentBusinessObjectCollection<DashAPInvoiceChargeLine, DashAPInvoiceCluster>((DashAPInvoiceCluster)master)
	{
		#region Implementation

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(DashAPInvoiceChargeLine);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return DashAPInvoiceChargeLineSchema.DPL_DPC_ClusterID; }
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			var dashAPInvoiceChargeLine = (DashAPInvoiceChargeLine)child;
			base.SetCollectionRelationships(dashAPInvoiceChargeLine);
		}

		#endregion
	}
}

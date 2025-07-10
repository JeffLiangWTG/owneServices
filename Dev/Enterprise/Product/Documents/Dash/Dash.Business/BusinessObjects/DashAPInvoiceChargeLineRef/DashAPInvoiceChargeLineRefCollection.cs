using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Dash.Integration.BusinessObjects;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Dash.Business.BusinessObjects
{
	public class DashAPInvoiceChargeLineRefCollection(IDashAPInvoiceChargeLine master) : DependentBusinessObjectCollection<DashAPInvoiceChargeLineRef, DashAPInvoiceChargeLine>((DashAPInvoiceChargeLine)master)
	{
		#region Implementation

		public override Type GetTypeOfElementsFromPK(ZGuid pk)
		{
			return typeof(DashAPInvoiceChargeLineRef);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return DashAPInvoiceChargeLineRefSchema.DLR_DPL_ChargeLineID; }
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			var dashAPInvoiceChargeLineRef = (DashAPInvoiceChargeLineRef)child;
			base.SetCollectionRelationships(dashAPInvoiceChargeLineRef);
		}

		#endregion
	}
}

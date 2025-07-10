using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Dash.Integration.BusinessObjects;

namespace Enterprise.Dash.Business
{
	public class DashAPInvoiceChargeLineRef : AutoDashAPInvoiceChargeLineRef, IDashAPInvoiceChargeLineRef
	{
		public DashAPInvoiceChargeLineRef(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("DashAPInvoiceChargeLine")]
		public override ZGuid DLR_DPL_ChargeLineID
		{
			get => base.DLR_DPL_ChargeLineID;
			set => base.DLR_DPL_ChargeLineID = value;
		}

		public DashAPInvoiceChargeLine DashAPInvoiceChargeLine
		{
			get
			{
				if (dashAPInvoiceChargeLine == null)
				{
					dashAPInvoiceChargeLine = Factory.Load<DashAPInvoiceChargeLine>(DLR_DPL_ChargeLineID);
				}

				return dashAPInvoiceChargeLine;
			}
		}
		DashAPInvoiceChargeLine dashAPInvoiceChargeLine;

		[RelatedBusinessObject("DashAPInvoiceRef")]
		public override ZGuid DLR_DPR_RefID
		{
			get => base.DLR_DPR_RefID;
			set => base.DLR_DPR_RefID = value;
		}

		public DashAPInvoiceRef DashAPInvoiceRef
		{
			get
			{
				if (dashAPInvoiceRef == null)
				{
					dashAPInvoiceRef = Factory.Load<DashAPInvoiceRef>(DLR_DPR_RefID);
				}

				return dashAPInvoiceRef;
			}
		}
		DashAPInvoiceRef dashAPInvoiceRef;

		public void SetDashAPInvoiceRef(DashAPInvoiceRef dashAPInvoiceRef)
		{
			this.dashAPInvoiceRef = dashAPInvoiceRef;
		}
	}
}

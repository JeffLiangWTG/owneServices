
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	class DocWhsReceiveInvoiceJobHistory : DocWhsDocketsInvoiceJobHistory
	{
		#region Static

		public new static DocWhsReceiveInvoiceJobHistory New(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
		{
			return (invoicingBase == null) ? null : new DocWhsReceiveInvoiceJobHistory(invoicingBase, factoryToWrap);
		}

		#endregion

		#region Constructors

		protected DocWhsReceiveInvoiceJobHistory(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
			: base(invoicingBase, factoryToWrap)
		{
			DocWhsJobChargeCollection headerChargeCollection = JobChargeLines;
		}

		#endregion

		#region Methods Overrides

		protected override bool AddChargeCondition(DocWhsJobCharge docWhsJobCharge)
		{
			WhsDocket docket = GetDocket(docWhsJobCharge.DocketPK);
			return (docket != null && docket.GetType() == typeof(WhsReceive) && base.AddChargeCondition(docWhsJobCharge));
		}

		protected override bool AddDocketCondition(WhsDocket docket, List<WhsDocket> list)
		{
			return (docket != null && docket.GetType() == typeof(WhsReceive) && base.AddDocketCondition(docket, list));
		}

		#endregion
	}
}

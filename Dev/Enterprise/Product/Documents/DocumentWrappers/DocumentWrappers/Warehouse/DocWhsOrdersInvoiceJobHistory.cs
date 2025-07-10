using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	class DocWhsOrdersInvoiceJobHistory : DocWhsDocketsInvoiceJobHistory
	{
		#region Static

		public new static DocWhsOrdersInvoiceJobHistory New(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
		{
			return (invoicingBase == null) ? null : new DocWhsOrdersInvoiceJobHistory(invoicingBase, factoryToWrap);
		}

		#endregion

		#region Constructors

		protected DocWhsOrdersInvoiceJobHistory(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
			: base(invoicingBase, factoryToWrap)
		{
			DocWhsJobChargeCollection headerChargeCollection = JobChargeLines;
		}

		#endregion

		#region Methods Overrides

		public override DocWhsDocketLineCollection GetDocDocketLineWithChargesCollection()
		{
			return new DocWhsPickableDocketLineCollection(Factory);
		}

		public override DocWhsDocketLine GetDocDocketLineWithCharges(WhsDocketLine docketLine)
		{
			return DocWhsPickableDocketLineWithCharges.New((WhsPickableDocketLine)docketLine, Factory);
		}

		protected override bool AddChargeCondition(DocWhsJobCharge docWhsJobCharge)
		{
			WhsDocket docket = GetDocket(docWhsJobCharge.DocketPK);
			return (docket != null && docket.GetType() == typeof(WhsOrder) && base.AddChargeCondition(docWhsJobCharge));
		}

		protected override bool AddDocketCondition(WhsDocket docket, List<WhsDocket> list)
		{
			return (docket != null && docket.GetType() == typeof(WhsOrder) && base.AddDocketCondition(docket, list));
		}

		#endregion
	}
}

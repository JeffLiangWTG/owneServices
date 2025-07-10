using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class InvoiceBulkBatchController : InvoiceBatchController
	{
		public InvoiceBulkBatchController()
		{
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new InvoiceBulkBatchForm(businessEntity as InvoiceBulkBatch);
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.InvoiceBulkBatch; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(InvoiceBulkBatch); }
		}
	}
}
